using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.IO;
using System.Text.RegularExpressions;

namespace KumariCinema
{
    public partial class MovieDetails : Page
    {
        private string connectionString = 
            ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;
        
        private string logPath = System.Web.HttpContext.Current.Server.MapPath("~/Logs/");

        private void LogError(string message)
        {
            try
            {
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
                
                string logFile = Path.Combine(logPath, $"MovieDetails_{DateTime.Now:yyyyMMdd}.log");
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
                File.AppendAllText(logFile, logMessage);
            }
            catch
            {
                // Silently fail if logging fails
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LogError("=== Page_Load Started ===");
                LogError($"Connection String: {connectionString}");
                LoadMovies();
            }
        }

        private void LoadMovies(string searchTerm = "")
        {
            try
            {
                LogError($"LoadMovies called with searchTerm: '{searchTerm}'");
                
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    LogError("Creating OracleConnection...");
                    conn.Open();
                    LogError("Connection opened successfully!");

                    string query = @"SELECT MovieID, Title, Genre, Language, Duration, ReleaseDate
                                   FROM Movie";

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        query += " WHERE UPPER(Title) LIKE UPPER(:searchterm) OR UPPER(Genre) LIKE UPPER(:searchterm)";
                    }
                    query += " ORDER BY ReleaseDate DESC";

                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            cmd.Parameters.Add(":searchterm", OracleDbType.Varchar2).Value = $"%{searchTerm}%";
                            LogError($"Search parameter added: {searchTerm}");
                        }

                        LogError($"Executing query: {cmd.CommandText}");

                        using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }

                    gvMovies.DataSource = dt;
                    gvMovies.DataBind();

