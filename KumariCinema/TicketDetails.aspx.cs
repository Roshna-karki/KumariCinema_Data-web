using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinema
{
    public partial class TicketDetails : Page
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
                LoadTickets();
            }
        }

        private void LoadTickets()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string q = "SELECT T.TicketID, T.TicketNumber, T.TicketStatus, " +
                               "B.ConfirmationCode, NVL(U.UserName, 'Guest') AS UserName, " +
                               "TH.TheatreName, H.HallName, M.Title, S.ShowDate, TO_CHAR(NVL(S.StartTime, S.ShowTime), 'HH24:MI') AS ShowTime, " +
                               "ST.SeatRow || '-' || ST.SeatNumber AS SeatInfo " +
                               "FROM " + SchemaPrefix + "Ticket T " +
                               "INNER JOIN " + SchemaPrefix + "Booking B ON T.BookingID = B.BookingID " +
                               "LEFT JOIN " + SchemaPrefix + "UserTable U ON B.UserID = U.UserID " +
                               "INNER JOIN " + SchemaPrefix + "Shows S ON B.ShowID = S.ShowID " +
                               "INNER JOIN " + SchemaPrefix + "Movie M ON S.MovieID = M.MovieID " +
                               "INNER JOIN " + SchemaPrefix + "Hall H ON S.HallID = H.HallID " +
                               "INNER JOIN " + SchemaPrefix + "Theatre TH ON H.TheatreID = TH.TheatreID " +
                               "INNER JOIN " + SchemaPrefix + "Seat ST ON T.SeatID = ST.SeatID " +
                               "ORDER BY T.TicketID DESC";
                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(q, conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    gvTickets.DataSource = dt;
                    gvTickets.DataBind();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error: " + ex.Message, true);
                }
            }
        }

        protected void gvTickets_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "CancelTicket")
            {
                if (!TryParseTicketId(e.CommandArgument, out int ticketId))
                {
                    ShowMessage("Invalid ticket ID.", true);
                    return;
                }
                CancelTicket(ticketId);
            }
        }

        private static bool TryParseTicketId(object commandArgument, out int ticketId)
        {
            ticketId = 0;
            if (commandArgument == null) return false;
            if (commandArgument is int i) { ticketId = i; return true; }
            if (commandArgument is long l) { ticketId = (int)l; return true; }
            if (commandArgument is decimal d) { ticketId = (int)d; return true; }
            var s = commandArgument.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return false;
            if (int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int parsed))
            {
                ticketId = parsed;
                return true;
            }
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal dec) && dec == Math.Floor(dec))
            {
                ticketId = (int)dec;
                return true;
            }
            return false;
        }

        protected void gvTickets_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvTickets.PageIndex = e.NewPageIndex;
            LoadTickets();
        }

        private void CancelTicket(int ticketId)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // RULE 6: Validate ticket cancellation timing (CRITICAL BUSINESS RULE)
                    // Cancellation is ONLY allowed if show start time is MORE THAN 1 hour away
                    var cancellationValidation = BusinessRuleValidator.ValidateTicketCancellationTiming(connectionString, ticketId);
                    
                    if (!cancellationValidation.IsValid)
                    {
                        ShowMessage(cancellationValidation.ErrorMessage, true);
                        return;
                    }

                    // Validation passed - proceed with cancellation
                    int seatId = 0;
                    using (OracleCommand cmd = new OracleCommand("SELECT SeatID FROM " + SchemaPrefix + "Ticket WHERE TicketID = :tid", conn))
                    {
                        cmd.Parameters.Add(":tid", OracleDbType.Int32).Value = ticketId;
                        object o = cmd.ExecuteScalar();
                        if (o != null && o != DBNull.Value) seatId = Convert.ToInt32(o);
                    }

                    // Update ticket status to CANCELLED
                    using (OracleCommand cmd = new OracleCommand("UPDATE " + SchemaPrefix + "Ticket SET TicketStatus = 'CANCELLED' WHERE TicketID = :tid", conn))
                    {
                        cmd.Parameters.Add(":tid", OracleDbType.Int32).Value = ticketId;
                        cmd.ExecuteNonQuery();
                    }

                    // Release the seat back to available
                    if (seatId > 0)
                    {
                        using (OracleCommand cmd = new OracleCommand("UPDATE " + SchemaPrefix + "Seat SET IsAvailable = 1 WHERE SeatID = :sid", conn))
                        {
                            cmd.Parameters.Add(":sid", OracleDbType.Int32).Value = seatId;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    ShowMessage("Ticket cancelled successfully. Seat released for rebooking.", false);
                    LoadTickets();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error: " + ex.Message, true);
                }
            }
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isError ? "alert alert-danger" : "alert alert-success";
            lblMessage.Visible = true;
        }
    }
}
