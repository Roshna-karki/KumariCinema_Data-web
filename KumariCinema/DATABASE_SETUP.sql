-- ========================================================================================================
-- KUMARI CINEMA MANAGEMENT SYSTEM - DATABASE SETUP SCRIPT
-- Oracle Database 21c XE
-- ========================================================================================================
-- IMPORTANT: Execute this script as SYSTEM user or DBA
-- ========================================================================================================

-- Step 1: Create Application User (Execute as SYSTEM)
-- ========================================================================================================

CREATE USER cinema IDENTIFIED BY 1234;
GRANT CONNECT, RESOURCE TO cinema;
GRANT CREATE VIEW, CREATE SEQUENCE TO cinema;
GRANT UNLIMITED TABLESPACE TO cinema;
COMMIT;

-- Step 2: Switch to Cinema User
-- ========================================================================================================

CONNECT cinema/1234@localhost:1522/XEPDB1

-- Step 3: Create Sequences (Auto-increment for IDs)
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

COMMIT;

-- Step 4: Create Tables
-- ========================================================================================================

-- USERS TABLE
CREATE TABLE Users (
    UserID INT PRIMARY KEY,
    UserName VARCHAR2(100) NOT NULL,
    Email VARCHAR2(100),
    ContactNumber VARCHAR2(15),
    Address VARCHAR2(150),
    DateOfBirth DATE,
    Gender VARCHAR2(20),
    MembershipTier VARCHAR2(20)
);

-- THEATRE TABLE
CREATE TABLE Theatre (
    TheatreID INT PRIMARY KEY,
    TheatreName VARCHAR2(100) NOT NULL,
    TheatreCity VARCHAR2(50) NOT NULL,
    TheatreAddress VARCHAR2(150),
    TheatreContactNumber VARCHAR2(15),
    TheatreEmail VARCHAR2(100),
    ParkingCapacity INT
);

-- HALL TABLE
CREATE TABLE Hall (
    HallID INT PRIMARY KEY,
    HallName VARCHAR2(100) NOT NULL,
    HallCapacity INT NOT NULL,
    ExperienceType VARCHAR2(50),
    ScreenSize VARCHAR2(30),
    TheatreID INT,
    Has3D CHAR(1) DEFAULT 'N',
    FOREIGN KEY (TheatreID) REFERENCES Theatre(TheatreID) ON DELETE CASCADE
);

-- MOVIE TABLE
CREATE TABLE Movie (
    MovieID INT PRIMARY KEY,
    Title VARCHAR2(100) NOT NULL,
    Duration INT NOT NULL,
    Genre VARCHAR2(50),
    Language VARCHAR2(50),
    ReleaseDate DATE,
    Director VARCHAR2(100),
    Cast VARCHAR2(300),
    Rating VARCHAR2(10),
    Description VARCHAR2(500)
);

-- SHOWS TABLE
CREATE TABLE Shows (
    ShowID INT PRIMARY KEY,
    ShowDate DATE NOT NULL,
    ShowTime VARCHAR2(10) NOT NULL,
    StartTime VARCHAR2(10),
    EndTime VARCHAR2(10),
    MovieID INT,
    HallID INT,
    FOREIGN KEY (MovieID) REFERENCES Movie(MovieID) ON DELETE CASCADE,
    FOREIGN KEY (HallID) REFERENCES Hall(HallID) ON DELETE CASCADE
);

-- PRICE TABLE
CREATE TABLE Price (
    PriceID INT PRIMARY KEY,
    Amount DECIMAL(10,2) NOT NULL,
    PriceCategory VARCHAR2(50),
    IsFestiveSeason CHAR(1),
    IsReleaseWeek CHAR(1),
    EffectiveDate DATE,
    ExpiryDate DATE,
    DiscountPercentage INT
);

