/**
 * Typed wrapper around every Phase 2 auth endpoint.
 *
 * Every function takes a body shape and returns the exact response
 * shape the backend ships (see `AuthController.cs`). On non-2xx the
 * helper throws an `AuthError` that carries the HTTP status, the
 * RFC 7807 `code`/`message` pair, and the `extensions` object the
 * backend attaches for password-policy reasons, account lockouts,
 * and rate-limit retry hints.
 *
 * The functions never call the API directly — they go through
 * `apiClient`, which is the only place that knows about axios and
 * the refresh-on-401 interceptor.
 */
import { apiClient, type ApiClient } from '@/services/api/apiClient';

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
  refreshTokenExpiresAtUtc: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  tenantName: string;
  tenantSlug: string;
}

export interface RegisterResponse {
  userId: string;
  tenantId: string;
  tenantSlug: string;
}

export interface VerifyEmailRequest {
  token: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export type LoginResponse =
  | { status: 'authenticated'; tokens: TokenResponse }
  | { status: 'mfa_required'; mfa: { userId: string; tenantId: string } };

export interface RefreshRequest {
  refreshToken: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface MfaSetupResponse {
  provisioningUri: string;
  recoveryCodes: readonly string[];
}

export interface MfaVerifyRequest {
  userId: string;
  code: string;
}

/**
 * Error shape mirrors RFC 7807 ProblemDetails plus the small set
 * of `extensions` the backend stamps onto the auth surface.
 */
export interface AuthError extends Error {
  status: number;
  code: string;
  message: string;
  type?: string;
  extensions?: {
    lockedUntilUtc?: string;
    reasons?: readonly string[];
    retryAfterSeconds?: number;
  };
}

interface ProblemDetailsBody {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  code?: string;
  message?: string;
  lockedUntilUtc?: string;
  reasons?: readonly string[];
  retryAfterSeconds?: number;
  [extension: string]: unknown;
}

function isProblemDetails(value: unknown): value is ProblemDetailsBody {
  return typeof value === 'object' && value !== null;
}

function buildAuthError(status: number, body: unknown, fallbackMessage: string): AuthError {
  const problem = isProblemDetails(body) ? body : {};
  const code =
    typeof problem.code === 'string' && problem.code.length > 0
      ? problem.code
      : `http.${status}`;
  const message =
    typeof problem.message === 'string' && problem.message.length > 0
      ? problem.message
      : typeof problem.detail === 'string' && problem.detail.length > 0
        ? problem.detail
        : fallbackMessage;

  const extensions: AuthError['extensions'] = {};
  if (typeof problem.lockedUntilUtc === 'string') {
    extensions.lockedUntilUtc = problem.lockedUntilUtc;
  }
  if (Array.isArray(problem.reasons)) {
    extensions.reasons = problem.reasons.filter((r): r is string => typeof r === 'string');
  }
  if (typeof problem.retryAfterSeconds === 'number' && Number.isFinite(problem.retryAfterSeconds)) {
    extensions.retryAfterSeconds = problem.retryAfterSeconds;
  }

  const error = new Error(message) as AuthError;
  error.name = 'AuthError';
  error.status = status;
  error.code = code;
  error.message = message;
  if (typeof problem.type === 'string') {
    error.type = problem.type;
  }
  if (Object.keys(extensions).length > 0) {
    error.extensions = extensions;
  }
  return error;
}

interface RequestOptions {
  /**
   * The auth interceptor MUST NOT try to refresh a token in response
   * to a 401 from the refresh endpoint itself (that would loop), nor
   * from the login endpoint (the user is by definition not logged in).
   * The `skipRefresh` flag tells the interceptor to bypass the
   * single-flight refresh queue and reject the original error.
   */
  skipRefresh?: boolean;
}

async function request<TRes>(
  method: 'POST',
  path: string,
  body: unknown,
  options: RequestOptions = {},
): Promise<TRes> {
  const client: ApiClient = apiClient;
  try {
    // We bypass `client.request`'s typed signature because we need
    // to attach our own __skipRefresh marker for the response
    // interceptor. The interceptor checks the property defensively.
    const response = await client.request<TRes>({
      method,
      url: path,
      data: body,
    } as Parameters<ApiClient['request']>[0] & { __skipRefresh?: boolean });
    // Mutate the captured config so the response interceptor can
    // read the marker without re-parsing the URL.
    (response.config as { __skipRefresh?: boolean }).__skipRefresh = options.skipRefresh === true;
    return response.data;
  } catch (raw) {
    const axiosError = raw as {
      response?: { status?: number; data?: unknown };
      message?: string;
      isAxiosError?: boolean;
    };
    if (axiosError?.isAxiosError === true && axiosError.response) {
      const status = axiosError.response.status ?? 0;
      throw buildAuthError(status, axiosError.response.data, axiosError.message ?? 'Request failed.');
    }
    // Network error or axios-internal failure.
    throw buildAuthError(0, null, axiosError?.message ?? 'Network error.');
  }
}

export function register(body: RegisterRequest): Promise<RegisterResponse> {
  return request<RegisterResponse>('POST', '/api/v1/auth/register', body);
}

export function verifyEmail(body: VerifyEmailRequest): Promise<void> {
  return request<void>('POST', '/api/v1/auth/verify-email', body);
}

export function login(body: LoginRequest): Promise<LoginResponse> {
  // The login endpoint is anonymous; never try to refresh on 401
  // here — that would mean the user's credentials are wrong, not
  // that the access token is stale.
  return request<LoginResponse>('POST', '/api/v1/auth/login', body, { skipRefresh: true });
}

export function refresh(body: RefreshRequest): Promise<TokenResponse> {
  // Same reason as login: a 401 from /refresh means the refresh
  // token itself is bad. There is nothing to refresh.
  return request<TokenResponse>('POST', '/api/v1/auth/refresh', body, { skipRefresh: true });
}

export function logout(): Promise<void> {
  return request<void>('POST', '/api/v1/auth/logout', {});
}

export function forgotPassword(body: ForgotPasswordRequest): Promise<void> {
  return request<void>('POST', '/api/v1/auth/forgot-password', body);
}

export function resetPassword(body: ResetPasswordRequest): Promise<void> {
  return request<void>('POST', '/api/v1/auth/reset-password', body);
}

export function changePassword(body: ChangePasswordRequest): Promise<void> {
  return request<void>('POST', '/api/v1/auth/change-password', body);
}

export function mfaSetup(): Promise<MfaSetupResponse> {
  return request<MfaSetupResponse>('POST', '/api/v1/auth/mfa/setup', {});
}

export function mfaVerify(body: MfaVerifyRequest): Promise<TokenResponse> {
  // /mfa/verify is the second half of an anonymous MFA login.
  // A 401 here means the TOTP code is wrong, not that the access
  // token is stale.
  return request<TokenResponse>('POST', '/api/v1/auth/mfa/verify', body, { skipRefresh: true });
}

export function mfaDisable(): Promise<void> {
  return request<void>('POST', '/api/v1/auth/mfa/disable', {});
}

/**
 * Convenience predicate used by the AuthErrorBanner to pick the
 * right localised message. Kept here so the message mapping is
 * independent of the React tree and easy to unit-test.
 */
export function describeAuthError(error: AuthError): 'invalidCredentials' | 'accountLocked' | 'emailNotVerified' | 'rateLimited' | 'tokenReplay' | 'network' | 'generic' {
  if (error.status === 0) {
    return 'network';
  }
  if (error.status === 401) {
    return 'invalidCredentials';
  }
  if (error.status === 410) {
    return 'tokenReplay';
  }
  if (error.status === 423) {
    return 'accountLocked';
  }
  if (error.status === 403) {
    return 'emailNotVerified';
  }
  if (error.status === 429) {
    return 'rateLimited';
  }
  return 'generic';
}
