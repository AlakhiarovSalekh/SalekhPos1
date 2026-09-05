/**
 * Selector hook for the auth context. Throws if used outside
 * <AuthProvider> so a missing-provider bug fails loudly at the
 * call site rather than silently producing a null-deref later.
 */
import { useContext } from 'react';
import { AuthContext, type AuthContextValue } from './AuthContext';

export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext);
  if (value === null) {
    throw new Error('useAuth must be used inside <AuthProvider>.');
  }
  return value;
}
