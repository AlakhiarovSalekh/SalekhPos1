/**
 * AuthRoutes — the <Routes> fragment containing all 8 auth
 * routes. Each route is wrapped in the appropriate guard.
 */
import { Route, Routes } from 'react-router-dom';
import { AccountPage } from '@/features/auth/pages/AccountPage';
import { ChangePasswordPage } from '@/features/auth/pages/ChangePasswordPage';
import { ForgotPasswordPage } from '@/features/auth/pages/ForgotPasswordPage';
import { LoginPage } from '@/features/auth/pages/LoginPage';
import { MfaSetupPage } from '@/features/auth/pages/MfaSetupPage';
import { MfaVerifyPage } from '@/features/auth/pages/MfaVerifyPage';
import { RegisterPage } from '@/features/auth/pages/RegisterPage';
import { ResetPasswordPage } from '@/features/auth/pages/ResetPasswordPage';
import { VerifyEmailPage } from '@/features/auth/pages/VerifyEmailPage';
import { RequireAnonymous, RequireAuth, RequireMfaPending } from './guards';

export function AuthRoutes() {
  return (
    <Routes>
      <Route
        path="/login"
        element={
          <RequireAnonymous>
            <LoginPage />
          </RequireAnonymous>
        }
      />
      <Route
        path="/register"
        element={
          <RequireAnonymous>
            <RegisterPage />
          </RequireAnonymous>
        }
      />
      <Route
        path="/verify-email"
        element={
          <RequireAnonymous>
            <VerifyEmailPage />
          </RequireAnonymous>
        }
      />
      <Route
        path="/forgot-password"
        element={
          <RequireAnonymous>
            <ForgotPasswordPage />
          </RequireAnonymous>
        }
      />
      <Route
        path="/reset-password"
        element={
          <RequireAnonymous>
            <ResetPasswordPage />
          </RequireAnonymous>
        }
      />
      <Route
        path="/mfa/verify"
        element={
          <RequireMfaPending>
            <MfaVerifyPage />
          </RequireMfaPending>
        }
      />
      <Route
        path="/mfa/setup"
        element={
          <RequireAuth>
            <MfaSetupPage />
          </RequireAuth>
        }
      />
      <Route
        path="/account"
        element={
          <RequireAuth>
            <AccountPage />
          </RequireAuth>
        }
      />
      <Route
        path="/account/change-password"
        element={
          <RequireAuth>
            <ChangePasswordPage />
          </RequireAuth>
        }
      />
    </Routes>
  );
}
