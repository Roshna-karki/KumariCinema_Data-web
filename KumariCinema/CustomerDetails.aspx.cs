using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinema
{
    public partial class CustomerDetails : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCustomers();
            }
        }

        private void LoadCustomers()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(
                        @"SELECT UserID AS ""UserID"", UserName AS ""UserName"", PhoneNumber AS ""PhoneNumber"", UserEmail AS ""UserEmail"", UserAddress AS ""UserAddress"" FROM UserTable ORDER BY UserName", conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    gvCustomers.DataSource = dt;
                    gvCustomers.DataBind();
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text)) { ShowMessage("Name required", true); return; }
            if (string.IsNullOrEmpty(txtEmail.Text)) { ShowMessage("Email required", true); return; }
            if (string.IsNullOrEmpty(txtPhone.Text)) { ShowMessage("Phone required", true); return; }

            // Validate email and phone uniqueness against UserTable
            int? excludeUserID = ViewState["EditUserID"] != null ? (int?)Convert.ToInt32(ViewState["EditUserID"]) : null;
            using (OracleConnection dupConn = new OracleConnection(connectionString))
            {
                try
                {
                    dupConn.Open();
                    string dupQuery = @"SELECT COUNT(*) FROM UserTable 
                                        WHERE (UPPER(TRIM(UserEmail)) = UPPER(TRIM(:email)) 
                                               OR TRIM(PhoneNumber) = TRIM(:phone))";
                    if (excludeUserID.HasValue)
                    {
                        dupQuery += " AND UserID <> :userid";
                    }

                    using (OracleCommand dupCmd = new OracleCommand(dupQuery, dupConn))
                    {
                        dupCmd.Parameters.Add(":email", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();
                        dupCmd.Parameters.Add(":phone", OracleDbType.Varchar2).Value = txtPhone.Text.Trim();
                        if (excludeUserID.HasValue)
                        {
                            dupCmd.Parameters.Add(":userid", OracleDbType.Int32).Value = excludeUserID.Value;
                        }

                        int dupCount = Convert.ToInt32(dupCmd.ExecuteScalar() ?? 0);
                        if (dupCount > 0)
                        {
                            ShowMessage("Email or phone number already exists.", true);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage("Error validating customer: " + ex.Message, true);
                    return;
                }
            }

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query;

                    if (ViewState["EditUserID"] != null)
                    {
                        query = @"UPDATE UserTable SET UserName = :name, UserEmail = :email, PhoneNumber = :phone, UserAddress = :address
                                 WHERE UserID = :userid";
                    }
                    else
                    {
                        int nextId = 1;
                        using (OracleCommand cmdMax = new OracleCommand("SELECT NVL(MAX(UserID), 0) + 1 FROM UserTable", conn))
                        {
                            object o = cmdMax.ExecuteScalar();
                            nextId = Convert.ToInt32(o);
                        }
                        query = @"INSERT INTO UserTable (UserID, UserName, UserEmail, PhoneNumber, UserAddress)
                                 VALUES (:userid, :name, :email, :phone, :address)";
                        using (OracleCommand cmd = new OracleCommand(query, conn))
                        {
                            cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = nextId;
                            cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                            cmd.Parameters.Add(":email", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();
                            cmd.Parameters.Add(":phone", OracleDbType.Varchar2).Value = txtPhone.Text.Trim();
                            cmd.Parameters.Add(":address", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtAddress.Text) ? DBNull.Value : (object)txtAddress.Text.Trim();
                            cmd.ExecuteNonQuery();
                            ShowMessage("Customer registered!", false);
                            ViewState["EditUserID"] = null;
                            LoadCustomers();
                            ClearForm();
                        }
                        return;
                    }

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":name", OracleDbType.Varchar2).Value = txtName.Text.Trim();
                        cmd.Parameters.Add(":email", OracleDbType.Varchar2).Value = txtEmail.Text.Trim();
                        cmd.Parameters.Add(":phone", OracleDbType.Varchar2).Value = txtPhone.Text.Trim();
                        cmd.Parameters.Add(":address", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(txtAddress.Text) ? DBNull.Value : (object)txtAddress.Text.Trim();

                        if (ViewState["EditUserID"] != null)
                        {
                            cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = Convert.ToInt32(ViewState["EditUserID"]);
                        }

                        cmd.ExecuteNonQuery();
                        ShowMessage(ViewState["EditUserID"] != null ? "Customer updated!" : "Customer registered!", false);
                        ViewState["EditUserID"] = null;
                        LoadCustomers();
                        ClearForm();
                    }
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            ViewState["EditUserID"] = null;
            lblFormTitle.Text = "Register New Customer";
        }

        protected void gvCustomers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int userID = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditCustomer") { LoadCustomerForEdit(userID); }
            else if (e.CommandName == "DeleteCustomer") { DeleteCustomer(userID); }
        }

        protected void gvCustomers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCustomers.PageIndex = e.NewPageIndex;
            LoadCustomers();
        }

        private void LoadCustomerForEdit(int userID)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("SELECT * FROM UserTable WHERE UserID = :userid", conn))
                    {
                        cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = userID;
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewState["EditUserID"] = userID;
                                lblFormTitle.Text = "Edit Customer";
                                txtName.Text = GetReaderString(reader, "UserName");
                                txtEmail.Text = GetReaderString(reader, "UserEmail");
                                txtPhone.Text = GetReaderString(reader, "PhoneNumber");
                                txtAddress.Text = GetReaderString(reader, "UserAddress");
                            }
                        }
                    }
                }
                catch (Exception ex) { ShowMessage("Error: " + ex.Message, true); }
            }
        }

        private void DeleteCustomer(int userID)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("DELETE FROM UserTable WHERE UserID = :userid", conn))
                    {
                        cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = userID;
                        cmd.ExecuteNonQuery();
                        ShowMessage("Customer deleted!", false);
                        LoadCustomers();
                    }
                }
                catch { ShowMessage("Cannot delete customer.", true); }
            }
        }

        private static string GetReaderString(OracleDataReader reader, string columnName)
        {
            try
            {
                object val = reader[columnName];
                if (val == null || val == DBNull.Value) return "";
                return val.ToString();
            }
            catch (IndexOutOfRangeException)
            {
                try
                {
                    object val = reader[columnName.ToUpperInvariant()];
                    return (val == null || val == DBNull.Value) ? "" : val.ToString();
                }
                catch { return ""; }
            }
        }

        private void ClearForm()
        {
            txtName.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtAddress.Text = "";
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isError ? "alert alert-danger" : "alert alert-success";
            lblMessage.Visible = true;
        }
    }
}
