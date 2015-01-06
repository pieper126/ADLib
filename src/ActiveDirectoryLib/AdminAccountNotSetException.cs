using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveDirectoryLib
{
    public class AdminAccountNotSetException : Exception
    {
        public AdminAccountNotSetException() : base("No Admin Account was set") 
        { 
        }
    }
}
