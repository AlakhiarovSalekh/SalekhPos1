/**
 * AuthErrorBanner — translates an AuthError into the right Alert
 * variant and a localised message. The HTTP status drives the
 * choice; the message itself comes from i18n so the three
 * locales stay in sync.
 */
import { useTranslation } from 'react-i18next';
import { Alert } from '@/components/ui/Alert';
import { describeAuthError, type AuthError } from '@/services/auth/authApi';

export interface AuthErrorBannerProps {
  error: AuthError | null;
}

export function AuthErrorBanner({ error }: AuthErrorBannerProps) {
  const { t } = useTranslation();
  if (!error) {
    return null;
  }
  const kind = describeAuthError(error);
  switch (kind) {
    case 'invalidCredentials':
      return <Alert variant="error">{t('auth.errors.invalidCredentials')}</Alert>;
    case 'accountLocked':
      return (
        <Alert variant="error">
          {t('auth.errors.accountLocked', {
            until: error.extensions?.lockedUntilUtc
              ? new Date(error.extensions.lockedUntilUtc).toLocaleString()
              : t('common.unknown'),
          })}
        </Alert>
      );
    case 'emailNotVerified':
      return <Alert variant="warning">{t('auth.errors.emailNotVerified')}</Alert>;
    case 'rateLimited':
      return (
        <Alert variant="warning">
          {t('auth.errors.rateLimited', { seconds: error.extensions?.retryAfterSeconds ?? 0 })}
        </Alert>
      );
    case 'tokenReplay':
      return <Alert variant="error">{t('auth.errors.tokenReplay')}</Alert>;
    case 'network':
      return <Alert variant="error">{t('auth.errors.network')}</Alert>;
    case 'generic':
    default:
      return <Alert variant="error">{error.message || t('auth.errors.generic')}</Alert>;
  }
}
