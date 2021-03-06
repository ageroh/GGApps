﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GGApps.Account
{
    public partial class Login : Common
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
            //OpenAuthLogin.ReturnUrl = Request.QueryString["ReturnUrl"];

            var returnUrl = HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                Response.Redirect(Request.QueryString["ReturnUrl"], true);
            }
        }
    }
}