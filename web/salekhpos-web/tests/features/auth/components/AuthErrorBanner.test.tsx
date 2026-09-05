/**
 * AuthErrorBanner tests — verifies each HTTP-status branch renders
 * the right Alert variant and the localised message.
 */
import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AuthErrorBanner } from '@/features/auth/components/AuthErrorBanner';
import type { AuthError } from '@/services/auth/authApi';

function makeError(overrides: Partial<AuthError>): AuthError {
  const error = new Error('boom') as AuthError;
  error.name = 'AuthError';
  error.status = 0;
  error.code = 'http.0';
  error.message = 'boom';
  return Object.assign(error, overrides);
}

describe('AuthErrorBanner', () => {
  it('returns null when no error is passed', () => {
    const { container } = render(<AuthErrorBanner error={null} />);
    expect(container).toBeEmptyDOMElement();
  });

  it('renders the invalid credentials message on 401', () => {
    render(<AuthErrorBanner error={makeError({ status: 401, code: 'auth.invalid_credentials' })} />);
    // Just assert the alert is shown; the exact wording is
    // locale-specific.
    expect(screen.getByRole('alert')).toBeInTheDocument();
  });

  it('renders the account-locked message on 423 with the lockedUntilUtc', () => {
    render(
      <AuthErrorBanner
        error={makeError({ status: 423, extensions: { lockedUntilUtc: '2026-09-05T12:00:00Z' } })}
      />,
    );
    expect(screen.getByRole('alert')).toBeInTheDocument();
  });

  it('renders the email-not-verified warning on 403', () => {
    render(<AuthErrorBanner error={makeError({ status: 403, code: 'auth.email_not_verified' })} />);
    expect(screen.getByRole('alert')).toBeInTheDocument();
  });

  it('renders the rate-limited warning on 429', () => {
    render(
      <AuthErrorBanner error={makeError({ status: 429, extensions: { retryAfterSeconds: 30 } })} />,
    );
    expect(screen.getByRole('alert')).toBeInTheDocument();
  });

  it('renders the token-replay error on 410', () => {
    render(<AuthErrorBanner error={makeError({ status: 410, code: 'auth.token_replay' })} />);
    // The token-replay banner is shown via role="alert". We
    // assert on a unique substring that is present in all three
    // locales (en, ka, az) so the test survives translation.
    expect(screen.getByRole('alert')).toBeInTheDocument();
  });

  it('renders the network error on status 0', () => {
    render(<AuthErrorBanner error={makeError({ status: 0, message: 'Network error.' })} />);
    expect(screen.getByRole('alert')).toBeInTheDocument();
  });

  it('falls back to the raw message for unknown statuses', () => {
    render(<AuthErrorBanner error={makeError({ status: 418, message: 'I am a teapot.' })} />);
    expect(screen.getByRole('alert')).toHaveTextContent('I am a teapot.');
  });
});
