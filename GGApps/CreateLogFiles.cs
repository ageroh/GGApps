using System;
using System.IO;
using System.Text;

namespace GGApps
{
	//<Summary>
	// This class used to created log files
	// Created by ali ahmad h - 2002
	//</Summary>

	public sealed class CreateLogFiles
	{
		private string sLogFormat;
		private string sErrorTime;
        
		public CreateLogFiles()
		{
			//sLogFormat used to create log files format :
			// dd/mm/yyyy hh:mm:ss AM/PM ==> Log Message
			sLogFormat = DateTime.Now.ToShortDateString().ToString()+" "+DateTime.Now.ToLongTimeString().ToString()+" ==> ";

            sErrorTime = DateTime.Now.ToString("yyyyMMdd");
            Common.HasErrors = false;
		}

		public void ErrorLog(string sPathName, string sErrMsg, string appName, string user = null)
		{
            sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
            StreamWriter sw ;
            if( appName.ToLower() == "generic")
                sw = new StreamWriter(sPathName + "generic.txt", true);
            else
                sw = new StreamWriter(sPathName + sErrorTime + "_" + appName + ".txt", true);

            sw.WriteLine(sLogFormat + "'" + user + "'> Error: " + sErrMsg);
			sw.Flush();
			sw.Close();
            Common.HasErrors = true;
		}


        public void ErrorLogAdmin(string sPathName, string sErrMsg, string appName, string user = null)
        {
            sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
            StreamWriter sw;
            if (appName.ToLower() == "generic")
                sw = new StreamWriter(sPathName + "generic.txt", true);
            else
                sw = new StreamWriter(sPathName + sErrorTime + "_" + appName + ".txt", true);

            sw.WriteLine(sLogFormat + "'" + user + "'> Error: " + sErrMsg);
            sw.Flush();
            sw.Close();
            Common.LogErrorAdmin = true;
        }


        public void InfoLog(string sPathName, string sInfoMsg, string appName, string user = null)
        {
            sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";

            StreamWriter sw ;
            if (appName.ToLower() == "generic")
                sw = new StreamWriter(sPathName + "generic.txt", true);
            else
                sw = new StreamWriter(sPathName + sErrorTime + "_" + appName + ".txt", true);

            sw.WriteLine(sLogFormat + "'"+user +"'> Info: " + sInfoMsg);
            sw.Flush();
            sw.Close();
        }
	}
}
