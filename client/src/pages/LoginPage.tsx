import React, { useState } from "react";
import { useLocation, useNavigate, Link } from "react-router-dom";
import { login } from "../api";
import "./LoginPage.css";

const API_BASE_URL = "https://localhost:7212";

const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation() as any;
  const redirectTo = location.state?.from?.pathname ?? "/dashboard";

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await login(username, password);
      navigate(redirectTo, { replace: true });
    } catch (err: any) {
      setError(err?.response?.data?.title ?? "Invalid credentials");
    } finally {
      setLoading(false);
    }
  };

  const startGoogleLogin = () => {
    const returnUrl = `${window.location.origin}/oauth-callback`;
    const url = `${API_BASE_URL}/api/auth/external/Google?returnUrl=${encodeURIComponent(returnUrl)}`;
    window.location.href = url;
  };

  return (
    <div className="login-container">
      <form className="login-form" onSubmit={onSubmit}>
        <img src="/6nuliai.png" alt="Logo" className="login-logo" />
        <h2>Sign in</h2>
        {error && <div className="error">{error}</div>}
        <label>
          Username
          <input
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            autoComplete="username"
          />
        </label>
        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="current-password"
          />
        </label>
        <button type="submit" disabled={loading}>
          {loading ? "Signing in..." : "Login"}
        </button>

        <div style={{ marginTop: 10 }}>
          <button type="button" onClick={startGoogleLogin}>
            Continue with Google
          </button>
        </div>

        <div style={{ marginTop: 8, textAlign: "center" }}>
          <Link to="/register">Don&apos;t have an account? Register here</Link>
        </div>
      </form>
    </div>
  );
};

export default LoginPage;