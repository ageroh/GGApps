using RefreshVersionsFiles.ServiceReference2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RefreshVersionsFiles
{
    class Program
    {
        static void Main(string[] args)
        {
          //
            GGServiceClient client = new GGServiceClient();

            // call the one and only webservice method to populate 
            client.DoWork();

            // Always close the client.
            client.Close();

            // force repetedtly to update Versions file from Production
        }

    }
}
