using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinema
{
    public partial class BookingDetails : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;

        private string SchemaPrefix
        {
            get
            {
                string s = ConfigurationManager.AppSettings["OracleSchema"];
                return string.IsNullOrWhiteSpace(s) ? "" : s.Trim().ToUpperInvariant() + ".";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTheatres();
                LoadCustomers();
                LoadBookings();
                ddlHall.Enabled = false;
                ddlShow.Enabled = false;
            }
        }

        private void LoadTheatres()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand("SELECT TheatreID, TheatreName FROM " + SchemaPrefix + "Theatre ORDER BY TheatreName", conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    ddlTheatre.DataSource = dt;
                    ddlTheatre.DataTextField = "TheatreName";
                    ddlTheatre.DataValueField = "TheatreID";
                    ddlTheatre.DataBind();
                    ddlTheatre.Items.Insert(0, new ListItem("-- Select Theatre --", ""));
                }
                catch (Exception ex) { ShowOracleMessage(ex, "loading theatres"); }
            }
        }

        protected void ddlTheatre_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlHall.Items.Clear();
            ddlShow.Items.Clear();
            pnlSeats.Visible = false;
            pnlPrice.Visible = false;
            chkSeats.Items.Clear();
            if (string.IsNullOrEmpty(ddlTheatre.SelectedValue)) { ddlHall.Enabled = false; ddlShow.Enabled = false; return; }
            ddlHall.Enabled = true;
            LoadHallsByTheatre(Convert.ToInt32(ddlTheatre.SelectedValue));
        }

        private void LoadHallsByTheatre(int theatreId)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand("SELECT HallID, HallName FROM " + SchemaPrefix + "Hall WHERE TheatreID = :tid ORDER BY HallName", conn))
                    {
                        cmd.Parameters.Add(":tid", OracleDbType.Int32).Value = theatreId;
                        using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                    ddlHall.DataSource = dt;
                    ddlHall.DataTextField = "HallName";
                    ddlHall.DataValueField = "HallID";
                    ddlHall.DataBind();
                    ddlHall.Items.Insert(0, new ListItem("-- Select Hall --", ""));
                }
                catch (Exception ex) { ShowOracleMessage(ex, "loading halls"); }
            }
        }

        protected void ddlHall_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlShow.Items.Clear();
            pnlSeats.Visible = false;
            pnlPrice.Visible = false;
            chkSeats.Items.Clear();
            if (string.IsNullOrEmpty(ddlHall.SelectedValue)) { ddlShow.Enabled = false; return; }
            ddlShow.Enabled = true;
            LoadShowsByHall(Convert.ToInt32(ddlHall.SelectedValue));
        }

        private void LoadShowsByHall(int hallId)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    string q = "SELECT S.ShowID, M.Title || ' (' || TO_CHAR(S.ShowDate, 'YYYY-MM-DD') || ' ' || TO_CHAR(NVL(S.StartTime, S.ShowTime), 'HH24:MI') || ')' AS ShowInfo " +
                               "FROM " + SchemaPrefix + "Shows S INNER JOIN " + SchemaPrefix + "Movie M ON S.MovieID = M.MovieID " +
                               "WHERE S.HallID = :hallid AND S.ShowDate >= TRUNC(SYSDATE) ORDER BY S.ShowDate, NVL(S.StartTime, S.ShowTime)";
                    using (OracleCommand cmd = new OracleCommand(q, conn))
                    {
                        cmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = hallId;
                        using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                    ddlShow.DataSource = dt;
                    ddlShow.DataTextField = "ShowInfo";
                    ddlShow.DataValueField = "ShowID";
                    ddlShow.DataBind();
                    ddlShow.Items.Insert(0, new ListItem("-- Select Show --", ""));
                }
                catch (Exception ex) { ShowOracleMessage(ex, "loading shows"); }
            }
        }

        protected void ddlShow_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlSeats.Visible = false;
            chkSeats.Items.Clear();
            if (string.IsNullOrEmpty(ddlShow.SelectedValue)) { pnlPrice.Visible = false; return; }
            int showId = Convert.ToInt32(ddlShow.SelectedValue);
            RefreshPriceDisplay(showId);
            int n = 0;
            int.TryParse(ddlNumTickets.SelectedValue, out n);
            if (n > 0 && !string.IsNullOrEmpty(ddlHall.SelectedValue))
            {
                LoadSeatsForHall(Convert.ToInt32(ddlHall.SelectedValue), n);
            }
        }

        protected void ddlNumTickets_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = 0;
            int.TryParse(ddlNumTickets.SelectedValue, out n);
            if (n > 0 && !string.IsNullOrEmpty(ddlHall.SelectedValue))
            {
                LoadSeatsForHall(Convert.ToInt32(ddlHall.SelectedValue), n);
            }
            if (!string.IsNullOrEmpty(ddlShow.SelectedValue))
            {
                RefreshPriceDisplay(Convert.ToInt32(ddlShow.SelectedValue));
            }
            else
            {
                pnlSeats.Visible = false;
            }
        }

        private void RefreshPriceDisplay(int showId)
        {
            decimal pricePerTicket = 0;
            int priceId = 0;
            GetPriceForShow(showId, out pricePerTicket, out priceId);
            ViewState["PriceID"] = priceId;
            int n = 0;
            int.TryParse(ddlNumTickets.SelectedValue, out n);
            decimal total = pricePerTicket * (n > 0 ? n : 1);
            lblPricePerTicket.Text = "Rs " + pricePerTicket.ToString("N2");
            lblTotalAmount.Text = "Rs " + total.ToString("N2");
            ViewState["PricePerTicket"] = pricePerTicket;
            pnlPrice.Visible = true;
        }

        /// <summary>Gets price per ticket for a show based on release week and festival. Sets priceId and pricePerTicket.</summary>
        private void GetPriceForShow(int showId, out decimal pricePerTicket, out int priceId)
        {
            pricePerTicket = 0;
            priceId = 0;
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    DateTime showDate = DateTime.MinValue;
                    DateTime releaseDate = DateTime.MinValue;
                    using (OracleCommand cmd = new OracleCommand(
                        "SELECT S.ShowDate, S.StartTime, S.ShowTime, M.ReleaseDate FROM " + SchemaPrefix + "Shows S INNER JOIN " + SchemaPrefix + "Movie M ON S.MovieID = M.MovieID WHERE S.ShowID = :showid", conn))
                    {
                        cmd.Parameters.Add(":showid", OracleDbType.Int32).Value = showId;
                        using (OracleDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                showDate = r["ShowDate"] != DBNull.Value ? Convert.ToDateTime(r["ShowDate"]) : DateTime.MinValue;
                                if (r["ReleaseDate"] != DBNull.Value) releaseDate = Convert.ToDateTime(r["ReleaseDate"]);
                            }
                        }
                    }
                    bool isReleaseWeek = (showDate.Date - releaseDate.Date).TotalDays <= 7 && releaseDate != DateTime.MinValue;
                    bool isFestival = false;
                    try
                    {
                        using (OracleCommand cmd = new OracleCommand("SELECT COUNT(*) FROM " + SchemaPrefix + "FestivalDate WHERE TRUNC(FestivalDate) = TRUNC(:d)", conn))
                        {
                            cmd.Parameters.Add(":d", OracleDbType.Date).Value = showDate;
                            object c = cmd.ExecuteScalar();
                            isFestival = Convert.ToInt32(c) > 0;
                        }
                    }
                    catch { /* FestivalDate table may not exist */ }
                    int rp = isReleaseWeek ? 1 : 0;
                    int fs = isFestival ? 1 : 0;
                    using (OracleCommand cmd = new OracleCommand(
                        "SELECT PriceID, PriceAmount FROM " + SchemaPrefix + "Price WHERE NVL(IsReleasePeriod,0) = :rp AND NVL(IsFestiveSeason,0) = :fs AND ROWNUM = 1", conn))
                    {
                        cmd.Parameters.Add(":rp", OracleDbType.Int32).Value = rp;
                        cmd.Parameters.Add(":fs", OracleDbType.Int32).Value = fs;
                        using (OracleDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                priceId = Convert.ToInt32(r["PriceID"]);
                                pricePerTicket = r["PriceAmount"] != DBNull.Value ? Convert.ToDecimal(r["PriceAmount"]) : 0;
                            }
                        }
                    }
                    if (priceId == 0)
                    {
                        using (OracleCommand cmd = new OracleCommand("SELECT PriceID, PriceAmount FROM " + SchemaPrefix + "Price WHERE ROWNUM = 1", conn))
                        using (OracleDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                priceId = Convert.ToInt32(r["PriceID"]);
                                pricePerTicket = r["PriceAmount"] != DBNull.Value ? Convert.ToDecimal(r["PriceAmount"]) : 0;
                            }
                        }
                    }
                }
                catch (Exception ex) { ShowOracleMessage(ex, "loading price"); }
            }
        }

        private void LoadSeatsForHall(int hallId, int numTickets)
        {
            pnlSeats.Visible = true;
            litSeatCount.Text = numTickets.ToString();
            chkSeats.Items.Clear();
            string seatQuery = "SELECT SeatID, SeatRow || '-' || SeatNumber AS SeatLabel FROM " + SchemaPrefix + "Seat WHERE IsAvailable = 1 ORDER BY SeatRow, SeatNumber";
            try
            {
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    try
                    {
                        seatQuery = "SELECT SeatID, SeatRow || '-' || SeatNumber AS SeatLabel FROM " + SchemaPrefix + "Seat WHERE HallID = :hallid AND IsAvailable = 1 ORDER BY SeatRow, SeatNumber";
                        using (OracleCommand cmd = new OracleCommand(seatQuery, conn))
                        {
                            cmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = hallId;
                            using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);
                                foreach (DataRow row in dt.Rows)
                                {
                                    chkSeats.Items.Add(new ListItem(row["SeatLabel"].ToString(), row["SeatID"].ToString()));
                                }
                            }
                        }
                    }
                    catch
                    {
                        seatQuery = "SELECT SeatID, SeatRow || '-' || SeatNumber AS SeatLabel FROM " + SchemaPrefix + "Seat WHERE IsAvailable = 1 ORDER BY SeatRow, SeatNumber";
                        using (OracleCommand cmd = new OracleCommand(seatQuery, conn))
                        using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            foreach (DataRow row in dt.Rows)
                            {
                                chkSeats.Items.Add(new ListItem(row["SeatLabel"].ToString(), row["SeatID"].ToString()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ShowOracleMessage(ex, "loading seats"); }
        }

        private void LoadCustomers()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand("SELECT UserID, UserName FROM " + SchemaPrefix + "UserTable ORDER BY UserName", conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    ddlCustomer.DataSource = dt;
                    ddlCustomer.DataTextField = "UserName";
                    ddlCustomer.DataValueField = "UserID";
                    ddlCustomer.DataBind();
                    ddlCustomer.Items.Insert(0, new ListItem("-- Select Customer --", ""));
                }
                catch (Exception ex) { ShowOracleMessage(ex, "loading customers"); }
            }
        }

        private void LoadBookings()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string q = "SELECT B.BookingID, B.ConfirmationCode, NVL(U.UserName, 'Guest') AS UserName, " +
                               "T.TheatreName, H.HallName, M.Title, S.ShowDate, TO_CHAR(NVL(S.StartTime, S.ShowTime), 'HH24:MI') AS ShowTime, " +
                               "NVL(P.PaymentAmount, 0) AS PaymentAmount " +
                               "FROM " + SchemaPrefix + "Booking B " +
                               "LEFT JOIN " + SchemaPrefix + "UserTable U ON B.UserID = U.UserID " +
                               "LEFT JOIN " + SchemaPrefix + "Payment P ON B.PaymentID = P.PaymentID " +
                               "INNER JOIN " + SchemaPrefix + "Shows S ON B.ShowID = S.ShowID " +
                               "INNER JOIN " + SchemaPrefix + "Movie M ON S.MovieID = M.MovieID " +
                               "INNER JOIN " + SchemaPrefix + "Hall H ON S.HallID = H.HallID " +
                               "INNER JOIN " + SchemaPrefix + "Theatre T ON H.TheatreID = T.TheatreID " +
                               "ORDER BY B.BookingID DESC";
                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(q, conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    gvBookings.DataSource = dt;
                    gvBookings.DataBind();
                }
                catch (Exception ex) { ShowOracleMessage(ex, "loading bookings"); }
            }
        }

        private void ShowOracleMessage(Exception ex, string action)
        {
            int oraCode = 0;
            OracleException ora = ex as OracleException;
            if (ora != null) oraCode = ora.Number;
            else if (ex.InnerException is OracleException)
                oraCode = ((OracleException)ex.InnerException).Number;
            if (oraCode == 942)
            {
                ShowMessage(
                    "Database tables not found (ORA-00942). Make sure all KumariCinema tables " +
                    "(UserTable, Payment, Movie, Theatre, Hall, Shows, Seat, Price, Cancellation, Booking, Ticket) " +
                    "exist in the same Oracle user as your connection string (cinema). If needed, run CREATE_TABLES_EXACT.sql as that user.",
                    true);
            }
            else
            {
                ShowMessage("Error (" + action + "): " + ex.Message, true);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (ViewState["EditBookingID"] != null)
            {
                int bookingId = (int)ViewState["EditBookingID"];
                if (string.IsNullOrEmpty(ddlCustomer.SelectedValue) || string.IsNullOrEmpty(ddlShow.SelectedValue)) { ShowMessage("Select customer and show.", true); return; }
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (OracleCommand cmd = new OracleCommand("UPDATE " + SchemaPrefix + "Booking SET UserID = :userid, ShowID = :showid WHERE BookingID = :bid", conn))
                        {
                            cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = Convert.ToInt32(ddlCustomer.SelectedValue);
                            cmd.Parameters.Add(":showid", OracleDbType.Int32).Value = Convert.ToInt32(ddlShow.SelectedValue);
                            cmd.Parameters.Add(":bid", OracleDbType.Int32).Value = bookingId;
                            cmd.ExecuteNonQuery();
                        }
                        ShowMessage("Booking updated.", false);
                        ViewState["EditBookingID"] = null;
                        LoadBookings();
                        ClearForm();
                    }
                    catch (Exception ex) { ShowOracleMessage(ex, "updating booking"); }
                }
                return;
            }

            if (string.IsNullOrEmpty(ddlCustomer.SelectedValue)) { ShowMessage("Select customer.", true); return; }
            if (string.IsNullOrEmpty(ddlTheatre.SelectedValue)) { ShowMessage("Select theatre.", true); return; }
            if (string.IsNullOrEmpty(ddlHall.SelectedValue)) { ShowMessage("Select hall.", true); return; }
            if (string.IsNullOrEmpty(ddlShow.SelectedValue)) { ShowMessage("Select show.", true); return; }
            int numTickets = 0;
            if (!int.TryParse(ddlNumTickets.SelectedValue, out numTickets) || numTickets < 1) { ShowMessage("Select number of tickets.", true); return; }
            if (string.IsNullOrEmpty(ddlPaymentMethod.SelectedValue)) { ShowMessage("Select payment method.", true); return; }
            int selectedSeats = 0;
            foreach (ListItem item in chkSeats.Items)
                if (item.Selected) selectedSeats++;
            if (selectedSeats != numTickets) { ShowMessage("Select exactly " + numTickets + " seat(s).", true); return; }

            int showId = Convert.ToInt32(ddlShow.SelectedValue);
            int userId = Convert.ToInt32(ddlCustomer.SelectedValue);
            decimal pricePerTicket = ViewState["PricePerTicket"] != null ? (decimal)ViewState["PricePerTicket"] : 0;
            int priceId = ViewState["PriceID"] != null ? (int)ViewState["PriceID"] : 0;
            decimal totalAmount = pricePerTicket * numTickets;

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    OracleTransaction txn = conn.BeginTransaction();
                    try
                    {
                        int paymentId = 1;
                        using (OracleCommand cmdMax = new OracleCommand("SELECT NVL(MAX(PaymentID), 0) + 1 FROM " + SchemaPrefix + "Payment", conn))
                        {
                            cmdMax.Transaction = txn;
                            object o = cmdMax.ExecuteScalar();
                            paymentId = Convert.ToInt32(o);
                        }
                        using (OracleCommand cmd = new OracleCommand(
                            "INSERT INTO " + SchemaPrefix + "Payment (PaymentID, PaymentAmount, PaymentStatus, PaymentMode) VALUES (:pid, :amt, 'COMPLETED', :paymode)", conn))
                        {
                            cmd.Transaction = txn;
                            cmd.Parameters.Add(":pid", OracleDbType.Int32).Value = paymentId;
                            cmd.Parameters.Add(":amt", OracleDbType.Decimal).Value = totalAmount;
                            cmd.Parameters.Add(":paymode", OracleDbType.Varchar2).Value = ddlPaymentMethod.SelectedValue;
                            cmd.ExecuteNonQuery();
                        }

                        int bookingId = 1;
                        using (OracleCommand cmdMax = new OracleCommand("SELECT NVL(MAX(BookingID), 0) + 1 FROM " + SchemaPrefix + "Booking", conn))
                        {
                            cmdMax.Transaction = txn;
                            object o = cmdMax.ExecuteScalar();
                            bookingId = Convert.ToInt32(o);
                        }
                        string confirmCode = "BOOK-" + DateTime.Now.Year + "-" + bookingId.ToString().PadLeft(6, '0');
                        using (OracleCommand cmd = new OracleCommand(
                            "INSERT INTO " + SchemaPrefix + "Booking (BookingID, ConfirmationCode, ShowID, UserID, PaymentID) VALUES (:bid, :confirmcode, :showid, :userid, :pid)", conn))
                        {
                            cmd.Transaction = txn;
                            cmd.Parameters.Add(":bid", OracleDbType.Int32).Value = bookingId;
                            cmd.Parameters.Add(":confirmcode", OracleDbType.Varchar2).Value = confirmCode;
                            cmd.Parameters.Add(":showid", OracleDbType.Int32).Value = showId;
                            cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = userId;
                            cmd.Parameters.Add(":pid", OracleDbType.Int32).Value = paymentId;
                            cmd.ExecuteNonQuery();
                        }

                        int ticketId = 0;
                        using (OracleCommand cmdMax = new OracleCommand("SELECT NVL(MAX(TicketID), 0) FROM " + SchemaPrefix + "Ticket", conn))
                        {
                            cmdMax.Transaction = txn;
                            object o = cmdMax.ExecuteScalar();
                            ticketId = Convert.ToInt32(o);
                        }
                        foreach (ListItem item in chkSeats.Items)
                        {
                            if (!item.Selected) continue;
                            int seatId = Convert.ToInt32(item.Value);
                            ticketId++;
                            string ticketNumber = "TKT-" + DateTime.Now.Year + "-" + ticketId.ToString().PadLeft(6, '0');
                            object priceIdVal = priceId > 0 ? (object)priceId : DBNull.Value;
                            using (OracleCommand cmdTicket = new OracleCommand(
                                "INSERT INTO " + SchemaPrefix + "Ticket (TicketID, TicketStatus, TicketNumber, SeatID, CancellationID, PriceID, BookingID) VALUES (:tid, 'CONFIRMED', :tnum, :seatid, NULL, :priceid, :bid)", conn))
                            {
                                cmdTicket.Transaction = txn;
                                cmdTicket.Parameters.Add(":tid", OracleDbType.Int32).Value = ticketId;
                                cmdTicket.Parameters.Add(":tnum", OracleDbType.Varchar2).Value = ticketNumber;
                                cmdTicket.Parameters.Add(":seatid", OracleDbType.Int32).Value = seatId;
                                cmdTicket.Parameters.Add(":priceid", OracleDbType.Int32).Value = priceIdVal;
                                cmdTicket.Parameters.Add(":bid", OracleDbType.Int32).Value = bookingId;
                                cmdTicket.ExecuteNonQuery();
                            }
                            using (OracleCommand cmdSeat = new OracleCommand("UPDATE " + SchemaPrefix + "Seat SET IsAvailable = 0 WHERE SeatID = :sid", conn))
                            {
                                cmdSeat.Transaction = txn;
                                cmdSeat.Parameters.Add(":sid", OracleDbType.Int32).Value = seatId;
                                cmdSeat.ExecuteNonQuery();
                            }
                        }
                        txn.Commit();
                        ShowMessage("Booking confirmed! " + confirmCode, false);
                        LoadBookings();
                        ClearForm();
                    }
                    catch
                    {
                        txn.Rollback();
                        throw;
                    }
                }
                catch (Exception ex) { ShowOracleMessage(ex, "saving booking"); }
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            ViewState["EditBookingID"] = null;
        }

        protected void gvBookings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int bookingID = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditBooking") LoadBookingForEdit(bookingID);
            else if (e.CommandName == "DeleteBooking") DeleteBooking(bookingID);
        }

        protected void gvBookings_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBookings.PageIndex = e.NewPageIndex;
            LoadBookings();
        }

        private void LoadBookingForEdit(int bookingID)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string q = "SELECT B.UserID, B.ShowID, S.HallID, H.TheatreID FROM " + SchemaPrefix + "Booking B " +
                               "INNER JOIN " + SchemaPrefix + "Shows S ON B.ShowID = S.ShowID INNER JOIN " + SchemaPrefix + "Hall H ON S.HallID = H.HallID WHERE B.BookingID = :bid";
                    using (OracleCommand cmd = new OracleCommand(q, conn))
                    {
                        cmd.Parameters.Add(":bid", OracleDbType.Int32).Value = bookingID;
                        using (OracleDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                ViewState["EditBookingID"] = bookingID;
                                ddlTheatre.SelectedValue = r["TheatreID"].ToString();
                                ddlTheatre_SelectedIndexChanged(null, null);
                                ddlHall.SelectedValue = r["HallID"].ToString();
                                ddlHall_SelectedIndexChanged(null, null);
                                ddlShow.SelectedValue = r["ShowID"].ToString();
                                ddlCustomer.SelectedValue = r["UserID"].ToString();
                                pnlPrice.Visible = true;
                                RefreshPriceDisplay(Convert.ToInt32(r["ShowID"]));
                            }
                        }
                    }
                }
                catch (Exception ex) { ShowOracleMessage(ex, "loading for edit"); }
            }
        }

        private void DeleteBooking(int bookingID)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    OracleTransaction txn = conn.BeginTransaction();
                    try
                    {
                        using (OracleCommand cmd = new OracleCommand("SELECT SeatID FROM " + SchemaPrefix + "Ticket WHERE BookingID = :bid", conn))
                        {
                            cmd.Transaction = txn;
                            cmd.Parameters.Add(":bid", OracleDbType.Int32).Value = bookingID;
                            using (OracleDataReader r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    int seatId = Convert.ToInt32(r["SeatID"]);
                                    using (OracleCommand up = new OracleCommand("UPDATE " + SchemaPrefix + "Seat SET IsAvailable = 1 WHERE SeatID = :sid", conn))
                                    {
                                        up.Transaction = txn;
                                        up.Parameters.Add(":sid", OracleDbType.Int32).Value = seatId;
                                        up.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        using (OracleCommand cmd = new OracleCommand("DELETE FROM " + SchemaPrefix + "Ticket WHERE BookingID = :bid", conn))
                        {
                            cmd.Transaction = txn;
                            cmd.Parameters.Add(":bid", OracleDbType.Int32).Value = bookingID;
                            cmd.ExecuteNonQuery();
                        }
                        int? paymentId = null;
                        using (OracleCommand cmd = new OracleCommand("SELECT PaymentID FROM " + SchemaPrefix + "Booking WHERE BookingID = :bid", conn))
                        {
                            cmd.Transaction = txn;
                            cmd.Parameters.Add(":bid", OracleDbType.Int32).Value = bookingID;
                            object o = cmd.ExecuteScalar();
                            if (o != null && o != DBNull.Value) paymentId = Convert.ToInt32(o);
                        }
                        using (OracleCommand cmd = new OracleCommand("DELETE FROM " + SchemaPrefix + "Booking WHERE BookingID = :bid", conn))
                        {
                            cmd.Transaction = txn;
                            cmd.Parameters.Add(":bid", OracleDbType.Int32).Value = bookingID;
                            cmd.ExecuteNonQuery();
                        }
                        if (paymentId.HasValue)
                        {
                            using (OracleCommand cmd = new OracleCommand("DELETE FROM " + SchemaPrefix + "Payment WHERE PaymentID = :pid", conn))
                            {
                                cmd.Transaction = txn;
                                cmd.Parameters.Add(":pid", OracleDbType.Int32).Value = paymentId.Value;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        txn.Commit();
                        ShowMessage("Booking cancelled.", false);
                        LoadBookings();
                    }
                    catch { txn.Rollback(); throw; }
                }
                catch (Exception ex) { ShowOracleMessage(ex, "cancelling booking"); }
            }
        }

        private void ClearForm()
        {
            ddlTheatre.SelectedIndex = 0;
            ddlHall.Items.Clear();
            ddlHall.Enabled = false;
            ddlShow.Items.Clear();
            ddlShow.Enabled = false;
            ddlCustomer.SelectedIndex = 0;
            ddlNumTickets.SelectedIndex = 0;
            ddlPaymentMethod.SelectedIndex = 0;
            chkSeats.Items.Clear();
            pnlSeats.Visible = false;
            pnlPrice.Visible = false;
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isError ? "alert alert-danger" : "alert alert-success";
            lblMessage.Visible = true;
        }
    }
}
