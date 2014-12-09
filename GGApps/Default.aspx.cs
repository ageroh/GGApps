using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Text;
using System.IO;
using Microsoft.AspNet.Membership.OpenAuth;
using System.Web.Hosting;

namespace GGApps
{
    public partial class _Default : Page
    {
        #region Variables - Properties
        public static StringBuilder sb = new StringBuilder();
        public static String mapPathError = "";
        public static String[] otherLangApps;
        public static bool HasErrors = false;

        public static string actualWorkDir = HostingEnvironment.MapPath("/Batch/");
        // "C:\\Users\\Argiris\\Desktop\\GG_Batch\\Batch\\";    

        public static CreateLogFiles Log
        {
            get
            {
                if ((CreateLogFiles)HttpContext.Current.Session["Log"] == null)
                {
                    return new CreateLogFiles();
                }

                return (CreateLogFiles)HttpContext.Current.Session["Log"];
            }
            set 
            {
                HttpContext.Current.Session["Log"] = value;
            }
                
        }


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

        

        #endregion


        protected void Page_Load(object sender, EventArgs e)
        {
            Session["mapPathError"] = MapPath("Logs/log_");
            mapPathError = Session["mapPathError"].ToString();

            if (!Page.IsPostBack)
            {
                //if (User.Identity.IsAuthenticated)
                //{
                    DropDownList ddStart = (DropDownList)LoginViewImportant.FindControl("ddStart");

                    ddStart.DataSource = FillDeptDropdownList();
                    ddStart.DataTextField = "appName";
                    ddStart.DataValueField = "id";
                    ddStart.Items.Insert(0, " - Select App - ");

                    ddStart.DataBind();
                    
               // }
            
            }
        }



        
        public static DataTable FillDeptDropdownList()
        {
            System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            try
            {
                if (rootWebConfig1.AppSettings.Settings["ContentAbilityGG"] != null)
                {
                    SqlConnection con = new SqlConnection(rootWebConfig1.AppSettings.Settings["ContentAbilityGG"].Value.ToString());
                    SqlCommand cmd = new SqlCommand("select catCategoryId as id , catName as appName from category  where catParentID = 2", con);
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adp.Fill(dt);

                    return dt;

                }
            }
            catch (Exception e)
            {
                HasErrors = true;
                Log.ErrorLog(mapPathError, e.Message, "generic", "");
            }
            return null;

        }


        public void ContinueBtn_Click(object sender, EventArgs e)
        {
            ClientScriptManager cs = Page.ClientScript;

            if (Session["appID"] != null && Session["appName"] != null)
            {
                Session["HasErrors"] = HasErrors;

                if (Session["FinishedProcessing"] == null)
                { 
                    Session["FinishedProcessing"] = true;
                    Response.Redirect("~/BuildApp");
                }
                else if ((bool)Session["FinishedProcessing"] == false)
                {
                    // Processing must finish first!
                    ContinueBtn.Visible = false;
                    ContinueBtn.Enabled = false;
                    Response.Write("<h1 class='parallel'>Wait for the Previous Processing to be finished, in order to continue!</h1>");
                }
                else
                {

                    Response.Redirect("~/BuildApp");
                }
            }
            else
            {
                // Check to see if the startup script is already registered.
                if (!cs.IsStartupScriptRegistered(this.GetType(), "myalert"))
                {
                    String cstext1 = "alert('Please re-run the Report generation for the App.');";
                    cs.RegisterStartupScript(this.GetType(), "myalert", cstext1, true);
                }
            }
        
        }


        public void GoFirst_Click(object sender, EventArgs e)
        {
            DropDownList ddStart = (DropDownList)LoginViewImportant.FindControl("ddStart");
            sb.Clear();
            String strRet = "";



            if (!String.IsNullOrEmpty(ddStart.SelectedValue))
            {
                Int32 appID = Int32.Parse(ddStart.SelectedValue);
                String appName = ddStart.SelectedItem.ToString();

                Log.InfoLog(mapPathError, "Started : Building Report for " + appName, appName, "");

#if !DEBUG                
                Refresh_DB(appID, appName);
#endif
                // if no error continue.. ??
                strRet = RunReportTestsForProducedDBs(appID, appName);


                // Initialize StringWriter instance.
                StringWriter stringWriter = new StringWriter();

                // Put HtmlTextWriter in using block because it needs to call Dispose.
                using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
                {
                    // The important part:
                    writer.RenderBeginTag(HtmlTextWriterTag.Div); // Begin #1
                    writer.Write("<h1>Report for " + appName + "</h1><hr>" + strRet);
                    writer.RenderEndTag();
                    Control reportDiv = LoginViewImportant.FindControl("reportDiv");
                    if (reportDiv != null)
                    {
                        LiteralControl ltCtrl = new LiteralControl();
                        ltCtrl.Text = writer.InnerWriter.ToString();
                        reportDiv.Controls.Add(ltCtrl);

                    }
                        
                }

                // Make Continue button visible and clickable in order to continue 
                Button btn = (Button)LoginViewImportant.FindControl("ContinueBtn");
                btn.Enabled = true;
                btn.Visible = true;
                btn.Text = "Produce App for "+ ddStart.SelectedItem.ToString();
                Session["appID"] = appID;
                Session["appName"] = appName;

                Log.InfoLog(mapPathError, "Finished : Building Report for " + appName, appName, "");
            }
        }




