-- ========================================================================================================
-- KUMARI CINEMA - CREATE TABLES (exact names for your app)
-- Run as the SAME user as your connection string (e.g. cinema/1234)
-- This avoids ORA-00942: table or view does not exist
-- ========================================================================================================

-- Sequences
CREATE SEQUENCE USER_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE PAYMENT_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE MOVIE_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE THEATRE_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE HALL_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE SHOW_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE SEAT_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE PRICE_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE CANCEL_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE BOOKING_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;
CREATE SEQUENCE TICKET_SEQ START WITH 1 INCREMENT BY 1 NOCYCLE;

-- Tables (do not use double-quotes so Oracle stores uppercase: USERTABLE, BOOKING, etc.)
CREATE TABLE UserTable (
    UserID NUMBER PRIMARY KEY,
    UserName VARCHAR2(100),
    PhoneNumber VARCHAR2(20),
    UserEmail VARCHAR2(100),
    UserAddress VARCHAR2(200)
);
CREATE TABLE Payment (
    PaymentID NUMBER PRIMARY KEY,
    PaymentAmount NUMBER(10,2),
    PaymentStatus VARCHAR2(50),
    PaymentMode VARCHAR2(50)
);
CREATE TABLE Movie (
    MovieID NUMBER PRIMARY KEY,
    Title VARCHAR2(100),
    Duration NUMBER,
    Genre VARCHAR2(50),
    Language VARCHAR2(50),
    ReleaseDate DATE
);
CREATE TABLE Theatre (
    TheatreID NUMBER PRIMARY KEY,
    TheatreName VARCHAR2(100),
    City VARCHAR2(50),
    TheatreAddress VARCHAR2(200),
    TheatrePhoneNumber VARCHAR2(20)
);
CREATE TABLE Hall (
    HallID NUMBER PRIMARY KEY,
    HallName VARCHAR2(50),
    HallCapacity NUMBER,
    HallType VARCHAR2(50),
    ScreenSize VARCHAR2(50),
    TheatreID NUMBER,
    FOREIGN KEY (TheatreID) REFERENCES Theatre(TheatreID)
);
CREATE TABLE Shows (
    ShowID NUMBER PRIMARY KEY,
    ShowDate DATE,
    ShowTime TIMESTAMP,
    StartTime TIMESTAMP,
    EndTime TIMESTAMP,
    MovieID NUMBER,
    HallID NUMBER,
    FOREIGN KEY (MovieID) REFERENCES Movie(MovieID),
    FOREIGN KEY (HallID) REFERENCES Hall(HallID)
);
CREATE TABLE Seat (
    SeatID NUMBER PRIMARY KEY,
    SeatRow VARCHAR2(10),
    SeatNumber NUMBER,
    IsAvailable NUMBER(1)
);
CREATE TABLE Price (
    PriceID NUMBER PRIMARY KEY,
    PriceAmount NUMBER(10,2),
    IsFestiveSeason NUMBER(1),
    IsReleasePeriod NUMBER(1)
);
CREATE TABLE Cancellation (
    CancellationID NUMBER PRIMARY KEY,
    CancellationTime TIMESTAMP,
    CancellationReason VARCHAR2(200),
    CancellationStatus VARCHAR2(50)
);
CREATE TABLE Booking (
    BookingID NUMBER PRIMARY KEY,
    ConfirmationCode VARCHAR2(50),
    ShowID NUMBER,
    PaymentID NUMBER,
    UserID NUMBER,
    FOREIGN KEY (ShowID) REFERENCES Shows(ShowID),
    FOREIGN KEY (PaymentID) REFERENCES Payment(PaymentID),
    FOREIGN KEY (UserID) REFERENCES UserTable(UserID)
);
CREATE TABLE Ticket (
    TicketID NUMBER PRIMARY KEY,
    TicketStatus VARCHAR2(50),
    TicketNumber VARCHAR2(50),
    SeatID NUMBER,
    CancellationID NUMBER,
    PriceID NUMBER,
    BookingID NUMBER,
    FOREIGN KEY (SeatID) REFERENCES Seat(SeatID),
    FOREIGN KEY (CancellationID) REFERENCES Cancellation(CancellationID),
    FOREIGN KEY (PriceID) REFERENCES Price(PriceID),
    FOREIGN KEY (BookingID) REFERENCES Booking(BookingID)
);

COMMIT;
