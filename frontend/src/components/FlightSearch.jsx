import { useState } from "react";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

export default function FlightSearch() {
  const [query, setQuery]     = useState("");
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searched, setSearched] = useState(false);

  async function handleSearch(e) {
    e.preventDefault();
    if (!query.trim()) return;
    setLoading(true);
    setSearched(true);
    try {
      const res  = await fetch(`${API_BASE_URL}/api/flights?search=${encodeURIComponent(query.trim())}&pageSize=20`);
      const data = await res.json();
      setResults(data.success ? (data.data || []) : []);
    } catch {
      setResults([]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="card">
      <h2>Flight Search</h2>
      <p className="card-subtitle">Search by airline, flight number, or city.</p>

      <form onSubmit={handleSearch}>
        <div className="search-row">
          <input
            type="text"
            value={query}
            onChange={e => setQuery(e.target.value)}
            placeholder="e.g. Qantas, QF401, Melbourne…"
          />
          <button type="submit" disabled={loading}>{loading ? "…" : "Search"}</button>
        </div>
      </form>

      {searched && !loading && (
        results.length > 0 ? (
          <ul className="flight-search-results">
            {results.map(f => (
              <li key={f.id}>
                <div className="fsr-main">
                  <span className="fsr-num">{f.flightNumber}</span>
                  <span className="fsr-airline">{f.airlineName}</span>
                  <span className="fsr-route">{f.fromCity} → {f.toCity}</span>
                </div>
                <div className="fsr-meta">
                  <span className="flight-price">{f.currency} {f.price.toFixed(2)}</span>
                  <span className={`flight-status status-${f.status?.toLowerCase()}`}>{f.status}</span>
                  <span>{f.availableSeats} seats left</span>
                </div>
              </li>
            ))}
          </ul>
        ) : (
          <div className="alert alert-info">No flights found for &quot;{query}&quot;</div>
        )
      )}
    </div>
  );
}
