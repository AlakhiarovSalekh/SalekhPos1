/**
 * Alert — inline status banner. The variant drives both the
 * colour and the default ARIA role: error/warning get
 * role="alert" so screen-readers announce them as soon as they
 * appear; info/success use role="status" which is announced
 * more politely.
 */
import type { ReactNode } from 'react';
import clsx from 'clsx';

export type AlertVariant = 'info' | 'success' | 'warning' | 'error';

export interface AlertProps {
  variant?: AlertVariant;
  title?: string;
  children: ReactNode;
  role?: 'alert' | 'status';
}

const variantClass: Record<AlertVariant, string> = {
  info: 'ui-alert--info',
  success: 'ui-alert--success',
  warning: 'ui-alert--warning',
  error: 'ui-alert--error',
};

export function Alert({ variant = 'info', title, role, children }: AlertProps) {
  const defaultRole: 'alert' | 'status' = variant === 'error' || variant === 'warning' ? 'alert' : 'status';
  return (
    <div className={clsx('ui-alert', variantClass[variant])} role={role ?? defaultRole}>
      {title && <p className="ui-alert__title">{title}</p>}
      <div className="ui-alert__body">{children}</div>
    </div>
  );
}
