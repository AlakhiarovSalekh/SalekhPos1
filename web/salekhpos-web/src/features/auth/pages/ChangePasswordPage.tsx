/**
 * ChangePasswordPage — `/account/change-password`. <RequireAuth>.
 * Rotates the user's password (which bumps the server-side
 * token_version, invalidating all other sessions). On success the
 * form clears and a banner is shown; the user is NOT signed out
 * (the rotation is a no-op for the current session because
 * refresh happens naturally on the next 401).
 */
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui';
import { AuthErrorBanner } from '@/features/auth/components/AuthErrorBanner';
import { AuthLayout } from '@/features/auth/components/AuthLayout';
import { PasswordField } from '@/features/auth/components/PasswordField';
import { useAuth } from '@/features/auth/state/useAuth';
import { useSubmitAuth } from '@/features/auth/components/useSubmitAuth';

const changeSchema = z
  .object({
    currentPassword: z.string().min(1, 'validation.required'),
    newPassword: z.string().min(12, 'validation.password.minLength'),
    confirmNewPassword: z.string(),
  })
  .refine((data) => data.newPassword === data.confirmNewPassword, {
    path: ['confirmNewPassword'],
    message: 'validation.passwordMismatch',
  });

type ChangeForm = z.infer<typeof changeSchema>;

export function ChangePasswordPage() {
  const { t } = useTranslation();
  const auth = useAuth();
  const { state, run, reset } = useSubmitAuth();
  const [success, setSuccess] = useState(false);

  const form = useForm<ChangeForm>({
    resolver: zodResolver(changeSchema),
    defaultValues: { currentPassword: '', newPassword: '', confirmNewPassword: '' },
  });

  const onSubmit = form.handleSubmit(async (values) => {
    await run(async () => {
      await auth.changePassword({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
      });
      form.reset();
      setSuccess(true);
      reset();
    });
  });

  return (
    <AuthLayout title={t('auth.changePassword.title')}>
      <AuthErrorBanner error={state.error} />
      {success && <p className="auth-shell__subtitle">{t('auth.changePassword.successBody')}</p>}
      <form className="auth-shell__form" onSubmit={onSubmit} noValidate>
        <PasswordField
          label={t('auth.changePassword.fields.currentPassword.label')}
          autoComplete="current-password"
          block
          {...form.register('currentPassword')}
          error={form.formState.errors.currentPassword ? t(form.formState.errors.currentPassword.message ?? 'validation.required') : undefined}
        />
        <PasswordField
          label={t('auth.changePassword.fields.newPassword.label')}
          autoComplete="new-password"
          block
          showStrengthHints
          {...form.register('newPassword')}
          error={form.formState.errors.newPassword ? t(form.formState.errors.newPassword.message ?? 'validation.password.minLength') : undefined}
        />
        <PasswordField
          label={t('auth.changePassword.fields.confirmNewPassword.label')}
          autoComplete="new-password"
          block
          {...form.register('confirmNewPassword')}
          error={form.formState.errors.confirmNewPassword ? t(form.formState.errors.confirmNewPassword.message ?? 'validation.passwordMismatch') : undefined}
        />
        <Button type="submit" block loading={state.loading}>
          {t('auth.changePassword.submit')}
        </Button>
      </form>
    </AuthLayout>
  );
}
