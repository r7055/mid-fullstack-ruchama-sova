import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { createTransactionHubConnection, RECEIVE_TRANSACTION_EVENT } from "../lib/signalr";
import { formatTimestamp, normalizeIncomingTransaction } from "../lib/transactions";
import { fetchTransactions } from "../lib/api";
import { TRANSACTION_STATUSES } from "../types";
import type { Transaction, TransactionFilter } from "../types";

type LiveConnectionState = "connecting" | "connected" | "reconnecting" | "disconnected" | "error";

const MAX_TRANSACTIONS = 1000;

export function MonitorPage() {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [filter, setFilter] = useState<TransactionFilter>("All");
  const [connectionState, setConnectionState] = useState<LiveConnectionState>("connecting");
  const [error, setError] = useState<string>("");

  const pendingTransactions = useRef<Transaction[]>([]);
  const rafId = useRef<number | null>(null);

  const flushBufferedTransactions = useCallback(() => {
    rafId.current = null;

    if (pendingTransactions.current.length === 0) {
      return;
    }

    const nextBatch = pendingTransactions.current.slice();
    pendingTransactions.current = [];

    setTransactions((previous) => {
      const merged = [...nextBatch.reverse(), ...previous];
      return merged.slice(0, MAX_TRANSACTIONS);
    });
  }, []);

  useEffect(() => {
    let isActive = true;

    void fetchTransactions()
      .then((items) => {
        if (!isActive) {
          return;
        }

        setTransactions(items.slice(-MAX_TRANSACTIONS).reverse());
      })
      .catch(() => {
        if (!isActive) {
          return;
        }
      });

    return () => {
      isActive = false;
    };
  }, []);

  function scheduleFlush() {
    if (rafId.current !== null) {
      return;
    }

    rafId.current = window.requestAnimationFrame(flushBufferedTransactions);
  }

  useEffect(() => {
    const connection = createTransactionHubConnection();
    let isActive = true;
    let startRetryTimerId: number | null = null;
    setConnectionState("connecting");
    setError("");

    connection.on(RECEIVE_TRANSACTION_EVENT, (payload: unknown) => {
      if (!isActive) {
        return;
      }
      pendingTransactions.current.push(normalizeIncomingTransaction(payload));
      scheduleFlush();
    });

    connection.onreconnecting(() => {
      if (!isActive) {
        return;
      }
      setConnectionState("reconnecting");
      setError("");
    });

    connection.onreconnected(() => {
      if (!isActive) {
        return;
      }
      setConnectionState("connected");
      setError("");
    });

    connection.onclose(() => {
      if (!isActive) {
        return;
      }
      setConnectionState("disconnected");
    });

    const startConnection = async (attempt: number): Promise<void> => {
      try {
        await connection.start();
        if (!isActive) {
          return;
        }
        setConnectionState("connected");
        setError("");
      } catch (startError) {
        if (!isActive) {
          return;
        }

        const message =
          startError instanceof Error ? startError.message : "SignalR connection failed.";
        const canRetry = attempt < 5;

        if (canRetry) {
          setConnectionState("connecting");
          setError("");

          startRetryTimerId = window.setTimeout(() => {
            void startConnection(attempt + 1);
          }, attempt * 300);
          return;
        }

        setConnectionState("error");
        setError(message);
      }
    };

    void startConnection(1);

    return () => {
      isActive = false;
      if (startRetryTimerId !== null) {
        window.clearTimeout(startRetryTimerId);
      }

      if (rafId.current !== null) {
        window.cancelAnimationFrame(rafId.current);
      }

      pendingTransactions.current = [];
      connection.off(RECEIVE_TRANSACTION_EVENT);
      void connection.stop();
    };
  }, [flushBufferedTransactions]);

  const filteredTransactions = useMemo(() => {
    if (filter === "All") {
      return transactions;
    }

    return transactions.filter((transaction) => transaction.status === filter);
  }, [filter, transactions]);

  return (
    <section className="card-panel">
      <h2>Live Monitor (/monitor)</h2>
      <p className="panel-subtitle">
        Connected clients receive each transaction over SignalR in real time.
      </p>

      <div className="monitor-toolbar">
        <div className={`connection-chip ${connectionState}`}>
          {connectionState === "error" ? "error" : connectionState}
        </div>

        <label className="inline-label">
          Show
          <select value={filter} onChange={(event) => setFilter(event.target.value as TransactionFilter)}>
            <option value="All">All</option>
            {TRANSACTION_STATUSES.map((status) => (
              <option key={status} value={status}>
                {status}
              </option>
            ))}
          </select>
        </label>

        <span className="count-pill">
          Showing {filteredTransactions.length} / {transactions.length}
        </span>

        <button type="button" onClick={() => setTransactions([])}>
          Clear
        </button>
      </div>

      {error && <p className="feedback error">{error}</p>}

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Time</th>
              <th>Transaction ID</th>
              <th>Status</th>
              <th>Currency</th>
              <th>Amount</th>
            </tr>
          </thead>
          <tbody>
            {filteredTransactions.map((transaction) => (
              <tr key={`${transaction.transactionId}-${transaction.timestamp}`}>
                <td>{formatTimestamp(transaction.timestamp)}</td>
                <td className="mono-cell">{transaction.transactionId}</td>
                <td>
                  <span className={`status-badge ${transaction.status.toLowerCase()}`}>
                    {transaction.status}
                  </span>
                </td>
                <td>{transaction.currency}</td>
                <td>{transaction.amount.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}
