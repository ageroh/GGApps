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

            FetchVersionsConfigurationFiles(appID, appName, RadioButtonLisEnvironment.SelectedItem.Value, RadioButtonListDevice.SelectedItem.Value);

        }

        protected void RadioButtonLisEnvironment_SelectedIndexChanged(object sender, EventArgs e)
        {
            RadioButtonList rdList = sender as RadioButtonList;
            string appName = SelectApp.SelectedItem.Text;
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);
            Session["appName"] = appName;
            Session["appID"] = appID;

            if (appID != -1)
            {
                FetchVersionsConfigurationFiles(appID, appName, rdList.SelectedItem.Value, RadioButtonListDevice.SelectedItem.Value);
            }
        }

        protected void RadioButtonListDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            RadioButtonList rdList = sender as RadioButtonList;
            string appName = SelectApp.SelectedItem.Text;
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);
            Session["appName"] = appName;
            Session["appID"] = appID;

            if (appID != -1)
            {
                FetchVersionsConfigurationFiles(appID, appName, RadioButtonLisEnvironment.SelectedItem.Value, rdList.SelectedItem.Value);
            }
        }

         
        


        private void FetchVersionsConfigurationFiles(int appID, string appName, string Environment, string mobileDevice)
        {
            if (Environment != null && mobileDevice != null)
            {
                // Fetch versions.txt
                VersionsTxt.Text = GetCofigureFile(mobileDevice, appName, appID, Environment, "versions.txt");

                // fetch configuration.txt
                ConfigurationTxt.Text = GetCofigureFile(mobileDevice, appName, appID, Environment, "configuration.txt");
            }
        }


        protected void SaveAll_Click(object sender, EventArgs e)
        {
            // Are you sure?
            string user = HttpContext.Current.User.Identity.Name;
            System.Web.UI.ScriptManager.RegisterClientScriptBlock(this
                , this.GetType()
                , "Warning!", "if(!confirm('" + user.Substring(user.IndexOf("\\") + 1) + ", are you sure you want to save all changes for " + Session["appName"] + " in " + RadioButtonLisEnvironment.SelectedItem.Text + " for " + RadioButtonListDevice.SelectedItem.Text + " ?'){return false;}", true);


            // save files from TextAreas
            SaveVersionsConfigurationFiles((int)Session["appID"], (string)Session["appName"], RadioButtonLisEnvironment.SelectedItem.Value, RadioButtonListDevice.SelectedItem.Value);

        }

        private object SaveVersionsConfigurationFiles(int appID, string appName, string Environment, string mobileDevice)
        {
            if (Environment != null && mobileDevice != null)
            {
                string res = SaveConfigureFile(mobileDevice, appName, appID, Environment, "versions.txt", VersionsTxt.Text);
                if (res == null)
                    return null;    // error
                else
                    PrintSaveMessage(res, appName, Environment, mobileDevice, "versions.txt");
                    
                res = SaveConfigureFile(mobileDevice, appName, appID, Environment, "configuration.txt", ConfigurationTxt.Text);
                if (res == null)
                    return null;    // error
                else
                    PrintSaveMessage(res, appName, Environment, mobileDevice, "configuration.txt");
                
                return 0;
            }
            return null;
        }

    }
}
