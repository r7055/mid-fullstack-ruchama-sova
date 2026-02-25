import type { NewTransaction, Transaction, TransactionStatus } from "../types";

const CURRENCIES = ["USD", "EUR", "GBP", "ILS"] as const;

export function normalizeStatus(status: unknown): TransactionStatus {
  if (typeof status === "string") {
    const normalized = status.trim().toLowerCase();
    if (normalized === "pending") {
      return "Pending";
    }
    if (normalized === "completed" || normalized === "success") {
      return "Completed";
    }
    if (normalized === "failed" || normalized === "error") {
      return "Failed";
    }
  }

  if (typeof status === "number") {
    if (status === 0) {
      return "Pending";
    }
    if (status === 1) {
      return "Completed";
    }
    if (status === 2) {
      return "Failed";
    }
  }

  return "Pending";
}

export function normalizeIncomingTransaction(payload: unknown): Transaction {
  const transaction = payload as Partial<Transaction> & { status?: unknown };

  return {
    transactionId: String(transaction.transactionId ?? crypto.randomUUID()),
    amount: Number(transaction.amount ?? 0),
    currency: String(transaction.currency ?? "USD").toUpperCase(),
    status: normalizeStatus(transaction.status),
    timestamp: transaction.timestamp
      ? new Date(transaction.timestamp).toISOString()
      : new Date().toISOString()
  };
}

export function createRandomTransaction(): NewTransaction {
  const statusRoll = Math.random();
  let status: TransactionStatus = "Completed";

  if (statusRoll < 0.2) {
    status = "Failed";
  } else if (statusRoll < 0.35) {
    status = "Pending";
  }

  const currency = CURRENCIES[Math.floor(Math.random() * CURRENCIES.length)];
  const amount = Number((Math.random() * 4999 + 1).toFixed(2));

  return {
    transactionId: crypto.randomUUID(),
    amount,
    currency,
    status
  };
}

export function formatTimestamp(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleTimeString();
}
