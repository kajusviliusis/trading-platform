import React, { useEffect, useState } from "react";
import { getWalletBalance } from "../api";
import { Wallet } from "../types";

const WalletComponent: React.FC = () => {
    const [wallet, setWallet] = useState<Wallet | null>(null);

    useEffect(() => {
        const fetchWallet = async () => {
            try {
                const data = await getWalletBalance();
                setWallet(data);
            } catch (err) {
                console.error("Error fetching wallet balance:", err);
            }
        };

        fetchWallet();
        const interval = setInterval(fetchWallet, 30000);
        return () => clearInterval(interval);
    }, []);

    return (
        <div className="wallet">
            <h2>Wallet</h2>
            {wallet ? (
                <ul>
                    <li>Cash Balance: ${wallet.balance.toFixed(2)} {wallet.currency}</li>
                    <li>Portfolio Value: ${wallet.portfolioValue.toFixed(2)}</li>
                    <li><strong>Total Balance: ${wallet.totalBalance.toFixed(2)}</strong></li>
                    <li>Last Updated: {new Date(wallet.updatedAt).toLocaleTimeString()}</li>
                </ul>
            ) : (
                <p>Loading...</p>
            )}
        </div>
    );
};

export default WalletComponent;
