/**
 * MfaSetupPage — `/mfa/setup`. <RequireAuth>. Calls setupMfa on
 * mount and renders the provisioning URI, the secret, and the
 * recovery codes. The QR code itself is not generated in this
 * slice — the user is told to paste the otpauth URI into their
 * authenticator (most apps accept the URI as a deep link, and
 * the secret alone works as a manual entry).
 */
import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui';
import { AuthErrorBanner } from '@/features/auth/components/AuthErrorBanner';
import { AuthLayout } from '@/features/auth/components/AuthLayout';
import { useAuth } from '@/features/auth/state/useAuth';
import type { MfaSetupResponse } from '@/services/auth/authApi';

function extractSecret(uri: string): string {
  // otpauth://totp/Issuer:Account?secret=BASE32&issuer=Issuer&...
  const match = uri.match(/secret=([A-Z2-7]+)/i);
  return match?.[1] ?? uri;
}

export function MfaSetupPage() {
  const { t } = useTranslation();
  const auth = useAuth();
  const [data, setData] = useState<MfaSetupResponse | null>(null);
  const [error, setError] = useState<unknown>(null);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const result = await auth.setupMfa();
        if (!cancelled) {
          setData(result);
        }
      } catch (raw) {
        if (!cancelled) {
          setError(raw);
        }
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [auth]);

  if (!data) {
    return (
      <AuthLayout title={t('auth.mfa.setup.title')}>
        <AuthErrorBanner error={error as never} />
        <p className="auth-shell__subtitle">{t('auth.mfa.setup.body')}</p>
      </AuthLayout>
    );
  }

  const secret = extractSecret(data.provisioningUri);

  return (
    <AuthLayout title={t('auth.mfa.setup.title')}>
      <p className="auth-shell__subtitle">{t('auth.mfa.setup.body')}</p>
      <div>
        <p className="ui-field__hint">{t('auth.mfa.setup.secretLabel')}</p>
        <p className="auth-mono">
          <code>{secret}</code>
        </p>
      </div>
      <div>
        <p className="ui-field__hint">{t('auth.mfa.setup.provisioningUriLabel')}</p>
        <p className="auth-mono">
          <code>{data.provisioningUri}</code>
        </p>
      </div>
      <div>
        <p className="ui-field__hint">{t('auth.mfa.setup.recoveryCodesLabel')}</p>
        <ul className="auth-recovery-codes">
          {data.recoveryCodes.map((code) => (
            <li key={code}>{code}</li>
          ))}
        </ul>
        <p className="ui-field__hint">{t('auth.mfa.setup.recoveryCodesWarning')}</p>
      </div>
      <Button onClick={() => window.print()} variant="secondary">
        {t('auth.mfa.setup.printButton')}
      </Button>
    </AuthLayout>
  );
}
