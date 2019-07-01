using System;
using System.Collections.Generic;
using Empress.Data.EmpressProvider;
using System.Data.Common;
using System.Security.Principal;
using System.Text;
using System.IO;

//- Add SQL constraints
//- Add insert table and checks into the transaction
//- Refactor code, put things into methods
//- Get the Error messages to output correctly

namespace MoneyTransferOop
{
    class MoneyTransfer
    {

        static void Main(string[] args)
        {
            MoneyTransferObject obj = new MoneyTransferObject();

            while (true)
            {
                Console.WriteLine("Welcome Teller to the Money Transfer program, what would you like to do? (make/view/exit): ");
                String option = Console.ReadLine();
                if (option.Equals("exit"))
                {
                    obj.close();
                    break;
                }
                else if (option.Equals("view"))
                {
                    //view transactions
                    Console.WriteLine("Please enter the account you wish to view: ");
                    int viewAccount = Int32.Parse(Console.ReadLine());
                    List<ViewObject> viewObjects = obj.viewTransactions(viewAccount);
                    if (viewObjects == null)
                    {
                        Console.WriteLine("Account does not exist");
                    }
                    else if (viewObjects.Count == 0)
                    {
                        Console.WriteLine("Account currently has no transactions");
                    }
                    else
                    {
                        Console.WriteLine("Trans. No      Source_Acct     Source_Name    Dist_Acct      Dist_Name      Amount         Date");
                        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
                        foreach (ViewObject viewObject in viewObjects)
                        {
                            Console.WriteLine("{0,-15}{1,-15} {2,-15}{3,-15}{4,-15}{5,-15}{6,-15}", viewObject.trans_no, viewObject.source_acct, viewObject.source_name, viewObject.dist_acct, viewObject.dist_name, viewObject.amount, viewObject.date);
                        }
                        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
                    }

                } 
                else if (option.Equals("make"))
                {
                    //Make a transaction
                    Console.WriteLine("Please enter the source account number: ");
                    int Source_Acct_No = Int32.Parse(Console.ReadLine());
                    Console.WriteLine("Please enter the distant account number: ");
                    int Distance_Acct_No = Int32.Parse(Console.ReadLine());
                    Console.WriteLine("Please enter the amount to transfer: ");
                    Double Amount = Convert.ToDouble(Console.ReadLine());
                    // Check if 0 < Amount <= 50,000
                    if (Amount <= 0 || Amount > 50000)
                    {
                        Console.WriteLine("TRANSACTION FAILED: Transaction amount " + Amount + " is out of bounds");
                        Console.WriteLine("*************************************************************************************");
                        continue;
                    }
                    //try making transaction, output error message otherwise
                    try
                    {
                        obj.makeTransaction(Source_Acct_No, Distance_Acct_No, Amount);
                        Console.WriteLine("TRANSACTION SUCCESS! $" + Amount + " transferred from Acct: " + Source_Acct_No + " to Acct: " + Distance_Acct_No);                    }
                    catch (MoneyTransferException m)
                    {
                        Console.WriteLine(m.Message);
                    }
                    Console.WriteLine("*************************************************************************************");
                }
            }

        }
    }
}

