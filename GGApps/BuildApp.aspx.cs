using System;
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

namespace GGApps
{
    public partial class BuildApp : System.Web.UI.Page
    {

        #region Properties
        public enum Lang : int 
        { 
            el = 1
            , en
            , miss
            , ru 
        };
        public static string[] LangStr = {"", "el", "en","miss", "ru"};
        public static StringBuilder sbExec = new StringBuilder();

        // change this on production
        public static string actualWorkDir = "C:\\Users\\Argiris\\Desktop\\GG_Batch\\Batch\\";


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
        #endregion


        protected void Page_Load(object sender, EventArgs e)
        {

            if (!Page.IsPostBack)
            {

                
                // LET USER DECIDE IF NEEDS TO RUN A SEPARATE STEP - OR EXECUTE THE FULL BATCH





#if DEBUG
                 Session["appName"] = "Pelion";
                 Session["appID"] = 405;
                 Session["mapPathError"] = MapPath("Logs/log_");
#endif

                CreateLogFiles Log = new CreateLogFiles();

                if (Session["appName"] != null && Session["appID"] != null)
                {

                    if (!Init_BuildApp(Convert.ToInt32(Session["appID"].ToString()), Session["appName"].ToString()))
                    {
                        Log.ErrorLog(_Default.mapPathError, "Failed to Initialize In-App update for " + Session["appName"].ToString());
                        Response.Write("<h2>ERROR, please Contact GG Admin!!!</h2>");
                    }
                    else
                    {

                        int appID = (int)Session["appID"];
                        string appName = Session["appName"].ToString();


                        // run batch step 3
                        //Func<CancellationToken, Task> workItem = RunAsyncExtractDBtoSQLLite;
                        HostingEnvironment.QueueBackgroundWorkItem(async ct =>
                            {
                                var result = await RunAsyncCommandBatch(ct, appID, appName, "3_convert_db.bat " + appName, actualWorkDir, "convert SQL Db to SQLLite", Session["mapPathError"].ToString(), Log);

                                if (!result.IsCancellationRequested)
                                {

                                    
                                    
                                    // CALL 3A IF NEEDED AND THEN THE REST.





                                    // do this SYNCRONOUS FOR IN FILE
                                    var result2 = await RunAsyncCommandBatch(ct, appID, appName, "4_db_stats.bat " + appID.ToString(), actualWorkDir, "Export Database Statistics", Session["mapPathError"].ToString(), Log);

                                    if (!result2.IsCancellationRequested)
                                    {

                                        var result3 = await RunAsyncCommandBatch(ct, appID, appName, "5_get_images.bat " + appName, actualWorkDir, "Transform All Images running Python", Session["mapPathError"].ToString(), Log);

                                        if (!result3.IsCancellationRequested)
                                        {
                                            // do this SYNCRONOUS FOR IN FILE
                                            var result5 = await RunAsyncCommandBatch(ct, appID, appName, "6_image_stats.bat " + appName + " " + appID.ToString(), actualWorkDir, "Export Image Statistics", Session["mapPathError"].ToString(), Log);

                                            if (!result3.IsCancellationRequested)
                                            {

                                                var result6 = await RunAsyncCommandBatch(ct, appID, appName, "7_ftp_fb_img.bat " + Char.ToLowerInvariant(appName[0]) + appName.Substring(1), actualWorkDir, "Upload Imaged to FTP", Session["mapPathError"].ToString(), Log);

                                                if (!result6.IsCancellationRequested)
                                                {

                                                    var result7 = await RunAsyncCommandBatch(ct, appID, appName, "8_ftp_entity_text.bat " + Char.ToLowerInvariant(appName[0]) + appName.Substring(1) + " " + appID.ToString(), actualWorkDir, "Export Image Statistics", Session["mapPathError"].ToString(), Log);

                                                    if (!result7.IsCancellationRequested)
                                                    {
                                                        var result8 = await RunAsyncCommandBatch(ct, appID, appName, "9_copy_img_databases.bat " + appName + " " + DateTime.Now.ToString("yyyyMMdd"), actualWorkDir, "Export Image Statistics", Session["mapPathError"].ToString(), Log);

                                                        // IF ALL DONE for this step.. 
                                                        // LOG THIS
                                                        // Send email to QA - Nadia - Galufos Team (for versions file)


                                                        // when this is finished..
                                                        // APP FINALY PRODUCED!

                                                    }

                                                }
                                            }
                                        }

                                    }

                                }
                                // DOES NOT WORK !
                                //Response.Redirect("completed.html", true); 
                            }

                        );

                    }
                }
            }
        }



