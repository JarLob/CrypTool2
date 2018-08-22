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
using CrypToolStoreLib.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

public class DatabaseConnection : IDisposable
{
    private Logger logger = Logger.GetLogger();
    private string databaseServer;
    private string databaseName;
    private string databaseUser;
    private string databasePassword;

    private MySqlConnection mySqlConnection;

    private Dictionary<string, MySqlCommand> preparedStatementCache = new Dictionary<string,MySqlCommand>();

    private volatile bool currentlyUsed = false;

    /// <summary>
    /// Constructor for a connection to a mysql database
    /// </summary>
    /// <param name="databaseServer"></param>
    /// <param name="databaseName"></param>
    /// <param name="databaseUser"></param>
    /// <param name="databasePassword"></param>
    public DatabaseConnection(string databaseServer, string databaseName, string databaseUser, string databasePassword)
	{
        this.databaseServer = databaseServer;        
        this.databaseName = databaseName;
        this.databaseUser = databaseUser;
        this.databasePassword = databasePassword;
	}

    /// <summary>
    /// Connect to defined database
    /// </summary>
    public void Connect()
    {
        lock (this)
        {
            if (mySqlConnection != null)
            {
                return;
            }
            logger.LogText(String.Format("Connecting to mysql database={0} on server={1} with username={2}", databaseName, databaseServer, databaseUser), this, Logtype.Debug);
            string connectstring = string.Format("Server={0}; database={1}; UID={2}; password={3}", databaseServer, databaseName, databaseUser, databasePassword);
            mySqlConnection = new MySqlConnection(connectstring);
            mySqlConnection.Open();
            logger.LogText(String.Format("Connected to mysql database={0} on server={1} with username={2}", databaseName, databaseServer, databaseUser), this, Logtype.Debug);
        }
    }

    /// <summary>
    /// Close connection to database
    /// </summary>
    public void Close()
    {
        lock (this)
        {
            if (mySqlConnection == null)
            {
                return;
            }
            logger.LogText("Disconnecting from mysql database", this, Logtype.Debug);
            mySqlConnection.Close();
            mySqlConnection = null;
            logger.LogText("Disconnected from mysql database", this, Logtype.Debug);
        }
    }

    /// <summary>
    /// Executes prepared statements
    /// Also caches the prepared statement and reuses cached ones
    /// 
    /// we only use prepared statements for several reasons
    ///  1) security - avoid sql injections
    ///  2) performance
    ///  
    /// </summary>
    /// <param name="query"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<Dictionary<string,object>> ExecutePreparedStatement(string query, object[][] parameters)
    {
        lock (this)
        {
            if (mySqlConnection == null)
            {
                throw new Exception("Not connected");
            }
        
            try
            {
                currentlyUsed = true;
                List<Dictionary<string, object>> resultset = new List<Dictionary<string, object>>();
                //Step 0: Create a prepared statement for cache, if it does not exist
                if (!preparedStatementCache.ContainsKey(query))
                {
                    logger.LogText(String.Format("Creating prepared statement and putting it into cache: {0}", query), this, Logtype.Debug);
                    MySqlCommand command = new MySqlCommand(query, mySqlConnection);
                    command.Prepare();
                    preparedStatementCache[query] = command;
                }

                //Step 1: Get prepared statement from cache. Also clear parameters of prepared statement
                logger.LogText(String.Format("Fetching prepared statement from cache: {0}", query), this, Logtype.Debug);
                MySqlCommand mySqlCommand = preparedStatementCache[query];
                mySqlCommand.Parameters.Clear();

                //Step 2: Fill parameters of prepared statement
                logger.LogText(String.Format("Filling parameters of prepared statement: {0}", query), this, Logtype.Debug);
                if (parameters != null && parameters.Length > 0)
                {
                    foreach (object[] o in parameters)
                    {
                        mySqlCommand.Parameters.AddWithValue(o[0].ToString(), o[1]);
                    }
                }

                //Step 3: Execute prepared statement and fetch results
                logger.LogText(String.Format("Executing prepared statement: {0}", query), this, Logtype.Debug);
                using (var reader = mySqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            int fieldCount = reader.FieldCount;

                            Dictionary<string, object> row = new Dictionary<string, object>();

                            for (int i = 0; i < fieldCount; i++)
                            {
                                string field_name = reader.GetName(i);
                                object field_value = reader.GetValue(i);


                                row.Add(field_name, field_value);
                            }
                            resultset.Add(row);
                        }
                    }
                }
                logger.LogText(String.Format("Returning result of prepared statement: {0}", query), this, Logtype.Debug);
                return resultset;
            }
            finally
            {
                currentlyUsed = false;
            }
        }        
    }

    /// <summary>
    /// Disposing this DatabaseConnection
    /// Closes the connection to the mysql database
    /// </summary>
    public void Dispose()
    {
        Close();
    }

    /// <summary>
    /// Returns true if this object is currently used
    /// </summary>
    /// <returns></returns>
    public bool CurrentlyUsed()
    {
        return currentlyUsed;
    }
}