                    LogError($"? Movies loaded successfully: {dt.Rows.Count} movies");
                }
            }
            catch (Exception ex)
            {
                LogError($"? ERROR in LoadMovies: {ex.GetType().Name}");
                LogError($"Message: {ex.Message}");
                LogError($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogError($"InnerException: {ex.InnerException.Message}");
                }
                
                ShowMessage($"Error loading movies: {ex.Message}", true);
                System.Diagnostics.Debug.WriteLine($"? Error: {ex.Message}");
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            LogError("=== btnSave_Click Started ===");
            
            // Validation
            if (string.IsNullOrEmpty(txtTitle.Text.Trim()))
            {
                LogError("Validation failed: Title is empty");
                ShowMessage("Title is required", true);
                return;
            }

            if (string.IsNullOrEmpty(txtDuration.Text.Trim()))
            {
                LogError("Validation failed: Duration is empty");
                ShowMessage("Duration is required", true);
                return;
            }

            if (string.IsNullOrEmpty(ddlGenre.SelectedValue))
            {
                LogError("Validation failed: Genre not selected");
                ShowMessage("Genre is required", true);
                return;
            }

            if (string.IsNullOrEmpty(ddlLanguage.SelectedValue))
            {
                LogError("Validation failed: Language not selected");
                ShowMessage("Language is required", true);
                return;
            }

            if (string.IsNullOrEmpty(txtReleaseDate.Text.Trim()))
            {
                LogError("Validation failed: ReleaseDate is empty");
                ShowMessage("Release Date is required", true);
                return;
            }

            // Prevent duplicate movies: Title must be unique (case-insensitive, ignoring extra spaces)
            DateTime releaseDate = Convert.ToDateTime(txtReleaseDate.Text.Trim());
            int existingMovieId = ViewState["EditMovieID"] != null ? Convert.ToInt32(ViewState["EditMovieID"]) : 0;
            using (OracleConnection dupConn = new OracleConnection(connectionString))
            {
                dupConn.Open();
                string dupQuery = "SELECT COUNT(*) FROM Movie WHERE UPPER(TRIM(Title)) = UPPER(TRIM(:title))";
                if (existingMovieId > 0)
                {
                    dupQuery += " AND MovieID <> :movieid";
                }

                using (OracleCommand dupCmd = new OracleCommand(dupQuery, dupConn))
                {
                    dupCmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                    if (existingMovieId > 0)
                    {
                        dupCmd.Parameters.Add(":movieid", OracleDbType.Int32).Value = existingMovieId;
                    }

                    int dupCount = Convert.ToInt32(dupCmd.ExecuteScalar());
                    if (dupCount > 0)
                    {
                        LogError("Duplicate movie detected: same title");
                        ShowMessage("A movie with the same title already exists.", true);
                        return;
                    }
                }
            }

            try
            {
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    LogError("Connection opened for Save operation");
                    
                    string query;
                    
                    if (ViewState["EditMovieID"] != null)
                    {
                        LogError($"Updating movie ID: {ViewState["EditMovieID"]}");
                        query = @"UPDATE Movie 
                                 SET Title = :title, 
                                     Duration = :duration, 
                                     Genre = :genre, 
                                     Language = :language, 
                                     ReleaseDate = :releasedate
                                 WHERE MovieID = :movieid";
                    }
                    else
                    {
                        LogError("Inserting new movie");
                        int nextId = 1;
                        using (OracleCommand cmdMax = new OracleCommand("SELECT NVL(MAX(MovieID), 0) + 1 FROM Movie", conn))
                        {
                            object o = cmdMax.ExecuteScalar();
                            nextId = Convert.ToInt32(o);
                        }
                        query = @"INSERT INTO Movie (MovieID, Title, Duration, Genre, Language, ReleaseDate) 
                                 VALUES (:movieid, :title, :duration, :genre, :language, :releasedate)";
                        using (OracleCommand cmd = new OracleCommand(query, conn))
                        {
                            cmd.Parameters.Add(":movieid", OracleDbType.Int32).Value = nextId;
                            cmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                            cmd.Parameters.Add(":duration", OracleDbType.Int32).Value = Convert.ToInt32(txtDuration.Text);
                            cmd.Parameters.Add(":genre", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                            cmd.Parameters.Add(":language", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                            cmd.Parameters.Add(":releasedate", OracleDbType.Date).Value = Convert.ToDateTime(txtReleaseDate.Text);
                            cmd.ExecuteNonQuery();
                            LogError("? Movie saved successfully");
                            ShowMessage("Movie added successfully!", false);
                            ViewState["EditMovieID"] = null;
                            LoadMovies();
                            ClearForm();
                        }
                        return;
                    }

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = txtTitle.Text.Trim();
                        cmd.Parameters.Add(":duration", OracleDbType.Int32).Value = Convert.ToInt32(txtDuration.Text);
                        cmd.Parameters.Add(":genre", OracleDbType.Varchar2).Value = ddlGenre.SelectedValue;
                        cmd.Parameters.Add(":language", OracleDbType.Varchar2).Value = ddlLanguage.SelectedValue;
                        cmd.Parameters.Add(":releasedate", OracleDbType.Date).Value = Convert.ToDateTime(txtReleaseDate.Text);

                        if (ViewState["EditMovieID"] != null)
                        {
                            cmd.Parameters.Add(":movieid", OracleDbType.Int32).Value = Convert.ToInt32(ViewState["EditMovieID"]);
                        }

                        cmd.ExecuteNonQuery();
                        LogError("? Movie saved successfully");

                        ShowMessage(ViewState["EditMovieID"] != null ? "Movie updated successfully!" : "Movie added successfully!", false);
                        ViewState["EditMovieID"] = null;
                        LoadMovies();
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"? ERROR in btnSave_Click: {ex.Message}");
                LogError($"StackTrace: {ex.StackTrace}");
                ShowMessage($"Error: {ex.Message}", true);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            ViewState["EditMovieID"] = null;
            lblFormTitle.Text = "Add New Movie";
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvMovies.PageIndex = 0;
            LoadMovies(txtSearch.Text.Trim());
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            gvMovies.PageIndex = 0;
            LoadMovies();
        }

        protected void gvMovies_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int movieID = Convert.ToInt32(e.CommandArgument);

                if (e.CommandName == "EditMovie")
                {
                    LoadMovieForEdit(movieID);
                }
                else if (e.CommandName == "DeleteMovie")
                {
                    DeleteMovie(movieID);
                }
            }
            catch (Exception ex)
            {
                LogError($"? ERROR in gvMovies_RowCommand: {ex.GetType().Name}");
                LogError($"Message: {ex.Message}");
                LogError($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogError($"InnerException: {ex.InnerException.Message}");
                }

                ShowMessage($"Error: {ex.Message}", true);
            }
        }

        protected void gvMovies_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvMovies.PageIndex = e.NewPageIndex;
            LoadMovies(txtSearch.Text.Trim());
        }

        private void LoadMovieForEdit(int movieID)
        {
            try
            {
                LogError($"LoadMovieForEdit called with movieID: {movieID}");
                
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    LogError("Connection opened for edit load");

                    string query = "SELECT * FROM Movie WHERE MovieID = :movieid";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":movieid", OracleDbType.Int32).Value = movieID;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewState["EditMovieID"] = movieID;
                                lblFormTitle.Text = "Edit Movie";

                                txtTitle.Text = reader["Title"].ToString();
                                txtDuration.Text = reader["Duration"].ToString();
                                ddlGenre.SelectedValue = reader["Genre"].ToString();
                                ddlLanguage.SelectedValue = reader["Language"].ToString();
                                txtReleaseDate.Text = Convert.ToDateTime(reader["ReleaseDate"]).ToString("yyyy-MM-dd");

                                LogError($"? Movie loaded for edit: {movieID}");
                            }
                            else
                            {
                                LogError($"? No movie found with ID: {movieID}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"? ERROR in LoadMovieForEdit: {ex.Message}");
                LogError($"StackTrace: {ex.StackTrace}");
                ShowMessage($"Error loading movie: {ex.Message}", true);
            }
        }

        private void DeleteMovie(int movieID)
        {
            try
            {
                LogError($"DeleteMovie called with movieID: {movieID}");
                
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    LogError("Connection opened for delete operation");

                    string query = "DELETE FROM Movie WHERE MovieID = :movieid";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":movieid", OracleDbType.Int32).Value = movieID;
                        int rowsAffected = cmd.ExecuteNonQuery();
                        
                        LogError($"? Movie deleted successfully. Rows affected: {rowsAffected}");
                        ShowMessage("Movie deleted successfully!", false);
                        LoadMovies(txtSearch.Text.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"? ERROR in DeleteMovie: {ex.Message}");
                LogError($"StackTrace: {ex.StackTrace}");
                ShowMessage("Cannot delete movie. It may be linked to shows.", true);
            }
        }

        private void ClearForm()
        {
            txtTitle.Text = "";
            txtDuration.Text = "";
            ddlGenre.SelectedIndex = 0;
            ddlLanguage.SelectedIndex = 0;
            txtReleaseDate.Text = "";
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isError ? "alert alert-danger" : "alert alert-success";
            lblMessage.Visible = true;
        }
    }

    /// <summary>
    /// Standalone business rule validator utility class
    /// </summary>
    public static class BusinessRuleValidator
    {
        public static ValidationResult ValidateMovieTitleUnique(string connectionString, string title, int? excludeMovieID = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                return new ValidationResult { IsValid = false, ErrorMessage = "Movie title cannot be empty." };

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Movie WHERE UPPER(TRIM(Title)) = UPPER(TRIM(:title))";
                    if (excludeMovieID.HasValue) query += " AND MovieID != :movieid";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = title.Trim();
                        if (excludeMovieID.HasValue) cmd.Parameters.Add(":movieid", OracleDbType.Int32).Value = excludeMovieID.Value;

                        int count = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                        if (count > 0)
                            return new ValidationResult { IsValid = false, ErrorMessage = $"Movie '{title}' already exists. Movie titles must be unique." };
                    }
                }
                catch (Exception ex)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = $"Database error: {ex.Message}" };
                }
            }
            return new ValidationResult { IsValid = true, ErrorMessage = "" };
        }

        public static ValidationResult ValidateTheatreNameInCity(string connectionString, string theatreName, string city, int? excludeTheatreID = null)
        {
            if (string.IsNullOrWhiteSpace(theatreName) || string.IsNullOrWhiteSpace(city))
                return new ValidationResult { IsValid = false, ErrorMessage = "Theatre name and city are required." };

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Theatre WHERE UPPER(TRIM(TheatreName)) = UPPER(TRIM(:name)) AND UPPER(TRIM(City)) = UPPER(TRIM(:city))";
                    if (excludeTheatreID.HasValue) query += " AND TheatreID != :theatreid";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = theatreName.Trim();
                        cmd.Parameters.Add(":city", OracleDbType.Varchar2).Value = city.Trim();
                        if (excludeTheatreID.HasValue) cmd.Parameters.Add(":theatreid", OracleDbType.Int32).Value = excludeTheatreID.Value;

                        int count = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                        if (count > 0)
                            return new ValidationResult { IsValid = false, ErrorMessage = $"Theatre '{theatreName}' already exists in {city}." };
                    }
                }
                catch (Exception ex)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = $"Database error: {ex.Message}" };
                }
            }
            return new ValidationResult { IsValid = true, ErrorMessage = "" };
        }

        public static ValidationResult ValidateEmailUnique(string connectionString, string email, int? excludeUserID = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return new ValidationResult { IsValid = false, ErrorMessage = "Email cannot be empty." };

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Users WHERE UPPER(TRIM(Email)) = UPPER(TRIM(:email)) OR UPPER(TRIM(UserEmail)) = UPPER(TRIM(:email))";
                    if (excludeUserID.HasValue) query += " AND UserID != :userid";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":email", OracleDbType.Varchar2).Value = email.Trim();
                        if (excludeUserID.HasValue) cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = excludeUserID.Value;

                        int count = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                        if (count > 0)
                            return new ValidationResult { IsValid = false, ErrorMessage = $"Email '{email}' is already registered." };
                    }
                }
                catch { }
            }
            return new ValidationResult { IsValid = true, ErrorMessage = "" };
        }

        public static ValidationResult ValidatePhoneNumberUnique(string connectionString, string phoneNumber, int? excludeUserID = null)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new ValidationResult { IsValid = false, ErrorMessage = "Phone number cannot be empty." };

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Users WHERE (UPPER(TRIM(ContactNumber)) = UPPER(TRIM(:phone)) OR UPPER(TRIM(PhoneNumber)) = UPPER(TRIM(:phone)))";
                    if (excludeUserID.HasValue) query += " AND UserID != :userid";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":phone", OracleDbType.Varchar2).Value = phoneNumber.Trim();
                        if (excludeUserID.HasValue) cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = excludeUserID.Value;

                        int count = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                        if (count > 0)
                            return new ValidationResult { IsValid = false, ErrorMessage = $"Phone '{phoneNumber}' is already registered." };
                    }
                }
                catch { }
            }
            return new ValidationResult { IsValid = true, ErrorMessage = "" };
        }

        public static ValidationResult ValidateTicketCancellationTiming(string connectionString, int ticketID)
        {
            if (ticketID <= 0)
                return new ValidationResult { IsValid = false, ErrorMessage = "Invalid ticket ID." };

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT S.ShowDate, S.StartTime, S.ShowTime, T.TicketStatus FROM Ticket T INNER JOIN Booking B ON T.BookingID = B.BookingID INNER JOIN Shows S ON B.ShowID = S.ShowID WHERE T.TicketID = :ticketid";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":ticketid", OracleDbType.Int32).Value = ticketID;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                return new ValidationResult { IsValid = false, ErrorMessage = "Ticket not found." };

                            string ticketStatus = reader["TicketStatus"]?.ToString() ?? "";
                            if (ticketStatus.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
                                return new ValidationResult { IsValid = false, ErrorMessage = "Ticket already cancelled." };

                            DateTime showDate;
                            object showDateVal = reader["ShowDate"];
                            if (showDateVal is DateTime dt)
                                showDate = dt.Date;
                            else if (showDateVal != null && showDateVal != DBNull.Value && DateTime.TryParse(showDateVal.ToString(), out DateTime parsedDate))
                                showDate = parsedDate.Date;
                            else
                                return new ValidationResult { IsValid = false, ErrorMessage = "Invalid show date." };

                            // Determine show time of day robustly from StartTime / ShowTime (which may be DATE or string)
                            TimeSpan showTimeOfDay = TimeSpan.Zero;
                            object startVal = reader["StartTime"] != DBNull.Value ? reader["StartTime"] : reader["ShowTime"];
                            if (startVal is DateTime dtTime)
                            {
                                showTimeOfDay = dtTime.TimeOfDay;
                            }
                            else if (startVal != null && startVal != DBNull.Value)
                            {
                                var raw = startVal.ToString().Trim();
                                if (DateTime.TryParse(raw, out DateTime parsedDateTime))
                                {
                                    showTimeOfDay = parsedDateTime.TimeOfDay;
                                }
                                else if (!string.IsNullOrEmpty(raw))
                                {
                                    // Last‑resort parsing: split numeric components
                                    int hours = 0, minutes = 0;
                                    string[] timeParts = raw.Split(new[] { ':', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (timeParts.Length >= 2)
                                    {
                                        int.TryParse(timeParts[0], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out hours);
                                        int.TryParse(timeParts[1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out minutes);
                                    }
                                    else if (timeParts.Length == 1 && decimal.TryParse(timeParts[0], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal hourDec))
                                    {
                                        hours = (int)Math.Floor(hourDec);
                                        minutes = (int)Math.Round((hourDec - hours) * 60);
                                    }
                                    showTimeOfDay = new TimeSpan(hours, minutes, 0);
                                }
                            }

                            DateTime showStartDateTime = showDate.Add(showTimeOfDay);

                            // Use Nepal Standard Time for cancellation rule so it matches Kathmandu local time
                            DateTime currentTime;
                            try
                            {
                                var nepalTz = TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");
                                currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, nepalTz);
                            }
                            catch
                            {
                                // Fallback to server local time if Nepal time zone is not available
                                currentTime = DateTime.Now;
                            }
                            TimeSpan timeUntilShow = showStartDateTime - currentTime;

                            if (timeUntilShow.TotalMinutes <= 60)
                            {
                                int minutesRemaining = (int)Math.Ceiling(timeUntilShow.TotalMinutes);
                                if (minutesRemaining < 0)
                                    return new ValidationResult { IsValid = false, ErrorMessage = "Show already started. Cannot cancel." };
                                else
                                    return new ValidationResult { IsValid = false, ErrorMessage = $"Cancellation window closed. Only {minutesRemaining} min left. Must be 1+ hour before show." };
                            }

                            int minutesBeforeShow = (int)Math.Floor(timeUntilShow.TotalMinutes);
                            return new ValidationResult { IsValid = true, ErrorMessage = $"Cancellation allowed ({minutesBeforeShow} min until show)." };
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = $"Database error: {ex.Message}" };
                }
            }
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = "";
    }
}
