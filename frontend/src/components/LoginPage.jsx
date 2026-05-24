import { useState } from "react";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

function LoginPage({ onLoginSuccess }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleLogin(event) {
    event.preventDefault();
    setLoading(true);
    setError("");
    setMessage("");

    try {
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
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
      localStorage.setItem("username", data.username || username);
      localStorage.setItem("userId", data.userId);
      localStorage.setItem("email", data.email);
      setMessage("Login successful. Redirecting to dashboard...");
      setUsername("");
      setPassword("");
      onLoginSuccess?.(data.username || username, data.token);
    } catch (err) {
      setError(err.message || "Unable to login");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="card auth-card">
      <h2>Login to Continue</h2>
      <p className="card-subtitle">Only authenticated users can access the dashboard.</p>

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

      {message && <p className="success">{message}</p>}
      {error && <p className="error">{error}</p>}

      <div className="login-hint">
        <p>Demo credentials:</p>
        <p><strong>Username:</strong> krit</p>
        <p><strong>Password:</strong> krit</p>
      </div>
    </div>
  );
}

export default LoginPage;
