using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Hosting;
using System.IO;
using System.Text;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;


namespace GGApps
{
    public class Common : System.Web.UI.Page
    {
        public static String[] otherLangApps;
        public static System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
        public static string actualWorkDir = HostingEnvironment.MapPath("~/Batch/");
        // "C:\\Users\\Argiris\\Desktop\\GG_Batch\\Batch\\";    
        public static StringBuilder sb = new StringBuilder();
        public static string mapPath = HostingEnvironment.MapPath("~/");

        public enum Lang : int { el = 1, en ,miss , ru };
        public static string[] LangStr = { "", "el", "en", "miss", "ru" };
        public static StringBuilder sbExec = new StringBuilder();
        public static string mapPathError = HostingEnvironment.MapPath("~/Logs") + "\\Log_";
        
        public static string producedAppPath = rootWebConfig.AppSettings.Settings["ProducedAppPath"].Value.ToString();
        public static string ToPublishZipDir = rootWebConfig.AppSettings.Settings["ToPublishZipDir"].Value.ToString();
        public static string unzipFileSSHcmd = rootWebConfig.AppSettings.Settings["unzipFileSSHcmd"].Value.ToString();
        public static string replaceDeviceOldSSHcmd = rootWebConfig.AppSettings.Settings["unzipFileSSHcmd"].Value.ToString();

        public static bool HasErrors = false;                       // MAKE THIS A SESSION VARIABLE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public static bool LogErrorAdmin = false;                       
        public static CreateLogFiles Log = new CreateLogFiles();

        
        public class AppVersionDetail
        {
            public string Environment;
            public string Device;
            public string DB_Version;
            public string App_Version;
            public string Config_Version;
            public string appName;
            public int appID;

            public AppVersionDetail(string Environment, string Device, string DB_Version, string App_Version, string Config_Version, string appName, int appID )
            {
                this.Device = Device;
                this.Environment = Environment;
                this.DB_Version = DB_Version;
                this.App_Version = App_Version;
                this.Config_Version = Config_Version;
                this.appName = appName;
                this.appID = appID;
            }
        }

        public int timesExec
        {
            get
            {
                if (HttpContext.Current.Session["timesExec"] == null)
                {
                    return 0;
                }

                return ((int)HttpContext.Current.Session["timesExec"]);
            }
            set
            {
                HttpContext.Current.Session["timesExec"] = value;
            }

        }

        

        public static string ConvertDataTableToHTML(DataTable dt)
        {

            string html = "<table>";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<th>" + dt.Columns[i].ColumnName + "</th>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (dt.Columns[j].ColumnName.ToLower() == "id")
                        html += "<td><a href='#' class='popAdm'>" + dt.Rows[i][j].ToString() + "</a></td>";
                    else
                    {
                        // truncate text if too long!
                        // {?}
                        string strText = dt.Rows[i][j].ToString();

                        if (strText.Length > 300)
                        {
                            // find {?} if exists
                            if (strText.Contains("{?}"))
                            {
                                int pos = strText.IndexOf("{?}");
                                int nPos = (pos - 150 < 0) ? 0 : pos - 150;

                                strText = String.Concat("...", strText.Substring(nPos));
                                pos = strText.IndexOf("{?}");

                                // Not really an exception, try to remove all last chars , leave only 100.
                                try { strText = strText.Remove(pos + 100); }
                                catch (Exception)
                                { strText = strText.Remove(pos + 2); }

                                strText = String.Concat(strText, "...");
                            }

                            html += "<td>" + strText + "</td>";

                        }
                        else
                            html += "<td>" + strText + "</td>";
                    }
                }
                html += "</tr>";
            }
            html += "</table>";


