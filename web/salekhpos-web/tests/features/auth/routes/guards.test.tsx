/**
 * Guards test — verifies each guard renders without crashing
 * when used inside <AuthProvider>. The authApi is mocked because
 * the AuthProvider calls refresh on mount if a stored session is
 * found.
 */
import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../testUtils';
import { RequireAnonymous, RequireAuth, RequireMfaPending } from '@/features/auth/routes/guards';

vi.mock('@/services/auth/authApi', async () => {
  const actual = await vi.importActual('@/services/auth/authApi');
  return {
    ...actual,
    refresh: vi.fn(),
  };
});

describe('RequireAnonymous', () => {
  it('renders children when status is anonymous (default after mount)', () => {
    renderWithProviders(
      <RequireAnonymous>
        <div>anon content</div>
      </RequireAnonymous>,
    );
    expect(screen.getByText('anon content')).toBeInTheDocument();
  });
});

describe('RequireAuth', () => {
  it('renders without crashing when used inside AuthProvider', () => {
    renderWithProviders(
      <RequireAuth>
        <div>auth content</div>
      </RequireAuth>,
    );
    // Without a session the guard returns <Navigate>. The assertion
    // is simply that no exception is thrown and the tree is
    // navigable.
    expect(document.body).toBeInTheDocument();
  });
});

describe('RequireMfaPending', () => {
  it('renders without crashing when there is no in-flight mfa session', () => {
    renderWithProviders(
      <RequireMfaPending>
        <div>mfa content</div>
      </RequireMfaPending>,
    );
    expect(document.body).toBeInTheDocument();
  });
});
