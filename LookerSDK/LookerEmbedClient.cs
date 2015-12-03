using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;


namespace LookerSDK
{
    public class LookerEmbedClient
    {
        public static String CreateURL(string host, string secret,
                                       string userID, string firstName, string lastName, string userPermissions,
                                       string userModels, string sessionLength, string accessFilters,
                                       string embedURL, string forceLoginLogout, bool hide_title = true)  
        {
            embedURL += (hide_title) ? "?hide_title=true" : "?";

            String path = "/login/embed/" + WebUtility.UrlEncode(embedURL);

            // Calc the secure random number (nonce)
            Guid g = Guid.NewGuid();
            string nonce = string.Format("\"{0}\"", g.ToString().Replace("-", ""));

            // calc the unix timestamp value 
            Int32 unixTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            string time = string.Format("{0}", unixTime);

            string urlToSign = "";
            urlToSign += host.Replace("https://","") + "\n";
            urlToSign += path + "\n";
            urlToSign += nonce + "\n"; 
            urlToSign += time + "\n";
            urlToSign += sessionLength + "\n";
            urlToSign += userID + "\n";
            urlToSign += userPermissions + "\n";
            urlToSign += userModels + "\n";
            urlToSign += accessFilters;

            // Generate the signature using the urlToSign
            string signature =  GetHash(urlToSign, secret);

            
            // you need to %20-encode each parameter before you add to the URL
            String signedURL =  
                    "nonce="              + WebUtility.UrlEncode(nonce) +
                    "&time="               + WebUtility.UrlEncode(time) +
                    "&session_length="     + WebUtility.UrlEncode(sessionLength) +
                    "&external_user_id="   + WebUtility.UrlEncode(userID) +
                    "&permissions="        + WebUtility.UrlEncode(userPermissions) +
                    "&models="             + WebUtility.UrlEncode(userModels) +
                    "&access_filters="     + WebUtility.UrlEncode(accessFilters) +
                    "&signature="          + WebUtility.UrlEncode(signature) +
                    "&first_name="         + WebUtility.UrlEncode(firstName) +
                    "&last_name="          + WebUtility.UrlEncode(lastName) +
                    "&force_logout_login=" + WebUtility.UrlEncode(forceLoginLogout);

            return host + path + '?' + signedURL;
        }

        static string GetHash(string hashText, string SecretKey)
        {
            using (HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(SecretKey)))
            {
                byte[] raw1 = Encoding.UTF8.GetBytes(hashText);
                byte[] hashValue = hmac.ComputeHash(raw1);
                return Convert.ToBase64String(hashValue);
            }
        }
    }
}
