using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.IO.Compression;
using System.Web;
using System.Security.Cryptography.X509Certificates;

namespace aibRetrieve
{
public class PostValue 
{ 
    public PostValue( String key, String value ) 
    { 
        Key = key; 
        Value = value; 
    } 
 
 
    public String Key { get; set; } 
 
    public String Value { get; set; } 
} 
 
[Serializable] 
public class WebPage 
{ 
    public WebPage(String html) 
    { 
        Html = html; 
    } 
 
    public WebPage(String html, WebPage parent) 
    { 
        Html = html; 
        Parent = parent; 
    } 
 
    public String Html { get; set; } 
    public WebPage Parent { get; set; } 
} 
 
 
internal class AcceptAllCertificatePolicy : ICertificatePolicy 
{ 
    public AcceptAllCertificatePolicy() 
    { 
    } 
 
    public bool CheckValidationResult(ServicePoint sPoint, 
       X509Certificate cert, WebRequest wRequest, int certProb) 
    { 
        // Always accept 
        return true; 
    } 
} 
 
public class WebSession 
{ 
    //storing of bank logon data
    string sLogonID = "25087845";
    string [] sPassKey = new string [5] {"5","2","1","1","2"};
    string[] sArrChallenge = new string[3] { "6404-home", "6539-work", "4001-primary" };

    public String BaseUrl { get; set; } 
    public String LastUrl { get; set; } 
    public String UserAgent { get; set; }

    public string htmlResult; 

    public int PageReattempts { get; set; } 
    
    public WebProxy Proxy { get; set; } 
    public String CookieString { get; set; } 
    public Dictionary<String, String> Cookies { get; set; } 
 
    private static WebSession instance { get; set; } 
    public static WebSession Instance { get { if (instance == null) instance = new WebSession(); return instance; } } 
 
    public const String DefaultAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.0.8) Gecko/2009032609 Firefox/3.0.8"; 
 
    public WebSession()
        : this(DefaultAgent, null) 
    { 
    } 
 
 
    public WebSession(String baseUrl) 
        : this(DefaultAgent, null) 
    { 
        BaseUrl = baseUrl; 
    } 
 
    public WebSession(String userAgent, WebProxy proxy) 
    { 
        ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy(); 
        CookieString = ""; 
        Cookies = new Dictionary<string, string>(); 
 
        if (userAgent == "") 
            UserAgent = DefaultAgent; 
        else 
            UserAgent = userAgent; 
 
        Proxy = proxy; 
        LastUrl = ""; 
        PageReattempts = 4; 
        ServicePointManager.Expect100Continue = false; 
    } 
 
 
    public WebPage RequestPage(string URL) 
    { 
        return RequestPage(new Uri(BaseUrl + URL)); 
    } 
 
    public WebPage RequestPage(string URL, string Values, string Method) 
    { 
        return RequestPage(new Uri(BaseUrl + URL), Values, Method); 
    } 
 
    public WebPage RequestPage(string URL, string Values, string Method, string ContentType) 
    { 
        return RequestPage(new Uri(BaseUrl + URL), Values, Method, "application/x-www-form-urlencoded"); 
    } 
 
    public WebPage RequestPage(Uri URL) 
    { 
        return RequestPage(URL, "", "GET"); 
    } 
 
 
    public WebPage RequestPage(String URL, params PostValue[] postValues) 
    { 
        String totalString = ""; 
 
        if (postValues.Length > 0) 
        { 
            for (int count = 0; count < postValues.Length; count++) 
            { 
                if (count > 0) 
                    totalString += "&"; 
 
                //totalString += postValues[count].Key + "=" + HttHttpUtility.UrlEncode(postValues[count].Value);
                totalString += postValues[count].Key + "=" + postValues[count].Value;
            } 
        } 
 
        return RequestPage(URL, totalString); 
    } 
 
 
    public WebPage RequestPage(string URL, string Values) 
    { 
        return RequestPage(new Uri(BaseUrl + URL), Values); 
    } 
 
 
    public WebPage RequestPage(Uri URL, string Values) 
    { 
        return RequestPage(URL, Values, "POST"); 
    } 
 
 
    public WebPage RequestPage(Uri URL, string Values, string Method) 
    { 
        return RequestPage(URL, Values, Method, "application/x-www-form-urlencoded"); 
    }
    
    public WebPage RequestPage(Uri url, string content, string method, string contentType) 
    { 
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url); 
        HttpWebResponse response = null; 
        ASCIIEncoding encoding = new ASCIIEncoding(); 
        byte[] contentData = encoding.GetBytes(content); 
 
        request.Proxy = Proxy; 
        request.Timeout = 60000; 
        request.Method = method; 
        request.AllowAutoRedirect = false; 
        request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"; 
        request.Referer = LastUrl; 
        request.KeepAlive = false; 
 
        request.UserAgent = UserAgent; 
 
        request.Headers.Add("Accept-Language", "en-us,en;q=0.5"); 
        //request.Headers.Add("UA-CPU", "x86"); 
        request.Headers.Add("Cache-Control", "no-cache"); 
        request.Headers.Add("Accept-Encoding", "gzip,deflate"); 
 
        String cookieString = ""; 
        foreach (KeyValuePair<String, String> cookiePair in Cookies) 
            cookieString += cookiePair.Key + "=" + cookiePair.Value + ";"; 
 
