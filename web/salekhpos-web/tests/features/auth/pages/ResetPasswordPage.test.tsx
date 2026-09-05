/**
 * ResetPasswordPage test — asserts the submit button is disabled
 * when no token is in the URL, and the form fields render.
 */
import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../testUtils';
import { ResetPasswordPage } from '@/features/auth/pages/ResetPasswordPage';

vi.mock('@/services/auth/authApi', async () => {
  const actual = await vi.importActual('@/services/auth/authApi');
  return {
    ...actual,
    resetPassword: vi.fn(),
    refresh: vi.fn(),
  };
});

import * as authApi from '@/services/auth/authApi';

describe('ResetPasswordPage', () => {
  it('renders both password fields', () => {
    renderWithProviders(<ResetPasswordPage />, { route: '/reset-password?token=opaque' });
    expect(screen.getByLabelText(/^new password$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^confirm new password$/i)).toBeInTheDocument();
  });

  it('disables submit when no token is in the URL', () => {
    renderWithProviders(<ResetPasswordPage />, { route: '/reset-password' });
    const submit = screen.getByRole('button', { name: /update password/i }) as HTMLButtonElement;
    expect(submit).toBeDisabled();
    expect(authApi.resetPassword).not.toHaveBeenCalled();
  });
});
