﻿using System;
using System.Web;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.AspNet.Membership.OpenAuth;

namespace GGApps.Account
{
    public partial class RegisterExternalLogin : Common
    {
        protected string ProviderName
        {
            get { return (string)ViewState["ProviderName"] ?? String.Empty; }
            private set { ViewState["ProviderName"] = value; }
        }

        protected string ProviderDisplayName
        {
            get { return (string)ViewState["ProviderDisplayName"] ?? String.Empty; }
            private set { ViewState["ProviderDisplayName"] = value; }
        }

        protected string ProviderUserId
        {
            get { return (string)ViewState["ProviderUserId"] ?? String.Empty; }
            private set { ViewState["ProviderUserId"] = value; }
        }

        protected string ProviderUserName
        {
            get { return (string)ViewState["ProviderUserName"] ?? String.Empty; }
            private set
            {
                ViewState["ProviderUserName"] = value;
            }
        }

        protected void Page_Load()
        {
            if (!IsPostBack)
            {
                ProcessProviderResult();
            }
        }

        protected void logIn_Click(object sender, EventArgs e)
        {
            CreateAndLoginUser();
        }

        protected void cancel_Click(object sender, EventArgs e)
        {
            RedirectToReturnUrl();
        }

        private void ProcessProviderResult()
        {
            // Process the result from an auth provider in the request
            ProviderName = OpenAuth.GetProviderNameFromCurrentRequest();

            if (String.IsNullOrEmpty(ProviderName))
            {
                Response.Redirect(FormsAuthentication.LoginUrl);
            }

            // Build the redirect url for OpenAuth verification
            var redirectUrl = "~/Account/RegisterExternalLogin";
            var returnUrl = Request.QueryString["ReturnUrl"];
            if (!String.IsNullOrEmpty(returnUrl))
            {
                redirectUrl += "?ReturnUrl=" + HttpUtility.UrlEncode(returnUrl);
            }

            // Verify the OpenAuth payload
            var authResult = OpenAuth.VerifyAuthentication(redirectUrl);
            ProviderDisplayName = OpenAuth.GetProviderDisplayName(ProviderName);
            if (!authResult.IsSuccessful)
            {
                Title = "External login failed";
                userNameForm.Visible = false;

                ModelState.AddModelError("Provider", String.Format("External login {0} failed.", ProviderDisplayName));

                // To view this error, enable page tracing in web.config (<system.web><trace enabled="true"/></system.web>) and visit ~/Trace.axd
                Trace.Warn("OpenAuth", String.Format("There was an error verifying authentication with {0})", ProviderDisplayName), authResult.Error);
                return;
            }
            else
            // let only specific email to register.
            {
                if (authResult.IsSuccessful)
                {

                    if (rootWebConfig.AppSettings.Settings.Count > 0)
                    {
                        System.Configuration.KeyValueConfigurationElement acceptedAccounts = rootWebConfig.AppSettings.Settings["acceptedAccounts"];
                        if (acceptedAccounts != null)
                        {
                            string[] arr = acceptedAccounts.Value.ToString().Split(';');
                            var results = Array.FindAll(arr, s => s.Equals(authResult.UserName));
                            if (results == null || results.Length == 0)    // accepted emails
                            {
                                // delete account if exists and return
                                OpenAuth.DeleteAccount(authResult.UserName, authResult.Provider, authResult.ProviderUserId);

                                Title = "External login failed";
                                userNameForm.Visible = false;

                                ModelState.AddModelError("Provider", String.Format("External login {0} not accepted, please contact administrator.", ProviderDisplayName));

                                // To view this error, enable page tracing in web.config (<system.web><trace enabled="true"/></system.web>) and visit ~/Trace.axd
                                Trace.Warn("OpenAuth", String.Format("Authentication not accepted for this account with {0})", ProviderDisplayName), authResult.Error);

                                return;
                            }
                        }

                    }

                }
            }

            // User has logged in with provider successfully
            // Check if user is already registered locally
            if (OpenAuth.Login(authResult.Provider, authResult.ProviderUserId, createPersistentCookie: false))
            {
                RedirectToReturnUrl();
            }

            // Store the provider details in ViewState
            ProviderName = authResult.Provider;
            ProviderUserId = authResult.ProviderUserId;
            ProviderUserName = authResult.UserName;

            // Strip the query string from action
            Form.Action = ResolveUrl(redirectUrl);

            if (User.Identity.IsAuthenticated)
            {
                // User is already authenticated, add the external login and redirect to return url
                OpenAuth.AddAccountToExistingUser(ProviderName, ProviderUserId, ProviderUserName, User.Identity.Name);
                RedirectToReturnUrl();
            }
            else
            {
                // User is new, ask for their desired membership name
                userName.Text = authResult.UserName;
            }
        }

        private void CreateAndLoginUser()
        {
            if (!IsValid)
            {
                return;
            }

            var createResult = OpenAuth.CreateUser(ProviderName, ProviderUserId, ProviderUserName, userName.Text);
            if (!createResult.IsSuccessful)
            {

                ModelState.AddModelError("UserName", createResult.ErrorMessage);

            }
            else
            {
                // User created & associated OK
                if (OpenAuth.Login(ProviderName, ProviderUserId, createPersistentCookie: false))
                {
                    RedirectToReturnUrl();
                }
            }
        }

        private void RedirectToReturnUrl()
        {
            var returnUrl = Request.QueryString["ReturnUrl"];
            if (!String.IsNullOrEmpty(returnUrl) && OpenAuth.IsLocalUrl(returnUrl))
            {
                Response.Redirect(returnUrl);
            }
            else
            {
                Response.Redirect("~/");
            }
        }
    }
}