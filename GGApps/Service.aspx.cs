using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class Service : System.Web.UI.Page 
{
    public static string Delimiter = "|";

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.Buffer = false;

        while (true)
        {
            Response.Write(Delimiter + DateTime.Now.ToString("HH:mm:ss.FFF"));
            Response.Flush();

            // Suspend the thread for 1/2 a second
            System.Threading.Thread.Sleep(500);
        }

        // Yes I know we'll never get here, it's just hard not to include it!
        Response.End();
    }
}
