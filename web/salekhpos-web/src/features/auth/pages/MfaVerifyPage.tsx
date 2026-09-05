/**
 * MfaVerifyPage — `/mfa/verify?userId=…&tenantId=…`. <RequireMfaPending>
 * — guards against deep-linking to this page without an in-flight
 * MFA challenge. Submits a 6-digit TOTP code; on success the user
 * is redirected to returnTo or /.
 */
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { Button, Input } from '@/components/ui';
import { AuthErrorBanner } from '@/features/auth/components/AuthErrorBanner';
import { AuthLayout } from '@/features/auth/components/AuthLayout';
import { useAuth } from '@/features/auth/state/useAuth';
import { useSubmitAuth } from '@/features/auth/components/useSubmitAuth';

const mfaSchema = z.object({
  code: z.string().regex(/^\d{6}$/, 'validation.mfa.format'),
});

type MfaForm = z.infer<typeof mfaSchema>;

export function MfaVerifyPage() {
  const { t } = useTranslation();
  const auth = useAuth();
  const { state, run } = useSubmitAuth();
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const userId = params.get('userId') ?? auth.mfaPending?.userId ?? '';
  const returnTo = params.get('returnTo') ?? '/';

  const form = useForm<MfaForm>({
    resolver: zodResolver(mfaSchema),
    defaultValues: { code: '' },
  });

  const onSubmit = form.handleSubmit(async (values) => {
    if (!userId) {
      return;
    }
    await run(async () => {
      await auth.signInWithMfa({ userId, code: values.code });
      navigate(returnTo, { replace: true });
    });
  });

  return (
    <AuthLayout title={t('auth.mfa.verify.title')} subtitle={t('auth.mfa.verify.body')}>
      <AuthErrorBanner error={state.error} />
      <form className="auth-shell__form" onSubmit={onSubmit} noValidate>
        <Input
          label={t('auth.mfa.verify.code.label')}
          type="text"
          inputMode="numeric"
          autoComplete="one-time-code"
          maxLength={6}
          block
          {...form.register('code')}
          error={form.formState.errors.code ? t(form.formState.errors.code.message ?? 'validation.mfa.format') : undefined}
        />
        <Button type="submit" block loading={state.loading} disabled={!userId}>
          {t('auth.mfa.verify.submit')}
        </Button>
      </form>
    </AuthLayout>
  );
}
