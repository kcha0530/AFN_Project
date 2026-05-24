import { useEffect, useState } from "react";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

function BackendProducts() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [stats, setStats] = useState(null);
  const token = localStorage.getItem("authToken");

  useEffect(() => {
    fetchProducts();
    fetchDashboardStats();
  }, []);

  function fetchProducts() {
    setLoading(true);
    setError("");

    const headers = {
      "Content-Type": "application/json",
    };

    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    fetch(`${API_BASE_URL}/products`, {
      headers,
    })
      .then((res) => {
        if (!res.ok) {
          throw new Error(`Failed to load products (${res.status})`);
        }
        return res.json();
      })
      .then((data) => {
        setProducts(data);
      })
      .catch((err) => {
        setError(err.message || "Error fetching backend products");
      })
      .finally(() => {
        setLoading(false);
      });
  }

  function fetchDashboardStats() {
    const headers = {
      "Content-Type": "application/json",
    };

    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    fetch(`${API_BASE_URL}/dashboard/stats`, {
      headers,
    })
      .then((res) => {
        if (!res.ok) {
          throw new Error(`Failed to load stats`);
        }
        return res.json();
      })
      .then((data) => {
        setStats(data);
      })
      .catch((err) => {
        console.error("Error fetching stats:", err);
      });
  }

  return (
    <div className="card" id="backend-products">
      <h2>Backend Products & Dashboard</h2>
      <p className="card-subtitle">Secure data from your PostgreSQL database.</p>
      <button onClick={() => { fetchProducts(); fetchDashboardStats(); }}>
        Refresh
      </button>

      {stats && (
        <div className="stats-grid">
          <div className="stat-item">
            <strong>Total Users:</strong> {stats.totalUsers}
          </div>
          <div className="stat-item">
            <strong>Active Users:</strong> {stats.activeUsers}
          </div>
          <div className="stat-item">
            <strong>Total Products:</strong> {stats.totalProducts}
          </div>
        </div>
      )}

      {loading && <p>Loading products...</p>}
      {error && <p className="error">{error}</p>}

      {!loading && !error && (
        <ul className="product-list">
          {products.map((product) => (
            <li key={product.id}>
              <strong>{product.name}</strong> — ${product.price}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default BackendProducts;
