-- ============================================================
--  Flight Management System — SQL Server Query Reference
--  Database: demodb
-- ============================================================

-- ── CONNECTION (VS Code mssql) ────────────────────────────
-- Server:   localhost,1433
-- Database: demodb
-- User:     sa
-- Password: yourStrong(!)Password
-- Encrypt:  Trust Server Certificate

-- ============================================================
--  FLIGHTS
-- ============================================================

-- All active flights (not soft-deleted)
SELECT Id, AirlineName, FlightNumber, AircraftType,
       FromCity, ToCity,
       FORMAT(DepartureTime,'yyyy-MM-dd HH:mm') AS Departure,
       FORMAT(ArrivalTime,  'yyyy-MM-dd HH:mm') AS Arrival,
       DurationMinutes, Price, Currency,
       AvailableSeats, TotalSeats, Status, CabinClass
FROM   Flights
WHERE  IsDeleted = 0
ORDER  BY DepartureTime;

-- Flights with available seats
SELECT FlightNumber, AirlineName, FromCity, ToCity,
       DepartureTime, AvailableSeats, Price, CabinClass
FROM   Flights
WHERE  IsDeleted = 0
  AND  AvailableSeats > 0
  AND  Status != 'Cancelled'
ORDER  BY DepartureTime;

-- Search by origin or destination (case-insensitive)
DECLARE @city NVARCHAR(100) = 'Melbourne';
SELECT FlightNumber, AirlineName, FromCity, ToCity, DepartureTime, Price
FROM   Flights
WHERE  IsDeleted = 0
  AND  (FromCity LIKE '%' + @city + '%'
     OR ToCity   LIKE '%' + @city + '%');

-- Filter: origin → destination on a specific date
DECLARE @from NVARCHAR(100) = 'Melbourne';
DECLARE @to   NVARCHAR(100) = 'Bangkok';
DECLARE @date DATE           = '2026-05-28';
SELECT FlightNumber, AirlineName, DepartureTime, ArrivalTime,
       Price, AvailableSeats, CabinClass
FROM   Flights
WHERE  IsDeleted = 0
  AND  FromCity = @from
  AND  ToCity   = @to
  AND  CAST(DepartureTime AS DATE) = @date
ORDER  BY Price;

-- Cheapest flights per route
SELECT FromCity, ToCity,
       MIN(Price) AS LowestPrice,
       COUNT(*)   AS FlightCount
FROM   Flights
WHERE  IsDeleted = 0
  AND  Status != 'Cancelled'
GROUP  BY FromCity, ToCity
ORDER  BY LowestPrice;

-- Flights by cabin class
SELECT FlightNumber, AirlineName, FromCity, ToCity,
       CabinClass, Price, AvailableSeats
FROM   Flights
WHERE  IsDeleted = 0
  AND  CabinClass = 'Business'
ORDER  BY DepartureTime;

-- Full-text style search (airline, flight number, city)
DECLARE @q NVARCHAR(100) = 'Qantas';
SELECT FlightNumber, AirlineName, FromCity, ToCity, DepartureTime, Price
FROM   Flights
WHERE  IsDeleted = 0
  AND  (AirlineName  LIKE '%' + @q + '%'
     OR FlightNumber LIKE '%' + @q + '%'
     OR FromCity     LIKE '%' + @q + '%'
     OR ToCity       LIKE '%' + @q + '%');

-- Update flight status
UPDATE Flights
SET    Status = 'Delayed', UpdatedAt = GETUTCDATE()
WHERE  FlightNumber = 'QF401';

-- Soft delete a flight
UPDATE Flights
SET    IsDeleted = 1, UpdatedAt = GETUTCDATE()
WHERE  Id = 1;

-- ============================================================
--  BOOKINGS
-- ============================================================

-- All bookings with flight details
SELECT b.BookingReference, b.PassengerName, b.PassengerEmail,
       f.FlightNumber, f.AirlineName, f.FromCity, f.ToCity,
       f.DepartureTime, b.Passengers, b.CabinClass,
       b.TotalPrice, b.Status,
       FORMAT(b.CreatedAt,'yyyy-MM-dd HH:mm') AS BookedAt
