/**
 * AccountPage — `/account`. <RequireAuth>. The signed-in landing
 * page. Shows the current user's identity, links to the change-
 * password and MFA-setup pages, and provides a sign-out button.
 * This page replaces the old BusinessShell placeholder so we can
 * prove the full authenticated tree works end-to-end.
 */
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/Button';
import { useAuth } from '@/features/auth/state/useAuth';

export function AccountPage() {
  const { t } = useTranslation();
  const auth = useAuth();
  if (!auth.user) {
    return null;
  }
  return (
    <main className="auth-shell">
      <section className="auth-shell__card" aria-labelledby="account-title">
        <h2 id="account-title" className="auth-shell__title">
          {t('auth.account.title')}
        </h2>
        <p className="auth-shell__subtitle">{t('auth.account.subtitle')}</p>
        <dl className="auth-shell__form">
          <div>
            <dt className="ui-field__hint">{t('auth.account.fields.userId.label')}</dt>
            <dd className="auth-mono"><code>{auth.user.userId}</code></dd>
          </div>
          <div>
            <dt className="ui-field__hint">{t('auth.account.fields.tenantId.label')}</dt>
            <dd className="auth-mono"><code>{auth.user.tenantId}</code></dd>
          </div>
          <div>
            <dt className="ui-field__hint">{t('auth.account.fields.role.label')}</dt>
            <dd className="auth-mono"><code>{auth.user.role}</code></dd>
          </div>
          <div>
            <dt className="ui-field__hint">{t('auth.account.fields.tokenVersion.label')}</dt>
            <dd className="auth-mono"><code>{auth.user.tokenVersion}</code></dd>
          </div>
        </dl>
        <div className="auth-shell__footer">
          <Link to="/account/change-password">{t('auth.account.actions.changePassword')}</Link>
          <Link to="/mfa/setup">{t('auth.account.actions.setupMfa')}</Link>
        </div>
        <Button variant="secondary" onClick={() => void auth.signOut()}>
          {t('auth.account.actions.signOut')}
        </Button>
      </section>
    </main>
  );
}
