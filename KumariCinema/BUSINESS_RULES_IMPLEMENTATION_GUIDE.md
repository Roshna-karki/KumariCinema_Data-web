================================================================================
BUSINESS RULES IMPLEMENTATION GUIDE
Kumari Cinema Management System - ASP.NET WebForms + Oracle
================================================================================

This document provides complete implementation guide for all 7 business rules
with database constraints, C# validation, and integration examples.

================================================================================
1. MOVIE TITLE MUST BE UNIQUE
================================================================================

DATABASE CONSTRAINT (Oracle):
```sql
ALTER TABLE Movie ADD CONSTRAINT uk_movie_title UNIQUE (Title);
```

C# VALIDATION (MovieDetails.aspx.cs):
```csharp
BusinessRuleValidator validator = new BusinessRuleValidator();
int? excludeMovieID = ViewState["EditMovieID"] != null ? 
    (int?)Convert.ToInt32(ViewState["EditMovieID"]) : null;
var titleValidation = validator.ValidateMovieTitleUnique(txtTitle.Text.Trim(), excludeMovieID);

if (!titleValidation.IsValid)
{
    ShowMessage(titleValidation.ErrorMessage, true);
    return;
}
```

ERROR MESSAGES:
? Success: Movie title validated successfully
? Error: "Movie 'Avatar 3' already exists in the system. Movie titles must be unique."

TESTING SCENARIOS:
1. Add movie "Avatar 3" ? Success
2. Try to add "Avatar 3" again ? Error with message above
3. Edit different movie ? Success (excludes current movie from check)
4. Try to update to existing movie title ? Error

================================================================================
2. THEATRE NAME CANNOT DUPLICATE IN THE SAME CITY
================================================================================

DATABASE CONSTRAINT (Oracle):
```sql
ALTER TABLE Theatre ADD CONSTRAINT uk_theatre_name_city UNIQUE (TheatreName, City);
```

C# VALIDATION (TheatreDetails.aspx.cs):
```csharp
BusinessRuleValidator validator = new BusinessRuleValidator();
int? excludeTheatreID = ViewState["EditTheatreID"] != null ? 
    (int?)Convert.ToInt32(ViewState["EditTheatreID"]) : null;

var theatreValidation = validator.ValidateTheatreNameInCity(
    txtTheatreName.Text.Trim(), 
    txtCity.Text.Trim(), 
    excludeTheatreID
);

if (!theatreValidation.IsValid)
{
    ShowMessage(theatreValidation.ErrorMessage, true);
    return;
}
```

ERROR MESSAGES:
? Success: Theatre validated for city
? Error: "Theatre 'Kumari Cinema' already exists in Kathmandu. Theatre names must be unique within each city."

TESTING SCENARIOS:
1. Add "Kumari Cinema" in "Kathmandu" ? Success
2. Try to add "Kumari Cinema" in "Kathmandu" again ? Error
3. Add "Kumari Cinema" in "Pokhara" ? Success (different city allowed)
4. Edit existing theatre in Kathmandu ? Success (excludes current theatre)

KEY POINTS:
- Same theatre name in DIFFERENT cities is allowed
- Same theatre name in SAME city is NOT allowed
- Case-insensitive comparison (UPPER/TRIM in SQL)

================================================================================
3. HALL NAME MUST BE UNIQUE WITHIN A THEATRE
================================================================================

DATABASE CONSTRAINT (Oracle):
```sql
ALTER TABLE Hall ADD CONSTRAINT uk_hall_name_theatre UNIQUE (HallName, TheatreID);
```

C# VALIDATION (HallDetails.aspx.cs - to be implemented):
```csharp
BusinessRuleValidator validator = new BusinessRuleValidator();
int? excludeHallID = ViewState["EditHallID"] != null ? 
    (int?)Convert.ToInt32(ViewState["EditHallID"]) : null;

var hallValidation = validator.ValidateHallNameInTheatre(
    txtHallName.Text.Trim(),
    Convert.ToInt32(ddlTheatre.SelectedValue),
    excludeHallID
);

if (!hallValidation.IsValid)
{
    ShowMessage(hallValidation.ErrorMessage, true);
    return;
}
```

