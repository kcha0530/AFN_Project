import { useState, useRef, useEffect } from "react";

const API = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

function LoginModal({ onLoginSuccess, onClose }) {
  const [tab, setTab]       = useState("login");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [email, setEmail]   = useState("");
  const [fullName, setFullName] = useState("");
  const [loading, setLoading]   = useState(false);
  const [error, setError]       = useState("");
  const inputRef = useRef(null);

  useEffect(() => { inputRef.current?.focus(); }, [tab]);

  async function handleLogin(e) {
    e.preventDefault();
    setError("");
    setLoading(true);
    const ctrl = new AbortController();
    const timer = setTimeout(() => ctrl.abort(), 8000);
    try {
      const res = await fetch(`${API}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
        signal: ctrl.signal,
      });
      if (res.status === 429) { setError("Too many requests. Please wait a minute."); return; }
      const data = await res.json();
      if (res.ok && data.success) {
        localStorage.setItem("authToken", data.token);
        localStorage.setItem("username", data.username);
        localStorage.setItem("userId", data.userId);
        localStorage.setItem("email", data.email || "");
        onLoginSuccess(data.username);
      } else {
        setError(data.message || "Invalid username or password.");
      }
    } catch (err) {
      setError(err.name === "AbortError" ? "Connection timed out." : "Could not connect to the server.");
    } finally {
      clearTimeout(timer);
      setLoading(false);
    }
  }

  async function handleRegister(e) {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const res = await fetch(`${API}/auth/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password, email, fullName }),
      });
      const data = await res.json();
      if (res.ok && data.success) {
        setTab("login");
        setError("");
        setPassword("");
      } else {
        setError(data.message || "Registration failed.");
      }
    } catch {
      setError("Could not connect to the server.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="modal-backdrop" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal modal--auth">
        <div className="modal-header">
          <div className="auth-tabs">
            <button className={`auth-tab${tab === "login" ? " active" : ""}`} onClick={() => { setTab("login"); setError(""); }}>Sign in</button>
            <button className={`auth-tab${tab === "register" ? " active" : ""}`} onClick={() => { setTab("register"); setError(""); }}>Register</button>
          </div>
          <button className="modal-close" onClick={onClose}>✕</button>
        </div>

        {tab === "login" ? (
          <form onSubmit={handleLogin} className="modal-form">
            <div className="form-group">
              <label>Username</label>
              <input ref={inputRef} type="text" value={username} onChange={e => setUsername(e.target.value)} placeholder="krit" required />
            </div>
            <div className="form-group">
              <label>Password</label>
              <input type="password" value={password} onChange={e => setPassword(e.target.value)} placeholder="••••••••" required />
            </div>
            {error && <p className="form-error">{error}</p>}
            <button type="submit" className="btn-confirm" disabled={loading}>
              {loading ? <span className="spinner-sm" /> : "Sign in"}
            </button>
          </form>
        ) : (
          <form onSubmit={handleRegister} className="modal-form">
            <div className="form-group">
              <label>Full name</label>
              <input ref={inputRef} type="text" value={fullName} onChange={e => setFullName(e.target.value)} placeholder="John Smith" />
            </div>
            <div className="form-group">
              <label>Username *</label>
              <input type="text" value={username} onChange={e => setUsername(e.target.value)} placeholder="johnsmith" required />
            </div>
            <div className="form-group">
              <label>Email *</label>
              <input type="email" value={email} onChange={e => setEmail(e.target.value)} placeholder="john@example.com" required />
            </div>
            <div className="form-group">
              <label>Password *</label>
              <input type="password" value={password} onChange={e => setPassword(e.target.value)} placeholder="••••••••" required />
            </div>
            {error && <p className="form-error">{error}</p>}
            <button type="submit" className="btn-confirm" disabled={loading}>
              {loading ? <span className="spinner-sm" /> : "Create account"}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}

export default LoginModal;
