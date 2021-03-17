using Knakit.Ifs.Auth.Openid;

namespace IfsOauthTest
{
    class Program
    {
        static void Main(string[] args)
        {
            IfsConnection ifsconn = new IfsConnection();

            //>IfsOauthTest.exe https://<SERVER>:<PORT>/ PartHandling.svc/PartCatalogSet(PartNo='TEST1')

            string ifsUrl = args[0];
            string resourcePath = args[1];
            ifsconn.Ifs_url = ifsUrl;

            string IfsResp;
            IfsResp = ifsconn.MakeGET(resourcePath);

        }
    }
}
