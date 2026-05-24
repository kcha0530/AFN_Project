import { useState } from "react";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

function ProductSearch() {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searched, setSearched] = useState(false);

  async function handleSearch(e) {
    e.preventDefault();
    if (!query.trim()) return;

    setLoading(true);
    setSearched(true);

    try {
      const res = await fetch(
        `${API_BASE_URL}/products/search?q=${encodeURIComponent(query.trim())}`
      );
      const data = await res.json();
      setResults(Array.isArray(data) ? data : []);
    } catch {
      setResults([]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="card">
      <h2>Product Search</h2>
      <p className="card-subtitle">Search products from the database.</p>

      <form onSubmit={handleSearch} className="login-form">
        <label>
          Search
          <input
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="e.g. Keyboard, Mouse..."
          />
        </label>
        <button type="submit" disabled={loading}>
          {loading ? "Searching..." : "Search"}
        </button>
      </form>

      {searched && !loading && (
        results.length > 0 ? (
          <ul className="product-list">
            {results.map((p) => (
              <li key={p.id}>
                <strong>{p.name}</strong> — ${p.price}
              </li>
            ))}
          </ul>
        ) : (
          <p className="error">No products found for &quot;{query}&quot;</p>
        )
      )}
    </div>
  );
}

export default ProductSearch;
