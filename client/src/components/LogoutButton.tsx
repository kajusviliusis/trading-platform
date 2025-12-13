import React from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { logout } from "../api";

const LogoutButton: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();

  const onLogout = () => {
    logout();
    navigate("/login", { replace: true, state: { from: location } });
  };

  return (
    <button className="logout-button" onClick={onLogout}>
      Logout
    </button>
  );
};

export default LogoutButton;