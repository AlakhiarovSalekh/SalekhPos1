/**
 * Input smoke test — verifies the label/aria/hint/error contract.
 */
import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Input } from '@/components/ui/Input';

describe('Input', () => {
  it('renders a label associated with the input', () => {
    render(<Input label="Email" name="email" />);
    const input = screen.getByLabelText('Email');
    expect(input).toBeInTheDocument();
    expect(input.tagName).toBe('INPUT');
  });

  it('renders hint text and wires aria-describedby', () => {
    render(<Input label="Email" name="email" hint="We never share it." />);
    const input = screen.getByLabelText('Email');
    expect(input).toHaveAttribute('aria-describedby');
    expect(screen.getByText('We never share it.')).toBeInTheDocument();
  });

  it('marks the input invalid and announces the error', () => {
    render(<Input label="Email" name="email" error="Email is required." />);
    const input = screen.getByLabelText('Email');
    expect(input).toHaveAttribute('aria-invalid', 'true');
    expect(screen.getByRole('alert')).toHaveTextContent('Email is required.');
  });
});
