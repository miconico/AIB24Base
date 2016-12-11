using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.IO.Compression;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Data.SqlClient;
using System.Data;

namespace aibRetrieve
{
    public class PostValue
    {
        public PostValue(String key, String value)
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

    public class AIBWebSession
    {
        //storing of bank logon data
        public string[] sPassKey = new string[5];
        public string[] sArrChallenge = new string[3];

        static string HTTP_METHOD_POST = "POST";
        //static string AIB_LOGIN = "https://aibinternetbanking.aib.ie/inet/roi/login.htm";
        static string AIB_LOGIN = "https://onlinebanking.aib.ie/inet/roi/login.htm";
        static string AIB_POST_FIRST = "_target1=true&jsEnabled=TRUE&regNumber=!regnumber!&transactionToken=";
        static string TRANS_TOKEN = "transactionToken=";
        static string AIB_POST_TWO = "jsEnabled=TRUE&transactionToken=&pacDetails.pacDigit1=1&pacDetails.pacDigit2=5&pacDetails.pacDigit3=2&challengeDetails.challengeEntered=6404&_finish=true";
                                      
        static string AIB_EXPAND_ACC = "https://aibinternetbanking.aib.ie/inet/roi/accountoverview.htm";
        static string AIB_EXPAND_POST = "&iBankFormSubmission=true&dsAccountListIndex=0&nonDSIndex=0";
        static string AIB_FULL_EXPAND = "https://aibinternetbanking.aib.ie/inet/roi/statement.htm";
        static string AIB_FULL_EXPAND_POST = "&index=0&viewAllRecentTransactions=view+all+recent+transactions";
        static string AIB_STATEMENT = "https://aibinternetbanking.aib.ie/inet/roi/statement.htm";
        static string AIB_LOGOUT = "https://onlinebanking.aib.ie/inet/roi/logout.htm";

        public String BaseUrl { get; set; }
        public String LastUrl { get; set; }
        public String UserAgent { get; set; }

        public string htmlResult;
        public string sCreditTracker = "";
        public string sDebitTracker = "";

        public ArrayList arrAccounts = new ArrayList();

        public int PageReattempts { get; set; }

        public WebProxy Proxy { get; set; }
        public String CookieString { get; set; }
        public Dictionary<String, String> Cookies { get; set; }

        private static AIBWebSession instance { get; set; }
        public static AIBWebSession Instance { get { if (instance == null) instance = new AIBWebSession(); return instance; } }

        public const String DefaultAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.0.8) Gecko/2009032609 Firefox/3.0.8";

        public AIBWebSession()
            : this(DefaultAgent, null)
        {
        }


        public AIBWebSession(String baseUrl)
            : this(DefaultAgent, null)
        {
            BaseUrl = baseUrl;
        }

        public AIBWebSession(String userAgent, WebProxy proxy)
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

        public bool AIBInitLogon()
        {
            RequestPage(AIB_LOGIN);
            return true;
        }

        public bool AIBFirstChallenge(string sRegNumber)
        {
            RequestPage(AIB_LOGIN, AIB_POST_FIRST.Replace("!regnumber!",sRegNumber) + sToken(htmlResult), HTTP_METHOD_POST);
            return true;
        }

        public bool AIBPINChallenge()
        {
            RequestPage(AIB_LOGIN, sAIB_PIN_CHALLENGE(htmlResult), HTTP_METHOD_POST);
            return true;
        }

        public bool AIBLastChallenge()
        {
            RequestPage(AIB_LOGIN, sAIB_CHALLENGE(htmlResult), HTTP_METHOD_POST);
            return true;
        }

        public bool AIBExpandACC()
        {
            RequestPage(AIB_EXPAND_ACC, TRANS_TOKEN + sToken(htmlResult) + AIB_EXPAND_POST, HTTP_METHOD_POST);
            return true;
        }