-- PAYMENT TABLE
CREATE TABLE Payment (
    PaymentID INT PRIMARY KEY,
    PaymentAmount NUMBER(10, 2),
    PaymentStatus VARCHAR2(50),
    PaymentMethod VARCHAR2(50),
    PaymentDate DATE DEFAULT SYSDATE,
    TransactionID VARCHAR2(100),
    PaymentGateway VARCHAR2(50)
);

-- BOOKING TABLE
CREATE TABLE Booking (
    BookingID INT PRIMARY KEY,
    ConfirmationNumber VARCHAR2(50),
    BookingTime DATE,
    ReservationDate DATE,
    ShowID INT,
    UserID INT,
    PaymentID INT,
    BookingStatus VARCHAR2(20),
    TotalAmount DECIMAL(10,2),
    NumberOfSeats INT,
    CONSTRAINT fk_booking_show FOREIGN KEY (ShowID) REFERENCES Shows(ShowID) ON DELETE CASCADE,
    CONSTRAINT fk_booking_user FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    CONSTRAINT fk_booking_payment FOREIGN KEY (PaymentID) REFERENCES Payment(PaymentID)
);

-- SEAT TABLE
CREATE TABLE Seat (
    SeatID INT PRIMARY KEY,
    SeatRow VARCHAR2(5),
    SeatNumber INT,
    SeatCategory VARCHAR2(30),
    HallID INT,
    IsAvailable CHAR(1) DEFAULT 'Y',
    SeatPrice DECIMAL(10,2),
    IsBlocked CHAR(1) DEFAULT 'N',
    FOREIGN KEY (HallID) REFERENCES Hall(HallID) ON DELETE CASCADE
);

-- CANCELLATION TABLE
CREATE TABLE Cancellation (
    CancellationID INT PRIMARY KEY,
    CancellationTime TIMESTAMP,
    CancellationReason VARCHAR2(200),
    CancellationStatus VARCHAR2(50),
    RefundAmount DECIMAL(10,2),
    RefundStatus VARCHAR2(50),
    CancelledBy VARCHAR2(100),
    RefundMethod VARCHAR2(50)
);

-- TICKET TABLE
CREATE TABLE Ticket (
    TicketID INT PRIMARY KEY,
    TicketStatus VARCHAR2(50),
    TicketNumber VARCHAR2(50),
    BookingID INT,
    SeatID INT,
    CancellationID INT,
    PriceID INT,
    QRCode VARCHAR2(200),
    CheckInTime TIMESTAMP,
    IsScanned CHAR(1) DEFAULT 'N',
    CONSTRAINT fk_ticket_booking FOREIGN KEY (BookingID) REFERENCES Booking(BookingID) ON DELETE CASCADE,
    CONSTRAINT fk_ticket_seat FOREIGN KEY (SeatID) REFERENCES Seat(SeatID),
    CONSTRAINT fk_ticket_cancel FOREIGN KEY (CancellationID) REFERENCES Cancellation(CancellationID),
    CONSTRAINT fk_ticket_price FOREIGN KEY (PriceID) REFERENCES Price(PriceID)
);

COMMIT;

-- Step 5: Create Indexes (Performance Optimization)
-- ========================================================================================================

CREATE INDEX idx_movie_release_date ON Movie(ReleaseDate);
CREATE INDEX idx_shows_date ON Shows(ShowDate);
CREATE INDEX idx_booking_user ON Booking(UserID);
CREATE INDEX idx_booking_show ON Booking(ShowID);
CREATE INDEX idx_ticket_booking ON Ticket(BookingID);
CREATE INDEX idx_ticket_status ON Ticket(TicketStatus);
CREATE INDEX idx_payment_status ON Payment(PaymentStatus);
CREATE INDEX idx_hall_theatre ON Hall(TheatreID);

COMMIT;

-- Step 6: Insert Sample Data (Optional)
-- ========================================================================================================

