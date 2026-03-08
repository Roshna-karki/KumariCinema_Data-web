-- ========================================================================================================
-- KUMARI CINEMA - SCHEMA ALIGNMENT SCRIPT
-- Run this if your database has the actual schema below. Creates sequences for IDs.
-- NOTE: If sequences already exist, comment out or drop them first.
-- ========================================================================================================

-- Sequences (run as cinema user)
-- ========================================================================================================
CREATE SEQUENCE USER_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE MOVIE_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE THEATRE_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE HALL_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE SHOW_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE BOOKING_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE TICKET_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE PAYMENT_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE PRICE_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE SEAT_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE CANCEL_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;

-- ========================================================================================================
-- EXPECTED SCHEMA (your actual tables)
-- ========================================================================================================
-- UserTable: UserID, UserName, PhoneNumber, UserEmail, UserAddress
-- Payment: PaymentID, PaymentAmount, PaymentStatus, PaymentMode
-- Movie: MovieID, Title, Duration, Genre, Language, ReleaseDate
-- Theatre: TheatreID, TheatreName, City, TheatreAddress, TheatrePhoneNumber
-- Hall: HallID, HallName, HallCapacity, HallType, ScreenSize, TheatreID (FK)
-- Shows: ShowID, ShowDate, ShowTime, StartTime, EndTime (TIMESTAMP), MovieID, HallID (FK)
-- Seat: SeatID, SeatRow, SeatNumber, IsAvailable (NUMBER 1/0)
-- Price: PriceID, PriceAmount, IsFestiveSeason, IsReleasePeriod
-- Cancellation: CancellationID, CancellationTime, CancellationReason, CancellationStatus
-- Booking: BookingID, ConfirmationCode, ShowID, PaymentID, UserID (FK)
-- Ticket: TicketID, TicketStatus, TicketNumber, SeatID, CancellationID, PriceID, BookingID (FK)
-- ========================================================================================================
