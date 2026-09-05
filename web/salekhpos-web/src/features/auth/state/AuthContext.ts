/**
 * Auth context type — the single source of truth for "who is the
 * current user and what's their auth status". The provider is in
 * `AuthProvider.tsx`; this file only declares the shape so the
 * hook (`useAuth.ts`) can be type-safe.
 */
import { createContext } from 'react';
import type { AuthError, TokenResponse } from '@/services/auth/authApi';
import type {
  ChangePasswordRequest,
  ForgotPasswordRequest,
  LoginRequest,
  MfaSetupResponse,
  RegisterRequest,
  RegisterResponse,
  ResetPasswordRequest,
  VerifyEmailRequest,
} from '@/services/auth/authApi';

export type AuthStatus =
  | 'loading'
  | 'anonymous'
  | 'authenticated'
  | 'mfa-pending';

export interface AuthenticatedUser {
  userId: string;
  tenantId: string;
  role: string;
  tokenVersion: number;
}

export interface MfaPendingState {
  userId: string;
  tenantId: string;
}

export interface AuthContextValue {
  status: AuthStatus;
  user: AuthenticatedUser | null;
  mfaPending: MfaPendingState | null;
  signIn(input: LoginRequest): Promise<void>;
  signInWithMfa(input: { userId: string; code: string }): Promise<void>;
  signOut(): Promise<void>;
  register(input: RegisterRequest): Promise<RegisterResponse>;
  verifyEmail(input: VerifyEmailRequest): Promise<void>;
  forgotPassword(input: ForgotPasswordRequest): Promise<void>;
  resetPassword(input: ResetPasswordRequest): Promise<void>;
  changePassword(input: ChangePasswordRequest): Promise<void>;
  setupMfa(): Promise<MfaSetupResponse>;
  disableMfa(): Promise<void>;
  /**
   * Internal: called by the apiClient refresh interceptor after a
   * successful /auth/refresh. Updates state and storage without
   * re-routing the user.
   */
  applyRefreshedTokens(tokens: TokenResponse): void;
  /**
   * Internal: called by the apiClient refresh interceptor when
   * /auth/refresh itself failed. Wipes state and storage.
   */
  handleAuthFailure(): void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export type { AuthError } from '@/services/auth/authApi';

export function isAuthError(value: unknown): value is AuthError {
  return value instanceof Error && value.name === 'AuthError';
}
