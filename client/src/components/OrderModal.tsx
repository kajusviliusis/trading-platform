import React, { useEffect, useMemo, useState } from "react";
import { createOrder, getLiveQuote, getWalletBalance } from "../api";

type OrderType = "BUY" | "SELL";

type Props = {
  stockId: number;
  symbol: string;
  type: OrderType;
  onClose: () => void;
  onSuccess?: () => void;
};

const overlayStyle: React.CSSProperties = {
  position: "fixed",
  inset: 0,
  background: "rgba(0,0,0,0.45)",
  display: "grid",
  placeItems: "center",
  zIndex: 1000,
};

const panelStyle: React.CSSProperties = {
  width: 360,
  minHeight: 260,
  border: "1px solid #333",
  borderRadius: 10,
  background: "#1e1e1e",
  color: "#fff",
  padding: 16,
  boxSizing: "border-box",
  display: "flex",
  flexDirection: "column",
  gap: 12,
};

const inputStyle: React.CSSProperties = {
  display: "block",
  width: "100%",
  padding: 8,
  marginTop: 4,
  border: "1px solid #555",
  borderRadius: 4,
  background: "#121212",
  color: "#fff",
};

const errorStyle: React.CSSProperties = {
  border: "1px solid #7a3a3a",
  background: "#2a1a1a",
  color: "#ffb3b3",
  padding: 8,
  borderRadius: 4,
  marginBottom: 10,
};

const sliderStyle: React.CSSProperties = {
  width: "100%",
};

const OrderModal: React.FC<Props> = ({ stockId, symbol, type, onClose, onSuccess }) => {
  const [quantityText, setQuantityText] = useState<string>("");
  const [cashText, setCashText] = useState<string>("");
  const [cashAmount, setCashAmount] = useState<number>(0);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [price, setPrice] = useState<number | null>(null);
  const [walletCash, setWalletCash] = useState<number | null>(null);

  useEffect(() => {
    let mounted = true;
    const load = async () => {
      try {
        const [quote, wallet] = await Promise.all([getLiveQuote(symbol), getWalletBalance()]);
        if (!mounted) return;
        const p =
          (quote as any).CurrentPrice ??
          (quote as any).currentPrice ??
          null;
        const parsedPrice = typeof p === "number" && Number.isFinite(p) ? p : null;
        setPrice(parsedPrice);
        setWalletCash(wallet.balance);
      } catch (err) {
        console.error("Failed to load quote or wallet:", err);
      }
    };
    load();
    const i = setInterval(load, 30000);
    return () => {
      mounted = false;
      clearInterval(i);
    };
  }, [symbol]);

  const parseQuantity = (): number | null => {
    if (quantityText.trim() === "") return null;
    const n = Number(quantityText);
    return Number.isFinite(n) ? n : null;
  };

  const { derivedQuantityExact, derivedQuantityFloor } = useMemo(() => {
    if (!price || cashAmount <= 0) return { derivedQuantityExact: null as number | null, derivedQuantityFloor: null as number | null };
    const exact = cashAmount / price;
    const floor = Math.floor(exact);
    return {
      derivedQuantityExact: exact > 0 ? exact : null,
      derivedQuantityFloor: floor > 0 ? floor : null,
    };
  }, [cashAmount, price]);

  useEffect(() => {
    if (derivedQuantityExact != null) {
      setQuantityText(derivedQuantityExact.toFixed(4));
    }
  }, [derivedQuantityExact]);

  const maxCash = useMemo(() => {
    if (type === "BUY") {
      return walletCash ?? 0;
    }
    return walletCash ?? 0;
  }, [type, walletCash]);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!Number.isFinite(stockId) || stockId <= 0) {
      setError("Invalid stock.");
      return;
    }

    const qtyFromCash =
      type === "SELL" ? derivedQuantityExact : derivedQuantityFloor;

    const qtyManualParsed = parseQuantity();
    const qtyManual =
      qtyManualParsed == null
        ? null
        : type === "SELL"
        ? qtyManualParsed
        : Math.floor(qtyManualParsed);

    const qty = qtyFromCash ?? qtyManual;

    if (!qty || qty <= 0) {
      setError("Quantity must be greater than 0.");
      return;
    }

    setSubmitting(true);
    try {
      await createOrder({ stockId, quantity: qty, type });
      setSubmitting(false);
      if (onSuccess) onSuccess();
      onClose();
    } catch (err: any) {
      setSubmitting(false);
      const status = err?.response?.status;
      const serverTitle = err?.response?.data?.title ?? err?.response?.data?.message;
      const message = serverTitle ?? err?.message ?? "Failed to place order";
      setError(status ? `${message} (HTTP ${status})` : message);
    }
  };

  return (
    <div style={overlayStyle} onClick={onClose}>
      <div style={panelStyle} onClick={(e) => e.stopPropagation()}>
        <h3 style={{ margin: 0, textAlign: "center" }}>
          {type} {symbol}
        </h3>
        <p style={{ margin: 0, textAlign: "center", color: "#cfcfcf" }}>Choose cash or enter quantity</p>
        <form onSubmit={onSubmit}>
          {error && <div style={errorStyle}>{error}</div>}
          <label style={{ display: "block", marginBottom: 10 }}>
            Cash amount {price ? `(≈ ${price.toFixed(2)} per share)` : "(price unavailable)"}
            <input
              type="range"
              min={0}
              max={Math.max(0, Math.floor((maxCash ?? 0) * 100) / 100)}
              step={0.01}
              value={cashAmount}
              onChange={(e) => {
                const v = Number(e.target.value);
                setCashAmount(Number.isFinite(v) ? v : 0);
                setCashText(Number.isFinite(v) ? v.toString() : "");
              }}
              aria-label="Cash amount slider"
              style={sliderStyle}
              disabled={!price || maxCash <= 0}
            />
            <input
              type="number"
              inputMode="decimal"
              min={0}
              step="0.01"
              value={cashText}
              onChange={(e) => {
                const t = e.target.value;
                setCashText(t);
                const v = Number(t);
                setCashAmount(Number.isFinite(v) ? v : 0);
              }}
              aria-label="Cash amount"
              style={inputStyle}
              placeholder="Enter cash (e.g., 100.00)"
              disabled={!price}
            />
            <small style={{ color: "#cfcfcf" }}>
              {type === "BUY"
                ? `Available cash: ${walletCash != null ? walletCash.toFixed(2) : "-"}$`
                : `Cash field converts to shares using current price.`}
            </small>
          </label>
          <label style={{ display: "block", marginBottom: 10 }}>
            Quantity
            <input
              type="number"
              inputMode="decimal"
              min={0}
              step="0.0001"
              value={quantityText}
              onChange={(e) => setQuantityText(e.target.value)}
              aria-label="Quantity"
              style={inputStyle}
              placeholder="Enter quantity (supports decimals)"
            />
          </label>
          <div style={{ color: "#cfcfcf", marginBottom: 8 }}>
            {price && quantityText
              ? `Estimated total: ${(Number(quantityText) * price).toFixed(2)}$`
              : cashAmount > 0 && price
              ? `Estimated shares: ${derivedQuantityExact != null ? derivedQuantityExact.toFixed(4) : 0}`
              : null}
          </div>
          <div style={{ display: "flex", gap: 8 }}>
            <button type="submit" className="logout-button" disabled={submitting}>
              {submitting ? "Submitting..." : "Confirm"}
            </button>
            <button type="button" className="logout-button" onClick={onClose}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default OrderModal;