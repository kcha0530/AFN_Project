import { useState } from "react";
import Header from "./components/Header.jsx";
import Footer from "./components/Footer.jsx";
import LoginPage from "./components/LoginPage.jsx";
import Dashboard from "./components/Dashboard.jsx";

function App() {
  const [userName, setUserName] = useState(localStorage.getItem("username") || "");
  const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem("authToken"));
  const [statusMessage, setStatusMessage] = useState("");

  function handleLoginSuccess(username) {
    setUserName(username);
    setIsAuthenticated(true);
    setStatusMessage("");
  }

  function handleLogout() {
    localStorage.removeItem("authToken");
    localStorage.removeItem("username");
    localStorage.removeItem("userId");
    localStorage.removeItem("email");
    setIsAuthenticated(false);
    setUserName("");
    setStatusMessage("You have been signed out successfully.");
  }

  if (!isAuthenticated) {
    return <LoginPage onLoginSuccess={handleLoginSuccess} />;
  }

  return (
    <div className="app">
      <Header isLoggedIn={isAuthenticated} onLogout={handleLogout} userName={userName} />

      <section className="hero">
        <p className="eyebrow">Flight Management System</p>
        <h1>Hello, {userName}!</h1>
        <p className="lead">
          Full CRUD flight management — paginated, filterable, sortable.
          Powered by ASP.NET Core + PostgreSQL via .NET Aspire.
        </p>
        {statusMessage && <p className="status-message">{statusMessage}</p>}
      </section>

      <Dashboard userName={userName} />
      <Footer />
    </div>
  );
}

export default App;
