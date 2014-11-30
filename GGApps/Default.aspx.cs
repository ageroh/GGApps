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


namespace GGApps
{
    public partial class _Default : Page
    {
        public static StringBuilder sb = new StringBuilder();
        public static String mapPathError = "";
        public static String[] otherLangApps;

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
        

        protected void Page_Load(object sender, EventArgs e)
        {
            mapPathError = MapPath("Logs/log_");

            if (!Page.IsPostBack)
            {
                if (User.Identity.IsAuthenticated)
                {
                    DropDownList ddStart = (DropDownList)LoginViewImportant.FindControl("ddStart");

                    ddStart.DataSource = FillDeptDropdownList();
                    ddStart.DataTextField = "appName";
                    ddStart.DataValueField = "id";
                    ddStart.Items.Insert(0, " - Select App - ");

                    ddStart.DataBind();
                    
                }
            
            }
        }

        private bool CheckThreeLanguages(int id)
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
                       if (x.Trim() == id.ToString() )
                       {
                           return true;
                       }
                   }
               }
            }

            return false;
        
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
                Log.ErrorLog(mapPathError, e.Message);
            }
            return null;

        }


        public void GoFirst_Click(object sender, EventArgs e)
        {
            DropDownList ddStart = (DropDownList)LoginViewImportant.FindControl("ddStart");
            sb.Clear();

            if (!String.IsNullOrEmpty(ddStart.SelectedValue))
            {
                Refresh_DB(Int32.Parse(ddStart.SelectedValue), ddStart.SelectedItem.ToString());

                // if no error continue.. ??


                RunReportTestsForProducedDBs(Int32.Parse(ddStart.SelectedValue), ddStart.SelectedItem.ToString());

            }
        }



        void myConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            sb.AppendLine(e.Message);
            Log.InfoLog(mapPathError, e.Message);
            
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
                Log.InfoLog(mapPathError, "Refresing Greek DB for " + name);
                Refresh_DB_inner(id, name, 1).ToString();

                // do for english
                Log.InfoLog(mapPathError, "Refresing English DB for " + name);
                Refresh_DB_inner(id, name, 2).ToString();

                // Do for Russian if needed.
                if (CheckThreeLanguages(id))
                {
                    Log.InfoLog(mapPathError, "Refresing Russian DB for " + name);
                    Refresh_DB_inner(id, name, 4).ToString();
                }

                string strHtml = sb.ToString();
                txtEditor.Text = strHtml;
                sb.Length = 0;
            }
            catch (Exception e)
            {
                Log.ErrorLog(mapPathError, e.Message + e.StackTrace);

            }
        }



        private StringBuilder Refresh_DB_inner(int id, string name, int langID)
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
                    command.CommandTimeout = 1000;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Log this information 
                            //Response.Write("</br>" + Convert.ToString(reader.GetValue(0)) + " = " + Convert.ToString(reader.GetValue(1)));
                            string str = Convert.ToString(reader.GetValue(0)) + " = " + Convert.ToString(reader.GetValue(1));
                            Log.InfoLog(mapPathError, str);
                            sb.AppendLine(str);
                        }

                    }
                }
            }

            return sb;
        
        }


        /// <summary>
        /// 2nd Step of Process
        /// Run all SQL queries against DB and show results on screen and log.
        /// </summary>
        /// <param name="id">The ID of the App</param>
        /// <param name="name">The name of the App</param>
        public void RunReportTestsForProducedDBs(int id,  string name)
        { 
            string path = MapPath("SQLScripts/");
            Literal txtEditor = (Literal)LoginViewImportant.FindControl("txtEditor1");

            sb.AppendLine("Real Report:\n\n");
            // Test 1 - entities without location 
            //ContentDB_165 db_test_2_1_entities_without_location.sql 
            sb.AppendLine(executeSQLScript(id, name, 0, path + "db_test_2_1_entities_without_location.sql", "ContentDB_165").ToString());
    
            //Test 1a - entities in multiple locations 
            //ContentDB_165_Lan_2_Cat_%1  db_test_2_1a_entities_in_multiple_locations.sql  
            sb.AppendLine(executeSQLScript(id, name, 2, path + "db_test_2_1a_entities_in_multiple_locations.sql").ToString()) ;

            // DONT FORGET CHECK FOR RUSSIAN !

            // Test 2 - entities connected to non leaves 
            //ContentDB_165_Lan_2_Cat_%1  db_test_2_2_entities_connected_to_non_leaves.sql 
            sb.AppendLine(executeSQLScript(id, name, 2, path + "db_test_2_2_entities_connected_to_non_leaves.sql").ToString());

            //Test 3 - entities without category 
            //ContentDB_165_Lan_2_Cat_%1  db_test_2_3_entities_without_category.sql 
            sb.AppendLine(executeSQLScript( id, name, 2, path + "db_test_2_3_entities_without_category.sql").ToString());

            // Test 4a - entities with invalid characters (GR) 
            //ContentDB_165_Lan_1_Cat_%1  db_test_2_4_entities_with_invalid_characters_OK.sql 
            sb.AppendLine(executeSQLScript( id, name, 1, path + "db_test_2_4_entities_with_invalid_characters.sql").ToString());

            // Test 4b - entities with invalid characters (EN) 
            // ContentDB_165_Lan_2_Cat_%1  db_test_2_4_entities_with_invalid_characters_OK.sql 
            sb.AppendLine(executeSQLScript( id, name, 2, path + "db_test_2_4_entities_with_invalid_characters.sql").ToString());
            
            // Test 5 - entities with greek characters in english 
            //ContentDB_165_Lan_2_Cat_%1  db_test_2_5_entities_with_greek_characters_in_english_OK.sql
            sb.AppendLine(executeSQLScript(id, name, 2, path + "db_test_2_5_entities_with_greek_characters_in_english.sql").ToString());

            txtEditor.Text = sb.ToString();
            // show messages after execution on screen
            // log report
            // sent mail ?
        }



        private string BuildDynamicConnectionStringForDB(int id, string name, int lang, string DBNameOver = null)
        {
             System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
             if (rootWebConfig1.AppSettings.Settings["DynamicConnectionString"] != null)
             {
                string dbName = "ContentDB_165_Lan_"+lang.ToString()+"_Cat_"+id.ToString();
                string ConStr = rootWebConfig1.AppSettings.Settings["DynamicConnectionString"].Value;

                if (!String.IsNullOrEmpty(DBNameOver))
                    dbName = DBNameOver;

                ConStr = ConStr.Replace("DBNAME", dbName);
                return ConStr;
             }

            return null;

        }


        private StringBuilder executeSQLScript(int id, string name, int langID, string sqlFile, string dbName = null)
        {
            FileInfo fileInfo = new FileInfo(sqlFile);
            string script = fileInfo.OpenText().ReadToEnd();
            StringBuilder sbSql = new StringBuilder();

            string connectionString = BuildDynamicConnectionStringForDB(id, name, langID, dbName);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.InfoMessage += new SqlInfoMessageEventHandler(myConnection_InfoMessage);
                con.Open();
                using (SqlCommand command = new SqlCommand(script, con))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 1000;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                sb.Append(Convert.ToString(reader.GetValue(i)));     
                            
                            }
                            sb.AppendLine();
                        }

                    }
                }
            }

            return sb;
            
        }


    }
}