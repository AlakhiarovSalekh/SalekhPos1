/**
 * Button — the primary call-to-action control. Forwards refs so
 * forms and focus-management libraries can imperatively focus the
 * element. The default `type` is "button" to avoid accidental
 * form submission; pages that need "submit" pass it explicitly.
 */
import { forwardRef } from 'react';
import type { ButtonHTMLAttributes, ReactNode } from 'react';
import clsx from 'clsx';
import { Spinner } from './Spinner';

export type ButtonVariant = 'primary' | 'secondary' | 'ghost';
export type ButtonSize = 'sm' | 'md';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  loading?: boolean;
  block?: boolean;
  children: ReactNode;
}

const variantClass: Record<ButtonVariant, string> = {
  primary: 'ui-button--primary',
  secondary: 'ui-button--secondary',
  ghost: 'ui-button--ghost',
};

const sizeClass: Record<ButtonSize, string> = {
  sm: 'ui-button--sm',
  md: 'ui-button--md',
};

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(function Button(
  {
    variant = 'primary',
    size = 'md',
    loading = false,
    block = false,
    type = 'button',
    disabled,
    className,
    children,
    ...rest
  },
  ref,
) {
  const isDisabled = disabled === true || loading;
  return (
    <button
      ref={ref}
      type={type}
      className={clsx('ui-button', variantClass[variant], sizeClass[size], block && 'ui-button--block', className)}
      disabled={isDisabled}
      aria-busy={loading}
      {...rest}
    >
      {loading && (
        <span className="ui-button__spinner" aria-hidden="true">
          <Spinner size="sm" />
        </span>
      )}
      <span className="ui-button__label">{children}</span>
    </button>
  );
});
