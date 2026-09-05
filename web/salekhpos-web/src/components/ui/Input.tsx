/**
 * Input — labeled text field with optional hint and error. Forwards
 * refs so react-hook-form's `register({ ref })` and similar APIs
 * attach to the underlying <input> element. The label is
 * always rendered; required/optional is conveyed through the
 * form-level validation messages, not the asterisk.
 */
import { forwardRef, useId } from 'react';
import type { InputHTMLAttributes, ReactNode } from 'react';
import clsx from 'clsx';

export interface InputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'size'> {
  label: string;
  hint?: ReactNode;
  error?: ReactNode;
  block?: boolean;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { label, hint, error, block = false, id, className, ...rest },
  ref,
) {
  const generatedId = useId();
  const inputId = id ?? generatedId;
  const hintId = hint ? `${inputId}-hint` : undefined;
  const errorId = error ? `${inputId}-error` : undefined;
  const describedBy = [hintId, errorId].filter((value): value is string => Boolean(value)).join(' ') || undefined;

  return (
    <div className={clsx('ui-field', block && 'ui-field--block', className)}>
      <label htmlFor={inputId} className="ui-field__label">
        {label}
      </label>
      <input
        ref={ref}
        id={inputId}
        className={clsx('ui-field__input', error && 'ui-field__input--invalid')}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy}
        {...rest}
      />
      {hint && (
        <p id={hintId} className="ui-field__hint">
          {hint}
        </p>
      )}
      {error && (
        <p id={errorId} className="ui-field__error" role="alert">
          {error}
        </p>
      )}
    </div>
  );
});
