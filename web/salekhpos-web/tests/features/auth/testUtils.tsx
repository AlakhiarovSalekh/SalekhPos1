/**
 * Test helpers for the auth feature. Centralised here so every
 * page test wires the AuthProvider + Router the same way and
 * avoids re-implementing the same boilerplate.
 *
 * How mocking works here:
 *   - Every page test file does `vi.mock('@/services/auth/authApi', ...)`
 *     at the top level (vitest hoists mocks to the top).
 *   - The page test then uses the mock functions directly to
 *     assert what the page called.
 *   - This file does NOT call vi.mock; it just provides the
 *     render helper.
 */
import { type ReactNode } from 'react';
import { MemoryRouter } from 'react-router-dom';
import { render, type RenderOptions } from '@testing-library/react';
import { AuthProvider } from '@/features/auth/state/AuthProvider';

export interface RenderWithProvidersOptions extends Omit<RenderOptions, 'wrapper'> {
  route?: string;
}

export function renderWithProviders(
  ui: ReactNode,
  { route = '/', ...options }: RenderWithProvidersOptions = {},
) {
  return render(
    <MemoryRouter initialEntries={[route]}>
      <AuthProvider>{ui}</AuthProvider>
    </MemoryRouter>,
    options,
  );
}

export { screen } from '@testing-library/react';