ERROR MESSAGES:
? Success: Hall name validated within theatre
? Error: "Hall 'Hall A - IMAX' already exists in this theatre. Hall names must be unique within a theatre."

TESTING SCENARIOS:
1. Add "Hall A" in "Kumari Cinema Kathmandu" ? Success
2. Try to add "Hall A" in same theatre ? Error
3. Add "Hall A" in different theatre ? Success
4. Rename "Hall A" to different name ? Success

================================================================================
4. SHOW SCHEDULING VALIDATION (NO DOUBLE BOOKING)
================================================================================

DATABASE CONSTRAINT (Oracle):
```sql
ALTER TABLE Shows ADD CONSTRAINT uk_shows_hall_date_time UNIQUE (HallID, ShowDate, ShowTime);
```

C# VALIDATION (ShowtimeDetails.aspx.cs - to be implemented):
```csharp
BusinessRuleValidator validator = new BusinessRuleValidator();
int? excludeShowID = ViewState["EditShowID"] != null ? 
    (int?)Convert.ToInt32(ViewState["EditShowID"]) : null;

var showValidation = validator.ValidateShowScheduleConflict(
    Convert.ToInt32(ddlHall.SelectedValue),
    Convert.ToDateTime(txtShowDate.Text),
    txtShowTime.Text.Trim(),
    excludeShowID
);

if (!showValidation.IsValid)
{
    ShowMessage(showValidation.ErrorMessage, true);
    return;
}
```

ERROR MESSAGES:
? Success: Show time slot available
? Error: "Show conflict detected. A show already exists in this hall on 2024-12-15 at 18:00. A hall cannot have two shows at the same date and time."

TESTING SCENARIOS:
1. Schedule show in Hall A on 2024-12-15 at 18:00 ? Success
2. Try to schedule another show in Hall A on same date/time ? Error
3. Schedule same movie in different hall at same time ? Success
4. Schedule show in Hall A at different time same date ? Success
5. Schedule show in Hall A same time different date ? Success

TIME SLOT RULES:
- Each hall can have only ONE show per date-time combination
- Different halls can have shows at same time
- Same hall can have multiple shows on same day (at different times)

================================================================================
5. CUSTOMER EMAIL AND PHONE NUMBER MUST BE UNIQUE
================================================================================

DATABASE CONSTRAINTS (Oracle):
```sql
ALTER TABLE Users ADD CONSTRAINT uk_users_email UNIQUE (Email);
ALTER TABLE Users ADD CONSTRAINT uk_users_contact UNIQUE (ContactNumber);

-- Alternative column names:
ALTER TABLE Users ADD CONSTRAINT uk_users_email UNIQUE (UserEmail);
ALTER TABLE Users ADD CONSTRAINT uk_users_phone UNIQUE (PhoneNumber);
```

C# VALIDATION (CustomerDetails.aspx.cs):
```csharp
BusinessRuleValidator validator = new BusinessRuleValidator();
int? excludeUserID = ViewState["EditUserID"] != null ? 
    (int?)Convert.ToInt32(ViewState["EditUserID"]) : null;

// Validate email
var emailValidation = validator.ValidateEmailUnique(
    txtEmail.Text.Trim(), 
    excludeUserID
);

if (!emailValidation.IsValid)
{
    ShowMessage(emailValidation.ErrorMessage, true);
    return;
}

// Validate phone
var phoneValidation = validator.ValidatePhoneNumberUnique(
    txtPhone.Text.Trim(), 
    excludeUserID
);

if (!phoneValidation.IsValid)
{
    ShowMessage(phoneValidation.ErrorMessage, true);
    return;
}
```

EMAIL VALIDATION:
? Valid: user@example.com, john.doe@kumari.com, customer123@mail.co.uk
? Invalid: plaintext, user@, @domain.com, user@domain (no TLD)

PHONE VALIDATION:
? Valid: 9841234567, +977-9841234567, 984-123-4567 (minimum 7 digits)
? Invalid: 12345 (less than 7 digits), empty string

