using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinema
{
    public partial class ShowtimeDetails : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadMovies();
                LoadHalls();
                LoadShows();
            }
        }

        private void LoadMovies()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand("SELECT MovieID, Title FROM Movie ORDER BY Title", conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    ddlMovie.DataSource = dt;
                    ddlMovie.DataTextField = "Title";
                    ddlMovie.DataValueField = "MovieID";
                    ddlMovie.DataBind();
                    ddlMovie.Items.Insert(0, new ListItem("-- Select Movie --", ""));
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        private void LoadHalls()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand("SELECT HallID, HallName FROM Hall ORDER BY HallName", conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    ddlHall.DataSource = dt;
                    ddlHall.DataTextField = "HallName";
                    ddlHall.DataValueField = "HallID";
                    ddlHall.DataBind();
                    ddlHall.Items.Insert(0, new ListItem("-- Select Hall --", ""));
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        private void LoadShows()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(
                        @"SELECT S.ShowID, M.Title AS MovieTitle, S.ShowDate, 
                                 TO_CHAR(S.ShowTime, 'HH24:MI') AS ShowTime, H.HallName 
                          FROM Shows S 
                          INNER JOIN Movie M ON S.MovieID = M.MovieID 
                          INNER JOIN Hall H ON S.HallID = H.HallID 
                          ORDER BY S.ShowDate DESC, S.ShowTime", conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    gvShows.DataSource = dt;
                    gvShows.DataBind();
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtShowDate.Text)) { ShowMessage("Show Date required", true); return; }
            if (string.IsNullOrEmpty(ddlShowTime.SelectedValue)) { ShowMessage("Show Time required", true); return; }
            if (string.IsNullOrEmpty(ddlMovie.SelectedValue)) { ShowMessage("Movie required", true); return; }
            if (string.IsNullOrEmpty(ddlHall.SelectedValue)) { ShowMessage("Hall required", true); return; }

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query;

                    DateTime showDate = Convert.ToDateTime(txtShowDate.Text);
                    string timeStr = ddlShowTime.SelectedValue + ":00";
                    string timestampStr = showDate.ToString("yyyy-MM-dd") + " " + timeStr;

                    // Prevent duplicate shows in the same hall at the same date/time
                    int hallId = Convert.ToInt32(ddlHall.SelectedValue);
                    int movieId = Convert.ToInt32(ddlMovie.SelectedValue);
                    int existingShowId = ViewState["EditShowID"] != null ? Convert.ToInt32(ViewState["EditShowID"]) : 0;
                    using (OracleCommand dupCmd = new OracleCommand(
                        @"SELECT COUNT(*) FROM Shows 
                          WHERE HallID = :hallid 
                            AND ShowDate = :showdate 
                            AND NVL(StartTime, ShowTime) = TO_TIMESTAMP(:timestr, 'YYYY-MM-DD HH24:MI:SS')", conn))
                    {
                        dupCmd.BindByName = true;
                        dupCmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = hallId;
                        dupCmd.Parameters.Add(":showdate", OracleDbType.Date).Value = showDate;
                        dupCmd.Parameters.Add(":timestr", OracleDbType.Varchar2).Value = timestampStr;

                        int dupCount = Convert.ToInt32(dupCmd.ExecuteScalar());
                        if (existingShowId == 0 && dupCount > 0)
                        {
                            ShowMessage("A show already exists in this hall at the selected date and time.", true);
                            return;
                        }
                    }

                    if (ViewState["EditShowID"] != null)
                    {
                        query = @"UPDATE Shows SET ShowDate = :showdate, 
                                 ShowTime = TO_TIMESTAMP(:timestr, 'YYYY-MM-DD HH24:MI:SS'),
                                 StartTime = TO_TIMESTAMP(:timestr, 'YYYY-MM-DD HH24:MI:SS'),
                                 EndTime = TO_TIMESTAMP(:timestr, 'YYYY-MM-DD HH24:MI:SS') + INTERVAL '2' HOUR,
                                 MovieID = :movieid, HallID = :hallid WHERE ShowID = :showid";
                    }
                    else
                    {
                        int nextId = 1;
                        using (OracleCommand cmdMax = new OracleCommand("SELECT NVL(MAX(ShowID), 0) + 1 FROM Shows", conn))
                        {
                            object o = cmdMax.ExecuteScalar();
                            nextId = Convert.ToInt32(o);
                        }
                        query = @"INSERT INTO Shows (ShowID, ShowDate, ShowTime, StartTime, EndTime, MovieID, HallID) 
                                 VALUES (:showid, :showdate, 
                                 TO_TIMESTAMP(:timestr, 'YYYY-MM-DD HH24:MI:SS'),
                                 TO_TIMESTAMP(:timestr, 'YYYY-MM-DD HH24:MI:SS'),
                                 TO_TIMESTAMP(:timestr, 'YYYY-MM-DD HH24:MI:SS') + INTERVAL '2' HOUR,
                                 :movieid, :hallid)";
                        using (OracleCommand cmd = new OracleCommand(query, conn))
                        {
                            cmd.BindByName = true;
                            cmd.Parameters.Add(":showid", OracleDbType.Int32).Value = nextId;
                            cmd.Parameters.Add(":showdate", OracleDbType.Date).Value = showDate;
                            cmd.Parameters.Add(":timestr", OracleDbType.Varchar2).Value = timestampStr;
                            cmd.Parameters.Add(":movieid", OracleDbType.Int32).Value = Convert.ToInt32(ddlMovie.SelectedValue);
                            cmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = Convert.ToInt32(ddlHall.SelectedValue);
                            cmd.ExecuteNonQuery();
                            ShowMessage("Show added!", false);
                            ViewState["EditShowID"] = null;
                            LoadShows();
                            ClearForm();
                        }
                        return;
                    }

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.Add(":showdate", OracleDbType.Date).Value = showDate;
                        cmd.Parameters.Add(":timestr", OracleDbType.Varchar2).Value = timestampStr;
                        cmd.Parameters.Add(":movieid", OracleDbType.Int32).Value = Convert.ToInt32(ddlMovie.SelectedValue);
                        cmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = Convert.ToInt32(ddlHall.SelectedValue);

                        if (ViewState["EditShowID"] != null)
                        {
                            cmd.Parameters.Add(":showid", OracleDbType.Int32).Value = Convert.ToInt32(ViewState["EditShowID"]);
                        }

                        cmd.ExecuteNonQuery();
                        ShowMessage(ViewState["EditShowID"] != null ? "Show updated!" : "Show added!", false);
                        ViewState["EditShowID"] = null;
                        LoadShows();
                        ClearForm();
                    }
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            ViewState["EditShowID"] = null;
            lblFormTitle.Text = "Add New Show";
        }

        protected void gvShows_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int showID = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditShow") { LoadShowForEdit(showID); }
            else if (e.CommandName == "DeleteShow") { DeleteShow(showID); }
        }

        protected void gvShows_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvShows.PageIndex = e.NewPageIndex;
            LoadShows();
        }

        private void LoadShowForEdit(int showID)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("SELECT * FROM Shows WHERE ShowID = :showid", conn))
                    {
                        cmd.Parameters.Add(":showid", OracleDbType.Int32).Value = showID;
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewState["EditShowID"] = showID;
                                lblFormTitle.Text = "Edit Show";
                                txtShowDate.Text = Convert.ToDateTime(reader["ShowDate"]).ToString("yyyy-MM-dd");
                                object showTimeVal = reader["ShowTime"];
                                if (showTimeVal != null && showTimeVal != DBNull.Value)
                                {
                                    ddlShowTime.SelectedValue = Convert.ToDateTime(showTimeVal).ToString("HH:mm");
                                }
                                ddlMovie.SelectedValue = reader["MovieID"].ToString();
                                ddlHall.SelectedValue = reader["HallID"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        private void DeleteShow(int showID)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("DELETE FROM Shows WHERE ShowID = :showid", conn))
                    {
                        cmd.Parameters.Add(":showid", OracleDbType.Int32).Value = showID;
                        cmd.ExecuteNonQuery();
                        ShowMessage("Show deleted!", false);
                        LoadShows();
                    }
                }
                catch { ShowMessage("Cannot delete show.", true); }
            }
        }

        private void ClearForm()
        {
            txtShowDate.Text = "";
            ddlShowTime.SelectedIndex = 0;
            ddlMovie.SelectedIndex = 0;
            ddlHall.SelectedIndex = 0;
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isError ? "alert alert-danger" : "alert alert-success";
            lblMessage.Visible = true;
        }
    }
}
