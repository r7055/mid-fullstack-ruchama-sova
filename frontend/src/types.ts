export const TRANSACTION_STATUSES = ["Pending", "Completed", "Failed"] as const;

export type TransactionStatus = (typeof TRANSACTION_STATUSES)[number];
export type TransactionFilter = "All" | TransactionStatus;

export interface Transaction {
  transactionId: string;
  amount: number;
  currency: string;
  status: TransactionStatus;
  timestamp: string;
}

export interface NewTransaction {
  transactionId: string;
  amount: number;
  currency: string;
  status: TransactionStatus;
}
