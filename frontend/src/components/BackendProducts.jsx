import { useEffect, useState } from "react";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

function BackendProducts() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [productStats, setProductStats] = useState(null);

  useEffect(() => {
    fetchAll();
  }, []);

  function authHeaders() {
    const token = localStorage.getItem("authToken");
    return {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    };
  }

  async function fetchAll() {
    setLoading(true);
    setError("");
    try {
      const [prodRes, statsRes] = await Promise.all([
        fetch(`${API_BASE_URL}/products`, { headers: authHeaders() }),
        fetch(`${API_BASE_URL}/products/stats`, { headers: authHeaders() }),
      ]);

      if (!prodRes.ok) throw new Error(`Products error (${prodRes.status})`);
      const prodData = await prodRes.json();
      setProducts(prodData);

      if (statsRes.ok) {
        const statsData = await statsRes.json();
        setProductStats(statsData);
      }
    } catch (err) {
      setError(err.message || "Failed to load products");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="card">
      <h2>Products from Database</h2>
      <p className="card-subtitle">Live data from your PostgreSQL database via the API.</p>

      <button className="btn-sm" onClick={fetchAll} disabled={loading}>
        {loading ? "Loading…" : "↻ Refresh"}
      </button>

      {error && <div className="alert alert-error" style={{ marginTop: 12 }}>{error}</div>}

      {productStats && (
        <div className="product-stats">
          <div className="stat-box">
            <div className="sb-val">{productStats.totalCount}</div>
            <div className="sb-label">Items</div>
          </div>
          <div className="stat-box">
            <div className="sb-val">${productStats.minPrice}</div>
            <div className="sb-label">Min</div>
          </div>
          <div className="stat-box">
            <div className="sb-val">${productStats.maxPrice}</div>
            <div className="sb-label">Max</div>
          </div>
        </div>
      )}

      {!loading && !error && (
        <ul className="product-list">
          {products.map((product) => (
            <li key={product.id}>
              <span className="p-name">{product.name}</span>
              <span className="p-price">${product.price}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default BackendProducts;
