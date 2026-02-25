const configuredApiBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim();

export const API_BASE_URL = (configuredApiBaseUrl && configuredApiBaseUrl.length > 0
  ? configuredApiBaseUrl
  : "http://localhost:5038"
).replace(/\/+$/, "");

export const TRANSACTIONS_ENDPOINT = `${API_BASE_URL}/transactions`;
export const TRANSACTION_HUB_ENDPOINT = `${API_BASE_URL}/hub/transactions`;
