using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveDirectoryLib
{
    public class ActiveDirectory
    {
        internal static string DomainPath { get; private set; }

        internal static string DomainName { get; private set; }

        internal static string LDAPDomainName { get; private set; }

        internal static string LDAPExtension { get; private set; }

        internal static string DomainPathUsers { get; private set; }

        internal static string AdminUsername { get; private set; }

        internal static string AdminPassword { get; private set; }

        /// <summary>
        /// Sets the currently active domain.
        /// </summary>
        /// <param name="LDAPPath">The LDAP path of the domain</param>
        public static void SetDomain(string LDAPPath)
        {
            DomainPath = LDAPPath;
            LDAPDomainName = LDAPPath.Substring(0, LDAPPath.LastIndexOf('/') + 1);
            DomainName = LDAPDomainName.Substring(7, LDAPDomainName.Length - 2 - 7);
            LDAPExtension = LDAPPath.Substring(LDAPPath.LastIndexOf('/') + 1);
            DomainPathUsers = LDAPDomainName + "OU = Users" + LDAPExtension;
        }

        /// <summary>
        /// Sets the currently active admin account, used when performing any action that requires administrator access. The active domain needs to be set to use this!
        /// To set the currently active domain user SetDomain(LDAPPath).
        /// </summary>
        /// <param name="username">The username of the admin account.</param>
        /// <param name="password">The password of the admin account.</param>
        /// <exception cref="DomainNotSetException">Gets thrown when the domain hasn't been set.</exception>
        public static void SetAdminAccount(string username, string password)
        {
            if (!Authenticate(username, password)) throw new DomainNotSetException();

            AdminUsername = username;
            AdminPassword = password;
        }

        /// <summary>
        /// Authenticates a user against the currently active domain. Use SetDomain(LDAPPath) to set the currently active domain.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>True if the account was authenticated, false if not.</returns>
        /// <exception cref="DomainNotSetException">Gets thrown when the domain hasn't been set.</exception>
        public static bool Authenticate(string username, string password)
        {
            if (DomainPath == null || DomainPath.Trim() == string.Empty) throw new DomainNotSetException();

            bool authenticated = false;
            try
            {
                DirectoryEntry entry = new DirectoryEntry(DomainPath, username, password);
                object nativeObject = entry.NativeObject;
                entry.Close();
                authenticated = true;
            }
            catch (DirectoryServicesCOMException) 
            {
                throw;
            }
            catch (Exception e)
            {
                if (e.Message == "Logon failure: unknown user name or bad password.\r\n")
                {
                    Debug.WriteLine(e.GetType());
                    throw;
                }

                throw;
            }

            return authenticated;
        }

        /// <summary>
        /// Creates a new user with the given name and password in the Organizational Unit 'Users'. 
        /// The currently active domain must be set to use this! To set the active domain use SetDomain(LDAPPath)
        /// The currently active admin account must be set to use this! To set the active admin account use SetAdminAccount(username, password)
        /// </summary>
        /// <param name="cn">The cn of the new user.</param>
        /// <param name="password">The password of the new user.</param>
        /// <returns>The newly created user.</returns>
        /// <exception cref="DomainNotSetException">Gets thrown when the domain hasn't been set.</exception>
        /// <exception cref="AdminAccountNotSetException">Gets thrown when the admin account hasn't been set.</exception>
        public static User CreateUser(string cn, string password)
        {
            List<string> OUs = new List<string>();
            OUs.Add("Users");
            return CreateUser(cn, password, OUs);
        }

        /// <summary>
        /// Creates a new user with the given name and password in the given Organizational Unit. 
        /// The currently active domain must be set to use this! To set the active domain use SetDomain(LDAPPath)
        /// The currently active admin account must be set to use this! To set the active admin account use SetAdminAccount(username, password)
        /// </summary>
        /// <param name="cn">The cn of the new user.</param>
        /// <param name="password">The password of the new user.</param>
        /// <param name="OUs">The Organizational Units for the new user.</param>
        /// <returns>The newly created user.</returns>
        /// <exception cref="DomainNotSetException">Gets thrown when the domain hasn't been set.</exception>
        /// <exception cref="AdminAccountNotSetException">Gets thrown when the admin account hasn't been set.</exception>
        public static User CreateUser(string cn, string password, List<string> OUs)
        {
            if (DomainPath == null || DomainPath.Trim() == string.Empty) throw new DomainNotSetException();
            if (AdminUsername == null || AdminUsername.Trim() == string.Empty ||
                AdminPassword == null || AdminPassword.Trim() == string.Empty) throw new AdminAccountNotSetException();

            string LDAP = CreateLDAPForOUs(OUs);
            DirectoryEntry entry = new DirectoryEntry(LDAP, AdminUsername, AdminPassword, AuthenticationTypes.Secure);
            DirectoryEntry newUser = entry.Children.Add("CN =" + cn, "user");

            newUser.Properties["sAMAccountName"].Value = cn;

            newUser.CommitChanges();

            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, cn);
            user.SetPassword(password);
            user.Save();

            newUser.Close();
            entry.Close();

            string userPath = LDAP.Replace(LDAPDomainName, string.Empty);
            userPath = LDAPDomainName + "CN = " + cn + ", " + userPath;

            Debug.WriteLine(userPath);

            return new User(cn, userPath);
        }

        /// <summary>
        /// Creates a new group with the given name in the given Organizational Unit.
        /// The currently active domain must be set to use this! To set the active domain use SetDomain(LDAPPath)
        /// The currently active admin account must be set to use this! To set the active admin account use SetAdminAccount(username, password)
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <param name="OUs">The Organizational units to place this group in.</param>
        /// <returns>The group that was created.</returns>
        /// <exception cref="DomainNotSetException">Gets thrown when the domain hasn't been set.</exception>
        /// <exception cref="AdminAccountNotSetException">Gets thrown when the admin account hasn't been set.</exception>
        public static Group CreateGroup(string name, List<string> OUs)
        {
            if (DomainPath == null || DomainPath.Trim() == string.Empty) throw new DomainNotSetException();
            if (AdminUsername == null || AdminUsername.Trim() == string.Empty ||
                AdminPassword == null || AdminPassword.Trim() == string.Empty) throw new AdminAccountNotSetException();

            DirectoryEntry entry = new DirectoryEntry(CreateLDAPForOUs(OUs), AdminUsername, AdminPassword, AuthenticationTypes.Secure);
            DirectoryEntry group = entry.Children.Add("CN=" + name, "group");
            group.Properties["sAmAccountName"].Value = name;
            group.CommitChanges();
            group.Close();
            entry.Close();
            return new Group(name, "temp");
        }

        /// <summary>
        /// Gets the CN based on the username.
        /// </summary>
        /// <param name="username">The username of the account.</param>
        /// <returns>The cn.</returns>
        public static string GetCNFromUsername(string username)
        {
            string ldapPath = LDAPDomainName + LDAPExtension;
            DirectoryEntry entry = new DirectoryEntry(ldapPath, AdminUsername, AdminPassword, AuthenticationTypes.Secure);

            DirectorySearcher search = new DirectorySearcher(entry);
            search.Filter = "(&(objectClass=user) (sAMAccountName=" + username + "))";

            search.PropertiesToLoad.Add("cn");

            SearchResult result = search.FindOne();

            Dictionary<string, object> foundProperties = new Dictionary<string, object>();

            if (result == null) return null;

            foreach (object val in result.Properties["cn"])
            {
                if (val is String)
                {
                    return (string)val;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a user from active directory based on it's cn.
        /// </summary>
        /// <param name="cn">The cn.</param>
        /// <returns>The user if one was found, else it will return null.</returns>
        public static User GetUser(string cn)
        {
            string ldapPath = LDAPDomainName + LDAPExtension;
            DirectoryEntry entry = new DirectoryEntry(ldapPath, AdminUsername, AdminPassword, AuthenticationTypes.Secure);

            DirectorySearcher search = new DirectorySearcher(entry);
            search.Filter = "(&(objectClass=user) (cn=" + cn + "))";

            foreach (string property in User.SupportedProperties)
                search.PropertiesToLoad.Add(property);

            SearchResult result = search.FindOne();

            Dictionary<string, object> foundProperties = new Dictionary<string, object>();

            if (result == null) return null;

            foreach (string property in User.SupportedProperties)
            {
                foreach (object resProp in result.Properties[property])
                {
                    foundProperties.Add(property, resProp);
                }
            }

            if (foundProperties["sAMAccountName"] != null)
            {
                User newUser = new User((string)foundProperties["sAMAccountName"], result.Path);

                newUser.FirstName = !foundProperties.Keys.Contains("givenName") ? null : foundProperties["givenName"] != null ? (string)foundProperties["givenName"] : null;
                newUser.LastName = !foundProperties.Keys.Contains("sn") ? null : foundProperties["sn"] != null ? (string)foundProperties["sn"] : null;
                newUser.HomeDirectory = !foundProperties.Keys.Contains("homeDirectory") ? null : foundProperties["homeDirectory"] != null ? (string)foundProperties["homeDirectory"] : null;
                newUser.Country = !foundProperties.Keys.Contains("co") ? null : foundProperties["co"] != null ? (string)foundProperties["co"] : null;
                newUser.Comment = !foundProperties.Keys.Contains("comment") ? null : foundProperties["comment"] != null ? (string)foundProperties["comment"] : null;
                newUser.Company = !foundProperties.Keys.Contains("company") ? null : foundProperties["company"] != null ? (string)foundProperties["company"] : null;
                newUser.Department = !foundProperties.Keys.Contains("department") ? null : foundProperties["department"] != null ? (string)foundProperties["department"] : null;
                newUser.Description = !foundProperties.Keys.Contains("description") ? null : foundProperties["description"] != null ? (string)foundProperties["description"] : null;
                newUser.DisplayName = !foundProperties.Keys.Contains("displayName") ? null : foundProperties["displayName"] != null ? (string)foundProperties["displayName"] : null;
                newUser.Division = !foundProperties.Keys.Contains("division") ? null : foundProperties["division"] != null ? (string)foundProperties["division"] : null;
                newUser.EmployeeID = !foundProperties.Keys.Contains("employeeID") ? null : foundProperties["employeeID"] != null ? (string)foundProperties["employeeID"] : null;
                newUser.City = !foundProperties.Keys.Contains("l") ? null : foundProperties["l"] != null ? (string)foundProperties["l"] : null;
                newUser.Mail = !foundProperties.Keys.Contains("mail") ? null : foundProperties["mail"] != null ? (string)foundProperties["mail"] : null;
                newUser.Manager = !foundProperties.Keys.Contains("manager") ? null : foundProperties["manager"] != null ? (string)foundProperties["manager"] : null;
                newUser.MiddleName = !foundProperties.Keys.Contains("middleName") ? null : foundProperties["middleName"] != null ? (string)foundProperties["middleName"] : null;
                newUser.MobileNumber = !foundProperties.Keys.Contains("mobile") ? null : foundProperties["mobile"] != null ? (string)foundProperties["mobile"] : null;
                newUser.InternationalISDNNumber = !foundProperties.Keys.Contains("internationalISDNNumber") ? null : foundProperties["internationalISDNNumber"] != null ? (string)foundProperties["internationalISDNNumber"] : null;
                newUser.HomePhone = !foundProperties.Keys.Contains("homePhone") ? null : foundProperties["homePhone"] != null ? (string)foundProperties["homePhone"] : null;
                newUser.Initials = !foundProperties.Keys.Contains("initials") ? null : foundProperties["initials"] != null ? (string)foundProperties["initials"] : null;
                newUser.PersonalTitle = !foundProperties.Keys.Contains("personalTitle") ? null : foundProperties["personalTitle"] != null ? (string)foundProperties["personalTitle"] : null;
                newUser.ZipCode = !foundProperties.Keys.Contains("postalCode") ? null : foundProperties["postalCode"] != null ? (string)foundProperties["postalCode"] : null;
                newUser.Province = !foundProperties.Keys.Contains("postalCode") ? null : foundProperties["postalCode"] != null ? (string)foundProperties["st"] : null;
                newUser.StreetAddress = !foundProperties.Keys.Contains("streetAddress") ? null : foundProperties["streetAddress"] != null ? (string)foundProperties["streetAddress"] : null;
                newUser.Title = !foundProperties.Keys.Contains("personalTitle") ? null : foundProperties["personalTitle"] != null ? (string)foundProperties["personalTitle"] : null;
                newUser.LogonWorkstations = !foundProperties.Keys.Contains("userWorkstations") ? null : foundProperties["userWorkstations"] != null ? (string)foundProperties["userWorkstations"] : null;
                newUser.CountryAbbreviation = !foundProperties.Keys.Contains("c") ? null : foundProperties["c"] != null ? (string)foundProperties["c"] : null;
                newUser.GenerationalSuffix = !foundProperties.Keys.Contains("generationQualifier") ? null : foundProperties["generationQualifier"] != null ? (string)foundProperties["generationQualifier"] : null;
                return newUser;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Generates LDAP path to given OU's.
        /// </summary>
        /// <param name="OUs">The OU's for the path.</param>
        /// <returns>The path</returns>
        private static string CreateLDAPForOUs(List<string> OUs)
        {
            string LDAP = LDAPDomainName;
            foreach (string OU in OUs)
            {
                LDAP += " OU = " + OU + ",";
            }

            return LDAP + LDAPExtension;
        }
    }
}
