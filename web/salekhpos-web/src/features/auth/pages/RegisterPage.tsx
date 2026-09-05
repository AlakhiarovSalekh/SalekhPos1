/**
 * RegisterPage — `/register`. Tenant name drives the slug by
 * default; the user can override. On 202 the user lands on the
 * "check your email" state — they have NOT been signed in.
 */
import { Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { Button, Input } from '@/components/ui';
import { AuthErrorBanner } from '@/features/auth/components/AuthErrorBanner';
import { AuthLayout } from '@/features/auth/components/AuthLayout';
import { PasswordField } from '@/features/auth/components/PasswordField';
import { useAuth } from '@/features/auth/state/useAuth';
import { useSubmitAuth } from '@/features/auth/components/useSubmitAuth';

const slugRegex = /^[a-z0-9](?:[a-z0-9-]{0,38}[a-z0-9])?$/;

const registerSchema = z
  .object({
    email: z.string().min(1, 'validation.required').email('validation.email'),
    fullName: z.string().min(1, 'validation.required').max(120),
    tenantName: z.string().min(1, 'validation.required').max(120),
    tenantSlug: z.string().regex(slugRegex, 'validation.tenantSlug.format'),
    password: z.string().min(12, 'validation.password.minLength'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    path: ['confirmPassword'],
    message: 'validation.passwordMismatch',
  });

type RegisterForm = z.infer<typeof registerSchema>;

function slugify(value: string): string {
  return value
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
    .slice(0, 40);
}

export function RegisterPage() {
  const { t } = useTranslation();
  const auth = useAuth();
  const { state, run } = useSubmitAuth();
  const form = useForm<RegisterForm>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      email: '',
      fullName: '',
      tenantName: '',
      tenantSlug: '',
      password: '',
      confirmPassword: '',
    },
  });
  const tenantName = form.watch('tenantName');
  const tenantSlug = form.watch('tenantSlug');
  const submitted = !!state.error === false && form.formState.isSubmitSuccessful && state.loading === false;

  const onSubmit = form.handleSubmit(async (values) => {
    await run(() => auth.register(values));
  });

  return (
    <AuthLayout
      title={t('auth.register.title')}
      subtitle={t('auth.register.subtitle')}
      footer={
        <>
          <span>{t('auth.register.haveAccount')}</span>{' '}
          <Link to="/login">{t('auth.register.loginLink')}</Link>
        </>
      }
    >
      <AuthErrorBanner error={state.error} />
      {submitted && (
        <div>
          <h3 className="auth-shell__title">{t('auth.register.successTitle')}</h3>
          <p className="auth-shell__subtitle">{t('auth.register.successBody')}</p>
        </div>
      )}
      {!submitted && (
        <form className="auth-shell__form" onSubmit={onSubmit} noValidate>
          <Input
            label={t('auth.register.fields.email.label')}
            type="email"
            autoComplete="email"
            block
            {...form.register('email')}
            error={form.formState.errors.email ? t(form.formState.errors.email.message ?? 'validation.required') : undefined}
          />
          <Input
            label={t('auth.register.fields.fullName.label')}
            type="text"
            autoComplete="name"
            block
            {...form.register('fullName')}
            error={form.formState.errors.fullName ? t(form.formState.errors.fullName.message ?? 'validation.required') : undefined}
          />
          <Input
            label={t('auth.register.fields.tenantName.label')}
            type="text"
            autoComplete="organization"
            block
            {...form.register('tenantName', {
              onChange: (event) => {
                if (!tenantSlug || tenantSlug === slugify(tenantName)) {
                  form.setValue('tenantSlug', slugify(event.target.value), { shouldValidate: true });
                }
              },
            })}
            error={form.formState.errors.tenantName ? t(form.formState.errors.tenantName.message ?? 'validation.required') : undefined}
          />
          <Input
            label={t('auth.register.fields.tenantSlug.label')}
            type="text"
            autoComplete="off"
            block
            hint={t('auth.register.fields.tenantSlug.hint')}
            {...form.register('tenantSlug')}
            error={form.formState.errors.tenantSlug ? t(form.formState.errors.tenantSlug.message ?? 'validation.tenantSlug.format') : undefined}
          />
          <PasswordField
            label={t('auth.register.fields.password.label')}
            autoComplete="new-password"
            block
            showStrengthHints
            {...form.register('password')}
            error={form.formState.errors.password ? t(form.formState.errors.password.message ?? 'validation.password.minLength') : undefined}
          />
          <PasswordField
            label={t('auth.register.fields.confirmPassword.label')}
            autoComplete="new-password"
            block
            {...form.register('confirmPassword')}
            error={form.formState.errors.confirmPassword ? t(form.formState.errors.confirmPassword.message ?? 'validation.passwordMismatch') : undefined}
          />
          <Button type="submit" block loading={state.loading}>
            {t('auth.register.submit')}
          </Button>
        </form>
      )}
    </AuthLayout>
  );
}
