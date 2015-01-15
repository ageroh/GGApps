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

                if (StagProdAppVersions == null)
                    BtnPublishApp.Enabled = false;
                
                if (StagProdAppVersions != null && StagProdAppVersions.Count <= 0)
                    BtnPublishApp.Enabled = true;
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
            StagProdAppVersions.Clear();

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

        protected void BtnPublishApp_Click(object sender, EventArgs e)
        {
            // Check  DB version of staging-production
            
            string appName = SelectApp.SelectedItem.Text;
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);

            if (!CheckStagingProductionDBVersions(appName, appID, StagProdAppVersions))
            {
                Response.Write("Please check DB version files.");
                return;
            }

            // if is ok must staging_db_version = production_db_version + 1 foreach app. on Staging
            if (!RefreshVersionFileStaging(appID, appName, "ios"))
            {
                Response.Write("some error occured please contact admin !!!");
                return; 
            }

            // if is ok must staging_db_version = production_db_version + 1 foreach app. on Staging
            if (!RefreshVersionFileStaging(appID, appName, "android"))
            {
                Response.Write("some error occured please contact admin !!!");
                return;
            }

            // rename existing Versions.txt on production to versions-getdate().txt
            RenameFileRemote(appName, appName + "//update//ios//versions.txt", appName + "//update//ios//versions_" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".txt");
            RenameFileRemote(appName, appName + "//update//android//versions.txt", appName + "//update//android//versions_" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".txt");

            // upload all files (replace if exists) [images]-[update.zip]-[promo]-[configuration?] - >> !!! except versions.txt !!!
            
            // upload versions.txt from staging to production

            // if error occurs rollback transition.

            // else print success message

        }

        private bool RefreshVersionFileStaging(int appID, string appName, string mobileDevice)
        {
            Finalize fin = new Finalize(appName, appID);
            string dbver;
            string appVersion = "", configVersion = "";
            int newdbVersion=0;

            if (StagProdAppVersions == null)
                return false;

            if (StagProdAppVersions != null)
                if (StagProdAppVersions.Count <= 0)
                    return false;

            Log.InfoLog(mapPathError, "Refresh version txt on staging before deploy to production for:" + mobileDevice, appName);

            // Get the Db_Version of production
            if (GetVersionsFileProduction(mobileDevice, out dbver, out appVersion, out configVersion, appName) != null)
            {
                Int32.TryParse(dbver, out newdbVersion);
                // add + 1 to production db_version.
                newdbVersion++;                         

                // Set Versions file for IOS only for one Lang
                if (!fin.SetVerionsFileProperty("db_version", newdbVersion.ToString(), appName, appID, mobileDevice, 1, "versions.txt"))
                    return false;

                // Add a row to Admin DB for history reasons, to create e new MAJOR number.
                if (fin.AddDBversionAdmin(appName, appID, dbver, newdbVersion.ToString(), mobileDevice, "EN", newdbVersion) == null)
                {
                    Log.ErrorLog(mapPathError, "Error occured in AddDBversionAdmin writing for " + mobileDevice + " in DB  " + appName, appName);
                    return false;
                }
            }
            else
                return false;

            Log.InfoLog(mapPathError, "Successfully!", appName);

            return true;
        }



        private bool CheckStagingProductionDBVersions(string appName, int appID, List<AppVersionDetail> StagProdAppVersions)
        {
            int dbverStag, dbverProd;
            bool android = false;
            bool ios = false;

            if (StagProdAppVersions != null)
            {
                foreach (AppVersionDetail x in StagProdAppVersions)
                {
                    if (x.Environment == "Staging")
                    {
                        foreach (AppVersionDetail y in StagProdAppVersions)
                        {
                            if (y.Environment == "Production" && y.Device == x.Device)
                            { 
                                Int32.TryParse(x.DB_Version, out dbverStag);
                                Int32.TryParse(y.DB_Version, out dbverProd);

                                if (dbverStag >= dbverProd)
                                {
                                    if (y.Device == "android") android = true;
                                    if (y.Device == "ios")  ios = true;
                                }
                            }
                        }
                    }
                }

                if( android && ios)
                    return true;
            }
            
            return false;


        }


        protected void refreshDD_Click(object sender, ImageClickEventArgs e)
        {
            string appName = SelectApp.SelectedItem.Text;
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);

            FetchAppDetailsProduction(appID, appName);
        }

    }
}