/**
 * SalekhPos web — application bootstrap.
 *
 * This file wires up the providers, the router, the API client, and the
 * realtime client. The Vite entry point. Keep this file small.
 */
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './app/App';
import { AppProviders } from './app/providers/AppProviders';
import { configureI18n } from './i18n';
import './styles/global.css';

configureI18n();

const rootElement = document.getElementById('root');
if (!rootElement) {
  throw new Error('Root element not found.');
}

createRoot(rootElement).render(
  <StrictMode>
    <AppProviders>
      <App />
    </AppProviders>
  </StrictMode>,
);
