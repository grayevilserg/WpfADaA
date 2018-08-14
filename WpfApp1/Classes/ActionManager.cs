using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class ActionManager
    {
        private static string[] User_propertys =
            { "givenName", "sn", "userPrincipalName", "mail", "whenCreated", "lockoutTime" };
        public static string LDAP_connection = "emea.roche.com";
        public static string LDAP_path = "LDAP://OU=Users,OU=IT,OU=Moscow,OU=AdminUnits,DC=emea,DC=roche,DC=com";

        // create ldap parametres
        public static DirectoryEntry createDirectoryEntry()
        {
            DirectoryEntry ldapconnection = new DirectoryEntry(LDAP_connection);
            ldapconnection.Path = LDAP_path;
            ldapconnection.AuthenticationType = AuthenticationTypes.Secure;

            return ldapconnection;
        }

        // check of existence of the user in AD
        public static bool Exists(string username)
        {
            bool found = false;
            DirectoryEntry myconnection = createDirectoryEntry();
            string SearchFilter = string.Format("(&((&(objectCategory=Person)(objectClass=User)))(userPrincipalName={0}",
                username);
            DirectorySearcher searchname = new DirectorySearcher(myconnection, SearchFilter);
            searchname.SearchScope = System.DirectoryServices.SearchScope.Subtree;
            searchname.PropertyNamesOnly = true;

            // get result values 
            SearchResult mysearchnameresult = searchname.FindOne();
            if (mysearchnameresult.Properties.Values.Equals(username))
            {
                found = true;
            }
            searchname.Dispose();
            return found;
        }

        // get user's information from AD and construct observable collection
        public static ObservableCollection<User> GetUserInfo(string username, ObservableCollection<User> user_propertys)
        {
            bool foundname = Exists(username);
            //pull request to domain controller
            if (foundname == true)
            {
                DirectoryEntry myldapconnection = createDirectoryEntry();
                string SearchFilter = string.Format("(&((&(objectCategory=Person)(objectClass=User)))(userPrincipalName={0}",
                    username);
                DirectorySearcher search = new DirectorySearcher(myldapconnection, SearchFilter, User_propertys);
                search.SearchScope = System.DirectoryServices.SearchScope.Subtree;

                // get result values 
                SearchResultCollection mysearchresult = search.FindAll();

                if (mysearchresult != null)
                {
                    foreach (SearchResult searchresult in mysearchresult)
                    {
                        var entry = searchresult.GetDirectoryEntry();
                        user_propertys.Add(new User
                        {
                            UserName = entry.Properties["userPrincipalName"].Value.ToString(),
                            FirstName = entry.Properties["givenName"].Value.ToString(),
                            LastName = entry.Properties["sn"].Value.ToString(),
                            Email = entry.Properties["mail"].Value.ToString(),
                            CreationDate = entry.Properties["whenCreated"].Value.ToString(),
                            AccountStatus = entry.Properties["lockoutTime"].Value.ToString(),
                        });
                        entry.Close();
                        entry.Dispose();
                    }
                }
                search.Dispose();
            }
            return user_propertys;
        }

        // reset/change user's password
        public static void ChangePassword(string username, string password)
        {
            DirectoryEntry de = new DirectoryEntry(username);
            de.Invoke("SetPassword", new object[] { password });
            de.Properties["lockoutTime"].Value = 0;
            de.Dispose();
            de.Close();
        }

        public static string RandPass()
        {
            Random rand = new Random();
            int count;
            char[] randomword = new char[10];
            string resultstring;

            for (int i = 0; i < 10; i++)
            {
                count = Convert.ToInt32(rand.Next(0, 3));
                switch (count)
                {
                    case 0:
                        randomword[i] = (char)rand.Next(48, 58);
                        break;
                    case 1:
                        randomword[i] = (char)rand.Next(65, 91);
                        break;
                    case 2:
                        randomword[i] = (char)rand.Next(97, 123);
                        break;
                }
            }
            resultstring = new string(randomword);
            return resultstring;
        }

        public static ObservableCollection<Groups> GetGroups()
        {
            var groups = new ObservableCollection<Groups>();

            groups.Add(new Groups { GroupName = "TestFirst" });
            groups.Add(new Groups { GroupName = "TestSecond" });

            return groups;
        }
    }
}
