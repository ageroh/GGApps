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

            int rc = 0;
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
            prodAndName.Text = appName;
            StagProdAppVersions.Add(new AppVersionDetail("production", "android", dbVersion, appVersion, configVersion, appName, appID));

            GetVersionsFile("android", out dbVersion, out appVersion, out configVersion, appName);

            stagAndDB.Text = dbVersion;
            stagAndAV.Text = appVersion;
            stagAndCV.Text = configVersion;
            stagAndName.Text = appName;
            StagProdAppVersions.Add(new AppVersionDetail("staging", "android", dbVersion, appVersion, configVersion, appName, appID));

            
            GetVersionsFileProduction("ios", out dbVersion, out appVersion, out configVersion, appName);

            prodIosDB.Text = dbVersion;
            prodIosAV.Text = appVersion;
            prodIosCV.Text = configVersion;
            prodIosName.Text = appName;
            StagProdAppVersions.Add(new AppVersionDetail("production", "ios", dbVersion, appVersion, configVersion, appName, appID));

            GetVersionsFile("ios", out dbVersion, out appVersion, out configVersion, appName);

            stagIosDB.Text = dbVersion;
            stagIosAV.Text = appVersion;
            stagIosCV.Text = configVersion;
            stagIosName.Text = appName;
            StagProdAppVersions.Add(new AppVersionDetail("staging", "ios", dbVersion, appVersion, configVersion, appName, appID));

            InitOnOffLineApps(appName);

        }

        private void InitOnOffLineApps(string appName)
        {
            // show either online or offline buttons.
            InitLiveControl(stagAndLIVEonoff, CheckVersionsFile(appName, "android"));
            InitLiveControl(stagIosLIVEonoff, CheckVersionsFile(appName, "ios"));
            InitLiveControl(prodAndLIVEEonoff, CheckVersionsFileProduction(appName, "android"));
            InitLiveControl(prodIosLIVEonoff, CheckVersionsFileProduction(appName, "ios"));
        }

        private void InitLiveControl(Button onoffbtn, bool status)
        {
            if (status)
            {
                onoffbtn.BackColor = System.Drawing.Color.Green;
                onoffbtn.Text = "On-Line";
            }
            else
            {
                onoffbtn.BackColor = System.Drawing.Color.Red;
                onoffbtn.Text = "Off-Line";
            }

            onoffbtn.Visible = true;
            onoffbtn.Enabled = true;
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
            List<string> mobileDevicesToPublish = new List<string>();
            mobileDevicesToPublish.Clear();

            string appName = SelectApp.SelectedItem.Text;
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);


            // Select mobile devices to Publish to Production. 
            foreach (ListViewDataItem item in latestVersions.Items)
            {
                var chk = item.FindControl("chkSelected") as CheckBox;
                if (chk != null && chk.Checked == true)
                {
                    var value = item.FindControl("txtmobileDevice") as TextBox;
                    mobileDevicesToPublish.Add(value.Text);
                }
            }

            Session["mobileDevicesPublish"] = mobileDevicesToPublish;


            Log.InfoLog(mapPathError, "Check if staging and production are valid for a publish.", appName);
            string mobDev = "";
            if (!CheckStagingProductionDBVersions(appName, appID, StagProdAppVersions, mobileDevicesToPublish, out mobDev))
            {
                DisplayCustomMessageInValidationSummary("Please check DB version files: " + mobDev + ", " + appName + ". Nothing Deployed to Production.");
                return;
            }


            foreach (string mobileDevice in mobileDevicesToPublish) 
            {
                Log.InfoLog(mapPathError, "============================================== STARTED PUBLISH TO PRODUCTION FOR " + appName + " for " + mobileDevice + " ==============================================", appName);
                // if is ok must staging_db_version = production_db_version + 1 foreach app. on Staging
                if (!RefreshVersionFileStaging(appID, appName, mobileDevice))
                {
                    DisplayCustomMessageInValidationSummary("Error, while deploying " + appName + " to Production for " + mobileDevice + ". Nothing Deployed to Production.");
                    return;
                }

                if (RenameFileRemote(appName, appName.ToLower() + "/update/" + mobileDevice + "/versions.txt", "versions_" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".txt") > 0)
                {

                    // This does the Real UPLOAD !
                    string res = UploadToProduction(appName, appID, mobileDevice);

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

                        if ((DbVersion = fin.AddProductionVerAdmin(appID, appName, mobileDevice)) == null)
                        {
                            DisplayCustomMessageInValidationSummary("Some Error occured while finalizing DB for " + mobileDevice);
                            return;
                        }
                        string topath = producedAppPath + appName.ToLower() + "\\update\\" + mobileDevice + "\\DBVER\\";
                        
                        // Create a version of DB zip files inside a Directory with all the Databases when generated.
                        fin.StoreNewDBtoHistory(appName, appID, mobileDevice, DbVersion, topath);

                        // Reset versions and configuration files of staging ?  or get in case of update data only for Admin DB...
                        // ?

                        Log.InfoLog(mapPathError, "============================================== FINISHED WITH SUCCESS - PUBLISH TO PRODUCTION FOR " + appName + " for " + mobileDevice + " ==============================================", appName);

                        // print Success Message...
                        ClientScript.RegisterStartupScript(this.GetType(), "SUCCESS", "alert('Successfully deployed " + appName + " for " + mobileDevice + " to Production, please download from open wi-fi to confirm.');", true);

                    }
                    else
                    {
                        DisplayCustomMessageInValidationSummary("Some Error occured: " + res);
                        return;
                    }
                }
                else
                {
                    DisplayCustomMessageInValidationSummary("Some Error occured, nothing deployed to Production for: " + mobileDevice + ", try again ");
                    return;
                }
            
            }


            // Refresh screen with new data.
            FetchAppDetailsProduction(appID, appName);

            // clear session variable
            Session["mobileDevicesPublish"] = null;
        }




        private string UploadToProduction(string appName, int appID, string mobileDevice, bool isAppUpdate = false)
        {

            if (UploadFilesRemote(appName, producedAppPath + appName + "\\update\\" + mobileDevice + "\\images\\", appName.ToLower() + "//update//" + mobileDevice + "//images//", true) >= 0)   // upload succeed for device
            {
                if (isAppUpdate)
                {
                    Log.InfoLog(mapPathError, "App Update for selected Application " + appName + " on " + mobileDevice + ", Successfully moved all /IMAGES files to Production!", appName);
                    return "success";
                }

                if (UploadFileRemote(appName, producedAppPath + appName + "\\update\\" + mobileDevice + "\\update.zip", appName.ToLower() + "//update//" + mobileDevice + "//update.zip", true) >= 0)     // upload ok for update.zip
                {
                    // upload configurations.txt if exists
                    try
                    {
                        UploadFileRemote(appName, producedAppPath + appName + "\\update\\" + mobileDevice + "\\configuration.txt", appName.ToLower() + "//update//" + mobileDevice + "//configuration.txt");
                    }
                    catch (Exception e)
                    {
                        Log.ErrorLog(mapPathError, "Failed to upload Configurations.txt file to production for " + mobileDevice + ", " + e.Message, "generic");
                        return null;
                    }

                    // upload Staging Versions.txt for both apps - make app raelly live and ready to be downloaded !
                    if (UploadFileRemote(appName, producedAppPath + appName + "\\update\\" + mobileDevice + "\\versions.txt", appName.ToLower() + "//update//" + mobileDevice + "//versions.txt") >= 1)
                        return "success";
                    else
                        return "Could not upload file : " + appName + "\\update\\" + mobileDevice + "\\versions.txt to production.";

                }
                else
                    return "Could not upload file : " + appName + "\\update\\" + mobileDevice + "\\update.zip to production.";
            }
            else
                return "Could not upload files : " + appName + "\\update\\" + mobileDevice + "\\images\\ to production.";
                
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



        private bool CheckStagingProductionDBVersions(string appName, int appID, List<AppVersionDetail> StagProdAppVersions, List<string> mobileDevicesSelected, out string mobDev)
        {
            mobDev = "";
            foreach (string mD in mobileDevicesSelected)
            {
                mobDev = mD;
                if (!CheckStagingProductionDBVersionsDevice(appName, appID, StagProdAppVersions, mD))
                    return false;
            }

            return true;

        }


        private bool CheckStagingProductionDBVersionsDevice(string appName, int appID, List<AppVersionDetail> StagProdAppVersions, string mobileDevice)
        {
            int dbverStag, dbverProd;

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

                                if (dbverStag > dbverProd && y.Device == mobileDevice)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
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

        protected void stagAndLIVEonoff_Click(object sender, EventArgs e)
        {
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);
            ToggleOnLine_OffLine(appID, SelectApp.SelectedItem.Text, "android", "staging");
        }

        protected void prodAndLIVEEonoff_Click(object sender, EventArgs e)
        {
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);
            ToggleOnLine_OffLine(appID,SelectApp.SelectedItem.Text, "android", "production");
        }

        protected void stagIosLIVEonoff_Click(object sender, EventArgs e)
        {
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);
            ToggleOnLine_OffLine(appID, SelectApp.SelectedItem.Text, "ios", "staging");
        }

        protected void prodIosLIVEonoff_Click(object sender, EventArgs e)
        {
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);
            ToggleOnLine_OffLine(appID,SelectApp.SelectedItem.Text, "ios", "production");
        }


        private void ToggleOnLine_OffLine(int appID, string appName, string mobileDevice, string environment)
        {

            if (environment == "staging")
            { 
              if( CheckVersionsFile(appName, mobileDevice) )    
                  PutOffLine(appName, mobileDevice, environment);   // put offline!
              else
                  PutOnLine(appName, mobileDevice, environment);    // put online!
            }

            if (environment == "production")
            { 
                if( CheckVersionsFileProduction(appName, mobileDevice) )    
                  PutOffLineProduction(appName, mobileDevice, environment);   // put offline!
              else
                  PutOnLineProduction(appName, mobileDevice, environment);    // put online!
            }

            InitOnOffLineApps(appName);
            FetchAppDetailsProduction(appID, appName);
        }





    }
}