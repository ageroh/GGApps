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

        public static int UpadteAppsBundleList()
        {
            try
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
                            int res =((int)cmd.ExecuteScalar());
                            con.Close();
                            return res;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorLogAdmin(mapPathError, "some exception occured on CheckAppsBundleList(), ", e.Message, "generic");
            }

            return 0;
        }


        /// <summary>
        /// Initialize for all Apps / Languages to keep updated Admin DB.
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appID"></param>
        /// <param name="path"></param>
        /// <param name="log"></param>
        /// <param name="filename"></param>
        public static object InitializeAdminDB()
        {
            Finalize fin = new Finalize();

            if (fin.InitializeDBFromFiles("ios") == null)
                return null;

            if( fin.InitializeDBFromFiles("android") == null)
                return null;

            return 0;
        }

    }
}