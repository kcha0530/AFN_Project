import { useEffect, useState } from "react";
import FlightList from "./FlightList.jsx";
import FlightSearch from "./FlightSearch.jsx";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

function Dashboard({ userName }) {
  const [stats, setStats] = useState(null);

  useEffect(() => {
    const token = localStorage.getItem("authToken");
    if (!token) return;
    fetch(`${API_BASE_URL}/dashboard/stats`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(r => r.ok ? r.json() : null)
      .then(data => { if (data?.success) setStats(data.data); })
      .catch(() => {});
  }, []);

  return (
    <main>
      {/* Welcome Banner */}
      <div className="card dashboard-welcome">
        <h2>Welcome back, {userName}!</h2>
        <p>Flight Management System — authenticated dashboard powered by .NET Aspire + ASP.NET Core + PostgreSQL.</p>
      </div>

      {/* Live Stats */}
      {stats && (
        <>
          <p className="section-label">Live backend stats</p>
          <div className="stats-row">
            <div className="stat-card blue">
              <span className="stat-label">Total Flights</span>
              <span className="stat-value">{stats.totalFlights}</span>
              <span className="stat-sub">In database</span>
            </div>
            <div className="stat-card green">
              <span className="stat-label">Active Flights</span>
              <span className="stat-value">{stats.activeFlights}</span>
              <span className="stat-sub">Not cancelled</span>
            </div>
            <div className="stat-card purple">
              <span className="stat-label">With Seats</span>
              <span className="stat-value">{stats.availableFlights}</span>
              <span className="stat-sub">Seats available</span>
            </div>
            <div className="stat-card orange">
              <span className="stat-label">Total Users</span>
              <span className="stat-value">{stats.totalUsers}</span>
              <span className="stat-sub">Registered</span>
            </div>
          </div>
        </>
      )}

      {/* Flight Search */}
      <p className="section-label">Search flights</p>
      <FlightSearch />

      {/* Flight Table */}
      <p className="section-label">All flights</p>
      <FlightList />
    </main>
  );
}

export default Dashboard;
