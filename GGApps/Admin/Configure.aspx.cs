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
                SelectApp.Items.Insert(0, new ListItem(" - Select Application - ", "-1"));
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

            FetchVersionsConfigurationFiles(appID, appName,  DDEnvironment.SelectedItem.Value, DDListDevice.SelectedItem.Value);

        }

        protected void DDEnvironment_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList rdList = sender as DropDownList;
            string appName = SelectApp.SelectedItem.Text;
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);
            Session["appName"] = appName;
            Session["appID"] = appID;

            if (appID != -1)
            {
                FetchVersionsConfigurationFiles(appID, appName, rdList.SelectedItem.Value, DDListDevice.SelectedItem.Value);
            }
        }

        protected void DDListDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList rdList = sender as DropDownList;
            string appName = SelectApp.SelectedItem.Text;
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);
            Session["appName"] = appName;
            Session["appID"] = appID;

            if (appID != -1)
            {
                FetchVersionsConfigurationFiles(appID, appName, DDEnvironment.SelectedItem.Value, rdList.SelectedItem.Value);
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
            // if client confirm is previously ok;
            
            // save files from TextAreas
            SaveVersionsConfigurationFiles((int)Session["appID"], (string)Session["appName"], DDEnvironment.SelectedItem.Value, DDListDevice.SelectedItem.Value);

            // Refresh data
            FetchVersionsConfigurationFiles((int)Session["appID"], (string)Session["appName"], DDEnvironment.SelectedItem.Value, DDListDevice.SelectedItem.Value);
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
