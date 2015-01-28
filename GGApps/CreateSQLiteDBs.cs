using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data.SQLite;

namespace GGApps
{
    public class CreateSQLiteDBs : Common 
    {
        //query gia dhmiourgia tou table bundled images an den yparxei idi sti kathe vasi
	    public static String createTable = "CREATE  TABLE  IF NOT EXISTS 'bundled_images' ('path' VARCHAR PRIMARY KEY  NOT NULL , 'size' BIGINT NOT NULL)";
	    public static String query;
	
	
        //query gia oles tis photos olwn twn entities
	     public static String query1 =   "SELECT  entEntityID, NEW_PATH_C, entEntityTypeID FROM Entity " +
	                                     "JOIN Entity_Relation ON enrEntityID = entEntityID AND (enrRelationID=0 OR enrRelationID=31) JOIN " +
	                                     "Filter_Entity ON fieEntityID=enrParentEntityID AND (fieFilterID=20101 OR fieFilterID = 20102 OR fieFilterID = 20103)" +
	                                     "WHERE NEW_PATH_C NOT NULL " +
	                                     "" +
	                                     "UNION " +
	                                     "" +
	                                     "SELECT entEntityID, NEW_PATH_C, entEntityTypeID FROM Entity " +
	                                     "JOIN Filter_Entity ON fieEntityID=entEntityID AND (fieFilterID=20101 OR fieFilterID = 20102 OR fieFilterID = 20103) " +
	                                     "WHERE entEntityTypeID <> 0 AND entEntityTypeID <> 24 AND NEW_PATH_C NOT NULL";
	
	     //query gia tin prwti fwto mono olwn twn entities
	     public static String query2 = "SELECT entEntityID, NEW_PATH_C FROM Entity JOIN Filter_Entity ON fieEntityID=entEntityID AND (fieFilterID=20101 OR fieFilterID = 20102 OR fieFilterID = 20103) WHERE entEntityTypeID <> 0 AND entEntityTypeID <> 24 AND NEW_PATH_C NOT NULL";
	
	     //query gia tin prwti fwto mono twn best kai basic
	     public static String query3 = "SELECT entEntityID, NEW_PATH_C FROM Entity JOIN Filter_Entity ON fieEntityID=entEntityID AND (fieFilterID=20101 OR fieFilterID = 20102) WHERE entEntityTypeID <> 0 AND entEntityTypeID <> 24 AND NEW_PATH_C NOT NULL";
	 
	     //query gia oles tis photos twn best kai basic
	     public static String query4 = "SELECT  entEntityID, NEW_PATH_C, entEntityTypeID FROM Entity " +
			                            "JOIN Entity_Relation ON enrEntityID = entEntityID AND (enrRelationID=0 OR enrRelationID=31) JOIN " +
			                            "Filter_Entity ON fieEntityID=enrParentEntityID AND (fieFilterID=20101 OR fieFilterID = 20102)" +
			                            "WHERE NEW_PATH_C NOT NULL " +
			                            "" +
			                            "UNION " +
			                            "" +
			                            "SELECT entEntityID, NEW_PATH_C, entEntityTypeID FROM Entity " +
			                            "JOIN Filter_Entity ON fieEntityID=entEntityID AND (fieFilterID=20101 OR fieFilterID = 20102) " +
			                            "WHERE entEntityTypeID <> 0 AND entEntityTypeID <> 24 AND NEW_PATH_C NOT NULL";
	 
        public static int CreateBundleDBAndFiles(string appName)
        {
            if (CreateSQLiteDBs.CreateBundledModeForDB(2, "C:\\temp\\images\\" + appName + "\\", "C:\\GGAppContent\\" + appName + "\\update\\android\\bundled_resources\\mode_" + 2 + "\\", appName) >= 0)

                if (CreateSQLiteDBs.CreateBundledModeForDB(3, "C:\\temp\\images\\" + appName + "\\", "C:\\GGAppContent\\" + appName + "\\update\\android\\bundled_resources\\mode_" + 3 + "\\", appName) >= 0)

                    if (CreateSQLiteDBs.CreateBundledModeForDB(4, "C:\\temp\\images\\" + appName + "\\", "C:\\GGAppContent\\" + appName + "\\update\\android\\bundled_resources\\mode_" + 4 + "\\", appName) >= 0)

                        // build bundle images mode 1 as DEFAULT.
                        if (CreateSQLiteDBs.CreateBundledModeForDB(1, "C:\\temp\\images\\" + appName + "\\", "C:\\GGAppContent\\" + appName + "\\update\\android\\images\\", appName) >= 0)
                            return 1;

            return -1;
            
        }

