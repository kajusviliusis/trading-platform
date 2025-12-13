import React, { useState } from "react";
import { createOrder } from "../api";

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
  minHeight: 220,
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

const OrderModal: React.FC<Props> = ({ stockId, symbol, type, onClose, onSuccess }) => {
  const [quantityText, setQuantityText] = useState<string>("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const parseQuantity = (): number | null => {
    if (quantityText.trim() === "") return null;
    const n = Number(quantityText);
    return Number.isFinite(n) ? n : null;
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!Number.isFinite(stockId) || stockId <= 0) {
      setError("Invalid stock.");
      return;
    }
    const qty = parseQuantity();
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
      setError(err?.response?.data?.title ?? "Failed to place order");
    }
  };

  return (
    <div style={overlayStyle} onClick={onClose}>
      <div style={panelStyle} onClick={(e) => e.stopPropagation()}>
        <h3 style={{ margin: 0, textAlign: "center" }}>
          {type} {symbol}
        </h3>
        <p style={{ margin: 0, textAlign: "center", color: "#cfcfcf" }}>Enter quantity</p>
        <form onSubmit={onSubmit}>
          {error && <div style={errorStyle}>{error}</div>}
          <label style={{ display: "block", marginBottom: 10 }}>
            Quantity
            <input
              type="number"
              inputMode="numeric"
              min={1}
              value={quantityText}
              onChange={(e) => setQuantityText(e.target.value)}
              aria-label="Quantity"
              style={inputStyle}
            />
          </label>
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