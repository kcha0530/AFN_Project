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
          <p className="hero-eyebrow">Powered by .NET Aspire + SQL Server</p>
          <h1 className="hero-title">Where do you want to fly?</h1>
          <p className="hero-sub">Search hundreds of flights. Book in seconds. No account required.</p>
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
