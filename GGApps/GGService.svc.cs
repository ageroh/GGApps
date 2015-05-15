using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace GGApps
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GGSevice" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select GGSevice.svc or GGSevice.svc.cs at the Solution Explorer and start debugging.
    public class GGService : IGGService
    {
        public void DoWork()
        {
            Finalize fin = new Finalize();
    
            // populate all versions.txt with correct data taken from configuration files found in production and 
            // from database records for AppVersion taken from webclient... maybe merge this in one app.. 
            fin.UpdateVersionsFilesProduction();
                        
            // Create asyncronlsy JSON delta files for patches of all DBs.
            //? 

        }

        // get all applications 

        // set versions.txt if exists for each application on production ! 
        
        // read all rows of table

        // update when needed

        // log it

    }


}
