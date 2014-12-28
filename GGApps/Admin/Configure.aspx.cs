using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GGApps
{
    public partial class Configure : CommonAdmin
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                Init();
            }
        }

        protected void EnvironmentConfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            ;
        }
    }
}