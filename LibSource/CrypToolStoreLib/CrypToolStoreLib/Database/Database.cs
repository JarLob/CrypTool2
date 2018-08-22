/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypToolStoreLib.DataObjects;
using CrypToolStoreLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CrypToolStoreLib.Database
{
    public class Database : IDisposable
    {
        private Logger logger = Logger.GetLogger();

        private int PBKDF2_ITERATION_COUNT = 10000;
        private int PBKDF2_HASH_LENGTH = 32;

        private string databaseServer;
        private string databaseName;
        private string databaseUser;
        private string databasePassword;
        
        private DatabaseConnection[] connections;

        /// <summary>
        /// Create a Database object using the given parameters
        /// </summary>
        /// <param name="databaseServer"></param>
        /// <param name="databaseName"></param>
        /// <param name="databaseUser"></param>
        /// <param name="databasePassword"></param>
        /// <param name="numberOfConnections"></param>
        public Database(string databaseServer, string databaseName, string databaseUser, string databasePassword, int numberOfConnections)
        {
            this.databaseServer = databaseServer;
            this.databaseName = databaseName;
            this.databaseUser = databaseUser;
            this.databasePassword = databasePassword;
            CreateConnections(numberOfConnections);
        }

        /// <summary>
        /// Creates connections to the mysql database
        /// </summary>
        /// <param name="numberOfConnections"></param>
        private void CreateConnections(int numberOfConnections)
        {
            connections = new DatabaseConnection[numberOfConnections];
            for (int i = 0; i < connections.Length; i++)
            {
                connections[i] = new DatabaseConnection(databaseServer, databaseName, databaseUser, databasePassword);
                connections[i].Connect();
            }
        }        

        /// <summary>
        /// Returns next unused connection
        /// if all are used, returns a random one
        /// </summary>
        /// <returns></returns>
        private DatabaseConnection GetConnection()
        {
            foreach (DatabaseConnection connection in connections)
            {
                if (!connection.CurrentlyUsed())
                {
                    return connection;
                }
            }
            Random random = new Random(Guid.NewGuid().GetHashCode());
            int i = random.Next(0, connections.Length - 1);
            return connections[i];
        }
        
        /// <summary>
        /// Closes all open connections to mysql database
        /// </summary>
        public void Dispose()
        {
            foreach (DatabaseConnection connection in connections)
            {
                connection.Close();
            }
        }

        #region database methods

        /// <summary>
        /// Creates a new developer account entry in the database
        /// uses pbkdf2 for creating the password hash
        /// uses RNGCryptoServiceProvider to create a salt for the hash
        /// </summary>
        /// <param name="username"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public void CreateDeveloper(string username, string firstname, string lastname, string email, string password, bool isAdmin = false)
        {
            logger.LogText(String.Format("Creating new developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Info);
            string query = "insert into developers (username, firstname, lastname, email, password, passwordsalt, passworditerations, isadmin) values (@username, @firstname, @lastname, @email, @password, @passwordsalt, @passworditerations, @isadmin)";

            byte[] hash_bytes;
            byte[] salt_bytes = new byte[PBKDF2_HASH_LENGTH];
            using (RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(salt_bytes);
                Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt_bytes, PBKDF2_ITERATION_COUNT);
                hash_bytes = rfc2898DeriveBytes.GetBytes(PBKDF2_HASH_LENGTH);
            }

            string hash_string = Tools.Tools.ByteArrayToHexString(hash_bytes);
            string salt_string = Tools.Tools.ByteArrayToHexString(salt_bytes);

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@username", username},
                new object[]{"@firstname", firstname},
                new object[]{"@lastname", lastname},
                new object[]{"@email", email},
                new object[]{"@password", hash_string},
                new object[]{"@passwordsalt", salt_string},
                new object[]{"@passworditerations", PBKDF2_ITERATION_COUNT},
                new object[]{"@isadmin", isAdmin}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Created new developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Info);
        }

        /// <summary>
        /// Checks, if a developer (username/password combination) exists
        /// returns false, if the username does not exist
        /// returns true, if the derived pbkdf2 hash from password is the same as the one in the database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool CheckDeveloperPassword(string username, string password)
        {
            DatabaseConnection connection = GetConnection();

            string query = "select * from developers where username=@username";

            object[][] parameters = new object[][]{
                new object[]{"@username", username},
            };
            
            var result = connection.ExecutePreparedStatement(query, parameters);

            //username does not exist, thus, return false
            if (result.Count == 0)
            {
                return false;
            }

            //otherwise, use salt and iterations to derive hash using pbkdf2
            string hash_from_database = (string)result[0]["password"];
            string salt_from_database = (string)result[0]["passwordsalt"];
            int password_iterations_from_database = (int)result[0]["passworditerations"];

            byte[] hash_bytes;
            byte[] salt_bytes = Tools.Tools.HexStringToByteArray(salt_from_database);
            using (RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt_bytes, password_iterations_from_database);
                hash_bytes = rfc2898DeriveBytes.GetBytes(hash_from_database.Length / 2);
            }
            string hash_string = Tools.Tools.ByteArrayToHexString(hash_bytes);
            //finally return true, if hashes match; otherwise return false
            return hash_string.Equals(hash_from_database);
        }

        /// <summary>
        /// Returns the developer identified by his username
        /// if the developer does not exist returns null
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Developer GetDeveloper(string username)
        {
            DatabaseConnection connection = GetConnection();

            string query = "select username, firstname, lastname, email, isadmin from developers where username=@username";

            object[][] parameters = new object[][]{
                new object[]{"@username", username},
            };

            var result = connection.ExecutePreparedStatement(query, parameters);

            //username does not exist, thus, return false
            if (result.Count == 0)
            {
                return null;
            }

            //Create a new Developer object, fill it with data retrieved from database and return it
            Developer developer = new Developer();
            developer.Username =  (string)result[0]["username"];
            developer.Firstname = (string)result[0]["firstname"];
            developer.Lastname = (string)result[0]["lastname"];
            developer.Email = (string)result[0]["email"];
            developer.IsAdmin = (bool)result[0]["isadmin"];
            return developer;
        }

       /// <summary>
       /// Updates an existing developer account entry in the database
       /// Does NOT update the password
       /// </summary>
       /// <param name="username"></param>
       /// <param name="firstname"></param>
       /// <param name="lastname"></param>
       /// <param name="email"></param>
       /// <param name="isAdmin"></param>
        public void UpdateDeveloper(string username, string firstname, string lastname, string email, bool isAdmin = false)
        {
            logger.LogText(String.Format("Updating existing developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Info);
            string query = "update developers set firstname=@firstname, lastname=@lastname, email=@email, isadmin=@isadmin where username=@username";            

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{                
                new object[]{"@firstname", firstname},
                new object[]{"@lastname", lastname},
                new object[]{"@email", email},
                new object[]{"@isadmin", isAdmin},
                new object[]{"@username", username},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Updated existing developer: username={0}, firstname={1}, lastname={2}, email={3}, isadmin={4}", username, firstname, lastname, email, isAdmin == true ? "true" : "false"), this, Logtype.Info);
        }

        /// <summary>
        /// Updates an existing developer's account password in the database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void UpdateDeveloperPassword(string username, string password)
        {
            logger.LogText(String.Format("Updating existing developer's password: username={0}", username), this, Logtype.Info);
            string query = "update developers set password=@password, passwordsalt=@passwordsalt, passworditerations=@passworditerations where username=@username";

            byte[] hash_bytes;
            byte[] salt_bytes = new byte[PBKDF2_HASH_LENGTH];
            using (RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(salt_bytes);
                Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt_bytes, PBKDF2_ITERATION_COUNT);
                hash_bytes = rfc2898DeriveBytes.GetBytes(PBKDF2_HASH_LENGTH);
            }

            string hash_string = Tools.Tools.ByteArrayToHexString(hash_bytes);
            string salt_string = Tools.Tools.ByteArrayToHexString(salt_bytes);

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{                         
                new object[]{"@password", hash_string},
                new object[]{"@passwordsalt", salt_string},
                new object[]{"@passworditerations", PBKDF2_ITERATION_COUNT},
                new object[]{"@username", username}       
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Updated existing developer's password: username={0}", username), this, Logtype.Info);
        }

        /// <summary>
        /// Deletes a developer entry from the database
        /// </summary>
        /// <param name="username"></param>
        public void DeleteDeveloper(string username)
        {
            logger.LogText(String.Format("Deleting developer account: username={0}", username), this, Logtype.Info);
            string query = "delete from developers where username=@username";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{                         

                new object[]{"@username", username}       
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Deleted developer account: username={0}", username), this, Logtype.Info);
        }


        /// <summary>
        /// Returns a list of all developers currently stored in the database
        /// </summary>
        public List<Developer> GetDevelopers()
        {
            string query = "select username, firstname, lastname, email, isadmin from developers";           

            DatabaseConnection connection = GetConnection();
            var resultset = connection.ExecutePreparedStatement(query, null);

            List<Developer> developers = new List<Developer>();

            foreach (var entry in resultset)
            {
                Developer developer = new Developer();
                developer.Username = (string)entry["username"];
                developer.Firstname = (string)entry["firstname"];
                developer.Lastname = (string)entry["lastname"];
                developer.Email = (string)entry["email"];
                developer.IsAdmin = (bool)entry["isadmin"];
                developers.Add(developer);
            }
            return developers;
        }

        /// <summary>
        /// Creates a new plugin for the dedicated developer, identified by his username
        /// </summary>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <param name="shortdescription"></param>
        /// <param name="longdescription"></param>
        /// <param name="authornames"></param>
        /// <param name="authoremails"></param>
        /// <param name="authorinstitutes"></param>
        /// <param name="icon"></param>
        public void CreatePlugin(string username, string name, string shortdescription, string longdescription, string authornames, string authoremails, string authorinstitutes, byte[] icon)
        {
            logger.LogText(String.Format("Creating new plugin: username={0}, name={1}, shortdescription={2}, longdescription={3}, authornames={4}, authoremails={5} authorinstitutes={6}", 
                username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon != null ? icon.Length.ToString() : "null"), this, Logtype.Info);
            string query = "insert into plugins (username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon) values (@username, @name, @shortdescription, @longdescription, @authornames, @authoremails, @authorinstitutes, @icon)";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@username", username},
                new object[]{"@name", name},
                new object[]{"@shortdescription", shortdescription},
                new object[]{"@longdescription", longdescription},
                new object[]{"@authornames", authornames},
                new object[]{"@authoremails", authoremails},
                new object[]{"@authorinstitutes", authorinstitutes},
                new object[]{"@icon", icon}                
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Created new plugin: username={0}, name={1}, shortdescription={2}, longdescription={3}, authornames={4}, authoremails={5} authorinstitutes={6}",
                username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes), this, Logtype.Info);
        }

        /// <summary>
        /// Updates the dedicated plugin identified by its id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <param name="shortdescription"></param>
        /// <param name="longdescription"></param>
        /// <param name="authornames"></param>
        /// <param name="authoremails"></param>
        /// <param name="authorinstitutes"></param>
        /// <param name="icon"></param>
        public void UpdatePlugin(int id, string username, string name, string shortdescription, string longdescription, string authornames, string authoremails, string authorinstitutes, byte[] icon)
        {
            logger.LogText(String.Format("Updating plugin: id={0}, username={1}, name={2}, shortdescription={3}, longdescription={4}, authornames={5}, authoremails={6} authorinstitutes={7}",
                id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon != null ? icon.Length.ToString() : "null"), this, Logtype.Info);
            string query = "update plugins set username=@username, name=@name, shortdescription=@shortdescription, longdescription=@longdescription, authornames=@authornames, authoremails=@authoremails, authorinstitutes=@authorinstitutes, icon=@icon where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@username", username},
                new object[]{"@name", name},
                new object[]{"@shortdescription", shortdescription},
                new object[]{"@longdescription", longdescription},
                new object[]{"@authornames", authornames},
                new object[]{"@authoremails", authoremails},
                new object[]{"@authorinstitutes", authorinstitutes},
                new object[]{"@icon", icon},
                new object[]{"@id", id}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Updated plugin: id={0}, username={1}, name={2}, shortdescription={3}, longdescription={4}, authornames={5}, authoremails={6} authorinstitutes={7}",
                id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon != null ? icon.Length.ToString() : "null"), this, Logtype.Info);
        }

        /// <summary>
        /// Updates the dedicated plugin identified by its id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <param name="shortdescription"></param>
        /// <param name="longdescription"></param>
        /// <param name="authornames"></param>
        /// <param name="authoremails"></param>
        /// <param name="authorinstitutes"></param>
        /// <param name="icon"></param>
        public void DeletePlugin(int id)
        {
            logger.LogText(String.Format("Deleting plugin: id={0}", id), this, Logtype.Info);
            string query = "delete from plugins where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@id", id}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Deleted plugin: id={0}", id), this, Logtype.Info);
        }

        /// <summary>
        /// Returns a plugin from the database identified by its id
        /// If the plugin does not exist returns null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Plugin GetPlugin(int id)
        {
            string query = "select id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon, activeversion, publish from plugins where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@id", id}
            };

            var resultset = connection.ExecutePreparedStatement(query, parameters);
            if (resultset.Count == 0)
            {
                return null;
            }

            Plugin plugin = new Plugin();
            plugin.Id = (int)resultset[0]["id"];
            plugin.Username = (string)resultset[0]["username"];
            plugin.Name = (string)resultset[0]["name"];
            plugin.ShortDescription = (string)resultset[0]["shortdescription"];
            plugin.LongDescription = (string)resultset[0]["longdescription"];
            plugin.Authornames = (string)resultset[0]["authornames"];
            plugin.Authoremails = (string)resultset[0]["authoremails"];
            plugin.Authorinstitutes = (string)resultset[0]["authorinstitutes"];
            plugin.Icon = (byte[])resultset[0]["icon"];
            plugin.ActiveVersion = (int)resultset[0]["activeversion"];
            plugin.Publish = (bool)resultset[0]["publish"];

            return plugin;            
        }

        /// <summary>
        /// Returns a list of plugins from the database identified by its id
        /// If username is set, it only returns plugins of that user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Plugin> GetPlugins(string username = null)
        {
            string query;
            if (username == null)
            {
               query = "select id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon, activeversion, publish from plugins";
            }
            else
            {
                query = "select id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon, activeversion, publish from plugins where username=@username";
            }

            DatabaseConnection connection = GetConnection();

            object[][] parameters = null;

            if (username != null)
            {
                parameters = new object[][]{
                new object[]{"@username", username}
            };
            }

            var resultset = connection.ExecutePreparedStatement(query, parameters);
            List<Plugin> plugins = new List<Plugin>();

            foreach (var entry in resultset)
            {
                Plugin plugin = new Plugin();
                plugin.Id = (int)entry["id"];
                plugin.Username = (string)entry["username"];
                plugin.Name = (string)entry["name"];
                plugin.ShortDescription = (string)entry["shortdescription"];
                plugin.LongDescription = (string)entry["longdescription"];
                plugin.Authornames = (string)entry["authornames"];
                plugin.Authoremails = (string)entry["authoremails"];
                plugin.Authorinstitutes = (string)entry["authorinstitutes"];
                plugin.Icon = (byte[])entry["icon"];
                plugin.ActiveVersion = (int)entry["activeversion"];
                plugin.Publish = (bool)entry["publish"];
                plugins.Add(plugin);
            }
            return plugins;
        }

        #endregion
    }
}