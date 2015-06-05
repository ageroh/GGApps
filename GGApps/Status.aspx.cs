using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace GGApps
{
    public class Status : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod]
        public static bool GetPublishStatus(string appid, string appName, string publID1, string publID2)
        {
            int _appID = -1;
            int _publID1 = -1;
            int _publID2 = -1;

            Int32.TryParse(appid, out _appID);
            Int32.TryParse(publID1, out _publID1);
            Int32.TryParse(publID2, out _publID2);


            bool doneAll = false;

            if (_publID2 == -1 && _publID1 > 0)
            {

                int ck1 = CheckPublIsReady(_publID1);
                if (ck1 == 1)
                    // -- 1: done, 0: working -1: failed
                    doneAll = true;


            }
            else if (_publID2 > 0)
            {
                int ck1 = CheckPublIsReady(_publID1);
                int ck2 = CheckPublIsReady(_publID2);
                if (ck1 == 1 && ck2 == 1)
                    doneAll = true;
                // -- 1: done, 0: working -1: failed
            }


            if (doneAll)
            {
                // make a refresh to screen client side!
                return doneAll;
            }

            return doneAll;

        }


        private static int CheckPublIsReady(int publID1)
        {
            int res = -1;
            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
            {
                using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_Check_Publishing", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@GGAppsPublishID", SqlDbType.Int).Value = publID1;

                        con.Open();
                        res = (int)cmd.ExecuteScalar();
                        // -- 1: done, 0: working -1: failed


                    }
                }
            }
            return res;
        }

    }
}