using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveDirectoryLib
{
    public class DomainNotSetException : Exception
    {
        public DomainNotSetException() : base("The currently active domain appears not to be set. You need to set the active domain if you wish to perform this actions.") 
        { 
        }
    }
}
