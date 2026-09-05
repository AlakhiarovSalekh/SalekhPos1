/**
 * SalekhPos web — top-level App component.
 *
 * Wires the top-level router. Providers live in AppProviders so
 * the React tree looks like:
 *   <AppProviders>
 *     <App>      ← Router
 *     </App>
 *   </AppProviders>
 *
 * AppProviders owns QueryClient + AuthProvider. The router is
 * rendered here (not in AppProviders) so the router can own
 * its own context without conflicting with the providers' tree.
 */
import { Router } from '@/routes/Router';

export default function App() {
  return <Router />;
}
