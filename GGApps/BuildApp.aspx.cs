using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Text;

namespace GGApps
{
    public partial class BuildApp : System.Web.UI.Page
    {
        public static StringBuilder sbExec = new StringBuilder();

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

            if (!Page.IsPostBack)
            {
#if DEBUG
                 Session["appName"] = "Santorini";
#endif


                if (Session["appName"] != null)
                {

                    string command = string.Format("3_convert_db.bat {0} ", Session["appName"].ToString());
                    ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
                    procStartInfo.WorkingDirectory = "C:\\Users\\Argiris\\Desktop\\GG_Batch\\Batch\\";
                    procStartInfo.RedirectStandardOutput = true;
                    procStartInfo.UseShellExecute= false;
                    procStartInfo.CreateNoWindow = false;

                    
                    // Now we create a process, assign its ProcessStartInfo and start it
                    Process proc = new Process();
                    //proc.OutputDataReceived += proc_OutputDataReceived;
                    //proc.Exited += myProcess_Exited;

                    proc.StartInfo = procStartInfo;
                    proc.Start();

                    //proc.WaitForExit();
                    
                    // instead of p.WaitForExit(), do
                    StringBuilder q = new StringBuilder();
                    while (!proc.HasExited)
                    {
                        q.AppendLine("</br>"+ proc.StandardOutput.ReadLine());
                      //  Response.Write("</br>"+ proc.StandardOutput.ReadLine());
                    }
                    Response.Write(q.ToString());

                   // proc.BeginOutputReadLine();

               }
            }
        }

        private void myProcess_Exited(object sender, EventArgs e)
        {
            Response.Write(HttpUtility.HtmlEncode(sbExec.ToString()));   
        }

        void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                sbExec.AppendLine(e.Data.ToString());
                // Log execution to file
                //Log.InfoLog(errorPa
                Response.Write(HttpUtility.HtmlEncode(e.Data.ToString()));
            }
        }
    

    }
}