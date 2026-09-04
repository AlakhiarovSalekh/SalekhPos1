/**
 * BusinessShell — the authenticated web app shell.
 *
 * In the foundation scaffold this is a minimal placeholder that demonstrates
 * the i18n + state hooks. The full header / sidebar / business selector /
 * store selector / global search / notifications / profile layout is added
 * in a later phase. The shell is intentionally a thin component; the page
 * content is composed in by the router.
 */
import { useTranslation } from 'react-i18next';
import { config } from '@/app/config/config';

export function BusinessShell(): JSX.Element {
  const { t } = useTranslation();

  return (
    <main
      style={{
        minHeight: '100vh',
        background: '#0b1220',
        color: '#e2e8f0',
        fontFamily: 'system-ui, -apple-system, "Segoe UI", Roboto, sans-serif',
        padding: '2rem',
      }}
    >
      <header style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
        <h1 style={{ fontSize: '1.5rem', margin: 0 }}>{t('app.name')}</h1>
        <span style={{ color: '#94a3b8' }}>{t('app.tagline')}</span>
      </header>

      <section
        style={{
          marginTop: '2rem',
          padding: '1.5rem',
          background: '#111a2e',
          borderRadius: '0.75rem',
          border: '1px solid #1e293b',
        }}
      >
        <h2 style={{ marginTop: 0 }}>{t('dashboard.title')}</h2>
        <p style={{ color: '#94a3b8' }}>{t('states.empty')}</p>
        <code style={{ display: 'block', marginTop: '1rem', color: '#64748b' }}>
          API: {config.apiBaseUrl}
        </code>
      </section>
    </main>
  );
}
