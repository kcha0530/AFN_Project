import { useEffect, useState } from "react";

const API = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

function StatsBar({ isAuthenticated }) {
  const [stats, setStats] = useState(null);

  useEffect(() => {
    if (!isAuthenticated) return;
    const token = localStorage.getItem("authToken");
    if (!token) return;
    fetch(`${API}/dashboard/stats`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(r => r.ok ? r.json() : null)
      .then(data => { if (data?.success) setStats(data.data); })
      .catch(() => {});
  }, [isAuthenticated]);

  if (!stats) return null;

  return (
    <section className="stats-bar">
      <div className="stats-bar-inner">
        <StatItem label="Total Flights" value={stats.totalFlights} />
        <StatItem label="Active Flights" value={stats.activeFlights} />
        <StatItem label="Seats Available" value={stats.availableFlights} />
        <StatItem label="Registered Users" value={stats.totalUsers} />
      </div>
    </section>
  );
}

function StatItem({ label, value }) {
  return (
    <div className="stats-bar-item">
      <span className="stats-bar-value">{value}</span>
      <span className="stats-bar-label">{label}</span>
    </div>
  );
}

export default StatsBar;
