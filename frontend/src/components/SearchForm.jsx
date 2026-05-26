import { useState } from "react";

const CITIES = [
  "Bangkok", "Brisbane", "Dubai", "Kuala Lumpur", "London",
  "Melbourne", "Singapore", "Sydney", "Tokyo", "Bali",
  "Hong Kong", "Seoul", "Paris", "New York", "Los Angeles",
];

const CABIN_CLASSES = ["Economy", "Business", "First"];

function today() {
  return new Date().toISOString().slice(0, 10);
}

function SearchForm({ onSearch }) {
  const [from, setFrom]       = useState("");
  const [to, setTo]           = useState("");
  const [date, setDate]       = useState(today());
  const [passengers, setPassengers] = useState(1);
  const [cabin, setCabin]     = useState("Economy");

  function handleSubmit(e) {
    e.preventDefault();
    onSearch({ from, to, date, passengers, cabin });
  }

  function handleSwap() {
    setFrom(to);
    setTo(from);
  }

  return (
    <form className="search-form" onSubmit={handleSubmit}>
      <div className="search-row">

        <div className="search-field">
          <label className="search-label">From</label>
          <select value={from} onChange={e => setFrom(e.target.value)} required className="search-select">
            <option value="">Select origin</option>
            {CITIES.map(c => <option key={c} value={c}>{c}</option>)}
          </select>
        </div>

        <button type="button" className="swap-btn" onClick={handleSwap} title="Swap cities">⇄</button>

        <div className="search-field">
          <label className="search-label">To</label>
          <select value={to} onChange={e => setTo(e.target.value)} required className="search-select">
            <option value="">Select destination</option>
            {CITIES.map(c => <option key={c} value={c}>{c}</option>)}
          </select>
        </div>

        <div className="search-field">
          <label className="search-label">Date</label>
          <input
            type="date"
            className="search-input"
            value={date}
            min={today()}
            onChange={e => setDate(e.target.value)}
            required
          />
        </div>

        <div className="search-field search-field-sm">
          <label className="search-label">Passengers</label>
          <select value={passengers} onChange={e => setPassengers(Number(e.target.value))} className="search-select">
            {[1,2,3,4,5,6,7,8,9].map(n => <option key={n} value={n}>{n}</option>)}
          </select>
        </div>

        <div className="search-field search-field-sm">
          <label className="search-label">Cabin</label>
          <select value={cabin} onChange={e => setCabin(e.target.value)} className="search-select">
            {CABIN_CLASSES.map(c => <option key={c} value={c}>{c}</option>)}
          </select>
        </div>

        <button type="submit" className="search-btn">Search Flights</button>
      </div>
    </form>
  );
}

export default SearchForm;
