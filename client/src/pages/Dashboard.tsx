import React from "react";
import WalletComponent from "../components/Wallet";
import Stocks from "../components/Stocks";
import Holdings from "../components/Holdings";
import Transactions from "../components/Transactions";
import LogoutButton from "../components/LogoutButton";
import "./Dashboard.css";

const Dashboard: React.FC = () => {
  return (
    <div className="dashboard-shell">
      <aside className="dashboard-sidebar">
        <img src="/6nuliai.png" alt="Logo" className="dashboard-logo-rotated" />
      </aside>
      <main className="dashboard">
        <header className="dashboard-header">
          <h2>Workspace</h2>
          <LogoutButton />
        </header>
        <div className="dashboard-grid">
          <div className="card"><WalletComponent /></div>
          <div className="card"><Stocks /></div>
          <div className="card"><Holdings /></div>
          <div className="card"><Transactions /></div>
        </div>
      </main>
    </div>
  );
};

export default Dashboard;
