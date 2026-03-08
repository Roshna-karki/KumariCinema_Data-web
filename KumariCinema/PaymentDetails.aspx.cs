using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace KumariCinema
{
    public partial class PaymentDetails : Page
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
                LoadPayments();
            }
        }

        private void LoadPayments()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string q = "SELECT P.PaymentID, P.PaymentAmount, P.PaymentStatus, P.PaymentMode, " +
                               "B.ConfirmationCode, NVL(U.UserName, 'Guest') AS UserName, " +
                               "T.TheatreName, H.HallName, M.Title, S.ShowDate " +
                               "FROM " + SchemaPrefix + "Payment P " +
                               "LEFT JOIN " + SchemaPrefix + "Booking B ON P.PaymentID = B.PaymentID " +
                               "LEFT JOIN " + SchemaPrefix + "USERS U ON B.UserID = U.UserID " +
                               "LEFT JOIN " + SchemaPrefix + "Shows S ON B.ShowID = S.ShowID " +
                               "LEFT JOIN " + SchemaPrefix + "Movie M ON S.MovieID = M.MovieID " +
                               "LEFT JOIN " + SchemaPrefix + "Hall H ON S.HallID = H.HallID " +
                               "LEFT JOIN " + SchemaPrefix + "Theatre T ON H.TheatreID = T.TheatreID " +
                               "ORDER BY P.PaymentID DESC";
                    DataTable dt = new DataTable();
                    using (OracleCommand cmd = new OracleCommand(q, conn))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    gvPayments.DataSource = dt;
                    gvPayments.DataBind();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error: " + ex.Message, true);
                }
            }
        }

        protected void gvPayments_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPayments.PageIndex = e.NewPageIndex;
            LoadPayments();
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isError ? "alert alert-danger" : "alert alert-success";
            lblMessage.Visible = true;
        }
    }
}
