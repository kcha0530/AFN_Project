import { useState } from "react";
import SearchForm from "./SearchForm.jsx";
import FlightResults from "./FlightResults.jsx";
import BookingModal from "./BookingModal.jsx";
import StatsBar from "./StatsBar.jsx";

function HomePage({ isAuthenticated, userName }) {
  const [searchParams, setSearchParams] = useState(null);
  const [selectedFlight, setSelectedFlight] = useState(null);

  return (
    <>
      {/* Hero */}
      <section className="hero-booking">
        <div className="hero-overlay" />
        <div className="hero-content">
          <p className="hero-eyebrow">Powered by AFN</p>
          <h1 className="hero-title">Let's fly?</h1>
          <p className="hero-sub">Search hundreds of flights. Book in seconds. Book Now!</p>
          <SearchForm onSearch={setSearchParams} />
        </div>
      </section>

      {/* Stats bar */}
      <StatsBar isAuthenticated={isAuthenticated} />

      {/* Results */}
      {searchParams && (
        <section className="results-section">
          <FlightResults
            searchParams={searchParams}
            onBook={setSelectedFlight}
          />
        </section>
      )}

      {/* Booking modal */}
      {selectedFlight && (
        <BookingModal
          flight={selectedFlight}
          onClose={() => setSelectedFlight(null)}
        />
      )}
    </>
  );
}

export default HomePage;
