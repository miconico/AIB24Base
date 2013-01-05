using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIBRetrieve_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Meteor oMeteor = new Meteor();
            AIBWebSession AIB24 = new AIBWebSession();

            string[] sPassKey = new string[5] { "5", "2", "1", "1", "2" };
            string[] sArrChallenge = new string[3] { "6404-home", "6539-work", "4001-primary" };

            AIB24.sPassKey = sPassKey;
            AIB24.sArrChallenge = sArrChallenge;
            AIB24.AIBInitLogon();
            AIB24.AIBFirstChallenge("25087845");
            AIB24.AIBLastChallenge();
            //transactionToken=1307639196941&iBankFormSubmission=false&x=64&y=8 
            string sToken = AIB24.sToken(AIB24.htmlResult);
            //sToken = (long.Parse(sToken) + 1).ToString();
            AIB24.RequestPage("https://aibinternetbanking.aib.ie/inet/roi/accountoverview.htm", "transactionToken=" + sToken + "&iBankFormSubmission=false&x=64&y=8", "post");
            
            if (oMeteor.bLogin("0877453339", "2508"))
            {
                oMeteor.bSendText("0877453339", AIB24.sParseAccountSMS());
                oMeteor.bSendText("0863032824", AIB24.sParseAccountSMS());
            }

            sPassKey = new string[5] { "2", "7", "1", "0", "8" };
            sArrChallenge = new string[3] { "2824-home", "2824-work", "2824-primary" };

            AIB24.sPassKey = sPassKey;
            AIB24.sArrChallenge = sArrChallenge;
            AIB24.AIBInitLogon();
            AIB24.AIBFirstChallenge("56505148");
            AIB24.AIBLastChallenge();

            if (oMeteor.bLogin("0877453339", "2508"))
            {
                oMeteor.bSendText("0877453339", AIB24.sParseAccountSMS());
                oMeteor.bSendText("0863032824", AIB24.sParseAccountSMS());
            }


        }
    }
}
