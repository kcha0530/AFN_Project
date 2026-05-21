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
    setIsAuthenticated(false);
    setUserName("");
    setStatusMessage("You have been logged out. Please sign in again to access the dashboard.");
  }

  return (
    <div className="app">
      <Header isLoggedIn={isAuthenticated} onLogout={handleLogout} />

      <section className="hero">
        <div>
          <p className="eyebrow">{isAuthenticated ? "Welcome back" : "Secure login required"}</p>
          <h1>{isAuthenticated ? `Hello, ${userName}!` : "Please login to access the dashboard"}</h1>
          <p className="lead">
            {isAuthenticated
              ? "You now have access to the protected frontend dashboard components and secure backend products."
              : "Login first with your credentials to continue. Unauthorized visitors cannot access the dashboard."}
          </p>
          {statusMessage && <p className="status-message">{statusMessage}</p>}
        </div>
      </section>

      {isAuthenticated ? (
        <Dashboard userName={userName} />
      ) : (
        <section className="grid two-column" id="login">
          <LoginPage onLoginSuccess={handleLoginSuccess} />
          <div className="card login-info-card">
            <h2>Login First</h2>
            <p>After you sign in, the secure dashboard opens with these build tasks:</p>
            <ul>
              <li>Counter component with +, - and Reset</li>
              <li>Toggle component with dynamic text</li>
              <li>List builder with input and add button</li>
              <li>Like button with a heart icon and count</li>
              <li>GitHub profile card</li>
              <li>Random joke generator</li>
              <li>Protected backend products</li>
            </ul>
          </div>
        </section>
      )}

      <Footer />
    </div>
  );
}

export default App;

// function Header() {
//   return <h1>Welcome to My React App</h1>;
// }

// function Profile() {
//   return (
//     <div className="profile">
//       <h2>My Profile</h2>
//       <img src="https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRVA_HrQLjkHiJ2Ag5RGuwbFeDKRLfldnDasw&s" alt="Profile" />
//       <p>Name: Krit Chaiyabud</p>
//       <p>Bio: This is my first React Profile.</p>
//     </div>
//   );
// }

// function Card() {
//   return (
//     <div className="card">
//       <h2>React Card</h2>
//       <p>
//         This is a simple card component with a title, description, and button.
//       </p>
//       <button>Read More</button>
//     </div>
//   );
// }

// function Footer() {
//   return <p>© 2026 All Rights Reserved</p>;
// }

// function App() {
//   return (
//     <div className="app">
//       <Header />
//       <Profile />
//       <Card />
//       <Footer />
//     </div>
//   );
// }

// export default App;
