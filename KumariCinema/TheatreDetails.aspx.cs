using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinema
{
    public partial class TheatreDetails : Page
    {
        private string connectionString = 
            ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTheatres();
            }
        }

        private void LoadTheatres()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"SELECT TheatreID, TheatreName, City, TheatreAddress, TheatrePhoneNumber
                                   FROM Theatre ORDER BY TheatreName";

                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    gvTheatres.DataSource = dt;
                    gvTheatres.DataBind();
                }
                catch (Exception ex)
                {
                    ShowMessage($"Error: {ex.Message}", true);
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTheatreName.Text.Trim()))
            {
                ShowMessage("Theatre Name is required", true);
                return;
            }

            if (string.IsNullOrEmpty(txtCity.Text.Trim()))
            {
                ShowMessage("City is required", true);
                return;
            }

            if (string.IsNullOrEmpty(txtContactNumber.Text.Trim()))
            {
                ShowMessage("Phone Number is required", true);
                return;
            }

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // If adding a new theatre, prevent duplicates with same name in same city
                    if (ViewState["EditTheatreID"] == null)
                    {
                        using (OracleCommand dupCmd = new OracleCommand(
                            @"SELECT COUNT(*) FROM Theatre 
                              WHERE UPPER(TRIM(TheatreName)) = UPPER(TRIM(:name)) 
                                AND UPPER(TRIM(City)) = UPPER(TRIM(:city))", conn))
                        {
                            dupCmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtTheatreName.Text.Trim();
                            dupCmd.Parameters.Add(":city", OracleDbType.Varchar2).Value = txtCity.Text.Trim();

                            int existing = Convert.ToInt32(dupCmd.ExecuteScalar() ?? 0);
                            if (existing > 0)
                            {
                                ShowMessage("Theatre with same name already exists in this city.", true);
                                return;
                            }
                        }
                    }

                    string query;

                    if (ViewState["EditTheatreID"] != null)
                    {
                        query = @"UPDATE Theatre 
                                 SET TheatreName = :name, 
                                     City = :city, 
                                     TheatreAddress = :address,
                                     TheatrePhoneNumber = :phone
                                 WHERE TheatreID = :theatreid";
                    }
                    else
                    {
                        int nextId = 1;
                        using (OracleCommand cmdMax = new OracleCommand("SELECT NVL(MAX(TheatreID), 0) + 1 FROM Theatre", conn))
                        {
                            object o = cmdMax.ExecuteScalar();
                            nextId = Convert.ToInt32(o);
                        }
                        query = @"INSERT INTO Theatre (TheatreID, TheatreName, City, TheatreAddress, TheatrePhoneNumber)
                                 VALUES (:theatreid, :name, :city, :address, :phone)";
                        using (OracleCommand cmd = new OracleCommand(query, conn))
                        {
                            cmd.Parameters.Add(":theatreid", OracleDbType.Int32).Value = nextId;
                            cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtTheatreName.Text.Trim();
                            cmd.Parameters.Add(":city", OracleDbType.Varchar2).Value = txtCity.Text.Trim();
                            cmd.Parameters.Add(":address", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtAddress.Text) ? DBNull.Value : (object)txtAddress.Text.Trim();
                            cmd.Parameters.Add(":phone", OracleDbType.Varchar2).Value = txtContactNumber.Text.Trim();
                            cmd.ExecuteNonQuery();
                            ShowMessage("Theatre added!", false);
                            ViewState["EditTheatreID"] = null;
                            LoadTheatres();
                            ClearForm();
                        }
                        return;
                    }

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtTheatreName.Text.Trim();
                        cmd.Parameters.Add(":city", OracleDbType.Varchar2).Value = txtCity.Text.Trim();
                        cmd.Parameters.Add(":address", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtAddress.Text) ? DBNull.Value : (object)txtAddress.Text.Trim();
                        cmd.Parameters.Add(":phone", OracleDbType.Varchar2).Value = txtContactNumber.Text.Trim();

                        if (ViewState["EditTheatreID"] != null)
                        {
                            cmd.Parameters.Add(":theatreid", OracleDbType.Int32).Value = Convert.ToInt32(ViewState["EditTheatreID"]);
                        }

                        cmd.ExecuteNonQuery();
                        ShowMessage(ViewState["EditTheatreID"] != null ? "Theatre updated!" : "Theatre added!", false);
                        ViewState["EditTheatreID"] = null;
                        LoadTheatres();
                        ClearForm();
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Error: {ex.Message}", true);
                }
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            ViewState["EditTheatreID"] = null;
            lblFormTitle.Text = "Add New Theatre";
        }

        protected void gvTheatres_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int theatreID = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditTheatre")
            {
                LoadTheatreForEdit(theatreID);
            }
            else if (e.CommandName == "DeleteTheatre")
            {
                DeleteTheatre(theatreID);
            }
        }

        protected void gvTheatres_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvTheatres.PageIndex = e.NewPageIndex;
            LoadTheatres();
        }

        private void LoadTheatreForEdit(int theatreID)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT * FROM Theatre WHERE TheatreID = :theatreid";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":theatreid", OracleDbType.Int32).Value = theatreID;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewState["EditTheatreID"] = theatreID;
                                lblFormTitle.Text = "Edit Theatre";

                                txtTheatreName.Text = reader["TheatreName"].ToString();
                                txtCity.Text = reader["City"].ToString();
                                txtAddress.Text = reader["TheatreAddress"] != DBNull.Value ? reader["TheatreAddress"].ToString() : "";
                                txtContactNumber.Text = reader["TheatrePhoneNumber"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Error: {ex.Message}", true);
                }
            }
        }

        private void DeleteTheatre(int theatreID)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "DELETE FROM Theatre WHERE TheatreID = :theatreid";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":theatreid", OracleDbType.Int32).Value = theatreID;
                        cmd.ExecuteNonQuery();

                        ShowMessage("Theatre deleted!", false);
                        LoadTheatres();
                    }
                }
                catch
                {
                    ShowMessage("Cannot delete theatre. It may have halls assigned.", true);
                }
            }
        }

        private void ClearForm()
        {
            txtTheatreName.Text = "";
            txtCity.Text = "";
            txtAddress.Text = "";
            txtContactNumber.Text = "";
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isError ? "alert alert-danger" : "alert alert-success";
            lblMessage.Visible = true;
        }
    }
}
