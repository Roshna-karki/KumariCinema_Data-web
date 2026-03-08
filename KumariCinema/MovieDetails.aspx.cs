using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.IO;

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
                                LogError($"?? No movie found with ID: {movieID}");
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
}
