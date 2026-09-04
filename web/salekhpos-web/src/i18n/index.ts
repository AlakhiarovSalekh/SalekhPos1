/**
 * i18n setup. Languages: ka (Georgian), en (English), az (Azerbaijani).
 * Currency display default: GEL. Backend stores UTC; UI renders tenant/store timezone.
 */
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import en from './locales/en.json';
import ka from './locales/ka.json';
import az from './locales/az.json';
import { config } from '@/app/config/config';

export function configureI18n(): void {
  void i18n.use(initReactI18next).init({
    resources: {
      en: { translation: en },
      ka: { translation: ka },
      az: { translation: az },
    },
    lng: config.defaultLocale,
    fallbackLng: 'en',
    interpolation: {
      // React already escapes; double-escape would mangle output.
      escapeValue: false,
    },
    returnNull: false,
  });
}

export default i18n;
