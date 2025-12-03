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
            <ul>
                {orders.map(o => (
                    <li key={o.id}>
                        {o.type} {o.quantity} shares of stock {o.stockId} at ${o.priceAtExecution}
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default Orders;
