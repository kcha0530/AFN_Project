import { useEffect, useState } from "react";

const API = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

function fmtTime(dt) {
  return new Date(dt).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
}
function fmtDate(dt) {
  return new Date(dt).toLocaleDateString([], { weekday: "short", month: "short", day: "numeric" });
}
function fmtDuration(mins) {
  const h = Math.floor(mins / 60);
  const m = mins % 60;
  return h > 0 ? `${h}h ${m}m` : `${m}m`;
}

function FlightResults({ searchParams, onBook }) {
  const [flights, setFlights] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError]     = useState("");
  const [sortBy, setSortBy]   = useState("price");

  useEffect(() => {
    if (!searchParams) return;
    setLoading(true);
    setError("");

    const params = new URLSearchParams({
      fromCity: searchParams.from || "",
      toCity:   searchParams.to   || "",
      pageSize: 50,
    });
    if (searchParams.cabin) params.set("cabinClass", searchParams.cabin);

    fetch(`${API}/api/flights?${params}`)
      .then(r => r.json())
      .then(data => {
        if (data.success) setFlights(data.data || []);
        else setError(data.message || "Could not load flights.");
      })
      .catch(() => setError("Could not connect to the server."))
      .finally(() => setLoading(false));
  }, [searchParams]);

  const sorted = [...flights].sort((a, b) => {
    if (sortBy === "price")     return a.price - b.price;
    if (sortBy === "duration")  return a.durationMinutes - b.durationMinutes;
    if (sortBy === "departure") return new Date(a.departureTime) - new Date(b.departureTime);
    return 0;
  });

  if (loading) return (
    <div className="results-loading">
      <div className="spinner" />
      <p>Searching flights…</p>
    </div>
  );

  if (error) return <div className="results-error">{error}</div>;

  if (flights.length === 0) return (
    <div className="results-empty">
      <p>No flights found for <strong>{searchParams.from} → {searchParams.to}</strong>.</p>
      <p className="results-hint">Try different cities or a different date.</p>
    </div>
  );

  return (
    <div className="results-wrap">
      <div className="results-header">
        <h2 className="results-title">
          {searchParams.from} → {searchParams.to}
          <span className="results-count">{flights.length} flight{flights.length !== 1 ? "s" : ""}</span>
        </h2>
        <div className="results-sort">
          <span>Sort by:</span>
          {["price","duration","departure"].map(s => (
            <button
              key={s}
              className={`sort-chip${sortBy === s ? " active" : ""}`}
              onClick={() => setSortBy(s)}
            >
              {s.charAt(0).toUpperCase() + s.slice(1)}
            </button>
          ))}
        </div>
      </div>

      <div className="flight-cards">
        {sorted.map(flight => (
          <FlightCard
            key={flight.id}
            flight={flight}
            passengers={searchParams.passengers}
            onBook={() => onBook({ ...flight, requestedPassengers: searchParams.passengers })}
          />
        ))}
      </div>
    </div>
  );
}

function FlightCard({ flight, passengers, onBook }) {
  const total = (flight.price * passengers).toFixed(2);
  const noSeats = flight.availableSeats < passengers;

  return (
    <div className={`flight-card${noSeats ? " flight-card--full" : ""}`}>
      <div className="fc-airline">
        <div className="fc-airline-dot" style={{ background: airlineColor(flight.airlineName) }} />
        <div>
          <p className="fc-airline-name">{flight.airlineName}</p>
          <p className="fc-flight-num">{flight.flightNumber} · {flight.aircraftType || "Aircraft"}</p>
        </div>
      </div>

      <div className="fc-route">
        <div className="fc-city">
          <p className="fc-time">{fmtTime(flight.departureTime)}</p>
          <p className="fc-city-name">{flight.fromCity}</p>
          <p className="fc-date">{fmtDate(flight.departureTime)}</p>
        </div>
        <div className="fc-line">
          <span className="fc-duration">{fmtDuration(flight.durationMinutes)}</span>
          <div className="fc-arrow">──────✈──────</div>
          <span className="fc-stops">Direct</span>
        </div>
        <div className="fc-city fc-city--right">
          <p className="fc-time">{fmtTime(flight.arrivalTime)}</p>
          <p className="fc-city-name">{flight.toCity}</p>
          <p className="fc-date">{fmtDate(flight.arrivalTime)}</p>
        </div>
      </div>

      <div className="fc-meta">
        <span className={`fc-badge fc-badge--${flight.cabinClass?.toLowerCase()}`}>{flight.cabinClass}</span>
        <span className={`fc-badge fc-badge--${flight.status?.toLowerCase()}`}>{flight.status}</span>
        {flight.isRefundable && <span className="fc-badge fc-badge--refund">Refundable</span>}
      </div>

      <div className="fc-price-col">
        <p className="fc-price">{flight.currency} {Number(flight.price).toLocaleString()}</p>
        <p className="fc-price-note">per person</p>
        {passengers > 1 && <p className="fc-total">{flight.currency} {Number(total).toLocaleString()} total</p>}
        <p className="fc-seats">{flight.availableSeats} seat{flight.availableSeats !== 1 ? "s" : ""} left</p>
        <button
          className="fc-book-btn"
          onClick={onBook}
          disabled={noSeats}
        >
          {noSeats ? "Sold out" : "Select"}
        </button>
      </div>
    </div>
  );
}

function airlineColor(name) {
  const map = {
    "Qantas": "#E8112D",
    "Singapore Airlines": "#E6001F",
    "Emirates": "#C60C30",
    "Jetstar": "#FF6600",
    "Virgin Australia": "#C4122F",
    "Thai Airways": "#6A0DAD",
    "Air Asia": "#FF0000",
  };
  return map[name] || "#2563eb";
}

export default FlightResults;
