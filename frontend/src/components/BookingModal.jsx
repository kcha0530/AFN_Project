import { useState } from "react";

const API = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";

function fmtTime(dt) {
  return new Date(dt).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
}
function fmtDate(dt) {
  return new Date(dt).toLocaleDateString([], { weekday: "long", year: "numeric", month: "long", day: "numeric" });
}

function BookingModal({ flight, onClose }) {
  const [name, setName]   = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError]   = useState("");
  const [confirmed, setConfirmed] = useState(null);

  const passengers = flight.requestedPassengers || 1;
  const total = (flight.price * passengers).toFixed(2);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const res = await fetch(`${API}/api/bookings`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          flightId:      flight.id,
          passengerName: name.trim(),
          passengerEmail: email.trim(),
          passengerPhone: phone.trim() || null,
          passengers,
          cabinClass:    flight.cabinClass,
          userId: null,
        }),
      });
      const data = await res.json();
      if (data.success) {
        setConfirmed(data.data);
      } else {
        setError(data.message || "Booking failed.");
      }
    } catch {
      setError("Could not connect to the server.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="modal-backdrop" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal">
        {confirmed ? (
          <ConfirmationScreen booking={confirmed} onClose={onClose} />
        ) : (
          <>
            <div className="modal-header">
              <h2>Complete Booking</h2>
              <button className="modal-close" onClick={onClose}>✕</button>
            </div>

            {/* Flight summary */}
            <div className="modal-flight-summary">
              <div className="mfs-airline">{flight.airlineName} · {flight.flightNumber}</div>
              <div className="mfs-route">
                <span>{flight.fromCity}</span>
                <span className="mfs-arrow">→</span>
                <span>{flight.toCity}</span>
              </div>
              <div className="mfs-details">
                <span>{fmtDate(flight.departureTime)}</span>
                <span>{fmtTime(flight.departureTime)} → {fmtTime(flight.arrivalTime)}</span>
                <span>{flight.cabinClass}</span>
              </div>
              <div className="mfs-price">
                {flight.currency} {Number(flight.price).toLocaleString()} × {passengers}
                {" "}= <strong>{flight.currency} {Number(total).toLocaleString()}</strong>
              </div>
            </div>

            <form onSubmit={handleSubmit} className="modal-form">
              <div className="form-group">
                <label>Full name *</label>
                <input
                  type="text"
                  value={name}
                  onChange={e => setName(e.target.value)}
                  placeholder="John Smith"
                  required
                  autoFocus
                />
              </div>
              <div className="form-group">
                <label>Email address *</label>
                <input
                  type="email"
                  value={email}
                  onChange={e => setEmail(e.target.value)}
                  placeholder="john@example.com"
                  required
                />
              </div>
              <div className="form-group">
                <label>Phone number (optional)</label>
                <input
                  type="tel"
                  value={phone}
                  onChange={e => setPhone(e.target.value)}
                  placeholder="+61 400 000 000"
                />
              </div>

              {error && <p className="form-error">{error}</p>}

              <button type="submit" className="btn-confirm" disabled={loading}>
                {loading ? "Processing…" : `Confirm Booking · ${flight.currency} ${Number(total).toLocaleString()}`}
              </button>
            </form>
          </>
        )}
      </div>
    </div>
  );
}

function ConfirmationScreen({ booking, onClose }) {
  return (
    <div className="confirmation">
      <div className="confirmation-icon">✓</div>
      <h2>Booking Confirmed!</h2>
      <p className="conf-ref">Booking Reference</p>
      <p className="conf-code">{booking.bookingReference}</p>
      <div className="conf-details">
        <p><strong>{booking.airlineName}</strong> · {booking.flightNumber}</p>
        <p>{booking.fromCity} → {booking.toCity}</p>
        <p>{booking.passengerName}</p>
        <p>{booking.passengerEmail}</p>
        <p>{booking.cabinClass} · {booking.passengers} passenger{booking.passengers !== 1 ? "s" : ""}</p>
        <p className="conf-total">Total: {booking.totalPrice ? `AUD ${Number(booking.totalPrice).toLocaleString()}` : ""}</p>
      </div>
      <p className="conf-note">Save your booking reference. You can look it up anytime.</p>
      <button className="btn-confirm" onClick={onClose}>Done</button>
    </div>
  );
}

export default BookingModal;
