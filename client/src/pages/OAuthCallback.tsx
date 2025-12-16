import React, { useEffect } from "react";
import { useNavigate } from "react-router-dom";

const OAuthCallback: React.FC = () => {
  const navigate = useNavigate();

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const token = params.get("access_token");
    console.log("Callback token:", token);
    if (token) {
      localStorage.setItem("accessToken", token);
      setTimeout(() => navigate("/dashboard", { replace: true }), 200);
    } else {
      navigate("/login", { replace: true });
    }
  }, [navigate]);

  return (
    <div style={{ color: "#fff", padding: 20 }}>
      Completing sign-in...
    </div>
  );
};

export default OAuthCallback;