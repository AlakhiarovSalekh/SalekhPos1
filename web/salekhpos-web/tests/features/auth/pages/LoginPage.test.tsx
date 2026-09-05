/**
 * LoginPage test. The test file hoists the authApi mock to the
 * top of the module so the AuthProvider's calls are intercepted
 * before any module-level code runs.
 */
import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../testUtils';
import { LoginPage } from '@/features/auth/pages/LoginPage';

vi.mock('@/services/auth/authApi', async () => {
  const actual = await vi.importActual('@/services/auth/authApi');
  return {
    ...actual,
    login: vi.fn(),
    refresh: vi.fn(),
  };
});

import * as authApi from '@/services/auth/authApi';

describe('LoginPage', () => {
  it('renders the title, fields and submit button', () => {
    renderWithProviders(<LoginPage />, { route: '/login' });
    expect(screen.getByRole('heading', { level: 2, name: /sign in/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/^email$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
  });

  it('shows validation error for invalid email', () => {
    renderWithProviders(<LoginPage />, { route: '/login' });
    const emailInput = screen.getByLabelText(/^email$/i) as HTMLInputElement;
    const nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value')?.set;
    nativeInputValueSetter?.call(emailInput, 'not-an-email');
    emailInput.dispatchEvent(new Event('input', { bubbles: true }));
    (screen.getByRole('button', { name: /sign in/i }) as HTMLButtonElement).click();
    // Validation fails before authApi.login is called.
    expect(authApi.login).not.toHaveBeenCalled();
  });
});
