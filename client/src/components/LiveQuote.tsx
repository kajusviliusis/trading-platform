import React, { useEffect, useState } from "react";
import { getLiveQuote } from "../api";
import { StockQuote } from "../types";

const LiveQuote: React.FC<{ symbol: string }> = ({ symbol }) => {
    const [quote, setQuote] = useState<StockQuote | null>(null);

    useEffect(() => {
        const fetchQuote = async () => {
            try {
                const data = await getLiveQuote(symbol);
                setQuote(data);
            } catch (err) {
                console.error("Error fetching quote:", err);
            }
        };

        fetchQuote();
        const interval = setInterval(fetchQuote, 5000); // kas 5 sekundes
        return () => clearInterval(interval);
    }, [symbol]);

    return (
        <div>
            <h3>{symbol}</h3>
            {quote ? (
                <p>
                    Current: ${quote.currentPrice} | High: ${quote.high} | Low: ${quote.low}
                </p>
            ) : (
                <p>Loading...</p>
            )}
        </div>
    );
};

export default LiveQuote;
