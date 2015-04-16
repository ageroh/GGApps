using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Data.SqlClient;

namespace GGApps
{
    public partial class ContentValidation : Common
    {
        // contains all EntityIDs from DB.
        public DataTable FetchEntitiesValidationCacheOrDB(int appID, string lang, string timeperiod)
        {
            // if selected index is change then clear cache..

            DataTable dataTable = HttpContext.Current.Cache["DTEntitiesToValidate"] as DataTable;
            if (dataTable == null)
            {
                dataTable = GetAllEntitiesDB(appID, lang, timeperiod);
                HttpContext.Current.Cache["DTEntitiesToValidate"] = dataTable;
            }
            return dataTable;
        }



        public int ddDestination 
        { 
            get; 
            set; 
        }

        public static string lastEntityShown = "-1";

        private DataTable GetAllEntitiesDB(int appID, string lang, string timeperiod)
        {
            DataTable dt = new DataTable();

            if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
            {
                using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_FetchEntitiesValidation", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
                        cmd.Parameters.Add("@lang", SqlDbType.NVarChar).Value = lang;
                        cmd.Parameters.Add("@timeperiod", SqlDbType.NVarChar).Value = timeperiod;
                        con.Open();
                        SqlDataAdapter adp = new SqlDataAdapter(cmd);
                        adp.Fill(dt);
                        return dt;
                    }
                }
            }
            return null;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                DropDownList ddStart = (DropDownList)LoginViewImportant.FindControl("ddStart");

                ddStart.DataSource = GetAllAppTable();
                ddStart.DataTextField = "appName";
                ddStart.DataValueField = "id";

