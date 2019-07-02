using System;
using System.Collections.Generic;
using Empress.Data.EmpressProvider;
using System.Data.Common;
using System.Security.Principal;
using System.Text;
using System.IO;

namespace MoneyTransferOop
{
    class ViewObject
    {

        public int trans_no;
        public int source_acct;
        public string source_name;
        public int dist_acct;
        public string dist_name;
        public double amount;
        public DateTime date;

        public ViewObject(int trans_no, int source_acct, string source_name, int dist_acct, string dist_name, double amount, DateTime date)
        {
            this.trans_no = trans_no;
            this.source_acct = source_acct;
            this.source_name = source_name;
            this.dist_acct = dist_acct;
            this.dist_name = dist_name;
            this.amount = amount;
            this.date = date;
        }

    }
    class MoneyTransferObject
    {
        private EmpressConnection connection;
        private EmpressDataReader rd;
        private EmpressCommand viewCommand;
        private EmpressCommand sourceUpdate;
        private EmpressCommand distUpdate;
        private EmpressCommand transIdUpdate;
        private EmpressCommand insertCommand;
        private EmpressCommand command;

        public MoneyTransferObject()
        {
            connection = getCSConnection();
            connection.Open();
            command = new EmpressCommand();
            command.Connection = connection;
            initializeView();
            initializeMake();
        }

        public void close()
        {
            connection.Close();
        }

        private void initializeMake()
        {
            //Prepare Source Command
            sourceUpdate = new EmpressCommand();
            sourceUpdate.Connection = connection;
            sourceUpdate.CommandText = "UPDATE Customer_Accounts SET Balance TO Balance - @Amount WHERE Acct_No = @Source_Acct_No";

            //Prepare Dist Command
            distUpdate = new EmpressCommand();
            distUpdate.Connection = connection;

            //Prepare IdUpdate Command
            transIdUpdate = new EmpressCommand();
            transIdUpdate.Connection = connection;
            transIdUpdate.CommandText = "SELECT COUNT(*) FROM transactions";
            //transIdUpdate.Prepare();

            //Prepare Insert Commmand
            insertCommand = new EmpressCommand();
            insertCommand.Connection = connection;

        }

        private void initializeView()
        {
            viewCommand = new EmpressCommand();
            viewCommand.Connection = connection;
            viewCommand.CommandText = "SELECT Trans_No, Source_Acct, C2.Acct_Name AS Source_Name, Dist_Acct, C4.Acct_Name AS Dist_Name, Amount, Date FROM transactions"
                           + " INNER JOIN Customer_Accounts AS C2 ON Source_Acct = C2.Acct_No"
                           + " INNER JOIN Customer_Accounts AS C4 ON Dist_Acct = C4.Acct_No"
                           + " WHERE Source_Acct = @viewAccount1 OR Dist_Acct = @viewAccount2 "
                           + " ORDER BY Date DESC";
            EmpressParameter p1 = new EmpressParameter("@viewAccount1");
            EmpressParameter p2 = new EmpressParameter("@viewAccount2");
            viewCommand.Parameters.Add(p1);
            viewCommand.Parameters.Add(p2);
            viewCommand.Prepare();
        }

        public void makeTransaction(int source_acct, int dist_acct, double amount)
        {
            try
            {
                command.Connection = connection;
                command.CommandText = "START WORK";
                command.ExecuteNonQuery();

                sourceUpdate.CommandText = "UPDATE Customer_Accounts SET Balance TO Balance - " + amount + " WHERE Acct_No = " + source_acct;
                sourceUpdate.ExecuteNonQuery();

                distUpdate.CommandText = "UPDATE Customer_Accounts SET Balance TO Balance + " + amount +  " WHERE Acct_No = " + dist_acct;
                distUpdate.ExecuteNonQuery();

                rd = transIdUpdate.ExecuteReader();
                int transId = 0;
                while (rd.Read())
                {
                    transId = rd.GetInt16(0);
                }

                insertCommand.CommandText = "INSERT INTO transactions VALUES ( " + transId + ", " + source_acct + ", " + dist_acct + ", " + amount + ",  CURRENT_TIMESTAMP)";
                insertCommand.ExecuteNonQuery();

                command.CommandText = "COMMIT WORK";
                command.ExecuteNonQuery();
            }
            catch (EmpressException e)
            {
                command.Connection = connection;
                command.CommandText = "ROLLBACK WORK";
                command.ExecuteNonQuery();
                throw new MoneyTransferException(e.ErrorCode);
                //Console.WriteLine("Error code :" + e.ErrorCode);
                //Console.WriteLine("Error message: " + e.Message);
                //Console.WriteLine(e.StackTrace);
            }
            catch (Exception e)
            {
                //Console.WriteLine("Error message: " + e.Message);
                //Console.WriteLine(e.StackTrace);
            }
        }

        public List<ViewObject> viewTransactions(int acct)
        {
            try
            {
                List<ViewObject> viewObjects = new List<ViewObject>();
                if (CheckIfExists(acct) == false)
                {
                    return null;
                }
                viewCommand.Parameters[0].Value = acct;
                viewCommand.Parameters[1].Value = acct;
                rd = viewCommand.ExecuteReader();
                while (rd.Read())
                {
                    ViewObject temp = new ViewObject(rd.GetInt16(0), rd.GetInt16(1), rd.GetString(2).TrimEnd(), rd.GetInt16(3), rd.GetString(4).TrimEnd(), rd.GetDouble(5), rd.GetDateTime(6));
                    viewObjects.Add(temp);
                }
                rd.Close();
                return viewObjects;
            }
            catch (EmpressException e)
            {
                //Console.WriteLine("Error code :" + e.ErrorCode);
                //Console.WriteLine("Error message: " + e.Message);
                //Console.WriteLine(e.StackTrace);
            }
            catch (Exception e)
            {
                //Console.WriteLine("Error message: " + e.Message);
                //Console.WriteLine(e.StackTrace);
            }
            return null;
        }

        private EmpressConnection getCSConnection()
        {
                DbConnectionStringBuilder csb = new DbConnectionStringBuilder();
                csb.ConnectionString = "Server=coral";
                csb.Add("database", "/tmp/BankDb");
                csb.Add("User", "dli");
                csb.Add("Port", 6681);

                EmpressConnection localConnection = new EmpressConnection();
                localConnection.ConnectionString = csb.ToString();
                
                return localConnection;
        }

        private bool CheckIfExists(int Acct_No)
        {
            command.Connection = connection;
            command.CommandText = "SELECT * FROM Customer_Accounts WHERE Acct_No = " + Acct_No;
            EmpressDataReader temp = command.ExecuteReader();
            temp.Read();
            temp.Close();
            if (temp.HasRows == false)
            {
                return false;
            }
            return true;
        }

    }
}
