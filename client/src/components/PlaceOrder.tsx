import React, { useState } from "react";
import { createOrder } from "../api";
import { Order } from "../types";

const PlaceOrder: React.FC = () => {
    const [stockId, setStockId] = useState<number>(0);
    const [quantity, setQuantity] = useState<number>(0);
    const [type, setType] = useState<"BUY" | "SELL">("BUY");
    const [result, setResult] = useState<Order | null>(null);

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        try {
            const order = await createOrder({ stockId, quantity, type });
            setResult(order);
        } catch (err) {
            console.error("Failed to place order", err);
        }
    }

    return (
        <div>
            <h2>Place Order</h2>
            <form onSubmit={handleSubmit}>
                <input
                    type="number"
                    placeholder="Stock ID"
                    value={stockId}
                    onChange={e => setStockId(Number(e.target.value))}
                />
                <input
                    type="number"
                    placeholder="Quantity"
                    value={quantity}
                    onChange={e => setQuantity(Number(e.target.value))}
                />
                <select value={type} onChange={e => setType(e.target.value as "BUY" | "SELL")}>
                    <option value="BUY">BUY</option>
                    <option value="SELL">SELL</option>
                </select>
                <button type="submit">Submit</button>
            </form>

            {result && (
                <p>
                    Order placed: {result.type} {result.quantity} shares of stock {result.stockId} at ${result.priceAtExecution}
                </p>
            )}
        </div>
    );
};

export default PlaceOrder;
