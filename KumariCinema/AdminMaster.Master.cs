using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace KumariCinema
{
    public partial class AdminMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set active menu based on current page
                string currentPage = System.IO.Path.GetFileNameWithoutExtension(Request.Url.AbsolutePath);
                
                foreach (Control control in FindControlRecursive(this, typeof(HyperLink)))
                {
                    HyperLink link = (HyperLink)control;
                    if (link.NavigateUrl.Contains(currentPage))
                    {
                        link.CssClass = "nav-item active";
                    }
                    else if (link.CssClass == "nav-item active")
                    {
                        link.CssClass = "nav-item";
                    }
                }
            }
        }

        private System.Collections.ArrayList FindControlRecursive(Control root, Type type)
        {
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            
            foreach (Control control in root.Controls)
            {
                if (control.GetType() == type)
                    list.Add(control);
                else if (control.HasControls())
                    list.AddRange(FindControlRecursive(control, type));
            }
            
            return list;
        }
    }
}
