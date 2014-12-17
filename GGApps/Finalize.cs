using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Hosting;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GGApps
{
    public class Finalize
    {

        public static bool HasErrors = false;
        public static string actualWorkDir = HostingEnvironment.MapPath("~/Batch/");
        public string appName = string.Empty;
        public int appID = -1;
        public string mapPathError;
        public string mapPath = "";
        public string logPath = string.Empty;
        public string producedAppPath = string.Empty;
        public CreateLogFiles log = new CreateLogFiles();
        System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
        private string p1;
        private string appName1;
        private int appID1;
        private string p2;

        public Finalize(string mapPath, string appName, int appID, string mapPathError)
        {
            this.mapPath = mapPath;
            this.appName = appName;
            this.appID = appID;
            this.mapPathError = mapPathError;
            this.logPath = mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt";
            this.producedAppPath = rootWebConfig.AppSettings.Settings["ProducedAppPath"].Value.ToString();
        }



        /// <summary>
        /// Check if configuration file exists in DB
        /// </summary>
        protected JObject GetConfigurationFile(string mobileDevice, out string configVersion)
        {
            // open Configuration file if exists
            var fileName = producedAppPath + appName + "\\update\\" + mobileDevice + "\\configuration.txt";
            configVersion = "1";    // this is the default configuration Version to play the App.

            // Check if file exists on expcted location.
            if (File.Exists(fileName))
            {
                // read JSON directly from a file
                using (StreamReader file = File.OpenText(fileName))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o2 = (JObject)JToken.ReadFrom(reader);
                        JToken jconfigVersion;
                        o2.TryGetValue("configuration_version", out jconfigVersion);
                        if (jconfigVersion != null)
                        {
                            configVersion = jconfigVersion.Value<string>();
                            return o2;
                        }
                    }
                }
            }
            return null;

        }



        protected JObject GetVersionsFile(string mobileDevice, out string dbVersion, out string appVersion, out string configVersion)
        {
            // open Configuration file if exists
            var fileName = producedAppPath + appName + "\\update\\" + mobileDevice + "\\versions.txt";
            dbVersion = null;           // something is wrong !
            configVersion = null;       // when return should ALWAYS have values..
            appVersion = null;          // when return should ALWAYS have values..

            // Check if file exists on expcted location.
            if (!File.Exists(fileName))
            { 
                // if configuration file does not exists, then create a default, and keep a LOG!
                CreateVersionsFile(fileName);

                log.ErrorLog(logPath, " Versions file did not found in the expected location: " + fileName, appName);
            }

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
            return null;

        }




        protected void CreateVersionsFile(string filename)
        { 
            // Create configuration JSON default file 
            File.WriteAllText(filename, @"{  ""app_version"": ""2.1"",  ""config_version"": ""1"",  ""db_version"": ""1""}", System.Text.Encoding.UTF8);
        }


        /// <summary>
        /// Update Version file +1 if needed.
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appID"></param>
        /// <param name="path"></param>
        /// <param name="log"></param>
        /// <param name="filename"></param>
        public void UpdateDBVersion()
        {
            // Get The list of all apps
            DataTable dt = GetAllAppBundles(appID, this.mapPathError);


            InitializeDBFromFiles("ios");
            InitializeDBFromFiles("android");

            UpdateSQLiteUserVersion("EN");

            UpdateSQLiteUserVersion("EL");

            if( _Default.CheckThreeLanguages(this.appID))
                UpdateSQLiteUserVersion("RU");

        }

        
        protected void UpdateSQLiteUserVersion(string dbLang, int addVersion = 1)
        {

            try
            {
                using (SQLiteConnection con = new SQLiteConnection("Data Source=" + mapPath + "Batch\\dbfiles\\GreekGuide_" + appName + "_" + dbLang + "_" + DateTime.Now.ToString("yyyyMMdd") + ".db; Version=3;"))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA USER_VERSION;", con))
                    {
                        int curVersion = 0;
                        con.Open();
                        Int32.TryParse(cmd.ExecuteScalar().ToString(), out curVersion);

                        curVersion = curVersion + addVersion;       // add plus one to existing version file.
                        cmd.CommandText = "PRAGMA USER_VERSION = " + curVersion + " ;";
                        cmd.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorLog(logPath, "Some IO exception occured on UpdateSQLiteUserVersion: " + ex.Message, appName);
            }
        
        }


        /// <summary>
        /// Get all App Versions bundles produced for Specific App only
        /// </summary>
        /// <returns></returns>
        protected DataTable GetAllAppBundles(int appID, string mapPathError)
        {
            try
            {
                if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
                {
                    using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                    {
                        using (SqlCommand cmd = new SqlCommand("usp_get_AllAppBundles", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
                    
                            con.Open();
                            cmd.ExecuteNonQuery();

                            SqlDataAdapter adp = new SqlDataAdapter(cmd);
                            DataTable dt = new DataTable();
                            adp.Fill(dt);

                            return dt;

                        }
                    }
                    
                }
            }
            catch (Exception e)
            {
                HasErrors = true;
                log.ErrorLog(mapPathError, e.Message, "generic", "");
            }
            return null;

        }


        // should be once used to produce records on DB, then may update...
        protected void InitializeDBFromFiles(string mobileDevice)
        {
            DirectoryInfo di = new DirectoryInfo(producedAppPath);

            foreach (DirectoryInfo dirInfo in di.GetDirectories())
            { 
                if(!dirInfo.Name.Contains("empty")) // leave out the empty directory
                {   
                    // scan based to DataTable of all apps, all folders ...
                    var query = _Default.GetAllAppTable().AsEnumerable().Where(appName => appName.ToString().ToLower() == dirInfo.Name.ToLower());

                    //when found create a record on DB, if its not already craeted.
                    if (query != null)
                    {
                        string configVersionNumber;
                        string DBVersionNumber;
                        string appVersionNumber;
                        string fileConfigVersionNumber;

                        float fconfigVersionNumber;
                        float fDBVersionNumber;
                        float fappVersionNumber;
                        float ffileConfigVersionNumber;

                        
                        // get Configuration File and Version
                        JObject configVer = GetConfigurationFile(mobileDevice, out configVersionNumber);

                        // get DB Version File, DB Version, App version, configVersion.
                        JObject dbVer = GetVersionsFile(mobileDevice, out DBVersionNumber, out appVersionNumber, out fileConfigVersionNumber);

                        
                        float.TryParse(configVersionNumber, out fconfigVersionNumber);
                        float.TryParse(DBVersionNumber, out fDBVersionNumber);
                        float.TryParse(appVersionNumber, out fappVersionNumber);
                        float.TryParse(fileConfigVersionNumber, out ffileConfigVersionNumber);

                        if (ffileConfigVersionNumber != fconfigVersionNumber)   // conflict to Configuration files!
                        {
                            log.ErrorLog(mapPathError, "Conflict on configuration.txt and versions.txt -> Configuration_Version. Resolved by keeping configuration.txt.\n\t\t\t -> versions.txt:(" + fconfigVersionNumber + ") " +
                                                        "\n\t\t\t -> configuration.txt:(" + ffileConfigVersionNumber + ") " , "generic");
                            fconfigVersionNumber = ffileConfigVersionNumber;
                        }

                        BackOffice.UpdateAppBundle(mobileDevice, appName, appID, fappVersionNumber, fconfigVersionNumber, fDBVersionNumber, configVer.ToString(), dbVer.ToString());
                        

                    }
                    else    // no app Found to match selected folder 
                    {
                        log.ErrorLog(mapPathError, "No Directory found for App : " + appName + " when initializing Backoffice DB.", "generic");
                            
                    }

                }
            }
        
        }



        /*
         *                         var results = from myRow in _Default.GetAllAppTable().AsEnumerable()
                                      where myRow.Field<int>("appID") == appID
                        select myRow;

         */
    }
}