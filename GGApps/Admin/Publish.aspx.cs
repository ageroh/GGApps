using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Renci.SshNet.Channels;
using Renci.SshNet;

using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

//using System.Data;
//using System.Collections.Generic;
//using System.Diagnostics.Tracing;
//using System.Diagnostics;
//using System.Text;
//using System.IO;
//using System.Xml;

namespace GGApps
{
    public partial class Publish : Common
    {
        public static string[] CommitCommandsDesc = { "commit", "rollback", "tryCommit", "rollbackLastPublish" };
        public enum CommitCommands : long { commit = 0, rollback, tryCommit, rollbackLastPublish };

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


        private void DisplayCustomMessageInValidationSummary(string message, bool append = false)
        {
            if (append == true)
            {
                //custValidation.Text = custValidation.Text + "<p>" + message + "</p>";
                txtMessageModal.InnerHtml = txtMessageModal.InnerHtml + "<p>" + message + "</p>";
                //custValidation.CssClass = "SuccessGeneral";
            }
            else
            {
                custValidation.Text = message;
                //custValidation.CssClass = "ErrorGeneral";
                custValidation.Enabled = true;
                custValidation.Visible = true;
            }
            

        }


        private void ClearCustomMessageValidationSummary()
        {
            txtMessageModal.InnerHtml = "";
            custValidation.Text = String.Empty;
            custValidation.Enabled = false;
            custValidation.Visible = false;
        }

