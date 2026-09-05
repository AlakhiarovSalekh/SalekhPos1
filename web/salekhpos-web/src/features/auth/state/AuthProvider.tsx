/**
 * AuthProvider — the single owner of the auth state.
 *
 * The provider:
 *   1. On mount, reads the persisted session from localStorage. If
 *      the access token is still valid, marks the user authenticated.
 *      If it has expired but a refresh token exists, attempts a silent
 *      /auth/refresh. If neither works, lands on anonymous.
 *   2. Registers itself with the apiClient as the auth bridge
 *      (provides access token, refresh token, refresh-success and
 *      auth-failure callbacks). On unmount it unregisters so a
 *      test that mounts/unmounts many providers in the same process
 *      does not leak callbacks.
 *   3. Exposes a stable method on each prop (signIn, signOut,
 *      register, ...). Each method calls the corresponding authApi
 *      function, updates state and storage on success, and re-throws
 *      AuthError so the page can render a localised banner.
 *
 * The provider does NOT own routing. The router is a sibling,
 * mounted in <App />. Navigation is performed by callers (the
 * pages) after a successful sign-in, not by the provider — that
 * keeps the provider composable and easy to test.
 */
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { AuthContext, type AuthContextValue, type AuthenticatedUser, type MfaPendingState } from './AuthContext';
import * as authApi from '@/services/auth/authApi';
import type { TokenResponse } from '@/services/auth/authApi';
import { clearAuth, readAuth, writeAuth } from '@/services/storage/storage';
import { resetAuthBridge, setAuthBridge, type AuthBridge } from '@/services/api/apiClient';

interface AuthProviderProps {
  readonly children: ReactNode;
}

function isExpired(iso: string, nowMs: number): boolean {
  const ts = Date.parse(iso);
  if (Number.isNaN(ts)) {
    return true;
  }
  return ts <= nowMs;
}

