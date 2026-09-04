/**
 * Smoke test: the BusinessShell placeholder renders without crashing
 * and the i18n strings are present.
 */
import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BusinessShell } from '@/components/layout/BusinessShell';

describe('BusinessShell', () => {
  it('renders the application name', () => {
    render(<BusinessShell />);
    expect(screen.getByRole('heading', { level: 1, name: /SalekhPos/i })).toBeInTheDocument();
  });
});
