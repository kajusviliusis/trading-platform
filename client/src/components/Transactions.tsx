import React, { useEffect, useState } from "react";
import { getTransactions } from "../api";
import { Transaction } from "../types";

const formatShares = (qty: number): string => Number.isFinite(qty) ? qty.toFixed(4) : "-";
const formatMoney = (v: number): string => Number.isFinite(v) ? v.toFixed(2) : "-";

const Transactions: React.FC = () => {
  const [transactions, setTransactions] = useState<Transaction[]>([]);

  useEffect(() => {
    getTransactions().then(setTransactions).catch(console.error);
  }, []);

  return (
    <div>
      <h2>Transactions</h2>
      <ul>
        {transactions.map((t) => {
          const qty = Number(t.quantity);
          const price = Number(t.priceAtExecution);
          const value = qty * price;
          const timestamp = new Date(t.timestamp).toLocaleString();
          return (
            <li key={t.id}>
              {t.type} {formatShares(qty)} shares of stock {t.stockId} @ ${formatMoney(price)} on {timestamp} &middot; Value: ${formatMoney(value)}
            </li>
          );
        })}
      </ul>
    </div>
  );
};

export default Transactions;
