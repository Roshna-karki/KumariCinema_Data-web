using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinema
{
    public partial class HallDetails : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTheatres();
                LoadHalls();
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
                    using (OracleCommand cmd = new OracleCommand("SELECT TheatreID, TheatreName FROM Theatre ORDER BY TheatreName", conn))
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
                catch (Exception ex) { ShowMessage("Error loading theatres: " + ex.Message, true); }
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
                    using (OracleCommand cmd = new OracleCommand(
                        @"SELECT H.HallID, H.HallName, H.HallCapacity, H.HallType, H.ScreenSize, T.TheatreName 
                          FROM Hall H INNER JOIN Theatre T ON H.TheatreID = T.TheatreID ORDER BY T.TheatreName, H.HallName", conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    gvHalls.DataSource = dt;
                    gvHalls.DataBind();
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtHallName.Text)) { ShowMessage("Hall Name required", true); return; }
            if (string.IsNullOrEmpty(txtCapacity.Text)) { ShowMessage("Capacity required", true); return; }
            if (string.IsNullOrEmpty(ddlScreenSize.SelectedValue)) { ShowMessage("Screen Size required", true); return; }
            if (string.IsNullOrEmpty(ddlTheatre.SelectedValue)) { ShowMessage("Theatre required", true); return; }

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query;

                    if (ViewState["EditHallID"] != null)
                    {
                        query = @"UPDATE Hall SET HallName = :name, HallCapacity = :capacity, HallType = :halltype,
                                 ScreenSize = :screen, TheatreID = :theatre
                                 WHERE HallID = :hallid";
                    }
                    else
                    {
                        // Ensure combination of HallName + Theatre does not already exist
                        using (OracleCommand cmdCheck = new OracleCommand(
                            "SELECT COUNT(*) FROM Hall WHERE UPPER(TRIM(HallName)) = UPPER(TRIM(:name)) AND TheatreID = :theatre", conn))
                        {
                            cmdCheck.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtHallName.Text.Trim();
                            cmdCheck.Parameters.Add(":theatre", OracleDbType.Int32).Value = Convert.ToInt32(ddlTheatre.SelectedValue);
                            object existing = cmdCheck.ExecuteScalar();
                            if (Convert.ToInt32(existing) > 0)
                            {
                                ShowMessage("Hall name already exists in this theatre.", true);
                                return;
                            }
                        }

                        int nextId = 1;
                        using (OracleCommand cmdMax = new OracleCommand("SELECT NVL(MAX(HallID), 0) + 1 FROM Hall", conn))
                        {
                            object o = cmdMax.ExecuteScalar();
                            nextId = Convert.ToInt32(o);
                        }
                        query = @"INSERT INTO Hall (HallID, HallName, HallCapacity, HallType, ScreenSize, TheatreID)
                                 VALUES (:hallid, :name, :capacity, :halltype, :screen, :theatre)";
                        using (OracleCommand cmd = new OracleCommand(query, conn))
                        {
                            cmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = nextId;
                            cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtHallName.Text.Trim();
                            cmd.Parameters.Add(":capacity", OracleDbType.Int32).Value = Convert.ToInt32(txtCapacity.Text);
                            cmd.Parameters.Add(":halltype", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(ddlHallType.SelectedValue) ? DBNull.Value : (object)ddlHallType.SelectedValue;
                            cmd.Parameters.Add(":screen", OracleDbType.Varchar2).Value = ddlScreenSize.SelectedValue;
                            cmd.Parameters.Add(":theatre", OracleDbType.Int32).Value = Convert.ToInt32(ddlTheatre.SelectedValue);
                            cmd.ExecuteNonQuery();
                            ShowMessage("Hall added!", false);
                            ViewState["EditHallID"] = null;
                            LoadHalls();
                            ClearForm();
                        }
                        return;
                    }

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtHallName.Text.Trim();
                        cmd.Parameters.Add(":capacity", OracleDbType.Int32).Value = Convert.ToInt32(txtCapacity.Text);
                        cmd.Parameters.Add(":halltype", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(ddlHallType.SelectedValue) ? DBNull.Value : (object)ddlHallType.SelectedValue;
                        cmd.Parameters.Add(":screen", OracleDbType.Varchar2).Value = ddlScreenSize.SelectedValue;
                        cmd.Parameters.Add(":theatre", OracleDbType.Int32).Value = Convert.ToInt32(ddlTheatre.SelectedValue);

                        if (ViewState["EditHallID"] != null)
                        {
                            cmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = Convert.ToInt32(ViewState["EditHallID"]);
                        }

                        cmd.ExecuteNonQuery();
                        ShowMessage(ViewState["EditHallID"] != null ? "Hall updated!" : "Hall added!", false);
                        ViewState["EditHallID"] = null;
                        LoadHalls();
                        ClearForm();
                    }
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            ViewState["EditHallID"] = null;
            lblFormTitle.Text = "Add New Hall";
        }

        protected void gvHalls_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int hallID = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditHall") { LoadHallForEdit(hallID); }
            else if (e.CommandName == "DeleteHall") { DeleteHall(hallID); }
        }

        protected void gvHalls_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvHalls.PageIndex = e.NewPageIndex;
            LoadHalls();
        }

        private void LoadHallForEdit(int hallID)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("SELECT * FROM Hall WHERE HallID = :hallid", conn))
                    {
                        cmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = hallID;
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewState["EditHallID"] = hallID;
                                lblFormTitle.Text = "Edit Hall";
                                txtHallName.Text = reader["HallName"].ToString();
                                txtCapacity.Text = reader["HallCapacity"].ToString();
                                ddlHallType.SelectedValue = reader["HallType"] != DBNull.Value && reader["HallType"] != null ? reader["HallType"].ToString() : "";
                                ddlScreenSize.SelectedValue = reader["ScreenSize"].ToString();
                                ddlTheatre.SelectedValue = reader["TheatreID"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        private void DeleteHall(int hallID)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("DELETE FROM Hall WHERE HallID = :hallid", conn))
                    {
                        cmd.Parameters.Add(":hallid", OracleDbType.Int32).Value = hallID;
                        cmd.ExecuteNonQuery();
                        ShowMessage("Hall deleted!", false);
                        LoadHalls();
                    }
                }
                catch { ShowMessage("Cannot delete. Hall may be in use.", true); }
            }
        }

        private void ClearForm()
        {
            txtHallName.Text = "";
            txtCapacity.Text = "";
            ddlHallType.SelectedIndex = 0;
            ddlScreenSize.SelectedIndex = 0;
            ddlTheatre.SelectedIndex = 0;
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isError ? "alert alert-danger" : "alert alert-success";
            lblMessage.Visible = true;
        }
    }
}
