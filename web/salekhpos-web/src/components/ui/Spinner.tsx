/**
 * Spinner — small indeterminate loading indicator. Always exposes
 * an aria-label so assistive tech announces it; if the caller
 * does not pass a label, a generic "Loading" message is used.
 */
import clsx from 'clsx';

export type SpinnerSize = 'sm' | 'md';

export interface SpinnerProps {
  size?: SpinnerSize;
  label?: string;
  className?: string;
}

const sizeClass: Record<SpinnerSize, string> = {
  sm: 'ui-spinner--sm',
  md: 'ui-spinner--md',
};

export function Spinner({ size = 'md', label, className }: SpinnerProps) {
  return (
    <span
      role="status"
      aria-live="polite"
      aria-label={label ?? 'Loading'}
      className={clsx('ui-spinner', sizeClass[size], className)}
    />
  );
}