        /// <summary>
        /// This is step 1 of batch process 
        /// Don't forget to add Check for 3rd Language (Russian - Halkidiki)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public void Refresh_DB(int id, string name)
        {
            Literal txtEditor = (Literal)LoginViewImportant.FindControl("txtEditor1");
            try
            {
                txtEditor.Text = "";
                sb.Clear();

                sb.AppendLine("\nRefresing DB for " + name);
                // do for greek
                Log.InfoLog(mapPathError, "Refresing Greek DB for " + name, name, "");
                Refresh_DB_inner(id, name, 1).ToString();

                // do for english
                Log.InfoLog(mapPathError, "Refresing English DB for " + name, name, "");
                Refresh_DB_inner(id, name, 2).ToString();

                // Do for Russian if needed.
                if (CheckThreeLanguages(id))
                {
                    Log.InfoLog(mapPathError, "Refresing Russian DB for " + name, name, "");
                    Refresh_DB_inner(id, name, 4).ToString();
                }

                string strHtml = sb.ToString();
                txtEditor.Text = strHtml;
                sb.Length = 0;
            }
            catch (Exception e)
            {
                HasErrors = true;
                Log.ErrorLog(mapPathError, e.Message +" Stack Trace:"  + e.StackTrace, name, "");

            }
        }



        private StringBuilder Refresh_DB_inner(int id, string name, int langID)
        {
            try
            {
                sb.AppendLine("\nRefresing for Lang: " + langID);

                string connectionString = BuildDynamicConnectionStringForDB(id, name, langID);

                using (SqlConnection con = new SqlConnection(connectionString))
                {

                    con.InfoMessage += new SqlInfoMessageEventHandler(myConnection_InfoMessage);
                    con.Open();
                    using (SqlCommand command = new SqlCommand("_pr_refresh_db", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 2000;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Log this information 
                                //Response.Write("</br>" + Convert.ToString(reader.GetValue(0)) + " = " + Convert.ToString(reader.GetValue(1)));
                                string str = Convert.ToString(reader.GetValue(0)) + " = " + Convert.ToString(reader.GetValue(1));
                                Log.InfoLog(mapPathError, str, name, "");
                                sb.AppendLine(str);
                            }

                        }
                    }

                    con.Close();
                }
            }
            catch (SqlException ex)
            {
                HasErrors = true;
                Log.ErrorLog(mapPathError, "Refresh_DB_inner for " + name + " lang:" + langID + " Exception:" + ex.Message, name);
            }
            
            return sb;
        
        }


