/**
 * AuthLayout — the centered card used by every auth page. Renders
 * the app brand at the top, the page title, an optional subtitle,
 * the page body, and an optional footer link line. The page
 * itself owns the form; this component only owns the chrome.
 */
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';

export interface AuthLayoutProps {
  title: string;
  subtitle?: string;
  children: ReactNode;
  footer?: ReactNode;
}

export function AuthLayout({ title, subtitle, children, footer }: AuthLayoutProps) {
  const { t } = useTranslation();
  return (
    <main className="auth-shell">
      <header className="auth-shell__header">
        <h1 className="auth-shell__brand">{t('app.name')}</h1>
        <p className="auth-shell__tagline">{t('app.tagline')}</p>
      </header>
      <section className="auth-shell__card" aria-labelledby="auth-title">
        <h2 id="auth-title" className="auth-shell__title">
          {title}
        </h2>
        {subtitle && <p className="auth-shell__subtitle">{subtitle}</p>}
        {children}
        {footer && <div className="auth-shell__footer">{footer}</div>}
      </section>
    </main>
  );
}
