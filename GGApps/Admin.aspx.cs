using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace GGApps
{
    public partial class Admin : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            UploadFilesRemote("lalal");
        }


        public void UploadFilesRemote(string appName)
        {
              FTP ftpClient = new FTP(@"ftp://10.0.64.41", "gregui", "Jnm()0poi");
              
  
           string localDir = "C:\\temp\\images\\testtest\\";
            
            foreach (string localFile in Directory.GetFiles(localDir))
            {
                FileInfo fi = new FileInfo(localFile);
                ftpClient.upload(localFile, "testtest/images/" + fi.Name);        
            }

            
            
        }



        public void UploadToFTP(string appName)
        {
            /* Create Object Instance */
            FTP ftpClient = new FTP(@"ftp://10.0.64.41", "gregui", "Jnm()0poi");


            ftpClient.uploadDirectories("C:\\temp\\images\\testtest\\", "testtest/images/");

            //            /* Upload a File */
            //           ftpClient.upload("C:\\temp\\"+appName+"\\fb-assets\\", @"/var/www/greekguide/" + appName.ToLower() +"/fb-assets");

            /* Download a File */
            //  ftpClient.download("etc/test.txt", @"C:\Users\metastruct\Desktop\test.txt");

            /* Delete a File */
            //  ftpClient.delete("etc/test.txt");

            /* Rename a File */
            //  ftpClient.rename("etc/test.txt", "test2.txt");

            /* Create a New Directory */
            // ftpClient.createDirectory("etc/test");

            /* Get the Date/Time a File was Created */
            //  string fileDateTime = ftpClient.getFileCreatedDateTime("etc/test.txt");
            //   Console.WriteLine(fileDateTime);

            /* Get the Size of a File */
            //  string fileSize = ftpClient.getFileSize("etc/test.txt");
            //    Console.WriteLine(fileSize);
            //
            /* Get Contents of a Directory (Names Only) */
            //string[] simpleDirectoryListing = ftpClient.directoryListDetailed("/etc");
            //for (int i = 0; i < simpleDirectoryListing.Count(); i++) { Console.WriteLine(simpleDirectoryListing[i]); }

            /* Get Contents of a Directory with Detailed File/Directory Info */
            //string[] detailDirectoryListing = ftpClient.directoryListDetailed("/etc");
            //for (int i = 0; i < detailDirectoryListing.Count(); i++) { Console.WriteLine(detailDirectoryListing[i]); }

            /* Release Resources */
            ftpClient = null;
        }


    }
}