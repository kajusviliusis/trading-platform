import React, { useEffect } from "react";
import { useNavigate } from "react-router-dom";

const OAuthCallback: React.FC = () => {
  const navigate = useNavigate();

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const token = params.get("access_token");
    const expiresIn = params.get("expires_in");

    if (token) {
      localStorage.setItem("accessToken", token);
      navigate("/dashboard", { replace: true });
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