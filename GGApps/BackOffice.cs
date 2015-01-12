using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GGApps
{
    public class BackOffice : Common
    {
        public BackOffice(string MPath, string mPathError) 
        {
            mapPath = MPath;
            mapPathError = mPathError;
            
            // always check for changes
            Initialize();
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


        /// <summary>
        /// Initialize for all Apps / Languages to keep updated Admin DB.
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appID"></param>
        /// <param name="path"></param>
        /// <param name="log"></param>
        /// <param name="filename"></param>
        public void InitializeAdminDB()
        {
            Finalize fin = new Finalize();

            if (fin.InitializeDBFromFiles("ios") == null)
                HasErrors = true;

            if( fin.InitializeDBFromFiles("android") == null)
                HasErrors = true;
        }

    }
}