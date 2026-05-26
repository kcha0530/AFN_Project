import { useState } from "react";
import Header from "./components/Header.jsx";
import Footer from "./components/Footer.jsx";
import HomePage from "./components/HomePage.jsx";
import LoginModal from "./components/LoginModal.jsx";

function App() {
  const [userName, setUserName] = useState(localStorage.getItem("username") || "");
  const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem("authToken"));
  const [showLogin, setShowLogin] = useState(false);

  function handleLoginSuccess(username) {
    setUserName(username);
    setIsAuthenticated(true);
    setShowLogin(false);
  }

  function handleLogout() {
    localStorage.removeItem("authToken");
    localStorage.removeItem("username");
    localStorage.removeItem("userId");
    localStorage.removeItem("email");
    setIsAuthenticated(false);
    setUserName("");
  }

  return (
    <div className="app">
      <Header
        isLoggedIn={isAuthenticated}
        onLogout={handleLogout}
        userName={userName}
        onLoginClick={() => setShowLogin(true)}
      />

      <HomePage isAuthenticated={isAuthenticated} userName={userName} />

      <Footer />

      {showLogin && (
        <LoginModal
          onLoginSuccess={handleLoginSuccess}
          onClose={() => setShowLogin(false)}
        />
      )}
    </div>
  );
}

export default App;
