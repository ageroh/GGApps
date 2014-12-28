using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace GGApps
{
    public class CommonAdmin : System.Web.UI.Page
    {

        public void Init()
        {
            if (CheckAccount())
            {
                InitializeControls();
            }
            else
            {
                Response.Redirect("~/");
            }
        }

        // Implement this as for multiple account users.
        public bool CheckAccount()
        {
            return true; 
        }


        public void InitializeControls()
        {
            InitializeAppDD();
        }

        protected void InitializeAppDD()
        {

            DropDownList SelectApp = (DropDownList)FindControl("SelectApp");

            if (SelectApp != null)
            {
                SelectApp.DataSource = Common.GetAllAppTable();
                SelectApp.DataTextField = "appName";
                SelectApp.DataValueField = "id";
                SelectApp.Items.Insert(0, " - Select App - ");

                SelectApp.DataBind();
            }
        }

    }
}