import React from "react";
import Holdings from "../components/Holdings";
import Orders from "../components/Orders";
import Transactions from "../components/Transactions";
import PlaceOrder from "../components/PlaceOrder";

const Dashboard: React.FC = () => {
    return (
        <div style={{ padding: "2rem" }}>
            <h1>Trading Dashboard</h1>
            <PlaceOrder />
            <Holdings />
            <Orders />
            <Transactions />
        </div>
    );
};

export default Dashboard;