function userFromStorage(
  userId: string,
  tenantId: string,
  role: string,
  tokenVersion: number,
): AuthenticatedUser {
  return { userId, tenantId, role, tokenVersion };
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [status, setStatus] = useState<AuthContextValue['status']>('loading');
  const [user, setUser] = useState<AuthenticatedUser | null>(null);
  const [mfaPending, setMfaPending] = useState<MfaPendingState | null>(null);
  const refreshAttemptedRef = useRef(false);

  // Hydrate from storage exactly once on mount. If a refresh token
  // exists and the access token is expired (or absent), try one
  // silent refresh; otherwise mark anonymous.
  useEffect(() => {
    let cancelled = false;
    async function hydrate(): Promise<void> {
      const stored = readAuth();
      if (!stored) {
        if (!cancelled) {
          setStatus('anonymous');
        }
        return;
      }
      const now = Date.now();
      if (!isExpired(stored.accessTokenExpiresAtUtc, now)) {
        if (!cancelled) {
          setUser(
            userFromStorage(
              stored.userId,
              stored.tenantId,
              stored.role,
              stored.tokenVersion,
            ),
          );
          setStatus('authenticated');
        }
        return;
      }
      if (refreshAttemptedRef.current) {
        if (!cancelled) {
          clearAuth();
          setStatus('anonymous');
        }
        return;
      }
      refreshAttemptedRef.current = true;
      try {
        const tokens = await authApi.refresh({ refreshToken: stored.refreshToken });
        if (cancelled) {
          return;
        }
        writeAuth({
          accessToken: tokens.accessToken,
          refreshToken: tokens.refreshToken,
          accessTokenExpiresAtUtc: tokens.accessTokenExpiresAtUtc,
          refreshTokenExpiresAtUtc: tokens.refreshTokenExpiresAtUtc,
          userId: stored.userId,
          tenantId: stored.tenantId,
          role: stored.role,
          tokenVersion: stored.tokenVersion,
        });
        setUser({
          userId: stored.userId,
          tenantId: stored.tenantId,
          role: stored.role,
          tokenVersion: stored.tokenVersion,
        });
        setStatus('authenticated');
      } catch {
        if (cancelled) {
          return;
        }
        clearAuth();
        setStatus('anonymous');
      }
    }
    void hydrate();
    return () => {
      cancelled = true;
    };
  }, []);

  const persistSignedIn = useCallback(
    (
      tokens: TokenResponse,
      identity: { userId: string; tenantId: string; role: string; tokenVersion: number },
    ): void => {
      writeAuth({
        accessToken: tokens.accessToken,
        refreshToken: tokens.refreshToken,
        accessTokenExpiresAtUtc: tokens.accessTokenExpiresAtUtc,
        refreshTokenExpiresAtUtc: tokens.refreshTokenExpiresAtUtc,
        userId: identity.userId,
        tenantId: identity.tenantId,
        role: identity.role,
        tokenVersion: identity.tokenVersion,
      });
      setUser(identity);
      setMfaPending(null);
      setStatus('authenticated');
    },
    [],
  );

  const signIn = useCallback<AuthContextValue['signIn']>(async (input) => {
    const result = await authApi.login(input);
    if (result.status === 'mfa_required') {
      setMfaPending({ userId: result.mfa.userId, tenantId: result.mfa.tenantId });
      setStatus('mfa-pending');
      return;
    }
    // Authenticated. Decode the JWT to extract the identity
    // claims (sub, tid, role, ver) so we don't need a separate
    // /me endpoint just to render the account page.
    const claims = decodeClaims(result.tokens.accessToken);
    if (!claims) {
      throw new Error('Access token is malformed.');
    }
    persistSignedIn(result.tokens, {
      userId: claims.sub,
      tenantId: claims.tid,
      role: claims.role,
      tokenVersion: claims.ver,
    });
  }, [persistSignedIn]);

  const signInWithMfa = useCallback<AuthContextValue['signInWithMfa']>(async (input) => {
    const tokens = await authApi.mfaVerify(input);
    const claims = decodeClaims(tokens.accessToken);
    if (!claims) {
      throw new Error('Access token is malformed.');
    }
    persistSignedIn(tokens, {
      userId: claims.sub,
      tenantId: claims.tid,
      role: claims.role,
      tokenVersion: claims.ver,
    });
  }, [persistSignedIn]);

  const signOut = useCallback<AuthContextValue['signOut']>(async () => {
    try {
      await authApi.logout();
    } catch {
      // Best-effort: the local session is wiped even if the
      // server call fails (token may already be invalid).
    }
    clearAuth();
    setUser(null);
    setMfaPending(null);
    setStatus('anonymous');
  }, []);

  const register = useCallback<AuthContextValue['register']>(async (input) => {
    return authApi.register(input);
  }, []);

  const verifyEmail = useCallback<AuthContextValue['verifyEmail']>(async (input) => {
    await authApi.verifyEmail(input);
  }, []);

  const forgotPassword = useCallback<AuthContextValue['forgotPassword']>(async (input) => {
    await authApi.forgotPassword(input);
  }, []);

  const resetPassword = useCallback<AuthContextValue['resetPassword']>(async (input) => {
    await authApi.resetPassword(input);
  }, []);

  const changePassword = useCallback<AuthContextValue['changePassword']>(async (input) => {
    await authApi.changePassword(input);
  }, []);

  const setupMfa = useCallback<AuthContextValue['setupMfa']>(async () => {
    return authApi.mfaSetup();
  }, []);

  const disableMfa = useCallback<AuthContextValue['disableMfa']>(async () => {
    await authApi.mfaDisable();
  }, []);

  const applyRefreshedTokens = useCallback<AuthContextValue['applyRefreshedTokens']>(
    (tokens) => {
      const current = readAuth();
      if (!current) {
        return;
      }
      writeAuth({
        accessToken: tokens.accessToken,
        refreshToken: tokens.refreshToken,
        accessTokenExpiresAtUtc: tokens.accessTokenExpiresAtUtc,
        refreshTokenExpiresAtUtc: tokens.refreshTokenExpiresAtUtc,
        userId: current.userId,
        tenantId: current.tenantId,
        role: current.role,
        tokenVersion: current.tokenVersion,
      });
    },
    [],
  );

  const handleAuthFailure = useCallback<AuthContextValue['handleAuthFailure']>(() => {
    clearAuth();
    setUser(null);
    setMfaPending(null);
    setStatus('anonymous');
  }, []);

  // Register/unregister the auth bridge with the apiClient.
  useEffect(() => {
    const bridge: AuthBridge = {
      getAccessToken: () => readAuth()?.accessToken ?? null,
      getRefreshToken: () => readAuth()?.refreshToken ?? null,
      onTokensRefreshed: (tokens) => applyRefreshedTokens(tokens),
      onAuthFailure: () => handleAuthFailure(),
    };
    setAuthBridge(bridge);
    return () => {
      resetAuthBridge();
    };
  }, [applyRefreshedTokens, handleAuthFailure]);

  const value = useMemo<AuthContextValue>(
    () => ({
      status,
      user,
      mfaPending,
      signIn,
      signInWithMfa,
      signOut,
      register,
      verifyEmail,
      forgotPassword,
      resetPassword,
      changePassword,
      setupMfa,
      disableMfa,
      applyRefreshedTokens,
      handleAuthFailure,
    }),
    [
      status,
      user,
      mfaPending,
      signIn,
      signInWithMfa,
      signOut,
      register,
      verifyEmail,
      forgotPassword,
      resetPassword,
      changePassword,
      setupMfa,
      disableMfa,
      applyRefreshedTokens,
      handleAuthFailure,
    ],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

interface JwtClaims {
  sub: string;
  tid: string;
  role: string;
  ver: number;
}

/**
 * EdDSA-signed JWTs are three base64url parts separated by dots.
 * We only need the payload to render the account page; signature
 * verification has already happened on the server before the
 * token reached the browser. We use `atob` on the URL-safe
 * variant and parse the JSON defensively.
 */
function decodeClaims(token: string): JwtClaims | null {
  const parts = token.split('.');
  if (parts.length !== 3) {
    return null;
  }
  const [, payload] = parts;
  if (!payload) {
    return null;
  }
  try {
    const padded = payload.replace(/-/g, '+').replace(/_/g, '/');
    const json = atob(padded);
    const parsed = JSON.parse(json) as Record<string, unknown>;
    if (
      typeof parsed.sub !== 'string' ||
      typeof parsed.tid !== 'string' ||
      typeof parsed.role !== 'string' ||
      typeof parsed.ver !== 'number'
    ) {
      return null;
    }
    return {
      sub: parsed.sub,
      tid: parsed.tid,
      role: parsed.role,
      ver: parsed.ver,
    };
  } catch {
    return null;
  }
}
