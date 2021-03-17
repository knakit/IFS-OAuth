using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace Knakit.Ifs.Auth.Openid
{
    public class IfsConnection
    {
        private int connectionTimeout = 60000;

        private const string MAIN_PROJECTION_PATH = "main/ifsapplications/projection";
        private const string USER_AGENT_HEADER = "knak.it IFS Oauth/0.1";


        string ifs_url;
        string authorization_uri;
        string client_id;
        string scope;
        string token_endpoint;
        string authorization_endpoint;
        string code_verifier;
        string nonce;
        string state;
        string code_challenge_method;
        string code_challenge;
        string authority;
        string resource;
        string redirect_uri;
        string code;
        string id_token;
        string access_token;
        string refresh_token;
        string token_type;
        int expires_in;

        public string Ifs_url { get => ifs_url; set => ifs_url = value; }
        public string Authorization_uri
        {
            get { return authorization_uri; }
            set { authorization_uri = value.Trim(new Char[] { '"' }); }
        }
        public string Client_id { get => client_id; set => client_id = value; }
        public string Scope
        {
            get { return scope; }
            set { scope = value.Trim(new Char[] { '"' }); }
        }
        public string Token_endpoint { get => token_endpoint; set => token_endpoint = value; }
        private string Authorization_endpoint { get => authorization_endpoint; set => authorization_endpoint = value; }
        public string Code_verifier { get => code_verifier; set => code_verifier = value; }
        public string Nonce { get => nonce; set => nonce = value; }
        public string State { get => state; set => state = value; }
        public string Code_challenge_method { get => code_challenge_method; set => code_challenge_method = value; }
        public string Code_challenge { get => code_challenge; set => code_challenge = value; }
        public string Authority { get => authority; set => authority = value; }
        public string Resource { get => resource; set => resource = value; }
        public string Redirect_uri { get => redirect_uri; set => redirect_uri = value; }
        public string Code { get => code; set => code = value; }
        public string Id_token { get => id_token; set => id_token = value; }
        public string Access_token { get => access_token; set => access_token = value; }
        public string Refresh_token { get => refresh_token; set => refresh_token = value; }
        public string Token_type { get => token_type; set => token_type = value; }
        public int Expires_in { get => expires_in; set => expires_in = value; }


        public void Authenticate()
        {
            GetWwwAuthenticateHeader();

            GetIdpEndpoints();

            this.Redirect_uri = this.Ifs_url + "main/ifsapplications/projection/oauth2/callback";
            Console.WriteLine(this.Redirect_uri);

            GetAuthorizationCode();

            GetAccessToken();

            




            Console.WriteLine(this.Token_endpoint);
            Console.WriteLine(this.Authorization_endpoint);
        }

        // request sent to MAIN gateway projection
        // this will return with 401 Unauthorized with "WWW-Authenticate" header
        // details of the authentication endpoint is included in the header
        public void GetWwwAuthenticateHeader()
        {
            string sRequestUri = this.Ifs_url + MAIN_PROJECTION_PATH;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sRequestUri);
            req.Method = "GET";
            req.UserAgent = USER_AGENT_HEADER;
            req.KeepAlive = true;

            //req.UnsafeAuthenticatedConnectionSharing = true;
            req.Timeout = this.connectionTimeout;

            req.SendChunked = false;
            req.AllowAutoRedirect = false;
            req.ServicePoint.Expect100Continue = false;
            req.ServicePoint.UseNagleAlgorithm = false;

            req.ContentType = "text/html";
            req = SetIfsHeaders(req);

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                HttpWebResponse errResp = (HttpWebResponse)ex.Response;
                if (errResp != null && errResp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    string bearerRealmHeader = ((HttpWebResponse)ex.Response).Headers["WWW-Authenticate"];
                    string resourceId = ((HttpWebResponse)ex.Response).Headers["X-IFS-OAuth2-Resource"];
                    string providerType = ((HttpWebResponse)ex.Response).Headers["X-IFS-OAuth2-IDP"];

                    if (string.IsNullOrEmpty(bearerRealmHeader) || string.IsNullOrEmpty(resourceId) || string.IsNullOrEmpty(providerType))
                    {
                        throw new Exception("One or more headers are missing from IFS, something is wrong!");
                    }

                    // read authorization_uri, client_id, scope from bearerRealmHeader

                    string[] bearerRecArray = bearerRealmHeader.Split(',');

                    foreach (var recStr in bearerRecArray)
                    {

                        string[] rec = recStr.Split('=');

                        string name_ = rec[0].Trim();
                        string value_ = rec[1].Trim();

                        Console.WriteLine(name_);
                        Console.WriteLine(value_);

                        switch (name_)
                        {
                            case "scope":
                                this.Scope = value_;
                                break;
                            case "authorization_uri":
                                this.Authorization_uri = value_;
                                break;
                        }
                    }

                    int indexAt = bearerRealmHeader.IndexOf('@');
                    int indexScope = bearerRealmHeader.IndexOf(", scope");

                    this.Resource = resourceId;
                    this.Client_id = bearerRealmHeader.Substring(14, indexAt - 14);
                    this.Authority = bearerRealmHeader.Substring(indexAt + 1, indexScope - indexAt - 2);

                }
            }
        }


        // OpenID Connect Discovery
        // typically appends .well-known/openid-configuration to the auth URI
        public void GetIdpEndpoints()
        {
            WebClient client = new WebClient();
            string document = client.DownloadString(this.Authorization_uri + "/.well-known/openid-configuration");
            this.Token_endpoint = GetOpenIdDiscoveryField("token_endpoint", document);
            this.Authorization_endpoint = GetOpenIdDiscoveryField("authorization_endpoint", document);

            Console.WriteLine("+++++" + this.Authorization_endpoint);

        }

        private HttpWebRequest SetIfsHeaders(HttpWebRequest req)
        {
            string HTTPHeader_XIFSTimeout = "X-ifs-Timeout";
            if (req.Timeout > System.Threading.Timeout.Infinite)
            {
                req.Headers.Add(HTTPHeader_XIFSTimeout, req.Timeout.ToString());
            }

            return req;
        }

        private string GetOpenIdDiscoveryField(string name, string document)
        {
            string search = name + "\":\"";
            int i = document.IndexOf(search);
            if (i < 0)
            {
                throw new Exception("Malformed Discovery Document .. !");
            }
            else
            {
                string URL = document.Substring(i + search.Length);
                URL = URL.Substring(0, URL.IndexOf("\""));
                URL = URL.Replace(@"\", "");
                return URL;
            }

        }

        private void GetAuthorizationCode()
        {
            Thread t = new Thread(() => GetAuthorizationCodeThreaded());
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }
        private void GetAuthorizationCodeThreaded()
        {

            Form a = new Form();
            IntPtr ptr = a.Handle;
            a.BeginInvoke((MethodInvoker)(delegate { CreateAuthenticationWindow(); }));
            Application.Run();
        }
        private async void CreateAuthenticationWindow()
        {
            string getAccessToken = string.Empty;
            string getRefreshToken = string.Empty;
            HttpWebResponse webResponse = null;

            Boolean authSuccess = await BrowserAuthentication();
            PostQuitMessage(0);
        }
        private async Task<Boolean> BrowserAuthentication()
        {
            Boolean AuthSuccess = false;

            this.Code_verifier = GenerateUniqueId();
            this.Nonce = GenerateUniqueId(16);
            this.State = GenerateUniqueId(16);
            this.Code_challenge_method = "S256";
            this.Code_challenge = CodeChallenge(Code_verifier);

            string url = string.Format("{0}?response_type={1}&nonce={2}&state={3}&code_challenge={4}&code_challenge_method={5}&client_id={6}&scope={7}&redirect_uri={8}&response_mode={9}&resource={10}&ui_locales={11}",
                        this.Authorization_endpoint,
                        "code%20id_token",
                        this.Nonce,
                        this.State,
                        this.Code_challenge,
                        this.Code_challenge_method,
                        this.Client_id,
                        this.Scope,
                        HttpUtility.UrlEncode(this.Redirect_uri),
                        "form_post",
                        this.Resource,
                        "en-US"
                        );

            EmbeddedBrowser browser = new EmbeddedBrowser();
            AuthResult result = await browser.CreateAuthenticateWindowAsync(url, this.Redirect_uri);

            try
            {
                string[] AuthcodeArray = result.Response.Split('&');
                string returnState_ = "";
                if (result.ResultType == AuthResultType.Success)
                    AuthSuccess = true;
                {
                    foreach (var recStr in AuthcodeArray)
                    {
                        string[] rec = recStr.Split('=');

                        string name_ = rec[0].Trim();
                        string value_ = rec[1].Trim();

                        switch (name_)
                        {
                            case "code":
                                this.Code = value_;
                                break;
                            case "id_token":
                                this.Id_token = value_;
                                break;
                            case "state":
                                returnState_ = value_;
                                break;
                        }
                    }

                }

                if (!this.State.Equals(returnState_))
                    throw new Exception("State returned from IDP does not match.");
                return AuthSuccess;
            }

            // response is null if user closes the auth window or other error
            catch (NullReferenceException ex)
            {
                Console.WriteLine("something wrong with browser authentication.");
                return false;
            }

        }

        private void GetAccessToken()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Token_endpoint);
            HttpWebResponse response = null;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            string postData = string.Format("client_id={0}&redirect_uri={1}&code_verifier={2}&code={3}&grant_type={4}",
                       this.Client_id,
                       HttpUtility.UrlEncode(this.Redirect_uri),
                       HttpUtility.UrlEncode(this.Code_verifier),
                       HttpUtility.UrlEncode(this.Code),
                       "authorization_code");

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            try
            {
                response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    StreamReader strRead = new StreamReader(responseStream);
                    string responseData = strRead.ReadToEnd();

                    Console.WriteLine("----------finally we got the access token!!!");

                    dynamic json = new JavaScriptSerializer().DeserializeObject(responseData);

                    this.Access_token = (String)json["access_token"];
                    this.Refresh_token = (String)json["refresh_token"];
                    this.Token_type = (String)json["token_type"];
                    this.Expires_in = (int)json["expires_in"];


                    Console.WriteLine("----------Access_token " + Access_token);
                    Console.WriteLine("----------Refresh_token " + Refresh_token);
                    Console.WriteLine("----------Token_type " + Token_type);
                    Console.WriteLine("----------Expires_in " + Expires_in);

                    //ParseResponseData(responseData);
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("something wrong while fetching access token.");
            }

        }
         
        //helper methods
        private string GenerateUniqueId(int length = 32)
        {
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);

            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
        private string CodeChallenge(string CodeVerifier)
        {
            string codeChallenge = string.Empty;
            using (var sha256 = SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(CodeVerifier));
                codeChallenge = Base64UrlEncode(challengeBytes);
            }
            return codeChallenge;
        }

        public static string Base64UrlEncode(byte[] arg)
        {
            var s = Convert.ToBase64String(arg); // Standard base64 encoder

            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding

            return s;
        }

        [DllImport("User32.dll")]
        private static extern void PostQuitMessage(Int32 nExitCode);



        // GET
        public string MakeGET(string resourcePath)
        {
            // Most imoportant thing :)
            Authenticate();

            string sRequestUri = this.Ifs_url + "main/ifsapplications/projection" + "/v1/" + resourcePath;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sRequestUri);
            req.Method = "GET";
            req.UserAgent = USER_AGENT_HEADER;
            req.KeepAlive = true;

            req.UnsafeAuthenticatedConnectionSharing = true;

            req.Timeout = this.connectionTimeout;

            req.SendChunked = false;
            req.AllowAutoRedirect = false;
            req.ServicePoint.Expect100Continue = false;
            req.ServicePoint.UseNagleAlgorithm = false;

            HttpWebRequest initReq2 = req;

            string IfsResponse = "";


            if (this.Access_token != null)
            {
                initReq2.KeepAlive = false;
                initReq2.ContentLength = 0;
                initReq2.Headers.Add("Authorization", "Bearer " + this.Access_token);

                try
                {
                    HttpWebResponse response = (HttpWebResponse)initReq2.GetResponse();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("HTTP 200 with Oauth!!!");
                        Stream responseStream = response.GetResponseStream();                        
                        StreamReader strRead = new StreamReader(responseStream);
                        IfsResponse = strRead.ReadToEnd();
                        Console.WriteLine(IfsResponse);
                        
                    }
                }

                catch (Exception)
                {
                    //this.abortLogin = true;
                    throw;
                }

            }
            return IfsResponse;
        }

    }
}
