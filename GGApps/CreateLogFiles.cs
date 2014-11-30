using System;
using System.IO;
using System.Text;

namespace GGApps
{
	//<Summary>
	// This class used to created log files
	// Created by ali ahmad h - 2002
	//</Summary>

	public class CreateLogFiles
	{
		private string sLogFormat;
		private string sErrorTime;

		public CreateLogFiles()
		{
			//sLogFormat used to create log files format :
			// dd/mm/yyyy hh:mm:ss AM/PM ==> Log Message
			sLogFormat = DateTime.Now.ToShortDateString().ToString()+" "+DateTime.Now.ToLongTimeString().ToString()+" ==> ";
			
			//this variable used to create log filename format "
			//for example filename : ErrorLogYYYYMMDD
			string sYear	= DateTime.Now.Year.ToString();
			string sMonth	= DateTime.Now.Month.ToString();
			string sDay	= DateTime.Now.Day.ToString();
			sErrorTime = sYear+sMonth+sDay;
		}

		public void ErrorLog(string sPathName, string sErrMsg)
		{
			StreamWriter sw = new StreamWriter(sPathName+sErrorTime + ".txt",true);
			sw.WriteLine(sLogFormat + "Error: " + sErrMsg);
			sw.Flush();
			sw.Close();
           //?  throw new Exception(sErrMsg);
		}

        public void InfoLog(string sPathName, string sInfoMsg)
        {
            StreamWriter sw = new StreamWriter(sPathName + sErrorTime + ".txt", true);
            sw.WriteLine(sLogFormat + "Info: " + sInfoMsg);
            sw.Flush();
            sw.Close();
        }
	}
}
