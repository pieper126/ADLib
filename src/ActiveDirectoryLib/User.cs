// <copyright file="User.cs">
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
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiveDirectoryLib.Exceptions;

namespace ActiveDirectoryLib
{
    public class User
    {
        #region SupportedProperties

        private static string[] supportedProperties = new string[] { "cn", "sAMAccountName", "sn", "co", "comment", "company", "department", "description", "displayName", "division", "employeeID", "givenName", "homeDirectory", "l", "mail", "manager", "middleName", "mobile", "internationalISDNNumber", "homePhone", "initials", "personalTitle", "postalCode", "st", "streetAddress", "title", "userWorkstations", "generationQualifier" };

        #endregion
        
        #region Properties
        public string UserPath { get; private set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Mail { get; set; }

        public string HomeDirectory { get; set; }

        public string CountryAbbreviation { get; set; }

        public string Country { get; set; }

        public string Comment { get; set; }

        public string Company { get; set; }

        public string Department { get; set; }

        public string Description { get; set; }

        public string DisplayName { get; set; }

        public string Division { get; set; }

        public string EmployeeID { get; set; }

        public string GenerationalSuffix { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }

        public string LogonWorkstations { get; set; }

        public string Title { get; set; }

        public string PersonalTitle { get; set; }

        public string Manager { get; set; }

        public string MiddleName { get; set; }

        public string MobileNumber { get; set; }

        public string InternationalISDNNumber { get; set; }

        public string HomePhone { get; set; }

        public string Initials { get; set; }

        public string Province { get; set; }

        public string StreetAddress { get; set; }

        internal static string[] SupportedProperties
        {
            get
            {
                return supportedProperties;
            }

            private set
            {
                supportedProperties = value;
            }
        }
        
        #endregion
        
        internal User(string username, string userPath)
        {
            this.Username = username;
            this.UserPath = userPath;
        }

        /// <summary>
        /// Commits all changes to the user.
        /// The currently active domain must be set. To set the domain use: SetDomain(LDAPPath).
        /// The currently active admin account must be set to use this. To set the active admin account use: SetAdminAccount(username, password)
        /// </summary>
        /// <exception cref="DomainNotSetException">Gets thrown when the domain hasn't been set.</exception>
        /// <exception cref="AdminAccountNotSetException">Gets thrown when the admin account hasn't been set.</exception>
        public void CommitChanges()
        {
            if (!ActiveDirectory.isDomainSet) throw new DomainNotSetException();
            if (!ActiveDirectory.isAdminAccountSet) throw new AdminAccountNotSetException();

            DirectoryEntry user = new DirectoryEntry(this.UserPath, ActiveDirectory.AdminUsername, ActiveDirectory.AdminPassword, AuthenticationTypes.Secure);

            user.Properties["sAMAccountName"].Value = this.Username;
            user.Properties["givenName"].Value = this.FirstName;
            user.Properties["sn"].Value = this.LastName;
            user.Properties["homeDirectory"].Value = this.HomeDirectory;
            user.Properties["c"].Value = this.CountryAbbreviation;
            user.Properties["co"].Value = this.Country;
            user.Properties["comment"].Value = this.Comment;
            user.Properties["company"].Value = this.Company;
            user.Properties["department"].Value = this.Department;
            user.Properties["description"].Value = this.Description;
            user.Properties["displayName"].Value = this.DisplayName;
            user.Properties["division"].Value = this.Division;
            user.Properties["employeeID"].Value = this.EmployeeID;
            user.Properties["l"].Value = this.City;
            user.Properties["mail"].Value = this.Mail;
            user.Properties["manager"].Value = this.Manager;
            user.Properties["middleName"].Value = this.MiddleName;
            user.Properties["mobile"].Value = this.MobileNumber;
            user.Properties["internationalISDNNumber"].Value = this.InternationalISDNNumber;
            user.Properties["homePhone"].Value = this.HomePhone;
            user.Properties["initials"].Value = this.Initials;
            user.Properties["personalTitle"].Value = this.PersonalTitle;
            user.Properties["postalCode"].Value = this.ZipCode;
            user.Properties["st"].Value = this.Province;
            user.Properties["streetAddress"].Value = this.StreetAddress;
            user.Properties["title"].Value = this.Title;
            user.Properties["userWorkstations"].Value = this.LogonWorkstations;
            user.Properties["generationQualifier"].Value = this.GenerationalSuffix;

            user.CommitChanges();

            user.Close();
        }

        /// <summary>
        /// Sets the password of the user.
        /// The currently active domain must be set. To set the domain use: SetDomain(LDAPPath).
        /// The currently active admin account must be set to use this. To set the active admin account use SetAdminAccount(username, password)
        /// </summary>
        /// <param name="password">The password to set to.</param>
        /// <exception cref="DomainNotSetException">Gets thrown when the domain hasn't been set.</exception>
        /// <exception cref="AdminAccountNotSetException">Gets thrown when the admin account hasn't been set.</exception>
        /// <exception cref="ArgumentNullException">Gets thrown when password is null.</exception>
        /// <exception cref="ArgumentException">Gets thrown when password is an empty string.</exception>
        public void SetPassword(string password)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (password == string.Empty) throw new ArgumentException("password was empty.");

            if (!ActiveDirectory.isDomainSet) throw new DomainNotSetException();
            if (!ActiveDirectory.isAdminAccountSet) throw new AdminAccountNotSetException();

            DirectoryEntry user = new DirectoryEntry(this.UserPath, ActiveDirectory.AdminUsername, ActiveDirectory.AdminPassword, AuthenticationTypes.Secure);
            user.Invoke("SetPassword", new object[] { password });
            user.Close();
        }

        /// <summary>
        /// Enables this user. 
        /// The currently active domain must be set. To set the domain use: SetDomain(LDAPPath).
        /// The currently active admin account must be set to use this. To set the active admin account use: SetAdminAccount(username, password)
        /// </summary>
        /// <exception cref="DomainNotSetException">Gets thrown when the domain hasn't been set.</exception>
        /// <exception cref="AdminAccountNotSetException">Gets thrown when the admin account hasn't been set.</exception>
        public void Enable()
        {
            if (!ActiveDirectory.isDomainSet) throw new DomainNotSetException();
            if (!ActiveDirectory.isAdminAccountSet) throw new AdminAccountNotSetException();

            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, this.UserPath, ActiveDirectory.AdminUsername, ActiveDirectory.AdminPassword);
            UserPrincipal userprincipal = UserPrincipal.FindByIdentity(principalContext, this.Username);

            userprincipal.Enabled = true;
            userprincipal.Save();
        }

        /// <summary>
        /// Disables this user. 
        /// The currently active domain must be set. To set the domain use: SetDomain(LDAPPath).
        /// The currently active admin account must be set to use this. To set the active admin account use: SetAdminAccount(username, password) <see cref="SetAdminAccount"/>
        /// </summary>
        /// <exception cref="DomainNotSetException">Gets thrown when the domain hasn't been set.</exception>
        /// <exception cref="AdminAccountNotSetException">Gets thrown when the admin account hasn't been set.</exception>
        public void Disable()
        {
            if (!ActiveDirectory.isDomainSet) throw new DomainNotSetException();
            if (!ActiveDirectory.isAdminAccountSet) throw new AdminAccountNotSetException();

            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, this.UserPath, ActiveDirectory.AdminUsername, ActiveDirectory.AdminPassword);
            UserPrincipal userprincipal = UserPrincipal.FindByIdentity(principalContext, this.Username);

            userprincipal.Enabled = false;
            userprincipal.Save();
        }
    }
}
