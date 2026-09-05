/**
 * NotFoundPage — 404 fallback. Shown for any path the router
 * does not match. Renders inside the auth shell so it stays
 * consistent with the rest of the unauthenticated tree.
 */
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { AuthLayout } from '@/features/auth/components/AuthLayout';

export function NotFoundPage() {
  const { t } = useTranslation();
  return (
    <AuthLayout
      title={t('notFound.title')}
      footer={
        <Link to="/">{t('notFound.homeLink')}</Link>
      }
    >
      <p className="auth-shell__subtitle">{t('notFound.body')}</p>
    </AuthLayout>
  );
}
