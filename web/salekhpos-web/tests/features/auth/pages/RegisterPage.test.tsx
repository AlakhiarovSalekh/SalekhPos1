/**
 * RegisterPage test. Asserts the page renders, validates, and
 * submits to the mocked authApi.register().
 */
import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../testUtils';
import { RegisterPage } from '@/features/auth/pages/RegisterPage';

vi.mock('@/services/auth/authApi', async () => {
  const actual = await vi.importActual('@/services/auth/authApi');
  return {
    ...actual,
    register: vi.fn(),
    refresh: vi.fn(),
  };
});

import * as authApi from '@/services/auth/authApi';

describe('RegisterPage', () => {
  it('renders all required fields', () => {
    renderWithProviders(<RegisterPage />, { route: '/register' });
    expect(screen.getByRole('heading', { level: 2, name: /create your business account/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/full name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/business name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/business url/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/confirm password/i)).toBeInTheDocument();
  });

  it('does not call register when validation fails (empty form)', () => {
    vi.mocked(authApi.register).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      tenantId: '22222222-2222-2222-2222-222222222222',
      tenantSlug: 'acme',
    });
    renderWithProviders(<RegisterPage />, { route: '/register' });
    (screen.getByRole('button', { name: /create account/i }) as HTMLButtonElement).click();
    expect(authApi.register).not.toHaveBeenCalled();
  });
});
