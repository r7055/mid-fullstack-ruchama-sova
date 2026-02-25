import { useMemo, useState } from "react";
import type { FormEvent } from "react";
import { postTransaction } from "../lib/api";
import { createRandomTransaction } from "../lib/transactions";
import { TRANSACTION_STATUSES } from "../types";
import type { TransactionStatus } from "../types";

interface FormState {
  amount: string;
  currency: string;
  status: TransactionStatus;
}

const defaultState: FormState = {
  amount: "100.00",
  currency: "USD",
  status: "Completed"
};

export function AddTransactionsPage() {
  const [form, setForm] = useState<FormState>(defaultState);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isBursting, setIsBursting] = useState(false);
  const [feedback, setFeedback] = useState<string>("");

  const parsedAmount = useMemo(() => Number(form.amount), [form.amount]);

  async function submitSingle(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!Number.isFinite(parsedAmount) || parsedAmount <= 0) {
      setFeedback("Amount must be greater than 0.");
      return;
    }

    setIsSubmitting(true);
    setFeedback("");

    try {
      await postTransaction({
        transactionId: crypto.randomUUID(),
        amount: Number(parsedAmount.toFixed(2)),
        currency: form.currency.trim().toUpperCase(),
        status: form.status
      });
      setFeedback("Transaction sent successfully.");
    } catch (error) {
      const message = error instanceof Error ? error.message : "Failed to send transaction.";
      setFeedback(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  async function sendRandomSingle() {
    setIsSubmitting(true);
    setFeedback("");

    try {
      await postTransaction(createRandomTransaction());
      setFeedback("Random transaction sent.");
    } catch (error) {
      const message = error instanceof Error ? error.message : "Random transaction failed.";
      setFeedback(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  async function sendBurst(count: number) {
    setIsBursting(true);
    setFeedback("");

    let failed = 0;

    try {
      for (let index = 0; index < count; index += 20) {
        const chunk = Array.from({ length: Math.min(20, count - index) }, () =>
          postTransaction(createRandomTransaction())
        );
        const results = await Promise.allSettled(chunk);
        failed += results.filter((item) => item.status === "rejected").length;
        await new Promise<void>((resolve) => setTimeout(resolve, 0));
      }

      if (failed > 0) {
        setFeedback(`Burst completed with ${failed} failed requests.`);
      } else {
        setFeedback(`Burst completed. ${count} transactions sent.`);
      }
    } finally {
      setIsBursting(false);
    }
  }

  const isBusy = isSubmitting || isBursting;

  return (
    <section className="card-panel">
      <h2>Transaction Simulator (/add)</h2>
      <p className="panel-subtitle">
        This route simulates an external producer and sends transactions using HTTP POST.
      </p>

      <form onSubmit={submitSingle} className="form-grid">
        <label>
          Amount
          <input
            type="number"
            min="0.01"
            step="0.01"
            value={form.amount}
            onChange={(event) => setForm((prev) => ({ ...prev, amount: event.target.value }))}
            required
          />
        </label>

        <label>
          Currency
          <input
            maxLength={3}
            value={form.currency}
            onChange={(event) => setForm((prev) => ({ ...prev, currency: event.target.value }))}
            required
          />
        </label>

        <label>
          Status
          <select
            value={form.status}
            onChange={(event) =>
              setForm((prev) => ({ ...prev, status: event.target.value as TransactionStatus }))
            }
          >
            {TRANSACTION_STATUSES.map((status) => (
              <option key={status} value={status}>
                {status}
              </option>
            ))}
          </select>
        </label>

        <button type="submit" disabled={isBusy}>
          Send Form Transaction
        </button>
      </form>

      <div className="actions-row">
        <button type="button" disabled={isBusy} onClick={sendRandomSingle}>
          Send Random Transaction
        </button>
        <button type="button" disabled={isBusy} onClick={() => void sendBurst(100)}>
          Burst 100 Fast
        </button>
      </div>

      <p className="feedback">{feedback}</p>
    </section>
  );
}
