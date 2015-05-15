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
        #region PROPERTIES - SESSIONS 

        public enum Checking { Start = 0, Finish = 1 };

        // contains all EntityIDs from DB.
        public DataTable FetchEntitiesValidationCacheOrDB(int appID, string lang, string timeperiod, bool clearCache=false)
        {
            DataTable dataTable;
            if (clearCache)
                HttpContext.Current.Cache.Remove("DTEntitiesToValidate");

            // if selected index is change then clear cache..
            if (ddDestination == appID)
            {
                dataTable = HttpContext.Current.Cache["DTEntitiesToValidate"] as DataTable;
                if (dataTable == null)
                {
                    dataTable = GetAllEntitiesDB(appID, lang, timeperiod, GetUserID());
                    HttpContext.Current.Cache["DTEntitiesToValidate"] = dataTable;
                }
            }
            else
            {
                ddDestination = appID;
                dataTable = GetAllEntitiesDB(appID, lang, timeperiod, GetUserID());
                HttpContext.Current.Cache["DTEntitiesToValidate"] = dataTable;
            }
            return dataTable;
        }

        public int ddDestination
        {
            get
            {
                if (Session["ddDestination"] == null)
                { 
                    DropDownList ddDest = (DropDownList)LoginViewImportant.FindControl("ddStart");
                    Session["ddDestination"] = Int32.Parse(ddDest.SelectedValue);
                }
                return (int)Session["ddDestination"];
            }
            set {
                Session["ddDestination"] = value;
            }
        }



        public int currentEntityID
        {
            get
            {
                if (Session["currentEntityID"] == null)
                    return -1;
                return (int)Session["currentEntityID"];
            }
            set
            {
                Session["currentEntityID"] = value;
            }
        }

        public string lastEntityShown
        {
            get
            {
                return (string)Session["lastEntityShown"];
            }
            set
            {
                Session["lastEntityShown"] = value;
            }
        }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
         
            if (!Page.IsPostBack)
            {
                ClearSessionSelected();     // ??

                lastEntityShown = "-1";
                DropDownList ddStart = (DropDownList)LoginViewImportant.FindControl("ddStart");

                ddStart.DataSource = GetAllAppTable();
                ddStart.DataTextField = "appName";
                ddStart.DataValueField = "id";

                ddStart.DataBind();
                ddStart.Items.Insert(0, new ListItem(" - Select Application - ", "-1"));
                ddStart.SelectedIndex = 0;

            }
        }


        private DataTable GetAllEntitiesDB(int appID, string lang, string timeperiod, int userID)
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
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
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

        protected void continueBtn_Click(object sender, EventArgs e)
        {
            goNextTbl.Visible = false;
            this.GetNextEntity.Visible = true;
            this.editEntiBtn.Visible = true;
            this.refreshEntityBtn.Visible = true;
            GetNextEntityCon();
        }


        protected void stopProcBtn_Click(object sender, EventArgs e)
        {
            ClearSessionSelected();
            Response.Redirect("~/ContentValidation.aspx");
        }



        protected void GetNextEntity_Click(object sender, EventArgs e)
        {   
            int userID = GetUserID();

            if ( (int)Session["ddDestSelected"] > 0)
            {
                // add a record to db for user Robot, that the entity is Valid and its content is ok.
                CheckEntity(currentEntityID, Session["ddLASelected"].ToString(), Session["ddTPSelected"].ToString(), (int)Checking.Finish, userID);
            }
            
            this.goNextTbl.Visible = true;
            this.GetNextEntity.Visible = false;
            this.editEntiBtn.Visible = false;
            this.refreshEntityBtn.Visible = false;

        }

        private int GetUserID()
        {
            if (User.Identity.IsAuthenticated)
            {
                string user = HttpContext.Current.User.Identity.Name;
                if( ! String.IsNullOrEmpty(user) )
                    if (user.Contains("ggOmaira") )
                        return 32;   // 32	omaira
                    if(user.Contains("ggdemo"))
                        return 38;  // demouser
            }
            return -1;
        }

        public void GetNextEntityCon()
        {
            goNextTbl.Visible = false;

            if ( (int)Session["ddDestSelected"] > 0)
            {
            
                // fetch next record.
                string entID = FetchNextEntityID(FetchEntitiesValidationCacheOrDB( (int)Session["ddDestSelected"], Session["ddLASelected"].ToString(),  Session["ddTPSelected"].ToString()));
                if (entID != null && entID != "finished")
                {
                    // if next record really exists
                    FetchRecord(Int32.Parse(entID));
                }
                else
                {
                    ClearSessionSelected();
                    RestartBtn.Visible = true;
                    GetNextEntity.Visible = false;
                    editEntiBtn.Visible = false;
                    refreshEntityBtn.Visible = false;
                }
            }
        }


        protected void refreshEntityBtn_Click(object sender, EventArgs e)
        { 
            // refresh the current entity 
            FetchRecord(currentEntityID);
        }

        protected void ClearSessionSelected()
        {
            Session["ddDestSelected"] = null;
            Session["ddLASelected"] = null;
            Session["ddTPSelected"] = null;
            Session["appName"] = null;
        }


        protected void Goload_Click(object sender, EventArgs e)
        { 
            DropDownList ddStart = (DropDownList)LoginViewImportant.FindControl("ddStart");
            DropDownList ddTP = (DropDownList)LoginViewImportant.FindControl("ddTimePeriod");
            DropDownList ddLA = (DropDownList)LoginViewImportant.FindControl("ddLang");

            Session["ddDestSelected"] = Int32.Parse(ddStart.SelectedValue);
            Session["ddLASelected"] = ddLA.SelectedValue;
            Session["ddTPSelected"] = ddTP.SelectedValue;
            Session["appName"] = ddStart.SelectedItem.ToString();

            String strRet = "";
            // if destination is not null
            if (ddStart.SelectedIndex > 0)
            {
                Int32 appID = Int32.Parse(ddStart.SelectedValue);
               
                // run query to fetch entities from selected destination, language and period, in a session table
                FetchEntitiesValidationCacheOrDB(appID, ddLA.SelectedValue, ddTP.SelectedValue, true);

                // load content (title, description, editorial, tip) of the first one
                FetchRecord();
                
            }

        }


        protected void FetchRecord(int entityID = 0)
        {

            if ( (int)Session["ddDestSelected"] > 0)
            {
                DataTable dt = FetchEntitiesValidationCacheOrDB((int)Session["ddDestSelected"], Session["ddLASelected"].ToString(), Session["ddTPSelected"].ToString() );

                if (dt != null)
                {
                    

                    if (dt.Rows.Count > 0)
                    {
                        destLangTimeTable.Visible = false;
                        AllValidated.Visible = false;

                        if (entityID == 0)
                        {
                            int firstentityId = Convert.ToInt32(dt.Rows[0]["EntityID"]);
                            lastEntityShown = firstentityId.ToString();
                            currentEntityID = firstentityId;
                            DrawEntity(firstentityId, Session["ddLASelected"].ToString() , Session["ddTPSelected"].ToString(), GetUserID());

                        }
                        else
                        {
                            currentEntityID = entityID;
                            DrawEntity(entityID, Session["ddLASelected"].ToString(), Session["ddTPSelected"].ToString(), GetUserID());
                        }
                    }
                    else
                    {
                        GetNextEntity.Visible = false;
                        editEntiBtn.Visible = false;
                        refreshEntityBtn.Visible = false;
                        destLangTimeTable.Visible = false;
                        AllValidated.Text = "All Entities for " + Session["appName"]  + " in " + Session["ddLASelected"].ToString()  + " of the last " + Session["ddTPSelected"].ToString()  + ", are succesfully validated recently!";
                        AllValidated.Visible = true;
                    }
                }
            }

        }

        private void DrawEntity(int entityID, string lang, string timePeriod, int userID)
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

                                    if (!String.IsNullOrEmpty(vTitle))
                                    {
                                        writer.Write("<h1>" + vTitle + "</h1>");
                                        writer.Write("<span>" + vBody + "</span>");
                                        if (!String.IsNullOrEmpty(vEditorial))
                                            writer.Write("<h3>Editorial</h3><span>" + vEditorial + "</span>");
                                        if (!String.IsNullOrEmpty(vTips))
                                            writer.Write("<h3>Useful Tips</h3><span>" + vTips + "</span>");
                                        GetNextEntity.Visible = true;
                                        editEntiBtn.Visible = true;
                                        refreshEntityBtn.Visible = true;

                                        // Start counting on the begging of checking of one entity!
                                        CheckEntity(entityID, lang, timePeriod, (int)Checking.Start, userID);
                                    }
                                    else
                                    {
                                        writer.Write("<h2>Please check your previous selection, you should maybe change language..</h2>");
                                        GetNextEntity.Visible = false;
                                        editEntiBtn.Visible = false;
                                        refreshEntityBtn.Visible = false;
                                    }

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

        private void CheckEntity(int entityID, string lang, string timePeriod, int startend, int userID)
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
                        cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                        cmd.Parameters.Add("@Checking", SqlDbType.Int).Value = startend;


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

                        if (lastEntityShown == null)
                            Response.Redirect("~/ContentValidation.aspx", true);

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

        protected void ddLang_SelectedIndexChanged(object sender, EventArgs e)
        {
           /* if (currentEntityID == -1)
                return;

            DropDownList ddDest = (DropDownList)LoginViewImportant.FindControl("ddStart");
            DropDownList ddTP = (DropDownList)LoginViewImportant.FindControl("ddTimePeriod");
            DropDownList rdLang = sender as DropDownList;

            DrawEntity(currentEntityID, rdLang.SelectedValue, ddTP.SelectedValue);
            * */
        }

        protected void ddStart_SelectedIndexChanged(object sender, EventArgs e)
        {

        }





    }
}