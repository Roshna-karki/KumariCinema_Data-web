================================================================================
IMPLEMENTATION EXAMPLES FOR REMAINING BUSINESS RULES
HallDetails.aspx.cs, ShowtimeDetails.aspx.cs, BookingDetails.aspx.cs
================================================================================

This file provides code snippets for implementing business rules 3, 4, and 7
in the remaining pages that interact with Hall, Shows, and Booking/Seat data.

================================================================================
RULE 3: HALL NAME MUST BE UNIQUE WITHIN A THEATRE
Location: HallDetails.aspx.cs
================================================================================

// Add this using statement at the top
using KumariCinema.Validators;

protected void btnSave_Click(object sender, EventArgs e)
{
    // Basic validation
    if (string.IsNullOrEmpty(txtHallName.Text.Trim()))
    {
        ShowMessage("Hall Name is required", true);
        return;
    }

    if (string.IsNullOrEmpty(ddlTheatre.SelectedValue) || ddlTheatre.SelectedValue == "0")
    {
        ShowMessage("Please select a Theatre", true);
        return;
    }

    if (string.IsNullOrEmpty(txtHallCapacity.Text.Trim()))
    {
        ShowMessage("Hall Capacity is required", true);
        return;
    }

    try
    {
        int theatreID = Convert.ToInt32(ddlTheatre.SelectedValue);
        
        // RULE 3: Validate hall name uniqueness within theatre
        BusinessRuleValidator validator = new BusinessRuleValidator();
        int? excludeHallID = ViewState["EditHallID"] != null ? 
            (int?)Convert.ToInt32(ViewState["EditHallID"]) : null;
        
        var hallValidation = validator.ValidateHallNameInTheatre(
            txtHallName.Text.Trim(),
            theatreID,
            excludeHallID
        );

        if (!hallValidation.IsValid)
        {
            ShowMessage(hallValidation.ErrorMessage, true);
            return;
        }

        // Validation passed - proceed with database operation
        using (OracleConnection conn = new OracleConnection(connectionString))
        {
            conn.Open();
            string query;

            if (ViewState["EditHallID"] != null)
            {
                // Update existing hall
                query = @"UPDATE Hall 
                         SET HallName = :name, 
                             HallCapacity = :capacity,
                             ExperienceType = :experience,
                             ScreenSize = :screensize,
                             TheatreID = :theatreid
                         WHERE HallID = :hallid";

                using (OracleCommand cmd = new OracleCommand(query, conn))
                {
                    cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtHallName.Text.Trim();
                    cmd.Parameters.Add(":capacity", OracleDbType.Int32).Value = Convert.ToInt32(txtHallCapacity.Text);
                    cmd.Parameters.Add(":experience", OracleDbType.Varchar2).Value = ddlExperience.SelectedValue;
                    cmd.Parameters.Add(":screensize", OracleDbType.Varchar2).Value = txtScreenSize.Text.Trim();
                    cmd.Parameters.Add(":theatreid", OracleDbType.Int32).Value = theatreID;
                    cmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = Convert.ToInt32(ViewState["EditHallID"]);
                    
                    cmd.ExecuteNonQuery();
                    ShowMessage("Hall updated successfully!", false);
                }
            }
            else
            {
                // Insert new hall
                int nextId = 1;
                using (OracleCommand cmdMax = new OracleCommand("SELECT NVL(MAX(HallID), 0) + 1 FROM Hall", conn))
                {
                    object o = cmdMax.ExecuteScalar();
                    nextId = Convert.ToInt32(o);
                }

                query = @"INSERT INTO Hall (HallID, HallName, HallCapacity, ExperienceType, ScreenSize, TheatreID)
                         VALUES (:hallid, :name, :capacity, :experience, :screensize, :theatreid)";

                using (OracleCommand cmd = new OracleCommand(query, conn))
                {
                    cmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = nextId;
                    cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtHallName.Text.Trim();
                    cmd.Parameters.Add(":capacity", OracleDbType.Int32).Value = Convert.ToInt32(txtHallCapacity.Text);
                    cmd.Parameters.Add(":experience", OracleDbType.Varchar2).Value = ddlExperience.SelectedValue;
                    cmd.Parameters.Add(":screensize", OracleDbType.Varchar2).Value = txtScreenSize.Text.Trim();
                    cmd.Parameters.Add(":theatreid", OracleDbType.Int32).Value = theatreID;
                    
                    cmd.ExecuteNonQuery();
                    ShowMessage("Hall added successfully!", false);
                }
            }

            ViewState["EditHallID"] = null;
            LoadHalls();
            ClearForm();
        }
    }
    catch (Exception ex)
    {
        ShowMessage($"Error: {ex.Message}", true);
    }
}

