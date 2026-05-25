import { useEffect, useState } from "react";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

function authHeaders() {
  const token = localStorage.getItem("authToken");
  return { "Content-Type": "application/json", ...(token ? { Authorization: `Bearer ${token}` } : {}) };
}

const STATUS_COLORS = {
  Scheduled: "status-scheduled",
  Departed:  "status-departed",
  Arrived:   "status-arrived",
  Cancelled: "status-cancelled",
  Delayed:   "status-delayed",
};

export default function FlightList() {
  const [flights, setFlights]   = useState([]);
  const [pagination, setPag]    = useState(null);
  const [loading, setLoading]   = useState(true);
  const [error, setError]       = useState("");
  const [page, setPage]         = useState(1);
  const [sortBy, setSortBy]     = useState("departureTime");
  const [sortDir, setSortDir]   = useState("asc");
  const [filters, setFilters]   = useState({ fromCity: "", toCity: "", status: "", cabinClass: "" });

  useEffect(() => { fetchFlights(); }, [page, sortBy, sortDir]);

  async function fetchFlights() {
    setLoading(true);
    setError("");
    try {
      const params = new URLSearchParams({
        page, pageSize: 8, sortBy, sortDir,
        ...(filters.fromCity   && { fromCity:   filters.fromCity }),
        ...(filters.toCity     && { toCity:     filters.toCity }),
        ...(filters.status     && { status:     filters.status }),
        ...(filters.cabinClass && { cabinClass: filters.cabinClass }),
      });
      const res  = await fetch(`${API_BASE_URL}/api/flights?${params}`, { headers: authHeaders() });
      const data = await res.json();
      if (!data.success) throw new Error(data.message || "Failed to load flights");
      setFlights(data.data || []);
      setPag(data.pagination);
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }

  function applyFilters(e) {
    e.preventDefault();
    setPage(1);
    fetchFlights();
  }

  function handleSort(field) {
    if (sortBy === field) setSortDir(d => d === "asc" ? "desc" : "asc");
    else { setSortBy(field); setSortDir("asc"); }
    setPage(1);
  }

  const SortIcon = ({ field }) => sortBy === field ? (sortDir === "asc" ? " ▲" : " ▼") : " ⇅";

  return (
    <div className="card">
      <h2>Flight Management</h2>
      <p className="card-subtitle">Live flight data from PostgreSQL via the API.</p>

      {/* Filters */}
      <form className="flight-filters" onSubmit={applyFilters}>
        <input placeholder="From city" value={filters.fromCity}   onChange={e => setFilters(f => ({ ...f, fromCity: e.target.value }))} />
        <input placeholder="To city"   value={filters.toCity}     onChange={e => setFilters(f => ({ ...f, toCity:   e.target.value }))} />
        <select value={filters.status} onChange={e => setFilters(f => ({ ...f, status: e.target.value }))}>
          <option value="">All statuses</option>
          {["Scheduled","Departed","Arrived","Cancelled","Delayed"].map(s => <option key={s}>{s}</option>)}
        </select>
        <select value={filters.cabinClass} onChange={e => setFilters(f => ({ ...f, cabinClass: e.target.value }))}>
          <option value="">All classes</option>
          {["Economy","Business","First"].map(c => <option key={c}>{c}</option>)}
        </select>
        <button type="submit">Filter</button>
        <button type="button" className="btn-ghost" onClick={() => { setFilters({ fromCity:"",toCity:"",status:"",cabinClass:"" }); setPage(1); setTimeout(fetchFlights, 0); }}>Clear</button>
      </form>

      {error && <div className="alert alert-error">{error}</div>}

      {loading ? (
        <div className="flight-loading">Loading flights…</div>
      ) : flights.length === 0 ? (
        <div className="alert alert-info">No flights found.</div>
      ) : (
        <>
          <div className="flight-table-wrap">
            <table className="flight-table">
              <thead>
                <tr>
                  <th onClick={() => handleSort("airline")}     className="sortable">Airline<SortIcon field="airline" /></th>
                  <th>Flight #</th>
                  <th onClick={() => handleSort("fromcity")}    className="sortable">From<SortIcon field="fromcity" /></th>
                  <th onClick={() => handleSort("tocity")}      className="sortable">To<SortIcon field="tocity" /></th>
                  <th onClick={() => handleSort("departureTime")} className="sortable">Departure<SortIcon field="departureTime" /></th>
                  <th onClick={() => handleSort("arrivaltime")} className="sortable">Arrival<SortIcon field="arrivaltime" /></th>
                  <th onClick={() => handleSort("price")}       className="sortable">Price<SortIcon field="price" /></th>
                  <th onClick={() => handleSort("availableseats")} className="sortable">Seats<SortIcon field="availableseats" /></th>
                  <th>Class</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {flights.map(f => (
                  <tr key={f.id}>
                    <td><strong>{f.airlineName}</strong></td>
                    <td className="mono">{f.flightNumber}</td>
                    <td>{f.fromCity}</td>
                    <td>{f.toCity}</td>
                    <td>{new Date(f.departureTime).toLocaleString("en-AU", { dateStyle:"short", timeStyle:"short" })}</td>
                    <td>{new Date(f.arrivalTime).toLocaleString("en-AU",   { dateStyle:"short", timeStyle:"short" })}</td>
                    <td><span className="flight-price">{f.currency} {f.price.toFixed(2)}</span></td>
                    <td><span className={f.availableSeats === 0 ? "seats-full" : "seats-avail"}>{f.availableSeats}/{f.totalSeats}</span></td>
                    <td>{f.cabinClass}</td>
                    <td><span className={`flight-status ${STATUS_COLORS[f.status] || ""}`}>{f.status}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {pagination && (
            <div className="pagination">
              <button onClick={() => setPage(p => Math.max(1, p-1))} disabled={page <= 1}>← Prev</button>
              <span>Page {pagination.page} of {pagination.totalPages} &nbsp;·&nbsp; {pagination.totalRecords} flights</span>
              <button onClick={() => setPage(p => p+1)} disabled={page >= pagination.totalPages}>Next →</button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