            return html;

        }


        /// <summary>
        /// Check if the app is using 3 languages. This can be found on Web.Config
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool CheckThreeLanguages(int id)
        {

            String threelang = rootWebConfig.AppSettings.Settings["ThreeLanguages"].Value;
            if (threelang != null)
            {
                otherLangApps = threelang.Split('#');
                if (otherLangApps != null)
                {
                    foreach (string x in otherLangApps)
                    {
                        if (x.Trim() == id.ToString())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        }

        public static string BuildDynamicConnectionStringForDB(int id, string name, int lang, string DBNameOver = null)
        {
            if (rootWebConfig.AppSettings.Settings["DynamicConnectionString"] != null)
            {
                string dbName = "ContentDB_165_Lan_" + lang.ToString() + "_Cat_" + id.ToString();
                string ConStr = rootWebConfig.AppSettings.Settings["DynamicConnectionString"].Value;

                if (!String.IsNullOrEmpty(DBNameOver))
                    dbName = DBNameOver;

                ConStr = ConStr.Replace("DBNAME", dbName);
                return ConStr;
            }

            return null;

        }

        /// <summary>
        /// Generate Entity.txt mainly, could be used to create other file to disk.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="langID"></param>
        /// <param name="sqlFile"></param>
        /// <param name="dbName"></param>
        /// <param name="fileNameToSave"></param>
        /// <returns></returns>
        public bool executeSQLScript(int id, string name, int langID, string sqlFile, string fileNameToSave, string dbName = null )
        {
            try
            {
                FileInfo fileInfo = new FileInfo(sqlFile);

                string script = File.ReadAllText(fileInfo.FullName, Encoding.Default);
                StringBuilder sbSql = new StringBuilder();

                string connectionString = BuildDynamicConnectionStringForDB(id, name, langID, dbName);

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    //con.InfoMessage += new SqlInfoMessageEventHandler(myConnection_InfoMessage);
                    con.Open();
                    using (SqlCommand command = new SqlCommand(script, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 2000;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            using (StreamWriter writer = new StreamWriter(fileNameToSave))
                            {
                                while (reader.Read())
                                {
                                    // Using Name and Phone as example columns.
                                    writer.WriteLine(reader[0].ToString());
                                }
                                return true;
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Log.ErrorLog(mapPathError, "executeSQLScript for " + name + " lang:" + langID + " Exception:" + e.Message, name);
                return false;
            }

        }



        /// <summary>
        /// Generate report for an App
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="langID"></param>
        /// <param name="sqlFile"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public String executeSQLScript(int id, string name, int langID, string sqlFile, string dbName = null)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(sqlFile);

                string script = File.ReadAllText(fileInfo.FullName, Encoding.Default);
                StringBuilder sbSql = new StringBuilder();

                string connectionString = BuildDynamicConnectionStringForDB(id, name, langID, dbName);

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    //con.InfoMessage += new SqlInfoMessageEventHandler(myConnection_InfoMessage);
                    con.Open();
                    using (SqlCommand command = new SqlCommand(script, con))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 2000;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                var dataTable = new DataTable();
                                dataTable.Load(reader);

                                // sure there are better ways..
                                return ConvertDataTableToHTML(dataTable);
                            }

                        }
                    }
                }

            }
            catch (Exception e)
            {
                Log.ErrorLogAdmin(mapPathError, "executeSQLScript for " + name + " lang:" + langID + " Exception:" + e.Message, name);
                return "<span class='error'>Error!</span>";
            }

            return "<span class='emptyTable'>Passed</span>";
        }


        public static DataTable GetAllAppTable()
        {

            try
            {
                if (rootWebConfig.AppSettings.Settings["ContentAbilityGG"] != null)
                {
                    SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["ContentAbilityGG"].Value.ToString());
                    SqlCommand cmd = new SqlCommand("select catCategoryId as id , catName as appName from category  where catParentID = 2 order by catName ", con);
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adp.Fill(dt);

                    return dt;

                }
            }
            catch (Exception e)
            {
                HttpContext.Current.Session["hErrors"] = true;
                Log.ErrorLogAdmin(mapPathError, e.Message, "generic", "");
            }
            return null;

        }




        /// <summary>
        /// Create a default Versions file, if file not EXISTS!.
        /// </summary>
        /// <param name="filename"></param>
        public static string CreateVersionsFile(string filename)
        {
            // Create configuration JSON default file 
            if (Directory.Exists(filename.Substring(0, filename.LastIndexOf("\\"))))
            {
                if (!File.Exists(filename))
                {
                    File.WriteAllText(filename, @"{  ""app_version"": ""2.1"",  ""config_version"": ""1"",  ""db_version"": ""1""}", System.Text.Encoding.UTF8);
                    return "created";
                }
                return "exists";
            }
            else
                return null;
        }


        /// <summary>
        /// Check if Versions.txt exists on Staging
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appID"></param>
        /// <param name="mobileDevice"></param>
        /// <returns></returns>
        public bool CheckVersionsFile(string appName, string mobileDevice)
        {
            var fileName = producedAppPath + appName + "\\update\\" + mobileDevice + "\\versions.txt";
            if (!File.Exists(fileName))
            {
                return false;
            }

            return true;

        }


        /// <summary>
        /// Checks if versions.txt exists in Production.
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="mobileDevice"></param>
        /// <returns></returns>
        public bool CheckVersionsFileProduction( string appName, string mobileDevice)
        {
            string remotefilename = appName.ToLower() + "//update//" + mobileDevice + "//versions.txt";
            
            FTP ftpClient = CreateFTPClientProduction(appName);

            if (ftpClient != null)
            {
                if (ftpClient.getFileCreatedDateTime(remotefilename) != null)                       
                {
                    return true;                // file exists and has a creation datetime!
                }
            }

            return false;

        }



        /// <summary>
        /// Read Properties from JSON Version file "versions.txt", and store in OUT params, for specific mobileDevice and App.
        /// </summary>
        /// <param name="mobileDevice"></param>
        /// <param name="dbVersion">db version found in file</param>
        /// <param name="appVersion">application version found in file</param>
        /// <param name="configVersion">config version found in file</param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public JObject GetVersionsFile(string mobileDevice, out string dbVersion, out string appVersion, out string configVersion, string appName)
        {
            // open Configuration file if exists
            var fileName = producedAppPath + appName + "\\update\\" + mobileDevice + "\\versions.txt";
            dbVersion = null;           // something is wrong !
            configVersion = null;       // when return should ALWAYS have values..
            appVersion = null;          // when return should ALWAYS have values..

            // Check if file exists on expcted location.
            if (!File.Exists(fileName))
            {
                // DONT CREATE A VERSION FILE HERE ?


                // if configuration file does not exists, then create a default, and keep a LOG!
                //if (CreateVersionsFile(fileName) != null)
                //{
                //    Log.ErrorLog(mapPathError, " Versions file did not found in the expected location but created: " + fileName, "generic");
                //}
                //else
                //    return null;
            }
            try
            {

                // read JSON directly from a file
                using (StreamReader file = File.OpenText(fileName))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o2 = (JObject)JToken.ReadFrom(reader);
                        configVersion = o2.Value<string>("config_version");

                        if (configVersion != null)
                        {
                            dbVersion = o2.Value<string>("db_version");
                            if (dbVersion != null)
                            {
                                // get also the AppVersion !
                                appVersion = o2.Value<string>("app_version");
                                if (appVersion != null)
                                {
                                    return o2;       // All good, app_version and db_version found.
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorLogAdmin(mapPathError, appName + ": Some Exception occured in GetVersionsFile(), " + e.Message, "generic");
                return null;
            }

            return null;

        }




        public static double UploadFilesRemote(string appName, string localDir, string remotePath,  bool overwrite = true)
        {
            try
            {
                Log.InfoLog(mapPathError, "Started uploading files to FTP", appName);
                long totalBytesUploaded = 0;
                FTP ftpClient = CreateFTPClientProduction(appName);


                if (ftpClient != null)
                {
                    foreach (string localFile in Directory.GetFiles(localDir))
                    {
                        FileInfo fi = new FileInfo(localFile);
                        totalBytesUploaded += ftpClient.upload(localFile, remotePath + fi.Name, overwrite);
                    }
                }

                if (totalBytesUploaded > 10)
                    Log.InfoLog(mapPathError, "Finished with uploading files from " + localDir + " to " + remotePath + " FTP, total Bytes uploaded: " + ftpClient.SizeSuffix(totalBytesUploaded), appName);
                else
                {
                    Log.ErrorLogAdmin(mapPathError, "Some error ocured while uploading files from " + localDir + " to " + remotePath + " FTP, total Bytes uploaded: " + ftpClient.SizeSuffix(totalBytesUploaded), appName);
                    return 0;
                }

                ftpClient = null;
                return totalBytesUploaded;
            }
            catch (Exception ex)
            {
                Log.ErrorLogAdmin(mapPathError, "Some error ocured while uploading files from " + localDir + " to " + remotePath + " FTP, Exception:  " + ex.Message, appName);
                return 0;
            }
        }


        internal int LangToInt(string Lng)
        {
            if (Lng != null)
            {
                switch (Lng.Trim().ToLower())
                {
                    case "en": return (int)Lang.en; 
                    case "el": return (int)Lang.el; 
                    case "ru": return (int)Lang.ru; 
                    default: return -1;
                }
            }
            return -1;
       }



        public static bool CheckDirectoryAndCreateRemote(string appName, string remoteDirectory)
        {
            try
            {
                FTP ftpClient = CreateFTPClientProduction(appName);

                if (ftpClient != null)
                {
                    if (ftpClient.directoryListSimple(remoteDirectory) == null)                   // directory does not exists so try to create it    
                    {
                        // create new dir.
                        if (ftpClient.createDirectory(remoteDirectory))
                            return true;                                                          // new dir created successfully.
                        else
                            return false;
                    }
                    else
                        return true;                                                             // dir already exists.

                }
                ftpClient = null;
                return false;                                                                    // ftp connection problem!
            }
            catch (Exception ex)
            {
                Log.ErrorLog(mapPathError, "Some error ocured CheckFileAndCreateRemote(), Exception:  " + ex.Message, appName);
                return false;
            }

        }



        public static double RenameFileRemote(string appName, string localFilename, string localFilenameNew)
        {
            try
            {
                FTP ftpClient = CreateFTPClientProduction(appName);

                if (ftpClient != null)
                {
                    if (ftpClient.getFileCreatedDateTime(localFilename) != null)                       // Cunrrent file does not exits contact admin !
                    {
                        if (ftpClient.rename(localFilename, localFilenameNew) == null)
                            return -1;
                    }
                    else
                        return 1;
                }
                ftpClient = null;
                
                return 1;
            }
            catch (Exception ex)
            {
                Log.ErrorLogAdmin(mapPathError, "Some error ocured while uploading files to FTP, Exception:  " + ex.Message, appName);
                return -1;
            }
        
        }

        private static FTP CreateFTPClientProduction(string appName)
        {
            FTP ftpClient = null;
            if (rootWebConfig.AppSettings.Settings["FTP_Upload_ConStr"] != null)
            {
                var ftpConStr = rootWebConfig.AppSettings.Settings["FTP_Upload_ConStr"].Value.Split(new string[] { "|@@|" }, StringSplitOptions.None);
                if (ftpConStr.Length == 3)
                {
                    ftpClient = new FTP(ftpConStr[0], ftpConStr[1], ftpConStr[2], mapPathError, appName);
                }
            }
            return ftpClient;
        }


        public static double UploadFileRemote(string appName, string localFilename, string remotePath, bool overwrite = true)
        {
            try
            {
                FTP ftpClient = CreateFTPClientProduction(appName);
                long totalBytesUploaded = 0;

                if (ftpClient != null)
                {
                    if (File.Exists(localFilename))
                        totalBytesUploaded += ftpClient.upload(localFilename, remotePath, overwrite);
                    else {
                        ftpClient = null;
                        return totalBytesUploaded;
                    }
                }
                
                if ( totalBytesUploaded <= 0 )
                    Log.ErrorLogAdmin(mapPathError, "Some error ocured while uploading file: " + localFilename + " to FTP, total Bytes uploaded: " + ftpClient.SizeSuffix(totalBytesUploaded), appName);

                ftpClient = null;
                return totalBytesUploaded;
            }
            catch (Exception ex)
            {
                Log.ErrorLogAdmin(mapPathError, "Some error ocured while uploading files to FTP, Exception:  " + ex.Message, appName);
                return -1;
            }
        }




        public void Initialize()
        {

            int check = CheckAccount();
            
#if DEBUG
            if (Request.Url.AbsolutePath != "/Admin/Publish.aspx")
                Response.Redirect("~/Admin/Publish.aspx");
#else
            if ( check < 0) 
            {
                Response.Redirect("~/");
            }
            else if (check == 0)
            {
                Response.Redirect("~/ContentValidation.aspx");
            }
            else if (check == 1)   // Administrator - GGAppAdmin
            {
                if( ! Request.Url.AbsolutePath.Contains("/Admin/Publish.aspx") )
                    Response.Redirect("~/Admin/Publish.aspx");
            }
#endif

        }

        // not so safe... ... but..
        public int CheckAccount()
        {
            if( HttpContext.Current.User.Identity != null)
                if( User.Identity.IsAuthenticated)
                {
                    string user = HttpContext.Current.User.Identity.Name;
                    if( rootWebConfig.AppSettings.Settings["authorized"].Value.Contains( user.Substring(user.IndexOf("\\")+1) ) )
                        return 1;
                    if (rootWebConfig.AppSettings.Settings["ContentValidation"].Value.Contains(user.Substring(user.IndexOf("\\") + 1)))
                        return 0;
                }
            return -1;
        }




        /// <summary>
        /// Get Latest produced Details of an App that udpated recently.
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        protected DataSet GetInfoAppData(int appID, string appName)
        {

            try
            {
                if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
                {
                    string conString = rootWebConfig.AppSettings.Settings["GG_Reporting"].Value;
                    using (SqlCommand cmd = new SqlCommand("usp_Get_Info_App", new SqlConnection(conString)))
                    {
                        using (SqlConnection con = new SqlConnection(conString))
                        {
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                cmd.Connection = con;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;

                                sda.SelectCommand = cmd;
                                using (DataSet ds = new DataSet())
                                {
                                    sda.Fill(ds);
                                    return ds;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorLogAdmin(mapPathError, appName + ": Some Exception occured in GetData(), " + e.Message, "generic");
                return null;
            }
            return null;
        }


        
        protected string GetCofigureFile(string mobileDevice, string appName, int appID, string Environment, string fileName)
        {
            string localFilename = actualWorkDir + "\\reports\\getTempfilename.txt";
            string getFileName;

            // Clear previous file.
            if (File.Exists(localFilename))
                File.Delete(localFilename);


            if (Environment == "Production")
            {
                getFileName = appName.ToLower() + "//update//" + mobileDevice + "//" + fileName;

                if (DownloadProductionFile(appName, getFileName, localFilename) == null)
                    return null;

                if (File.Exists(localFilename))
                    return File.ReadAllText(localFilename, Encoding.UTF8);
                else
                    return "";
            }
            else 
            {
                getFileName = producedAppPath + appName + "\\update\\" + mobileDevice + "\\" + fileName;

                if (File.Exists(getFileName))
                    return File.ReadAllText(getFileName, Encoding.UTF8);
                else
                    return "";
            }

        }



        private object DownloadProductionFile(string appName, string remotefilename, string localFilename)
        {
            Log.InfoLog(mapPathError, appName + ": Try get Production File " + remotefilename + " via FTP", "generic");
            FTP ftpClient = CreateFTPClientProduction(appName);

            //   try get file from production and check it.
            if (ftpClient.download(remotefilename, localFilename) <= 0)
            {
                // Check if file exists on expcted location.
                Log.ErrorLogAdmin(mapPathError, appName + ": File did not found in PRODUCTION: " + remotefilename, "generic");
                return null;
            }
            Log.InfoLog(mapPathError, appName + ": Successfully", "generic");
            return 0;
        }




        public JObject GetVersionsFileProduction(string mobileDevice, out string dbVersion, out string appVersion, out string configVersion, string appName)
        {
            dbVersion = null;           // something is wrong !
            configVersion = null;       // when return should ALWAYS have values..
            appVersion = null;          // when return should ALWAYS have values..

            string localFilename = actualWorkDir + "\\reports\\tempVersions_" + System.Guid.NewGuid().ToString() +".txt";
            string remotefilename = appName.ToLower() + "//update//" + mobileDevice + "//versions.txt";

            if (DownloadProductionFile(appName, remotefilename, localFilename) == null)
                return null;
            
            JObject o2;
            bool ok = false;
            // read JSON directly from a file
            using (StreamReader file = File.OpenText(localFilename))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    o2 = (JObject)JToken.ReadFrom(reader);
                    configVersion = o2.Value<string>("config_version");

                    if (configVersion != null)
                    {
                        dbVersion = o2.Value<string>("db_version");
                        if (dbVersion != null)
                        {
                            // get also the AppVersion !
                            appVersion = o2.Value<string>("app_version");
                            if (appVersion != null)
                            {
                                ok = true;
                            }
                        }
                    }
                }
            }

            if (ok)
            {
                if (File.Exists(localFilename))
                    File.Delete(localFilename);
                return o2;       // All good, app_version and db_version found.
            }

            return null;


        }



        protected void PrintSaveMessage(string message, string appName, string Environment, string mobileDevice, string filename)
        {
            System.Web.UI.ScriptManager.RegisterClientScriptBlock(this
                , this.GetType()
                , "Info", "alert('File "+ filename+ "saved successfuly, for " + appName + " in " + Environment + " for " + mobileDevice+ ".')", true);
        }


        protected string SaveConfigureFile(string mobileDevice, string appName, int appID, string Environment, string fileName, string fileContents)
        {

            try
            {
                // check JSON , save with intended!
                fileContents = JsonConvert.DeserializeObject(fileContents).ToString();
                
            }
            catch (Exception je)
            {
                Log.ErrorLogAdmin(mapPathError, appName + ": not valid json for " + fileName + ", " + je.Message, "generic");
                return "Not A Valid JSON file.";
            }
            
                        
            string localFilename = actualWorkDir + "\\reports\\tempVersions_" + System.Guid.NewGuid().ToString() + ".txt";
            string setFileName;

            // Clear previous file.
            if (File.Exists(localFilename))
                File.Delete(localFilename);


            if (Environment == "Production")
            {
                setFileName = appName.ToLower() + "//update//" + mobileDevice + "//" + fileName;

                File.WriteAllText(localFilename, fileContents, Encoding.UTF8);

                if (File.Exists(localFilename))
                {
                    try
                    {
                        if (UploadFileRemote(appName, localFilename, setFileName) <= 0)
                            return null;
                        else
                            return fileName;
                    }
                    catch (Exception e)
                    {
                        Log.ErrorLogAdmin(mapPathError, appName + ": Some Critical Exception occured when try to save file: " + setFileName + ", " + appName + ", " + mobileDevice + ", " + Environment + "  , " + e.Message, "generic");
                        return null;
                    }
                }
                else
                    return "";
            }
            else
            {
                setFileName = producedAppPath + appName + "\\update\\" + mobileDevice + "\\" + fileName;

                if (File.Exists(setFileName))
                    File.Delete(setFileName);
                
                try{
                    File.WriteAllText(setFileName, fileContents, Encoding.UTF8);
                    return fileName;
                } 
                catch(Exception e) 
                {
                    Log.ErrorLogAdmin(mapPathError, appName + ": Some Critical Exceprion occured when try to save file: " + setFileName + ", " + appName + ", " + mobileDevice + ", " + Environment + "  , " + e.Message, "generic"); 
                    return null;
                }
                
            }

        }



        protected bool PutOnLineProduction(string appName, string mobileDevice, string environment)
        {
            if (RenameFileRemote(appName, appName.ToLower() + "/update/" + mobileDevice + "/versions_OFFLINE.txt", "versions.txt") > 0)
                return true;

            return false;
        }

        protected bool PutOffLineProduction(string appName, string mobileDevice, string environment)
        {
            if (RenameFileRemote(appName, appName.ToLower() + "/update/" + mobileDevice + "/versions.txt", "versions_OFFLINE.txt") > 0)
                return true;
            
            return false;

        }

        protected bool PutOnLine(string appName, string mobileDevice, string environment)
        {
            var fileName = producedAppPath + appName + "\\update\\" + mobileDevice + "\\versions.txt";
            var fileNameOffLine = producedAppPath + appName + "\\update\\" + mobileDevice + "\\versions_OFFLINE.txt";

            if (File.Exists(fileNameOffLine))
            {
                File.Move(fileNameOffLine, fileName); // move old OFFLINE versions to versions.txt.
            }
            else
                return false;

            return true;
        }

        protected bool PutOffLine(string appName, string mobileDevice, string environment)
        {
            var fileName = producedAppPath + appName + "\\update\\" + mobileDevice + "\\versions.txt";
            var fileNameOffLine = producedAppPath + appName + "\\update\\" + mobileDevice + "\\versions_OFFLINE.txt";

            if (File.Exists(fileName))
            {
                File.Move(fileName, fileNameOffLine); // move old OFFLINE versions to versions.txt.
            }
            else
                return false;

            return true;
        }


        public static bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;

            return input.Any(c => c > MaxAnsiCode);
        }

        
        /// <summary>
        /// Send Email to Users
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="team"></param>
        /// <param name="attachmentFilenames"></param>
        /// <param name="emailBody"></param>
        /// <param name="emailSubject"></param>
        /// <returns></returns>
        public async Task SendMailToUsers(string appName, List<string> team, List<string> attachmentFilenames, string emailBody, string emailSubject, string mapPath, CreateLogFiles log)
        {

            using (var message = new MailMessage())
            {
                message.From = new MailAddress("noreply@greekguide.com");
                message.Subject = emailSubject;
                message.Body = emailBody;
                message.IsBodyHtml = true;

                // Build Receipents List
                foreach (String recStr in team)
                {
                    message.To.Add(new MailAddress(recStr));
                }

                try
                {
                    // ADD FILE ATTACHEMENTS IF THEY EXIST
                    foreach (string attachmentFilename in attachmentFilenames)
                    {
                        if (attachmentFilename != null)
                        {
                            Attachment attachment = new Attachment(attachmentFilename, MediaTypeNames.Application.Octet);
                            ContentDisposition disposition = attachment.ContentDisposition;
                            disposition.CreationDate = File.GetCreationTime(attachmentFilename);
                            disposition.ModificationDate = File.GetLastWriteTime(attachmentFilename);
                            disposition.ReadDate = File.GetLastAccessTime(attachmentFilename);
                            disposition.FileName = Path.GetFileName(attachmentFilename);
                            disposition.Size = new FileInfo(attachmentFilename).Length;
                            disposition.DispositionType = DispositionTypeNames.Attachment;
                            message.Attachments.Add(attachment);
                        }
                    }

                }
                catch (Exception ex)
                {
                    // FileAttachements not found for App! for this date.
                    log.InfoLog(mapPath, "Exception thrown while trying to collect attachment files: " + ex.Message, appName);
                }

                try
                {
                    string GenericEmailUserName = "", GenericEmailPswd = "", sSmtpClient = "";
                    if (rootWebConfig.AppSettings.Settings["GenericEmailUserName"] != null)
                    {
                        GenericEmailUserName = rootWebConfig.AppSettings.Settings["GenericEmailUserName"].Value;
                    }

                    if (rootWebConfig.AppSettings.Settings["GenericEmailPswd"] != null)
                    {
                        GenericEmailPswd = rootWebConfig.AppSettings.Settings["GenericEmailPswd"].Value;
                    }

                    if (rootWebConfig.AppSettings.Settings["SmtpClient"] != null)
                    {
                        sSmtpClient = rootWebConfig.AppSettings.Settings["SmtpClient"].Value;
                    }



                    // Finaly send email ..
                    var smtp = new SmtpClient
                    {
                        Host = sSmtpClient,
                        Port = 25,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new System.Net.NetworkCredential(GenericEmailUserName, GenericEmailPswd)
                    };

                    smtp.SendCompleted += smtp_SendCompleted;
                    await smtp.SendMailAsync(message);

                }
                catch (SmtpException sE)
                {
                    log.InfoLog(mapPath, "Exception while sending notification email! " + sE.Message, appName);
                }
            }

        }


        // Verify that email had been sent.
        void smtp_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Log.InfoLog(mapPathError, "e-Mails sent completed.", Session["appName"].ToString());
            }
        }


        /// <summary>
        /// Get the email list from web.config file
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public List<string> GetEmailList(string team)
        {
            List<string> mailList = new List<string>();


            if (rootWebConfig.AppSettings.Settings[team] != null)
            {
                string teamMembers = rootWebConfig.AppSettings.Settings[team].Value;
                mailList = teamMembers.Split('|').ToList<string>();

                return mailList;
            }

            return null;

        }



        public string EmailTemplate(string kind, string appName, string startProcessing, string LogErrorRealPath = null)
        {
            String str = String.Empty;

            switch (kind)
            {
                case "Success":
                    if (File.Exists(Server.MapPath("EmailTemplates//EmailTemplateSuccess.html")))
                    {
                        str = File.ReadAllText(Server.MapPath("EmailTemplates//EmailTemplateSuccess.html"), Encoding.UTF8);
                    }
                    break;
                case "Failure":
                    if (File.Exists(Server.MapPath("EmailTemplates//EmailTemplateFailure.html")))
                    {
                        str = File.ReadAllText(Server.MapPath("EmailTemplates//EmailTemplateFailure.html"), Encoding.UTF8);
                    }
                    break;
                case "Info":
                    if (File.Exists(Server.MapPath("EmailTemplates//EmailTemplateInfo.html")))
                    {
                        str = File.ReadAllText(Server.MapPath("EmailTemplates//EmailTemplateInfo.html"), Encoding.UTF8);
                    }
                    break;
                case "Start":
                    if (File.Exists(Server.MapPath("EmailTemplates//EmailTemplateStart.html")))
                    {
                        str = File.ReadAllText(Server.MapPath("EmailTemplates//EmailTemplateStart.html"), Encoding.UTF8);
                    }
                    break;
                case "InitAdmin":
                    if (File.Exists(Path.Combine(HttpRuntime.AppDomainAppPath, "EmailTemplates//EmailTemplateInitAdmin.html")))
                    {
                        str = File.ReadAllText(Path.Combine(HttpRuntime.AppDomainAppPath, "EmailTemplates//EmailTemplateInitAdmin.html"), Encoding.UTF8);
                    }
                    break;
                default: break;
            }

            if (!String.IsNullOrWhiteSpace(str))
            {
                str = str.Replace("{1}", startProcessing);
                str = str.Replace("{2}", appName);
                if (LogErrorRealPath != null)
                    str = str.Replace("{3}", LogErrorRealPath);

                return str;
            }

            return "<html></html>";
        }


    }
}
