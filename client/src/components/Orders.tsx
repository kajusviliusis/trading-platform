import React, { useEffect, useState } from "react";
import { getOrders } from "../api";
import { Order } from "../types";

const Orders: React.FC = () => {
    const [orders, setOrders] = useState<Order[]>([]);

    useEffect(() => {
        getOrders().then(setOrders).catch(console.error);
    }, []);

    return (
        <div>
            <h2>Orders</h2>
            <table className="table">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Stock</th>
                        <th>Quantity</th>
                        <th>Type</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    {orders.map(o => (
                        <tr key={o.id}>
                            <td>{o.id}</td>
                            <td>{o.stockId}</td>
                            <td>{o.quantity}</td>
                            <td style={{ color: o.type === "BUY" ? "limegreen" : "red" }}>
                                {o.type}
                            </td>
                            <td>{"Executed"}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );

};

export default Orders;