        /// <summary>
        /// Create a bundled db images on selected mode.
        /// 
        /// </summary>
         /// <param name="i">Enter 1 for all photos, 2 for only the first photos, 3 for the first photos of best and basic only and 4 for all of best and basic only</param>
         /// <param name="dbPath">Give the path of dbs</param>
         /// <param name="inputphotosPath">Give the input path photos</param>
         /// <param name="outputPhotosPath">Give the output path photos</param>
        public static int CreateBundledModeForDB(int i, string inputphotosPath, string outputPhotosPath, string appName) 
        {
            //GR
            if (createDBs(i, inputphotosPath, outputPhotosPath, "android", appName, "EL") >= 0)
                //EN
                if (createDBs(i, inputphotosPath, outputPhotosPath, "android", appName, "EN") >= 0)
                    //RU
                    if (createDBs(i, inputphotosPath, outputPhotosPath, "android", appName, "RU") >= 0)
                        return 0;
            return -1;
        }

        
	    private static void copyAssets(List<String> entitiesPaths, int mode, String inputPhotoPath, String outputPhotosPath,  string mobileDevice, string appName, string langID) 
        {
            try
            {
                using (SQLiteConnection con = new SQLiteConnection("Data Source=" + mapPath + "Batch\\dbfiles\\tempGG.db; Version=3;"))
                {
                    con.Open();

                    SQLiteCommand sqCommand = new SQLiteCommand();
                    sqCommand.Connection = con;
                    SQLiteTransaction myTrans;
                    string tmpfile = "";

                    // Start a local transaction 
                    myTrans = con.BeginTransaction(System.Data.IsolationLevel.Serializable);
                    // Assign transaction object for a pending local transaction 
                    sqCommand.Transaction = myTrans;

                    try
                    {
                        Log.InfoLog(mapPathError, "Try Inserting in bundled_images for "+mobileDevice+" on: " + appName + " " + langID, appName);

                        foreach (String filename in entitiesPaths)
                        {
                            long Size = 0;
                            tmpfile = filename;

                            //folder to extract photos px "/home/sth/Projects/ggandroid/Thessaloniki/res/drawable/"
                            if (!Directory.Exists(outputPhotosPath))
                                Directory.CreateDirectory(outputPhotosPath);

                            // inputPhotoPath : folder pou vriskontai oles oi photos px "/home/sth/Projects/ggandroid/Thessaloniki/res/drawable/"
                            if (File.Exists(inputPhotoPath + filename + ".jpg"))
                            {
                                File.Copy(inputPhotoPath + filename + ".jpg", outputPhotosPath + filename + ".jpg", true);

                                FileInfo inFile = new FileInfo(outputPhotosPath + filename + ".jpg");
                                Size = inFile.Length;

                                String query = "INSERT OR REPLACE INTO bundled_images ('path', 'size') VALUES ('" + filename + "','" + Size + "')";
                                sqCommand.CommandText = query;
                                sqCommand.ExecuteNonQuery();
                            }

                        }
                        myTrans.Commit();
                        Log.InfoLog(mapPathError, "Completed successfully", appName);
                    }
                    catch (Exception e)
                    {
                        myTrans.Rollback();
                        Log.ErrorLog(mapPathError, "Neither record was written to database., Failed to copy file: " + tmpfile + ".jpg Exception: " + e.Message, appName);
                    }

                }
            }
            catch (Exception e)
            {
                Log.ErrorLog(mapPathError, "Some Exception occured in copyAssets(), " + e.Message, appName);
            }
			
			
		
	    }



	    private static int createDBs(int mode, String inputPhotoPath, String outputPhotosPath, string mobileDevice, string appName, string langID)
        {
            // move original DB to temp, and work with temp one in a later version.
            string localDBfile = mapPath + "Batch\\dbfiles\\" + mobileDevice + "\\GreekGuide_" + appName + "_" + langID + "_" + DateTime.Now.ToString("yyyyMMdd") + ".db";
            string tempLocalDBfile = mapPath + "Batch\\dbfiles\\tempGG.db";


            if (File.Exists(localDBfile))
            {

                // copy file to temp Path.
                File.Copy(localDBfile, tempLocalDBfile, true);


                // work on tempLocalDBfile !

                try
                {
                    using (SQLiteConnection con = new SQLiteConnection("Data Source=" + tempLocalDBfile + "; Version=3;"))
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(createTable, con))
                        {
                            con.Open();
                            Log.InfoLog(mapPathError, "Opened database successfully for " + appName + " " + langID, appName);
                            cmd.ExecuteNonQuery();
                            con.Close();
                            Log.InfoLog(mapPathError, "Table added succesfully " + appName + " " + langID, appName);

                        }
                    }
                }
                catch (Exception e)
                {
                    Log.ErrorLog(mapPathError, "Failed to initialize DB Version on SQLite DB for " + langID + " Exception: " + e.Message, appName);
                    return -1;
                }

