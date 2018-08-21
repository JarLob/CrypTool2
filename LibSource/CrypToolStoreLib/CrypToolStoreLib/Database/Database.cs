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
        public void CreateDeveloper(string username, string firstname, string lastname, string email, string password)
        {
            logger.LogText(String.Format("Creating new user: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Info);
            string query = "insert into developers (username, firstname, lastname, email, password, passwordsalt, passworditerations) values (@username, @firstname, @lastname, @email, @password, @passwordsalt, @passworditerations)";

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
                new object[]{"@passworditerations", PBKDF2_ITERATION_COUNT}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Created new user: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Info);
        }

        /// <summary>
        /// Checks, if a developer (username/password combination) exists
        /// returns false, if the username does not exist
        /// returns true, if the derived pbkdf2 hash from password is the same as the one in the database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool CheckDeveloper(string username, string password)
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

        #endregion
    }
}