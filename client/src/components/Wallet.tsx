import React, { useEffect, useState } from "react";
import { depositFunds, getWalletBalance } from "../api";
import { Wallet } from "../types";

const WalletComponent: React.FC = () => {
  const [wallet, setWallet] = useState<Wallet | null>(null);
  const [amountText, setAmountText] = useState<string>("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchWallet = async () => {
    try {
      const data = await getWalletBalance();
      setWallet(data);
    } catch (err) {
      console.error("Error fetching wallet balance:", err);
    }
  };

  useEffect(() => {
    fetchWallet();
    const interval = setInterval(fetchWallet, 30000);
    return () => clearInterval(interval);
  }, []);

  const parseAmount = (): number | null => {
    if (amountText.trim() === "") return null;
    const n = Number(amountText);
    return Number.isFinite(n) ? n : null;
  };

  const onDeposit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    const amt = parseAmount();
    if (!amt || amt <= 0) {
      setError("Amount must be greater than 0.");
      return;
    }
    setLoading(true);
    try {
      await depositFunds(amt);
      setAmountText("");
      await fetchWallet();
    } catch (err: any) {
      setError(err?.response?.data?.title ?? "Failed to deposit funds");
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (v: number) => `$${v.toFixed(2)}`;

  return (
    <div className="wallet">
      <h2>Wallet</h2>
      {wallet ? (
        <>
          <ul>
            <li>Cash Balance: {formatCurrency(wallet.balance)} {wallet.currency}</li>
            <li>Portfolio Value: {formatCurrency(wallet.portfolioValue)}</li>
            <li><strong>Total Balance: {formatCurrency(wallet.totalBalance)}</strong></li>
            <li>Last Updated: {new Date(wallet.updatedAt).toLocaleTimeString()}</li>
          </ul>
          <div
            style={{
              display: "flex",
              gap: 12,
              alignItems: "center",
              marginTop: 8,
              fontWeight: 600
            }}
            aria-label="Profit/Loss summary"
          >
            <span>Profit/Loss:</span>
            <span
              style={{
                color:
                  wallet.unrealizedPnl > 0 ? "#2e7d32" : wallet.unrealizedPnl < 0 ? "#c62828" : "#ccc"
              }}
            >
              {formatCurrency(wallet.unrealizedPnl)} ({wallet.unrealizedPnlPercent.toFixed(2)}%)
            </span>
          </div>

          <form
            onSubmit={onDeposit}
            style={{ display: "flex", gap: 8, alignItems: "flex-end", marginTop: 12 }}
          >
            <label style={{ display: "flex", flexDirection: "column", flex: "0 1 200px" }}>
              Deposit amount
              <input
                type="number"
                inputMode="decimal"
                min={1}
                step="0.01"
                value={amountText}
                onChange={(e) => setAmountText(e.target.value)}
                style={{ padding: 8, border: "1px solid #bbb", borderRadius: 4 }}
                aria-label="Deposit amount"
                placeholder="Enter amount"
              />
            </label>
            <button type="submit" className="logout-button" disabled={loading}>
              {loading ? "Depositing..." : "Deposit"}
            </button>
          </form>
          {error && (
            <div style={{ marginTop: 8, border: "1px solid #7a3a3a", background: "#2a1a1a", color: "#ffb3b3", padding: 8, borderRadius: 4 }}>
              {error}
            </div>
          )}
        </>
      ) : (
        <p>Loading...</p>
      )}
    </div>
  );
};

export default WalletComponent;