-- Sample Theatres
INSERT INTO Theatre VALUES (THEATRE_SEQ.NEXTVAL, 'Kumari Cinema Kathmandu', 'Kathmandu', 'New Road', '9851234567', 'kathmandu@kumari.com', 150);
INSERT INTO Theatre VALUES (THEATRE_SEQ.NEXTVAL, 'Kumari Cinema Pokhara', 'Pokhara', 'Lakeside', '9851234568', 'pokhara@kumari.com', 100);
INSERT INTO Theatre VALUES (THEATRE_SEQ.NEXTVAL, 'Kumari Cinema Lalitpur', 'Lalitpur', 'Pulchowk', '9851234569', 'lalitpur@kumari.com', 120);

-- Sample Halls
INSERT INTO Hall VALUES (HALL_SEQ.NEXTVAL, 'Hall A - IMAX', 250, 'IMAX', 'Large', 1, 'Y');
INSERT INTO Hall VALUES (HALL_SEQ.NEXTVAL, 'Hall B - 4DX', 200, '4DX', 'Large', 1, 'N');
INSERT INTO Hall VALUES (HALL_SEQ.NEXTVAL, 'Hall C - Standard', 150, 'Standard', 'Medium', 1, 'N');
INSERT INTO Hall VALUES (HALL_SEQ.NEXTVAL, 'Hall A - Dolby Atmos', 220, 'Dolby Atmos', 'Large', 2, 'Y');
INSERT INTO Hall VALUES (HALL_SEQ.NEXTVAL, 'Hall B - Standard', 160, 'Standard', 'Medium', 2, 'N');
INSERT INTO Hall VALUES (HALL_SEQ.NEXTVAL, 'Hall A - Standard', 140, 'Standard', 'Medium', 3, 'N');

-- Sample Movies
INSERT INTO Movie VALUES (MOVIE_SEQ.NEXTVAL, 'Avatar 3', 180, 'Sci-Fi', 'English', TO_DATE('2024-12-15', 'YYYY-MM-DD'), 'James Cameron', 'Sam Worthington, Zoe Saldana', 'UA', 'Epic sci-fi adventure');
INSERT INTO Movie VALUES (MOVIE_SEQ.NEXTVAL, 'Inception', 148, 'Thriller', 'English', TO_DATE('2024-11-20', 'YYYY-MM-DD'), 'Christopher Nolan', 'Leonardo DiCaprio', 'A', 'Mind-bending thriller');
INSERT INTO Movie VALUES (MOVIE_SEQ.NEXTVAL, 'Pushpa 2', 187, 'Action', 'Telugu', TO_DATE('2024-12-05', 'YYYY-MM-DD'), 'Sukumar', 'Allu Arjun, Rashmika Mandanna', 'A', 'Action drama');
INSERT INTO Movie VALUES (MOVIE_SEQ.NEXTVAL, 'Dilwale Dulhania Le Jayenge', 189, 'Romance', 'Hindi', TO_DATE('2024-11-15', 'YYYY-MM-DD'), 'Aditya Chopra', 'Shah Rukh Khan, Kajol', 'U', 'Romantic classic');
INSERT INTO Movie VALUES (MOVIE_SEQ.NEXTVAL, '3 Idiots', 170, 'Comedy', 'Hindi', TO_DATE('2024-10-20', 'YYYY-MM-DD'), 'Rajkumar Hirani', 'Aamir Khan, Madhavan', 'U', 'Educational comedy');

-- Sample Customers
INSERT INTO Users VALUES (USER_SEQ.NEXTVAL, 'Raj Kumar', 'raj@email.com', '9841234567', 'Kathmandu', TO_DATE('1990-05-15', 'YYYY-MM-DD'), 'Male', 'Gold');
INSERT INTO Users VALUES (USER_SEQ.NEXTVAL, 'Priya Singh', 'priya@email.com', '9841234568', 'Pokhara', TO_DATE('1992-08-20', 'YYYY-MM-DD'), 'Female', 'Silver');
INSERT INTO Users VALUES (USER_SEQ.NEXTVAL, 'Amit Patel', 'amit@email.com', '9841234569', 'Lalitpur', TO_DATE('1988-03-10', 'YYYY-MM-DD'), 'Male', 'Standard');
INSERT INTO Users VALUES (USER_SEQ.NEXTVAL, 'Deepa Gupta', 'deepa@email.com', '9841234570', 'Kathmandu', TO_DATE('1995-11-25', 'YYYY-MM-DD'), 'Female', 'Platinum');
INSERT INTO Users VALUES (USER_SEQ.NEXTVAL, 'Vikram Sharma', 'vikram@email.com', '9841234571', 'Pokhara', TO_DATE('1991-07-30', 'YYYY-MM-DD'), 'Male', 'Gold');

