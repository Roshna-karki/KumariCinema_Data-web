using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinema
{
    public partial class SeatDetails : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSeats();
            }
        }

        private void LoadSeats()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(
                        @"SELECT SeatID, SeatRow, SeatNumber, 
                                 CASE WHEN IsAvailable = 1 THEN 'Yes' ELSE 'No' END AS IsAvailable
                          FROM Seat ORDER BY SeatRow, SeatNumber", conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    gvSeats.DataSource = dt;
                    gvSeats.DataBind();
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlRow.SelectedValue)) { ShowMessage("Row required", true); return; }
            if (string.IsNullOrEmpty(txtSeatNumber.Text)) { ShowMessage("Seat Number required", true); return; }

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    int nextId = 1;
                    using (OracleCommand cmdMax = new OracleCommand("SELECT NVL(MAX(SeatID), 0) + 1 FROM Seat", conn))
                    {
                        object o = cmdMax.ExecuteScalar();
                        nextId = Convert.ToInt32(o);
                    }
                    string query = @"INSERT INTO Seat (SeatID, SeatRow, SeatNumber, IsAvailable)
                                   VALUES (:seatid, :seatrow, :seatnumber, :available)";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":seatid", OracleDbType.Int32).Value = nextId;
                        cmd.Parameters.Add(":seatrow", OracleDbType.Varchar2).Value = ddlRow.SelectedValue;
                        cmd.Parameters.Add(":seatnumber", OracleDbType.Int32).Value = Convert.ToInt32(txtSeatNumber.Text);
                        cmd.Parameters.Add(":available", OracleDbType.Int32).Value = chkAvailable.Checked ? 1 : 0;

                        cmd.ExecuteNonQuery();
                        ShowMessage("Seat added!", false);
                        LoadSeats();
                        ClearForm();
                    }
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }

        protected void gvSeats_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteSeat")
            {
                int seatID = Convert.ToInt32(e.CommandArgument);
                DeleteSeat(seatID);
            }
        }

        protected void gvSeats_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvSeats.PageIndex = e.NewPageIndex;
            LoadSeats();
        }

        private void DeleteSeat(int seatID)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("DELETE FROM Seat WHERE SeatID = :seatid", conn))
                    {
                        cmd.Parameters.Add(":seatid", OracleDbType.Int32).Value = seatID;
                        cmd.ExecuteNonQuery();
                        ShowMessage("Seat deleted!", false);
                        LoadSeats();
                    }
                }
                catch { ShowMessage("Cannot delete.", true); }
            }
        }

        private void ClearForm()
        {
            ddlRow.SelectedIndex = 0;
            txtSeatNumber.Text = "";
            chkAvailable.Checked = false;
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isError ? "alert alert-danger" : "alert alert-success";
            lblMessage.Visible = true;
        }
    }
}
