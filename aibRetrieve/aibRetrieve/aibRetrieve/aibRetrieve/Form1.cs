using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Collections;

namespace aibRetrieve
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        Cookie cookies;
        HttpWebRequest req;
        CookieContainer cookieContainer;

        private clsAccountsDB oDB = new clsAccountsDB();

        static string HTTP_METHOD_POST = "POST";
        static string AIB_LOGIN = "https://aibinternetbanking.aib.ie/inet/roi/login.htm";
        static string AIB_POST_FIRST = "_target1=true&jsEnabled=TRUE&regNumber=25087845&transactionToken=";
        static string TRANS_TOKEN = "transactionToken=";
        static string AIB_POST_TWO = "&pacDetails.pacDigit1=1&pacDetails.pacDigit2=5&pacDetails.pacDigit3=2&challengeDetails.challengeEntered=6404&_finish=true";

        private void Form1_Load(object sender, EventArgs e)
        {
            WebSession ws = new WebSession();
            ws.RequestPage(AIB_LOGIN);
            ws.RequestPage(AIB_LOGIN, AIB_POST_FIRST + ws.sToken(ws.htmlResult), HTTP_METHOD_POST);
            ws.RequestPage(AIB_LOGIN, ws.sAIB_CHALLENGE(ws.htmlResult) , HTTP_METHOD_POST);
            ws.RequestPage("https://aibinternetbanking.aib.ie/inet/roi/accountoverview.htm","transactionToken=" + ws.sToken(ws.htmlResult) + "&iBankFormSubmission=true&dsAccountListIndex=0&nonDSIndex=0",HTTP_METHOD_POST);
            
            string sParseHTML = ws.htmlResult;
            ArrayList arrAccounts = new ArrayList();
            string sSpanData = "";
            string sSMS = "";
            
            try
            {

                while (sParseHTML.Contains("<span>"))
                {
                    //addAccounts.add        
                    sSpanData = sParseHTML.Substring(sParseHTML.IndexOf("<span>") + 6, (sParseHTML.IndexOf("</span>") - sParseHTML.IndexOf("<span>") - 6));
                    if (sSpanData.Contains("-"))
                    {
                        int iStartPoint = sParseHTML.IndexOf(sSpanData);
                        string sBalance = sParseHTML.Substring(sParseHTML.IndexOf("<h3>", iStartPoint) + 4, (sParseHTML.IndexOf("</h3>", iStartPoint) - sParseHTML.IndexOf("<h3>", iStartPoint) - 4)).Replace("\t", "").Replace("\r", "").Replace("\n", "").Replace("&nbsp;", "");
                        sBalance = sBalance.Replace("&nbsp;", "");
                        if (sBalance.ToUpper().Contains("DR"))
                            sBalance = "-" + sBalance.Replace("DR", "");
                        
                        string[] sAccData = new string[2] { sSpanData, sBalance };
                        
                        arrAccounts.Add(sAccData);
                        //sSMS += sSpanData + "%0A";//%0A line break in SMS
                    }
                    sParseHTML = sParseHTML.Replace(sParseHTML.Substring(sParseHTML.IndexOf("<span>"), (sParseHTML.IndexOf("</span>") - sParseHTML.IndexOf("<span>")) + 7), "");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            ws.RequestPage("https://aibinternetbanking.aib.ie/inet/roi/statement.htm", "transactionToken=" + ws.sToken(ws.htmlResult) + "&index=0&viewAllRecentTransactions=view+all+recent+transactions", HTTP_METHOD_POST);
            
            sParseHTML = ws.htmlResult;

            ws.RequestPage("https://aibinternetbanking.aib.ie/inet/roi/logout.htm", "transactionToken=" + ws.sToken(ws.htmlResult) + "&index=0&viewAllRecentTransactions=view+all+recent+transactions", HTTP_METHOD_POST);

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
                    sDesc = sTable.Substring(iStartIndex, (sTable.IndexOf("</td>", iStartIndex) - iStartIndex) + 5);
                    sDesc = sDesc.Substring(sDesc.IndexOf(">") + 1, sDesc.IndexOf("<", 1) - sDesc.IndexOf(">") - 1);

                    iStartIndex = sTable.IndexOf("<td", iStartIndex + 1);
                    sDebit = sTable.Substring(iStartIndex, (sTable.IndexOf("</td>", iStartIndex) - iStartIndex) + 5);
                    sDebit = sDebit.Substring(sDebit.IndexOf(">") + 1, sDebit.IndexOf("<", 1) - sDebit.IndexOf(">") - 1);

                    iStartIndex = sTable.IndexOf("<td", iStartIndex + 1);
                    sCredit = sTable.Substring(iStartIndex, (sTable.IndexOf("</td>", iStartIndex) - iStartIndex) + 5);
                    sCredit = sCredit.Substring(sCredit.IndexOf(">") + 1, sCredit.IndexOf("<", 1) - sCredit.IndexOf(">") - 1);

                    iStartIndex = sTable.IndexOf("<td", iStartIndex + 1);
                    sBalance = sTable.Substring(iStartIndex, (sTable.IndexOf("</td>", iStartIndex) - iStartIndex) + 5);
                    sBalance = sBalance.Substring(sBalance.IndexOf(">") + 1, sBalance.IndexOf("<", 1) - sBalance.IndexOf(">") - 1).Replace("&nbsp;","").Trim();

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
                    oDB.bInsertDetails(sAccData[0].ToString(), sDate, sDesc, sDebit, sCredit, sBalance);
                    
                    iStartIndex = sTable.IndexOf("<td", iStartIndex+1);

                    if (iStartIndex == -1)
                    {
                        sBalance = sAccData[1].ToString().Replace("&nbsp;","");
                        if (sBalance.ToUpper().Contains("DR"))
                            sBalance = "-" + sBalance.Replace("DR", "");
                        oDB.bInsertDetails(sAccData[0].ToString(), DateTime.Today.ToString(), "Balance", "0", "0", sBalance);
                    }
                }while (iStartIndex > -1);
            }

            foreach (string[] Acc in arrAccounts)
            {
                string sBalance = Acc[1];
                sBalance = sBalance.Replace("&nbsp;","");
                if (sBalance.ToUpper().Contains("DR"))
                    sBalance = "-" + sBalance.Replace("DR", "");
                        
                oDB.bUpdateSnapshots(Acc[0], sBalance);
            }

            Meteor oMeteor = new Meteor();

            if (oMeteor.bLogin("0877453339", "2508"))
            {
                oMeteor.bSendText("0877453339", sSMS);
            }

        }   
    }
}