-- Sample Prices
INSERT INTO Price VALUES (PRICE_SEQ.NEXTVAL, 250, 'Regular', 'N', 'N', SYSDATE, NULL, 0);
INSERT INTO Price VALUES (PRICE_SEQ.NEXTVAL, 350, 'Premium', 'N', 'N', SYSDATE, NULL, 0);
INSERT INTO Price VALUES (PRICE_SEQ.NEXTVAL, 500, 'VIP', 'N', 'N', SYSDATE, NULL, 0);
INSERT INTO Price VALUES (PRICE_SEQ.NEXTVAL, 280, 'Weekend', 'N', 'N', SYSDATE, NULL, 0);

-- Sample Shows
INSERT INTO Shows VALUES (SHOW_SEQ.NEXTVAL, TRUNC(SYSDATE) + 1, '09:00', '09:00', '11:30', 1, 1);
INSERT INTO Shows VALUES (SHOW_SEQ.NEXTVAL, TRUNC(SYSDATE) + 1, '12:00', '12:00', '14:30', 2, 2);
INSERT INTO Shows VALUES (SHOW_SEQ.NEXTVAL, TRUNC(SYSDATE) + 1, '15:00', '15:00', '17:30', 3, 3);
INSERT INTO Shows VALUES (SHOW_SEQ.NEXTVAL, TRUNC(SYSDATE) + 1, '18:00', '18:00', '20:30', 4, 4);
INSERT INTO Shows VALUES (SHOW_SEQ.NEXTVAL, TRUNC(SYSDATE) + 1, '21:00', '21:00', '23:30', 5, 5);

-- Sample Payments
INSERT INTO Payment VALUES (PAYMENT_SEQ.NEXTVAL, 500, 'COMPLETED', 'Credit Card', SYSDATE, 'TXN001', 'Stripe');
INSERT INTO Payment VALUES (PAYMENT_SEQ.NEXTVAL, 750, 'COMPLETED', 'UPI', SYSDATE, 'TXN002', 'PayU');
INSERT INTO Payment VALUES (PAYMENT_SEQ.NEXTVAL, 400, 'PENDING', 'Net Banking', SYSDATE, 'TXN003', 'Bank');
INSERT INTO Payment VALUES (PAYMENT_SEQ.NEXTVAL, 600, 'COMPLETED', 'Debit Card', SYSDATE, 'TXN004', 'Stripe');
INSERT INTO Payment VALUES (PAYMENT_SEQ.NEXTVAL, 300, 'FAILED', 'Credit Card', SYSDATE, 'TXN005', 'Stripe');

-- Sample Bookings
INSERT INTO Booking VALUES (BOOKING_SEQ.NEXTVAL, 'BOOK-2024-0001', SYSDATE, TRUNC(SYSDATE) + 1, 1, 1, 1, 'CONFIRMED', 1000, 2);
INSERT INTO Booking VALUES (BOOKING_SEQ.NEXTVAL, 'BOOK-2024-0002', SYSDATE, TRUNC(SYSDATE) + 1, 2, 2, 2, 'CONFIRMED', 1500, 3);
INSERT INTO Booking VALUES (BOOKING_SEQ.NEXTVAL, 'BOOK-2024-0003', SYSDATE, TRUNC(SYSDATE) + 1, 3, 3, 3, 'PENDING', 800, 2);
INSERT INTO Booking VALUES (BOOKING_SEQ.NEXTVAL, 'BOOK-2024-0004', SYSDATE, TRUNC(SYSDATE) + 1, 4, 4, 4, 'CONFIRMED', 1200, 2);
INSERT INTO Booking VALUES (BOOKING_SEQ.NEXTVAL, 'BOOK-2024-0005', SYSDATE, TRUNC(SYSDATE) + 1, 5, 5, 5, 'CONFIRMED', 600, 2);

