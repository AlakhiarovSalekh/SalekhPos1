/**
 * PasswordField tests — verifies the show/hide toggle changes the
 * input type and updates aria-pressed.
 */
import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { PasswordField } from '@/features/auth/components/PasswordField';

describe('PasswordField', () => {
  it('renders a password input by default', () => {
    render(<PasswordField label="Password" name="password" />);
    const input = screen.getByLabelText('Password');
    expect(input).toHaveAttribute('type', 'password');
  });

  it('toggles between password and text on click', async () => {
    const user = userEvent.setup();
    render(<PasswordField label="Password" name="password" />);
    const input = screen.getByLabelText('Password');
    const toggle = screen.getByRole('button', { name: /show/i });
    await user.click(toggle);
    expect(input).toHaveAttribute('type', 'text');
    expect(screen.getByRole('button', { name: /hide/i })).toHaveAttribute('aria-pressed', 'true');
  });
});
