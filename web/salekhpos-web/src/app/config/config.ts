/**
 * Centralised, typed configuration for the web client.
 *
 * Secrets must NEVER appear here. Only the public API base URL and
 * non-sensitive feature flags belong in this file. Anything sensitive
 * is loaded at build time from a server-injected configuration.
 */
import { z } from 'zod';

const envSchema = z.object({
  VITE_API_BASE_URL: z.string().url().default('http://localhost:8080'),
  VITE_HUB_BASE_URL: z.string().url().default('http://localhost:8080'),
  VITE_DEFAULT_LOCALE: z.enum(['ka', 'en', 'az']).default('en'),
  VITE_ENABLE_DEVTOOLS: z
    .string()
    .optional()
    .transform((value) => value === 'true'),
});

const parsed = envSchema.safeParse({
  VITE_API_BASE_URL: import.meta.env.VITE_API_BASE_URL,
  VITE_HUB_BASE_URL: import.meta.env.VITE_HUB_BASE_URL,
  VITE_DEFAULT_LOCALE: import.meta.env.VITE_DEFAULT_LOCALE,
  VITE_ENABLE_DEVTOOLS: import.meta.env.VITE_ENABLE_DEVTOOLS,
});

if (!parsed.success) {
  // eslint-disable-next-line no-console
  console.error('Invalid web client environment configuration.', parsed.error.format());
  throw new Error('Invalid web client environment configuration.');
}

export const config = Object.freeze({
  apiBaseUrl: parsed.data.VITE_API_BASE_URL,
  hubBaseUrl: parsed.data.VITE_HUB_BASE_URL,
  defaultLocale: parsed.data.VITE_DEFAULT_LOCALE,
  enableDevtools: parsed.data.VITE_ENABLE_DEVTOOLS ?? false,
} as const);

export type AppConfig = typeof config;
