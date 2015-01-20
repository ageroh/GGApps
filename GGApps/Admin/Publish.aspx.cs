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

            CustomValidator CustomValidatorCtrl = new CustomValidator();

            CustomValidatorCtrl.IsValid = false;
            CustomValidatorCtrl.Text = message;
            CustomValidatorCtrl.ErrorMessage = message;

            BtnPublishApp.Controls.Add(CustomValidatorCtrl);
//            this.Page.Controls.Add(CustomValidatorCtrl);

        }  

        protected void cusCustom_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            if (e.Value.Length == 8)
                e.IsValid = true;
            else
                e.IsValid = false;
        }



        protected void BtnPublishApp_Click(object sender, EventArgs e)
        {
            // Check  DB version of staging-production
            
            string appName = SelectApp.SelectedItem.Text;
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);
#if DEBUG
            appName = "HYDRATEST";
            appID = 999;
#endif


            if (!CheckStagingProductionDBVersions(appName, appID, StagProdAppVersions))
            {
                DisplayCustomMessageInValidationSummary("Please check DB version files.");
                return;
            }

            // if is ok must staging_db_version = production_db_version + 1 foreach app. on Staging
            if (!RefreshVersionFileStaging(appID, appName, "ios"))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "error", "alert('error! while deploying " + appName + " to production!');", true);
                Response.Write("<span class='ErrorGeneral'>Some Error occured please contact Admin!</span>");
                return;
            }

            // if is ok must staging_db_version = production_db_version + 1 foreach app. on Staging
            if (!RefreshVersionFileStaging(appID, appName, "android"))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "error", "alert('error! while deploying " + appName + " to production!');", true);
                Response.Write("<span class='ErrorGeneral'>Some Error occured please contact Admin!</span>");
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

                        Response.Write("<span class='ErrorGeneral'>Some Error occured while Uploading Files to Production!</span>");
                        return;
                    }
                    else if (res == "success")
                    {
                        // print Success Message...
                        ClientScript.RegisterStartupScript(this.GetType(), "SUCCESS", "alert('SUCCESS! " + appName + " is up to Production, ready to download.');", true);
                        // maybe add a DB record for this deployment.
                    }
                    else
                    {

                        ClientScript.RegisterStartupScript(this.GetType(), "Error!", "alert('ERROR ! " + res + "');", true);
                        Response.Write("<span class='ErrorGeneral'>Some Error occured: " + res + " </span>");
                    }
                }

            }
            else
                Response.Write("<span class='ErrorGeneral'>Some Error occured, nothing deployed!</span>");

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