using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft;
using Newtonsoft.Json.Linq;

namespace RefreshVersionsFiles
{
    class Program
    {
        //public static string cf = ConfigurationManager.AppSettings[""];

        public static string actualWorkDir = "C:\\temp\\";

        public static string producedAppPath = "C:\\GGAppContent\\";

       
        static void Main(string[] args)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(actualWorkDir + "log.txt", true);

            sw.AutoFlush = true;

            Console.SetOut(sw);
            Console.WriteLine("Logging time :>> " + DateTime.Now.ToString("ddMMyyyy hh:mm:ss"));
            Console.WriteLine();
            bool updated;
            if (!UpdateVersionsFilesProduction(out updated))
                Console.WriteLine("Some error occured!");

            if (!updated)
                Console.WriteLine("Nothing updated. All ok.");
        }



        public static bool UpdateVersionsFilesProduction(out bool updated)
        {
            DateTime dt;
            updated = false;
            foreach (DataRow dr in GetAllAppsVersionsLive().Rows)
            {
                // first check if results are really valid , puthon script is executing with no errors..
                DateTime.TryParse(dr["LastCheckDate"].ToString(), out dt);
                if (dt == DateTime.MinValue || dt < DateTime.Now.AddHours(-48))
                {
                    // send email to Admins to check for Python errors !
                    Console.WriteLine("Python app Versions is not synched!");
                    return false;
                }


                if (dr["HasChanged"].ToString() == "True")
                {
                    // Destination	catCategoryId	GGAppsVersionsID	AppName	AppVersion	AppPlatform	HasChanged	LastModifiedDate	        LastCheckDate
                    // Athens	    3	            1	                athens	3.0	        iOS	        1	        2015-03-13 12:50:56.623	    2015-04-03 10:51:23.617
                    if (UpdateVerionsProductionLive(dr["AppVersion"].ToString()
                                                    , dr["Destination"].ToString().ToLower()
                                                    , Int32.Parse(dr["catCategoryId"].ToString())
                                                    , dr["AppPlatform"].ToString().ToLower()) == null)
                    {
                        Console.WriteLine("Some Error Occured while batch update App Versions - Config Versions for all Destinations!");
                        return false;
                    }

                    if (UpdateAppVersion(Convert.ToInt32(dr["GGAppsVersionsID"])) < 0)
                        return false;

                    updated = true;
                }
            }

            return true;
        }

