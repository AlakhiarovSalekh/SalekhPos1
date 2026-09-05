/**
 * FormStatusBanner — positive/notice banner shown after a
 * successful submission (e.g., "check your email"). Kept
 * separate from AuthErrorBanner so callers can be explicit
 * about which side of the form-state they are rendering.
 */
import { useTranslation } from 'react-i18next';
import { Alert } from '@/components/ui/Alert';

export type FormStatusKind = 'success' | 'info';

export interface FormStatusBannerProps {
  kind?: FormStatusKind;
  title?: string;
  messageKey: string;
  values?: Record<string, string | number>;
}

export function FormStatusBanner({ kind = 'success', title, messageKey, values }: FormStatusBannerProps) {
  const { t } = useTranslation();
  const body = values ? t(messageKey, values) : t(messageKey);
  const titleProp = title !== undefined ? { title } : {};
  return (
    <Alert variant={kind} {...titleProp}>
      {body}
    </Alert>
  );
}
