using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Empress.Data.EmpressProvider;

namespace MoneyTransferOop
{
    class MoneyTransferException : Exception
    {
        public virtual string Message { get; }
        public MoneyTransferException(int code)
        {
            if (code == 20117)
            {
                Message = "TRANSACTION STILL IN PROGRESS";
            }
            else if (code == 2388)
            {
                Message = "TRANSACTION FAILED, BALANCE IS OUT OT BOUNDS";
            }
            else if (code == 2608)
            {
                Message = "TRANSACTION FAILED, ACCOUNT DOES NOT EXIST";
            }
            else if (code == 20101)
            {
                Message = "ACCOUNT IS CURRENTLY LOCKED, PLEASE TRY AGAIN";
            }
            else
            {
                Message = "Unhandled exception";
            }

        }

    }
}
