/**
 * ForgotPasswordPage test — asserts render and submission.
 */
import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../testUtils';
import { ForgotPasswordPage } from '@/features/auth/pages/ForgotPasswordPage';

vi.mock('@/services/auth/authApi', async () => {
  const actual = await vi.importActual('@/services/auth/authApi');
  return {
    ...actual,
    forgotPassword: vi.fn(),
    refresh: vi.fn(),
  };
});

import * as authApi from '@/services/auth/authApi';

describe('ForgotPasswordPage', () => {
  it('renders the email field and submit button', () => {
    renderWithProviders(<ForgotPasswordPage />, { route: '/forgot-password' });
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /send reset link/i })).toBeInTheDocument();
  });

  it('does not call forgotPassword when validation fails', () => {
    renderWithProviders(<ForgotPasswordPage />, { route: '/forgot-password' });
    (screen.getByRole('button', { name: /send reset link/i }) as HTMLButtonElement).click();
    expect(authApi.forgotPassword).not.toHaveBeenCalled();
  });
});
