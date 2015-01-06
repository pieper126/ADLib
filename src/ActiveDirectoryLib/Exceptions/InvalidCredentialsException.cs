using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveDirectoryLib.Exceptions
{
    class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException()
            : base("The credentials used where invalid, unable to authenticate.")
        {}
    }
}