        public string SSHConnectExecute(string cmdInput, string appName)
        {
            // NOT YET COMPLETED, NEEDS TO PRODUCE THE CORRECT COMMIT, ROLLBACK COMMANDS etc..
            return "";

            // setup the correct connection for GIT server!
            string gitUserFTP = "";
            string gitPassFTP="";
            string gitHostFTP="";

            if (rootWebConfig.AppSettings.Settings["GitUserFTP"] != null)
                gitUserFTP = rootWebConfig.AppSettings.Settings["GitUserFTP"].Value.ToString();

            if (rootWebConfig.AppSettings.Settings["GitPassFTP"] != null)
                gitPassFTP = rootWebConfig.AppSettings.Settings["GitPassFTP"].Value.ToString();

            if (rootWebConfig.AppSettings.Settings["GitHostFTP"] != null)
                gitHostFTP = rootWebConfig.AppSettings.Settings["GitHostFTP"].Value.ToString();

            if (String.IsNullOrEmpty(gitHostFTP) || String.IsNullOrEmpty(gitPassFTP) || String.IsNullOrEmpty(gitUserFTP))
            {
                Log.ErrorLogAdmin(mapPathError, "SSH connection: PLEASE PROVIDE SSH CREDENTIALS TO CONNECT for: " + appName , "generic");
                return null;
            }

            var PasswordConnection = new PasswordAuthenticationMethod(gitUserFTP, gitPassFTP);
            var KeyboardInteractive = new KeyboardInteractiveAuthenticationMethod(gitUserFTP);
            string myData = null;

            var connectionInfo = new ConnectionInfo(gitHostFTP, 22, gitUserFTP, PasswordConnection, KeyboardInteractive);

            try
            {
                using (SshClient ssh = new SshClient(connectionInfo))
                {
                    ssh.Connect();
                    var command = ssh.RunCommand(cmdInput);
                    myData = command.Result;
                    ssh.Disconnect();
                }
            }
            catch (Exception e) 
            {
                Log.ErrorLogAdmin(mapPathError, "SSH connection-command error : " + e.Message, appName);
                return null;
            }

            return myData;
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


            if (Session["mobileDevicesPublish"] != null)
            {
                DisplayCustomMessageInValidationSummary("Please wait for previous publish to finish! Nothing Deployed to Production.");
                return;
            }

            Session["mobileDevicesPublish"] = mobileDevicesToPublish;

                      
    #if DEBUG
            Log.InfoLog(mapPathError, "this is a SHH connection test and run: " + SSHConnectExecute(CommitCommandsDesc[(int)CommitCommands.commit], appName), appName);
    #endif


 
            Log.InfoLog(mapPathError, "Check if staging and production are valid for a publish.", appName);
            string mobDev = "";
            if (!CheckStagingProductionDBVersions(appName, appID, StagProdAppVersions, mobileDevicesToPublish, out mobDev))
            {
                DisplayCustomMessageInValidationSummary("Please check DB version files: " + mobDev + ", " + appName + ". Nothing Deployed to Production.");
                Session["mobileDevicesPublish"] = null;
                return;
            }

            // check return message.
#if DEBUG 
            SSHConnectExecute(CommitCommandsDesc[(int)CommitCommands.tryCommit], appName);
#endif
            string mobSuccess = "";
            int [] GGAppsPublishID = new int[2];
            int ipGG = 0;
            

            // Will to try to publish for the below specified applications.
            foreach (string mobileDevice in mobileDevicesToPublish) 
            {
                // Create a record in DB to long poll publish !
                GGAppsPublishID[ipGG] = StartPublishDBAdmin(appID, appName, mobileDevice, getAppVer(StagProdAppVersions, mobileDevice), getDBVer(StagProdAppVersions, mobileDevice), getConfigVer(StagProdAppVersions, mobileDevice));
                ipGG++;
            }
            ipGG = 0;

            HostingEnvironment.QueueBackgroundWorkItem(async ct =>
            {

                foreach (string mobileDevice in mobileDevicesToPublish)
                {    
             
                        Log.InfoLog(mapPathError, "============================================== STARTED PUBLISH TO PRODUCTION FOR " + appName + " for " + mobileDevice + " ==============================================", appName);
                        string errv ;


                        if (GGAppsPublishID[ipGG] == -1)
                        {
                            FinishPublish(GGAppsPublishID[ipGG], "Could not start publishing app for " + mobileDevice + ", please contact Admin!", -1);

                            // update long poll record for publish FAILED!
                            Session["mobileDevicesPublish"] = null;
    #if DEBUG
                            SSHConnectExecute(CommitCommandsDesc[(int)CommitCommands.rollback], appName);
    #endif
                        }

                        // if is ok must staging_db_version = production_db_version + 1 foreach app. on Staging
                        if ( (errv = RefreshVersionFileStaging(appID, appName, mobileDevice)) != "OK" )
                        {
                            FinishPublish(GGAppsPublishID[ipGG], errv + ", while deploying " + appName + " to Production for " + mobileDevice + ". Nothing Deployed to Production.", -1);

                            // update long poll record for publish FAILED!
                            Session["mobileDevicesPublish"] = null;
        #if DEBUG 
                            SSHConnectExecute(CommitCommandsDesc[(int)CommitCommands.rollback], appName);
        #endif
                            return;
                        }

                        // take off_line the app in production.
                        if (RenameFileRemote(appName, appName.ToLower() + "/update/" + mobileDevice + "/versions.txt", "versions_" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".txt") > 0)
                        {

                            // This does the Real UPLOAD !
                            string res = UploadToProduction(appName, appID, mobileDevice);

                            if (res == null)
                            {
                                //DisplayCustomMessageInValidationSummary("Some Error occured while Uploading Files to Production");
                                FinishPublish(GGAppsPublishID[ipGG], "Some Error occured while Uploading Files to Production", -1);

                                // update long poll record for publish FAILED!
                                Session["mobileDevicesPublish"] = null;
        #if DEBUG 
                                SSHConnectExecute(CommitCommandsDesc[(int)CommitCommands.rollback], appName);
        #endif
                                return;
                            }
                            else if (res == "success")
                            {
                                // Create a version for this DB and Version just produced.
                                Finalize fin = new Finalize();
                                string DbVersion = "";

                                if ((DbVersion = fin.AddProductionVerAdmin(appID, appName, mobileDevice)) == null)
                                {
                                    //DisplayCustomMessageInValidationSummary("Some Error occured while finalizing DB for " + mobileDevice);
                                    FinishPublish(GGAppsPublishID[ipGG], "Some Error occured while finalizing DB for " + mobileDevice, -1);

                                    // update long poll record for publish FAILED!
                                    Session["mobileDevicesPublish"] = null;
        #if DEBUG
                                    SSHConnectExecute(CommitCommandsDesc[(int)CommitCommands.rollback], appName);
        #endif
                                    return;
                                }
                                string topath = producedAppPath + appName.ToLower() + "\\update\\" + mobileDevice + "\\DBVER\\";
                        
                                // Create a version of DB zip files inside a Directory with all the Databases when generated.
                                fin.StoreNewDBtoHistory(appName, appID, mobileDevice, DbVersion, topath);

                                // Reset versions and configuration files of staging ?  or get in case of update data only for Admin DB...
                                // NOT IMPLEMENTED YET!
                                //
                                //undoPublish.Enabled = true;
                                //

                                Log.InfoLog(mapPathError, "============================================== FINISHED WITH SUCCESS - PUBLISH TO PRODUCTION FOR " + appName + " for " + mobileDevice + " ==============================================", appName);

                                // write message to DIV.
                                //DisplayCustomMessageInValidationSummary("Successfully deployed <b>" + appName + "</b> for <i>" + mobileDevice + "</i> to Production, please download app from an open wi-fi to confirm.", true);

                                // update long poll record for publish
                                FinishPublish(GGAppsPublishID[ipGG], "Successfully deployed <b>" + appName + "</b> for <i>" + mobileDevice + "</i> to Production, please download app from an open wi-fi to confirm." + mobileDevice, 1);

                                // show Modal at the end with result message..
                                //ClientScript.RegisterStartupScript(this.GetType(), "SUCCESS", "window.location.hash = 'openModal';", true);
                            

                            }
                            else
                            {
                                //DisplayCustomMessageInValidationSummary("Some Error occured: " + res);
                                FinishPublish(GGAppsPublishID[ipGG], "Some Error occured: " + res, -1);

                                // update long poll record for publish FAILED!
                                Session["mobileDevicesPublish"] = null;

        #if DEBUG
                                SSHConnectExecute(CommitCommandsDesc[(int)CommitCommands.rollback], appName);
        #endif
                                return;
                            }
                        }
                        else
                        {
                            //DisplayCustomMessageInValidationSummary("Cannot rename file remote, nothing deployed to Production for: " + mobileDevice + ", try again ");
                            FinishPublish(GGAppsPublishID[ipGG], "Cannot rename file remote, nothing deployed to Production for: " + mobileDevice + ", try again ", -1);

                            // update long poll record for publish FAILED!
                            Session["mobileDevicesPublish"] = null;
        #if DEBUG
                            SSHConnectExecute(CommitCommandsDesc[(int)CommitCommands.rollback], appName);
        #endif
                            return;
                        }
                }
            });

    #if DEBUG
                SSHConnectExecute(CommitCommandsDesc[(int)CommitCommands.commit], appName);
    #endif

            // clear session variable
            Session["mobileDevicesPublish"] = null;

            // not good code..
            if( ipGG > 1 )
                // open Modal for polling, DB here.
                ClientScript.RegisterStartupScript(this.GetType(), "INFO", "var statusPub = '{'statusPubData':['" + appID + "', '" + appName + "', '" + GGAppsPublishID[0] + "', '" + GGAppsPublishID[1] + "'  ]}'; getPublishStatus();", true);
            else if( ipGG > 0 )
                ClientScript.RegisterStartupScript(this.GetType(), "INFO", "var statusPub = '{'statusPubData':['" + appID + "', '" + appName + "', '" + GGAppsPublishID[0] + "' ]}'; getPublishStatus();", true);



           
        }

