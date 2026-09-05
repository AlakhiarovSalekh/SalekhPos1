/**
 * useSubmitAuth — small hook shared by every auth page. The
 * pages only need to know: "call this on submit; it will hand
 * the AuthError to the error banner". The hook owns the loading
 * flag and the error state.
 */
import { useCallback, useState } from 'react';
import { isAuthError, type AuthError } from '@/features/auth/state/AuthContext';

export interface SubmitState {
  loading: boolean;
  error: AuthError | null;
}

export interface SubmitAuth {
  state: SubmitState;
  run<T>(action: () => Promise<T>): Promise<T | null>;
  reset(): void;
}

export function useSubmitAuth(): SubmitAuth {
  const [state, setState] = useState<SubmitState>({ loading: false, error: null });

  const run = useCallback(async <T>(action: () => Promise<T>): Promise<T | null> => {
    setState({ loading: true, error: null });
    try {
      const result = await action();
      setState({ loading: false, error: null });
      return result;
    } catch (raw) {
      let error: AuthError;
      if (isAuthError(raw)) {
        error = raw;
      } else {
        const fallback = new Error(raw instanceof Error ? raw.message : String(raw)) as AuthError;
        fallback.name = 'AuthError';
        fallback.status = 0;
        fallback.code = 'unknown';
        error = fallback;
      }
      setState({ loading: false, error });
      return null;
    }
  }, []);

  const reset = useCallback(() => {
    setState({ loading: false, error: null });
  }, []);

  return { state, run, reset };
}
