import React, { useEffect, useState } from "react";
import { getCurrentUserId, getUserById } from "../api";

type UserInfo = {
  id: number;
  username: string;
  email: string;
  createdAt: string;
};

const ProfileModal: React.FC<{ open: boolean; onClose: () => void }> = ({ open, onClose }) => {
  const [loading, setLoading] = useState(false);
  const [user, setUser] = useState<UserInfo | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!open) return;
    setError(null);
    setLoading(true);
    const id = getCurrentUserId();
    if (id == null) {
      setError("Not authenticated.");
      setLoading(false);
      return;
    }
    getUserById(id)
      .then((u) => setUser(u))
      .catch((e) => setError(e?.response?.data?.title ?? e.message ?? "Failed to load profile"))
      .finally(() => setLoading(false));
  }, [open]);

  if (!open) return null;

  return (
    <div style={backdropStyle} onClick={onClose}>
      <div style={modalStyle} onClick={(e) => e.stopPropagation()}>
        <h3 style={{ marginTop: 0 }}>Profile</h3>
        {loading && <div>Loading...</div>}
        {error && <div style={errorStyle}>{error}</div>}
        {user && !loading && !error && (
          <div style={{ lineHeight: 1.6 }}>
            <div><strong>ID:</strong> {user.id}</div>
            <div><strong>Username:</strong> {user.username}</div>
            <div><strong>Email:</strong> {user.email || "-"}</div>
            <div><strong>Created:</strong> {new Date(user.createdAt).toLocaleString()}</div>
          </div>
        )}
        <div style={{ marginTop: 16, textAlign: "right" }}>
          <button onClick={onClose} className="logout-button">Close</button>
        </div>
      </div>
    </div>
  );
};

const backdropStyle: React.CSSProperties = {
  position: "fixed",
  inset: 0,
  background: "rgba(0,0,0,0.5)",
  display: "flex",
  alignItems: "center",
  justifyContent: "center",
  zIndex: 1000,
};

const modalStyle: React.CSSProperties = {
  width: 360,
  maxWidth: "90vw",
  background: "#1e1e1e",
  color: "#fff",
  border: "1px solid #333",
  borderRadius: 8,
  padding: 16,
  boxShadow: "0 6px 18px rgba(0,0,0,0.5)",
};

const errorStyle: React.CSSProperties = {
  border: "1px solid #7a3a3a",
  background: "#2a1a1a",
  color: "#ffb3b3",
  padding: 8,
  borderRadius: 4,
  marginBottom: 10,
};

export default ProfileModal;