        if (cookieString.Length > 2) 
        { 
            String cookie = cookieString.Substring(0, cookieString.Length - 1); 
            request.Headers.Add("Cookie", cookie); 
        } 
 
        if (method == "POST") 
        {
            request.ContentLength = contentData.Length; 
            request.ContentType = contentType; 
 
            Stream contentWriter = request.GetRequestStream(); 
            contentWriter.Write(contentData, 0, contentData.Length); 
            contentWriter.Close(); 
        } 
 
        int attempts = 0; 
 
        while (true) 
        { 
            try 
            { 
                response = (HttpWebResponse)request.GetResponse(); 
                if (response == null) 
                    throw new WebException(); 
 
                break; 
            } 
            catch (Exception ex) 
            { 
                if (response != null) 
                    response.Close(); 
 
                if (attempts == PageReattempts) 
                    throw; 
 
                // Wait three seconds before trying again 
                Thread.Sleep(3000); 
            } 
 
            attempts += 1; 
        } 
 
        // Tokenize cookies 
        if (response.Headers["Set-Cookie"] != null) 
        { 
            String headers = response.Headers["Set-Cookie"].Replace("path=/,", ";").Replace("HttpOnly,", ""); 
            foreach (String cookie in headers.Split(';')) 
            { 
                if (cookie.Contains("=")) 
                { 
                    String[] splitCookie = cookie.Split('='); 
                    String cookieKey = splitCookie[0].Trim(); 
                    String cookieValue = splitCookie[1].Trim(); 
 
                    if (Cookies.ContainsKey(cookieKey)) 
                        Cookies[cookieKey] = cookieValue; 
                    else 
                        Cookies.Add(cookieKey, cookieValue); 
                } 
                else 
                { 
                    if (Cookies.ContainsKey(cookie)) 
                        Cookies[cookie] = ""; 
                    else 
                        Cookies.Add(cookie, ""); 
                } 
            } 
        } 
 
        htmlResult = ReadResponseStream(response);

        
        response.Close(); 
 
        if (response.Headers["Location"] != null) 
        { 
            response.Close(); 
            Thread.Sleep(1500); 
            String newLocation = response.Headers["Location"]; 
            WebPage result = RequestPage(newLocation); 
            return new WebPage(result.Html, new WebPage(htmlResult)); 
        } 
 
        LastUrl = url.ToString(); 
 
        return new WebPage(htmlResult); 
    }

    public string sToken(string htmlResult)
    {
        string slToken = "";
        slToken = htmlResult.Substring(htmlResult.LastIndexOf("=", htmlResult.IndexOf("/>", htmlResult.IndexOf("transactionToken"))) + 2, 13);
        return slToken;
    }

    public string sAIB_CHALLENGE(string htmlResult)
    {
        string AIB_CHALLENGE_POST = "transactionToken=!token!&pacDetails.pacDigit1=!pac1!&pacDetails.pacDigit2=!pac2!&pacDetails.pacDigit3=!pac3!&challengeDetails.challengeEntered=!challenge!&_finish=true";

        int sDigit1 = 0;
        int sDigit2 = 0;
        int sDigit3 = 0;

        string sChallenge = "";

        int.TryParse(htmlResult.Substring(htmlResult.IndexOf("/", htmlResult.IndexOf(@"for=""digit1""><")) - 2, 1),out sDigit1);
        int.TryParse(htmlResult.Substring(htmlResult.IndexOf("/", htmlResult.IndexOf(@"for=""digit2""><")) - 2, 1), out sDigit2);
        int.TryParse(htmlResult.Substring(htmlResult.IndexOf("/", htmlResult.IndexOf(@"for=""digit3""><")) - 2, 1), out sDigit3);
        
        sChallenge = htmlResult.Substring(htmlResult.IndexOf("/", htmlResult.IndexOf(@"for=""challenge"">")) - 25, 69).Replace("<strong>", "").Replace("</strong>", "");
        
        AIB_CHALLENGE_POST = AIB_CHALLENGE_POST.Replace("!pac1!",sPassKey[sDigit1-1]).Replace("!pac2!",sPassKey[sDigit2-1]).Replace("!pac3!",sPassKey[sDigit3-1]);

        for (int i = 0; i < 3; i++)
        {
            if (sChallenge.Contains(sArrChallenge[i].ToString().Substring(5,4)))
                AIB_CHALLENGE_POST = AIB_CHALLENGE_POST.Replace("!challenge!", sArrChallenge[i].ToString().Substring(0, 4));
        }
        
        AIB_CHALLENGE_POST = AIB_CHALLENGE_POST.Replace("!token!",sToken(htmlResult));

        return AIB_CHALLENGE_POST;
    }

    public string ReadResponseStream(HttpWebResponse response) 
    { 
        Stream responseStream = null; 
        StreamReader reader = null; 
 
        try 
        { 
            responseStream = response.GetResponseStream(); 
            responseStream.ReadTimeout = 5000; 
 
            if (response.ContentEncoding.ToLower().Contains("gzip")) 
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress); 
            else if (response.ContentEncoding.ToLower().Contains("deflate")) 
                responseStream = new DeflateStream(responseStream, CompressionMode.Decompress); 
 
            reader = new StreamReader(responseStream); 
 
            return reader.ReadToEnd(); 
        } 
        finally 
        { 
            reader.Close(); 
            responseStream.Close(); 
        } 
    } 
} 
    }

