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


namespace GGApps
{
    public partial class Publish : Common
    {
        public static string[] SSHCommandsDesc = { 
                                                      "commit"
                                                    , "rollback"
                                                    , "tryCommit"
                                                    , "rollbackLastPublish"
                                                     };

        public enum SSHCommands : long { commit = 0, rollback, tryCommit, rollbackLastPublish };
        public static List<AppVersionDetail> StagProdAppVersions = new List<AppVersionDetail>();
        public static List<MobileDevice> mobileDevicesToPublish = new List<MobileDevice>();

        public class MobileDevice
        {

            public string name { get; set; }
            public string app_Version { get; set; }
            public int db_version { get; set; }
            public string config_version { get; set; }

            public MobileDevice(string device, string aV, int db, string cV)
            {
                this.name = device;
                this.app_Version = aV;
                this.db_version = db;
                this.config_version = cV;

                if (db < 0)
                    throw new Exception();
            }
        }

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
                CheckUndoAvailable(appID, appName);
            }
            else
            {
                BtnPublishApp.CssClass = "InputDisabledCustom";
                BtnPublishApp.Enabled = false;
            }

            ClearCustomMessageValidationSummary();

        }

        private void CheckUndoAvailable(int appID, string appName)
        {
            // check for ios and android
            if ( CheckIfFolderExistsProduction("ios-old", appName.ToLower()) || CheckIfFolderExistsProduction("android-old", appName.ToLower())) 
            {
                undoPublishBtn.CssClass = null;
                undoPublishBtn.Enabled = true;
            }
            else{
                undoPublishBtn.CssClass = "InputDisabledCustom";
                undoPublishBtn.Enabled = false;
            }
        }

        private bool CheckIfFolderExistsProduction(string deviceFolder, string appName)
        {
            return ExistsDirecotryRemote(appName, "//" + appName.ToLower() + "//update//" + deviceFolder.ToLower() + "//");
        }



        private void FetchAppDetailsProduction(int appID, string appName)
        {
            string dbVersion, appVersion, configVersion;
            StagProdAppVersions.Clear();

            if (GetVersionsFileProduction("android", out dbVersion, out appVersion, out configVersion, appName) == null)
                Session["PublishFirstandroid"] = true;
            else
                Session["PublishFirstandroid"] = false;

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


            if (GetVersionsFileProduction("ios", out dbVersion, out appVersion, out configVersion, appName) == null)
                Session["PublishFirstios"] = true;
            else Session["PublishFirstios"] = false;

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
            txtInfoPublishing.InnerHtml = "";
            custValidation.Text = String.Empty;
            custValidation.Enabled = false;
            custValidation.Visible = false;
        }

        public string SSHConnectExecute(string cmdInput, string appName)
        {
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
            string errmsg = "";
            try
            {
                using (SshClient ssh = new SshClient(connectionInfo))
                {
                    ssh.Connect();
                    var command = ssh.RunCommand(cmdInput);
                    errmsg = command.Error;
                    myData = command.Result;
                    ssh.Disconnect();
                }
            }
            catch (Exception e) 
            {
                Log.ErrorLogAdmin(mapPathError, "SSH connection-command error : " + e.Message, appName);
                return "unexpected error in SSH connection.";
            }
            if (!String.IsNullOrEmpty(errmsg))
            {
                Log.ErrorLogAdmin(mapPathError, "SSH connection-command error after execution : " + errmsg, appName);
                return errmsg;
            }
            return "ok";
        }


        protected void BtnPublishApp_Click(object sender, EventArgs e)
        {
            // Check  DB version of staging-production

            mobileDevicesToPublish.Clear();

            string appName = SelectApp.SelectedItem.Text;
            int appID = -1;
            Int32.TryParse(SelectApp.SelectedItem.Value, out appID);

            if (Session["mobileDevicesPublish"] != null )
            {
                if (((List<MobileDevice>)Session["mobileDevicesPublish"]).Count == 0)
                {
                    Session["mobileDevicesPublish"] = null;
                }
                else
                {
                    DisplayCustomMessageInValidationSummary("Please wait for previous publish to finish! Nothing Deployed to Production.");
                    return;
                }
            }

            try
            {

                // Select mobile devices to Publish to Production. 
                foreach (ListViewDataItem item in latestVersions.Items)
                {
                    var chk = item.FindControl("chkSelected") as CheckBox;
                    if (chk != null && chk.Checked == true)
                    {
                        var value = item.FindControl("txtmobileDevice") as TextBox;

                        if ((bool)Session["PublishFirst" + value.Text])
                        {
                            // really check that path not exists.
                            if (!CheckIfFolderExistsProduction(value.Text, appName.ToLower()))
                            {
                                if (InitAppProductionFirst(value.Text, appName.ToLower()) < 0)
                                {
                                    DisplayCustomMessageInValidationSummary("Some error occured while Initializing app to Publish, nothing changed to Produciton.");
                                    return;
                                }
                                else
                                {
                                    // new path  and versions.txt created to prod!
                                    foreach (var ver in StagProdAppVersions)
                                    {
                                        if (ver.Device == value.Text && ver.Environment == "production")
                                        {
                                            ver.DB_Version = "1";
                                            ver.App_Version = "2.1";
                                            ver.Config_Version = "1";
                                        }
                                    }
                                }
                            }

                        }


                        mobileDevicesToPublish.Add(new MobileDevice(
                                                                  value.Text
                                                                , getAppVer(StagProdAppVersions, value.Text, "production")
                                                                , getDBVer(StagProdAppVersions, value.Text, "production")
                                                                , getConfigVer(StagProdAppVersions, value.Text, "production")));
                    }
                }

            }
            catch (Exception xxe)
            {
                Log.ErrorLogAdmin(mapPathError, "Some error occured, whilie init to Publish, " + appName + " ex: " + xxe.Message + xxe.InnerException, appName);
                DisplayCustomMessageInValidationSummary("Some error occured, whilie init to Publish. Nothing Deployed to Production.");
                Session["mobileDevicesPublish"] = null;
                return;

            }


            string mobSuccess = "";
            int[] GGAppsPublishID = new int[2];
            int ipGG1 = 0;


            // try to publish for the below specified applications.
            foreach (var mobileDevice in mobileDevicesToPublish)
            {
                // Create a record in DB to long poll publish !
                GGAppsPublishID[ipGG1] = StartPublishDBAdmin(appID, appName, mobileDevice.name, mobileDevice.app_Version, mobileDevice.db_version, mobileDevice.config_version);

                // print out what to publish.
                txtInfoPublishing.InnerHtml += String.Format("Destination: <b>{0}</b> for <i>{1}</i> Details( V:{2}, D:{3}. C:{4} ) <br/>", appName, mobileDevice.name.ToUpper(), mobileDevice.app_Version, mobileDevice.db_version, mobileDevice.config_version);
                
                ipGG1++;
            }

            Session["mobileDevicesPublish"] = mobileDevicesToPublish;
            

            Log.InfoLog(mapPathError, "Check if staging and production are valid for a publish.", appName);
            string mobDev = "";
            if (!CheckStagingProductionDBVersions(appName, appID, StagProdAppVersions, mobileDevicesToPublish, out mobDev))
            {
                DisplayCustomMessageInValidationSummary("Please check DB version files: " + mobDev + ", " + appName + ". Nothing Deployed to Production.");
                Session["mobileDevicesPublish"] = null;
                return;
            }



            
            HostingEnvironment.QueueBackgroundWorkItem(async ct =>
            {
                int ipGG = 0;
                foreach (var mobileDevice in mobileDevicesToPublish)
                {    
             
                        Log.InfoLog(mapPathError, "============================================== STARTED PUBLISH TO PRODUCTION FOR " + appName + " for " + mobileDevice.name + " ==============================================", appName);
                        string errv ;


                        if (GGAppsPublishID[ipGG] == -1)
                        {
                            FinishPublish(GGAppsPublishID[ipGG], "Could not start publishing app for " + mobileDevice.name + ", please contact Admin!", -1);

                            // update long poll record for publish FAILED!
                            Session["mobileDevicesPublish"] = null;
                            return;
                        }

                        // if is ok must staging_db_version = production_db_version + 1 foreach app. on Staging
                        if ( (errv = RefreshVersionFileStaging(appID, appName, mobileDevice.name)) != "OK" )
                        {
                            FinishPublish(GGAppsPublishID[ipGG], errv + ", while deploying " + appName + " to Production for " + mobileDevice.name + ". Nothing Deployed to Production.", -1);

                            // update long poll record for publish FAILED!
                            Session["mobileDevicesPublish"] = null;
                            return;
                        }

                        // take off_line the app in production. - for publishing
                        if (RenameFileRemote(appName, appName.ToLower() + "/update/" + mobileDevice.name + "/versions.txt", "versions_before_publish.txt") > 0)
                        {

                            // This does the Real UPLOAD !
                            string res = UploadToProduction(GGAppsPublishID[ipGG], appName.ToLower(), appID, mobileDevice.name, mobileDevice.app_Version, mobileDevice.db_version, mobileDevice.config_version);

                            if (res == null)
                            {
                                //DisplayCustomMessageInValidationSummary("Some Error occured while Uploading Files to Production");
                                FinishPublish(GGAppsPublishID[ipGG], "Some Error occured while Uploading Files to Production", -1);

                                // update long poll record for publish FAILED!
                                Session["mobileDevicesPublish"] = null;
                                return;
                            }
                            else if (res == "success")
                            {
                                // Create a version for this DB and Version just produced.
                                Finalize fin = new Finalize();
                                string DbVersion = "";

                                //:ARG: add later an update to have db update with production.!! UpdateAppBundle(publish);.....

                                
                                //if ((DbVersion = fin.AddProductionVerAdmin(appID, appName, mobileDevice.name)) == null)
                                //{
                                    //DisplayCustomMessageInValidationSummary("Some Error occured while finalizing DB for " + mobileDevice);
                                //    FinishPublish(GGAppsPublishID[ipGG], "Some Error occured while finalizing DB for " + mobileDevice.name, -1);

                                    // update long poll record for publish FAILED!
                               //     Session["mobileDevicesPublish"] = null;
                               //     return;
                                //}
                                string topath = producedAppPath + appName.ToLower() + "\\update\\" + mobileDevice.name + "\\DBVER\\";
                        
                                // Create a version of DB zip files inside a Directory with all the Databases when generated.
                                fin.StoreNewDBtoHistory(appName.ToLower(), appID, mobileDevice.name, DbVersion, topath);

                                Log.InfoLog(mapPathError, "============================================== FINISHED SUCCESSFULLY - PUBLISH TO PRODUCTION FOR " + appName + " for " + mobileDevice.name + " ==============================================", appName);

                                // write message to DIV.
                                //DisplayCustomMessageInValidationSummary("Successfully deployed <b>" + appName + "</b> for <i>" + mobileDevice + "</i> to Production, please download app from an open wi-fi to confirm.", true);

                                // update long poll record for publish
                                FinishPublish(GGAppsPublishID[ipGG], "Successfully deployed <b>" + appName + "</b> for <i>" + mobileDevice.name + "</i> to Production, please download app from an open wi-fi to confirm." + mobileDevice.name, 1);

                                // show Modal at the end with result message..
                                //ClientScript.RegisterStartupScript(this.GetType(), "SUCCESS", "window.location.hash = 'openModal';", true);
                            

                            }
                            else
                            {
                                //DisplayCustomMessageInValidationSummary("Some Error occured: " + res);
                                FinishPublish(GGAppsPublishID[ipGG], "Some Error occured: " + res, -1);

                                // update long poll record for publish FAILED!
                                Session["mobileDevicesPublish"] = null;

                                return;
                            }
                        }
                        else
                        {
                            //DisplayCustomMessageInValidationSummary("Cannot rename file remote, nothing deployed to Production for: " + mobileDevice + ", try again ");
                            FinishPublish(GGAppsPublishID[ipGG], "Cannot rename file remote, nothing deployed to Production for: " + mobileDevice.name + ", try again ", -1);

                            // update long poll record for publish FAILED!
                            Session["mobileDevicesPublish"] = null;
                            return;
                        }

                        // ((List<MobileDevice>)Session["mobileDevicesPublish"]).RemoveAll(t => t.name == mobileDevice.name);
                        ipGG++;

                }
                Session["mobileDevicesPublish"] = null;

            });


            // not good code..
            //if (mobileDevicesToPublish.Count == 2)
            //    // open Modal for polling, DB here.
            //    Page.ClientScript.RegisterStartupScript(this.GetType(), "myFuncStatus",
            //                        " statusPub = JSON.stringify({ appid: '" + appID + "', appName: '" + appName.ToLower() + "' , publID1: '" + GGAppsPublishID[0] + "', publID2: '" + GGAppsPublishID[1] + "' }); openModalPublish(); refreshIntervalId = setInterval(getPublishStatus, 5000); ", true);
            //else if (mobileDevicesToPublish.Count == 1)
            //    Page.ClientScript.RegisterStartupScript(this.GetType(), "myFuncStatus",
            //                          " statusPub = JSON.stringify({ appid: '" + appID + "', appName: '" + appName.ToLower() + "' , publID1: '" + GGAppsPublishID[0] + "', publID2: '-1' }); openModalPublish(); refreshIntervalId = setInterval(getPublishStatus, 5000); ", true);
     
            //?
            // do the redirect to Status.
            Session["showInfoPublish"] = txtInfoPublishing.InnerHtml;
            if (mobileDevicesToPublish.Count == 2)
                Response.Redirect("~/PublishStatus.aspx?appID=" + appID + "&appName=" + appName.ToLower() + "&publID1=" + GGAppsPublishID[0] + "&publID2="+  GGAppsPublishID[1]);
            else if (mobileDevicesToPublish.Count == 1)
                Response.Redirect("~/PublishStatus.aspx?appID=" + appID + "&appName=" + appName.ToLower() + "&publID1=" + GGAppsPublishID[0] + "&publID2=-1");


        }

        private int InitAppProductionFirst(string mobileDevice, string appName)
        {
            // provide full path to create, versions.txt default content.
            string res= SSHConnectExecute(String.Format( initFirstPublishSSHcmd
                                                            , appName + "/update/" + mobileDevice
                                                            ,  @"{  ""app_version"": ""2.1"",  ""config_version"": ""1"",  ""db_version"": ""1""}")
                                                            , appName);
            return (res=="ok")?1:-1;
            
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





        private string getAppVer(List<AppVersionDetail> StagProdAppVersions, string mobileDevice, string kind)
        {
            foreach (AppVersionDetail appdet in StagProdAppVersions)
            {
                if (appdet.Device == mobileDevice && appdet.Environment == kind)
                {
                    return appdet.App_Version;
                }
            }
            return null;
        }

        private string getConfigVer(List<AppVersionDetail> StagProdAppVersions, string mobileDevice, string kind)
        {
            
            foreach (AppVersionDetail appdet in StagProdAppVersions)
            {
                if (appdet.Device == mobileDevice && appdet.Environment == kind)
                {
                    return appdet.Config_Version;
                }
            }
            return null;
        }

        private int getDBVer(List<AppVersionDetail> StagProdAppVersions, string mobileDevice, string kind)
        {
            foreach (AppVersionDetail appdet in StagProdAppVersions)
            {
                if (appdet.Device == mobileDevice && appdet.Environment == kind)
                {
                    try
                    {
                        return Int32.Parse(appdet.DB_Version) + 1;  // real prod new version.
                    }
                    catch(Exception )
                    { return -1; }
                }
            }
            return -1;
        }



        private string UploadToProduction(int GGAppsPublishID, string appName, int appID, string mobileDevice, string appVersion, int db_version, string config_version, bool isAppUpdate = false)
        {
            Finalize fin = new Finalize();

            // create filename of zipped
            string filenameToPublish = appName + "-"
                                        + mobileDevice + "-"
                                        + DateTime.Now.ToString("yyyyMMddHHmmss")
                                        + "-v" + appVersion.Replace(".", "_").ToString()
                                        + "-d" + db_version
                                        + "-c" + config_version
                                        + ".zip" 
                                        ;
            if( Session["filenameToPublish_" + mobileDevice] != null )
                Session["filenameToPublish_" + mobileDevice] = null;

            Session.Add("filenameToPublish_" + mobileDevice, filenameToPublish);
            

            // zip file under vm location
            try
            {

                // Create a zip with all files generated under /appName/update/ with name appName.zip.
                var result10 = RunSyncCommandBatch(appID, appName, "12_create_zip_publish.bat "
                                                                + ToPublishZipDir + filenameToPublish 
                                                                + " "
                                                                + producedAppPath + appName + "\\update\\" + mobileDevice + "\\*"
                                                                , actualWorkDir, "Zip all files created under UPDATE to zip file for Mobile", mapPathError, Log);

            }
            catch (Exception e)
            {
                Log.ErrorLogAdmin(mapPathError, "Failed to Zip file " + filenameToPublish + " for Publish", "generic");
            }

            // Do the upload to Production FTP.
            if (UploadFileRemote(appName, ToPublishZipDir + Session["filenameToPublish_" + mobileDevice].ToString()
                                    , appName.ToLower() + "//update//" + filenameToPublish
                                    , true) >= 0)     // upload ok for Bundle device app
            {


                // Unizp through ssh the .zip file to correct destination.
                //unzip {0} -d {1}-new
                if (SSHConnectExecute(String.Format(unzipFileSSHcmd, appName + "/update/" + filenameToPublish, appName + "/update/" + mobileDevice), appName) != "ok")
                    return "unziped failed to Production";

                // replace <device>-old -> new
                // rm -rf {0}-old &amp;&amp; mv {0} {0}-old &amp;&amp; mv {0}-new {0};
                if (SSHConnectExecute(String.Format(replaceDeviceOldSSHcmd, appName + "/update/" + mobileDevice), appName) != "ok")
                    return "replace <device> with <device>-new failed.";

                if (RenameFileRemote(appName, "versions_before_publish.txt", "versions.txt") <= 0)
                    return "failed to rename Versions.txt while Publishing";


                /* DB_VERSION prepare for UNDO, +2  */
                string dbver, appVertmp, configVersion;
                int newdbVersion;
                if (GetVersionsFileProduction(mobileDevice + "-old", out dbver, out appVertmp, out configVersion, appName, "versions_before_publish.txt") != null)
                {
                    Int32.TryParse(dbver, out newdbVersion);
                    newdbVersion = newdbVersion + 2;

                    string nVersionTXT = "{  \"app_version\": \"" + appVertmp + "\",   \"config_version\": \"" + configVersion + "\",   \"db_version\": \"" + newdbVersion + "\" }";

                    // upload to server
                    string res = SaveConfigureFile(mobileDevice + "-old", appName, appID, "Production", "versions.txt", nVersionTXT);
                    if (res == null)
                        return "error while update db_version for <device>-old.";    // error

                }
                else
                    return "failed to get <device>-old versions.txt";


                // Delete temp zip filename to publish from production.
                if (DeleteFileRemote(appName, appName + "/update/" + filenameToPublish) < 0)
                    return "failed to delete zip file from production.";

                return "success";
            }

            return null;

           
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

                // Set Versions file for device only for one Lang
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



        private bool CheckStagingProductionDBVersions(string appName, int appID, List<AppVersionDetail> StagProdAppVersions, List<MobileDevice> mobileDevicesSelected, out string mobDev)
        {
            mobDev = "";
            foreach (var mD in mobileDevicesSelected)
            {
                if (!CheckStagingProductionDBVersionsDevice(appName, appID, StagProdAppVersions, mD.name))
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
            // if is visible then undo is possible.
            DropDownList SelectApp = (DropDownList)LoginViewImportant.FindControl("SelectApp");
            if (SelectApp.SelectedIndex > 0)
            {
                Int32 appID = Int32.Parse(SelectApp.SelectedValue);
                String appName = SelectApp.SelectedItem.ToString();


                string msg =  UndoPublishProduction(appID, appName.ToLower());


                ClearCustomMessageValidationSummary();
                FetchAppDetailsProduction(appID, appName);
                CheckUndoAvailable(appID, appName);
                DisplayCustomMessageInValidationSummary(msg);
               
            }
        }


        private string UndoPublishProduction(int appID, string appName)
        {
            string msg1 = SSHConnectExecute(String.Format(undoPublishSSHcmd, appName + "/update/android"), appName);
            if ( msg1 != "ok")
            {
                string msg = "Error occured while trying to Undo Last Publish for Android: " + appName + " msg: " + msg1;
                Log.ErrorLogAdmin(mapPathError, msg, appName);
                return msg;
            }

            msg1 = SSHConnectExecute(String.Format(undoPublishSSHcmd, appName + "/update/ios"), appName);
            if(msg1 != "ok")
            {
                string msg = "Error occured while trying to Undo Last Publish for iOS: " + appName + " msg: " + msg1;
                Log.ErrorLogAdmin(mapPathError, msg, appName);
                return msg;
            }

            return "Successfully Undo Last Publish.";
        }



        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod]
        public static bool GetPublishStatus(string appid, string appName, string publID1, string publID2)
        {
            int _appID = -1;
            int _publID1 = -1;
            int _publID2 = -1;

            Int32.TryParse(appid, out _appID);
            Int32.TryParse(publID1, out _publID1);
            Int32.TryParse(publID2, out _publID2);


            bool doneAll = false;

            if (_publID2 == -1 && _publID1 > 0)
            {

                int ck1 = CheckPublIsReady(_publID1);
                if (ck1 == 1)
                    // -- 1: done, 0: working -1: failed
                    doneAll = true;


            }
            else if (_publID2 > 0)
            {
                int ck1 = CheckPublIsReady(_publID1);
                int ck2 = CheckPublIsReady(_publID2);
                if (ck1 == 1 && ck2 == 1)
                    doneAll = true;
                // -- 1: done, 0: working -1: failed
            }


            if (doneAll)
            {
                // make a refresh to screen client side!
                return doneAll;
            }

            return doneAll;

        }


        private static int CheckPublIsReady(int publID1)
        {
            int res = -1;
            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
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



    }
}


// NOT USED
/// <summary>
/// Upload new App, for a Mobile Device to production. Upload images, configuration.txt and last versions.txt from staging.
/// </summary>
/// <param name="appName"></param>
/// <param name="appID"></param>
/// <param name="mobileDevice"></param>
/// <param name="isAppUpdate"></param>
/// <returns></returns>
/*
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
*/