        public bool AIBFullExpandACC()
        {
            RequestPage(AIB_FULL_EXPAND, TRANS_TOKEN + sToken(htmlResult) + AIB_FULL_EXPAND_POST, HTTP_METHOD_POST);
            return true;
        }

        public bool AIBLogout()
        {
            RequestPage(AIB_LOGOUT, TRANS_TOKEN + sToken(htmlResult), HTTP_METHOD_POST);
            return true;
        }   

        public WebPage RequestPage(Uri url, string content, string method, string contentType)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse response = null;
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] contentData = encoding.GetBytes(content);

            System.Net.WebProxy proxy = System.Net.WebProxy.GetDefaultProxy();
            proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

            request.Proxy = proxy;
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

        public string sParseAccTrans()
        {
            // assign account trans html 
            string sParseHTML = htmlResult;

            // concat all the debits/credits
            string sAccountData = "";
            sDebitTracker = "";
            sCreditTracker = "";

            // create the DB class
            clsAccountsDB oDB = new clsAccountsDB();

            while (sParseHTML.Contains("<tbody>"))
            {
                string sTable = sParseHTML.Substring(sParseHTML.IndexOf("<tbody>"), sParseHTML.IndexOf("</tbody>", sParseHTML.IndexOf("<tbody>")) - sParseHTML.IndexOf("<tbody>"));
                sParseHTML = sParseHTML.Replace(sTable, "");

                int iStartIndex = sTable.IndexOf("<td", 0);

                do
                {
                    string sDate = "";
                    string sDesc = "";
                    string sDebit = "";
                    string sCredit = "";
                    string sBalance = "";

                    sDate = sTable.Substring(iStartIndex, (sTable.IndexOf("</td>", iStartIndex) - iStartIndex) + 5);
                    sDate = sDate.Substring(sDate.IndexOf(">") + 1, sDate.IndexOf("<", 1) - sDate.IndexOf(">") - 1);

                    iStartIndex = sTable.IndexOf("<td", iStartIndex + 1);
                    if (iStartIndex > 0)
                    {
                        sDesc = sTable.Substring(iStartIndex, (sTable.IndexOf("</td>", iStartIndex) - iStartIndex) + 5);
                        sDesc = sDesc.Substring(sDesc.IndexOf(">") + 1, sDesc.IndexOf("<", 1) - sDesc.IndexOf(">") - 1);
                    }
                    iStartIndex = sTable.IndexOf("<td", iStartIndex + 1);
                    if (iStartIndex>0)
                        sDebit = sTable.Substring(iStartIndex, (sTable.IndexOf("</td>", iStartIndex) - iStartIndex) + 5);
                    if (iStartIndex>0)
                        sDebit = sDebit.Substring(sDebit.IndexOf(">") + 1, sDebit.IndexOf("<", 1) - sDebit.IndexOf(">") - 1);

                    try
                    {
                        iStartIndex = sTable.IndexOf("<td", iStartIndex + 1);
                        sCredit = sTable.Substring(iStartIndex, (sTable.IndexOf("</td>", iStartIndex) - iStartIndex) + 5);
                        sCredit = sCredit.Substring(sCredit.IndexOf(">") + 1, sCredit.IndexOf("<", 1) - sCredit.IndexOf(">") - 1);
                    }
                    catch (Exception ex)
                    { 
                    }
                    try
                    {
                        iStartIndex = sTable.IndexOf("<td", iStartIndex + 1);
                        sBalance = sTable.Substring(iStartIndex, (sTable.IndexOf("</td>", iStartIndex) - iStartIndex) + 5);
                        sBalance = sBalance.Substring(sBalance.IndexOf(">") + 1, sBalance.IndexOf("<", 1) - sBalance.IndexOf(">") - 1).Replace("&nbsp;", "").Trim();
                    }
                    catch (Exception ex)
                    { }

                    if (sDebit.Length == 0)
                        sDebit = "0";
                    if (sCredit.Length == 0)
                        sCredit = "0";
                    if (sBalance.ToUpper().Contains("DR"))
                        sBalance = "-" + sBalance.Replace("DR", "");
                    if (sBalance.Length == 0)
                        sBalance = "0";

                    string[] sAccData = new string[2];
                    sAccData = (string[])arrAccounts[0];

                    double d = 0;
                    bool bIsNumericD = double.TryParse(sDebit, out d);
                    bool bIsNumericC = double.TryParse(sCredit, out d);

                    if ((bIsNumericD && sDebit.Contains(".")) || (bIsNumericC && sCredit.Contains(".")))
                    {
                        if (sDate.Contains(@"/"))
                        {
                            sAccountData += sDate + "::" + sDesc + "::";
                            if (sDebit != "0") sAccountData += sDebit + "DR" + Environment.NewLine + "~"; else sAccountData += sCredit + "CR" + Environment.NewLine + "~";
                        }
                    }

                    iStartIndex = sTable.IndexOf("<td", iStartIndex + 1);

                    if (iStartIndex == -1)
                    {
                        sBalance = sAccData[1].ToString().Replace("&nbsp;", "");
                        if (sBalance.ToUpper().Contains("DR"))
                            sBalance = "-" + sBalance.Replace("DR", "");
                        //oDB.bInsertDetails(sAccData[0].ToString(), DateTime.Today.ToString(), "Balance", "0", "0", sBalance);
                    }
                } while (iStartIndex > -1);
            }

            string[] sSort = sAccountData.Split('~');
            List<string> sTags = new List<string>();

            SqlConnection oConn = new SqlConnection();
            SqlCommand cmdRead;
            SqlDataReader oReader = null;
            try
            { 
                oConn.ConnectionString = @"Data Source=woad.arvixe.com;Initial Catalog=budget;User ID=budget;Password=Ireland1;";

                cmdRead = new SqlCommand("SELECT * FROM BudgetTags", oConn);
                cmdRead.CommandType = CommandType.Text;
                oConn.Open();
                oReader = cmdRead.ExecuteReader();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                        sTags.Add(oReader["Tag"].ToString());
                }

            }
            catch (Exception ex)
            { 
                
            }
            for (int i=sSort.Length-1;i>0;i--)
            {
                if (sSort[i] != "")
                {
                    if (sSort[i].ToString().Contains("DR"))
                        sDebitTracker += sSort[i];
                    else
                        sCreditTracker += sSort[i];
                }
                if ((sSort[i] != ""))
                {
                    try
                    {
                        SqlConnection oTagConn = new SqlConnection();
                        SqlCommand oTagCom = null;
                        oTagConn.ConnectionString = @"Data Source=woad.arvixe.com;Initial Catalog=budget;User ID=budget;Password=Ireland1;";

                        oTagCom = new SqlCommand("spiBudgetTag", oTagConn);

                        oTagCom.CommandType = CommandType.StoredProcedure;

                        oTagCom.Parameters.AddWithValue("@Tag", sSort[i].Split(':')[2]);
                        oTagCom.Parameters.AddWithValue("@Description", "Not Tagged");
                        oTagCom.Parameters.AddWithValue("@IncludeCalc", 1);
                        oTagCom.Parameters.AddWithValue("@TotalSpend", (sSort[i].Contains("DR")?"-"+sSort[i].Split(':')[4].Replace("DR\r\n",""):sSort[i].Split(':')[4].Replace("CR\r\n","")));
                        oTagCom.Parameters.AddWithValue("@Date", sSort[i].Split(':')[0]);
                        oTagCom.Parameters.AddWithValue("@Active", 1);

                        if (!(oTagConn.State == ConnectionState.Open))
                            oTagConn.Open();
                        oTagCom.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    { }
                }
            }

            return "[Money Spent]" + Environment.NewLine + sDebitTracker + Environment.NewLine + "[Money In]" + Environment.NewLine + sCreditTracker + Environment.NewLine + Environment.NewLine + Environment.NewLine;
        }

