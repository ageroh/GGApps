using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GGApps
{
    public class BackOffice : CommonAdmin
    {
        public BackOffice(string MPath, string mPathError) 
        {
            MapPath = MPath;
            mapPathError = mPathError;
            
            // always check for changes
            Initialize();
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
            }
            catch (Exception e)
            {
                Log.ErrorLog(mapPathError, "Exception thrown UpdateAppBundle: " + e.Message, "generic");
            }
            return null;

        }


        public static void Initialize()
        {
            // check if new app is created, and add a record on GGAppsBundle
            CheckAppsBundleList();

            // check other concerns the Backoffice.
        
        }

        private static void CheckAppsBundleList()
        {
            if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
            {
                using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_Check_App_Bundle", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@appLanguages", SqlDbType.VarChar).Value = rootWebConfig.AppSettings.Settings["ThreeLanguages"].Value.ToString();
                        con.Open();
                        string res = ((string)cmd.ExecuteScalar());

                        // Log a message maybe or not
                    }
                }

            }
            
        }




  





        // always insert record if not exits !

        // update record Configuration file only ?

    }
}