import React from "react";
import WalletComponent from "../components/Wallet";
import LiveQuote from "../components/LiveQuote";
import Holdings from "../components/Holdings";
import Orders from "../components/Orders";
import Transactions from "../components/Transactions";
import PlaceOrder from "../components/PlaceOrder";

const Dashboard: React.FC = () => {
    return (
        <div className="dashboard">
            <h1 className="dashboard-title">Trading Dashboard</h1>
            <div className="dashboard-grid">
                <div className="card"><WalletComponent /></div>
                <div className="card"><LiveQuote symbol ="AAPL" /></div>
                <div className="card"><PlaceOrder /></div>
                <div className="card"><Holdings /></div>
                <div className="card"><Orders /></div>
                <div className="card"><Transactions /></div>
            </div>
        </div>
    );
};

export default Dashboard;
