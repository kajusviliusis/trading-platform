import React, { useEffect, useState } from "react";
import { getTransactions } from "../api";
import { Transaction } from "../types";

const Transactions: React.FC = () => {
    const [transactions, setTransactions] = useState<Transaction[]>([]);

    useEffect(() => {
        getTransactions().then(setTransactions).catch(console.error);
    }, []);

    return (
        <div>
            <h2>Transactions</h2>
            <ul>
                {transactions.map(t => (
                    <li key={t.id}>
                        {t.type} {t.quantity} shares of stock {t.stockId} at ${t.priceAtExecution} on {new Date(t.timestamp).toLocaleString()}
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default Transactions;
