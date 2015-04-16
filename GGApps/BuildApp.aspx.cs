using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics.Tracing;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Configuration;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Data.SQLite;

namespace GGApps
{
    public partial class BuildApp : Common
    {

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!Page.IsPostBack)
            {

                if (Session["appName"] != null && Session["appID"] != null)
                {


                    if (!Init_BuildApp(Convert.ToInt32(Session["appID"].ToString()), Session["appName"].ToString()))
                    {

                        Log.ErrorLogAdmin(mapPathError, "Failed to Initialize In-App update for <span class='appName'>" + Session["appName"].ToString() + "</span>", Session["appName"].ToString());
                        AddMessageToScreen("ExecutionMessages",
                                            @"<h2>Failed to Initialize In-App update, <strong>please contact Admin!</strong></h2>", this.Page);

                    }
                    else
                    {
                        // ready to upDate!
                        lbl1.Text = "Ready to perform update for <span class='appName'>" + Session["appName"].ToString() + "</span>, please select what to do from the below list.";
                    }

                }
                else
                    Response.Redirect("~/");
            }
        }


        /// <summary>
        /// Export only DB for App.
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        private void BatchExecuteDBExport(int appID, string appName)
        {
            // CreateLogFiles Log = new CreateLogFiles();
            string startProcessing = DateTime.Now.ToString("HH:mm - ddd d MMM yyyy");


            Session["FinishedProcessing"] = false;
            HostingEnvironment.QueueBackgroundWorkItem(async ct =>
            {
                //SendMail To All Teams
                await SendMailToUsers(appName
                                        , GetEmailList("AllTeams")
                                        , null
                                        , EmailTemplate("Start", appName, startProcessing)
                                        , "GG App-update Started for: " + appName
                                        , mapPathError, Log);

                var result3 = await RunAsyncCommandBatch(ct, appID, appName, "3_convert_db.bat " + appName, actualWorkDir, "convert SQL Db to SQLLite", mapPathError, Log);

                if (!result3.IsCancellationRequested && !HasErrors)
                {

                    if (CheckThreeLanguages(appID) && !HasErrors)
                    {
                        var result3a = await RunAsyncCommandBatch(ct, appID, appName, "3a_fix_html_entities.bat " + appName, actualWorkDir
                                                                                    , "convert HTML ENTITIES", mapPathError, Log);

                        if (!result3a.IsCancellationRequested && !HasErrors)
                            HasErrors = false;
                        else
                            HasErrors = true;
                    }


                    // move DB files generated to Android and Ios folder
                    var res3_5 = MoveGeneratedDBtoPaths(appName, appID, mapPath + "Batch\\dbfiles\\", "GreekGuide_" + appName, DateTime.Now.ToString("yyyyMMdd") + ".db");
                    if (res3_5 == null)
                        HasErrors = true;
                        


                    if (res3_5 != null && !HasErrors)
                    {

                        // Redirect result to temp file for APP with dateFormat
                        var result4 = await RunAsyncCommandBatch(ct, appID
                                                                    , appName
                                                                    , "4_db_stats.bat " + appID.ToString() + "  reports/" + appName + "_db_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".txt"
                                                                    , actualWorkDir
                                                                    , "Export Database Statistics", mapPathError, Log);


                        if (!result4.IsCancellationRequested && !HasErrors)
                        {
                            CancellationToken result9, result10;

                            if (CreateSQLiteDBs.CreateBundleDBAndFiles(appName) < 0)
                                HasErrors = true;

#if !DEBUG

                            if (!HasErrors )
                            {
                                // Add a minor version number to DB, on DB file already produced to be tested, before zipped and moved to be downloaded and tested.
                                if (IncreaseDBMinorVersion(appID, appName) == null)
                                    HasErrors = true;
                            }

                            if (!HasErrors)
                            {
                                if (UpdateVersionsFile(appID, appName) == null)
                                {
                                    HasErrors = true;
                                }
                                else
                                {
                                    /* Call the Bat that moves only DB files without images */
                                    result9 = await RunAsyncCommandBatch(ct, appID, appName, "9b_copy_img_databases.bat  " + appName + "  " + DateTime.Now.ToString("yyyyMMdd"), actualWorkDir
                                                                                                        , "Copy files from Local to Server", mapPathError, Log);
                                }
                            }

                            // Send email to QA - Nadia - Galufos Team (for versions file)
                            if (!result9.IsCancellationRequested && !HasErrors)
                            {

                                // Create a zip with all files generated under /appName/update/ with name appName.zip.
                                result10 = await RunAsyncCommandBatch(ct, appID, appName, "10_create_zip.bat " + appName, actualWorkDir, "Zip all files created under UPDATE to zip file for Mobile", mapPathError, Log);

                                if (!result10.IsCancellationRequested && !HasErrors)
                                {
                                    List<string> _listAttachments = new List<string>();
                                    _listAttachments.Add(actualWorkDir + "reports\\" + appName + "_db_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                                    
                                    if ((bool)HasErrors == false)
                                    {
                                        //SendMail To All Teams
                                        await SendMailToUsers(appName
                                                                , GetEmailList("AllTeams")
                                                                , _listAttachments
                                                                , EmailTemplate("Success", appName, startProcessing), " GG App produced for " + appName + " >Success!"
                                                                , mapPathError, Log);

                                        //Send Always email with Log info for currnet execution to ME.
                                        //_listAttachments.Add(mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt");
                                        await SendMailToUsers(appName
                                                                , GetEmailList("ErrorTeam")
                                                                , _listAttachments
                                                                , EmailTemplate("Info", appName, startProcessing, "http://app-update.greekguide.com/GGApps/Logs/Log_" + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt")
                                                                , " GG App produced for " + appName + " >Success!"
                                                                , mapPathError, Log
                                                                );

                                        ClearGeneratedDB(appName, appID, mapPath + "Batch\\dbfiles\\", "GreekGuide_" + appName, DateTime.Now.ToString("yyyyMMdd") + ".db", mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt");


                                       // var result11 = await RunAsyncCommandBatch(ct, appID, appName, "11_commit_to_git.bat " + appName, actualWorkDir, "Commit changes to SVN", mapPathError, Log);


                                        Log.InfoLog(mapPathError, appName + " Produced Successfully over Staging Content.", appName);

                                        HasErrors = false;
                                        Session["FinishedProcessing"] = true;
                                        return;
                                    }
                                }
                            }
#endif
                        }
                    }
                }

#if !DEBUG
                // Send failure email if execution was interupted / or other error occured!!
                if (HasErrors)
                {
                    List<string> _listAttachments = new List<string>();
                    _listAttachments.Add(actualWorkDir + "reports\\" + appName + "_db_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                    
                    //SendMail To All Teams
                    await SendMailToUsers(appName
                                            , GetEmailList("AllTeams")
                                            , _listAttachments
                                            , EmailTemplate("Failure", appName, startProcessing), "GG App produced for " + appName + " >Failed!"
                                            , mapPathError, Log);

                    //Send Always email with Log info for currnet execution to ME.
                    await SendMailToUsers(appName
                                            , GetEmailList("ErrorTeam")
                                            , _listAttachments
                                            , EmailTemplate("Info", appName, startProcessing, "http://app-update.greekguide.com/GGApps/Logs/Log_" + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt")
                                            , "GG App produced for " + appName + " >Failed!"
                                            , mapPathError, Log
                                            );

                    ClearGeneratedDB(appName, appID, mapPath + "Batch\\dbfiles\\", "GreekGuide_" + appName, DateTime.Now.ToString("yyyyMMdd") + ".db", mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt");

                    Log.InfoLog(mapPathError, appName + " Produced WITH ERRORS over Staging Content.", appName);

                    HasErrors = false;
                    Session["FinishedProcessing"] = true;
                    return;
                }
#endif
              }
            );


        }

        

        /// <summary>
        /// Run all Steps from 3 to 9 of Batch Build asyncronously.
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        public void BatchExecuteAllSteps(int appID, string appName)
        {
            // CreateLogFiles Log = new CreateLogFiles();
            string startProcessing = DateTime.Now.ToString("HH:mm - ddd d MMM yyyy");




            Session["FinishedProcessing"] = false;
            HostingEnvironment.QueueBackgroundWorkItem(async ct =>
            {
#if !DEBUG
                //SendMail To All Teams
                await SendMailToUsers(appName
                                        , GetEmailList("AllTeams")
                                        , null
                                        , EmailTemplate("Start", appName, startProcessing)
                                        , "GG App-update Started for: " + appName
                                        , mapPathError, Log);

#endif
                var result3 = await RunAsyncCommandBatch(ct, appID, appName, "3_convert_db.bat " + appName, actualWorkDir, "convert SQL Db to SQLLite", mapPathError, Log);
                
                    if (!result3.IsCancellationRequested && !HasErrors)
                    {

                        if (CheckThreeLanguages(appID) && !HasErrors)
                        {
                            var result3a = await RunAsyncCommandBatch(ct, appID, appName, "3a_fix_html_entities.bat " + appName, actualWorkDir
                                                                                        , "convert HTML ENTITIES", mapPathError, Log);

                            if (!result3a.IsCancellationRequested && !HasErrors)
                                HasErrors = false;
                            else
                                HasErrors = true;
                        }


                        // move DB files generated to Android and Ios folder
                        var res3_5 = MoveGeneratedDBtoPaths(appName, appID, mapPath + "Batch\\dbfiles\\", "GreekGuide_" + appName, DateTime.Now.ToString("yyyyMMdd") + ".db");
                        if (res3_5 == null)
                            HasErrors = true;
                        


                        if (res3_5 != null && !HasErrors)
                        {
                            // Redirect result to temp file for APP with dateFormat
                            var result4 = await RunAsyncCommandBatch(ct, appID
                                                                        , appName
                                                                        , "4_db_stats.bat " + appID.ToString() + "  reports/" + appName + "_db_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".txt"
                                                                        , actualWorkDir
                                                                        , "Export Database Statistics", mapPathError, Log);


                            if (!result4.IsCancellationRequested && !HasErrors)
                            {

                                var result5 = await RunAsyncCommandBatch(ct, appID, appName, "5_get_images.bat " + appName, actualWorkDir
                                                                                            , "Transform All Images running Python", mapPathError, Log);
                                if (!result5.IsCancellationRequested && !HasErrors)
                                {

                                    Log.InfoLog(mapPathError, appName + "> Started Execution of Image Statics for App ", appName);
                                    var result6 = ExecuteStep6(appID
                                                    , appName
                                                    , Server.MapPath("~/")
                                                    , Log
                                                    , mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt"
                                                    , "Batch/reports/" + appName + "_image_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".html");
                                    Log.InfoLog(mapPathError, appName + "> Finished Execution of Image Statics for App ", appName);
                                    // Do this suncrounsly


                                    if (result6 != null && !HasErrors)
                                    {

                                        object result7 = null, result8 = null;
                                        CancellationToken result9, result10;


                                        if (CreateSQLiteDBs.CreateBundleDBAndFiles(appName) < 0)
                                            HasErrors = true;
                                         

                                        // upload fb-images to production suncronusly. or remove it from here.
                                        result7 = ExecuteStep7(appID, appName, Server.MapPath("~/"), Log, mapPathError);
                                        if (result7 == null)
                                            HasErrors = true;
                                        else
                                        {
                                            // Do this Syncronously through create entity_text file and upload it to Production !?
                                            result8 = ExecuteStep8(appID, appName);
                                        }

                                        
                                        if (result8 == null)
                                            HasErrors = true;
                                        else
                                        {

                                          // Add a minor version number to DB, on DB file already produced to be tested, before zipped and moved to be downloaded and tested.
                                           // its not supported from ANDROID APK.
                                           if (IncreaseDBMinorVersion(appID, appName) == null)
                                                HasErrors = true;
                                           // ;


                                        }

                                        if (!HasErrors)
                                        {

                                            if (UpdateVersionsFile(appID, appName) == null)
                                            {
                                                HasErrors = true;
                                            }

                                            else
                                            {
                                                result9 = await RunAsyncCommandBatch(ct, appID, appName, "9_copy_img_databases.bat " + appName + " " + DateTime.Now.ToString("yyyyMMdd"), actualWorkDir
                                                                                                                    , "Copy files from Local to Server", mapPathError, Log);
                                            }
                                        }



                                        // LOG THIS
                                        // Send email to QA - Nadia - Galufos Team (for versions file)
                                        if (!result9.IsCancellationRequested && !HasErrors)
                                        {

                                            // Create a zip with all files generated under /appName/update/ with name appName.zip.
                                            result10 = await RunAsyncCommandBatch(ct, appID, appName, "10_create_zip.bat " + appName, actualWorkDir, "Zip all files created under UPDATE to zip file for Mobile", mapPathError, Log);

                                            if (!result10.IsCancellationRequested && !HasErrors)
                                            {
                                                List<string> _listAttachments = new List<string>();
                                                _listAttachments.Add(actualWorkDir + "reports\\" + appName + "_db_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                                                _listAttachments.Add(actualWorkDir + "reports\\" + appName + "_image_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".html");

                                                if ((bool)HasErrors == false)
                                                {
#if !DEBUG                                                    
                                                    //SendMail To All Teams
                                                    await SendMailToUsers(appName
                                                                            , GetEmailList("AllTeams")
                                                                            , _listAttachments
                                                                            , EmailTemplate("Success", appName, startProcessing)
                                                                            , " GG App produced for " + appName + " >Success!"
                                                                            , mapPathError, Log);

                                                    //Send Always email with Log info for currnet execution to ME.
                                                    //_listAttachments.Add(mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt");
                                                    await SendMailToUsers(appName
                                                                            , GetEmailList("ErrorTeam")
                                                                            , _listAttachments
                                                                            , EmailTemplate("Info", appName, startProcessing, "http://app-update.greekguide.com/GGApps/Logs/Log_" + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt")
                                                                            , " GG App produced for " + appName + " >Success!"
                                                                            , mapPathError, Log
                                                                            );
#endif
                                                    ClearGeneratedDB(appName, appID, mapPath + "Batch\\dbfiles\\", "GreekGuide_" + appName, DateTime.Now.ToString("yyyyMMdd") + ".db", mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt");

                                                 //   var result11 = await RunAsyncCommandBatch(ct, appID, appName, "11_commit_to_git.bat " + appName, actualWorkDir, "Commit changes to SVN", mapPathError, Log);

                                                        
                                                    Log.InfoLog(mapPathError, appName + " Produced Successfully over Staging Content.", appName);

                                                    HasErrors = false;
                                                    Session["FinishedProcessing"] = true;
                                                    return;
                                                }
                                            }
                                        }


                                    }
                                }

                            }

                        }
                    }


                    // Send failure email if execution was interupted / or other error occured!!
                    if (HasErrors)
                    {
                        List<string> _listAttachments = new List<string>();
                        _listAttachments.Add(actualWorkDir + "reports\\" + appName + "_db_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                        _listAttachments.Add(actualWorkDir + "reports\\" + appName + "_image_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".html");

#if !DEBUG

                        //SendMail To All Teams
                        await SendMailToUsers(appName
                                                , GetEmailList("AllTeams")
                                                , _listAttachments
                                                , EmailTemplate("Failure", appName, startProcessing)
                                                , "GG App produced for " + appName + " >Failed!"
                                                , mapPathError, Log);

                        //Send Always email with Log info for currnet execution to ME.
                        await SendMailToUsers(appName
                                                , GetEmailList("ErrorTeam")
                                                , _listAttachments
                                                , EmailTemplate("Info", appName, startProcessing, "http://app-update.greekguide.com/GGApps/Logs/Log_" + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt")
                                                , "GG App produced for " + appName + " >Failed!"
                                                , mapPathError, Log
                                                );
#endif
                        ClearGeneratedDB(appName, appID, mapPath + "Batch\\dbfiles\\", "GreekGuide_" + appName, DateTime.Now.ToString("yyyyMMdd") + ".db", mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt");

                        Log.InfoLog(mapPathError, appName + " Produced WITH ERRORS over Staging Content.", appName);

                        HasErrors = false;
                        Session["FinishedProcessing"] = true;
                        return;
                    }


            }

            );



        }



        private object MoveGeneratedDBtoPaths(string appName, int appID, string path, string filenameHl1, string filenameHl2)
        {
            try
            {
                Log.InfoLog(mapPathError, "Started> Moving local batch db files", appName);
                InitCopySQLitesLocalPath(path);
                CopySQLitesLocalPath(path, filenameHl1 + "_EN_" + filenameHl2);
                CopySQLitesLocalPath(path, filenameHl1 + "_EL_" + filenameHl2);

                if (CheckThreeLanguages(appID))
                {
                    CopySQLitesLocalPath(path, filenameHl1 + "_RU_" + filenameHl2);
                    ClearLocalPath(path, filenameHl1 + "_RU_" + filenameHl2);
                }
                Log.InfoLog(mapPathError, "Finished> Moving local batch db files", appName);
                return 1;
            }
            catch (Exception ex)
            {
                Log.ErrorLog(mapPathError, "Exception in MoveGeneratedDBtoPaths(), " + ex.Message, appName);
                return null;
            }

        }


        private object ClearGeneratedDB(string appName, int appID, string path, string filenameHl1, string filenameHl2, string logPath)
        {
            try
            {
                Log.InfoLog(mapPathError, "Started> Clear local batch db files", appName);
                if (CheckThreeLanguages(appID))
                {
                    ClearLocalPath(path, filenameHl1 + "_RU_" + filenameHl2);
                }

                ClearLocalPath(path, filenameHl1 + "_EL_" + filenameHl2);
                ClearLocalPath(path, filenameHl1 + "_EN_" + filenameHl2);

                Log.InfoLog(mapPathError, "Finished> Clear local batch db files", appName);
                return 1;
            }
            catch (Exception ex)
            {
                Log.ErrorLog(logPath, "Exception in ClearGeneratedDB(), " + ex.Message, appName);
                return null;
            }
        }

        private void InitCopySQLitesLocalPath(string localPath)
        {
            if (Directory.Exists(localPath + "android\\"))
                Directory.Delete(localPath + "android\\", true);
            Directory.CreateDirectory(localPath + "android\\");

            if (Directory.Exists(localPath + "ios\\"))
                Directory.Delete(localPath + "ios\\", true);
            Directory.CreateDirectory(localPath + "ios\\");

        }


        private void CopySQLitesLocalPath(string localPath, string filename)
        {
            if (!File.Exists(localPath + filename))
                throw new FileNotFoundException();

            File.Copy(localPath + filename, localPath + "android\\" + filename);

            if (!File.Exists(localPath + filename))
                throw new FileNotFoundException();

            File.Copy(localPath + filename, localPath + "ios\\" + filename);
        }

        private void ClearLocalPath(string localPath, string filename)
        {
            // Clear DB files 
            if (File.Exists(localPath + filename))
                File.Delete(localPath + filename);
        }



        /// <summary>
        /// Read excpected DB version from Admin DB, assign this plus a minor ver to produced SQLite DB.
        /// If DB version already in minor version, add minor version
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        private object IncreaseDBMinorVersion(int appID, string appName)
        {
            Finalize fin = new Finalize(appName, appID);
            Log.InfoLog(mapPathError, appName + "> Started increase DB minor Version", appName);
            // makes a minor version update
            if (fin.UpdateDBVersion("ios") == null)
                return null;

            if (fin.UpdateDBVersion("android") == null)
                return null;

            fin = null;
            Log.InfoLog(mapPathError, appName + "> Finished increase DB minor Version", appName);
            return 0;

        }

        /// <summary>
        /// Update the JSON property db_version on Versions.txt
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        private object UpdateVersionsFile(int appID, string appName)
        {
            try
            {
                /* Pending while APK is not ready for testing this.*/
                Log.InfoLog(mapPathError, "Started> Update Versions.txt", appName);

                Finalize fin = new Finalize(appName, appID);
                string dbver;

                // Set Versions file for IOS only for one Lang
                dbver = fin.InitializeSQLiteVersionFromDB(appID, appName, 1, "ios");
                if (!fin.SetVerionsFileProperty("db_version", dbver, appName, appID, "ios", 1, "versions.txt"))
                    return null;

                // Set Versions file for ANDROID
                dbver = fin.InitializeSQLiteVersionFromDB(appID, appName, 1, "android");
                if (!fin.SetVerionsFileProperty("db_version", dbver, appName, appID, "android", 1, "versions.txt"))
                    return null;

                // also Set configuraion version taken from Configuration.txt doc.!!

                Log.InfoLog(mapPathError, "Finished> Update Versions.txt to " + dbver, appName);


                return 0;
            }
            catch (Exception ex)
            {
                Log.ErrorLog(mapPathError, "Some exception occured in UpdateVersionsFile(), ", ex.Message, appName);
                return null;
            }
        }


        /// <summary>
        /// Upload Entity txt to FTP
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        /// <param name="p1"></param>
        /// <param name="Log"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private object ExecuteStep8(int appID, string appName)
        {
            try
            {
                double totalBytesUploaded = 0;

                if (rootWebConfig.AppSettings.Settings["ProducedAppPath"] != null)
                {
                    string ProducedAppPath = rootWebConfig.AppSettings.Settings["ProducedAppPath"].Value;
                    Log.InfoLog(mapPathError, ":> Start Create-upload Entity_text to  FTP", appName);

                    if (GenerateEntityText(appName, appID, ProducedAppPath + "\\" + appName + "\\config\\", "entity_text.txt") == null)
                    {
                        Log.ErrorLog(mapPathError, ":> Error: in genaration of Entity_Text", appName);
                        return null;
                    }


                    // Check if CONFIG exists on production, if not exists create it.
                    if (!CheckDirectoryAndCreateRemote(appName, appName.ToLower() + "/config/"))
                        return null;

                    if (File.Exists(ProducedAppPath + "\\" + appName + "\\config\\entity_text.txt"))
                        totalBytesUploaded = UploadFileRemote(appName, ProducedAppPath + "\\" + appName + "\\config\\entity_text.txt", appName.ToLower() + "/config/entity_text.txt");
                    else
                    {
                        Log.ErrorLog(mapPathError, ":> Error: Entity_text.txt not found locally!", appName);
                        return null;
                    }


                    if (totalBytesUploaded <= 0)
                    {
                        Log.ErrorLog(mapPathError, ":>Error: Entity_text.txt too small or Zero size.", appName);
                        return null;
                    }
                    else
                    {
                        Log.InfoLog(mapPathError, ":> Success finish Upload Entity_text.txt to FTP", appName);
                        return totalBytesUploaded;
                    }
                }
                return null;
            }
            catch (Exception e)
            { 
                Log.ErrorLog(mapPathError, "Some exception occurred in ExecuteStep8(), "+ e.Message, appName);
                return null;
            }
        }



        private object GenerateEntityText(string appName, int appID, string path, string filenametoSave)
        {
            // check if path exists, else create it
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // run query to produce entity_text under test ..
            if (!executeSQLScript(appID, appName, 2, mapPath + "SQLScripts\\" + "db_create_entity.sql", path + filenametoSave, null))
                return null;

            return 1;
        }


        private object ExecuteStep7(int appID, string appName, string mapPath, CreateLogFiles Log, string logPath)
        {
            try
            {
                double totalBytesUploaded = 0;
                // Upload Files Syncronously

                Log.InfoLog(mapPathError, ":> Start Upload Images FB to FTP", appName);

                // Check if fb-assets exists on production, if not exists create it.
                if (!CheckDirectoryAndCreateRemote(appName, appName.ToLower() + "/fb-assets/"))
                    return null;


                totalBytesUploaded = UploadFilesRemote(appName, "C:\\temp\\images\\" + appName + "-fb\\", appName.ToLower() + "/fb-assets/");
                if (totalBytesUploaded <= 0)
                {
                    Log.InfoLog(mapPathError, ":> Error! finish Upload Images FB to FTP", appName);
                    return null;
                }
                else
                {
                    Log.InfoLog(mapPathError, ":> Success finish Upload Images FB to FTP", appName);
                    return totalBytesUploaded;
                }
            }
            catch (Exception ex)
            {

                Log.ErrorLog(mapPathError, "Some Excepetion occured in ExecuteStep7(): " + ex.Message, appName);
                return null;
            }
        }

        private void CheckFb_AssetsProduction(string p)
        {
            throw new NotImplementedException();
        }




        private object ExecuteStep6(int appID, string appName, string actualWorkDir, CreateLogFiles Log, string logPath, string fileName)
        {
            StringBuilder sb = new StringBuilder();

            // Start Log

            string path = actualWorkDir + "SQLScripts\\";
            sb.Length = 0;
            sb.Clear();

            sb.AppendLine("<h2>*** images in db <h2>");
            sb.AppendLine("<br/>* ");

            try
            {
                // call SQL script to fetch data 
                sb.AppendLine(executeSQLScript(appID, appName, 1, path + "db_number_of_images.sql"));
            }
            catch (Exception ex)
            {
                Log.ErrorLog(mapPathError, "ExecuteStep6 a." + ex.Message, appName);
                return null;
            }


            sb.AppendLine("<br/>* ");
            string path_from = "c:\\Temp\\Images\\" + appName;

            try
            {

                sb.AppendLine("<br/>*** fb images in local dir (" + path_from + "-fb)");
                sb.AppendLine("<br/>* ");
                if (Directory.Exists(path_from + "-fb"))
                {
                    int cnt = Directory.GetFiles(path_from + "-fb").Length;
                    sb.AppendLine("<br/>* " + cnt);
                }
                sb.AppendLine("<br/>* ");
                sb.AppendLine("<br/>*** fb images in local dir (" + path_from + ")");
                sb.AppendLine("<br/>* ");
                if (Directory.Exists(path_from))
                {
                    int cnt = Directory.GetFiles(path_from).Length;
                    sb.AppendLine("<br/>* " + cnt);
                }

                //sb.AppendLine("<br/>* ");
                //sb.AppendLine("<br/>*** fb images in remote dir (" + path_to + ")");
                //sb.AppendLine("<br/>* ");
                //if (Directory.Exists(path_to))
                //{
                //    int cnt = Directory.GetFiles(path_to).Length;
                //    sb.AppendLine("<br/>* " + cnt);
                //}
                sb.AppendLine("<br/>* ");
                File.WriteAllText(actualWorkDir + fileName, sb.ToString(), Encoding.UTF8);

            }
            catch (IOException ex)
            {
                Log.ErrorLog(mapPathError, "Some IO exception occured on ExecuteStep6 b. " + ex.Message, appName);
                return null;
            }


            return sb.ToString();


        }



        /// <summary>
        /// Dynamically prints a message on Screen
        /// </summary>
        /// <param name="divID"></param>
        /// <param name="msg"></param>
        /// <param name="baseCtrl"></param>
        private void AddMessageToScreen(string divID, string msg, Control baseCtrl)
        {
            // Add to Div message for execution // Don't close window !!
            StringWriter stringWriter = new StringWriter();

            // Put HtmlTextWriter in using block because it needs to call Dispose.
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                // The important part:
                writer.RenderBeginTag(HtmlTextWriterTag.Div); // Begin #1
                writer.Write(msg);
                writer.RenderEndTag();
                Control ExecutionMessages = baseCtrl.FindControl(divID);
                if (ExecutionMessages != null)
                {
                    LiteralControl ltCtrl = new LiteralControl();
                    ltCtrl.Text = writer.InnerWriter.ToString();
                    ExecutionMessages.Controls.Add(ltCtrl);

                }

            }

        }

        /// <summary>
        /// Select What to Do, process all, process only DB, process only Files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCheckBox_Click(object sender, EventArgs e)
        {
            RadioButtonList chkList = (RadioButtonList)((Control)sender).FindControl("BuildAppListID");
            List<string> chkListActions = new List<string>();

            if (Session["appName"] == null || Session["appID"] == null)
                Response.Redirect("~/");

            if (chkList != null)
            {
                foreach (ListItem item in chkList.Items)
                {
                    if (item.Selected)
                    {
                        // If the item is selected, add the value to the list.
                        chkListActions.Add(item.Value);
                    }
                }

                // Perform nessacary actions
                if (chkListActions != null)
                {
                    foreach (String cmdStr in chkListActions)
                    {
                        if (cmdStr == "FullBatch")
                        {
                            Log.InfoLog(mapPathError, "================================= START EXECUTION: Batch Full, In-App UPDATE FOR:  " + Session["appName"].ToString().ToUpper() + "================================= ", Session["appName"].ToString());
                            BatchExecuteAllSteps((int)Session["appID"], Session["appName"].ToString());
                            ((Control)sender).Parent.FindControl("mainSubPanel").Visible = false;
                            AddMessageToScreen("ExecutionMessages"
                                               , "<h2>Batch process execution has started for <span class='appName'>" + Session["appName"].ToString() + "</span>. </h2>"
                                                  + "<h3>You will be notified via e-mail upon completition.</h3>"
                                                  + "<strong>Thank You!</strong>", ((Control)sender).Parent);
                        }
                        else if (cmdStr == "DBOnly")
                        {
                            Log.InfoLog(mapPathError, "================================= START EXECUTION: DB Only, In-App UPDATE FOR:  " + Session["appName"].ToString().ToUpper() + "================================= ", Session["appName"].ToString());
                            BatchExecuteDBExport((int)Session["appID"], Session["appName"].ToString());
                            ((Control)sender).Parent.FindControl("mainSubPanel").Visible = false;
                            AddMessageToScreen("ExecutionMessages"
                                               , "<h2>Batch process execution has started for <span class='appName'>" + Session["appName"].ToString() + "</span>. </h2>"
                                                  + "<h3>You will be notified via e-mail upon completition.</h3>"
                                                  + "<strong>Thank You!</strong>", ((Control)sender).Parent);
                        }
                        //else if (cmdStr == "ImagesOnly")
                        //{
                        //    BatchExecuteAllSteps((int)Session["appID"], Session["appName"].ToString());
                        //    AddMessageToScreen("ExecutionMessages"
                        //                       , "<h2>Batch process execution has begun for " + appName.ToString() + ". </h2>"
                        //                          + "<h3>Teams will be notified when execution is finished.</h3>"
                        //                          + "<strong>Thank You!</strong>");
                        //}
                        else
                        {

                            AddMessageToScreen("ExecutionMessages"
                                       , "<h2>Please select what to process from above radio button.</h2>"
                                          + "<strong>Thank You!</strong>", ((Control)sender).Parent);

                        }

                    }

                    if (chkListActions.Count == 0)
                    {
                        AddMessageToScreen("ExecutionMessages"
                                          , "<h2>Please select what to process from above radio button.</h2>"
                                             + "<strong>Thank You!</strong>", ((Control)sender).Parent);
                    }
                }
            }


        }


        private async Task<CancellationToken> RunAsyncCommandBatch(CancellationToken ct, int appID, string appName, string command, string actualWorkDir, string ExplainCmd, string mapPath, CreateLogFiles log, bool redirectOut = true)
        {
            // perform a long running operation, e.g. network service call, computation, file IO
            ProcessStartInfo procStartInfo;

            try
            {
                if (redirectOut)
                {
                    procStartInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
                }
                else
                // This is when we have a pipe execution!
                {
                    procStartInfo = new ProcessStartInfo("cmd.exe");
                    procStartInfo.Arguments = @"/C """ + command.ToString() + "\"";
                }


                procStartInfo.WorkingDirectory = actualWorkDir;
                procStartInfo.RedirectStandardOutput = redirectOut;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                procStartInfo.LoadUserProfile = false;
                procStartInfo.RedirectStandardError = true;



                // Now we create a process, assign its ProcessStartInfo and start it
                Process proc = new Process();

                proc.StartInfo = procStartInfo;
                proc.ErrorDataReceived += new DataReceivedEventHandler(NetErrorDataHandler);


                log.InfoLog(mapPath, "Started:> " + ExplainCmd + " for APP: " + appName, appName);
                proc.Start();
                proc.BeginErrorReadLine();


                // instead of p.WaitForExit(), do
                StringBuilder q = new StringBuilder();
                while (!proc.HasExited)
                {
                    if (redirectOut)
                    {
                        q.AppendLine("</br>" + proc.StandardOutput.ReadLine());

                        // Log the process
                        log.InfoLog(mapPath, " PID:" + proc.Id + "> " + proc.StandardOutput.ReadLine(), appName);
                    }

                }

                
                proc.WaitForExit();

                log.InfoLog(mapPath, "Finished:> " + ExplainCmd + " for APP: " + appName, appName);

            }
            catch (Exception ex)
            {
                log.ErrorLog(mapPath, "Error while creating Process : " + ex.Message, appName);
                ct.ThrowIfCancellationRequested();
                return ct;
            }

            return ct;//q.ToString();
        }


        private static void NetErrorDataHandler(object sendingProcess, DataReceivedEventArgs errLine)
        {

            // Write the error text to the file if there is something 
            string realErrorData = errLine.Data;
            // to write and an error file has been specified. 
            if (errLine != null && sendingProcess != null)
                if (!String.IsNullOrEmpty(errLine.Data))
                {
                    if (!ContainsUnicodeCharacter(errLine.Data.ToString()))
                        Log.ErrorLog(mapPathError, "Error while executing Process: " + ((Process)sendingProcess).Id + " Details: " + errLine.Data + " \n\t\t DETAILS: " + realErrorData, "generic");
                }
        }


        /// <summary>
        /// Verify that all directories are created for an app in order to run proprerly
        /// </summary>
        /// <returns>True if all ok even if dirs are created etc., config files updated etc, false if something wrong happend when updating</returns>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        public bool Init_BuildApp(int appID, string appName)
        {

            try
            {
                // Create directories under C:\temp\images if not exists, else create them
                if (!Directory.Exists("C:\\temp\\images\\" + appName))
                {
                    Directory.CreateDirectory("C:\\temp\\images\\" + appName);
                }

                if (!Directory.Exists("C:\\temp\\images\\" + appName + "-fb"))
                {
                    Directory.CreateDirectory("C:\\temp\\images\\" + appName + "-fb");
                }


                // add to next version !
                //if (!CheckExternalAppConfigSettings(appID, appName))
                //    return false;

                BackOffice.CheckAppsBundleList();

            }
            catch (IOException e)
            {
                Log.ErrorLogAdmin(mapPathError, "Initialize of App-Update failed! " + e.Message, appName);
                return false;
            }

            return true;

        }



        #region NOT USED  - do for v.2
        private bool CheckExternalAppConfigSettings(int appID, string appName)
        {
            try
            {
                bool[] checkLangs = new bool[10];

                //ExeConfigurationFileMap configMap = new System.Configuration.ExeConfigurationFileMap();
                //configMap.ExeConfigFilename = Server.MapPath("ExternalApp/") + @"SQLiteConverter.exe.config";
                //Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

                XmlDocument xml = new XmlDocument();
                xml.Load(Server.MapPath("ExternalApp/") + @"SQLiteConverter.exe.config");

                foreach (XmlNode node in xml.SelectNodes("configuration/DBConfigSection/DBSettings"))
                {
                    //<DBSetting name="Zakynthos" lang="el" db="ContentDB_165_Lan_1_Cat_369"/>
                    //<DBSetting name="Zakynthos" lang="en" db="ContentDB_165_Lan_2_Cat_369"/>      

                    if (node.Attributes["name"] != null && node.Attributes["lang"] != null)
                    {
                        if (node.Attributes["name"].ToString() == appName)
                            if (node.Attributes["lang"].ToString() == "el")
                                if (node.Attributes["db"].ToString() == "ContentDB_165_Lan_1_Cat_" + appID.ToString())
                                    checkLangs[(int)Lang.el] = true;

                        if (node.Attributes["name"].ToString() == appName)
                            if (node.Attributes["lang"].ToString() == "en")
                                if (node.Attributes["db"].ToString() == "ContentDB_165_Lan_2_Cat_" + appID.ToString())
                                    checkLangs[(int)Lang.en] = true;

                        if (CheckThreeLanguages(appID))
                            if (node.Attributes["name"].ToString() == appName)
                                if (node.Attributes["lang"].ToString() == "ru")
                                    if (node.Attributes["db"].ToString() == "ContentDB_165_Lan_4_Cat_" + appID.ToString())
                                        checkLangs[(int)Lang.ru] = true;
                    }

                }


                // Append new nodes where necessary
                for (int i = 0; i < checkLangs.Length; i++)
                {
                    if (checkLangs[i] != null)
                    {
                        xml.SelectSingleNode("configuration/DBConfigSection/DBSettings").AppendChild(addMissingNode(appID, appName, i, xml));
                    }

                }


                xml.Save(Server.MapPath("ExternalApp/") + @"SQLiteConverter.exe.config");


                return true;
            }
            catch (Exception e)
            {
                // log exception
                return false;
            }
        }



        private XmlElement addMissingNode(int appID, string appName, int lang, XmlDocument xml)
        {

            // name="Zakynthos" lang="el" db="ContentDB_165_Lan_1_Cat_369"/>

            //Create a new node.
            XmlElement elem = xml.CreateElement("DBSetting");
            XmlAttribute attr = xml.CreateAttribute("name");
            attr.Value = appName;
            elem.Attributes.Append(attr);

            attr = xml.CreateAttribute("lang");
            attr.Value = LangStr[lang];
            elem.Attributes.Append(attr);

            attr = xml.CreateAttribute("db");
            attr.Value = "ContentDB_165_Lan_" + lang.ToString() + "_Cat_" + appID.ToString();
            elem.Attributes.Append(attr);

            return elem;
        }


        //private async Task<CancellationToken> addLogAsync(CancellationToken ct, int currentLogCount, string msg)
        //{

        //    try
        //    {
        //        for (int i = 0; i < 5; i++)
        //        {
        //            if (ct.IsCancellationRequested)
        //            {
        //                Trace.Write(string.Format("{0} - signaled cancellation : msg {1}", DateTime.Now.ToLongTimeString(), msg));
        //                break;
        //            }
        //            Trace.Write(string.Format("{0} - msg:{1} - logcount:{2}",  DateTime.Now.Second.ToString(), msg, currentLogCount));

        //            // "Simulate" this operation took a long time, but was able to run without
        //            // blocking the calling thread (i.e., it's doing I/O operations which are async)
        //            // We use Task.Delay rather than Thread.Sleep, because Task.Delay returns
        //            // the thread immediately back to the thread-pool, whereas Thread.Sleep blocks it.
        //            // Task.Delay is essentially the asynchronous version of Thread.Sleep:
        //            await Task.Delay(2000, ct);
        //        }
        //    }
        //    catch (TaskCanceledException tce)
        //    {
        //        Trace.Write("Caught TaskCanceledException - signaled cancellation " + tce.Message);
        //    }
        //    return ct;
        //}



        #endregion



    }
}