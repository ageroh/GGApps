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
                ClearCustomMessageValidationSummary();
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

            latestVersions.DataSource = GetInfoAppData(appID, appName);
            latestVersions.DataBind();

            // Fetch details from Production for App
            FetchAppDetailsProduction(appID, appName);

            int rc =0;
            try{rc = ((DataSet)(latestVersions.DataSource)).Tables[0].Rows.Count;}
            catch(Exception ex){rc = 0;}


            //StagProdAppVersions.Clear();
            if (SelectApp.SelectedIndex > 0 && rc > 0)
            {
                BtnPublishApp.CssClass = null;
                BtnPublishApp.Enabled = true;
            }
            else
            {
                BtnPublishApp.CssClass = "InputDisabledCustom";
                BtnPublishApp.Enabled = false;
            }
        }

        private void FetchAppDetailsProduction(int appID, string appName)
        {
            string dbVersion, appVersion, configVersion;
            StagProdAppVersions.Clear();

            GetVersionsFileProduction("android", out dbVersion, out appVersion, out configVersion, appName);
            
            prodAndDB.Text = dbVersion;
            prodAndAV.Text = appVersion;
            prodAndCV.Text = configVersion;
            StagProdAppVersions.Add(new AppVersionDetail("production", "android", dbVersion, appVersion, configVersion, appName, appID));

            GetVersionsFile("android", out dbVersion, out appVersion, out configVersion, appName);

            stagAndDB.Text = dbVersion;
            stagAndAV.Text = appVersion;
            stagAndCV.Text = configVersion;
            StagProdAppVersions.Add(new AppVersionDetail("staging", "android", dbVersion, appVersion, configVersion, appName, appID));

            
            GetVersionsFileProduction("ios", out dbVersion, out appVersion, out configVersion, appName);

            prodIosDB.Text = dbVersion;
            prodIosAV.Text = appVersion;
            prodIosCV.Text = configVersion;
            StagProdAppVersions.Add(new AppVersionDetail("production", "ios", dbVersion, appVersion, configVersion, appName, appID));

            GetVersionsFile("ios", out dbVersion, out appVersion, out configVersion, appName);

            stagIosDB.Text = dbVersion;
            stagIosAV.Text = appVersion;
            stagIosCV.Text = configVersion;
            StagProdAppVersions.Add(new AppVersionDetail("staging", "ios", dbVersion, appVersion, configVersion, appName, appID));

        }

        private void DisplayCustomMessageInValidationSummary(string message)
        {
            custValidation.Text = message;
            custValidation.Enabled = true;
            custValidation.Visible = true;
        }

        private void ClearCustomMessageValidationSummary()
        {

            custValidation.Text = String.Empty;
            custValidation.Enabled = false;
            custValidation.Visible = false;
        }


        protected void BtnPublishApp_Click(object sender, EventArgs e)
        {
            // Check  DB version of staging-production
            
            string appName = SelectApp.SelectedItem.Text;
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);

            if (!CheckStagingProductionDBVersions(appName, appID, StagProdAppVersions))
            {
                DisplayCustomMessageInValidationSummary("Please check DB version files for: " + appName + ". Nothing Deployed to Production.");
                return;
            }

            Log.InfoLog(mapPathError, "============================================== STARTED PUBLISH TO PRODUCTION FOR " + appName + " ==============================================", appName);
            // if is ok must staging_db_version = production_db_version + 1 foreach app. on Staging
            if (!RefreshVersionFileStaging(appID, appName, "ios"))
            {
                DisplayCustomMessageInValidationSummary("Error, while deploying " + appName + " to Production for iOS. Nothing Deployed to Production.");
                return;
            }

            // if is ok must staging_db_version = production_db_version + 1 foreach app. on Staging
            if (!RefreshVersionFileStaging(appID, appName, "android"))
            {
                DisplayCustomMessageInValidationSummary("Error, while deploying " + appName + " to Production for Android. Nothing Deployed to Production."); 
                return;
            }

            // rename existing Versions.txt on production to versions-getdate().txt
            if (RenameFileRemote(appName, appName.ToLower() + "/update/ios/versions.txt", "versions_" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".txt") > 0)
            {
                if (RenameFileRemote(appName, appName.ToLower() + "/update/android/versions.txt", "versions_" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".txt") > 0)
                {

                    // upload all files (replace if exists) [images]-[update.zip]-[promo]-[configuration?] - >> !!! except versions.txt !!!
                    string res = UploadToProduction(appName, appID);
                    if (res == null)
                    {

                        DisplayCustomMessageInValidationSummary("Some Error occured while Uploading Files to Production"); 
                        return;
                    }
                    else if (res == "success")
                    {
                        // Create a version for this DB and Version just produced.
                        Finalize fin = new Finalize();
                        string DbVersion = "";

                        if ((DbVersion = fin.AddProductionVerAdmin(appID, appName, "android")) == null)
                        {
                            DisplayCustomMessageInValidationSummary("Some Error occured while finalizing DB for Android.");
                            return;
                        }
                        string topath = producedAppPath + appName.ToLower() + "\\update\\android\\DBVER\\";
                        // Create a version of DB zip files inside a Directory with all the Databases when generated.
                        fin.StoreNewDBtoHistory(appName, appID, "android", DbVersion, topath);


                        if (fin.AddProductionVerAdmin(appID, appName, "ios") == null)
                        {
                            DisplayCustomMessageInValidationSummary("Some Error occured while finalizing DB for iOS.");
                            return;
                        }
                        topath = producedAppPath + appName.ToLower() + "\\update\\ios\\DBVER\\";
                        // Create a version of DB zip files inside a Directory with all the Databases when generated.
                        fin.StoreNewDBtoHistory(appName, appID, "ios", DbVersion, topath);


                        Log.InfoLog(mapPathError, "============================================== FINISHED WITH SUCCESS - PUBLISH TO PRODUCTION FOR " + appName + " ==============================================", appName);

                        // print Success Message...
                        ClientScript.RegisterStartupScript(this.GetType(), "SUCCESS", "alert('Successfully deployed " + appName + " to Production, please download from open wi-fi to confirm.');", true);

                    }
                    else
                    {
                        DisplayCustomMessageInValidationSummary("Some Error occured: "+res);
                        return;
                    }
                }
            }
            else
                DisplayCustomMessageInValidationSummary("Some Error occured, nothing deployed to Production, try again.");

           
            // Refresh screen with new data.
            FetchAppDetailsProduction(appID, appName);
        }

        private string UploadToProduction(string appName, int appID)
        {
            if( UploadFileRemote(appName, producedAppPath + appName + "\\update\\android\\update.zip", appName.ToLower() + "//update//android//update.zip", true) >= 0)     // upload ok android for update.zip
                if( UploadFilesRemote(appName, producedAppPath + appName + "\\update\\android\\images\\", appName.ToLower() + "//update//android//images//", true) >= 0 )   // upload succed for android
                    if( UploadFileRemote(appName, producedAppPath + appName + "\\update\\ios\\update.zip", appName.ToLower() + "//update//ios//update.zip", true) >= 0)     // upload ok for ios update.zip
                        if (UploadFilesRemote(appName, producedAppPath + appName + "\\update\\ios\\images\\", appName.ToLower() + "//update//ios//images//", true) >= 0)   // upload succed for ios
                        { 
                            // upload configurations.txt if exists
                            try
                            {
                                UploadFileRemote(appName, producedAppPath + appName + "\\update\\android\\configuration.txt", appName.ToLower() + "//update//android//configuration.txt");
                                UploadFileRemote(appName, producedAppPath + appName + "\\update\\ios\\configuration.txt", appName.ToLower() + "//update//ios//configuration.txt");
                            }
                            catch (Exception e)
                            {
                                Log.ErrorLog(mapPathError, "Failed to upload Configurations.txt file to production, " + e.Message, "generic");
                                return null;
                            }

                            // upload Staging Versions.txt for both apps - make app raelly live and ready to be downloaded !
                            if (UploadFileRemote(appName, producedAppPath + appName + "\\update\\android\\versions.txt", appName.ToLower() + "//update//android//versions.txt") >= 1)
                                if (UploadFileRemote(appName, producedAppPath + appName + "\\update\\ios\\versions.txt", appName.ToLower() + "//update//ios//versions.txt") >= 1)
                                    return "success";
                                else
                                    return "Could not upload file : " + appName + "\\update\\ios\\versions.txt to production.";
                            else
                                return "Could not upload file : " + appName + "\\update\\android\\versions.txt to production.";
                        } 
                        else
                            return "Could not upload files : " + appName + "\\update\\ios\\images\\ to production.";
                    else
                        return "Could not upload file : " + appName + "\\update\\ios\\update.zip to production.";
                else
                    return "Could not upload files : " + appName + "\\update\\android\\images\\ to production.";
            else
                return "Could not upload file : " + appName + "\\update\\android\\update.zip to production.";
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
                    if (x.Environment == "staging")
                    {
                        foreach (AppVersionDetail y in StagProdAppVersions)
                        {
                            if (y.Environment == "production" && y.Device == x.Device)
                            { 
                                Int32.TryParse(x.DB_Version, out dbverStag);
                                Int32.TryParse(y.DB_Version, out dbverProd);

                                if (dbverStag > dbverProd)
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