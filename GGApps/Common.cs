using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Hosting;
using System.IO;
using System.Text;
using System.Data.SqlClient;

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

        public bool HasErrors
        {
            get {
                if (HttpContext.Current.Session["hErrors"] == null)
                    return false;
                return (bool)HttpContext.Current.Session["hErrors"];
            }
            set { HttpContext.Current.Session["hErrors"] = value; }
        }

        public static CreateLogFiles Log = new CreateLogFiles();

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
                HttpContext.Current.Session["hErrors"] = true;
                Log.ErrorLog(mapPathError, "executeSQLScript for " + name + " lang:" + langID + " Exception:" + e.Message, name);
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
                    SqlCommand cmd = new SqlCommand("select catCategoryId as id , catName as appName from category  where catParentID = 2", con);
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adp.Fill(dt);

                    return dt;

                }
            }
            catch (Exception e)
            {
                HttpContext.Current.Session["hErrors"] = true;
                Log.ErrorLog(mapPathError, e.Message, "generic", "");
            }
            return null;

        }


        public DataTable GetAllAppBundles()
        {

            try
            {
                if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
                {
                    SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString());
                    SqlCommand cmd = new SqlCommand(@"select *
                                                        from GGAppsBundleDetails
                                                        inner join GGAppsBundle
                                                        on GGAppsBundle.GGAppsBundleID = GGAppsBundleDetails.GGAppsBundleID", con);

                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adp.Fill(dt);

                    return dt;

                }
            }
            catch (Exception e)
            {
                HttpContext.Current.Session["hErrors"] = true;
                Log.ErrorLog(mapPathError, e.Message, "generic", "");
            }
            return null;

        }


        public static double UploadFilesRemote(string appName, string localDir, string remotePath, bool overwrite = true)
        {
            Log.InfoLog(mapPath, "Started uploading files to FTP", appName);
            FTP ftpClient = null;
            long totalBytesUploaded = 0;
            if (rootWebConfig.AppSettings.Settings["FTP_Upload_ConStr"] != null)
            {
                var ftpConStr = rootWebConfig.AppSettings.Settings["FTP_Upload_ConStr"].Value.Split(new string[] { "|@@|" }, StringSplitOptions.None);
                if (ftpConStr.Length == 3)
                {
                    ftpClient = new FTP(ftpConStr[0], ftpConStr[1], ftpConStr[2], mapPathError, appName);
                }
            }


            if (ftpClient != null)
            {
                
                ftpClient.directoryListDetailed(remotePath);
                foreach (string localFile in Directory.GetFiles(localDir))
                {
                    FileInfo fi = new FileInfo(localFile);
                    totalBytesUploaded += ftpClient.upload(localFile, remotePath + fi.Name, overwrite);
                }
            }
            ftpClient = null;

            if( totalBytesUploaded > 10)
                Log.InfoLog(mapPath, "Finished with uploading files to FTP, total Bytes uploaded: " + FTP.SizeSuffix(totalBytesUploaded), appName);
            else
                Log.ErrorLog(mapPath, "Some error ocured while uploading files to FTP, total Bytes uploaded: " + FTP.SizeSuffix(totalBytesUploaded), appName);

            return totalBytesUploaded;

        }

    }
}