        /// <summary>
        /// 2nd Step of Process
        /// Run all SQL queries against DB and show results on screen and log.
        /// </summary>
        /// <param name="id">The ID of the App</param>
        /// <param name="name">The name of the App</param>
        public String RunReportTestsForProducedDBs(int id,  string name)
        { 
            string path = MapPath("SQLScripts/");
            //Literal txtEditor = (Literal)LoginViewImportant.FindControl("txtEditor1");
            sb.Length = 0;
            sb.Clear();

            // Test 1 - entities without location 
            sb.AppendLine("<h3>Test 1 - entities without location</h3>\n");
            Log.InfoLog(mapPathError, "Exporting Report stage 1", name, "");
            sb.AppendLine(executeSQLScript(id, name, 0, path + "db_test_2_1_entities_without_location.sql", "ContentDB_165"));
    
            //Test 1a - entities in multiple locations 
            sb.AppendLine("<h3>Test 1a - entities in multiple locations</h3>\n");
            Log.InfoLog(mapPathError, "Exporting Report stage 2_1a", name, "");
            sb.AppendLine(executeSQLScript(id, name, 2, path + "db_test_2_1a_entities_in_multiple_locations.sql")) ;

            // DONT FORGET CHECK FOR RUSSIAN !

            // Test 2 - entities connected to non leaves 
            sb.AppendLine("<h3>Test 2 - entities connected to non leaves</h3>\n");
            Log.InfoLog(mapPathError, "Exporting Report stage 2", name, "");
            sb.AppendLine(executeSQLScript(id, name, 2, path + "db_test_2_2_entities_connected_to_non_leaves.sql"));

            //Test 3 - entities without category 
            sb.AppendLine("<h3>Test 3 - entities without category</h3>\n");
            Log.InfoLog(mapPathError, "Exporting Report stage 3", name, "");
            sb.AppendLine(executeSQLScript( id, name, 2, path + "db_test_2_3_entities_without_category.sql"));

            // Test 4a - entities with invalid characters (GR) 
            sb.AppendLine("<h3>Test 4a - entities with invalid characters (GR)</h3>\n");
            Log.InfoLog(mapPathError, "Exporting Report stage 4a", name, "");
            sb.AppendLine(executeSQLScript( id, name, 1, path + "db_test_2_4_entities_with_invalid_characters.sql"));

            // Test 4b - entities with invalid characters (EN) 
            sb.AppendLine("<h3>Test 4b - entities with invalid characters (EN)</h3>\n");
            Log.InfoLog(mapPathError, "Exporting Report stage 4b", name, User.Identity.Name);
            sb.AppendLine(executeSQLScript( id, name, 2, path + "db_test_2_4_entities_with_invalid_characters.sql"));
            
            // Test 5 - entities with greek characters in english 
            sb.AppendLine("<h3>Test 5 - entities with greek characters in english</h3>\n");
            Log.InfoLog(mapPathError, "Exporting Report stage 5", name, User.Identity.Name);
            sb.AppendLine(executeSQLScript(id, name, 2, path + "db_test_2_5_entities_with_greek_characters_in_english.sql"));


            // Save report to File
            string filepath;
            if (!File.Exists(actualWorkDir + "reports\\" + name + "_report_" + DateTime.Now.ToString("yyyyMMdd") + ".html"))
            {
                filepath = actualWorkDir + "reports\\" + name + "_report_" + DateTime.Now.ToString("yyyyMMdd") + ".html";

                File.WriteAllText(filepath , sb.ToString(), Encoding.UTF8);
            }
            else {
                filepath = actualWorkDir + "reports\\" + name + "_report_" + DateTime.Now.ToString("yyyyMMdd") + "_" + (timesExec++).ToString() + ".html";
                File.WriteAllText(filepath, sb.ToString(), Encoding.UTF8);
            }


            //
            Log.InfoLog(mapPathError, "Report for App: " + name + " Succesfully Generated and saved to:" + filepath, name, User.Identity.Name);
             
            // sent mail ?



            return sb.ToString();
            // show messages after execution on screen
            
        }




        public static String executeSQLScript(int id, string name, int langID, string sqlFile, string dbName = null)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(sqlFile);
                string script = fileInfo.OpenText().ReadToEnd();
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
                HasErrors = true;
                Log.ErrorLog(mapPathError, "executeSQLScript for " + name + " lang:" + langID + " Exception:" + e.Message, name);
            }

            return "";
        }



        #region Helpers

        void myConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            try
            {
                sb.AppendLine(e.Message);
                //Log.InfoLog(mapPathError, e.Message, Session["appName"].ToString(), "");
            }
            catch (Exception ex)
            {
                HasErrors = true;
                //Log.ErrorLog(mapPathError, "myConnection_InfoMessage: " + ex.Message, Session["appName"].ToString(), "");
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
                    if (dt.Columns[j].ColumnName.ToLower() == "ententityid")
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
            System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            String threelang = rootWebConfig1.AppSettings.Settings["ThreeLanguages"].Value;
            if (threelang != null)
            {
                otherLangApps = threelang.Split('|');
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

        private static string BuildDynamicConnectionStringForDB(int id, string name, int lang, string DBNameOver = null)
        {
            System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            if (rootWebConfig1.AppSettings.Settings["DynamicConnectionString"] != null)
            {
                string dbName = "ContentDB_165_Lan_" + lang.ToString() + "_Cat_" + id.ToString();
                string ConStr = rootWebConfig1.AppSettings.Settings["DynamicConnectionString"].Value;

                if (!String.IsNullOrEmpty(DBNameOver))
                    dbName = DBNameOver;

                ConStr = ConStr.Replace("DBNAME", dbName);
                return ConStr;
            }

            return null;

        }
        #endregion


    }
}