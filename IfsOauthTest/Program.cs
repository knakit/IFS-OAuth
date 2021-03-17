using Knakit.Ifs.Auth.Openid;

namespace IfsOauthTest
{
    class Program
    {
        static void Main(string[] args)
        {
            IfsConnection ifsconn = new IfsConnection();
            ifsconn.Ifs_url = "https://<SERVER>:<PORT>/";

            string IfsResp;
            IfsResp = ifsconn.MakeGET("PartHandling.svc/PartCatalogSet(PartNo='TEST1')");

        }
    }
}
