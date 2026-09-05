/**
 * AuthLayout smoke test — verifies the title, subtitle, and
 * footer slots are wired correctly.
 */
import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AuthLayout } from '@/features/auth/components/AuthLayout';

describe('AuthLayout', () => {
  it('renders the title in a heading', () => {
    render(<AuthLayout title="Sign in">body</AuthLayout>);
    expect(screen.getByRole('heading', { level: 2, name: 'Sign in' })).toBeInTheDocument();
  });

  it('renders the subtitle when provided', () => {
    render(
      <AuthLayout title="Sign in" subtitle="Welcome back.">
        body
      </AuthLayout>,
    );
    expect(screen.getByText('Welcome back.')).toBeInTheDocument();
  });

  it('renders the footer slot when provided', () => {
    render(
      <AuthLayout title="Sign in" footer={<a href="/register">Create one</a>}>
        body
      </AuthLayout>,
    );
    expect(screen.getByRole('link', { name: 'Create one' })).toBeInTheDocument();
  });
});