-- Sample Seats
INSERT INTO Seat VALUES (SEAT_SEQ.NEXTVAL, 'A', 1, 'Regular', 1, 'Y', 250, 'N');
INSERT INTO Seat VALUES (SEAT_SEQ.NEXTVAL, 'A', 2, 'Regular', 1, 'Y', 250, 'N');
INSERT INTO Seat VALUES (SEAT_SEQ.NEXTVAL, 'A', 3, 'Premium', 1, 'Y', 350, 'N');
INSERT INTO Seat VALUES (SEAT_SEQ.NEXTVAL, 'A', 4, 'Premium', 1, 'Y', 350, 'N');
INSERT INTO Seat VALUES (SEAT_SEQ.NEXTVAL, 'B', 1, 'Regular', 1, 'Y', 250, 'N');
INSERT INTO Seat VALUES (SEAT_SEQ.NEXTVAL, 'B', 2, 'Regular', 1, 'Y', 250, 'N');
INSERT INTO Seat VALUES (SEAT_SEQ.NEXTVAL, 'B', 3, 'Premium', 1, 'Y', 350, 'N');
INSERT INTO Seat VALUES (SEAT_SEQ.NEXTVAL, 'B', 4, 'Premium', 1, 'Y', 350, 'N');

-- Sample Tickets
INSERT INTO Ticket VALUES (TICKET_SEQ.NEXTVAL, 'CONFIRMED', 'TKT-2024-0001', 1, 1, NULL, 1, NULL, NULL, 'N');
INSERT INTO Ticket VALUES (TICKET_SEQ.NEXTVAL, 'CONFIRMED', 'TKT-2024-0002', 1, 2, NULL, 1, NULL, NULL, 'N');
INSERT INTO Ticket VALUES (TICKET_SEQ.NEXTVAL, 'CONFIRMED', 'TKT-2024-0003', 2, 3, NULL, 2, NULL, NULL, 'N');
INSERT INTO Ticket VALUES (TICKET_SEQ.NEXTVAL, 'CONFIRMED', 'TKT-2024-0004', 2, 4, NULL, 2, NULL, NULL, 'N');
INSERT INTO Ticket VALUES (TICKET_SEQ.NEXTVAL, 'CONFIRMED', 'TKT-2024-0005', 2, 5, NULL, 2, NULL, NULL, 'N');

COMMIT;

-- Step 7: Verify Data
-- ========================================================================================================

SELECT 'Users' AS Table_Name, COUNT(*) AS Record_Count FROM Users
UNION ALL
SELECT 'Theatre', COUNT(*) FROM Theatre
UNION ALL
SELECT 'Hall', COUNT(*) FROM Hall
UNION ALL
SELECT 'Movie', COUNT(*) FROM Movie
UNION ALL
SELECT 'Shows', COUNT(*) FROM Shows
UNION ALL
SELECT 'Booking', COUNT(*) FROM Booking
UNION ALL
SELECT 'Ticket', COUNT(*) FROM Ticket
UNION ALL
SELECT 'Payment', COUNT(*) FROM Payment
UNION ALL
SELECT 'Seat', COUNT(*) FROM Seat
UNION ALL
SELECT 'Price', COUNT(*) FROM Price
ORDER BY Table_Name;

-- ========================================================================================================
-- DATABASE SETUP COMPLETE
-- ========================================================================================================
-- Connection String: User Id=cinema;Password=1234;Data Source=localhost:1522/XEPDB1;
-- ========================================================================================================
