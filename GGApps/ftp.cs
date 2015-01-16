using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Web.Hosting;

namespace GGApps
{
    public class FTP
    {
        private string host = null;
        private string user = null;
        private string pass = null;
        private FtpWebRequest ftpRequest = null;
        private FtpWebResponse ftpResponse = null;
        private Stream ftpStream = null;
        private int bufferSize = 2048;
        public static CreateLogFiles Log = new CreateLogFiles();
        private string mapErrorPath = string.Empty;
        public string actualWorkDir = HostingEnvironment.MapPath("~/Batch/");
        public string appName = string.Empty;
                 

        /* Construct Object */
        public FTP(string hostIP, string userName, string password, string mapParthError, string appName) 
        { 
            host = hostIP; 
            user = userName; 
            pass = password; 

            // The Logger.
            this.mapErrorPath = mapParthError;
            this.appName = appName;

        }

        /* Download File */
        public long download(string remoteFile, string localFile)
        {
            long totalBytes = -1;
            
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);
                
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                
                /* Get the FTP Server's Response Stream */
                ftpStream = ftpResponse.GetResponseStream();
                
                /* Open a File Stream to Write the Downloaded File */
                FileStream localFileStream = new FileStream(localFile, FileMode.Create);
                
                /* Buffer for the Downloaded Data */
                byte[] byteBuffer = new byte[bufferSize];
                int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                
                /* Download the File by Writing the Buffered Data Until the Transfer is Complete */
                try
                {
                    while (bytesRead > 0)
                    {
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                    }
                    if (localFileStream != null)
                        totalBytes += localFileStream.Length;
                }
                catch (Exception ex)
                {
                    Log.ErrorLog(mapErrorPath, "Error while downloading file " + localFile + ": " + ex.Message, appName);
                    return -1;
                }
                
                /* Resource Cleanup */
                localFileStream.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            {
                Log.ErrorLog(mapErrorPath, "Error in download()" + ex.Message, appName);
                return -1;
            }
            return totalBytes;
        }



        /* Upload File */
        public long upload(string localFile, string remoteFile, bool overwrite)
        {
            long totalBytes = 0;

            try
            {
                
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);

                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpRequest.GetRequestStream();
                
                /* Open a File Stream to Read the File for Upload */
                FileStream localFileStream = new FileStream(localFile, FileMode.Open);
                
                /* Buffer for the Downloaded Data */
                byte[] byteBuffer = new byte[bufferSize];
                int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                
                /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
                try
                {
                    while (bytesSent != 0)
                    {
                        ftpStream.Write(byteBuffer, 0, bytesSent);
                        bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                    }
                    if( localFileStream != null)
                        totalBytes += localFileStream.Length;

                    // Log the file upload 
                    Log.InfoLog(mapErrorPath, appName + " File " + localFile + " uploaded to " + remoteFile + " Size: " + SizeSuffix(localFileStream.Length), appName);
                }
                catch (Exception ex)
                {
                    Log.ErrorLog(mapErrorPath, "Error while uploading file " + localFile + ": " + ex.Message, appName); 
                }
                /* Resource Cleanup */
                localFileStream.Close();
                ftpStream.Close();
                ftpRequest = null;
            }
            catch (Exception ex) 
            {
                if (ex.Message.Contains("File name not allowed"))   // this is not really an error
                {
                    Log.InfoLog(mapErrorPath, "Error while initialize and upload file " + localFile + ": " + ex.Message, appName);
                    return totalBytes;
                }
                else
                {
                    Log.ErrorLog(mapErrorPath, "Error while initialize and upload file " + localFile + ": " + ex.Message, appName);
                    return -1; 
                }
            }
            return totalBytes;
        }

        /* Delete File */
        public void delete(string deleteFile)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + deleteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return;
        }

        /* Rename File */
        public string rename(string currentFileNameAndPath, string newFileName)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentFileNameAndPath);

                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);

                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;

                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.Rename;

                /* Rename the File */
                ftpRequest.RenameTo = newFileName;

                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex) 
            {
                Log.InfoLog(mapErrorPath, "Error while rename file in production: " + currentFileNameAndPath + " to " + newFileName + " : " + ex.Message, appName);
                return null;
            }
            return currentFileNameAndPath;
        }

        /* Create a New Directory on the FTP Server */
        public void createDirectory(string newDirectory)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + newDirectory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return;
        }

        /* Get the Date/Time a File was Created */
        public string getFileCreatedDateTime(string fileName)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string fileInfo = null;
                /* Read the Full Response Stream */
                try { fileInfo = ftpReader.ReadToEnd(); }
                catch (Exception ex) 
                {
                    // this is not really an error..
                    Log.InfoLog(mapErrorPath, "PUBLISH> File " + fileName + " does not exist in production: " + ex.Message, appName);
                    return null; 
                }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return File Created Date Time */
                return fileInfo;
            }
            catch (Exception ex) 
            {
                // this is not really an error..
                Log.InfoLog(mapErrorPath, "PUBLISH> file " + fileName + " does not exist in production: " + ex.Message, appName);
                return null; 
            }

        }

        /* Get the Size of a File */
        public string getFileSize(string fileName)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string fileInfo = null;
                /* Read the Full Response Stream */
                try { while (ftpReader.Peek() != -1) { fileInfo = ftpReader.ReadToEnd(); } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return File Size */
                return fileInfo;
            }
            catch (Exception ex) { return null; }
            /* Return an Empty string Array if an Exception Occurs */
            return "";
        }

        /* List Directory Contents File/Folder Name Only */
        public string[] directoryListSimple(string directory)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string directoryRaw = null;
                /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] { "" };
        }

        /* List Directory Contents in Detail (Name, Size, Created, etc.) */
        public string[] directoryListDetailed(string directory)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string directoryRaw = null;
                /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] { "" };
        }


        public static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(Int64 value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }
}