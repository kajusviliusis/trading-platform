import React from "react";
import Holdings from "../components/Holdings";
import Orders from "../components/Orders";
import Transactions from "../components/Transactions";
import PlaceOrder from "../components/PlaceOrder";
import "./Dashboard.css";

const Dashboard: React.FC = () => {
    return (
        <div className="dashboard">
            <h1 className="dashboard-title">Trading Dashboard</h1>
            <div className="dashboard-grid">
                <div className="card"><PlaceOrder /></div>
                <div className="card"><Holdings /></div>
                <div className="card"><Orders /></div>
                <div className="card"><Transactions /></div>
            </div>
        </div>
    );
};

export default Dashboard;