ERROR MESSAGES:
Email Errors:
? "Email 'john@example.com' is already registered. Customer emails must be unique."
? "Invalid email format. Please enter a valid email address (e.g., user@example.com)."
? "Email cannot be empty."

Phone Errors:
? "Phone number '9841234567' is already registered. Phone numbers must be unique."
? "Phone number must contain at least 7 digits."
? "Phone number cannot be empty."

TESTING SCENARIOS:
1. Register customer with email john@test.com ? Success
2. Try to register with same email ? Error
3. Register customer with phone 9841234567 ? Success
4. Try to register with same phone ? Error
5. Edit customer, keep email same ? Success (excluded from check)
6. Edit customer, change to existing email ? Error
7. Invalid email format john@.com ? Error
8. Phone with only 6 digits ? Error

================================================================================
6. TICKET CANCELLATION TIMING RULE (CRITICAL BUSINESS RULE)
================================================================================

RULE DEFINITION:
Ticket cancellation is ONLY ALLOWED if the show start time is MORE THAN 1 hour 
away from the current time.

TIMING EXAMPLES:
? 2 hours before show starts ? ALLOW cancellation
? 1 hour 30 minutes before show ? ALLOW cancellation
? 1 hour 1 minute before show ? ALLOW cancellation
? Exactly 1 hour before show ? DENY cancellation
? 59 minutes before show ? DENY cancellation
? Show has started ? DENY cancellation

DATABASE IMPLEMENTATION:
No specific constraint - this is a business logic rule enforced in C#
using DateTime and TimeSpan comparison.

C# VALIDATION (TicketDetails.aspx.cs):
```csharp
BusinessRuleValidator validator = new BusinessRuleValidator();
var cancellationValidation = validator.ValidateTicketCancellationTiming(ticketId);

if (!cancellationValidation.IsValid)
{
    ShowMessage(cancellationValidation.ErrorMessage, true);
    return;
}

// Proceed with cancellation if validation passed
int seatId = 0;
using (OracleCommand cmd = new OracleCommand(
    "SELECT SeatID FROM Ticket WHERE TicketID = :tid", conn))
{
    cmd.Parameters.Add(":tid", OracleDbType.Int32).Value = ticketId;
    object o = cmd.ExecuteScalar();
    if (o != null && o != DBNull.Value) seatId = Convert.ToInt32(o);
}

using (OracleCommand cmd = new OracleCommand(
    "UPDATE Ticket SET TicketStatus = 'CANCELLED' WHERE TicketID = :tid", conn))
{
    cmd.Parameters.Add(":tid", OracleDbType.Int32).Value = ticketId;
    cmd.ExecuteNonQuery();
}

// Release seat back to available
if (seatId > 0)
{
    using (OracleCommand cmd = new OracleCommand(
        "UPDATE Seat SET IsAvailable = 1 WHERE SeatID = :sid", conn))
    {
        cmd.Parameters.Add(":sid", OracleDbType.Int32).Value = seatId;
        cmd.ExecuteNonQuery();
    }
}
```

CODE LOGIC (from BusinessRuleValidator.cs):
```csharp
// Calculate time remaining until show start
DateTime currentTime = DateTime.Now;
TimeSpan timeUntilShow = showStartDateTime - currentTime;

// CRITICAL: Cancellation allowed ONLY if > 1 hour remaining
if (timeUntilShow.TotalMinutes <= 60)
{
    // Calculate remaining minutes
    int minutesRemaining = (int)Math.Ceiling(timeUntilShow.TotalMinutes);
    
    if (minutesRemaining < 0)
        return ShowError("Show has already started...");
    else if (minutesRemaining == 0)
        return ShowError("Cancellation window has closed...");
    else
        return ShowError($"Only {minutesRemaining} minutes remaining...");
}

// More than 1 hour away - cancellation allowed
int minutesBeforeShow = (int)Math.Floor(timeUntilShow.TotalMinutes);
return ShowSuccess($"Cancellation is allowed. Show starts in {minutesBeforeShow} minutes...");
```

ERROR MESSAGES:
Success:
? "Cancellation is allowed. Show starts in 125 minutes at 2024-12-15 18:00."