================================================================================
RULE 4: SHOW SCHEDULING VALIDATION (NO DOUBLE BOOKING)
Location: ShowtimeDetails.aspx.cs
================================================================================

// Add this using statement at the top
using KumariCinema.Validators;

protected void btnSave_Click(object sender, EventArgs e)
{
    // Basic validation
    if (string.IsNullOrEmpty(txtShowDate.Text))
    {
        ShowMessage("Show Date is required", true);
        return;
    }

    if (string.IsNullOrEmpty(ddlShowTime.SelectedValue) || ddlShowTime.SelectedValue == "0")
    {
        ShowMessage("Show Time is required", true);
        return;
    }

    if (string.IsNullOrEmpty(ddlMovie.SelectedValue) || ddlMovie.SelectedValue == "0")
    {
        ShowMessage("Movie is required", true);
        return;
    }

    if (string.IsNullOrEmpty(ddlHall.SelectedValue) || ddlHall.SelectedValue == "0")
    {
        ShowMessage("Hall is required", true);
        return;
    }

    try
    {
        DateTime showDate = Convert.ToDateTime(txtShowDate.Text);
        string showTime = ddlShowTime.SelectedValue;  // Format: "HH:MM"
        int hallID = Convert.ToInt32(ddlHall.SelectedValue);

        // RULE 4: Validate show schedule (no double booking)
        BusinessRuleValidator validator = new BusinessRuleValidator();
        int? excludeShowID = ViewState["EditShowID"] != null ? 
            (int?)Convert.ToInt32(ViewState["EditShowID"]) : null;

        var showValidation = validator.ValidateShowScheduleConflict(
            hallID,
            showDate,
            showTime,
            excludeShowID
        );

        if (!showValidation.IsValid)
        {
            ShowMessage(showValidation.ErrorMessage, true);
            return;
        }

        // Validation passed - proceed with database operation
        using (OracleConnection conn = new OracleConnection(connectionString))
        {
            conn.Open();
            string query;

            if (ViewState["EditShowID"] != null)
            {
                // Update existing show
                query = @"UPDATE Shows 
                         SET ShowDate = :showdate,
                             ShowTime = :showtime,
                             MovieID = :movieid,
                             HallID = :hallid
                         WHERE ShowID = :showid";

                using (OracleCommand cmd = new OracleCommand(query, conn))
                {
                    cmd.Parameters.Add(":showdate", OracleDbType.Date).Value = showDate;
                    cmd.Parameters.Add(":showtime", OracleDbType.Varchar2).Value = showTime;
                    cmd.Parameters.Add(":movieid", OracleDbType.Int32).Value = Convert.ToInt32(ddlMovie.SelectedValue);
                    cmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = hallID;
                    cmd.Parameters.Add(":showid", OracleDbType.Int32).Value = Convert.ToInt32(ViewState["EditShowID"]);
                    
                    cmd.ExecuteNonQuery();
                    ShowMessage("Show updated successfully!", false);
                }
            }
            else
            {
                // Insert new show
                int nextId = 1;
                using (OracleCommand cmdMax = new OracleCommand("SELECT NVL(MAX(ShowID), 0) + 1 FROM Shows", conn))
                {
                    object o = cmdMax.ExecuteScalar();
                    nextId = Convert.ToInt32(o);
                }

                query = @"INSERT INTO Shows (ShowID, ShowDate, ShowTime, MovieID, HallID)
                         VALUES (:showid, :showdate, :showtime, :movieid, :hallid)";

                using (OracleCommand cmd = new OracleCommand(query, conn))
                {
                    cmd.Parameters.Add(":showid", OracleDbType.Int32).Value = nextId;
                    cmd.Parameters.Add(":showdate", OracleDbType.Date).Value = showDate;
                    cmd.Parameters.Add(":showtime", OracleDbType.Varchar2).Value = showTime;
                    cmd.Parameters.Add(":movieid", OracleDbType.Int32).Value = Convert.ToInt32(ddlMovie.SelectedValue);
                    cmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = hallID;
                    
                    cmd.ExecuteNonQuery();
                    ShowMessage("Show scheduled successfully!", false);
                }
            }

            ViewState["EditShowID"] = null;
            LoadShows();
            ClearForm();
        }
    }
    catch (Exception ex)
    {
        ShowMessage($"Error: {ex.Message}", true);
    }
}

