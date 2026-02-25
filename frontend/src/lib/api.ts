import { TRANSACTIONS_ENDPOINT } from "./config";
import type { NewTransaction } from "../types";
import { normalizeIncomingTransaction } from "./transactions";
import type { Transaction } from "../types";

export async function postTransaction(transaction: NewTransaction): Promise<void> {
  const response = await fetch(TRANSACTIONS_ENDPOINT, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(transaction)
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed with status ${response.status}`);
  }
}

export async function fetchTransactions(): Promise<Transaction[]> {
  const response = await fetch(TRANSACTIONS_ENDPOINT, {
    method: "GET"
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed with status ${response.status}`);
  }

  const payload = (await response.json()) as unknown[];
  return payload.map((item) => normalizeIncomingTransaction(item));
}
