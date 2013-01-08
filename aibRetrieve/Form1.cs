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
using System.Net.Mail;


namespace aibRetrieve
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }        

        private void Form1_Load(object sender, EventArgs e)
        {            
            AIBWebSession AIB24 = new AIBWebSession();

            string[] sPassKey = new string[5] { "5", "2", "1", "1", "2" };
            string[] sArrChallenge = new string[3] { "6404-home", "6539-work", "4001-primary" };
            string sSMS = "";

            // pass in security details
            AIB24.sPassKey = sPassKey;
            AIB24.sArrChallenge = sArrChallenge;

            // create the session
            AIB24.AIBInitLogon();

            // post the first logon screen
            AIB24.AIBFirstChallenge("25087845");

            // post the challenge screen
            AIB24.AIBLastChallenge();

            // retrieve the sSMS style account data
            sSMS = "Marks Accounts" + Environment.NewLine + Environment.NewLine + AIB24.sParseAccountSMS();            
            
            // do a full expand on main account with all data
            AIB24.AIBFullExpandACC();
            
            // parse the acc data HTML
            sSMS += Environment.NewLine + AIB24.sParseAccTrans();
            
            // Logout of AIB24
            AIB24.AIBLogout();
            
            sPassKey = new string[5] { "2", "7", "1", "0", "8" };
            sArrChallenge = new string[3] { "2824-home", "2824-work", "2824-primary" };

            AIB24.sPassKey = sPassKey;
            AIB24.sArrChallenge = sArrChallenge;
            AIB24.AIBInitLogon();
            AIB24.AIBFirstChallenge("56505148");
            AIB24.AIBLastChallenge();
            AIB24.AIBExpandACC();

            sSMS += Environment.NewLine + "Sandees Accounts" + Environment.NewLine + Environment.NewLine + AIB24.sParseAccountSMS() + Environment.NewLine;
            
            // do a full expand on main account with all data
            AIB24.AIBFullExpandACC();

            // parse the acc data HTML
            sSMS += AIB24.sParseAccTrans();

            string sSpend = AIB24.sGetSpend(sSMS);

            // send email with compiled data
            SendEmail.SendMessage("Budget", sSpend + Environment.NewLine + Environment.NewLine + sSMS, "budget@aviva.ie", "miconico@gmail.com;mark.gavin@aviva.ie", "");

            // Logout of AIB24
            AIB24.AIBLogout();
        }   
    }
    class SendEmail
    {
        /// <summary>
        /// Sends an e-mail message using the designated SMTP mail server.
        /// </summary>
        /// <param name="subject">The subject of the message being sent.</param>
        /// <param name="messageBody">The message body.</param>
        /// <param name="fromAddress">The sender's e-mail address.</param>
        /// <param name="toAddress">The recipient's e-mail address (separate multiple e-mail addresses
        /// with a semi-colon).</param>
        /// <param name="ccAddress">The address(es) to be CC'd (separate multiple e-mail addresses with
        /// a semi-colon).</param>
        /// <remarks>You must set the SMTP server within this method prior to calling.</remarks>
        /// <example>
        /// <code>
        ///   // Send a quick e-mail message
        ///   SendEmail.SendMessage("This is a Test", 
        ///                         "This is a test message...",
        ///                         "noboday@nowhere.com",
        ///                         "somebody@somewhere.com", 
        ///                         "ccme@somewhere.com");
        /// </code>
        /// </example>
        public static void SendMessage(string subject, string messageBody, string fromAddress, string toAddress, string ccAddress)
        {
            MailMessage message = new MailMessage();

            // Set the sender's address
            message.From = new MailAddress(fromAddress);

            // Allow multiple "To" addresses to be separated by a semi-colon
            if (toAddress.Trim().Length > 0)
            {
                foreach (string addr in toAddress.Split(';'))
                {
                    message.To.Add(new MailAddress(addr));
                }
            }

            // Allow multiple "Cc" addresses to be separated by a semi-colon
            if (ccAddress.Trim().Length > 0)
            {
                foreach (string addr in ccAddress.Split(';'))
                {
                    message.CC.Add(new MailAddress(addr));
                }
            }

            // Set the subject and message body text
            message.Subject = subject;
            message.Body = messageBody;

            // TODO: *** Modify for your SMTP server ***
            // Set the SMTP server to be used to send the message
            //client.Host = "76.1.2.186";
            SmtpClient client = new SmtpClient("smtp.gmail.com");

            client.Credentials = new System.Net.NetworkCredential("miconico@gmail.com", "augustchild1");
            // Send the e-mail message
            client.EnableSsl = true;
            client.Send(message);
        }
    }
}
