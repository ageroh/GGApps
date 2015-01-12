using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace GGApps
{
    public partial class Publish : Common
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                InitializeAppDD();
            }
        }

        protected void InitializeAppDD()
        {

            System.Web.UI.WebControls.DropDownList SelectApp = (System.Web.UI.WebControls.DropDownList)LoginViewImportant.FindControl("SelectApp");

            if (SelectApp != null)
            {
                SelectApp.DataSource = Common.GetAllAppTable();
                SelectApp.DataTextField = "appName";
                SelectApp.DataValueField = "id";
                
                SelectApp.DataBind();
                SelectApp.Items.Insert(0, new ListItem(" - Select Application - "));
                SelectApp.SelectedIndex = 0;
            }
        }

        protected DataSet GetData(int appID, string appName)
        {
            
            try
            {
                if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
                {
                    string conString = rootWebConfig.AppSettings.Settings["GG_Reporting"].Value;
                    using (SqlCommand cmd = new SqlCommand("usp_Get_Info_App", new SqlConnection(conString)))
                    {
                        using (SqlConnection con = new SqlConnection(conString))
                        {
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                cmd.Connection = con;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
                                
                                sda.SelectCommand = cmd;
                                using (DataSet ds = new DataSet())
                                {
                                    sda.Fill(ds);
                                    return ds;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorLog(mapPathError, "Some Exception occured in GetData(), " + e.Message, appName);
                return null;
            }
            return null;
        }

        protected void SelectApp_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList dropDown = sender as DropDownList;
            string appName = dropDown.SelectedItem.Text;
            int appID = -1;
            Int32.TryParse(dropDown.SelectedItem.Value, out appID);

            //store in session / force change if exists
            Session["appName"] = appName;
            Session["appID"] = appID;

            latestVersions.DataSource = GetData(appID, appName);
            latestVersions.DataBind();

            // Fetch details from Production for App
            FetchAppDetailsProduction(appID, appName);

        }

        private void FetchAppDetailsProduction(int appID, string appName)
        {
            throw new NotImplementedException();
        }

    }
}