        public string sParseAccountSMS(string sAcc = "")
        {
            string sParseHTML = htmlResult;
            string sSpanData = "";
            string sSMS = "";

            try
            {

                while (sParseHTML.Contains("<span>"))
                {
                    //addAccounts.add        
                    sSpanData = sParseHTML.Substring(sParseHTML.IndexOf("<span>") + 6, (sParseHTML.IndexOf(@"</span>", sParseHTML.IndexOf(@"<span>"))) - sParseHTML.IndexOf("<span>") - 6);
                    
                    if (sSpanData.Contains("-"))
                    {
                        int iStartPoint = sParseHTML.IndexOf(sSpanData);
                        //string sBalance = sParseHTML.Substring(sParseHTML.IndexOf("<h3>", iStartPoint) + 4, (sParseHTML.IndexOf("</h3>", iStartPoint) - sParseHTML.IndexOf("<h3>", iStartPoint) - 4)).Replace("\t", "").Replace("\r", "").Replace("\n", "").Replace("&nbsp;", "");
                        string sBalance = sParseHTML.Substring(sParseHTML.IndexOf(@"<em>", iStartPoint), 100).Replace(" ", "").Replace("\n\t", "").Replace("<em>", "");
                        if (sBalance.ToUpper().Contains("DR"))
                            sBalance = "-" + sBalance.Replace("DR", "");

                        string[] sAccData = new string[2] { sSpanData, sBalance };

                        arrAccounts.Add(sAccData);
                        if (sAcc == "")
                            sSMS += sSpanData + ":" + sBalance + Environment.NewLine;//%0A line break in SMS
                        else
                        {
                            if (sSpanData.Contains(sAcc))
                                sSMS += sSpanData + ":" + sBalance + Environment.NewLine;//%0A line break in SMS
                        }
                      }
                    sParseHTML = sParseHTML.Replace(sParseHTML.Substring(sParseHTML.IndexOf("<span>"), sParseHTML.IndexOf("<span>", sParseHTML.IndexOf("<span>") + 6) - sParseHTML.IndexOf("<span>")), "");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return sSMS;
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

            int.TryParse(htmlResult.Substring(htmlResult.IndexOf("/", htmlResult.IndexOf(@"for=""digit1Text""><")) - 2, 1), out sDigit1);
            int.TryParse(htmlResult.Substring(htmlResult.IndexOf("/", htmlResult.IndexOf(@"for=""digit2Text""><")) - 2, 1), out sDigit2);
            int.TryParse(htmlResult.Substring(htmlResult.IndexOf("/", htmlResult.IndexOf(@"for=""digit3Text""><")) - 2, 1), out sDigit3);

            if (htmlResult.IndexOf(@"for=""challengeText"">")!=-1)
            {
                sChallenge = htmlResult.Substring(htmlResult.IndexOf("/", htmlResult.IndexOf(@"for=""challengeText"">")) - 25, 69).Replace("<strong>", "").Replace("</strong>", "");
            }

            AIB_CHALLENGE_POST = AIB_CHALLENGE_POST.Replace("!pac1!", sPassKey[sDigit1 - 1]).Replace("!pac2!", sPassKey[sDigit2 - 1]).Replace("!pac3!", sPassKey[sDigit3 - 1]);

            for (int i = 0; i < 3; i++)
            {
                if (sChallenge.ToLower().Contains(sArrChallenge[i].ToString().Substring(5, 4)))
                    AIB_CHALLENGE_POST = AIB_CHALLENGE_POST.Replace("!challenge!", sArrChallenge[i].ToString().Substring(0, 4));
            }

            
                AIB_CHALLENGE_POST = AIB_CHALLENGE_POST.Replace("!token!", sToken(htmlResult));
            
            
            return AIB_CHALLENGE_POST;
        }

        public string sAIB_PIN_CHALLENGE(string htmlResult)
        {
            //string AIB_CHALLENGE_POST = "transactionToken=!token!&pacDetails.pacDigit1=!pac1!&pacDetails.pacDigit2=!pac2!&pacDetails.pacDigit3=!pac3!";
            string AIB_CHALLENGE_POST = "jsEnabled=TRUE&transactionToken=!token!&pacDetails.pacDigit1=!pac1!&pacDetails.pacDigit2=!pac2!&pacDetails.pacDigit3=!pac3!&_finish=true";

            int sDigit1 = 0;
            int sDigit2 = 0;
            int sDigit3 = 0;

            string sChallenge = "";

            int.TryParse(htmlResult.Substring(htmlResult.IndexOf("/", htmlResult.IndexOf(@"for=""digit1Text""><")) - 2, 1), out sDigit1);
            int.TryParse(htmlResult.Substring(htmlResult.IndexOf("/", htmlResult.IndexOf(@"for=""digit2Text""><")) - 2, 1), out sDigit2);
            int.TryParse(htmlResult.Substring(htmlResult.IndexOf("/", htmlResult.IndexOf(@"for=""digit3Text""><")) - 2, 1), out sDigit3);

            if (htmlResult.IndexOf(@"for=""challengeText"">") != -1)
            {
                sChallenge = htmlResult.Substring(htmlResult.IndexOf("/", htmlResult.IndexOf(@"for=""challengeText"">")) - 25, 69).Replace("<strong>", "").Replace("</strong>", "");
            }

            AIB_CHALLENGE_POST = AIB_CHALLENGE_POST.Replace("!pac1!", sPassKey[sDigit1 - 1]).Replace("!pac2!", sPassKey[sDigit2 - 1]).Replace("!pac3!", sPassKey[sDigit3 - 1]);

            for (int i = 0; i < 3; i++)
            {
                if (sChallenge.ToLower().Contains(sArrChallenge[i].ToString().Substring(5, 4)))
                    AIB_CHALLENGE_POST = AIB_CHALLENGE_POST + "&challengeDetails.challengeEntered=!challenge!".Replace("!challenge!", sArrChallenge[i].ToString().Substring(0, 4));
            }

            AIB_CHALLENGE_POST = AIB_CHALLENGE_POST.Replace("!token!", sToken(htmlResult));

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
        public string sGetSpend(string sTotals)
        {
            string sReturn = "";
            double Current188 = 0;
            double Savings261 = 0;
            double Savings345 = 0;
            double OnlineSavings501 = 0;
            double Current076 = 0;
            double OnlineSavings159 = 0;

            int iStartIndex = -1;
            int iEndIndex = -1;

            iStartIndex = sTotals.IndexOf("CURRENT-188:");
            iEndIndex = sTotals.IndexOf("\r", iStartIndex + 12);
            Current188 = double.Parse(sTotals.Substring(iStartIndex + 12, (iEndIndex - (iStartIndex + 12))));

            iStartIndex = sTotals.IndexOf("SAVINGS-261:");
            iEndIndex = sTotals.IndexOf("\r", iStartIndex + 12);
            Savings261 = double.Parse(sTotals.Substring(iStartIndex + 12, (iEndIndex - (iStartIndex + 12))));

            iStartIndex = sTotals.IndexOf("SAVINGS-345:");
            iEndIndex = sTotals.IndexOf("\r", iStartIndex + 12);
            Savings345 = double.Parse(sTotals.Substring(iStartIndex + 12, (iEndIndex - (iStartIndex + 12))));

            iStartIndex = sTotals.IndexOf("ONLINE SAVINGS-501:");
            iEndIndex = sTotals.IndexOf("\r", iStartIndex + 19);
            OnlineSavings501 = double.Parse(sTotals.Substring(iStartIndex + 19, (iEndIndex - (iStartIndex + 19))));
            
            iStartIndex = sTotals.IndexOf("CURRENT-076:");
            iEndIndex = sTotals.IndexOf("\r", iStartIndex + 12);
            Current076 = double.Parse(sTotals.Substring(iStartIndex + 12, (iEndIndex - (iStartIndex + 12))));

            iStartIndex = sTotals.IndexOf("ONLINE SAVINGS-159:");
            iEndIndex = sTotals.IndexOf("\r", iStartIndex + 19);

            double TotalIn = Current188 + Savings261 + Savings345 + OnlineSavings501 + Current076 + OnlineSavings159;
            double Spend = 0;
            DateTime dt = DateTime.Now;
            int Days = 0;
            double MarkSpend = 0;
            double SandeeSpend = 0;
            double DailySpend = 0;
            double DailyMarkSpend = 0;
            double DailySandeeSpend = 0;
            SqlConnection oConn = new SqlConnection();
            SqlCommand command;
            SqlCommand cmdRead;

            oConn.ConnectionString = @"Data Source=woad.arvixe.com;Initial Catalog=budget;User ID=budget;Password=Ireland1;";

            cmdRead = new SqlCommand("SELECT * FROM BudgetSpend order by balancedate desc", oConn);
            cmdRead.CommandType = CommandType.Text;
            oConn.Open();
            SqlDataReader oReader = cmdRead.ExecuteReader();

            if (oReader.HasRows)
            {
                oReader.Read();
                Spend = Math.Round((TotalIn - double.Parse(oReader["AccountBalance"].ToString())), 2, MidpointRounding.ToEven);
                dt = DateTime.Parse(oReader["BalanceDate"].ToString());
                TimeSpan ts = DateTime.Now - dt;
                Days = ts.Days;
                
                MarkSpend = (Current188 + Savings261 + Savings345 + OnlineSavings501) - (double.Parse(oReader["Current188"].ToString()) + double.Parse(oReader["Savings261"].ToString()) + double.Parse(oReader["Savings345"].ToString()) + double.Parse(oReader["OnlineSavings501"].ToString()));
                SandeeSpend = (Current076 + OnlineSavings159) - (double.Parse(oReader["Current076"].ToString()) + double.Parse(oReader["Savings261"].ToString()));

                if (Days > 0)
                {
                    DailySpend = Math.Round((Spend / Days), 2, MidpointRounding.ToEven);
                    DailyMarkSpend = Math.Round((MarkSpend / Days), 2, MidpointRounding.ToEven);
                    DailySandeeSpend = Math.Round((SandeeSpend / Days), 2, MidpointRounding.ToEven);
                }       
            }
            command = new SqlCommand("spiGetBalance", oConn);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Current188", Current188);
            command.Parameters.AddWithValue("@Savings261", Savings261);
            command.Parameters.AddWithValue("@Savings345", Savings345);
            command.Parameters.AddWithValue("@OnlineSavings501", OnlineSavings501);
            command.Parameters.AddWithValue("@Current076", Current076);
            command.Parameters.AddWithValue("@OnlineSavings159", OnlineSavings159);
            command.Parameters.AddWithValue("@AccountBalance", Math.Round(TotalIn,2,MidpointRounding.ToEven));
            command.Parameters.AddWithValue("@MoneyIn", Math.Round(Spend, 2, MidpointRounding.ToEven));
            command.Parameters.AddWithValue("@Spend", Math.Round(Spend, 2, MidpointRounding.ToEven));

          
            if (!(oConn.State == ConnectionState.Open))
                oConn.Open();
            if (!oReader.IsClosed)
                oReader.Close();
            command.ExecuteNonQuery();

            return "Spend: " + Spend.ToString() + "| Marks Spend: " + MarkSpend.ToString() + "| Sandees Spend: " + SandeeSpend.ToString() ;
        }
    }
}

