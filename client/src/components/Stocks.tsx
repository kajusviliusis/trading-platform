import React, { useEffect, useState } from "react";
import { getStocks } from "../api";
import { Stock } from "../types";

const Stocks: React.FC = () => {
    const [stocks, setStocks] = useState<Stock[]>([]);

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

    return (
        <div className="stocks">
            <h2>Stocks</h2>
            <table>
                <thead>
                    <tr>
                        <th>Symbol</th>
                        <th>Name</th>
                        <th>Price</th>
                        <th>Updated At</th>
                    </tr>
                </thead>
                <tbody>
                    {stocks.map(s => (
                        <tr key={s.id}>
                            <td>{s.symbol}</td>
                            <td>{s.name}</td>
                            <td style={{ color: s.price >= 0 ? "green" : "red" }}>
                                ${s.price}
                            </td>
                            <td>{new Date(s.updatedAt).toLocaleTimeString()}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default Stocks;
