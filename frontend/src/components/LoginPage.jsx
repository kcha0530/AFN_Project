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

    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 8000);

    try {
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
        signal: controller.signal,
      });
      clearTimeout(timeoutId);

      if (response.status === 429) {
        throw new Error("Too many requests. Please wait a moment and try again.");
      }
      if (response.status === 401) {
        throw new Error("Invalid username or password.");
      }

      const data = await response.json();
      if (!response.ok) throw new Error(data?.error || "Login failed");

      localStorage.setItem("authToken", data.token);
      localStorage.setItem("username", data.username || username);
      localStorage.setItem("userId", data.userId);
      localStorage.setItem("email", data.email);

      setMessage("Login successful! Loading dashboard…");
      setUsername("");
      setPassword("");
      onLoginSuccess?.(data.username || username, data.token);
    } catch (err) {
      clearTimeout(timeoutId);
      if (err.name === "AbortError") {
        setError("Connection timed out. Check that the server is running and try again.");
      } else {
        setError(err.message || "Unable to login. Please try again.");
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-root">
      <div className="login-panel">
        {/* Logo */}
        <div className="login-logo-row">
          <div className="login-logo-icon">✈️</div>
          <div>
            <div className="login-logo-text">AFN Project</div>
            <div className="login-logo-sub">React + ASP.NET Core + .NET Aspire</div>
          </div>
        </div>

        <h1 className="login-heading">Welcome back</h1>
        <p className="login-subheading">Sign in to access the secure dashboard</p>

        <form onSubmit={handleLogin}>
          <div className="form-field">
            <label htmlFor="username">Username</label>
            <input
              id="username"
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Enter your username"
              autoComplete="username"
              autoFocus
              required
            />
          </div>

          <div className="form-field">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter your password"
              autoComplete="current-password"
              required
            />
          </div>

          <button type="submit" className="btn-primary" disabled={loading}>
            {loading ? (
              <><span className="spinner" />Signing in…</>
            ) : (
              "Sign in →"
            )}
          </button>
        </form>

        {message && <div className="login-feedback success">{message}</div>}
        {error   && <div className="login-feedback error">{error}</div>}

        {/* Demo hint */}
        <div className="demo-hint">
          <div className="demo-hint-title">Demo credentials</div>
          <div className="demo-hint-row">Username <span>krit</span></div>
          <div className="demo-hint-row">Password <span>krit</span></div>
        </div>
      </div>
    </div>
  );
}

export default LoginPage;
