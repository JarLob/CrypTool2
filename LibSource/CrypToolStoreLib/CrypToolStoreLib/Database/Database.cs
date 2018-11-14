﻿/*
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
    /// <summary>
    /// The Database manages connections to the mysql database. It also offers methods to insert, update, and delete all objects of CrypToolStore in the database.
    /// Furthermore, it offers some check methods (e.g. developer's password)
    /// </summary>
    public class CrypToolStoreDatabase : IDisposable
    {
        private Logger logger = Logger.GetLogger();
        private const int PBKDF2_ITERATION_COUNT = 10000;
        private const int PBKDF2_HASH_LENGTH = 32;
        private string databaseServer;
        private string databaseName;
        private string databaseUser;
        private string databasePassword;        
        private DatabaseConnection[] connections;
        private static CrypToolStoreDatabase database;

        /// <summary>
        /// Return the instance of the database
        /// </summary>
        /// <returns></returns>
        public static CrypToolStoreDatabase GetDatabase()
        {
            if (database == null)
            {
                database = new CrypToolStoreDatabase();
            }

            return database;
        }

        /// <summary>
        /// Set constructor to private for singleton pattern
        /// </summary>
        private CrypToolStoreDatabase()
        {

        }

        /// <summary>
        /// Initializes the database and connects to the mysql database
        /// </summary>
        /// <param name="databaseServer"></param>
        /// <param name="databaseName"></param>
        /// <param name="databaseUser"></param>
        /// <param name="databasePassword"></param>
        /// <param name="numberOfConnections"></param>
        public bool InitAndConnect(string databaseServer, string databaseName, string databaseUser, string databasePassword, int numberOfConnections)
        {
            logger.LogText(String.Format("Connecting to mysql database with databaseServer={0}, databaseName={1}, databaseUser={2}", databaseServer, databaseName, databaseUser), this, Logtype.Info);
            try
            {
                this.databaseServer = databaseServer;
                this.databaseName = databaseName;
                this.databaseUser = databaseUser;
                this.databasePassword = databasePassword;
                CreateConnections(numberOfConnections);
                logger.LogText("Connection successfully established", this, Logtype.Info);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogText(String.Format("Connection failed with exception: {0}", ex.Message), this, Logtype.Error);
                return false;
            }
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
                    connection.CheckConnection();
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
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Closes all open connections to mysql database
        /// </summary>
        public void Dispose()
        {
            logger.LogText("Closing all connections to database", this, Logtype.Debug);
            foreach (DatabaseConnection connection in connections)
            {
                try
                {
                    connection.Close();                    
                }
                catch (Exception ex)
                {
                    logger.LogText(String.Format("Exception occured while closing a connection to database: {0}", ex.Message), this, Logtype.Error);
                }
            }
            logger.LogText("All connections to database closed", this, Logtype.Debug);
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
            logger.LogText(String.Format("Creating new developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Debug);
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

            logger.LogText(String.Format("Created new developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Debug);
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
            logger.LogText(String.Format("Updating existing developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Debug);
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

            logger.LogText(String.Format("Updated existing developer: username={0}, firstname={1}, lastname={2}, email={3}, isadmin={4}", username, firstname, lastname, email, isAdmin == true ? "true" : "false"), this, Logtype.Debug);
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
        public void UpdateDeveloperNoAdmin(string username, string firstname, string lastname, string email)
        {
            logger.LogText(String.Format("Updating existing developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Debug);
            string query = "update developers set firstname=@firstname, lastname=@lastname, email=@email where username=@username";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{                
                new object[]{"@firstname", firstname},
                new object[]{"@lastname", lastname},
                new object[]{"@email", email},
                new object[]{"@username", username},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Updated existing developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates an existing developer's account password in the database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void UpdateDeveloperPassword(string username, string password)
        {
            logger.LogText(String.Format("Updating existing developer's password: username={0}", username), this, Logtype.Debug);
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

            logger.LogText(String.Format("Updated existing developer's password: username={0}", username), this, Logtype.Debug);
        }

        /// <summary>
        /// Deletes a developer entry from the database
        /// </summary>
        /// <param name="username"></param>
        public void DeleteDeveloper(string username)
        {
            logger.LogText(String.Format("Deleting developer account: username={0}", username), this, Logtype.Debug);
            string query = "delete from developers where username=@username";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{                         

                new object[]{"@username", username}       
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Deleted developer account: username={0}", username), this, Logtype.Debug);
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
            logger.LogText(String.Format("Creating new plugin: username={0}, name={1}, shortdescription={2}, longdescription={3}, authornames={4}, authoremails={5} authorinstitutes={6}, icon={7}", 
                username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon != null ? icon.Length.ToString() : "null"), this, Logtype.Debug);
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
                username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes), this, Logtype.Debug);
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
            logger.LogText(String.Format("Updating plugin: id={0}, username={1}, name={2}, shortdescription={3}, longdescription={4}, authornames={5}, authoremails={6} authorinstitutes={7}, icon={8}",
                id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon != null ? icon.Length.ToString() : "null"), this, Logtype.Debug);
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

            logger.LogText(String.Format("Updated plugin: id={0}, username={1}, name={2}, shortdescription={3}, longdescription={4}, authornames={5}, authoremails={6} authorinstitutes={7}, icon={8}",
                id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon != null ? icon.Length.ToString() : "null"), this, Logtype.Debug);
        }

        /// <summary>
        /// Deletes the dedicated plugin identified by its id
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
            logger.LogText(String.Format("Deleting plugin: id={0}", id), this, Logtype.Debug);
            string query = "delete from plugins where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@id", id}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Deleted plugin: id={0}", id), this, Logtype.Debug);
        }

        /// <summary>
        /// Returns a plugin from the database identified by its id
        /// If the plugin does not exist returns null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Plugin GetPlugin(int id)
        {
            string query = "select id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon from plugins where id=@id";

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

            return plugin;            
        }

        /// <summary>
        /// Returns a list of plugins from the database
        /// If username is set, it only returns plugins of that user
        /// icons are NOT included to save bandwith for this request
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Plugin> GetPlugins(string username = null)
        {
            string query;
            if (username == null)
            {
               query = "select id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes from plugins";
            }
            else
            {
                query = "select id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes from plugins where username=@username";
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
                plugins.Add(plugin);
            }
            return plugins;
        }

        /// <summary>
        /// Creates a new source entry in the database
        /// </summary>
        /// <param name="source"></param>
        public void CreateSource(Source source)
        {
            logger.LogText(String.Format("Creating new source: pluginid={0}, pluginversion={1}, buildstate={2}, buildlog={3}", source.PluginId, source.PluginVersion, source.BuildState, source.BuildLog), this, Logtype.Debug);
            string query = "insert into sources (pluginid, pluginversion, zipfilename, assemblyfilename, buildstate, buildlog, publishstate) values (@pluginid, @pluginversion, @zipfilename, @assemblyfilename, @buildstate, @buildlog, @publishstate)";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@pluginid", source.PluginId},
                new object[]{"@pluginversion", source.PluginVersion},             
                new object[]{"@zipfilename", String.Empty},       
                new object[]{"@assemblyfilename", String.Empty},       
                new object[]{"@buildstate", source.BuildState},       
                new object[]{"@buildlog", source.BuildLog},
                new object[]{"@publishstate", PublishState.NOTPUBLISHED.ToString()}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Created new source: pluginid={0}, pluginversion={1}, buildstate={2}, buildlog={3}", source.PluginId, source.PluginVersion, source.BuildState, source.BuildLog), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates a source in the database identified by pluginid and pluginversion
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        /// <param name="zipfilename"></param>
        /// <param name="buildstate"></param>
        /// <param name="buildlog"></param>
        /// <param name="uploaddate"></param>
        public void UpdateSource(int pluginid, int pluginversion, string zipfilename, string buildstate, string buildlog, DateTime uploaddate)
        {
            logger.LogText(String.Format("Updating source: pluginid={0}, pluginversion={1}, zipfilename={2}, buildstate={3}, buildlog={4}, uploaddate={5}", pluginid, pluginversion, zipfilename, buildstate, buildlog, uploaddate), this, Logtype.Debug);
            string query = "update sources set zipfilename = @zipfilename, buildstate=@buildstate, buildlog=@buildlog, uploaddate=@uploaddate where pluginid=@pluginid and pluginversion=@pluginversion";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{                
                
                new object[]{"@zipfilename", zipfilename},
                new object[]{"@buildstate", buildstate},
                new object[]{"@buildlog", buildlog},
                new object[]{"@pluginid", pluginid},
                new object[]{"@pluginversion", pluginversion},
                new object[]{"@uploaddate", uploaddate}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Updating source: pluginid={0}, pluginversion={1}, zipfilename={2}, buildstate={3}, buildlog={4}, uploaddate={5}", pluginid, pluginversion, zipfilename, buildstate, buildlog, uploaddate), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates a source (only assembly file name) in the database identified by pluginid and pluginversion
        /// automatically sets the builddate to the uploadtime
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        /// <param name="assemblyfilename"></param>
        public void UpdateSource(int pluginid, int pluginversion, string assemblyfilename)
        {
            logger.LogText(String.Format("Updating source: pluginid={0}, pluginversion={1}, assemblyfilename={2}, builddate={3}", pluginid, pluginversion, assemblyfilename, DateTime.Now), this, Logtype.Debug);
            string query = "update sources set assemblyfilename=@assemblyfilename, builddate=@builddate where pluginid=@pluginid and pluginversion=@pluginversion";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{                
                
                new object[]{"@assemblyfilename", assemblyfilename},
                new object[]{"@builddate", DateTime.Now},
                new object[]{"@pluginid", pluginid},
                new object[]{"@pluginversion", pluginversion},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Updated source: pluginid={0}, pluginversion={1}, assemblyfilename={2}, builddate={3}", pluginid, pluginversion, assemblyfilename, DateTime.Now), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates a source (only publishstate file name) in the database identified by pluginid and pluginversion
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        /// <param name="publishstate"></param>
        public void UpdateSource(int pluginid, int pluginversion, PublishState publishstate)
        {
            logger.LogText(String.Format("Updating source: pluginid={0}, pluginversion={1}, publishstate={2}", pluginid, pluginversion, publishstate.ToString()), this, Logtype.Debug);
            string query = "update sources set publishstate=@publishstate where pluginid=@pluginid and pluginversion=@pluginversion";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{                
                new object[]{"@pluginid", pluginid},
                new object[]{"@pluginversion", pluginversion},
                new object[]{"@publishstate", publishstate.ToString()}                
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Updated source: pluginid={0}, pluginversion={1}, publishstate={2}", pluginid, pluginversion, publishstate.ToString()), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates a source in the database identified by pluginid and pluginversion
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        /// <param name="zipfilename"></param>
        /// <param name="buildstate"></param>
        /// <param name="buildlog"></param>
        public void UpdateSource(int pluginid, int pluginversion, string zipfilename, string buildstate, string buildlog, int buildversion)
        {
            logger.LogText(String.Format("Updating source: pluginid={0}, pluginversion={1}, zipfilename={2}, buildstate={3}, buildlog={4}, buildversion={5}", pluginid, pluginversion, zipfilename, buildstate, buildlog, buildversion), this, Logtype.Debug);
            string query = "update sources set zipfilename = @zipfilename, buildstate=@buildstate, buildlog=@buildlog, buildversion=@buildversion where pluginid=@pluginid and pluginversion=@pluginversion";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{                
                
                new object[]{"@zipfilename", zipfilename},
                new object[]{"@buildstate", buildstate},
                new object[]{"@buildlog", buildlog},
                new object[]{"@pluginid", pluginid},
                new object[]{"@pluginversion", pluginversion},
                new object[]{"@buildversion", buildversion},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Updated source: pluginid={0}, pluginversion={1}, zipfilename={2}, buildstate={3}, buildlog={4}, buildversion={5}", pluginid, pluginversion, zipfilename, buildstate, buildlog, buildversion), this, Logtype.Debug);
        }

        /// <summary>
        /// Deletes the dedicated source idenfified by pluginid and pluginversion
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        public void DeleteSource(int pluginid, int pluginversion)
        {
            logger.LogText(String.Format("Deleting source: pluginid={0}, pluginversion={1}", pluginid, pluginversion), this, Logtype.Debug);
            string query = "delete from sources where pluginid=@pluginid and pluginversion=@pluginversion";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@pluginid", pluginid},
                new object[]{"@pluginversion", pluginversion}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Deleted source: pluginid={0}, pluginversion={1}", pluginid, pluginversion), this, Logtype.Debug);
        }

        /// <summary>
        /// Returns the dedicated Source identified by pluginid and pluginversion
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        /// <returns></returns>
        public Source GetSource(int pluginid, int pluginversion)
        {
            string query = "select pluginid, pluginversion, buildversion, zipfilename, buildstate, buildlog, assemblyfilename, uploaddate, builddate, publishstate from sources where pluginid=@pluginid and pluginversion=@pluginversion";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@pluginid", pluginid},
                new object[]{"@pluginversion", pluginversion}
            };

            var resultset = connection.ExecutePreparedStatement(query, parameters);
            if (resultset.Count == 0)
            {
                return null;
            }

            Source source = new Source();
            source.PluginId = (int)resultset[0]["pluginid"];
            source.PluginVersion = (int)resultset[0]["pluginversion"];
            source.BuildVersion = (int)resultset[0]["buildversion"];
            source.ZipFileName = (string)resultset[0]["zipfilename"];
            source.BuildState = (string)resultset[0]["buildstate"];
            source.BuildLog = (string)resultset[0]["buildlog"];
            source.AssemblyFileName = (string)resultset[0]["assemblyfilename"];
            source.UploadDate = (DateTime)resultset[0]["uploaddate"];
            source.BuildDate = (DateTime)resultset[0]["builddate"];
            source.PublishState = (string)resultset[0]["publishstate"];

            return source;
        }

        /// <summary>
        /// Returns a list of sources for the dedicated plugin idenfified by the pluginid
        /// </summary>
        /// <param name="pluginid"></param>
        /// <returns></returns>
        public List<Source> GetSources(int pluginid)
        {
            string query = "select pluginid, pluginversion, buildversion, buildstate, buildlog, uploaddate, builddate, zipfilename, assemblyfilename, publishstate from sources where pluginid=@pluginid";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@pluginid", pluginid},
            };

            var resultset = connection.ExecutePreparedStatement(query, parameters);

            List<Source> sources = new List<Source>();

            foreach (var entry in resultset)
            {
                Source source = new Source();
                source.PluginId = (int)entry["pluginid"];
                source.PluginVersion = (int)entry["pluginversion"];
                source.BuildVersion = (int)entry["buildversion"];                
                source.BuildState = (string)entry["buildstate"];
                source.BuildLog = (string)entry["buildlog"];                
                source.UploadDate = (DateTime)entry["uploaddate"];
                source.BuildDate = (DateTime)entry["builddate"];
                source.ZipFileName = (string)entry["zipfilename"];
                source.AssemblyFileName = (string)entry["assemblyfilename"];
                source.PublishState = (string)entry["publishstate"];
                sources.Add(source);
            }
            return sources;
        }

        /// <summary>
        /// Returns a list of sources for with the dedicated buildstate
        /// <param name="buildstate"></param>
        /// </summary>
        public List<Source> GetSources(string buildstate)
        {
            string query = "select pluginid, pluginversion, buildversion, buildstate, buildlog, uploaddate, builddate, zipfilename, assemblyfilename, publishstate from sources where buildstate=@buildstate";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@buildstate", buildstate},
            };

            var resultset = connection.ExecutePreparedStatement(query, parameters);

            List<Source> sources = new List<Source>();

            foreach (var entry in resultset)
            {
                Source source = new Source();
                source.PluginId = (int)entry["pluginid"];
                source.PluginVersion = (int)entry["pluginversion"];
                source.BuildVersion = (int)entry["buildversion"];
                source.BuildState = (string)entry["buildstate"];
                source.BuildLog = (string)entry["buildlog"];
                source.UploadDate = (DateTime)entry["uploaddate"];
                source.BuildDate = (DateTime)entry["builddate"];
                source.ZipFileName = (string)entry["zipfilename"];
                source.AssemblyFileName = (string)entry["assemblyfilename"];
                source.PublishState = (string)entry["publishstate"];
                sources.Add(source);
            }
            return sources;
        }

        /// <summary>
        /// Creates a new resource entry in the database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public void CreateResource(string username, string name, string description)
        {
            logger.LogText(String.Format("Creating new resource: username={0}, name={1}, description={2}", username, name, description), this, Logtype.Debug);
            string query = "insert into resources (username, name, description) values (@username, @name, @description)";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@username", username},
                new object[]{"@name", name},
                new object[]{"@description", description}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Created new resource: username={0}, name={1}, description={2}", username, name, description), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates the dedicated resource identified by its id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public void UpdateResource(int id, string name, string description)
        {
            logger.LogText(String.Format("Updating resource: id={0}, name={1}, description={2}", id, name, description), this, Logtype.Debug);
            string query = "update resources set name=@name, description=@description where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@name", name},
                new object[]{"@description", description},
                new object[]{"@id", id}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Updated resource: id={0}, name={1}, description={2}", id, name, description), this, Logtype.Debug);
        }

        /// <summary>
        /// Deletes the dedicated resource identified by its id
        /// </summary>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public void DeleteResource(int id)
        {
            logger.LogText(String.Format("Deleting resource: id={0}", id), this, Logtype.Debug);
            string query = "delete from resources where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@id", id},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Deleted resource: id={0}", id), this, Logtype.Debug);
        }

        /// <summary>
        /// Returns the dedicated resource identified by its id
        /// Returns null, if the resource does not exist
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Resource GetResource(int id)
        {
            string query = "select id, username, name, description from resources where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@id", id},
            };

            var resultset = connection.ExecutePreparedStatement(query, parameters);
            if (resultset.Count == 0)
            {
                return null;
            }

            Resource resource = new Resource();
            resource.Id = (int)resultset[0]["id"];
            resource.Username = (string)resultset[0]["username"];
            resource.Name = (string)resultset[0]["name"];
            resource.Description = (string)resultset[0]["description"];    
            return resource;
        }

        /// <summary>
        /// Returns a list of resources from the database
        /// If username is set, it only returns resources of that user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Resource> GetResources(string username = null)
        {
            string query;
            if (username == null)
            {
                query = "select id, username, name, description from resources";
            }
            else
            {
                query = "select id, username, name, description from resources where username=@username";
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
            List<Resource> resources = new List<Resource>();

            foreach (var entry in resultset)
            {
                Resource resource = new Resource();
                resource.Id = (int)entry["id"];
                resource.Username = (string)entry["username"];
                resource.Name = (string)entry["name"];
                resource.Description = (string)entry["description"];       
                resources.Add(resource);
            }
            return resources;
        }

        /// <summary>
        /// Creates a new resource data entry in the database
        /// </summary>
        /// <param name="version"></param>
        /// <param name="datafilename"></param>
        /// <param name="uploaddate"></param>
        public void CreateResourceData(int resourceid, int version, string datafilename, DateTime uploaddate)
        {
            logger.LogText(String.Format("Creating new resource data: resourceid={0}, version={1}, datafilename={2}, uploaddate={3}", resourceid, version, datafilename != null ? datafilename.Length.ToString() : "null", uploaddate), this, Logtype.Debug);
            string query = "insert into resourcesdata (resourceid, version, datafilename, uploaddate, publishstate) values (@resourceid, @version, @datafilename, @uploaddate, @publishstate)";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@resourceid", resourceid},
                new object[]{"@version", version},
                new object[]{"@datafilename", datafilename},
                new object[]{"@uploaddate", uploaddate},
                new object[]{"@publishstate", PublishState.NOTPUBLISHED.ToString()}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Created new resource data: resourceid={0}, version={1}, datafilename={2}, uploaddate={3}", resourceid, version, datafilename, uploaddate), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates a resource data entry in the database
        /// </summary>
        /// <param name="version"></param>
        /// <param name="datafilename"></param>
        /// <param name="uploaddate"></param>
        /// <param name="publishstate"></param>
        public void UpdateResourceData(int resourceid, int version, string datafilename, DateTime uploaddate, string publishstate)
        {
            logger.LogText(String.Format("Updating resource data: resourceid={0}, version={1}, datafilename={2}, uploaddate={3}", resourceid, version, datafilename, uploaddate), this, Logtype.Debug);
            string query = "update resourcesdata set datafilename=@datafilename, uploaddate=@uploaddate, publishstate=@publishstate where resourceid=@resourceid and version=@version";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{                
                new object[]{"@datafilename", datafilename},
                new object[]{"@uploaddate", uploaddate},
                new object[]{"@resourceid", resourceid},
                new object[]{"@version", version},
                new object[]{"@publishstate", publishstate},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Updated resource data: resourceid={0}, version={1}, datafilename={2}, uploaddate={3}", resourceid, version, datafilename != null ? datafilename.Length.ToString() : "null", uploaddate), this, Logtype.Debug);
        }

        /// Updates a resource data entry in the database
        /// </summary>
        /// <param name="version"></param>
        /// <param name="datafilename"></param>
        /// <param name="uploaddate"></param>
        public void UpdateResourceData(int resourceid, int version, string datafilename)
        {
            logger.LogText(String.Format("Updating resource data: resourceid={0}, version={1}, datafilename={2}", resourceid, version, datafilename), this, Logtype.Debug);
            string query = "update resourcesdata set datafilename=@datafilename, uploaddate=@uploaddate where resourceid=@resourceid and version=@version";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{                
                new object[]{"@datafilename", datafilename},
                new object[]{"@resourceid", resourceid},
                new object[]{"@version", version},
                new object[]{"@uploaddate", DateTime.Now}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Updated resource data: resourceid={0}, version={1}, datafilename={2}", resourceid, version, datafilename != null ? datafilename.Length.ToString() : "null"), this, Logtype.Debug);
        }

        /// Updates a resource data entry in the database
        /// </summary>
        /// <param name="resourceid"></param>
        /// <param name="resourceversion"></param>
        /// <param name="publishstate"></param>
        public void UpdateResourceData(int resourceid, int version, PublishState publishstate)
        {
            logger.LogText(String.Format("Updating resourcedata: resourceid={0}, version={1}, publishstate={2}", resourceid, version, publishstate.ToString()), this, Logtype.Debug);
            string query = "update resourcesdata set publishstate=@publishstate where resourceid=@resourceid and version=@version";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{                
                new object[]{"@resourceid", resourceid},
                new object[]{"@version", version},
                new object[]{"@publishstate", publishstate.ToString()}                
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Updated resourcedata: resourceid={0}, version={1}, publishstate={2}", resourceid, version, publishstate.ToString()), this, Logtype.Debug);
        }

        /// <summary>
        /// Deletes a resource data entry in the database
        /// </summary>
        /// <param name="version"></param>
        /// <param name="data"></param>
        /// <param name="uploaddate"></param>
        public void DeleteResourceData(int resourceid, int version)
        {
            logger.LogText(String.Format("Deleting resource data: resourceid={0}, version={1}", resourceid, version), this, Logtype.Debug);
            string query = "delete from resourcesdata where resourceid=@resourceid and version=@version";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@resourceid", resourceid},
                new object[]{"@version", version},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(String.Format("Deleted resource data: resourceid={0}, version={1}", resourceid, version), this, Logtype.Debug);
        }

        /// <summary>
        /// Returns the dedicated resource data identified by its id
        /// Returns null, if the resource does not exist
        /// </summary>
        /// <param name="version"></param>
        /// <param name="data"></param>
        /// <param name="uploaddate"></param>
        public ResourceData GetResourceData(int resourceid, int version)
        {
            string query = "select resourceid, version, datafilename, uploaddate, publishstate from resourcesdata where resourceid=@resourceid and version=@version";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@resourceid", resourceid},
                new object[]{"@version", version},
            };

            var resultset = connection.ExecutePreparedStatement(query, parameters);

            if (resultset.Count == 0)
            {
                return null;
            }

            ResourceData resourceData = new ResourceData();
            resourceData.ResourceId = (int)resultset[0]["resourceid"];
            resourceData.ResourceVersion = (int)resultset[0]["version"];
            resourceData.DataFilename = (string)resultset[0]["datafilename"];
            resourceData.UploadDate = (DateTime)resultset[0]["uploaddate"];
            resourceData.PublishState = (string)resultset[0]["publishstate"];
            
            return resourceData;
        }

        /// <summary>
        /// Returns a list of resource data from the database
        /// If resourceid is set, it only returns resources of that user
        /// </summary>
        /// <param name="version"></param>
        /// <param name="data"></param>
        /// <param name="uploaddate"></param>
        public List<ResourceData> GetResourceDatas(int resourceid = -1)
        {
            string query;

            if (resourceid != -1)
            {
                query = "select resourceid, version, datafilename, uploaddate, publishstate from resourcesdata where resourceid=@resourceid";
            }
            else
            {
                query = "select resourceid, version, datafilename, uploaddate, publishstate from resourcesdata";
            }

            DatabaseConnection connection = GetConnection();

            object[][] parameters = null;

            if (resourceid != -1)
            {
                parameters = new object[][]{
                    new object[]{"@resourceid", resourceid}
                };
            }
            var resultset = connection.ExecutePreparedStatement(query, parameters);

            List<ResourceData> resourceDataList = new List<ResourceData>();

            foreach (var entry in resultset)
            {
                ResourceData resourceData = new ResourceData();
                resourceData.ResourceId = (int)entry["resourceid"];
                resourceData.ResourceVersion = (int)entry["version"];
                resourceData.DataFilename = (string)entry["datafilename"];
                resourceData.UploadDate = (DateTime)entry["uploaddate"];
                resourceData.PublishState = (string)entry["publishstate"];
                resourceDataList.Add(resourceData);
            }
            return resourceDataList;
        }

        #endregion
    }
}