import React, { useState } from "react";
import { login, register } from "../api";
import "./LoginPage.css";

const RegisterPage: React.FC = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    if (!username || !password) {
      setError("Username and password are required.");
      return;
    }
    setSubmitting(true);
    try {
      await register(username, password);
      await login(username, password);
      window.location.href = "/";
    } catch (err: any) {
      const status = err?.response?.status;
      const serverTitle = err?.response?.data?.title ?? err?.response?.data?.message;
      const message = serverTitle ?? err?.message ?? "Registration failed";
      setError(status ? `${message} (HTTP ${status})` : message);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="login-container">
      <form className="login-form" onSubmit={onSubmit}>
        <img src="/6nuliai.png" alt="Logo" className="login-logo" />
        <h2>Create account</h2>
        {error && <div className="error">{error}</div>}
        <label>
          Username
          <input
            type="text"
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
            autoComplete="new-password"
          />
        </label>
        <button type="submit" className="logout-button" disabled={submitting}>
          {submitting ? "Registering..." : "Register"}
        </button>
        <div style={{ marginTop: 8, textAlign: "center" }}>
          <a href="/login">Already have an account? Sign in</a>
        </div>
      </form>
    </div>
  );
};

export default RegisterPage;