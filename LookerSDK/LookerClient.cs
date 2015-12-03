using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace LookerSDK
{
    public class LookerClient
    {
        public string AccessToken { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public int Port { get; set; }
        public string Token { get; set;}

        public string BaseHost
        {
            get { return string.Format("{0}:{1}", this.Host, this.Port); } 
        }

        public LookerClient(string clientId, string secret, string host = "https://randallreilly.looker.com", int port = 19999)
        {
            this.ClientId = clientId;
            this.Secret = secret;
            this.Host = host;
            this.Port = port;
        }

        public bool Alive()
        {
            // TODO: do http call to check http status (200 ok)
            return true;
        }

        public string Login()
        {
            string action = "/api/3.0/login";

            string command = string.Format("{0}{1}?client_id={2}&client_secret={3}", this.BaseHost, action, this.ClientId, this.Secret);

            Dictionary<string, object> data = WebHelper(command, false, "POST");

            this.AccessToken = data["access_token"].ToString();

            return AccessToken;
        }

        public Dictionary<string, object> CreateUser(string firstName, string lastName, string email, string accessModel, string accessField, string accessValue)
        {
            Dictionary<string, object> dictUser = new Dictionary<string, object>();

            dictUser.Add("first_name", firstName);
            dictUser.Add("last_name", lastName);
            //dictUser.Add("email", email);

            //dictUser.Add("access_filters", dictSecurity);

            string body = JsonConvert.SerializeObject(dictUser);

            string action = "/api/3.0/users";
            string command = string.Format("{0}{1}", this.BaseHost, action);


            // Step 1: Create the user with First & Last Name
            Dictionary<string, object> data = this.WebHelper(command, true, "POST", body);

            // Step 2: If that goes well, add the additional attributes
            if (data != null)
            {
                // Get the user id
                string id = data["id"].ToString();
                action = string.Format("/api/3.0/users/{0}/credentials_email", id);


                dictUser.Clear();
                dictUser.Add("email", email);
                body = JsonConvert.SerializeObject(dictUser);

                command = string.Format("{0}{1}?", this.BaseHost, action); 
                Dictionary<string, object> em = this.WebHelper(command, true, "POST", body);

                // Step 3: if you the email attached add the other attributes
                if (em != null)
                {
                    Dictionary<string, string> dictSecurity = new Dictionary<string, string>();
                    dictSecurity.Add("model", accessModel);
                    dictSecurity.Add("field", accessField);
                    dictSecurity.Add("value", accessValue);

                    action = string.Format("/api/3.0/users/{0}/access_filters", id);
                    body = JsonConvert.SerializeObject(dictSecurity);

                    command = string.Format("{0}{1}?", this.BaseHost, action); 

                    Dictionary<string, object> af = this.WebHelper(command, true, "POST", body);

                    if (af != null)
                    {


                    }

                }

            }

            return data;
        }

        public Dictionary<string, object> GetUsersByFirstName(string firstName)
        {
            //https://randallreilly.looker.com:19999/api/3.0/users/search?first_name=Sonny

            string action = "/api/3.0/users/search";
            string searchParams = string.Format("first_name={0}", firstName);

            string command = string.Format("{0}{1}?{2}", this.BaseHost, action, searchParams);

            Dictionary<string, object> data = this.WebHelper(command, true, "GET");

            return data;
        }

        private Dictionary<string, object> WebHelper(string url, bool useToken = true, string method = "GET", string body = "")
        {
            Uri u = new Uri(url);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(u);
            request.Method = method;

            // We don't use the token for login
            if (useToken)
            {
                //request.Headers.Add("token", _access_token);
                request.Headers.Add(HttpRequestHeader.Authorization, "token " + this.AccessToken);
            }

            if ((method == "POST") && (!string.IsNullOrWhiteSpace(body)))
            {
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = encoding.GetBytes(body);

                request.ContentType = "application/json";

                request.ContentLength = bytes.Length;

                Stream postStream = request.GetRequestStream();

                postStream.Write(bytes, 0, bytes.Length);
            }

            DateTime dt = DateTime.Now; //DateTime.Now.ToUniversalTime();

            string dts = dt.ToString();

            request.Credentials = CredentialCache.DefaultCredentials;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream dataStream = response.GetResponseStream();

            StreamReader reader = new StreamReader(dataStream);

            string responseFromServer = reader.ReadToEnd();

            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseFromServer);

            return data;
        }

    }
}
