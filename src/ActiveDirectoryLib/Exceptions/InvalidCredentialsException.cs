﻿// <copyright file="InvalidCredentialsException.cs">
// Copyright (c) 2015 All Rights Reserved. 

// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.

// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public
// License along with this library.
// </copyright>
// <author>Stijn Bijnen</author>
// <author>Stan van den Broek</author> 
// <date>6/1/2015 </date>

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
