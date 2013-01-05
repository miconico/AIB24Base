using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace aibRetrieve
{
    class clsAccountsDB
    {
        public bool bUpdateSnapshots(string sAccID, string sBalance)
        {
            SqlConnection oConn = new SqlConnection();
            SqlCommand command;

            oConn.ConnectionString = @"Data Source=YOUR-AD3E76B51B\SQLEXPRESS;Initial Catalog=aibBudget;Integrated Security=SSPI;";
            command = new SqlCommand("spiuAccountSnapshots", oConn);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@AccID", sAccID);
            command.Parameters.AddWithValue("@Balance", sBalance);

            oConn.Open();
            SqlDataReader reader = command.ExecuteReader();

            return true;
        }
        public bool bInsertDaily(string sAcc, string sBalance)
        {
            SqlConnection oConn = new SqlConnection();
            oConn.ConnectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename='C:\Documents and Settings\Mark Gavin\My Documents\Visual Studio 2008\Projects\aibRetrieve\DB\aibRetrieve.mdf';Integrated Security=True;Connect Timeout=30;User Instance=True";
            SqlCommand command = new SqlCommand("spiAccountSnapshots", oConn);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = 1;
            oConn.Open();
            SqlDataReader reader = command.ExecuteReader();

            return true;
        }

        public bool bInsertDetails(string sAccID, string sDate, string sDesc, string sDebit, string sCredit, string sBalance)
        {
            SqlConnection oConn = new SqlConnection();
            SqlCommand command;

            oConn.ConnectionString = @"Data Source=YOUR-AD3E76B51B\SQLEXPRESS;Initial Catalog=aibBudget;Integrated Security=SSPI;";
            command = new SqlCommand("spiAccountData", oConn);
            
            command.CommandType = CommandType.StoredProcedure;
            
            command.Parameters.AddWithValue("@AccID", sAccID);
            if (sDate.Length < 10)
            {
                DateTime dt = new DateTime(int.Parse("20" + sDate.Substring(6, 2)), int.Parse(sDate.Substring(3, 2)), int.Parse(sDate.Substring(0, 2)));
                command.Parameters.AddWithValue("@Date", dt);
            }
            else
                command.Parameters.AddWithValue("@Date", DateTime.Parse(sDate));
            command.Parameters.AddWithValue("@Desc", sDesc);
            command.Parameters.AddWithValue("@Debit", sDebit);
            command.Parameters.AddWithValue("@Credit", sCredit);
            command.Parameters.AddWithValue("@Balance", sBalance);

            oConn.Open();
            SqlDataReader reader = command.ExecuteReader();

            return true;
        }
    }
}
