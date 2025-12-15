import React, { useEffect, useMemo, useState } from "react";
import { getStocks } from "../api";
import { Stock } from "../types";
import OrderModal from "./OrderModal";

type OrderType = "BUY" | "SELL";

const Stocks: React.FC = () => {
  const [stocks, setStocks] = useState<Stock[]>([]);
  const [query, setQuery] = useState("");
  const [modal, setModal] = useState<{ stockId: number; symbol: string; type: OrderType } | null>(null);

  useEffect(() => {
    const fetchStocks = async () => {
      try {
        const data = await getStocks();
        setStocks(data);
      } catch (err) {
        console.error("Error fetching stocks:", err);
      }
    };
    fetchStocks();
    const interval = setInterval(fetchStocks, 5000);
    return () => clearInterval(interval);
  }, []);

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return [];
    return stocks.filter(
      (s) =>
        s.symbol.toLowerCase().includes(q) ||
        (s.name?.toLowerCase() ?? "").includes(q)
    );
  }, [stocks, query]);

  return (
    <div className="stocks">
      <h2>Stocks</h2>

      <div style={{ marginBottom: 12 }}>
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search by symbol or name..."
          style={{
            width: "100%",
            padding: 8,
            border: "1px solid #bbb",
            borderRadius: 4,
          }}
          aria-label="Search stocks"
        />
      </div>

      {query.trim().length === 0 ? (
        <div className="empty-state">
          Start typing to search stocks...
        </div>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Symbol</th>
              <th>Name</th>
              <th>Price</th>
              <th>Updated At</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((s) => (
              <tr key={s.id}>
                <td>{s.symbol}</td>
                <td>{s.name}</td>
                <td style={{ color: s.price >= 0 ? "green" : "red" }}>
                  ${s.price}
                </td>
                <td>{new Date(s.updatedAt).toLocaleTimeString()}</td>
                <td>
                  <button
                    className="logout-button"
                    onClick={() => setModal({ stockId: s.id, symbol: s.symbol, type: "BUY" })}
                    style={{ marginRight: 8 }}
                  >
                    Buy
                  </button>
                  <button
                    className="logout-button"
                    onClick={() => setModal({ stockId: s.id, symbol: s.symbol, type: "SELL" })}
                  >
                    Sell
                  </button>
                </td>
              </tr>
            ))}
            {filtered.length === 0 && (
              <tr>
                <td colSpan={5} style={{ textAlign: "center", padding: 12 }}>
                  No matching stocks
                </td>
              </tr>
            )}
          </tbody>
        </table>
      )}

      {modal && (
        <OrderModal
          stockId={modal.stockId}
          symbol={modal.symbol}
          type={modal.type}
          onClose={() => setModal(null)}
          onSuccess={() => {
            window.dispatchEvent(new CustomEvent("wallet:refresh"));
            window.dispatchEvent(new CustomEvent("holdings:refresh"));
            window.dispatchEvent(new CustomEvent("transactions:refresh"));
          }}
        />
      )}
    </div>
  );
};

export default Stocks;