                try
                {
                    // run for selected mode appropriate query.
                    if (mode == 1)
                        query = query1;
                    else if (mode == 2)
                        query = query2;
                    else if (mode == 3)
                        query = query3;
                    else if (mode == 4)
                        query = query4;

                    List<String> entitiesPaths = new List<String>();

                    using (SQLiteConnection con = new SQLiteConnection("Data Source=" + tempLocalDBfile + "; Version=3;"))
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                        {
                            con.Open();
                            Log.InfoLog(mapPathError, "Opened database successfully for " + appName + " " + langID, appName);

                            using (SQLiteDataReader resultSet = cmd.ExecuteReader())
                            {
                                

                                if (resultSet.HasRows)
                                {
                                    while (resultSet.Read())
                                    {
                                        String id = resultSet["entEntityID"].ToString();
                                        String path = resultSet["NEW_PATH_C"].ToString();

                                        // mono oi prwtes fwtografies
                                        if (mode == 2 || mode == 3)
                                        {
                                            entitiesPaths.Add("entity_" + id + "_" + path + "_220_220");
                                            entitiesPaths.Add("entity_" + id + "_" + path + "_640_426");
                                        }

                                        // oles oi fwtografies
                                        else if (mode == 1 || mode == 4)
                                        {
                                            int typeID = 0;
                                            Int32.TryParse(resultSet["entEntityTypeID"].ToString(), out typeID);
                                            if (typeID != 0)
                                            {
                                                if (typeID == 24)
                                                    entitiesPaths.Add("entity_" + id + "_" + path + "_608_352");
                                                else
                                                {
                                                    entitiesPaths.Add("entity_" + id + "_" + path + "_220_220");
                                                    entitiesPaths.Add("entity_" + id + "_" + path + "_640_426");
                                                }
                                            }
                                            else
                                                entitiesPaths.Add("entity_" + id + "_" + path + "_640_426");
                                        }
                                    }
                                    Log.InfoLog(mapPathError, "Finished reading query for images", appName);


                                }
                                else
                                {
                                    Log.ErrorLog(mapPathError, "Some error occured while reading sql lite db for bundled images.", appName);
                                    return -1;
                                }
                            }
                        }
                    }

                    
                    copyAssets(entitiesPaths, mode, inputPhotoPath, outputPhotosPath, mobileDevice, appName, langID);


                    // copy temp DB to real path -- this is only needed for mode = 1
                    if (mode == 1)
                        File.Copy(tempLocalDBfile, localDBfile, true);
                    else
                    {
                        if (langID == "EL")
                            langID = "GR";
                        File.Copy(tempLocalDBfile, outputPhotosPath + "Content" + langID + ".db", true);
                    }
                    if (File.Exists(tempLocalDBfile))
                        File.Delete(tempLocalDBfile);

                    // for other modes move db to necessary path.

                   
                }

                catch (Exception e)
                {
                    Log.ErrorLog(mapPathError, "exception in createDBs(), " + e.Message, appName);
                    return -1;
                }


            }
            else
                return 2;   // no DB file exists, no bundled created.

            return 1;   // all ok
	    }

//	private static void exportDB() {
//
//		File sd = Environment.getExternalStorageDirectory();
//		File data = Environment.getDataDirectory();
//		FileChannel source = null;
//		FileChannel destination = null;
//		String currentDBPath = "/data/" + "com.greekguide.apps.android.Athens" + "/databases/ContentGR.db";
//		String backupDBPath = "ContentGR.db";
//		File currentDB = new File(data, currentDBPath);
//		File backupDB = new File(sd, backupDBPath);
//		try {
//			source = new FileInputStream(currentDB).getChannel();
//			destination = new FileOutputStream(backupDB).getChannel();
//			destination.transferFrom(source, 0, source.size());
//			source.close();
//			destination.close();
//			Toast.makeText(this, "DB Exported!", Toast.LENGTH_LONG).show();
//		}
//		catch (IOException e) {
//			e.printStackTrace();
//		}
//	}
     

    }
}


