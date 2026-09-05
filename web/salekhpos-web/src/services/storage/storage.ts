/**
 * Typed localStorage wrapper for the auth feature.
 *
 * Why one key, not many: the access token, refresh token, expiry
 * timestamps, and the decoded identity claims all live and die
 * together. A single `salekhpos.auth` JSON blob makes sign-out a
 * single remove() and removes the chance of stale partial state.
 *
 * Why localStorage and not an httpOnly cookie: the backend issues
 * the refresh token as an opaque string inside the JSON body. The
 * only way to read it on subsequent requests is to put it somewhere
 * the JavaScript can see. This is the same trade-off the backend
 * ADR (ADR-009) documents, and it assumes HTTPS in production and
 * a strict CSP that blocks third-party scripts.
 */
import { z } from 'zod';

const AUTH_NAMESPACE = 'salekhpos.auth';

export const authStorageSchema = z.object({
  accessToken: z.string().min(1),
  refreshToken: z.string().min(1),
  accessTokenExpiresAtUtc: z.string().datetime({ offset: false }),
  refreshTokenExpiresAtUtc: z.string().datetime({ offset: false }),
  userId: z.string().uuid(),
  tenantId: z.string().uuid(),
  role: z.string().min(1),
  tokenVersion: z.number().int().nonnegative(),
});

export type StoredAuth = z.infer<typeof authStorageSchema>;

function getStorage(): Storage | null {
  if (typeof window === 'undefined') {
    return null;
  }
  try {
    return window.localStorage;
  } catch {
    // Safari private mode, disabled storage, etc.
    return null;
  }
}

export function readAuth(): StoredAuth | null {
  const storage = getStorage();
  if (!storage) {
    return null;
  }
  const raw = storage.getItem(AUTH_NAMESPACE);
  if (!raw) {
    return null;
  }
  try {
    const parsed = JSON.parse(raw) as unknown;
    const result = authStorageSchema.safeParse(parsed);
    if (!result.success) {
      // Corrupted/old-shape entry; discard so the user lands on
      // /login instead of looping on a hydration error.
      storage.removeItem(AUTH_NAMESPACE);
      return null;
    }
    return result.data;
  } catch {
    storage.removeItem(AUTH_NAMESPACE);
    return null;
  }
}

export function writeAuth(value: StoredAuth): void {
  const storage = getStorage();
  if (!storage) {
    return;
  }
  try {
    storage.setItem(AUTH_NAMESPACE, JSON.stringify(value));
  } catch {
    // Quota exceeded or storage disabled; carry on in-memory only.
  }
}

export function clearAuth(): void {
  const storage = getStorage();
  if (!storage) {
    return;
  }
  storage.removeItem(AUTH_NAMESPACE);
}
