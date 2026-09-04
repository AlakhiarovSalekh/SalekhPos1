/**
 * Vitest setup. Imports jest-dom matchers so unit tests can use
 * toBeInTheDocument, toHaveTextContent, etc. Initialises i18n once
 * so component tests can use react-i18next without per-test setup.
 */
import '@testing-library/jest-dom/vitest';
import { configureI18n } from '@/i18n';

configureI18n();
