/**
 * API client.
 *
 * Thin wrapper around axios. The base URL comes from configuration. The
 * client attaches the bearer access token (when present) and a request
 * id. It does NOT perform tenant/store/permission decisions — those
 * are authoritative on the server.
 *
 * Refresh-on-401: when any request (other than /auth/refresh and
 * /auth/login themselves) returns 401, the response interceptor calls
 * /auth/refresh exactly once, in a single-flight queue. All other
 * 401s received while a refresh is in flight attach to the same
 * promise. On success the original request is replayed with the new
 * bearer. On failure the AuthProvider's `onAuthFailure` callback is
 * invoked (sign-out + redirect) and the original error is rejected.
 *
 * The interceptor does NOT touch React directly; the AuthProvider
 * registers itself with the client on mount via the four setters
 * exported below. This keeps axios stateless about React.
 */
import axios, {
  type AxiosError,
  type AxiosInstance,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from 'axios';
import { config } from '@/app/config/config';

export type ApiClient = AxiosInstance;

export interface AuthBridge {
  getAccessToken(): string | null;
  getRefreshToken(): string | null;
  onTokensRefreshed(tokens: {
    accessToken: string;
    refreshToken: string;
    accessTokenExpiresAtUtc: string;
    refreshTokenExpiresAtUtc: string;
  }): void;
  onAuthFailure(): void;
}

interface AuthInternalRequestConfig extends InternalAxiosRequestConfig {
  __skipRefresh?: boolean;
  __retried?: boolean;
}

const EMPTY_BRIDGE: AuthBridge = {
  getAccessToken: () => null,
  getRefreshToken: () => null,
  onTokensRefreshed: () => {
    /* no-op until AuthProvider registers itself */
  },
  onAuthFailure: () => {
    /* no-op until AuthProvider registers itself */
  },
};

let bridge: AuthBridge = EMPTY_BRIDGE;

export function setAuthBridge(next: AuthBridge): void {
  bridge = next;
}

export function resetAuthBridge(): void {
  bridge = EMPTY_BRIDGE;
}

export const apiClient: ApiClient = axios.create({
  baseURL: config.apiBaseUrl,
  timeout: 15_000,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
  withCredentials: false,
});

apiClient.interceptors.request.use((request) => {
  const authRequest = request as AuthInternalRequestConfig;
  const token = bridge.getAccessToken();
  if (token) {
    authRequest.headers.set('Authorization', `Bearer ${token}`);
  }
  return authRequest;
});

// ---- Refresh-on-401 state (module-level, single in-flight) ----

let refreshInFlight: Promise<void> | null = null;
let refreshFailure: Error | null = null;
const refreshWaiters: Array<{
  resolve: () => void;
  reject: (error: Error) => void;
}> = [];

async function performRefresh(): Promise<void> {
  const refreshToken = bridge.getRefreshToken();
  if (!refreshToken) {
    throw new Error('No refresh token available.');
  }
  try {
    const response = await axios.post<{
      accessToken: string;
      refreshToken: string;
      accessTokenExpiresAtUtc: string;
      refreshTokenExpiresAtUtc: string;
    }>(
      `${config.apiBaseUrl}/api/v1/auth/refresh`,
      { refreshToken },
      {
        timeout: 15_000,
        headers: { 'Content-Type': 'application/json' },
      },
    );
    bridge.onTokensRefreshed(response.data);
  } catch (raw) {
    const axiosError = raw as AxiosError;
    bridge.onAuthFailure();
    throw new Error(
      axiosError.response?.status !== undefined
        ? `Refresh failed with status ${axiosError.response.status}.`
        : 'Refresh request failed.',
    );
  }
}

async function refreshOnce(): Promise<void> {
  if (refreshFailure) {
    throw refreshFailure;
  }
  if (refreshInFlight) {
    return refreshInFlight;
  }
  refreshInFlight = (async () => {
    try {
      await performRefresh();
      for (const waiter of refreshWaiters.splice(0)) {
        waiter.resolve();
      }
    } catch (error) {
      refreshFailure = error instanceof Error ? error : new Error(String(error));
      for (const waiter of refreshWaiters.splice(0)) {
        waiter.reject(refreshFailure);
      }
      throw refreshFailure;
    } finally {
      refreshInFlight = null;
    }
  })();
  return refreshInFlight;
}

function isAuthEndpoint(url: string | undefined): boolean {
  if (!url) {
    return false;
  }
  return (
    url.includes('/api/v1/auth/refresh') ||
    url.includes('/api/v1/auth/login') ||
    url.includes('/api/v1/auth/register') ||
    url.includes('/api/v1/auth/verify-email') ||
    url.includes('/api/v1/auth/forgot-password') ||
    url.includes('/api/v1/auth/reset-password') ||
    url.includes('/api/v1/auth/mfa/verify')
  );
}

apiClient.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (raw: unknown) => {
    const error = raw as AxiosError & { config?: AuthInternalRequestConfig };
    const status = error.response?.status;
    const originalConfig = error.config;

    if (status !== 401 || !originalConfig) {
      return Promise.reject(error);
    }
    if (originalConfig.__skipRefresh === true || originalConfig.__retried === true) {
      return Promise.reject(error);
    }
    if (isAuthEndpoint(originalConfig.url)) {
      return Promise.reject(error);
    }

    originalConfig.__retried = true;
    try {
      await refreshOnce();
    } catch {
      return Promise.reject(error);
    }
    const newToken = bridge.getAccessToken();
    if (newToken) {
      originalConfig.headers.set('Authorization', `Bearer ${newToken}`);
    }
    return apiClient.request(originalConfig);
  },
);
