/**
 * Route guards. Three small components, each ~10 lines:
 *   <RequireAuth>          — user must be authenticated.
 *   <RequireAnonymous>     — user must NOT be authenticated.
 *   <RequireMfaPending>    — user must have an in-flight MFA challenge.
 *
 * Each one short-circuits with a <Navigate />, preserving the
 * intended destination so the user lands where they wanted
 * after sign-in.
 */
import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '@/features/auth/state/useAuth';

export interface RequireAuthProps {
  children: ReactNode;
}

export function RequireAuth({ children }: RequireAuthProps) {
  const auth = useAuth();
  const location = useLocation();
  if (auth.status === 'loading') {
    return null;
  }
  if (auth.status !== 'authenticated' || !auth.user) {
    const target = `/login?returnTo=${encodeURIComponent(location.pathname + location.search)}`;
    return <Navigate to={target} replace state={{ from: location }} />;
  }
  return <>{children}</>;
}

export interface RequireAnonymousProps {
  children: ReactNode;
}

export function RequireAnonymous({ children }: RequireAnonymousProps) {
  const auth = useAuth();
  const location = useLocation();
  if (auth.status === 'loading') {
    return null;
  }
  if (auth.status === 'authenticated') {
    const returnTo = new URLSearchParams(location.search).get('returnTo') ?? '/';
    return <Navigate to={returnTo} replace />;
  }
  return <>{children}</>;
}

export interface RequireMfaPendingProps {
  children: ReactNode;
}

export function RequireMfaPending({ children }: RequireMfaPendingProps) {
  const auth = useAuth();
  if (auth.status === 'loading') {
    return null;
  }
  if (auth.status !== 'mfa-pending' || !auth.mfaPending) {
    return <Navigate to="/login" replace />;
  }
  return <>{children}</>;
}