        private async Task<CancellationToken> RunAsyncCommandBatch(CancellationToken ct, int appID, string appName, string command, string actualWorkDir, string ExplainCmd, string mapPath, CreateLogFiles log)
        {
            // perform a long running operation, e.g. network service call, computation, file IO
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            procStartInfo.WorkingDirectory = actualWorkDir;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = false;

            // Now we create a process, assign its ProcessStartInfo and start it
            Process proc = new Process();
            //proc.OutputDataReceived += proc_OutputDataReceived;
            
            proc.StartInfo = procStartInfo;

            log.InfoLog(mapPath, "Started:> " + ExplainCmd + " for APP: " + appName);
            proc.Start();

//            await addLogAsync(ct, msg);

            // instead of p.WaitForExit(), do
            StringBuilder q = new StringBuilder();
            while (!proc.HasExited)
            {
                q.AppendLine("</br>" + proc.StandardOutput.ReadLine());
            
                // Log the process
                log.InfoLog(mapPath, proc.StandardOutput.ReadLine());

            }
            log.InfoLog(mapPath, "Finished:> " + ExplainCmd + " for APP: " + appName);


            //await addLogAsync(ct, currentLogCount);
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

                // 10.168.10.70 M:
                if (!Directory.Exists("M:\\GreekGuide\\Images\\" + appName))
                {
                    Directory.CreateDirectory("M:\\GreekGuide\\Images\\" + appName);
                }

                if (!Directory.Exists("M:\\GreekGuide\\Databases\\" + appName))
                {
                    Directory.CreateDirectory("M:\\GreekGuide\\Databases\\" + appName);
                }


                // add to next version !
                //if (!CheckExternalAppConfigSettings(appID, appName))
                //    return false;

            }
            catch (IOException e)
            { 
                //Log this execption
                Log.ErrorLog(_Default.mapPathError, "Initialize of App-Update failed! " + e.Message);
                return false;
            }

            return true;
            
        }


        //private async Task SendMailAsync(string email)
        //{
        //    var myMessage = new SendGridMessage();

        //    myMessage.From = new MailAddress("Rick@Contoso.com");
        //    myMessage.AddTo(email);
        //    myMessage.Subject = "Using QueueBackgroundWorkItem";

        //    //Add the HTML and Text bodies
        //    myMessage.Html = "<p>Check out my new blog at "
        //          + "<a href=\"http://blogs.msdn.com/b/webdev/\">"
        //          + "http://blogs.msdn.com/b/webdev/</a></p>";
        //    myMessage.Text = "Check out my new blog at http://blogs.msdn.com/b/webdev/";

        //    using (var attachmentFS = new FileStream(GH.FilePath, FileMode.Open))
        //    {
        //        myMessage.AddAttachment(attachmentFS, "My Cool File.jpg");
        //    }

        //    var credentials = new NetworkCredential(
        //       ConfigurationManager.AppSettings["mailAccount"],
        //       ConfigurationManager.AppSettings["mailPassword"]
        //       );

        //    // Create a Web transport for sending email.
        //    var transportWeb = new Web(credentials);

        //    if (transportWeb != null)
        //        await transportWeb.DeliverAsync(myMessage);
        //}



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

        #endregion

    }
}