Errors:
? "Cancellation window has closed. Only 45 minutes remaining before show starts at 18:00. Tickets must be cancelled at least 1 hour before show start."
? "Show has already started at 2024-12-15 18:00. Tickets cannot be cancelled after the show begins."
? "This ticket has already been cancelled and cannot be cancelled again."

TESTING SCENARIOS:
Create show scheduled for 2024-12-15 18:00, test cancellation at:
1. 2024-12-15 14:30 (210 minutes before) ? Allow (7+ min before buffer)
2. 2024-12-15 16:45 (75 minutes before) ? Allow
3. 2024-12-15 17:00 (60 minutes before) ? DENY
4. 2024-12-15 17:30 (30 minutes before) ? DENY
5. 2024-12-15 18:05 (after start) ? DENY
6. Already cancelled ticket ? DENY with "already cancelled" message

================================================================================
7. PREVENT DOUBLE SEAT BOOKING
================================================================================

DATABASE CONSTRAINT (Oracle):
```sql
-- Ensure same seat cannot be booked twice for same show
CREATE UNIQUE INDEX uk_ticket_seat_show ON Ticket (SeatID, BookingID, 
    (SELECT ShowID FROM Booking WHERE Booking.BookingID = Ticket.BookingID))
    WHERE TicketStatus IN ('CONFIRMED', 'USED');
```

C# VALIDATION (BookingDetails.aspx.cs or TicketDetails.aspx.cs - to be implemented):
```csharp
BusinessRuleValidator validator = new BusinessRuleValidator();
int? excludeBookingID = ViewState["EditBookingID"] != null ? 
    (int?)Convert.ToInt32(ViewState["EditBookingID"]) : null;

var seatValidation = validator.ValidateSeatNotDoubleBooked(
    Convert.ToInt32(ddlSeat.SelectedValue),
    Convert.ToInt32(ddlShow.SelectedValue),
    excludeBookingID
);

if (!seatValidation.IsValid)
{
    ShowMessage(seatValidation.ErrorMessage, true);
    return;
}
```

ERROR MESSAGE:
? "Seat is already booked for this show. The same seat cannot be booked twice for the same show."

TESTING SCENARIOS:
1. Book Seat A-1 for Show 1 ? Success (TicketStatus = CONFIRMED)
2. Try to book same seat for same show ? Error
3. Book Seat A-1 for different show ? Success
4. Book Seat A-2 for Show 1 ? Success (different seat)
5. Cancel ticket for Seat A-1 ? Allow rebooking (or mark as CANCELLED)
6. Rebook Seat A-1 after cancellation ? Success (status not CONFIRMED/USED)

================================================================================
INTEGRATION CHECKLIST
================================================================================

DATABASE SETUP:
[ ] Create database user 'cinema' in Oracle
[ ] Run SQL_BUSINESS_RULES_CONSTRAINTS.sql to add all constraints
[ ] Verify constraints with: SELECT * FROM user_constraints
[ ] Ensure sequences exist for all tables

C# CODE SETUP:
[ ] Copy BusinessRuleValidator.cs to KumariCinema folder
[ ] Add using statement: using KumariCinema.Validators;
[ ] Rebuild solution to ensure no compilation errors
[ ] Verify OracleConnectionString in Web.config

INTEGRATION IN EACH PAGE:
[ ] MovieDetails.aspx.cs - Movie title validation
[ ] CustomerDetails.aspx.cs - Email and phone validation
[ ] TheatreDetails.aspx.cs - Theatre name-city validation
[ ] HallDetails.aspx.cs - Hall name-theatre validation
[ ] ShowtimeDetails.aspx.cs - Show schedule conflict validation
[ ] TicketDetails.aspx.cs - Ticket cancellation timing validation
[ ] BookingDetails.aspx.cs - Seat double booking validation (if applicable)

TESTING STRATEGY:
[ ] Test each rule individually with valid data
[ ] Test each rule with duplicate/invalid data
[ ] Test error messages are user-friendly
[ ] Test database constraints and C# validation work together
[ ] Test update scenarios (exclude current record from check)
[ ] Test cancel scenarios (release seat, update status)
[ ] Monitor database logs for constraint violations

