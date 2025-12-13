import React, { useEffect, useState } from "react";
import { getHoldings } from "../api";
import { Holding } from "../types";

const formatShares = (qty: number): string => qty.toFixed(4);
const formatMoney = (v: number): string => v.toFixed(2);

const Holdings: React.FC = () => {
  const [holdings, setHoldings] = useState<Holding[]>([]);

  useEffect(() => {
    getHoldings().then(setHoldings).catch(console.error);
  }, []);

  return (
    <div>
      <h2>Holdings</h2>
      <ul>
        {holdings.map((h) => {
          const value = Number(h.quantity) * Number(h.currentPrice);
          return (
            <li key={h.stockId}>
              {h.stockName}: {formatShares(Number(h.quantity))} shares @ ${formatMoney(Number(h.currentPrice))}
              {" • "}
              Value: ${formatMoney(value)}
            </li>
          );
        })}
      </ul>
    </div>
  );
};

export default Holdings;
