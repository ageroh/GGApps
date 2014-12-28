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
    public class Finalize : Common
    {
        public string appName = string.Empty;
        public int appID = -1;
        public string producedAppPath = string.Empty;
        public static string logPath = string.Empty;

        public Finalize(string appName, int appID)
        {
            this.appName = appName;
            this.appID = appID;
            this.producedAppPath = rootWebConfig.AppSettings.Settings["ProducedAppPath"].Value.ToString();
            logPath = mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt";
        }



        /// <summary>
        /// Check if configuration file exists in DB
        /// </summary>
        protected JObject GetConfigurationFile(string mobileDevice, out string configVersion, string appName)
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



        protected JObject GetVersionsFile(string mobileDevice, out string dbVersion, out string appVersion, out string configVersion, string appName)
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

                Log.ErrorLog(logPath, " Versions file did not found in the expected location: " + fileName, appName);
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
            DataTable dt = GetAllAppBundles(appID, mapPathError);


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
                Log.ErrorLog(logPath, "Some IO exception occured on UpdateSQLiteUserVersion: " + ex.Message, appName);
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
                Log.ErrorLog(mapPathError, e.Message, "generic", "");
            }
            return null;

        }


        // should be once used to produce records on DB, then may update...
        protected void InitializeDBFromFiles(string mobileDevice)
        {
            DirectoryInfo di = new DirectoryInfo(producedAppPath);

            foreach (DirectoryInfo dirInfo in di.GetDirectories())
            {
                if (!dirInfo.Name.Contains("empty")) // leave out the empty directory
                {
                    // scan based to DataTable of all apps, all folders ...
                    DataTable dt = GetAllAppTable();

                    //when found create a record on DB, if its not already craeted.
                    string configVersionNumber;
                    string DBVersionNumber;
                    string appVersionNumber;
                    string fileConfigVersionNumber;

                    // get Configuration File and Version
                    JObject configVerJSON = GetConfigurationFile(mobileDevice, out configVersionNumber, dirInfo.Name.ToLower());

                    // get DB Version File, DB Version, App version, configVersion.
                    JObject dbVerJSON = GetVersionsFile(mobileDevice, out DBVersionNumber, out appVersionNumber, out fileConfigVersionNumber, dirInfo.Name.ToLower());


                    if (fileConfigVersionNumber != configVersionNumber)   // conflict to Configuration files!
                    {
                        Log.ErrorLog(mapPathError, "Conflict on configuration.txt and versions.txt -> Configuration_Version. Resolved by keeping configuration.txt.\n\t\t\t -> versions.txt:(" + configVersionNumber + ") " +
                                                    "\n\t\t\t -> configuration.txt:(" + fileConfigVersionNumber + ") ", "generic");
                        configVersionNumber = fileConfigVersionNumber;
                    }

                    if (dt != null)
                    {

                        if (dt.Rows.Count > 0)
                        {
                            //dirInfo.Name.ToLower()


                            var results = from dsx in dt.AsEnumerable()
                                          where dsx.Field<string>("appName").ToLower().Trim() == dirInfo.Name.ToLower().Trim()
                                          select dsx;


                            BackOffice.UpdateAppBundle(mobileDevice, ((string)results.CopyToDataTable().Rows[0][1]).ToLower(), ((int)results.CopyToDataTable().Rows[0][0]), appVersionNumber, configVersionNumber, DBVersionNumber, configVerJSON, dbVerJSON);
                        }
                    }
                    //// no app Found to match selected folder 
                    //{
                    //    log.ErrorLog(mapPathError, "No Directory found for App : " + appName + " when initializing Backoffice DB.", "generic");

                    //}
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