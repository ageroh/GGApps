using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics.Tracing;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Configuration;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Data.SQLite;

namespace GGApps
{
    public partial class BuildApp : Common
    {

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!Page.IsPostBack)
            {

                //CreateLogFiles Log = new CreateLogFiles();
                
                if (Session["appName"] != null && Session["appID"] != null)
                {
                    

                    if (!Init_BuildApp(Convert.ToInt32(Session["appID"].ToString()), Session["appName"].ToString()))
                    {
                       
                        Log.ErrorLog(mapPathError, "Failed to Initialize In-App update for <span class='appName'>" + Session["appName"].ToString() + "</span>", Session["appName"].ToString() );
                        AddMessageToScreen("ExecutionMessages",
                                            @"<h2>Failed to Initialize In-App update, <strong>please contact Admin!</strong></h2>", this.Page);

                    }
                    else
                    { 
                        // ready to upDate!
                        lbl1.Text = "Ready to perform update for <span class='appName'>" + Session["appName"].ToString() + "</span>, please select what to do from the below list.";
                    }
                    
                }
                else
                    Response.Redirect("~/");
            }
        }


        /// <summary>
        /// Export only DB for App.
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        private void BatchExecuteDBExport(int appID, string appName)
        {
            string startProcessing = DateTime.Now.ToString("HH:mm - ddd d MMM yyyy");
            Session["FinishedProcessing"] = false;
            HostingEnvironment.QueueBackgroundWorkItem(async ct =>
                  {

                      var result3 = await RunAsyncCommandBatch(ct, appID, appName, "3_convert_db.bat " + appName, actualWorkDir, "convert SQL Db to SQLLite", mapPathError, Log);

                      if (!result3.IsCancellationRequested)
                      {

                          if (_Default.CheckThreeLanguages(appID))
                          {
                              var result3a = await RunAsyncCommandBatch(ct, appID, appName, "3a_fix_html_entities.bat " + appName, actualWorkDir, "convert HTML ENTITIES", mapPathError, Log);

                              if (!result3a.IsCancellationRequested)
                              {

                                  // do the copy-paste of DBs to server.
                                  var result9b = await RunAsyncCommandBatch(ct, appID, appName, "9b_copy_databases.bat " + appName, actualWorkDir, "COPY DATABASES Files", mapPathError, Log);

                                  if (!result9b.IsCancellationRequested)
                                  {
                                      
                                      Log.InfoLog(mapPathError, appName + " Produced successfully over Test Content.", appName);
                                      List<string> _listAttachments = new List<string>();

                                      if ((bool)HasErrors == false) 
                                      {
                                          //SendMail To All Teams
                                          await SendMailToUsers(appName
                                                                  , GetEmailList("AllTeams")
                                                                  , null
                                                                  , EmailTemplate("Success", appName, startProcessing), "GG DB produced for " + appName
                                                                  , mapPathError, Log);

                                      }
                                      else// Send Failure Message!
                                      {
                                          //SendMail To All Teams
                                          await SendMailToUsers(appName
                                                                  , GetEmailList("AllTeams")
                                                                  , null
                                                                  , EmailTemplate("Failure", appName, startProcessing), "GG DB produced for " + appName
                                                                  , mapPathError, Log);

                                          //Send Always email with Log info for currnet execution to ME.
                                          _listAttachments.Add(mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt");
                                          await SendMailToUsers(appName
                                                                  , GetEmailList("ErrorTeam")
                                                                  , _listAttachments
                                                                  , EmailTemplate("Failure", appName, startProcessing), "GG DB produced for " + appName
                                                                  , mapPathError, Log);

                                      }


                                      HasErrors = false;
                                      Session["FinishedProcessing"] = true;
                                      return;
                                  }

                              }
                          }
                          else
                          {

                              // do the copy-paste of DBs to server.
                              var result9b = await RunAsyncCommandBatch(ct, appID, appName, "9b_copy_databases.bat " + appName, actualWorkDir, "COPY DATABASES Files", mapPathError, Log);

                              if (!result9b.IsCancellationRequested)
                              {

                                  Log.InfoLog(mapPathError, appName + " Produced successfully over Test Content.", appName);
                                  List<string> _listAttachments = new List<string>();


                                  if ((bool)HasErrors == false)
                                  {  
                                      //SendMail To All Teams
                                      await SendMailToUsers(appName
                                                              , GetEmailList("AllTeams")
                                                              , null
                                                              , EmailTemplate("Success", appName, startProcessing), "GG DB produced for " + appName
                                                              , mapPathError, Log);

                                  }
                                  else// Send Failure Message!
                                  {
                                      //SendMail To All Teams
                                      await SendMailToUsers(appName
                                                              , GetEmailList("AllTeams")
                                                              , null
                                                              , EmailTemplate("Failure", appName, startProcessing), "GG DB produced for " + appName
                                                              , mapPathError, Log);

                                      //Send Always email with Log info for currnet execution to ME.
                                      _listAttachments.Add(mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt");
                                      await SendMailToUsers(appName
                                                              , GetEmailList("ErrorTeam")
                                                              , _listAttachments
                                                              , EmailTemplate("Failure", appName, startProcessing), "GG DB produced for " + appName
                                                              , mapPathError, Log);
                                    
                                  }

                                  HasErrors = false;
                                  Session["FinishedProcessing"] = true;
                                  return;
                              }
                          
                          }
                      }

                      // send error mail for failure !

                  }
          );

        }


        /// <summary>
        /// Run all Steps from 3 to 9 of Batch Build asyncronously.
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        public void BatchExecuteAllSteps(int appID, string appName)
        {
           // CreateLogFiles Log = new CreateLogFiles();
            string startProcessing = DateTime.Now.ToString("HH:mm - ddd d MMM yyyy");
 

            Session["FinishedProcessing"] = false;
            HostingEnvironment.QueueBackgroundWorkItem(async ct =>
                {


                    var result3 = await RunAsyncCommandBatch(ct, appID, appName, "3_convert_db.bat " + appName, actualWorkDir
                                                                               , "convert SQL Db to SQLLite", mapPathError, Log);

                    if (!result3.IsCancellationRequested)
                    {

                        if (_Default.CheckThreeLanguages(appID))
                        {
                            var result3a = await RunAsyncCommandBatch(ct, appID, appName, "3a_fix_html_entities.bat " + appName, actualWorkDir
                                                                                        , "convert HTML ENTITIES", mapPathError, Log);
                        }

                        // Redirect result to temp file for APP with dateFormat
                        var result4 = await RunAsyncCommandBatch(ct, appID
                                                                    , appName
                                                                    , "4_db_stats.bat " + appID.ToString() + "  reports/" + appName + "_db_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".txt"
                                                                    , actualWorkDir
                                                                    , "Export Database Statistics", mapPathError, Log);

                        if (!result4.IsCancellationRequested)
                        {

                            var result5 = await RunAsyncCommandBatch(ct, appID, appName, "5_get_images.bat " + appName, actualWorkDir
                                                                                        , "Transform All Images running Python", mapPathError, Log);

                            if (!result5.IsCancellationRequested)
                            {

                                Log.InfoLog(mapPathError, appName + "> Started Execution of Image Statics for App ", appName);
                                var result6 = ExecuteStep6(appID
                                                , appName
                                                , Server.MapPath("~/")
                                                , Log
                                                , mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt"
                                                , "Batch/reports/" + appName + "_image_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".html");
                                Log.InfoLog(mapPathError, appName + "> Finished Execution of Image Statics for App ", appName);
                                // Do this suncrounsly


                                if ( result6 != null)
                                {
                             
                                    // DO this syncronusly through .NET
                                    var result7 =// await RunAsyncCommandBatch(ct, appID, appName, "7_ftp_fb_img.bat " +  appName.ToLower(), actualWorkDir
                                                  //                                             , "Upload Images to FTP", mapPathError, Log);
                                    ExecuteStep7(appID, appName, Server.MapPath("~/"), Log, mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt");


                                    if (result7 != null)
                                    {


                                        // Do this Syncronously through .NET
                                        var result8 = await RunAsyncCommandBatch(ct, appID, appName, "8_ftp_entity_text.bat " + appName.ToLower() + " " + appID.ToString(), actualWorkDir
                                                                                                   , "Upload Entity to FTP", mapPathError, Log);

                                        if (!result8.IsCancellationRequested)
                                        {

                                            var result9 = await RunAsyncCommandBatch(ct, appID, appName, "9_copy_img_databases.bat " + appName + " " + DateTime.Now.ToString("yyyyMMdd"), actualWorkDir
                                                                                                        , "Copy files from Local to Server", mapPathError, Log);

                                            // LOG THIS
                                            // Send email to QA - Nadia - Galufos Team (for versions file)
                                            if (!result9.IsCancellationRequested)
                                            {
                                                
                                                List<string> _listAttachments = new List<string>();
                                                _listAttachments.Add(actualWorkDir + "reports\\" + appName + "_db_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                                                _listAttachments.Add(actualWorkDir + "reports\\" + appName + "_image_stats_" + DateTime.Now.ToString("yyyyMMdd") + ".html");
                                                Log.InfoLog(mapPathError, appName + " Produced successfully over Test Content.", appName);

                                                if ((bool)HasErrors == false)
                                                {
                                                    
                                               

                                                    
                                                    //SendMail To All Teams
                                                    await SendMailToUsers(appName
                                                                            , GetEmailList("AllTeams")
                                                                            , _listAttachments
                                                                            , EmailTemplate("Success", appName, startProcessing), "GG App produced for " + appName
                                                                            , mapPathError, Log);

                                                    //Send Always email with Log info for currnet execution to ME.
                                                    //_listAttachments.Add(mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt");
                                                    await SendMailToUsers(appName
                                                                            , GetEmailList("ErrorTeam")
                                                                            , _listAttachments
                                                                            , EmailTemplate("Info", appName, startProcessing, mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt")
                                                                            , "GG App produced for " + appName
                                                                            , mapPathError, Log
                                                                            );


                                                }
                                                else// Send Failure Message!
                                                {
                                                    //SendMail To All Teams
                                                    await SendMailToUsers(appName
                                                                            , GetEmailList("AllTeams")
                                                                            , _listAttachments
                                                                            , EmailTemplate("Failure", appName, startProcessing), "GG App produced for " + appName
                                                                            , mapPathError, Log);

                                                    //Send Always email with Log info for currnet execution to ME.
                                                    //_listAttachments.Add(mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt");
                                                    await SendMailToUsers(appName
                                                                            , GetEmailList("ErrorTeam")
                                                                            , _listAttachments
                                                                            , EmailTemplate("Failure", appName, startProcessing, mapPathError + DateTime.Now.ToString("yyyyMMdd") + "_" + appName + ".txt")
                                                                            , "GG App produced for " + appName
                                                                            , mapPathError, Log);

                                                    
                                                    
                                                }


                                                HasErrors = false;
                                                Session["FinishedProcessing"] = true;
                                                return;

                                            }

                                        }

                                    }
                                }
                            }

                        }

                    }


                    // Send failure email if execution was interupted / or other error occured!!
                    // ?


                }



            );

        }

        private object ExecuteStep7(int appID, string appName, string mapPath, CreateLogFiles Log, string logFileName)
        {
            double totalBytesUploaded = 0;
            // Upload Files Syncronously

            totalBytesUploaded = UploadFilesRemote(appName, "C:\\temp\\images\\" + appName + "\\", "GGAPPS/Corfu/", true, step);
            if (totalBytesUploaded < 10)
                return null;
            else
            {
                return totalBytesUploaded;

            }
        }
    

        private object ExecuteStep6(int appID, string appName, string actualWorkDir, CreateLogFiles Log, string logPath, string fileName)
        {
            StringBuilder sb = new StringBuilder();
        
            // Start Log
            
            string path = actualWorkDir + "SQLScripts\\";
            sb.Length = 0;
            sb.Clear();

            sb.AppendLine("<h2>*** images in db <h2>");
            sb.AppendLine("<br/>* ");

            try
            {
                // call SQL script to fetch data 
                sb.AppendLine(executeSQLScript(appID, appName, 1, path + "db_number_of_images.sql"));
            }
            catch (Exception ex)
            {
                HasErrors = true;
                Log.ErrorLog(logPath, "ExecuteStep6 a." + ex.Message, appName);
                return null;
            }


            sb.AppendLine("<br/>* ");
            string path_from = "c:\\Temp\\Images\\" + appName;
            
            try
            {

                sb.AppendLine("<br/>*** fb images in local dir (" + path_from + "-fb)");
                sb.AppendLine("<br/>* ");
                if (Directory.Exists(path_from + "-fb"))
                {
                    int cnt = Directory.GetFiles(path_from + "-fb").Length;
                    sb.AppendLine("<br/>* " + cnt);
                }
                sb.AppendLine("<br/>* ");
                sb.AppendLine("<br/>*** fb images in local dir (" + path_from + ")");
                sb.AppendLine("<br/>* ");
                if (Directory.Exists(path_from))
                {
                    int cnt = Directory.GetFiles(path_from).Length;
                    sb.AppendLine("<br/>* " + cnt);
                }
                
                //sb.AppendLine("<br/>* ");
                //sb.AppendLine("<br/>*** fb images in remote dir (" + path_to + ")");
                //sb.AppendLine("<br/>* ");
                //if (Directory.Exists(path_to))
                //{
                //    int cnt = Directory.GetFiles(path_to).Length;
                //    sb.AppendLine("<br/>* " + cnt);
                //}
                sb.AppendLine("<br/>* ");
                File.WriteAllText(actualWorkDir + fileName, sb.ToString(), Encoding.UTF8);

            }
            catch (IOException ex)
            {
                HasErrors = true;
                Log.ErrorLog(logPath, "Some IO exception occured on ExecuteStep6 b. " + ex.Message, appName);
                return null;
            }


            return sb.ToString();


        }



        /// <summary>
        /// Dynamically prints a message on Screen
        /// </summary>
        /// <param name="divID"></param>
        /// <param name="msg"></param>
        /// <param name="baseCtrl"></param>
        private void AddMessageToScreen(string divID, string msg, Control baseCtrl)
        {
            // Add to Div message for execution // Don't close window !!
            StringWriter stringWriter = new StringWriter();

            // Put HtmlTextWriter in using block because it needs to call Dispose.
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                // The important part:
                writer.RenderBeginTag(HtmlTextWriterTag.Div); // Begin #1
                writer.Write(msg);
                writer.RenderEndTag();
                Control ExecutionMessages = baseCtrl.FindControl(divID);
                if (ExecutionMessages != null)
                {
                    LiteralControl ltCtrl = new LiteralControl();
                    ltCtrl.Text = writer.InnerWriter.ToString();
                    ExecutionMessages.Controls.Add(ltCtrl);

                }

            }
        
        }

        /// <summary>
        /// Select What to Do, process all, process only DB, process only Files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCheckBox_Click(object sender, EventArgs e)
        {
            RadioButtonList chkList = (RadioButtonList)((Control)sender).FindControl("BuildAppListID");
            List<string> chkListActions = new List<string>();

            if (Session["appName"] == null || Session["appID"] == null)
                Response.Redirect("~/");

            if (chkList != null)
            {
                if (HasErrors == null)
                    HasErrors = false;

                foreach (ListItem item in chkList.Items)
                {
                    if (item.Selected)
                    {
                        // If the item is selected, add the value to the list.
                        chkListActions.Add(item.Value);
                    }
                }

                // Perform nessacary actions
                if (chkListActions != null)
                {
                    foreach (String cmdStr in chkListActions)
                    {
                        if (cmdStr == "FullBatch")
                        {
                            BatchExecuteAllSteps((int)Session["appID"], Session["appName"].ToString());
                            ((Control)sender).Parent.FindControl("mainSubPanel").Visible = false;
                            AddMessageToScreen("ExecutionMessages"
                                               , "<h2>Batch process execution has started for <span class='appName'>" + Session["appName"].ToString() + "</span>. </h2>"
                                                  + "<h3>You will be notified via e-mail upon completition.</h3>"
                                                  + "<strong>Thank You!</strong>", ((Control)sender).Parent);
                        }
                        else if (cmdStr == "DBOnly")
                        {
                            BatchExecuteDBExport((int)Session["appID"], Session["appName"].ToString());
                            ((Control)sender).Parent.FindControl("mainSubPanel").Visible = false;
                            AddMessageToScreen("ExecutionMessages"
                                               , "<h2>Batch process execution has started for <span class='appName'>" + Session["appName"].ToString() + "</span>. </h2>"
                                                  + "<h3>You will be notified via e-mail upon completition.</h3>"
                                                  + "<strong>Thank You!</strong>", ((Control)sender).Parent);
                        }
                        //else if (cmdStr == "ImagesOnly")
                        //{
                        //    BatchExecuteAllSteps((int)Session["appID"], Session["appName"].ToString());
                        //    AddMessageToScreen("ExecutionMessages"
                        //                       , "<h2>Batch process execution has begun for " + appName.ToString() + ". </h2>"
                        //                          + "<h3>Teams will be notified when execution is finished.</h3>"
                        //                          + "<strong>Thank You!</strong>");
                        //}
                        else
                        {

                            AddMessageToScreen("ExecutionMessages"
                                       , "<h2>Please select what to process from above radio button.</h2>"
                                          + "<strong>Thank You!</strong>", ((Control)sender).Parent);

                        }

                    }

                    if (chkListActions.Count == 0)
                    {
                        AddMessageToScreen("ExecutionMessages"
                                          , "<h2>Please select what to process from above radio button.</h2>"
                                             + "<strong>Thank You!</strong>", ((Control)sender).Parent);
                    }
                }
            }

            
         }


        private async Task<CancellationToken> RunAsyncCommandBatch(CancellationToken ct, int appID, string appName, string command, string actualWorkDir, string ExplainCmd, string mapPath, CreateLogFiles log, bool redirectOut = true)
        {
            // perform a long running operation, e.g. network service call, computation, file IO
            ProcessStartInfo procStartInfo;

            try
            {
                if (redirectOut)
                {
                    procStartInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
                }
                else
                // This is when we have a pipe execution!
                {
                    procStartInfo = new ProcessStartInfo("cmd.exe");
                    procStartInfo.Arguments = @"/C """ + command.ToString() + "\"";
                }


                procStartInfo.WorkingDirectory = actualWorkDir;
                procStartInfo.RedirectStandardOutput = redirectOut;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = false;
                procStartInfo.LoadUserProfile = false;
                procStartInfo.RedirectStandardError = true;



                // Now we create a process, assign its ProcessStartInfo and start it
                Process proc = new Process();

                proc.StartInfo = procStartInfo;


                log.InfoLog(mapPath, "Started:> " + ExplainCmd + " for APP: " + appName, appName);
                proc.Start();

                // instead of p.WaitForExit(), do
                StringBuilder q = new StringBuilder();
                while (!proc.HasExited)
                {
                    if (redirectOut)
                    {
                        q.AppendLine("</br>" + proc.StandardOutput.ReadLine());

                        // Log the process
                        log.InfoLog(mapPath, " PID:" + proc.Id + "> " + proc.StandardOutput.ReadLine(), appName);
                    }

                }

                proc.WaitForExit();

                log.InfoLog(mapPath, "Finished:> " + ExplainCmd + " for APP: " + appName, appName);

            }
            catch (Exception ex)
            {
                log.ErrorLog(mapPath, "Error while creating Process : " + ex.Message, appName);
                ct.ThrowIfCancellationRequested();
                return ct;
            }

            return ct;//q.ToString();
        }



        /// <summary>
        /// Verify that all directories are created for an app in order to run proprerly
        /// </summary>
        /// <returns>True if all ok even if dirs are created etc., config files updated etc, false if something wrong happend when updating</returns>
        /// <param name="appID"></param>
        /// <param name="appName"></param>
        public bool Init_BuildApp(int appID, string appName)
        {
            
            try
            {
                // Create directories under C:\temp\images if not exists, else create them
                if (!Directory.Exists("C:\\temp\\images\\" + appName))
                {
                    Directory.CreateDirectory("C:\\temp\\images\\" + appName);
                }

                if (!Directory.Exists("C:\\temp\\images\\" + appName + "-fb"))
                {
                    Directory.CreateDirectory("C:\\temp\\images\\" + appName + "-fb");
                }


                // add to next version !
                //if (!CheckExternalAppConfigSettings(appID, appName))
                //    return false;

            }
            catch (IOException e)
            {
                HasErrors = true;
                Log.ErrorLog(mapPathError, "Initialize of App-Update failed! " + e.Message, appName);
                return false;
            }

            return true;
            
        }


        /// <summary>
        /// Get the email list from web.config file
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        private List<string> GetEmailList(string team)
        {
            List<string> mailList = new List<string>();


            if (rootWebConfig.AppSettings.Settings[team] != null)
            {
                string teamMembers = rootWebConfig.AppSettings.Settings[team].Value;
                mailList = teamMembers.Split('|').ToList<string>();

                return mailList;
            }

            return null;

        }




        /// <summary>
        /// Send Email to Users
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="team"></param>
        /// <param name="attachmentFilenames"></param>
        /// <param name="emailBody"></param>
        /// <param name="emailSubject"></param>
        /// <returns></returns>
        private async Task SendMailToUsers(string appName, List<string> team, List<string> attachmentFilenames, string emailBody, string emailSubject, string mapPath, CreateLogFiles log)
        {

            using (var message = new MailMessage())
            {
                message.From = new MailAddress("noreply@greekguide.com");
                message.Subject = emailSubject;
                message.Body = emailBody;
                message.IsBodyHtml = true;

                // Build Receipents List
                foreach (String recStr in team)
                {
                    message.To.Add(new MailAddress(recStr));
                }

                try
                {
                    // ADD FILE ATTACHEMENTS IF THEY EXIST
                    foreach (string attachmentFilename in attachmentFilenames)
                    {
                        if (attachmentFilename != null)
                        {
                            Attachment attachment = new Attachment(attachmentFilename, MediaTypeNames.Application.Octet);
                            ContentDisposition disposition = attachment.ContentDisposition;
                            disposition.CreationDate = File.GetCreationTime(attachmentFilename);
                            disposition.ModificationDate = File.GetLastWriteTime(attachmentFilename);
                            disposition.ReadDate = File.GetLastAccessTime(attachmentFilename);
                            disposition.FileName = Path.GetFileName(attachmentFilename);
                            disposition.Size = new FileInfo(attachmentFilename).Length;
                            disposition.DispositionType = DispositionTypeNames.Attachment;
                            message.Attachments.Add(attachment);
                        }
                    }

                }
                catch (Exception ex)
                {
                    // FileAttachements not found for App! for this date.
                    log.InfoLog(mapPath, "Exception thrown while trying to collect attachment files: " + ex.Message, appName);
                }

                try
                {
                    // Finaly send email ..
                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 25,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new System.Net.NetworkCredential("admin@primemedia.gr", "ferrari35#71")
                    };

                    smtp.SendCompleted += smtp_SendCompleted;
                    await smtp.SendMailAsync(message);

                }
                catch (SmtpException sE)
                {
                    log.InfoLog(mapPath, "Exception while sending notification email! " + sE.Message, appName);
                }
            }
            
        }


        // Verify that email had been sent.
        void smtp_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (! e.Cancelled)
            {
                CreateLogFiles Log = new CreateLogFiles();
                Log.InfoLog(mapPathError, "e-Mails sent completed.", Session["appName"].ToString());
            }
        }



        private string EmailTemplate(string kind, string appName, string startProcessing, string LogErrorRealPath = null)
        {
            String str = String.Empty;

            switch (kind)
            {
                case "Success":
                    if (File.Exists(Server.MapPath("EmailTemplates//EmailTemplateSuccess.html")))
                    {
                        str = File.ReadAllText(Server.MapPath("EmailTemplates//EmailTemplateSuccess.html"), Encoding.UTF8);
                    }
                    break;
                case "Failure":
                    if (File.Exists(Server.MapPath("EmailTemplates//EmailTemplateFailure.html")))
                    {
                        str = File.ReadAllText(Server.MapPath("EmailTemplates//EmailTemplateFailure.html"), Encoding.UTF8);
                    }
                    break;
                case "Info":
                    if (File.Exists(Server.MapPath("EmailTemplates//EmailTemplateInfo.html")))
                    {
                        str = File.ReadAllText(Server.MapPath("EmailTemplates//EmailTemplateInfo.html"), Encoding.UTF8);
                    }
                    break;
                default: break;
            }

            if (!String.IsNullOrWhiteSpace(str))
            {
                str = str.Replace("{1}", startProcessing);
                str = str.Replace("{2}", appName);
                if( LogErrorRealPath != null)
                    str = str.Replace("{3}", LogErrorRealPath);

                return str;
            }

            return "<html></html>";
        }


        

     


        #region NOT USED  - do for v.2
        private bool CheckExternalAppConfigSettings(int appID, string appName)
        {
            try
            {
                bool[] checkLangs = new bool[10];

                //ExeConfigurationFileMap configMap = new System.Configuration.ExeConfigurationFileMap();
                //configMap.ExeConfigFilename = Server.MapPath("ExternalApp/") + @"SQLiteConverter.exe.config";
                //Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

                XmlDocument xml = new XmlDocument();
                xml.Load(Server.MapPath("ExternalApp/") + @"SQLiteConverter.exe.config");

                foreach (XmlNode node in xml.SelectNodes("configuration/DBConfigSection/DBSettings"))
                {
                    //<DBSetting name="Zakynthos" lang="el" db="ContentDB_165_Lan_1_Cat_369"/>
                    //<DBSetting name="Zakynthos" lang="en" db="ContentDB_165_Lan_2_Cat_369"/>      

                    if (node.Attributes["name"] != null && node.Attributes["lang"] != null)
                    {
                        if (node.Attributes["name"].ToString() == appName)
                            if (node.Attributes["lang"].ToString() == "el")
                                if (node.Attributes["db"].ToString() == "ContentDB_165_Lan_1_Cat_" + appID.ToString())
                                    checkLangs[(int)Lang.el] = true;

                        if (node.Attributes["name"].ToString() == appName)
                            if (node.Attributes["lang"].ToString() == "en")
                                if (node.Attributes["db"].ToString() == "ContentDB_165_Lan_2_Cat_" + appID.ToString())
                                    checkLangs[(int)Lang.en] = true;

                        if (_Default.CheckThreeLanguages(appID))
                            if (node.Attributes["name"].ToString() == appName)
                                if (node.Attributes["lang"].ToString() == "ru")
                                    if (node.Attributes["db"].ToString() == "ContentDB_165_Lan_4_Cat_" + appID.ToString())
                                        checkLangs[(int)Lang.ru] = true;
                    }

                }


                // Append new nodes where necessary
                for (int i = 0; i < checkLangs.Length; i++)
                {
                    if (checkLangs[i] != null)
                    {
                        xml.SelectSingleNode("configuration/DBConfigSection/DBSettings").AppendChild(addMissingNode(appID, appName, i, xml));
                    }
                
                }


                xml.Save(Server.MapPath("ExternalApp/") + @"SQLiteConverter.exe.config");


                return true;
            }
            catch (Exception e) {
                // log exception
                return false;
            }
        }



        private XmlElement addMissingNode(int appID, string appName, int lang, XmlDocument xml)
        {

            // name="Zakynthos" lang="el" db="ContentDB_165_Lan_1_Cat_369"/>

            //Create a new node.
            XmlElement elem = xml.CreateElement("DBSetting");
            XmlAttribute attr = xml.CreateAttribute("name");
            attr.Value = appName;
            elem.Attributes.Append(attr);

            attr = xml.CreateAttribute("lang");
            attr.Value = LangStr[lang];
            elem.Attributes.Append(attr);

            attr = xml.CreateAttribute("db");
            attr.Value = "ContentDB_165_Lan_"+lang.ToString()+"_Cat_" + appID.ToString();
            elem.Attributes.Append(attr);

            return elem;
        }


        //private async Task<CancellationToken> addLogAsync(CancellationToken ct, int currentLogCount, string msg)
        //{

        //    try
        //    {
        //        for (int i = 0; i < 5; i++)
        //        {
        //            if (ct.IsCancellationRequested)
        //            {
        //                Trace.Write(string.Format("{0} - signaled cancellation : msg {1}", DateTime.Now.ToLongTimeString(), msg));
        //                break;
        //            }
        //            Trace.Write(string.Format("{0} - msg:{1} - logcount:{2}",  DateTime.Now.Second.ToString(), msg, currentLogCount));

        //            // "Simulate" this operation took a long time, but was able to run without
        //            // blocking the calling thread (i.e., it's doing I/O operations which are async)
        //            // We use Task.Delay rather than Thread.Sleep, because Task.Delay returns
        //            // the thread immediately back to the thread-pool, whereas Thread.Sleep blocks it.
        //            // Task.Delay is essentially the asynchronous version of Thread.Sleep:
        //            await Task.Delay(2000, ct);
        //        }
        //    }
        //    catch (TaskCanceledException tce)
        //    {
        //        Trace.Write("Caught TaskCanceledException - signaled cancellation " + tce.Message);
        //    }
        //    return ct;
        //}



        #endregion



    }
}