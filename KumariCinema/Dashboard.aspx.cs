using System;
using System.Configuration;
using System.Data;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinema
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private string connectionString = 
            ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                System.Diagnostics.Debug.WriteLine("=== Loading Dashboard ===");
                try
                {
                    LoadStatistics();
                    LoadPaymentStatistics();
                    LoadTicketStatistics();
                    LoadNowShowing();
                    LoadRecentBookings();
                    LoadHalls();
                    LoadUpcomingShows();
                    
                    lblRefreshTime.Text = $"Last updated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    System.Diagnostics.Debug.WriteLine("? Dashboard loaded successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? Error loading dashboard: {ex.Message}");
                }
            }
        }

        private void LoadStatistics()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // 1. Total Movies
                    int totalMovies = GetCount(conn, "SELECT COUNT(*) FROM Movie");
                    lblTotalMovies.Text = totalMovies.ToString();

                    // Movies added this month
                    int newMovies = GetCount(conn, 
                        "SELECT COUNT(*) FROM Movie WHERE TRUNC(ReleaseDate, 'MM') = TRUNC(SYSDATE, 'MM')");
                    lblMoviesChange.Text = $"+{newMovies} this month";

                    // 2. Total Theatres
                    int totalTheatres = GetCount(conn, "SELECT COUNT(*) FROM Theatre");
                    lblTotalTheatres.Text = totalTheatres.ToString();
                    lblTheatresChange.Text = $"{totalTheatres} active";

                    // 3. Showtimes Today
                    int showsToday = GetCount(conn, 
                        "SELECT COUNT(*) FROM Shows WHERE TRUNC(ShowDate) = TRUNC(SYSDATE)");
                    lblShowsToday.Text = showsToday.ToString();
                    lblShowsChange.Text = "Today";

                    // 4. Tickets Sold Today
                    int ticketsSold = GetCount(conn,
                        @"SELECT COUNT(*) FROM Ticket T
                          INNER JOIN Booking B ON T.BookingID = B.BookingID
                          INNER JOIN Shows S ON B.ShowID = S.ShowID
                          WHERE TRUNC(S.ShowDate) = TRUNC(SYSDATE)
                          AND UPPER(T.TicketStatus) = 'CONFIRMED'");
                    lblTicketsSold.Text = ticketsSold.ToString();
                    lblTicketsChange.Text = "Today";

                    // 5. Total Customers
                    int totalCustomers = GetCount(conn, "SELECT COUNT(*) FROM USERS");
                    lblTotalCustomers.Text = totalCustomers.ToString();
                    lblCustomersChange.Text = $"{totalCustomers} members";

                    // 6. Revenue
                    string revenueQuery = @"SELECT NVL(SUM(PaymentAmount), 0) FROM Payment 
                                          WHERE UPPER(PaymentStatus) = 'COMPLETED'";
                    using (OracleCommand cmd = new OracleCommand(revenueQuery, conn))
                    {
                        decimal revenue = Convert.ToDecimal(cmd.ExecuteScalar());
                        lblRevenue.Text = "Rs " + revenue.ToString("N0");
                    }
                    lblRevenueChange.Text = "Completed";

                    System.Diagnostics.Debug.WriteLine("? Statistics loaded");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? Error loading statistics: {ex.Message}");
                }
            }
        }

        private void LoadPaymentStatistics()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"SELECT 
                                    NVL(UPPER(TRIM(PaymentStatus)), 'UNKNOWN') AS STATUS,
                                    COUNT(*) AS COUNT,
                                    NVL(SUM(PaymentAmount), 0) AS AMOUNT
                                  FROM Payment
                                  GROUP BY UPPER(TRIM(PaymentStatus))
                                  ORDER BY COUNT(*) DESC";

                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    int completedCount = 0, pendingCount = 0, failedCount = 0;
                    decimal completedAmount = 0, pendingAmount = 0, failedAmount = 0;

                    var labels = new System.Collections.Generic.List<string>();
                    var values = new System.Collections.Generic.List<int>();

                    foreach (DataRow row in dt.Rows)
                    {
                        string status = row["STATUS"].ToString();
                        int count = Convert.ToInt32(row["COUNT"]);
                        decimal amount = Convert.ToDecimal(row["AMOUNT"]);

                        labels.Add(status);
                        values.Add(count);

                        if (status == "COMPLETED")
                        {
                            completedCount = count;
                            completedAmount = amount;
                        }
                        else if (status == "PENDING")
                        {
                            pendingCount = count;
                            pendingAmount = amount;
                        }
                        else if (status == "FAILED" || status == "CANCELLED")
                        {
                            failedCount += count;
                            failedAmount += amount;
                        }
                    }

                    // Update UI Labels
                    lblCompletedPayments.Text = completedCount.ToString();
                    lblCompletedAmount.Text = $"Rs {completedAmount:N0}";
                    lblPendingPayments.Text = pendingCount.ToString();
                    lblPendingAmount.Text = $"Rs {pendingAmount:N0}";
                    lblFailedPayments.Text = failedCount.ToString();
                    lblFailedAmount.Text = $"Rs {failedAmount:N0}";

                    // Generate JSON for Chart
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var chartData = new { labels = labels.ToArray(), values = values.ToArray() };
                    hdnPaymentData.Value = serializer.Serialize(chartData);

                    System.Diagnostics.Debug.WriteLine("? Payment statistics loaded");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? Error loading payment statistics: {ex.Message}");
                }
            }
        }

        private void LoadTicketStatistics()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"SELECT 
                                    NVL(UPPER(TRIM(TicketStatus)), 'UNKNOWN') AS STATUS,
                                    COUNT(*) AS COUNT
                                  FROM Ticket
                                  GROUP BY UPPER(TRIM(TicketStatus))
                                  ORDER BY COUNT(*) DESC";

                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    int activeCount = 0, usedCount = 0, cancelledCount = 0, totalCount = 0;

                    var labels = new System.Collections.Generic.List<string>();
                    var values = new System.Collections.Generic.List<int>();

                    foreach (DataRow row in dt.Rows)
                    {
                        string status = row["STATUS"].ToString();
                        int count = Convert.ToInt32(row["COUNT"]);

                        labels.Add(status);
                        values.Add(count);
                        totalCount += count;

                        if (status == "CONFIRMED" || status == "BOOKED" || status == "ACTIVE")
                            activeCount += count;
                        else if (status == "USED" || status == "COMPLETED")
                            usedCount += count;
                        else if (status == "CANCELLED")
                            cancelledCount += count;
                    }

                    // Update UI Labels
                    lblActiveTickets.Text = activeCount.ToString();
                    lblUsedTickets.Text = usedCount.ToString();
                    lblCancelledTickets.Text = cancelledCount.ToString();

                    // Cancellation Rate
                    if (totalCount > 0)
                    {
                        double cancelRate = (cancelledCount * 100.0) / totalCount;
                        lblCancelledRate.Text = $"{cancelRate:F1}%";
                    }
                    else
                    {
                        lblCancelledRate.Text = "N/A";
                    }

                    // Generate JSON for Chart
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var chartData = new { labels = labels.ToArray(), values = values.ToArray() };
                    hdnTicketData.Value = serializer.Serialize(chartData);

                    System.Diagnostics.Debug.WriteLine("? Ticket statistics loaded");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? Error loading ticket statistics: {ex.Message}");
                }
            }
        }

        private void LoadNowShowing()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"SELECT * FROM (
                                    SELECT MovieID, Title, Genre, Language, Duration, ReleaseDate
                                    FROM Movie
                                    WHERE ReleaseDate <= SYSDATE
                                    ORDER BY ReleaseDate DESC
                                )
                                WHERE ROWNUM <= 5";

                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    gvNowShowing.DataSource = dt;
                    gvNowShowing.DataBind();

                    System.Diagnostics.Debug.WriteLine($"? Now showing loaded: {dt.Rows.Count} movies");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? Error loading now showing: {ex.Message}");
                }
            }
        }

        private void LoadRecentBookings()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"SELECT * FROM (
                                    SELECT B.BookingID, B.ConfirmationCode, NVL(U.UserName, 'Guest') AS UserName, S.ShowDate AS ShowDate
                                    FROM Booking B
                                    LEFT JOIN USERS U ON B.UserID = U.UserID
                                    INNER JOIN Shows S ON B.ShowID = S.ShowID
                                    ORDER BY B.BookingID DESC
                                )
                                WHERE ROWNUM <= 5";

                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    gvRecentBookings.DataSource = dt;
                    gvRecentBookings.DataBind();

                    System.Diagnostics.Debug.WriteLine($"? Recent bookings loaded: {dt.Rows.Count} bookings");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? Error loading recent bookings: {ex.Message}");
                }
            }
        }

        private void LoadHalls()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"SELECT H.HallID, H.HallName, H.HallCapacity, H.HallType, T.TheatreName
                                   FROM Hall H
                                   INNER JOIN Theatre T ON H.TheatreID = T.TheatreID
                                   ORDER BY T.TheatreName, H.HallName";

                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    gvHalls.DataSource = dt;
                    gvHalls.DataBind();

                    System.Diagnostics.Debug.WriteLine($"? Halls loaded: {dt.Rows.Count} halls");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? Error loading halls: {ex.Message}");
                }
            }
        }

        private void LoadUpcomingShows()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"SELECT M.Title, S.ShowDate, TO_CHAR(S.ShowTime, 'HH24:MI') AS ShowTime, H.HallName
                                   FROM Shows S
                                   INNER JOIN Movie M ON S.MovieID = M.MovieID
                                   INNER JOIN Hall H ON S.HallID = H.HallID
                                   WHERE S.ShowDate >= TRUNC(SYSDATE)
                                   ORDER BY S.ShowDate, S.ShowTime";

                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    gvUpcomingShows.DataSource = dt;
                    gvUpcomingShows.DataBind();

                    System.Diagnostics.Debug.WriteLine($"? Upcoming shows loaded: {dt.Rows.Count} shows");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? Error loading upcoming shows: {ex.Message}");
                }
            }
        }

        private int GetCount(OracleConnection conn, string query)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand(query, conn))
                {
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error in GetCount: {ex.Message}");
                return 0;
            }
        }
    }
}
