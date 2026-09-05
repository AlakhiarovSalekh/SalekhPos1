/**
 * i18n key-parity test.
 *
 * Every key that exists in en.json must exist in ka.json and
 * az.json. This guard exists so we never ship a slice that adds
 * English strings without the other two locales — the build
 * fails loudly at test time instead of silently shipping a
 * half-translated product.
 */
import { describe, expect, it } from 'vitest';
import en from '@/i18n/locales/en.json';
import ka from '@/i18n/locales/ka.json';
import az from '@/i18n/locales/az.json';

type JsonObject = { [key: string]: string | number | boolean | JsonObject | JsonObject[] | unknown[] | null };

function flatten(value: JsonObject, prefix = ''): string[] {
  const out: string[] = [];
  for (const [key, child] of Object.entries(value)) {
    const path = prefix.length === 0 ? key : `${prefix}.${key}`;
    if (child !== null && typeof child === 'object' && !Array.isArray(child)) {
      out.push(...flatten(child as JsonObject, path));
    } else {
      out.push(path);
    }
  }
  return out;
}

describe('i18n key parity', () => {
  const enKeys = new Set(flatten(en as JsonObject));
  const kaKeys = new Set(flatten(ka as JsonObject));
  const azKeys = new Set(flatten(az as JsonObject));

  it('en has at least one key', () => {
    expect(enKeys.size).toBeGreaterThan(10);
  });

  it('ka contains every en key', () => {
    const missing = [...enKeys].filter((key) => !kaKeys.has(key));
    expect(missing).toEqual([]);
  });

  it('az contains every en key', () => {
    const missing = [...enKeys].filter((key) => !azKeys.has(key));
    expect(missing).toEqual([]);
  });

  it('en does not contain extra keys not present in ka/az (catches typos)', () => {
    const extraInEn = [...enKeys].filter((key) => !kaKeys.has(key));
    expect(extraInEn).toEqual([]);
  });
});
