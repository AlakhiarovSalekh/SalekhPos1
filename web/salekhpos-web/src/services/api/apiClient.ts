/**
 * API client.
 *
 * Thin wrapper around axios. The base URL comes from configuration. The
 * client attaches the bearer access token (when present) and a request
 * id. It does NOT perform tenant/store/permission decisions — those
 * are authoritative on the server.
 */
import axios, { type AxiosInstance, type InternalAxiosRequestConfig } from 'axios';
import { config } from '@/app/config/config';

let bearerToken: string | null = null;

export function setBearerToken(token: string | null): void {
  bearerToken = token;
}

export function getBearerToken(): string | null {
  return bearerToken;
}

export const apiClient: AxiosInstance = axios.create({
  baseURL: config.apiBaseUrl,
  timeout: 15_000,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
  withCredentials: false,
});

apiClient.interceptors.request.use((request: InternalAxiosRequestConfig) => {
  if (bearerToken) {
    request.headers.set('Authorization', `Bearer ${bearerToken}`);
  }
  return request;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // Surface the response shape; do not log tokens or secrets.
    return Promise.reject(error);
  },
);
