import { useState } from "react";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [token, setToken] = useState(localStorage.getItem("authToken") || "");
  const [message, setMessage] = useState("");
  const [secureMessage, setSecureMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleLogin(event) {
    event.preventDefault();
    setLoading(true);
    setError("");
    setMessage("");
    setSecureMessage("");

    try {
      const response = await fetch(`${API_BASE_URL}/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ username, password }),
      });

      const data = await response.json();
      if (!response.ok) {
        throw new Error(data?.error || "Login failed");
      }

      localStorage.setItem("authToken", data.token);
      setToken(data.token);
      setMessage("Login successful. JWT token stored locally.");
      setUsername("");
      setPassword("");
    } catch (err) {
      setError(err.message || "Unable to login");
    } finally {
      setLoading(false);
    }
  }

  async function handleSecureCheck() {
    setLoading(true);
    setError("");
    setSecureMessage("");

    try {
      const authToken = localStorage.getItem("authToken");
      if (!authToken) {
        throw new Error("Please login first to get a JWT token.");
      }

      const response = await fetch(`${API_BASE_URL}/secure`, {
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      const data = await response.json();
      if (!response.ok) {
        throw new Error(data?.error || "Secure check failed");
      }

      setSecureMessage(data.message || "Secure route accessed.");
    } catch (err) {
      setError(err.message || "Unable to verify secure route.");
    } finally {
      setLoading(false);
    }
  }

  function handleLogout() {
    localStorage.removeItem("authToken");
    setToken("");
    setMessage("Logged out successfully.");
    setSecureMessage("");
  }

  return (
    <div className="card auth-card">
      <h2>Login to Your API</h2>
      <p className="card-subtitle">Authenticate with JWT and check secure API access.</p>

      <form onSubmit={handleLogin} className="login-form">
        <label>
          Username
          <input
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            placeholder="Enter username"
          />
        </label>

        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Enter password"
          />
        </label>

        <button type="submit" disabled={loading}>
          {loading ? "Signing in..." : "Sign In"}
        </button>
      </form>

      <div className="login-actions">
        <button type="button" onClick={handleSecureCheck} disabled={loading || !token}>
          Check Secure Route
        </button>
        <button type="button" onClick={handleLogout} className="outline">
          Logout
        </button>
      </div>

      {message && <p className="success">{message}</p>}
      {secureMessage && <p className="success">{secureMessage}</p>}
      {error && <p className="error">{error}</p>}

      <div className="login-hint">
        <p>Try credentials: <strong>admin</strong> / <strong>P@ssw0rd</strong></p>
      </div>
    </div>
  );
}

export default LoginPage;
