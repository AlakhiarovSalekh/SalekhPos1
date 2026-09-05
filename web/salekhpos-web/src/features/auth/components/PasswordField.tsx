/**
 * PasswordField — Input with a show/hide toggle. Used by every
 * page that takes a password. The strength hint is rendered from
 * the list of reasons the server's PasswordPolicyException returns
 * (or from the i18n validation.* keys when the form is validating
 * client-side).
 */
import { forwardRef, useId, useState } from 'react';
import type { InputHTMLAttributes, ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { Input } from '@/components/ui/Input';

export interface PasswordFieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type' | 'size'> {
  label: string;
  hint?: ReactNode;
  error?: ReactNode;
  block?: boolean;
  showStrengthHints?: boolean;
}

export const PasswordField = forwardRef<HTMLInputElement, PasswordFieldProps>(function PasswordField(
  { label, hint, error, block, showStrengthHints = false, id, ...rest },
  ref,
) {
  const generatedId = useId();
  const inputId = id ?? generatedId;
  const { t } = useTranslation();
  const [visible, setVisible] = useState(false);
  return (
    <div className="password-field">
      <Input
        ref={ref}
        id={inputId}
        label={label}
        type={visible ? 'text' : 'password'}
        autoComplete={rest.autoComplete ?? 'current-password'}
        {...(block === undefined ? {} : { block })}
        {...(error === undefined ? {} : { error })}
        {...rest}
      />
      <button
        type="button"
        className="password-field__toggle"
        onClick={() => setVisible((current) => !current)}
        aria-label={visible ? t('auth.actions.hidePassword') : t('auth.actions.showPassword')}
        aria-pressed={visible}
      >
        {visible ? t('auth.actions.hide') : t('auth.actions.show')}
      </button>
      {showStrengthHints && !error && (
        <p className="ui-field__hint">
          {hint ?? t('validation.password.hints')}
        </p>
      )}
    </div>
  );
});
