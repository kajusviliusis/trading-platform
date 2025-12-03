import React, { useEffect, useState } from "react";
import { getHoldings } from "../api";
import { Holding } from "../types";

const Holdings: React.FC = () => {
    const [holdings, setHoldings] = useState<Holding[]>([]);

    useEffect(() => {
        getHoldings().then(setHoldings);
    }, []);

    return (
        <div>
            <h2>Holdings</h2>
            <ul>
                {holdings.map(h => (
                    <li key={h.stockId}>
                        {h.stockName}: {h.quantity} shares @ ${h.currentPrice}
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default Holdings;