================================================================================
RULE 7: PREVENT DOUBLE SEAT BOOKING
Location: BookingDetails.aspx.cs or TicketDetails.aspx.cs
================================================================================

// Add this using statement at the top
using KumariCinema.Validators;

// This would be in the method that adds a ticket/seat to a booking
protected void btnAddSeatToBooking_Click(object sender, EventArgs e)
{
    // Basic validation
    if (string.IsNullOrEmpty(ddlSeat.SelectedValue) || ddlSeat.SelectedValue == "0")
    {
        ShowMessage("Please select a Seat", true);
        return;
    }

    if (string.IsNullOrEmpty(ddlShow.SelectedValue) || ddlShow.SelectedValue == "0")
    {
        ShowMessage("Please select a Show", true);
        return;
    }

    try
    {
        int seatID = Convert.ToInt32(ddlSeat.SelectedValue);
        int showID = Convert.ToInt32(ddlShow.SelectedValue);

        // RULE 7: Validate that seat is not already booked for this show
        BusinessRuleValidator validator = new BusinessRuleValidator();
        int? excludeBookingID = ViewState["EditBookingID"] != null ? 
            (int?)Convert.ToInt32(ViewState["EditBookingID"]) : null;

        var seatValidation = validator.ValidateSeatNotDoubleBooked(
            seatID,
            showID,
            excludeBookingID
        );

        if (!seatValidation.IsValid)
        {
            ShowMessage(seatValidation.ErrorMessage, true);
            return;
        }

        // Validation passed - proceed with adding seat to booking
        using (OracleConnection conn = new OracleConnection(connectionString))
        {
            conn.Open();

            // First, get the booking ID
            if (ViewState["BookingID"] == null)
            {
                ShowMessage("No booking selected. Please create a booking first.", true);
                return;
            }

            int bookingID = Convert.ToInt32(ViewState["BookingID"]);

            // Create ticket record with this seat for this booking
            int nextTicketId = 1;
            using (OracleCommand cmdMax = new OracleCommand("SELECT NVL(MAX(TicketID), 0) + 1 FROM Ticket", conn))
            {
                object o = cmdMax.ExecuteScalar();
                nextTicketId = Convert.ToInt32(o);
            }

            string query = @"INSERT INTO Ticket (TicketID, TicketNumber, TicketStatus, BookingID, SeatID)
                           VALUES (:ticketid, :ticketnumber, 'CONFIRMED', :bookingid, :seatid)";

            using (OracleCommand cmd = new OracleCommand(query, conn))
            {
                string ticketNumber = $"TKT-{DateTime.Now:yyyy}-{nextTicketId:00000}";
                
                cmd.Parameters.Add(":ticketid", OracleDbType.Int32).Value = nextTicketId;
                cmd.Parameters.Add(":ticketnumber", OracleDbType.Varchar2).Value = ticketNumber;
                cmd.Parameters.Add(":bookingid", OracleDbType.Int32).Value = bookingID;
                cmd.Parameters.Add(":seatid", OracleDbType.Int32).Value = seatID;
                
                cmd.ExecuteNonQuery();
            }

            // Mark seat as unavailable
            using (OracleCommand cmd = new OracleCommand("UPDATE Seat SET IsAvailable = 0 WHERE SeatID = :seatid", conn))
            {
                cmd.Parameters.Add(":seatid", OracleDbType.Int32).Value = seatID;
                cmd.ExecuteNonQuery();
            }

            ShowMessage("Seat added to booking successfully!", false);
            LoadBookingSeats(bookingID);
            LoadAvailableSeats(showID);
        }
    }
    catch (Exception ex)
    {
        ShowMessage($"Error: {ex.Message}", true);
    }
}

================================================================================
TESTING CODE FOR ALL RULES
================================================================================

