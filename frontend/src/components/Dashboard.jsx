import { useEffect, useState } from "react";
import Counter from "./Counter.jsx";
import ToggleText from "./ToggleText.jsx";
import ItemList from "./ItemList.jsx";
import LikeButton from "./LikeButton.jsx";
import GitHubProfile from "./GitHubProfile.jsx";
import RandomJoke from "./RandomJoke.jsx";
import BackendProducts from "./BackendProducts.jsx";
import ProductSearch from "./ProductSearch.jsx";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

function Dashboard({ userName }) {
  const [stats, setStats] = useState(null);

  useEffect(() => {
    const token = localStorage.getItem("authToken");
    if (!token) return;
    fetch(`${API_BASE_URL}/dashboard/stats`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then((r) => r.ok ? r.json() : null)
      .then((data) => { if (data) setStats(data); })
      .catch(() => {});
  }, []);

  return (
    <main>
      {/* Welcome Banner */}
      <div className="card dashboard-welcome" style={{ marginBottom: "20px" }}>
        <h2>Welcome back, {userName}!</h2>
        <p>
          You are authenticated. Explore interactive components below and live
          PostgreSQL data served by your ASP.NET Core API.
        </p>
      </div>

      {/* Stats Row */}
      {stats && (
        <>
          <p className="section-label">Live backend stats</p>
          <div className="stats-row">
            <div className="stat-card blue">
              <span className="stat-label">Total Users</span>
              <span className="stat-value">{stats.totalUsers}</span>
              <span className="stat-sub">Registered accounts</span>
            </div>
            <div className="stat-card green">
              <span className="stat-label">Active Users</span>
              <span className="stat-value">{stats.activeUsers}</span>
              <span className="stat-sub">Currently active</span>
            </div>
            <div className="stat-card purple">
              <span className="stat-label">Products</span>
              <span className="stat-value">{stats.totalProducts}</span>
              <span className="stat-sub">In database</span>
            </div>
          </div>
        </>
      )}

      {/* Interactive Components */}
      <p className="section-label">Interactive components</p>
      <div className="grid two-column">
        <Counter />
        <ToggleText />
        <ItemList />
        <LikeButton />
      </div>

      {/* API Components */}
      <p className="section-label">External API demos</p>
      <div className="grid two-column">
        <GitHubProfile />
        <RandomJoke />
      </div>

      {/* Backend Data */}
      <p className="section-label">Backend data</p>
      <div className="grid two-column">
        <ProductSearch />
        <BackendProducts />
      </div>
    </main>
  );
}

export default Dashboard;
