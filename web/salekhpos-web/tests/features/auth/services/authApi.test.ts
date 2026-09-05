/**
 * authApi test — verifies the describeAuthError mapping, the
 * only piece of authApi that is a pure function and easy to
 * unit-test in isolation. The HTTP calls themselves are
 * exercised end-to-end in the Playwright smoke (Slice 5 H).
 */
import { describe, expect, it } from 'vitest';
import { describeAuthError, type AuthError } from '@/services/auth/authApi';

function makeError(overrides: Partial<AuthError>): AuthError {
  const err = new Error('boom') as AuthError;
  err.name = 'AuthError';
  err.status = 0;
  err.code = 'http.0';
  err.message = 'boom';
  return Object.assign(err, overrides);
}

describe('describeAuthError', () => {
  it('returns "network" for status 0', () => {
    expect(describeAuthError(makeError({ status: 0 }))).toBe('network');
  });
  it('returns "invalidCredentials" for 401', () => {
    expect(describeAuthError(makeError({ status: 401 }))).toBe('invalidCredentials');
  });
  it('returns "tokenReplay" for 410', () => {
    expect(describeAuthError(makeError({ status: 410 }))).toBe('tokenReplay');
  });
  it('returns "accountLocked" for 423', () => {
    expect(describeAuthError(makeError({ status: 423 }))).toBe('accountLocked');
  });
  it('returns "emailNotVerified" for 403', () => {
    expect(describeAuthError(makeError({ status: 403 }))).toBe('emailNotVerified');
  });
  it('returns "rateLimited" for 429', () => {
    expect(describeAuthError(makeError({ status: 429 }))).toBe('rateLimited');
  });
  it('returns "generic" for other statuses', () => {
    expect(describeAuthError(makeError({ status: 418 }))).toBe('generic');
  });
});