                ddStart.DataBind();
                ddStart.Items.Insert(0, new ListItem(" - Select Application - ", "-1"));
                ddStart.SelectedIndex = 0;


            }
        }

        protected void GetNextEntity_Click(object sender, EventArgs e)
        { 
           // check it in DB
           // insert record in hisotry table ! 
            
            DropDownList ddDest = (DropDownList)LoginViewImportant.FindControl("ddStart");
            DropDownList ddTP = (DropDownList)LoginViewImportant.FindControl("ddTimePeriod");
            DropDownList ddLA = (DropDownList)LoginViewImportant.FindControl("ddLang");

            if (ddDest.SelectedIndex > 0)
            {
                // fetch next record.
                string entID = FetchNextEntityID(FetchEntitiesValidationCacheOrDB(Int32.Parse(ddDest.SelectedValue), ddLA.SelectedValue, ddTP.SelectedValue));
                if (entID != null && entID != "finished")
                {
                    FetchRecord(Int32.Parse(entID));
                }
                else
                {
                    RefreshBtn.Visible = true; 
                    GetNextEntity.Visible = false;
                }
            }
        }


        protected void Goload_Click(object sender, EventArgs e)
        { 
            //                GetNextEntity 
            DropDownList ddStart = (DropDownList)LoginViewImportant.FindControl("ddStart");
            DropDownList ddTP = (DropDownList)LoginViewImportant.FindControl("ddTimePeriod");
            DropDownList ddLA = (DropDownList)LoginViewImportant.FindControl("ddLang");

            String strRet = "";
            // if destination is not null
            if (ddStart.SelectedIndex > 0)
            {
                Int32 appID = Int32.Parse(ddStart.SelectedValue);
                String appName = ddStart.SelectedItem.ToString();

                // run query to fetch entities from selected destination, language and period, in a session table
                FetchEntitiesValidationCacheOrDB(appID, ddLA.SelectedValue, ddTP.SelectedValue);

                // load content (title, description, editorial, tip) of the first one
                FetchRecord();

                // enable OK button
                GetNextEntity.Visible = true;
            }

        }


        protected void FetchRecord(int entityID = 0)
        {
            DropDownList ddDest = (DropDownList)LoginViewImportant.FindControl("ddStart");
            DropDownList ddTP = (DropDownList)LoginViewImportant.FindControl("ddTimePeriod");
            DropDownList ddLA = (DropDownList)LoginViewImportant.FindControl("ddLang");

            if (ddDest.SelectedIndex > 0)
            {
                DataTable dt = FetchEntitiesValidationCacheOrDB(Int32.Parse(ddDest.SelectedValue), ddLA.SelectedValue, ddTP.SelectedValue);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        if (entityID == 0)
                        {
                            int firstentityId = Convert.ToInt32(dt.Rows[0]["EntityID"]);
                            lastEntityShown = firstentityId.ToString();
                            DrawEntity(firstentityId, ddLA.SelectedValue, ddTP.SelectedValue);

                        }
                        else
                        {
                            DrawEntity(entityID, ddLA.SelectedValue, ddTP.SelectedValue);
                        }
                    }
                }
            }

        }

        private void DrawEntity(int entityID, string lang, string timePeriod)
        {

            if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
            {
                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_FetchSingleEntity", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@entityID", SqlDbType.Int).Value = entityID;
                        cmd.Parameters.Add("@lang", SqlDbType.NVarChar).Value = lang;
                        cmd.Parameters.Add("@timeperiod", SqlDbType.NVarChar).Value = timePeriod;
                        con.Open();
                        SqlDataAdapter adp = new SqlDataAdapter(cmd);
                        adp.Fill(dt);

                        if (dt != null)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                //var h1 = new HtmlGenericControl("h1");
                                //h1.InnerHtml 
                                string vTitle  = (string)dt.Rows[0]["Title"];
                                string vBody = (string)dt.Rows[0]["Body"];
                                string vTips = (string)dt.Rows[0]["Useful Tips"];
                                string vEditorial = (string)dt.Rows[0]["Editorial"];

                                // Initialize StringWriter instance.
                                System.IO.StringWriter stringWriter = new System.IO.StringWriter();

                                // Put HtmlTextWriter in using block because it needs to call Dispose.
                                using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
                                {
                                    // The important part:
                                    writer.RenderBeginTag(HtmlTextWriterTag.Div); // Begin #1
                                    writer.Write("<h1>" + vTitle + "</h1><hr>");
                                    writer.Write("<h2>Body</h2><hr><span>" + vBody + "</span>");
                                    writer.Write("<h2>Editorial</h2><hr><span>" + vEditorial + "</span>");
                                    writer.Write("<h2>Useful Tips</h2><hr><span>" + vTips + "</span>");
                                    writer.RenderEndTag();

                                    Control div = LoginViewImportant.FindControl("entityPlaceHolder");
                                    if (div != null)
                                    {
                                        LiteralControl ltCtrl = new LiteralControl();
                                        ltCtrl.Text = writer.InnerWriter.ToString();
                                        div.Controls.Add(ltCtrl);
                                    }

                                }

                            }
                        }

                    }
                }
            }
        }

        private void CheckEntity(int entityID, string lang, string timePeriod)
        { 
            
            if (rootWebConfig.AppSettings.Settings["GG_Reporting"] != null)
            {
                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(rootWebConfig.AppSettings.Settings["GG_Reporting"].Value.ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_CheckedSingleEntity", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@entityID", SqlDbType.Int).Value = entityID;
                        cmd.Parameters.Add("@lang", SqlDbType.NVarChar).Value = lang;
                        cmd.Parameters.Add("@timeperiod", SqlDbType.NVarChar).Value = timePeriod;
                        con.Open();
                        string res = (string)cmd.ExecuteScalar();
                        
                    }
                }
            }
        }

        private string FetchNextEntityID(DataTable dt)
        {
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count - 1; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        DataRow nextRow;
                        try
                        {
                            nextRow = dt.Rows[i + 1];
                        }
                        catch (Exception e)
                        {
                            return "finished";
                        }

                        if (dr["EntityID"].ToString() == lastEntityShown.ToString())
                        {
                            lastEntityShown = nextRow["EntityID"].ToString();
                            return nextRow["EntityID"].ToString();
                        }
                    }

                }

            }

            return null;
        }

    }
}