        /// <summary>
        /// Start publishng an application and a select every 30sec to poll the DB to see if publish is finished.
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        /// <param name="mobileDevice"></param>
        /// <param name="appVer"></param>
        /// <param name="dbVer"></param>
        /// <param name="configVer"></param>
        /// <returns></returns>
        private int StartPublishDBAdmin(int appID, string appName, string mobileDevice, string appVer, int dbVer, string configVer)
        {
            int result = -1;
            if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
            {
                using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_Start_Publish_App", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;

                        cmd.Parameters.Add("@appName", SqlDbType.NVarChar).Value = appName;
                        cmd.Parameters.Add("@mobileDevice", SqlDbType.NVarChar).Value = mobileDevice;
                        cmd.Parameters.Add("@App_Version", SqlDbType.NVarChar).Value = appVer;
                        cmd.Parameters.Add("@DB_Version", SqlDbType.NVarChar).Value = dbVer.ToString();
                        cmd.Parameters.Add("@Config_Version", SqlDbType.NVarChar).Value = configVer;
                        
                        SqlParameter sout = new SqlParameter("@GGAppsPublishID", SqlDbType.Int);
                        sout.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(sout);

                        con.Open();
                        cmd.ExecuteNonQuery();  // *** since you don't need the returned data - just call ExecuteNonQuery
                        result = (int)cmd.Parameters["@GGAppsPublishID"].Value;
                        con.Close();

                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Write for specified try of publish to DB if it is success or what error occured.
        /// </summary>
        /// <param name="GGAppsPublishID"></param>
        /// <param name="statusDesc"></param>
        /// <param name="status"></param>
        private void FinishPublish(int GGAppsPublishID, string statusDesc, int status)
        {
            if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
            {
                using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_Finish_Publish", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@GGAppsPublishID", SqlDbType.Int).Value = GGAppsPublishID;
                        cmd.Parameters.Add("@StatusDesc", SqlDbType.NVarChar).Value = statusDesc;
                        cmd.Parameters.Add("@Status", SqlDbType.Int).Value = status;

                        con.Open();
                        cmd.ExecuteNonQuery();

                    }
                }
            }

        }



