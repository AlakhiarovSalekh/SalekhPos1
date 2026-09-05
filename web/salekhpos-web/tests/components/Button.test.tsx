/**
 * Button smoke test — verifies the documented ARIA and behaviour
 * contract without depending on CSS. The test will run as the
 * component is used in the auth pages.
 */
import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Button } from '@/components/ui/Button';

describe('Button', () => {
  it('renders its children and is enabled by default', () => {
    render(<Button>Sign in</Button>);
    const button = screen.getByRole('button', { name: /sign in/i });
    expect(button).toBeEnabled();
    expect(button).toHaveAttribute('type', 'button');
  });

  it('does not submit forms by default (type=button)', () => {
    render(
      <form>
        <Button>Sign in</Button>
      </form>,
    );
    const button = screen.getByRole('button', { name: /sign in/i });
    expect(button).toHaveAttribute('type', 'button');
  });

  it('exposes type=submit when requested', () => {
    render(
      <form>
        <Button type="submit">Sign in</Button>
      </form>,
    );
    const button = screen.getByRole('button', { name: /sign in/i });
    expect(button).toHaveAttribute('type', 'submit');
  });

  it('is disabled and aria-busy when loading', () => {
    render(<Button loading>Sign in</Button>);
    const button = screen.getByRole('button', { name: /sign in/i });
    expect(button).toBeDisabled();
    expect(button).toHaveAttribute('aria-busy', 'true');
  });

  it('is disabled when the disabled prop is set', () => {
    render(<Button disabled>Sign in</Button>);
    expect(screen.getByRole('button', { name: /sign in/i })).toBeDisabled();
  });

  it('invokes onClick when clicked', async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();
    render(<Button onClick={onClick}>Sign in</Button>);
    await user.click(screen.getByRole('button', { name: /sign in/i }));
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it('does not invoke onClick when loading', async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();
    render(
      <Button loading onClick={onClick}>
        Sign in
      </Button>,
    );
    await user.click(screen.getByRole('button', { name: /sign in/i }));
    expect(onClick).not.toHaveBeenCalled();
  });
});
