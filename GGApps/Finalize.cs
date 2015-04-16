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
        ///  Get Versions.txt from production, check its configuration and app_version numbers, update necessarily based to python executed results found in GGAppsVersions
        ///  Also update approprietly the Configuration_version based to configuration id found in configuration.txt if exists.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="appName"></param>
        /// <param name="appId"></param>
        /// <param name="mobileDevice"></param>
        /// <param name="LangID"></param>
        /// <returns></returns>
        public object UpdateVerionsProductionLive(string realAppVersion, string appName, int appId, string mobileDevice)
        {
            string configVersionNumber;
            string DBVersionNumber;
            string appVersionNumber;
            string fileConfigVersionNumber;
            bool needsUpdate = false;
            string nVersionTXT = "";

            // get Configuration File and Version
            JObject configVerJSON = GetConfigurationFileProduction(appName, appID, mobileDevice, out configVersionNumber);
            
            // get DB Version File, DB Version, App version, configVersion.
            JObject dbVerJSON = GetVersionsFileProduction(mobileDevice, out DBVersionNumber, out appVersionNumber, out fileConfigVersionNumber, appName);
            if (dbVerJSON == null)
                return null;

            if( configVerJSON != null )
                if (fileConfigVersionNumber != configVersionNumber)   // conflict to Configuration files!
                {
                    fileConfigVersionNumber = configVersionNumber;
                    needsUpdate = true;
                }

            if (realAppVersion != appVersionNumber)
                needsUpdate = true;
            
            if( needsUpdate )
            {
                // then really needs to change !
                nVersionTXT = "{  \"app_version\": \"" + realAppVersion + "\",   \"config_version\": \"" + fileConfigVersionNumber + "\",   \"db_version\": \"" + DBVersionNumber + "\" }";

                // upload to server
                string res = SaveConfigureFile(mobileDevice, appName, appID, "Production", "versions.txt", nVersionTXT);
                if (res == null)
                    return null;    // error

                Log.InfoLog(mapPathError, appName + "::" + mobileDevice + ":: New App Version - Configuration found: New App_Version = " + realAppVersion + " New Configuration_Version = " + fileConfigVersionNumber, "generic");
            }

            return nVersionTXT;


        }

        public bool SetVerionsFileProperty(string property, string value, string appName, int appId, string mobileDevice, int LangID, string typeOfConfigfile)
        {
            string strPropertyValue, fileName = "";
            //// read versions file

            try
            {
                JObject nObject;
                fileName = producedAppPath + appName + "\\update\\" + mobileDevice + "\\" + typeOfConfigfile;

                // Update JObject property
                using (StreamReader file = File.OpenText(fileName))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject code = (JObject)JToken.ReadFrom(reader);
                        strPropertyValue = code.Value<string>(property);  //"db_version");
                        code[property] = value;
                        nObject = code;
                    }
                }

                if(File.Exists(fileName))
                    File.Delete(fileName);

                using (Stream stream = File.OpenWrite(fileName))
                {
                    stream.Flush();
                    using (var streamWriter = new StreamWriter(stream))
                    {
                        using (var writer = new JsonTextWriter(streamWriter))
                        {
                            writer.Formatting = Formatting.Indented;
                            writer.WriteRaw(nObject.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorLogAdmin(mapPathError, "JSON> Setting property " + property + " with value: " + value + " on file " + fileName + " failed! , " + ex.Message, appName);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Get Configuration version from cofiguration.txt if exists
        /// </summary>
        /// <param name="mobileDevice"></param>
        /// <param name="configVersion"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
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


        public JObject GetConfigurationFileProduction(string appName, int appID, string mobileDevice, out string configVersion)
        {
            configVersion = "";
            try
            {
                // always from production 
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(GetCofigureFile(mobileDevice, appName, appID, "Production", "configuration.txt"));

                // read JSON directly from a file
                using (var file = new MemoryStream(byteArray))
                {
                    file.Position = 0;
                    var sr = new StreamReader(file);

                    using (JsonTextReader reader = new JsonTextReader(sr))
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
                return null;
            }
            catch (Exception e)
            {
                Log.ErrorLogAdmin(mapPathError, "Some exception occured in GetConfigurationFileProduction(), " + e.Message, appName);
                return null;
            }
            
        }

        




        /// <summary>
        /// Update all DB SQLite versions to a Minor ver. 
        /// Use MAJOR const if an MAJOR update is needed (production)
        /// USe int to update to any other Version you wish
        /// </summary>
        /// <param name="ver">Default is MINOR always</param>
        public object UpdateDBVersion(string mobileDevice, int ver = MINOR)
        {

            if (UpdateSQLiteUserVersionSIMPLE("EN", mobileDevice, ver) == null)
                return null;

            if (UpdateSQLiteUserVersionSIMPLE("EL", mobileDevice, ver) == null)
                return null;

            if (CheckThreeLanguages(this.appID))
                if (UpdateSQLiteUserVersionSIMPLE("RU", mobileDevice, ver) == null)
                    return null;

            return 0;

        }


        /// <summary>
        /// For device and Lang and App, auto-update DB version in SQLite Db, based on value in ADMIN MSSQL, NO MINOR-MAJOR Versions!
        /// </summary>
        /// <param name="dbLang"></param>
        /// <param name="mobileDevice"></param>
        /// <param name="addVersion"></param>
        /// <returns></returns>
        public object UpdateSQLiteUserVersionSIMPLE(string dbLang, string mobileDevice, int addVersion = 1)
        {

            try
            {

                string curVerStr = "";

                using (SQLiteConnection con = new SQLiteConnection("Data Source=" + mapPath + "Batch\\dbfiles\\" + mobileDevice + "\\GreekGuide_" + this.appName + "_" + dbLang + "_" + DateTime.Now.ToString("yyyyMMdd") + ".db; Version=3;"))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA USER_VERSION;", con))
                    {
                        int major = 0;
                        string newVersionStr = "";

                        // always initialize with version shown from DB.
                        curVerStr = InitializeSQLiteVersionFromDB(appID, appName, LangToInt(dbLang), mobileDevice);


                        if (String.IsNullOrEmpty(curVerStr))
                        {
                            Log.ErrorLogAdmin(mapPathError, "No DB version for SQL Lite for " + dbLang + " of " + this.appName, this.appName);
                            return null;
                        }

                        if (addVersion == 0)        // Add one version to existing DB_version.     
                        {
                            // get curVerStr
                            Int32.TryParse(curVerStr, out major);
                            major++;                                // just add +1 to ver.
                            newVersionStr = major.ToString();

                        }
                        else// MEANS PRODUCTION
                        {
                           // make ver whatever addVersion is
                            newVersionStr = addVersion.ToString();
                        }

                        /*
                            * resolve BUG to Android APK first.

                            con.Open();
                            cmd.CommandText = "PRAGMA USER_VERSION = " + newVersionStr + " ;";
                            cmd.ExecuteNonQuery();
                            con.Close();
                                
                        */

                        if (AddDBversionAdmin(appName, appID, curVerStr, newVersionStr, mobileDevice, dbLang) == null)
                        {
                            Log.ErrorLogAdmin(logPath, "Error occured in AddDBversionAdmin DB ver numbers, for " + dbLang + " of " + this.appName, this.appName);
                            return null;
                        }

                        Log.InfoLog(mapPathError, "DB ver added : " + newVersionStr, this.appName);
                        return 1;



                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorLogAdmin(mapPathError, "Some exception occured on UpdateSQLiteUserVersion: " + ex.Message, appName);
                return null;
            }

        }




        /// <summary>
        /// For device and Lang and App, auto-update DB version in SQLite Db, based on value in ADMIN MSSQL.
        /// </summary>
        /// <param name="dbLang"></param>
        /// <param name="mobileDevice"></param>
        /// <param name="addVersion"></param>
        /// <returns></returns>
        /*
        public object UpdateSQLiteUserVersion(string dbLang , string mobileDevice, int addVersion = 1)
        {

            try
            {
                
                string curVerStr = "";
                
                using (SQLiteConnection con = new SQLiteConnection("Data Source=" + mapPath + "Batch\\dbfiles\\" + mobileDevice +"\\GreekGuide_" + this.appName + "_" + dbLang + "_" + DateTime.Now.ToString("yyyyMMdd") + ".db; Version=3;"))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA USER_VERSION;", con))
                    {
                        int curVersion = 0;
                        int major = 0, minor = 0;
                        string newVersionStr = "";
                        
                        // always initialize with version shown from DB.
                        curVerStr = InitializeSQLiteVersionFromDB(appID, appName, LangToInt(dbLang), mobileDevice);
                        

                        if ( String.IsNullOrEmpty(curVerStr))
                        {
                            Log.ErrorLog(mapPathError, "No DB version for SQL Lite for " + dbLang + " of " + this.appName, this.appName);
                            return null;
                        }

                        if (addVersion == 0)    // means add MINOR version to SQLite
                        {                       // maybe needs a record in Admin DB that an minor update occured.

                            // already in a minor version!
                            if (curVerStr.Length > 2)
                            {
                                if (!getMajorMinorVer(curVerStr, out major, out minor))
                                {
                                    Log.ErrorLog(mapPathError, "MINOR ver update, Error occured while anlayzing minor-major DB ver numbers, for " + dbLang + " of " + this.appName + " major:" + major + " minor:" + minor + " curVersion: " + curVerStr, this.appName);
                                    return null;
                                }

                                minor++;
                                newVersionStr = setVerFromMajorMinor(major, minor);

                                
                               //  * resolve BUG to Android APK first.

                               // con.Open();
                               // cmd.CommandText = "PRAGMA USER_VERSION = " + newVersionStr + " ;";
                               // cmd.ExecuteNonQuery();
                               // con.Close();
                                 
                                 
                                
                                //add new version as record to DB
                                if (AddDBversionAdmin(appName, appID, curVerStr, newVersionStr, mobileDevice, dbLang)==null)
                                {
                                    Log.ErrorLog(mapPathError, "Error occured in AddDBversionAdmin DB ver numbers, for " + dbLang + " of " + this.appName, this.appName);
                                    return null;
                                }

                                Log.InfoLog(mapPathError, "DB to moved to MAJOR Version from MINOR: " + curVerStr + " to  :" + newVersionStr, this.appName);
                                return 1;

                            }
                            else // first minor add.
                            {
                                Int32.TryParse(curVerStr, out major);
                                newVersionStr = setVerFromMajorMinor(major, 1);

                              // resolve BUG to Android APK first.

                              //  con.Open();
                              //  cmd.CommandText = "PRAGMA USER_VERSION = " + newVersionStr + " ;";
                              //  cmd.ExecuteNonQuery();
                              //  con.Close();
                                
                                

                                if (AddDBversionAdmin(appName, appID, curVerStr, newVersionStr, mobileDevice, dbLang) == null)
                                {
                                    Log.ErrorLog(logPath, "Error occured in AddDBversionAdmin DB ver numbers, for " + dbLang + " of " + this.appName, this.appName);
                                    return null;
                                }

                                Log.InfoLog(mapPathError, "DB created first MINOR version: " + newVersionStr, this.appName);
                                return 2;

                            }
                        }
                        else// Means MAJOR add
                        {

                            // we are in a MINOR version.
                            if (curVerStr.Length > 2)
                            {
                                // get major minor versions.
                                if (!getMajorMinorVer(curVerStr, out major, out minor))
                                {
                                    Log.ErrorLog(mapPathError, "MAJOR ver update, Error occured while anlayzing minor-major DB ver numbers, for " + dbLang + " of " + this.appName + " major:" + major + " minor:" + minor + " curVersion: " + curVerStr, this.appName);
                                    return null;
                                }


                                if (addVersion > 1) 
                                    major = major + addVersion;
                                else 
                                    major++;

                                newVersionStr = major.ToString();
                                
                                // resolve BUG to Android APK first.

                                //cmd.CommandText = "PRAGMA USER_VERSION = " + newVersionStr + " ;";
                                //con.Open();
                                //cmd.ExecuteNonQuery();
                                //con.Close();
                                

                                if (AddDBversionAdmin(appName, appID, curVerStr, newVersionStr, mobileDevice, dbLang, MAJOR) == null)
                                {
                                    Log.ErrorLog(mapPathError, "Error occured in AddDBversionAdmin DB ver numbers, for " + dbLang + " of " + this.appName, this.appName);
                                    return null;
                                }
                                Log.InfoLog(mapPathError, "DB to moved to MAJOR Version from MINOR: " + major + "." + minor + " to  :" + newVersionStr, this.appName);
                                return 3;
                            }
                            else
                            {
                                Int32.TryParse(curVerStr, out curVersion);
                                curVersion = curVersion + addVersion;       // add plus one to existing version file.
                                newVersionStr = curVersion.ToString();
                                
                                //resolve BUG to Android APK first.

                                //con.Open();
                                //cmd.CommandText = "PRAGMA USER_VERSION = " + newVersionStr + " ;";
                                //cmd.ExecuteNonQuery();
                                //con.Close();
                                

                                if (AddDBversionAdmin(appName, appID, curVerStr, newVersionStr, mobileDevice, dbLang, MAJOR) == null)
                                {
                                    Log.ErrorLog(mapPathError, "Error occured in AddDBversionAdmin DB ver numbers, for " + dbLang + " of " + this.appName, this.appName);
                                    return null;
                                }
                                Log.InfoLog(mapPathError, "DB to moved to MAJOR Version from MAJOR: " + curVerStr + " to  :" + newVersionStr, this.appName);
                                return 4;
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorLog(mapPathError, "Some exception occured on UpdateSQLiteUserVersion: " + ex.Message, appName);
                return null;
            }

        }
        */

        /// <summary>
        /// Set new DB version to Admin, given App, Lang, and Mobile Device.
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appID"></param>
        /// <param name="curVerStr"></param>
        /// <param name="newVersionStr"></param>
        /// <param name="mobileDevice"></param>
        /// <param name="dbLang"></param>
        /// <param name="ver"></param>
        /// <returns></returns>
        public object AddDBversionAdmin(string appName, int appID, string curVerStr, string newVersionStr, string mobileDevice, string dbLang, int ver = MINOR)
        {
            int LangID = LangToInt(dbLang);
            int res;

            try
            {
                if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
                {
                    using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                    {
                        con.Open();

                        // Get initial values of versions from SQL server Admin.
                        using (SqlCommand command = new SqlCommand("usp_Set_DB_Version", con))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
                            command.Parameters.Add("@langID", SqlDbType.Int).Value = LangID;
                            command.Parameters.Add("@mobileDevice", SqlDbType.VarChar).Value = mobileDevice;
                            command.Parameters.Add("@curVersion", SqlDbType.VarChar).Value = curVerStr;
                            command.Parameters.Add("@newVersion", SqlDbType.VarChar).Value = newVersionStr;
                            command.Parameters.Add("@kind", SqlDbType.Int).Value = ver;
                            command.CommandTimeout = 2000;
                            res = (int)command.ExecuteScalar();
                            con.Close();
                            return res;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorLogAdmin(mapPathError, "Some excepetion occured in AddDBversionAdmin " + e.Message, appName);
                return null;
            }

            return null;

        }

         /// <summary>
         ///  Normailize major, minor to be store as string, add leading zeros
         /// </summary>
         /// <param name="major"></param>
         /// <param name="minor"></param>
         /// <returns></returns>
        private string setVerFromMajorMinor(int major, int minor)
        {
            if( major == null || minor == null)
                return null;
            
            string smajor = major.ToString();
            string sminor = minor.ToString();
            //dbVersion.PadLeft(myString.Length + 5, '0');

            if (smajor.Length == 1) smajor = "0" + smajor;
            if (sminor.Length == 1) sminor = "0" + sminor;

            return smajor + sminor;
        }

        /// <summary>
        /// Get two integers, major, minor as DB version, from a String. (2 first is MAJOR, 2 last is MINOR)
        /// </summary>
        /// <param name="curVerStr"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <returns></returns>
        private bool getMajorMinorVer(string curVerStr, out int major, out int minor)
        {
            major = 0;
            minor = 0;
            bool res = false;

            if (curVerStr != null)
            {
                if (curVerStr.Length == 3)
                {
                    res = Int32.TryParse(curVerStr.Substring(0, 1), out major);  // 403 , 4 major 03 minor
                    if (!res) major = -1;

                    res = Int32.TryParse(curVerStr.Substring(1, 2), out minor);  // 403 , 4 major 03 minor
                    if (!res) minor = -1;

                    return res; 
                }

                if (curVerStr.Length == 4)
                {
                    res = Int32.TryParse(curVerStr.Substring(0, 2), out major);  // 2433 , 24 major 33 minor
                    if (!res) major = -1;

                    res = Int32.TryParse(curVerStr.Substring(2, 2), out minor);  // 2453 , 24 major 53 minor
                    if (!res) minor = -1;

                    return res;
                }

            }

            return false;
        }


        /// <summary>
        /// Get the Latest Version found on Admin DB, set this DB version to SQLite.
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        /// <param name="dbLang"></param>
        /// <param name="mobileVersion"></param>
        /// <returns></returns>
        public string InitializeSQLiteVersionFromDB(int appID, string appName, int dbLang, string mobileVersion)
        {
            string db_Ver;

            // read DB version from admin table.
            if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
            {
                using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                {
                    con.Open();

                    // Get initial values of versions from SQL server Admin.
                    using (SqlCommand command = new SqlCommand("usp_Get_DB_version", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
                        command.Parameters.Add("@langID", SqlDbType.Int).Value = dbLang;
                        command.Parameters.Add("@mobileDevice", SqlDbType.VarChar).Value = mobileVersion;
                        command.CommandTimeout = 2000;
                        db_Ver = (string)command.ExecuteScalar();
                    }
                    con.Close();
                }

                // Do the work with DataTable
                if (String.IsNullOrEmpty(db_Ver))
                {
                    Log.InfoLog(mapPathError, "No App configuration found for " + appName, appName);

                    // try build new bundle details record on DB !
                    Finalize fin = new Finalize(appName, appID);
                    db_Ver = fin.UpdateAppBundleDataTable(mobileVersion, appName, appID, true);                 // create a record in DB and return new DB_Version number (always 1 !)
                    if (db_Ver == null)
                        return null;
                    fin = null;
                }

                return SetDBVersionForApp(db_Ver, mobileVersion, LangStr[dbLang]);
            }
            return null;
        }

        /// <summary>
        /// Set DB Version on SQLite DB, found on LOCAL path!
        /// </summary>
        /// <param name="db_version"></param>
        /// <param name="mobiledevice"></param>
        /// <param name="langID"></param>
        /// <returns></returns>
        private string SetDBVersionForApp(string db_version, string mobiledevice, string langID)
        {
            try
            {
                using (SQLiteConnection con = new SQLiteConnection("Data Source=" + mapPath + "Batch\\dbfiles\\" + mobiledevice + "\\GreekGuide_" + this.appName + "_" + langID + "_" + DateTime.Now.ToString("yyyyMMdd") + ".db; Version=3;"))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA USER_VERSION;", con))
                    {
                        con.Open();
                        /*
                         * resolve BUG to Android APK first.
                         *  
                         * cmd.CommandText = "PRAGMA USER_VERSION = " + db_version + " ;";
                         * cmd.ExecuteNonQuery();
                         * 
                         */
                        con.Close();
                        return db_version;
                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorLogAdmin(mapPathError, "Failed to initialize DB Version on SQLite DB for Lang:" + langID + " for " + mobiledevice + " Exception: " + e.Message, appName);
                return "0";
            }

        }


        ///// <summary>
        ///// Get all App Versions bundles produced for Specific App only
        ///// </summary>
        ///// <returns></returns>
        //public DataTable GetAllAppBundles(int appID, string mapPathError)
        //{
        //    try
        //    {
        //        if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
        //        {
        //            using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
        //            {
        //                using (SqlCommand cmd = new SqlCommand("usp_get_AllAppBundles", con))
        //                {
        //                    cmd.CommandType = CommandType.StoredProcedure;
        //                    cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
                    
        //                    con.Open();
                            
        //                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
        //                    DataTable dt = new DataTable();
        //                    adp.Fill(dt);

        //                    return dt;

        //                }
        //            }
                    
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.ErrorLog(mapPathError, e.Message, "generic", "");
        //        return null;
        //    }
        //    return null;

        //}




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
                        return UpdateAppBundleDataTable(mobileDevice, dirInfo.Name.ToLower());

                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorLogAdmin(mapPathError, "Exception on  InitializeDBFromFiles when initializing Backoffice DB." + ex.Message, "generic");
                return null;
            }

            return 0;
        }

        internal string UpdateAppBundleDataTable(string mobileDevice, string appName, int appID = 0, bool oneApp = false)
        {
            // scan based to DataTable of all apps, all folders ...
            DataTable dt = GetAllAppTable();

            //when found create a record on DB, if its not already craeted.
            string configVersionNumber;
            string DBVersionNumber;
            string appVersionNumber;
            string fileConfigVersionNumber;

            // get Configuration File and Version
            JObject configVerJSON = GetConfigurationFile(mobileDevice, out configVersionNumber, appName);

            // get DB Version File, DB Version, App version, configVersion.
            JObject dbVerJSON = GetVersionsFile(mobileDevice, out DBVersionNumber, out appVersionNumber, out fileConfigVersionNumber, appName);

            if (fileConfigVersionNumber != configVersionNumber)   // conflict to Configuration files!
            {
                Log.ErrorLogAdmin(mapPathError, "Conflict on configuration.txt and versions.txt -> Configuration_Version. Resolved by keeping configuration.txt.\n\t\t\t -> versions.txt:(" + configVersionNumber + ") " +
                                            "\n\t\t\t -> configuration.txt:(" + fileConfigVersionNumber + ") ", "generic");
                configVersionNumber = fileConfigVersionNumber;
            }

            // Create Detailed Bundle record for a non existing new app.
            if (oneApp && appID > 0)
            {
                if (UpdateAppBundle(mobileDevice, appName, appID, appVersionNumber, configVersionNumber, DBVersionNumber, configVerJSON, dbVerJSON) == null)
                    return null;
                return DBVersionNumber;
            }


            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    //dirInfo.Name.ToLower()

                    var results = from dsx in dt.AsEnumerable()
                                  where dsx.Field<string>("appName").ToLower().Trim() == appName.Trim()
                                  select dsx;

                    if (UpdateAppBundle(mobileDevice, ((string)results.CopyToDataTable().Rows[0][1]).ToLower(), ((int)results.CopyToDataTable().Rows[0][0]), appVersionNumber, configVersionNumber, DBVersionNumber, configVerJSON, dbVerJSON) == null)
                        return null;

                    return DBVersionNumber;
                }
            }

            return DBVersionNumber;
        }



        /// <summary>
        /// Add a new version either DB, App, Config to GGAppsBundleDetails in order to keep tracking of history changes.
        /// Data is collected from Production env.
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        /// <param name="mobileDevice"></param>
        /// <returns></returns>
        internal string AddProductionVerAdmin(int appID, string appName, string mobileDevice)
        {
            try
            {
                //when found create a record on DB, if its not already craeted.
                string configVersionNumber;
                string DBVersionNumber;
                string appVersionNumber;
                string fileConfigVersionNumber;

                // get Configuration File and Version
                JObject configVerJSON = GetConfigurationFileProduction(appName, appID, mobileDevice, out configVersionNumber);

                // get DB Version File, DB Version, App version, configVersion.
                JObject dbVerJSON = GetVersionsFileProduction(mobileDevice, out DBVersionNumber, out appVersionNumber, out fileConfigVersionNumber, appName);

                if (fileConfigVersionNumber != configVersionNumber && configVerJSON != null)   // conflict to Configuration files!
                {
                    Log.ErrorLogAdmin(mapPathError, "Conflict on configuration.txt and versions.txt -> Configuration_Version. Resolved by keeping configuration.txt.\n\t\t\t -> versions.txt:(" + configVersionNumber + ") " +
                                                "\n\t\t\t -> configuration.txt:(" + fileConfigVersionNumber + ") ", "generic");
                    fileConfigVersionNumber = configVersionNumber;
                }

                if (UpdateAppBundle(mobileDevice, appName, appID, appVersionNumber, fileConfigVersionNumber, DBVersionNumber, configVerJSON, dbVerJSON) == null)
                    return null;

                return DBVersionNumber;
            }
            catch (Exception ex)
            {
                Log.ErrorLogAdmin(mapPathError, "Exception on  AddProductionVerAdmin when initializing Backoffice DB." + ex.Message, "generic");
                return null;
            }

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
                Log.ErrorLogAdmin(mapPathError, "Exception thrown UpdateAppBundle: " + e.Message, "generic");
                return null;
            }
            

        }



        /// <summary>
        /// Store DB produced to history path, for other needs. Patch update and other needs.
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appID"></param>
        /// <param name="mobileDevice"></param>
        /// <param name="dbVersion"></param>
        /// <param name="toHistoryPath">Where to store in Staging</param>
        /// <returns></returns>
        internal object StoreNewDBtoHistory(string appName, int appID, string mobileDevice, string dbVersion, string toHistoryPath)
        {
            
            try
            {

                // copy existing DB to path update / appname / dbver / <ver_number> 
                if (!Directory.Exists(toHistoryPath + dbVersion))
                {
                    Directory.CreateDirectory(toHistoryPath + dbVersion);
                }

                if (File.Exists(producedAppPath + appName.ToLower() + "\\update\\" + mobileDevice + "\\ContentEN.db"))
                    File.Copy(producedAppPath + appName.ToLower() + "\\update\\" + mobileDevice + "\\ContentEN.db",
                              toHistoryPath + dbVersion + "\\ContentEN.db");

                if (File.Exists(producedAppPath + appName.ToLower() + "\\update\\" + mobileDevice + "\\ContentGR.db"))
                    File.Copy(producedAppPath + appName.ToLower() + "\\update\\" + mobileDevice + "\\ContentGR.db",
                        toHistoryPath + dbVersion + "\\ContentGR.db");


                if (File.Exists(producedAppPath + appName.ToLower() + "\\update\\" + mobileDevice + "\\ContentRU.db"))
                    File.Copy(producedAppPath + appName.ToLower() + "\\update\\" + mobileDevice + "\\ContentRU.db",
                        toHistoryPath + dbVersion + "\\ContentRU.db");

            }
            catch (Exception ex)
            {
                Log.ErrorLogAdmin(mapPathError, "Some exception occured in StoreNedDBtoHistory()", appName);
                return null;
            }
            return 0;
        }


        internal DataTable GetAllAppsVersionsLive()
        {
            string query = "select * from dbo.uvw_Get_All_Apps inner join GGAppsVersions on LOWER(GGAppsVersions.AppName) = LOWER(uvw_Get_All_Apps.Destination)";
            DataTable dt = new DataTable();
            if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
            {
                using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            return dt;
        }


        internal bool UpdateVersionsFilesProduction()
        {
            DateTime dt;
            foreach (DataRow dr in GetAllAppsVersionsLive().Rows) 
            { 
                // first check if results are really valid , puthon script is executing with no errors..
                DateTime.TryParse(dr["LastCheckDate"].ToString(), out dt);
                if (dt == DateTime.MinValue || dt < DateTime.Now.AddHours(-2)) 
                {
                    // send email to Admins to check for Python errors !
                    SendMailToUsers("generic"
                        , GetEmailList("ErrorTeam")
                        , null
                        , EmailTemplate("InitAdmin"
                                            , appName
                                            , DateTime.Now.ToString("HH:mm - ddd d MMM yyyy")
                                            , "http://app-update.greekguide.com/GGApps/Logs/Log_generic.txt"
                                       )
                        , "Initialization Process Failed "
                        , mapPathError, Log
                        );

                    return false;
                }

                // Destination	catCategoryId	GGAppsVersionsID	AppName	AppVersion	AppPlatform	HasChanged	LastModifiedDate	        LastCheckDate
                // Athens	    3	            1	                athens	3.0	        iOS	        1	        2015-03-13 12:50:56.623	    2015-04-03 10:51:23.617
                if (UpdateVerionsProductionLive(dr["AppVersion"].ToString()
                                                , dr["Destination"].ToString().ToLower()
                                                , Int32.Parse(dr["catCategoryId"].ToString())
                                                , dr["AppPlatform"].ToString().ToLower()) == null)
                {
                    Log.ErrorLogAdmin(mapPathError, "Some Error Occured while batch update App Versions - Config Versions for all Destinations!", "generic");
                    return false;
                }
            }
           
            return true;
        }

        
        // get the real app version produced from python script stored in 
        internal int GetRealAppVersion(string appName, int appID, string mobileDevice, out string appVersionReal)
        {
            string query = "select * from GGAppsVersions where AppName = '" + appName.ToLower() + "' and UPPER(AppPlatform) = '"+mobileDevice.ToUpper()+"' ";
            string q2 = "update GGAppsVersions set HasChanged = 0 where GGAppsVersionsID = ";
            int id = -1;
            appVersionReal = null;

            if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
            {
                using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        using (SqlDataReader resultSet = cmd.ExecuteReader())
                        {
                            if (resultSet.HasRows)
                            {
                                while (resultSet.Read())
                                {
                                    DateTime dt;
                                    DateTime.TryParse(resultSet["LastCheckDate"].ToString(), out dt);
                                    if( dt != null)
                                    {
                                        // is lastly checked successfully from python
                                        if (dt >= DateTime.Now.AddHours(-2)) //&& (int)resultSet["HasChanged"] == 1)
                                        {
                                            appVersionReal = resultSet["AppVersion"].ToString();        // this is the real version.
                                            id = Convert.ToInt32(resultSet["GGAppsVersionsID"]);

                                        }
                                        // oops, check python script for errors!!!
                                        else
                                        {
                                            return 0; 
                                        }
                                    }

                                    // GGAppsVersionsID	AppName	AppVersion	AppPlatform	HasChanged	LastModifiedDate	LastCheckDate

                                }
                                Log.InfoLog(mapPathError, "Finished reading query for images", appName);
                            }
                            else
                            {
                                Log.ErrorLog(mapPathError, "Some error occured while reading sql lite db for bundled images.", appName);
                                return -1;
                            }
                        }
                        con.Close();
                    }

                    // update db entry
                    if (appVersionReal != null)
                    {
                        if (id >= 0)
                            q2 += id;
                        else
                            return -1;

                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(q2, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        con.Close();
                        return 1;
                    }
                    
                }
            }

            return -1;
        }


    }
}