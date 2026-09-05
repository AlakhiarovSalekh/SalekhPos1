/**
 * ResetPasswordPage — `/reset-password?token=…`. The deep link
 * from the recovery email lands here with a one-time token. On
 * success the user is redirected to /login with a `reset=1`
 * query string so the login page can show a success banner.
 */
import { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
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

const resetSchema = z
  .object({
    newPassword: z.string().min(12, 'validation.password.minLength'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    path: ['confirmPassword'],
    message: 'validation.passwordMismatch',
  });

type ResetForm = z.infer<typeof resetSchema>;

export function ResetPasswordPage() {
  const { t } = useTranslation();
  const auth = useAuth();
  const { state, run } = useSubmitAuth();
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const token = params.get('token') ?? '';
  const [submitted, setSubmitted] = useState(false);

  const form = useForm<ResetForm>({
    resolver: zodResolver(resetSchema),
    defaultValues: { newPassword: '', confirmPassword: '' },
  });

  const onSubmit = form.handleSubmit(async (values) => {
    if (!token) {
      return;
    }
    await run(async () => {
      await auth.resetPassword({ token, newPassword: values.newPassword });
      setSubmitted(true);
      navigate('/login?reset=1', { replace: true });
    });
  });

  return (
    <AuthLayout
      title={t('auth.resetPassword.title')}
      footer={
        <Link to="/login">{t('auth.forgotPassword.backToLogin')}</Link>
      }
    >
      <AuthErrorBanner error={state.error} />
      {submitted ? (
        <p className="auth-shell__subtitle">{t('auth.resetPassword.successBody')}</p>
      ) : (
        <form className="auth-shell__form" onSubmit={onSubmit} noValidate>
          <PasswordField
            label={t('auth.resetPassword.fields.newPassword.label')}
            autoComplete="new-password"
            block
            showStrengthHints
            {...form.register('newPassword')}
            error={form.formState.errors.newPassword ? t(form.formState.errors.newPassword.message ?? 'validation.password.minLength') : undefined}
          />
          <PasswordField
            label={t('auth.resetPassword.fields.confirmPassword.label')}
            autoComplete="new-password"
            block
            {...form.register('confirmPassword')}
            error={form.formState.errors.confirmPassword ? t(form.formState.errors.confirmPassword.message ?? 'validation.passwordMismatch') : undefined}
          />
          <Button type="submit" block loading={state.loading} disabled={!token}>
            {t('auth.resetPassword.submit')}
          </Button>
        </form>
      )}
    </AuthLayout>
  );
}
