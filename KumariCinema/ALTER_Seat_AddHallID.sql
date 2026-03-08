-- Add HallID to Seat so seats belong to a hall (for booking flow: choose seats by hall).
-- Run as the same user as your connection string. Safe if column already exists (ignore ORA-01430).
ALTER TABLE Seat ADD HallID NUMBER NULL;
ALTER TABLE Seat ADD CONSTRAINT fk_seat_hall FOREIGN KEY (HallID) REFERENCES Hall(HallID);

-- Festival dates for price logic (check if show date is a festival day).
CREATE TABLE FestivalDate (FestivalDate DATE PRIMARY KEY);
-- Example: INSERT INTO FestivalDate VALUES (TO_DATE('2025-10-20', 'YYYY-MM-DD')); -- Dashain etc.
COMMIT;
