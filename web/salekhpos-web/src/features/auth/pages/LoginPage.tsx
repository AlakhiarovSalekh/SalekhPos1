/**
 * LoginPage — `/login`. Wrapped in <RequireAnonymous> so signed-in
 * users bounce to /. Supports `?returnTo=` for post-login redirect.
 */
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { Alert, Button, Input } from '@/components/ui';
import { AuthErrorBanner } from '@/features/auth/components/AuthErrorBanner';
import { AuthLayout } from '@/features/auth/components/AuthLayout';
import { PasswordField } from '@/features/auth/components/PasswordField';
import { useAuth } from '@/features/auth/state/useAuth';
import { useSubmitAuth } from '@/features/auth/components/useSubmitAuth';

const loginSchema = z.object({
  email: z.string().min(1, 'validation.required').email('validation.email'),
  password: z.string().min(1, 'validation.required'),
});

type LoginForm = z.infer<typeof loginSchema>;

interface LocationState {
  from?: { pathname?: string };
}

export function LoginPage() {
  const { t } = useTranslation();
  const auth = useAuth();
  const { state, run } = useSubmitAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [params] = useSearchParams();
  const returnTo =
    params.get('returnTo') ?? (location.state as LocationState | null)?.from?.pathname ?? '/';
  const resetFlag = params.get('reset') === '1';

  const form = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  });

  const onSubmit = form.handleSubmit(async (values) => {
    await run(async () => {
      await auth.signIn(values);
      if (auth.status === 'mfa-pending' && auth.mfaPending) {
        navigate(
          `/mfa/verify?userId=${encodeURIComponent(auth.mfaPending.userId)}&tenantId=${encodeURIComponent(
            auth.mfaPending.tenantId,
          )}`,
          { replace: true },
        );
        return;
      }
      navigate(returnTo, { replace: true });
    });
  });

  return (
    <AuthLayout
      title={t('auth.login.title')}
      subtitle={t('auth.login.subtitle')}
      footer={
        <>
          <span>{t('auth.login.noAccount')}</span>{' '}
          <Link to="/register">{t('auth.login.registerLink')}</Link>
        </>
      }
    >
      {resetFlag && <Alert variant="success">{t('auth.resetPassword.successBody')}</Alert>}
      <AuthErrorBanner error={state.error} />
      <form className="auth-shell__form" onSubmit={onSubmit} noValidate>
        <Input
          label={t('auth.login.email.label')}
          type="email"
          autoComplete="email"
          block
          {...form.register('email')}
          error={form.formState.errors.email ? t(form.formState.errors.email.message ?? 'validation.required') : undefined}
        />
        <PasswordField
          label={t('auth.login.password.label')}
          autoComplete="current-password"
          block
          {...form.register('password')}
          error={form.formState.errors.password ? t(form.formState.errors.password.message ?? 'validation.required') : undefined}
        />
        <Button type="submit" block loading={state.loading}>
          {t('auth.login.submit')}
        </Button>
      </form>
      <p className="auth-shell__row">
        <Link to="/forgot-password">{t('auth.login.forgotLink')}</Link>
      </p>
    </AuthLayout>
  );
}
