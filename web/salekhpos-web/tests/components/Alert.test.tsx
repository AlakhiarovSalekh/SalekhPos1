/**
 * Alert smoke test — verifies the role mapping per variant.
 */
import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Alert } from '@/components/ui/Alert';

describe('Alert', () => {
  it('uses role="alert" for error', () => {
    render(<Alert variant="error">Something went wrong.</Alert>);
    const node = screen.getByRole('alert');
    expect(node).toHaveTextContent('Something went wrong.');
  });

  it('uses role="alert" for warning', () => {
    render(<Alert variant="warning">Check your inbox.</Alert>);
    expect(screen.getByRole('alert')).toBeInTheDocument();
  });

  it('uses role="status" for success', () => {
    render(<Alert variant="success">Welcome.</Alert>);
    expect(screen.getByRole('status')).toBeInTheDocument();
  });

  it('renders a title when provided', () => {
    render(
      <Alert variant="info" title="Heads up">
        Some context.
      </Alert>,
    );
    expect(screen.getByText('Heads up')).toBeInTheDocument();
  });
});
