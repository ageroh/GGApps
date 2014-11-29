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

namespace GGApps
{
    public partial class _Default : Page
    {

        public static StringBuilder sb = new StringBuilder();

        protected void Page_Load(object sender, EventArgs e)
        {
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
                ;
                // log this exception.
            }
            return null;

        }

        public void GoFirst_Click(object sender, EventArgs e)
        {
            DropDownList ddStart = (DropDownList)LoginViewImportant.FindControl("ddStart");

            if ( ! String.IsNullOrEmpty(ddStart.SelectedValue ))
                Refresh_DB(Int32.Parse(ddStart.SelectedValue), ddStart.SelectedItem.ToString());
            
        }



        void myConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            sb.AppendLine("</br>" + e.Message);
        }

        /* This is step 1 of batch process */
        public void Refresh_DB(int id, string name)
        {
            sb.Clear();

            sb.AppendLine("</br>Refresing for Lang: 1");
            sb.AppendLine("</br>");

            string connectionString = BuildDynamicConnectionStringForDB(id, name, 1);
             
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
                            Response.Write("</br>" + Convert.ToString(reader.GetValue(0)) + " = " + Convert.ToString(reader.GetValue(1)));
                        }

                    }
                }
            }

            sb.AppendLine("</br>");
            sb.AppendLine("</br>Refresing for Lang: 2");
            sb.AppendLine("</br>");

            connectionString = BuildDynamicConnectionStringForDB(id, name, 2);
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
                            Response.Write("</br>" + Convert.ToString(reader.GetValue(0)) + " = " + Convert.ToString(reader.GetValue(1)));
                        }

                    }

                }
            }

            Response.Write(sb.ToString());


        }



        private string BuildDynamicConnectionStringForDB(int id, string name, int lang)
        {
             System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
             if (rootWebConfig1.AppSettings.Settings["DynamicConnectionString"] != null)
             {
                string dbName = "ContentDB_165_Lan_"+lang.ToString()+"_Cat_"+id.ToString();
                string ConStr = rootWebConfig1.AppSettings.Settings["DynamicConnectionString"].Value;
                ConStr = ConStr.Replace("DBNAME", dbName);
                return ConStr;
             }

            return null;

        }

    }
}