Test Scenario 1: Movie Title Uniqueness
```csharp
// Add this to a test button click handler
protected void btnTestMovieTitle_Click(object sender, EventArgs e)
{
    BusinessRuleValidator validator = new BusinessRuleValidator();

    // Test 1: Valid new title
    var result1 = validator.ValidateMovieTitleUnique("Test Movie 1");
    Debug.WriteLine($"Test 1 (new title): {result1.IsValid} - {result1.ErrorMessage}");

    // Test 2: Duplicate title (assuming already in DB)
    var result2 = validator.ValidateMovieTitleUnique("Avatar 3");
    Debug.WriteLine($"Test 2 (duplicate): {result2.IsValid} - {result2.ErrorMessage}");

    // Test 3: Exclude current movie from check
    var result3 = validator.ValidateMovieTitleUnique("Avatar 3", excludeMovieID: 1);
    Debug.WriteLine($"Test 3 (update same title): {result3.IsValid} - {result3.ErrorMessage}");
}
```

Test Scenario 2: Theatre Name-City Uniqueness
```csharp
protected void btnTestTheatreName_Click(object sender, EventArgs e)
{
    BusinessRuleValidator validator = new BusinessRuleValidator();

    // Test 1: Valid new theatre
    var result1 = validator.ValidateTheatreNameInCity("New Cinema", "Bhaktapur");
    Debug.WriteLine($"Test 1 (new theatre): {result1.IsValid} - {result1.ErrorMessage}");

    // Test 2: Duplicate in same city
    var result2 = validator.ValidateTheatreNameInCity("Kumari Cinema", "Kathmandu");
    Debug.WriteLine($"Test 2 (duplicate): {result2.IsValid} - {result2.ErrorMessage}");

    // Test 3: Same name different city (should be allowed)
    var result3 = validator.ValidateTheatreNameInCity("Kumari Cinema", "Pokhara");
    Debug.WriteLine($"Test 3 (different city): {result3.IsValid} - {result3.ErrorMessage}");
}
```

Test Scenario 3: Ticket Cancellation Timing
```csharp
protected void btnTestCancellation_Click(object sender, EventArgs e)
{
    BusinessRuleValidator validator = new BusinessRuleValidator();

    // Test with existing ticket
    int ticketID = 1;  // Assume this ticket exists
    var result = validator.ValidateTicketCancellationTiming(ticketID);
    
    Debug.WriteLine($"Cancellation allowed: {result.IsValid}");
    Debug.WriteLine($"Message: {result.ErrorMessage}");
}
```

================================================================================
DEBUGGING ORACLE CONSTRAINT VIOLATIONS
================================================================================

When a constraint is violated at the database level, catch OracleException:

```csharp
catch (OracleException ex)
{
    if (ex.Number == 1)  // ORA-00001: unique constraint violated
    {
        // Map the constraint name to business rule
        if (ex.Message.Contains("UK_MOVIE_TITLE"))
            ShowMessage("Movie title already exists", true);
        else if (ex.Message.Contains("UK_THEATRE_NAME_CITY"))
            ShowMessage("Theatre already exists in this city", true);
        else if (ex.Message.Contains("UK_HALL_NAME_THEATRE"))
            ShowMessage("Hall name already exists in this theatre", true);
        else if (ex.Message.Contains("UK_SHOWS_HALL_DATE_TIME"))
            ShowMessage("Show already scheduled for this hall at this time", true);
        else if (ex.Message.Contains("UK_USERS_EMAIL"))
            ShowMessage("Email already registered", true);
        else if (ex.Message.Contains("UK_USERS_CONTACT"))
            ShowMessage("Phone number already registered", true);
        else
            ShowMessage("Duplicate record detected", true);
    }
    else if (ex.Number == 2290)  // ORA-02290: check constraint violated
    {
        ShowMessage("Invalid data: Check constraint violated", true);
    }
    else if (ex.Number == 2291)  // ORA-02291: integrity constraint violated
    {
        ShowMessage("Foreign key reference error", true);
    }
    else
    {
        ShowMessage($"Database error: {ex.Message}", true);
    }
}
```

================================================================================
BEST PRACTICES SUMMARY
================================================================================

1. ALWAYS validate before database operations
2. ALWAYS use parameterized queries
3. ALWAYS handle exceptions gracefully
4. ALWAYS provide user-friendly error messages
5. ALWAYS exclude current record from uniqueness checks during updates
6. ALWAYS log validation attempts for debugging
7. ALWAYS test both success and failure scenarios
8. ALWAYS test database constraints independently
9. ALWAYS verify time-based validations (especially cancellation rule)
10. ALWAYS release resources (seats, bookings) properly on cancellation

================================================================================
END OF IMPLEMENTATION EXAMPLES
================================================================================