        [System.Web.Services.WebMethod]
        public bool GetStatus(string[] statusPubData)
        {
            int appId = 0;
            string appName = "";
            int publID1 = -1;
            int publID2 = -1;
            bool doneAll = false;

            if (statusPubData.Length == 3)
            {
                appId = Int32.Parse(statusPubData[0]);
                appName = statusPubData[1];
                publID1 = Int32.Parse(statusPubData[2]);

                int ck1 = CheckPublIsReady(publID1);
                if (ck1 == 1)
                    // -- 1: done, 0: working -1: failed
                    doneAll = true;
                

            }
            else if (statusPubData.Length == 4)
            {
                appId = Int32.Parse(statusPubData[0]);
                appName = statusPubData[1];
                publID1 = Int32.Parse(statusPubData[2]);
                publID2 = Int32.Parse(statusPubData[3]);

                int ck1 = CheckPublIsReady(publID1);
                int ck2 = CheckPublIsReady(publID2);
                if (ck1 == 1 && ck2 == 1)
                    doneAll = true;
                // -- 1: done, 0: working -1: failed
            }


            if (doneAll)
            {
                // make a refresh to screen ?
                FetchAppDetailsProduction(appId, appName);
                return doneAll;
            }

            return doneAll;

        }

        private int CheckPublIsReady(int publID1)
        {
            int res = -1;
            if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
            {
                using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_Check_Publishing", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@GGAppsPublishID", SqlDbType.Int).Value = publID1;
                        
                        con.Open();
                        res = (int)cmd.ExecuteScalar();
                        // -- 1: done, 0: working -1: failed
                       

                    }
                }
            }
            return res;
        }




        private string getAppVer(List<AppVersionDetail> StagProdAppVersions, string mobileDevice)
        {
            foreach (AppVersionDetail appdet in StagProdAppVersions)
            {
                if (appdet.Device == mobileDevice && appdet.Environment == "Staging")
                {
                    return appdet.App_Version;
                }
            }
            return null;
        }

        private string getConfigVer(List<AppVersionDetail> StagProdAppVersions, string mobileDevice)
        {
            foreach (AppVersionDetail appdet in StagProdAppVersions)
            {
                if (appdet.Device == mobileDevice && appdet.Environment == "Staging")
                {
                    return appdet.Config_Version;
                }
            }
            return null;
        }

        private int getDBVer(List<AppVersionDetail> StagProdAppVersions, string mobileDevice)
        {
            foreach (AppVersionDetail appdet in StagProdAppVersions)
            {
                if (appdet.Device == mobileDevice && appdet.Environment == "Production")
                {
                    return Int32.Parse(appdet.DB_Version) + 1;  // real prod new version.
                }
            }
            return -1;
        }




        /// <summary>
        /// Upload new App, for a Mobile Device to production. Upload images, configuration.txt and last versions.txt from staging.
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appID"></param>
        /// <param name="mobileDevice"></param>
        /// <param name="isAppUpdate"></param>
        /// <returns></returns>
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
                        Log.ErrorLogAdmin(mapPathError, "Failed to upload Configurations.txt file to production for " + mobileDevice + ", " + e.Message, "generic");
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



        private string RefreshVersionFileStaging(int appID, string appName, string mobileDevice)
        {
            string retError = " Some Error occured ";
            Finalize fin = new Finalize(appName, appID);
            string dbver;
            string appVersion = "", configVersion = "", appVersionReal = "";
            int newdbVersion=0;

            if (StagProdAppVersions == null)
                return retError;

            if (StagProdAppVersions != null)
                if (StagProdAppVersions.Count <= 0)
                    return retError;

            Log.InfoLog(mapPathError, "Refresh version txt on staging before deploy to production for:" + mobileDevice, appName);

            // Get the Db_Version of production
            if (GetVersionsFileProduction(mobileDevice, out dbver, out appVersion, out configVersion, appName) != null)
            {
                Int32.TryParse(dbver, out newdbVersion);
                // add + 1 to production db_version.
                newdbVersion++;                         

                // Set Versions file for IOS only for one Lang
                if (!fin.SetVerionsFileProperty("db_version", newdbVersion.ToString(), appName, appID, mobileDevice, 1, "versions.txt"))
                    return retError;

                // set also the property for App Version taken from taken from Python script.
                int rlver = fin.GetRealAppVersion(appName, appID, mobileDevice, out appVersionReal);
                if (rlver == 1)
                {
                    if (appVersionReal != appVersion)
                    {
                        Log.InfoLog(mapPathError, "App Version had changed for " + mobileDevice, appName);
                    }
                    if (!fin.SetVerionsFileProperty("app_version", appVersionReal, appName, appID, mobileDevice, 1, "versions.txt"))
                        return retError;
                }
                else if (rlver == -1)
                {
                    Log.ErrorLogAdmin(mapPathError, "Error occured in GetRealAppVersion writing for " + mobileDevice + " in DB  " + appName, appName);
                    return retError;
                }
                else 
                {
                    Log.ErrorLogAdmin(mapPathError, "Please check the automatic Application Versioning process(python), nothing deployed to Production for: " + mobileDevice + " in DB  " + appName, appName);

                    // error message to check update for Python script. 
                    return "Please check the automatic Application Versioning process(python), nothing deployed to Production, contact Admins!";
                }


                // Add a row to Admin DB for history reasons, to create e new MAJOR number.
                if (fin.AddDBversionAdmin(appName, appID, dbver, newdbVersion.ToString(), mobileDevice, "EN", newdbVersion) == null)
                {
                    Log.ErrorLogAdmin(mapPathError, "Error occured in AddDBversionAdmin writing for " + mobileDevice + " in DB  " + appName, appName);
                    return retError;
                }
            }
            else
                return retError;

            Log.InfoLog(mapPathError, "Successfully!", appName);

            return "OK";
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

            ClearCustomMessageValidationSummary();
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

        protected void undoPublish_Click(object sender, EventArgs e)
        {
#if DEBUG
            SSHConnectExecute( CommitCommandsDesc[(int)CommitCommands.rollbackLastPublish], Session["appName"].ToString() );
#endif
            DisplayCustomMessageInValidationSummary("Not implemented yet!");
            undoPublish.Enabled = false;
        }





    }
}