        // id = Convert.ToInt32(resultSet["GGAppsVersionsID"]);
        public static  int UpdateAppVersion(int GGAppsVersionsID)
        {
            string q2 = "update GGAppsVersions set HasChanged = 0 where GGAppsVersionsID = ";


            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["GG_Reporting"].ToString()))
            {
                if (GGAppsVersionsID >= 0)
                    q2 += GGAppsVersionsID;
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
            
            return -1;
        }




        public  static DataTable GetAllAppsVersionsLive()
        {
            string query = "select * from dbo.uvw_Get_All_Apps inner join GGAppsVersions on LOWER(GGAppsVersions.AppName) = LOWER(uvw_Get_All_Apps.Destination) and isActive = 1 order by Destination ";
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["GG_Reporting"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            
            return dt;
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
        public static object UpdateVerionsProductionLive(string realAppVersion, string appName, int appId, string mobileDevice)
        {
            string configVersionNumber;
            string DBVersionNumber;
            string appVersionNumber;
            string fileConfigVersionNumber;
            bool needsUpdate = false;
            string nVersionTXT = "";

            // get Configuration File and Version
            //JObject configVerJSON = GetConfigurationFileProduction(appName, appId, mobileDevice, out configVersionNumber);

            // get DB Version File, DB Version, App version, configVersion.
            JObject dbVerJSON = GetVersionsFileProduction(mobileDevice, out DBVersionNumber, out appVersionNumber, out fileConfigVersionNumber, appName);
            if (dbVerJSON == null)
                return null;

            /*
             * Dont update Config Version from Configuration.txt yet.
             * 
            if( configVerJSON != null )
                if (fileConfigVersionNumber != configVersionNumber)   // conflict to Configuration files!
                {
                    fileConfigVersionNumber = configVersionNumber;
                    needsUpdate = true;
                }
             * */

            if (realAppVersion != appVersionNumber)
                needsUpdate = true;

            if (needsUpdate)
            {
                // then really needs to change !
                nVersionTXT = "{  \"app_version\": \"" + realAppVersion + "\",   \"config_version\": \"" + fileConfigVersionNumber + "\",   \"db_version\": \"" + DBVersionNumber + "\" }";

                // upload to server
                string res = SaveConfigureFile(mobileDevice, appName, appId, "Production", "versions.txt", nVersionTXT);
                if (res == null)
                    return null;    // error

                Console.WriteLine(appName + "::" + mobileDevice + ":: New App Version - Configuration found: New App_Version = " + realAppVersion + " New Configuration_Version = " + fileConfigVersionNumber);
            }

            return nVersionTXT;


        }





        public static JObject GetConfigurationFileProduction(string appName, int appID, string mobileDevice, out string configVersion)
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
                Console.WriteLine( "Some exception occured in GetConfigurationFileProduction(), " + e.Message + " : " + appName);
                return null;

            }

        }




        protected static string GetCofigureFile(string mobileDevice, string appName, int appID, string Environment, string fileName)
        {
            string localFilename = actualWorkDir + "\\getTempfilename.txt";
            string getFileName;

            // Clear previous file.
            if (File.Exists(localFilename))
                File.Delete(localFilename);


            if (Environment == "Production")
            {
                getFileName = appName.ToLower() + "//update//" + mobileDevice + "//" + fileName;

                if (DownloadProductionFile(appName, getFileName, localFilename) < 0)
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





        protected static string SaveConfigureFile(string mobileDevice, string appName, int appID, string Environment, string fileName, string fileContents)
        {

            try
            {
                // check JSON , save with intended!
                fileContents = JsonConvert.DeserializeObject(fileContents).ToString();

            }
            catch (Exception je)
            {
                Console.WriteLine( appName + ": not valid json for " + fileName + ", " + je.Message);
                return "Not A Valid JSON file.";
            }


            string localFilename = actualWorkDir + "\\tempVersions_" + System.Guid.NewGuid().ToString() + ".txt";
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
                        {
                            File.Delete(localFilename);
                            return null;
                        }
                        else
                            return fileName;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(appName + ": Some Critical Exception occured when try to save file: " + setFileName + ", " + appName + ", " + mobileDevice + ", " + Environment + "  , " + e.Message);
                        File.Delete(localFilename);
                        return null;
                    }
                }
                else
                {
                    return "";
                }
            }

            return "";
           
        }


        public static JObject GetVersionsFileProduction(string mobileDevice, out string dbVersion, out string appVersion, out string configVersion, string appName, string versionPublish = null)
        {
            dbVersion = null;           // something is wrong !
            configVersion = null;       // when return should ALWAYS have values..
            appVersion = null;          // when return should ALWAYS have values..
            string versFilename = "versions.txt";
            if (versionPublish != null)
                versFilename = versionPublish;


            string localFilename = actualWorkDir + "\\tempVersions_" + System.Guid.NewGuid().ToString() + ".txt";
            string remotefilename = appName.ToLower() + "//update//" + mobileDevice + "//" + versFilename;

            if (DownloadProductionFile(appName, remotefilename, localFilename) < 0)
            {
                dbVersion = "NaN";
                configVersion = "NaN";
                appVersion = "NaN";
                return null;
            }

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

            if (File.Exists(localFilename))
                File.Delete(localFilename);

            if (ok)
            {
                return o2;       // All good, app_version and db_version found.
            }

            return null;


        }


        private static FTP CreateFTPClientProduction(string appName)
        {
            FTP ftpClient = null;


            if (ConfigurationManager.AppSettings["FTP_Upload_ConStr"] != null)
            {
                var ftpConStr = ConfigurationManager.AppSettings["FTP_Upload_ConStr"].Split(new string[] { "|@@|" }, StringSplitOptions.None);
                if (ftpConStr.Length == 3)
                {
                    ftpClient = new FTP(ftpConStr[0], ftpConStr[1], ftpConStr[2], "", appName);
                }
            }
            return ftpClient;
        }


        private static long DownloadProductionFile(string appName, string remotefilename, string localFilename)
        {
            Console.WriteLine(appName + ": Try get Production File " + remotefilename + " via FTP");
            FTP ftpClient = CreateFTPClientProduction(appName);

            try
            {
                //   try get file from production and check it.
                return ftpClient.download(remotefilename, localFilename);

            }
            catch (Exception ex)
            {
                Console.WriteLine(appName + ": File did not found in PRODUCTION: " + remotefilename);
                return -3;
            }
            finally
            {
                ftpClient = null;
            }
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
                    else
                    {
                        ftpClient = null;
                        return totalBytesUploaded;
                    }
                }

                if (totalBytesUploaded <= 0)
                    Console.WriteLine("Some error ocured while uploading file: " + localFilename + " to FTP, total Bytes uploaded: " + ftpClient.SizeSuffix(totalBytesUploaded));

                ftpClient = null;
                return totalBytesUploaded;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Some error ocured while uploading files to FTP, Exception:  " + ex.Message);
                return -1;
            }
        }



    }
}
