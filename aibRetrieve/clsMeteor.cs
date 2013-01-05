using System;
using System.Text;
using System.Net;
using System.IO;

namespace aibRetrieve
{
    public class Meteor
    {
        Cookie cookies;
        HttpWebRequest req;
        CookieContainer cookieContainer;

        public Meteor()
        {
            cookies = new Cookie();
            cookies.Domain = "http://www.meteor.ie";
            cookies.Name = "meteor";
            cookieContainer = new CookieContainer();
        }


        public bool bLogin(String u, String p)
        {
            req = WebRequest.Create("https://www.mymeteor.ie/go/mymeteor-login-manager") as HttpWebRequest;

            System.Net.WebProxy proxy = System.Net.WebProxy.GetDefaultProxy();
            proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
        
            req.CookieContainer = cookieContainer;
            req.Proxy = proxy;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            // add POST data
            string reqString = "username=" + u + "&userpass=" + p + "&x=19&y=13&returnTo=%2F";

            byte[] reqData = Encoding.UTF8.GetBytes(reqString);
            req.ContentLength = reqData.Length;

            // send request
            using (Stream reqStream = req.GetRequestStream())
                reqStream.Write(reqData, 0, reqData.Length);

            string response;

            // retrieve response
            using (WebResponse res = req.GetResponse())
            using (Stream resSteam = res.GetResponseStream())
            using (StreamReader sr = new StreamReader(resSteam))

                response = sr.ReadToEnd();

            if (response.IndexOf("Invalid login") > 0)
            {
                return false;
            }
            else if (response.IndexOf("<h3>My Account</h3>") > 0)
            {
                return true;
            }
            else if (response.IndexOf("additional security") > 0)
            {
                return true;
            }
            else
            {
                return false;	//could not connect to site
            }

        }

        public bool bSendText(string sTo, string sToMessage)
        {
            string sURL = "https://www.mymeteor.ie/go/freewebtext";

            System.Net.WebProxy proxy = System.Net.WebProxy.GetDefaultProxy();
            proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

            req = WebRequest.Create(sURL) as HttpWebRequest;
            req.Proxy = proxy;
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = cookieContainer;

            string response = "";

            // retrieve response
            using (WebResponse res = req.GetResponse())
            using (Stream resSteam = res.GetResponseStream())
            using (StreamReader sr = new StreamReader(resSteam))

                response = sr.ReadToEnd();


            sURL = "https://www.mymeteor.ie/mymeteorapi/index.cfm?event=smsAjax&func=addEnteredMsisdns&ajaxRequest=addEnteredMSISDNs&remove=-&add=111111|" + sTo.ToString() + "|no%20one";

            req = WebRequest.Create(sURL) as HttpWebRequest;
        
            req.Proxy = proxy;
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = cookieContainer;

            // retrieve response
            using (WebResponse res = req.GetResponse())
            using (Stream resSteam = res.GetResponseStream())
            using (StreamReader sr = new StreamReader(resSteam))

                response = sr.ReadToEnd();

            sURL = "https://www.mymeteor.ie/mymeteorapi/index.cfm";

            string sURLParam = "event=smsAjax&func=sendSMS&ajaxRequest=sendSMS&messageText=" + sToMessage;
            req = WebRequest.Create(sURL) as HttpWebRequest;

            req.CookieContainer = cookieContainer;
            req.Proxy = proxy;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";


            byte[] reqData = Encoding.UTF8.GetBytes(sURLParam);
            req.ContentLength = reqData.Length;

            // send request
            using (Stream reqStream = req.GetRequestStream())
                reqStream.Write(reqData, 0, reqData.Length);

            // retrieve response
            using (WebResponse res = req.GetResponse())
            using (Stream resSteam = res.GetResponseStream())
            using (StreamReader sr = new StreamReader(resSteam))


                //response.GetResponseStream();

                return true;
        }

    }

}
