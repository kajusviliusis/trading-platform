import React, { useEffect, useState } from "react";
import { getStocks } from "../api";
import { Stock } from "../types";

const Stocks: React.FC = () => {
    const [stocks, setStocks] = useState<Stock[]>([]);

    useEffect(() => {
        getStocks().then(setStocks).catch(console.error);
    }, []);

    return (
        <div>
            <h2>Stocks</h2>
            <table className="table">
                <thead>
                    <tr>
                        <th>Symbol</th>
                        <th>Name</th>
                        <th>Current Price</th>
                    </tr>
                </thead>
                <tbody>
                    {stocks.map(s => (
                        <tr key={s.id}>
                            <td>{s.symbol}</td>
                            <td>{s.name}</td>
                            <td>${s.price}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default Stocks;
