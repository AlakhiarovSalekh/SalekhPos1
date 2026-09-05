/**
 * Spinner smoke test — verifies the aria-label is always present.
 */
import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Spinner } from '@/components/ui/Spinner';

describe('Spinner', () => {
  it('uses a default aria-label when none is provided', () => {
    render(<Spinner />);
    expect(screen.getByRole('status')).toHaveAccessibleName('Loading');
  });

  it('uses a custom aria-label when provided', () => {
    render(<Spinner label="Signing you in" />);
    expect(screen.getByRole('status')).toHaveAccessibleName('Signing you in');
  });
});