FROM   Bookings b
JOIN   Flights  f ON b.FlightId = f.Id
ORDER  BY b.CreatedAt DESC;

-- Bookings for a specific passenger email
DECLARE @email NVARCHAR(255) = 'john@example.com';
SELECT b.BookingReference, f.FlightNumber, f.AirlineName,
       f.FromCity, f.ToCity, f.DepartureTime,
       b.Passengers, b.TotalPrice, b.Status
FROM   Bookings b
JOIN   Flights  f ON b.FlightId = f.Id
WHERE  b.PassengerEmail = @email
ORDER  BY b.CreatedAt DESC;

-- Look up one booking by reference
SELECT b.*, f.FlightNumber, f.AirlineName, f.FromCity, f.ToCity,
       f.DepartureTime, f.ArrivalTime
FROM   Bookings b
JOIN   Flights  f ON b.FlightId = f.Id
WHERE  b.BookingReference = 'AB12CD34';

-- Revenue per flight
SELECT f.FlightNumber, f.AirlineName, f.FromCity, f.ToCity,
       COUNT(b.Id)       AS TotalBookings,
       SUM(b.Passengers) AS PassengersSold,
       SUM(b.TotalPrice) AS TotalRevenue
FROM   Flights  f
LEFT   JOIN Bookings b ON b.FlightId = f.Id
WHERE  f.IsDeleted = 0
GROUP  BY f.Id, f.FlightNumber, f.AirlineName, f.FromCity, f.ToCity
ORDER  BY TotalRevenue DESC;

-- ============================================================
--  USERS
-- ============================================================

-- All active users
SELECT Id, Username, Email, FullName,
       FORMAT(CreatedAt,'yyyy-MM-dd') AS Joined,
       IsActive
FROM   Users
WHERE  IsActive = 1
ORDER  BY CreatedAt DESC;

-- Find user by username
SELECT Id, Username, Email, FullName, IsActive
FROM   Users
WHERE  Username = 'krit';

-- ============================================================
--  DASHBOARD STATS
-- ============================================================

SELECT
    (SELECT COUNT(*) FROM Users   WHERE IsActive = 1)               AS TotalUsers,
    (SELECT COUNT(*) FROM Flights WHERE IsDeleted = 0)              AS TotalFlights,
    (SELECT COUNT(*) FROM Flights WHERE IsDeleted = 0
                                  AND  Status != 'Cancelled')       AS ActiveFlights,
    (SELECT COUNT(*) FROM Flights WHERE IsDeleted = 0
                                  AND  AvailableSeats > 0)          AS FlightsWithSeats,
    (SELECT COUNT(*) FROM Bookings WHERE Status = 'Confirmed')      AS TotalBookings,
    (SELECT ISNULL(SUM(TotalPrice),0) FROM Bookings
                                      WHERE Status = 'Confirmed')   AS TotalRevenue;

-- ============================================================
--  ADMIN / MAINTENANCE
-- ============================================================

-- Seat occupancy rate per flight
SELECT FlightNumber, AirlineName,
       TotalSeats,
       TotalSeats - AvailableSeats AS SeatsSold,
       AvailableSeats,
       CAST((TotalSeats - AvailableSeats) * 100.0 / TotalSeats AS DECIMAL(5,1)) AS OccupancyPct
FROM   Flights
WHERE  IsDeleted = 0
ORDER  BY OccupancyPct DESC;

-- Cancelled flights
SELECT FlightNumber, AirlineName, FromCity, ToCity, DepartureTime
FROM   Flights
WHERE  IsDeleted = 0
  AND  Status = 'Cancelled';

-- List all tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Row counts
SELECT 'Users'    AS [Table], COUNT(*) AS Rows FROM Users
UNION ALL
SELECT 'Flights'  AS [Table], COUNT(*) AS Rows FROM Flights
UNION ALL
SELECT 'Bookings' AS [Table], COUNT(*) AS Rows FROM Bookings;