================================================================================
ERROR HANDLING BEST PRACTICES
================================================================================

1. DATABASE CONSTRAINT VIOLATIONS:
   When Oracle constraint is violated, OracleException is thrown
   
   ```csharp
   catch (OracleException ex)
   {
       if (ex.Number == 1)  // ORA-00001: unique constraint violated
       {
           ShowMessage("Duplicate record: This entry already exists.", true);
       }
       else if (ex.Number == 2290)  // ORA-02290: check constraint violated
       {
           ShowMessage("Invalid data: Check constraint violated.", true);
       }
       else
       {
           ShowMessage($"Database error: {ex.Message}", true);
       }
   }
   ```

2. BUSINESS RULE VALIDATION ERRORS:
   These are caught by C# before reaching database
   
   ```csharp
   var result = validator.ValidateMovieTitleUnique(title);
   if (!result.IsValid)
   {
       ShowMessage(result.ErrorMessage, true);  // User-friendly message
       return;  // Do not proceed with database operation
   }
   ```

3. USER-FRIENDLY ERROR MESSAGES:
   - Describe what went wrong
   - Explain the business rule
   - Suggest corrective action
   - Avoid technical jargon (no "ORA-00001")

================================================================================
DEBUGGING TIPS
================================================================================

1. Enable SQL logging in MovieDetails.aspx.cs:
   ```csharp
   LogError($"Validating title: {title}");
   var result = validator.ValidateMovieTitleUnique(title);
   LogError($"Validation result: {result.IsValid}, Message: {result.ErrorMessage}");
   ```

2. Monitor database constraints:
   ```sql
   SELECT constraint_name, constraint_type, status
   FROM user_constraints
   WHERE table_name = 'MOVIE'
   AND constraint_type = 'U';  -- Unique constraints
   ```

3. Test unique constraints directly:
   ```sql
   INSERT INTO Movie (MovieID, Title, Duration, Genre, Language, ReleaseDate)
   VALUES (999, 'Avatar 3', 180, 'Sci-Fi', 'English', SYSDATE);
   
   -- This will fail if title is duplicate
   INSERT INTO Movie (MovieID, Title, Duration, Genre, Language, ReleaseDate)
   VALUES (1000, 'Avatar 3', 180, 'Sci-Fi', 'English', SYSDATE);
   -- ORA-00001: unique constraint (CINEMA.UK_MOVIE_TITLE) violated
   ```

================================================================================
PARAMETERIZED QUERIES & SQL INJECTION PREVENTION
================================================================================

ALL implementations use parameterized queries to prevent SQL injection:

? CORRECT (Safe):
```csharp
using (OracleCommand cmd = new OracleCommand(
    "SELECT COUNT(*) FROM Movie WHERE UPPER(TRIM(Title)) = UPPER(TRIM(:title))", conn))
{
    cmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = title;
    // ...
}
```

? WRONG (Vulnerable):
```csharp
using (OracleCommand cmd = new OracleCommand(
    "SELECT COUNT(*) FROM Movie WHERE Title = '" + title + "'", conn))
{
    // SQL injection possible if title contains: ' OR '1'='1
}
```

All BusinessRuleValidator methods use parameterized queries exclusively.

================================================================================
COMPLIANCE MATRIX
================================================================================

Rule | DB Constraint | C# Validation | Error Message | Test Status
-----|---------------|---------------|---------------|-------------
1    | UNIQUE        | ? Implemented | ? Clear      | Ready
2    | COMPOSITE     | ? Implemented | ? Clear      | Ready
3    | COMPOSITE     | Ready (HallDetails todo) | ? Clear | Ready
4    | COMPOSITE     | Ready (ShowtimeDetails todo) | ? Clear | Ready
5    | UNIQUE x2     | ? Implemented | ? Clear      | Ready
6    | Logic-based   | ? Implemented | ? Detailed   | Ready
7    | Index-based   | Ready (BookingDetails todo) | ? Clear | Ready

================================================================================
END OF BUSINESS RULES IMPLEMENTATION GUIDE
================================================================================
