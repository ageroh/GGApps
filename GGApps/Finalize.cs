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
        public static string producedAppPath = rootWebConfig.AppSettings.Settings["ProducedAppPath"].Value.ToString();
        public static string logPath = string.Empty;
        public const int MAJOR = 1;
        public const int MINOR = 0;

        public Finalize(string appName, int appID)
        {
            this.appName = appName;
            this.appID = appID;
            logPath = mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt";
        }


        public Finalize()
        {
            this.appName = "generic";
            logPath = mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt";
        }


        /// <summary>
        /// Check if configuration file exists in DB
        /// </summary>
        public JObject GetConfigurationFile(string mobileDevice, out string configVersion, string appName)
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




        public static void CreateVersionsFile(string filename)
        { 
            // Create configuration JSON default file 
            File.WriteAllText(filename, @"{  ""app_version"": ""2.1"",  ""config_version"": ""1"",  ""db_version"": ""1""}", System.Text.Encoding.UTF8);
        }




        /// <summary>
        /// Update all DB SQLite versions to a Minor ver. 
        /// Use MAJOR const if an MAJOR update is needed (production)
        /// USe int to update to any other Version you wish
        /// </summary>
        /// <param name="ver">Default is MINOR always</param>
        public object UpdateDBVersion(string mobileDevice, int ver = MINOR)
        {

            if (UpdateSQLiteUserVersion("EN", mobileDevice, ver) == null)
                return null;

            if (UpdateSQLiteUserVersion("EL", mobileDevice, ver) == null)
                return null;

            if (CheckThreeLanguages(this.appID))
                if (UpdateSQLiteUserVersion("RU", mobileDevice, ver) == null)
                    return null;

            return 0;

        }


        
        public object UpdateSQLiteUserVersion(string dbLang , string mobileDevice, int addVersion = 1)
        {

            try
            {
                
                string curVerStr = "";
                string curVerStrProduced = null;

                // two connection strings 
                // get current version of DB from last Test occured!
                using (SQLiteConnection conProduced = new SQLiteConnection("Data Source=" + "C:\\GGAppContent\\"+ this.appName + "\\update\\" + mobileDevice + "\\Content" + dbLang.ToUpper() + ".db; Version=3;"))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA USER_VERSION;", conProduced))
                    {
                        conProduced.Open();
                        curVerStrProduced = cmd.ExecuteScalar().ToString();
                        conProduced.Close();
                    }
                }

                using (SQLiteConnection con = new SQLiteConnection("Data Source=" + mapPath + "Batch\\dbfiles\\" + mobileDevice +"\\GreekGuide_" + this.appName + "_" + dbLang + "_" + DateTime.Now.ToString("yyyyMMdd") + ".db; Version=3;"))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA USER_VERSION;", con))
                    {
                        int curVersion = 0;
                        int major = 0, minor = 0;
                        string newVersionStr = "";
                        con.Open();
                        curVerStr = cmd.ExecuteScalar().ToString();
                        con.Close();

                        if (curVerStr == "0" && (curVerStrProduced == null || curVerStrProduced == "0"))
                            curVerStr = InitializeSQLiteVersionFromDB(appID, appName, LangToInt(dbLang), mobileDevice);
                        else
                            curVerStr = curVerStrProduced;


                        if (curVerStr == null)
                        {
                            Log.ErrorLog(logPath, "No DB version for SQL Lite for " + dbLang + " of " + this.appName, this.appName);
                            return null;
                        }

                        if (addVersion == 0)    // means add MINOR version to SQLite
                        {                       // maybe needs a record in Admin DB that an minor update occured.

                            if (curVerStr.Contains("."))
                            {
                                // already in a minor, so increase minor.
                                major = Convert.ToInt32(curVerStr.Split('.')[0]);
                                minor = Convert.ToInt32(curVerStr.Split('.')[1]);
                                curVerStr = major.ToString() + "." + minor.ToString();
                                minor++;
                                newVersionStr = major.ToString() + "." + minor.ToString();

                                con.Open();
                                cmd.CommandText = "PRAGMA USER_VERSION = " + newVersionStr + " ;";
                                cmd.ExecuteNonQuery();
                                con.Close();
                                Log.InfoLog(logPath, "DB to moved to MAJOR Version from MINOR: " + curVerStr + " to  :" + newVersionStr, this.appName);
                                return 1;

                            }
                            else // first minor add.
                            {
                                newVersionStr = curVerStr + ".1";
                                con.Open();
                                cmd.CommandText = "PRAGMA USER_VERSION = " + newVersionStr + " ;";
                                cmd.ExecuteNonQuery();
                                con.Close();
                                Log.InfoLog(logPath, "DB created first MINOR version: " + newVersionStr, this.appName);
                                return 2;

                            }
                        }
                        else// Means MAJOR add
                        {

                            if (curVerStr.Contains("."))
                            {
                                // get only major ver and ADD!
                                major = Convert.ToInt32(curVerStr.Split('.')[0]);
                                minor = Convert.ToInt32(curVerStr.Split('.')[1]);

                                if (addVersion > 1) 
                                    major = major + addVersion;
                                else 
                                    major++;

                                newVersionStr = major.ToString();
                                cmd.CommandText = "PRAGMA USER_VERSION = " + newVersionStr + " ;";
                                con.Open();
                                cmd.ExecuteNonQuery();
                                con.Close();
                                Log.InfoLog(logPath, "DB to moved to MAJOR Version from MINOR: " + major + "." + minor + " to  :" + newVersionStr, this.appName);
                                return 3;
                            }
                            else
                            {
                                Int32.TryParse(curVerStr, out curVersion);
                                curVersion = curVersion + addVersion;       // add plus one to existing version file.
                                newVersionStr = curVersion.ToString();
                                con.Open();
                                cmd.CommandText = "PRAGMA USER_VERSION = " + newVersionStr + " ;";
                                cmd.ExecuteNonQuery();
                                con.Close();
                                Log.InfoLog(logPath, "DB to moved to MAJOR Version from MAJOR: " + curVersion + " to  :" + newVersionStr, this.appName);
                                return 4;
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorLog(logPath, "Some exception occured on UpdateSQLiteUserVersion: " + ex.Message, appName);
                return null;
            }

        }

        private string InitializeSQLiteVersionFromDB(int appID, string appName, int dbLang, string mobileVersion)
        {
            DataTable dt = new DataTable();

            // read DB version from admin table.
            if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
            {
                using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                {
                    con.Open();

                    // Get initial values of versions from SQL server Admin.
                    using (SqlCommand command = new SqlCommand("usp_Get_Info_App", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
                        command.Parameters.Add("@langID", SqlDbType.Int).Value = dbLang;
                        command.Parameters.Add("@mobileDevice", SqlDbType.VarChar).Value = mobileVersion;

                        command.CommandTimeout = 2000;
                        SqlDataAdapter adp = new SqlDataAdapter(command);
                        adp.Fill(dt);

                    }
                    con.Close();
                }

                // Do the work with DataTable
                if(dt == null)
                {
                    Log.ErrorLog(mapPathError, "No App configuration found for " + appName, appName);
                    return null;
                }

                
                // most reasonably one row would be returned every time.
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["db_version"] != DBNull.Value &&
                        dr["mobileDevice"] != DBNull.Value &&
                        dr["langID"] != DBNull.Value)
                        return SetDBVersionForApp(dr["db_version"].ToString(), dr["mobileDevice"].ToString(), dr["langID"].ToString());
                }

                return "0";
                
            }

            return null;

        }



        private string SetDBVersionForApp(string db_version, string mobiledevice, string langID)
        {
            try
            {
                using (SQLiteConnection con = new SQLiteConnection("Data Source=" + mapPath + "Batch\\dbfiles\\" + mobiledevice + "\\GreekGuide_" + this.appName + "_" + langID + "_" + DateTime.Now.ToString("yyyyMMdd") + ".db; Version=3;"))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA USER_VERSION;", con))
                    {
                        con.Open();
                        cmd.CommandText = "PRAGMA USER_VERSION = " + db_version + " ;";
                        cmd.ExecuteNonQuery();
                        con.Close();
                        return db_version;
                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorLog(mapPathError, "Failed to initialize DB Version on SQLite DB for Lang:" + langID + " for " + mobiledevice + " Exception: " + e.Message, appName);
                return "0";
            }

        }


        /// <summary>
        /// Get all App Versions bundles produced for Specific App only
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllAppBundles(int appID, string mapPathError)
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
                Log.ErrorLog(mapPathError, e.Message, "generic", "");
                return null;
            }
            return null;

        }




        // should be once used to produce records on DB, then may update...
        internal object InitializeDBFromFiles(string mobileDevice)
        {
            try
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


                                if (UpdateAppBundle(mobileDevice, ((string)results.CopyToDataTable().Rows[0][1]).ToLower(), ((int)results.CopyToDataTable().Rows[0][0]), appVersionNumber, configVersionNumber, DBVersionNumber, configVerJSON, dbVerJSON) == null)
                                    return null;

                                return 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorLog(mapPathError, "Exception on  InitializeDBFromFiles when initializing Backoffice DB." + ex.Message, "generic");
                return null;
            }

            return 0;
        }


        public static string UpdateAppBundle(string mobileDevice, string appName, int appID, string versionNumber, string ConfiguratioNumber, string DBVersionNumber, JObject ConfigurationJSON, JObject VersionJSON)
        {
            try
            {

                if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
                {
                    string sConfigurationJSON = "", sVersionJSON = "";
                    try { sConfigurationJSON = ConfigurationJSON.ToString(); sVersionJSON = VersionJSON.ToString(); }
                    catch (Exception e) { }
                    using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                    {
                        using (SqlCommand cmd = new SqlCommand("usp_Update_Bundle", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
                            cmd.Parameters.Add("@App_VersionNumber", SqlDbType.NVarChar).Value = versionNumber;
                            cmd.Parameters.Add("@ConfiguratioNumber", SqlDbType.NVarChar).Value = ConfiguratioNumber;
                            cmd.Parameters.Add("@DBVersionNumber", SqlDbType.NVarChar).Value = DBVersionNumber;
                            cmd.Parameters.Add("@VersionJSON", SqlDbType.NVarChar).Value = sVersionJSON;
                            cmd.Parameters.Add("@ConfigurationJSON", SqlDbType.NVarChar).Value = sConfigurationJSON;
                            cmd.Parameters.Add("@mobileDevice", SqlDbType.NVarChar).Value = mobileDevice;

                            con.Open();
                            string res = ((string)cmd.ExecuteScalar());

                            return res;

                        }
                    }

                }
                else
                    return null;
            }
            catch (Exception e)
            {
                Log.ErrorLog(mapPathError, "Exception thrown UpdateAppBundle: " + e.Message, "generic");
                return null;
            }
            

        }



    }
}