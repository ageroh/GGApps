using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace GGApps
{
    public partial class Publish : Common
    {

        public static List<AppVersionDetail> StagProdAppVersions = new List<AppVersionDetail>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
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

            latestVersions.DataSource = GetInfoAppData(appID, appName);
            latestVersions.DataBind();

            // Fetch details from Production for App
            FetchAppDetailsProduction(appID, appName);

        }

        private void FetchAppDetailsProduction(int appID, string appName)
        {
            string dbVersion, appVersion, configVersion;
            
            GetVersionsFileProduction("android", out dbVersion, out appVersion, out configVersion, appName);
            
            prodAndDB.Text = dbVersion;
            prodAndAV.Text = appVersion;
            prodAndCV.Text = configVersion;
            StagProdAppVersions.Add(new AppVersionDetail("production", "android", dbVersion, appVersion, configVersion));

            GetVersionsFile("android", out dbVersion, out appVersion, out configVersion, appName);

            stagAndDB.Text = dbVersion;
            stagAndAV.Text = appVersion;
            stagAndCV.Text = configVersion;
            StagProdAppVersions.Add(new AppVersionDetail("staging", "android", dbVersion, appVersion, configVersion));

            
            GetVersionsFileProduction("ios", out dbVersion, out appVersion, out configVersion, appName);

            prodIosDB.Text = dbVersion;
            prodIosAV.Text = appVersion;
            prodIosCV.Text = configVersion;
            StagProdAppVersions.Add(new AppVersionDetail("production", "ios", dbVersion, appVersion, configVersion));

            GetVersionsFile("ios", out dbVersion, out appVersion, out configVersion, appName);

            stagIosDB.Text = dbVersion;
            stagIosAV.Text = appVersion;
            stagIosCV.Text = configVersion;
            StagProdAppVersions.Add(new AppVersionDetail("staging", "ios", dbVersion, appVersion, configVersion));

        }

    }
}