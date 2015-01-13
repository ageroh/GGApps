using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GGApps
{
    public partial class Configure : Common
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                Initialize();

                InitializeAppDD();
            }
        }

        protected void EnvironmentConfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            ;
        }

        protected void InitializeAppDD()
        {

            System.Web.UI.WebControls.DropDownList SelectApp = (System.Web.UI.WebControls.DropDownList)LoginViewImportant.FindControl("SelectApp");

            if (SelectApp != null)
            {
                SelectApp.DataSource = Common.GetAllAppTable();
                SelectApp.DataTextField = "appName";
                SelectApp.DataValueField = "id";

                SelectApp.DataBind();
                SelectApp.Items.Insert(0, new ListItem(" - Select Application - "));
                SelectApp.SelectedIndex = 0;
            }
        }


        protected void SelectApp_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList dropDown = sender as DropDownList;
            string appName = dropDown.SelectedItem.Text;
            int appID = -1;
            Int32.TryParse(dropDown.SelectedItem.Value, out appID);

            //store in session / force change if exists
            Session["appName"] = appName;
            Session["appID"] = appID;

            //// Fetch details from Production for App
            //FetchAppDetailsProduction(appID, appName);

            FetchVersionsConfigurationFiles(appID, appName);

        }

        private void FetchVersionsConfigurationFiles(int appID, string appName)
        {
            throw new NotImplementedException();
        }

    }
}