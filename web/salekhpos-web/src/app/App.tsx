/**
 * SalekhPos web — top-level App component.
 *
 * In the foundation scaffold this renders the BusinessShell placeholder.
 * Real routes (auth, dashboard, products, inventory, ...) are added in
 * later phases.
 */
import { BusinessShell } from '@/components/layout/BusinessShell';

export default function App(): JSX.Element {
  return <BusinessShell />;
}
