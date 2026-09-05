/**
 * VerifyEmailPage — `/verify-email?token=…`. Calls the verify
 * endpoint on mount. Renders a success or error state. The page
 * is anonymous (RequireAnonymous).
 */
import { useEffect, useRef, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { AuthErrorBanner } from '@/features/auth/components/AuthErrorBanner';
import { AuthLayout } from '@/features/auth/components/AuthLayout';
import { useAuth } from '@/features/auth/state/useAuth';
import { isAuthError, type AuthError } from '@/features/auth/state/AuthContext';

export function VerifyEmailPage() {
  const { t } = useTranslation();
  const [params] = useSearchParams();
  const token = params.get('token') ?? '';
  const auth = useAuth();
  const [error, setError] = useState<AuthError | null>(null);
  const [success, setSuccess] = useState(false);
  const ranRef = useRef(false);

  useEffect(() => {
    if (ranRef.current) {
      return;
    }
    ranRef.current = true;
    if (!token) {
      const err = new Error(t('auth.verifyEmail.errorBody')) as AuthError;
      err.name = 'AuthError';
      err.status = 400;
      err.code = 'auth.missing_token';
      setError(err);
      return;
    }
    let cancelled = false;
    (async () => {
      try {
        await auth.verifyEmail({ token });
        if (!cancelled) {
          setSuccess(true);
        }
      } catch (raw) {
        if (!cancelled) {
          setError(isAuthError(raw) ? raw : null);
        }
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [token, auth, t]);

  return (
    <AuthLayout title={t('auth.verifyEmail.title')}>
      <AuthErrorBanner error={error} />
      {success && (
        <>
          <p className="auth-shell__subtitle">{t('auth.verifyEmail.successBody')}</p>
          <p>
            <Link to="/login">{t('common.continue')}</Link>
          </p>
        </>
      )}
    </AuthLayout>
  );
}
