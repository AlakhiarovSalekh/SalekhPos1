/**
 * ForgotPasswordPage — `/forgot-password`. Always shows the same
 * success message regardless of whether the email exists, to
 * avoid email enumeration. The success state replaces the form.
 */
import { Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { Button, Input } from '@/components/ui';
import { AuthErrorBanner } from '@/features/auth/components/AuthErrorBanner';
import { AuthLayout } from '@/features/auth/components/AuthLayout';
import { useAuth } from '@/features/auth/state/useAuth';
import { useSubmitAuth } from '@/features/auth/components/useSubmitAuth';

const forgotSchema = z.object({
  email: z.string().min(1, 'validation.required').email('validation.email'),
});

type ForgotForm = z.infer<typeof forgotSchema>;

export function ForgotPasswordPage() {
  const { t } = useTranslation();
  const auth = useAuth();
  const { state, run } = useSubmitAuth();
  const form = useForm<ForgotForm>({
    resolver: zodResolver(forgotSchema),
    defaultValues: { email: '' },
  });

  const onSubmit = form.handleSubmit(async (values) => {
    await run(() => auth.forgotPassword(values));
  });

  const submitted = form.formState.isSubmitSuccessful && state.loading === false && !state.error;

  return (
    <AuthLayout
      title={t('auth.forgotPassword.title')}
      subtitle={t('auth.forgotPassword.body')}
      footer={
        <Link to="/login">{t('auth.forgotPassword.backToLogin')}</Link>
      }
    >
      <AuthErrorBanner error={state.error} />
      {submitted ? (
        <p className="auth-shell__subtitle">{t('auth.forgotPassword.successBody')}</p>
      ) : (
        <form className="auth-shell__form" onSubmit={onSubmit} noValidate>
          <Input
            label={t('auth.forgotPassword.email.label')}
            type="email"
            autoComplete="email"
            block
            {...form.register('email')}
            error={form.formState.errors.email ? t(form.formState.errors.email.message ?? 'validation.required') : undefined}
          />
          <Button type="submit" block loading={state.loading}>
            {t('auth.forgotPassword.submit')}
          </Button>
        </form>
      )}
    </AuthLayout>
  );
}
