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
      <p className="card-subtitle">Search products by name in real time.</p>

      <form onSubmit={handleSearch}>
        <div className="search-row">
          <input
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="e.g. Keyboard, Mouse…"
          />
          <button type="submit" disabled={loading}>
            {loading ? "…" : "Search"}
          </button>
        </div>
      </form>

      {searched && !loading && (
        results.length > 0 ? (
          <ul className="product-list">
            {results.map((p) => (
              <li key={p.id}>
                <span className="p-name">{p.name}</span>
                <span className="p-price">${p.price}</span>
              </li>
            ))}
          </ul>
        ) : (
          <div className="alert alert-info">No products found for &quot;{query}&quot;</div>
        )
      )}
    </div>
  );
}

export default ProductSearch;
