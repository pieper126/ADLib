﻿// <copyright file="Group.cs">
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
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiveDirectoryLib.Exceptions;

namespace ActiveDirectoryLib
{
    public class Group
    {
        private string groupPath;

        public string GroupName { get; set; }

        public Group(string groupName, string groupPath)
        {
            this.GroupName = groupName;
            this.groupPath = groupPath;
        }

        /// <summary>
        /// Adds a user to the group.
        /// The currently active domain must be set to use this! To set the active domain use SetDomain(LDAPPath)
        /// The currently active admin account must be set to use this! To set the active admin account use SetAdminAccount(username, password)
        /// </summary>
        /// <param name="user">The user to be added.</param>
        /// <exception cref="DomainNotSetException">Gets thrown when the domain hasn't been set.</exception>
        /// <exception cref="AdminAccountNotSetException">Gets thrown when the admin account hasn't been set.</exception>
        public void AddUser(User user)
        {
            if (ActiveDirectory.DomainPath == null || ActiveDirectory.DomainPath.Trim() == string.Empty) 
                throw new DomainNotSetException();
            if (ActiveDirectory.AdminUsername == null || ActiveDirectory.AdminUsername.Trim() == string.Empty ||
                ActiveDirectory.AdminPassword == null || ActiveDirectory.AdminPassword.Trim() == string.Empty) 
                throw new AdminAccountNotSetException();

            DirectoryEntry entry = new DirectoryEntry(this.groupPath, ActiveDirectory.AdminUsername, ActiveDirectory.AdminPassword, AuthenticationTypes.Secure);
            entry.Properties["member"].Add(user.UserPath);
            entry.CommitChanges();
            entry.Close();
         }

        /// <summary>
        /// Remove a user from the group.
        /// The currently active domain must be set to use this! To set the active domain use SetDomain(LDAPPath)
        /// The currently active admin account must be set to use this! To set the active admin account use SetAdminAccount(username, password)
        /// </summary>
        /// <param name="user">The user to be removed.</param>
        /// <exception cref="DomainNotSetException">Gets thrown when the domain hasn't been set.</exception>
        /// <exception cref="AdminAccountNotSetException">Gets thrown when the admin account hasn't been set.</exception>
        public void RemoveUser(User user)
        {
            if (ActiveDirectory.DomainPath == null || ActiveDirectory.DomainPath.Trim() == string.Empty)
                throw new DomainNotSetException();
            if (ActiveDirectory.AdminUsername == null || ActiveDirectory.AdminUsername.Trim() == string.Empty ||
                ActiveDirectory.AdminPassword == null || ActiveDirectory.AdminPassword.Trim() == string.Empty)
                throw new AdminAccountNotSetException();

            DirectoryEntry entry = new DirectoryEntry(this.groupPath, ActiveDirectory.AdminUsername, ActiveDirectory.AdminPassword, AuthenticationTypes.Secure);
            entry.Properties["member"].Remove(user.UserPath);
            entry.CommitChanges();
            entry.Close();
        }
    }
}