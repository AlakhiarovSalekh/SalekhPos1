/**
 * Router — top-level <BrowserRouter> + <Routes>. The auth feature
 * owns the auth routes; the root path is a temporary redirect
 * to /account while the rest of the business pages are
 * scaffolded in later slices. The catch-all path renders the
 * 404 page.
 */
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthRoutes } from '@/features/auth/routes/AuthRoutes';
import { NotFoundPage } from '@/pages/NotFoundPage';

export function Router() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/account" replace />} />
        <Route path="/auth/*" element={<AuthRoutes />} />
        <Route path="/*" element={<AuthRoutes />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </BrowserRouter>
  );
}
