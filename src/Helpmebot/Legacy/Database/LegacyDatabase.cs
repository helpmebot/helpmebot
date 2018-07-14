// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegacyDatabase.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Legacy.Database
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Net.Sockets;
    using System.Threading;
    using Castle.Core.Logging;
    using Helpmebot.Configuration;
    using MySql.Data.MySqlClient;

    public class LegacyDatabase : IDisposable, ILegacyDatabase
    {
        private readonly DatabaseConfiguration databaseConfiguration;
        private readonly ILogger logger;
        private MySqlConnection connection;

        public LegacyDatabase(ILogger logger, DatabaseConfiguration databaseConfiguration)
        {
            this.databaseConfiguration = databaseConfiguration;
            this.logger = logger.CreateChildLogger("Helpmebot.Legacy.Database.LegacyDatabase");
        }

        private bool Connect()
        {
            try
            {
                lock (this)
                {
                    this.logger.Info("Opening database connection...");

                    this.connection = new MySqlConnection(this.databaseConfiguration.ConnectionString);
                    this.connection.Open();
                }

                return true;
            }
            catch (MySqlException ex)
            {
                this.logger.Error(ex.Message, ex);
                return false;
            }
        }

        public void Initialize()
        {
            if (!this.Connect())
            {
                this.logger.Error("Could not connect using legacy database.");
                throw new Exception("Cannot connect using legacy database.");
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void ExecuteCommand(MySqlCommand command)
        {
            this.ExecuteNonQuery(ref command);
        }

        public string ExecuteScalarSelect(MySqlCommand cmd)
        {
            ArrayList al = this.ExecuteSelect(cmd);
            return al.Count > 0 ? ((object[]) al[0])[0].ToString() : string.Empty;
        }

        private ArrayList ExecuteSelect(MySqlCommand cmd)
        {
            var resultSet = new ArrayList();
            lock (this)
            {
                MySqlDataReader result = null;

                this.logger.Debug("Executing (reader)query: " + cmd.CommandText);

                try
                {
                    this.RunConnectionTest();
                    cmd.Connection = this.connection;

                    result = cmd.ExecuteReader();
                    this.logger.Debug("Done executing (reader)query: " + cmd.CommandText);
                }
                catch (Exception ex)
                {
                    this.logger.Error("Problem executing (reader)query", ex);
                }

                if (result == null)
                {
                    return resultSet;
                }

                try
                {
                    while (result.Read())
                    {
                        var row = new object[result.FieldCount];

                        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                        result.GetValues(row);
                        resultSet.Add(row);
                    }
                }
                catch (MySqlException ex)
                {
                    this.logger.Error(ex.Message, ex);
                    throw;
                }
                finally
                {
                    result.Close();
                }
            }

            return resultSet;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.connection.Dispose();
            }
        }

        private void ExecuteNonQuery(ref MySqlCommand cmd)
        {
            lock (this)
            {
                this.logger.Debug(string.Format("Executing non-query: {0}", cmd.CommandText));
                try
                {
                    this.RunConnectionTest();
                    cmd.Connection = this.connection;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex.Message, ex);
                }

                this.logger.Debug("Done executing query");
            }
        }

        private void RunConnectionTest()
        {
            // ok, first let's assume the connection is dead.
            var connectionOk = false;

            // first time through, skip the connection attempt
            var firstTime = true;
            var sleepTime = 1000;
            var totalTimeSlept = 0;

            while (!connectionOk || totalTimeSlept >= 180 /*seconds*/ * 1000 /*transform to milliseconds*/)
            {
                if (!firstTime)
                {
                    this.logger.Warn("Reconnecting to database...");

                    this.Connect();

                    Thread.Sleep(sleepTime);
                    totalTimeSlept += sleepTime;
                }

                while (this.connection.State == ConnectionState.Connecting)
                {
                    Thread.Sleep(100);
                    totalTimeSlept += 100;
                }

                connectionOk = (this.connection.State == ConnectionState.Open)
                               || (this.connection.State == ConnectionState.Fetching)
                               || (this.connection.State == ConnectionState.Executing);

                firstTime = false;
            }

            if (!connectionOk)
            {
                throw new SocketException();
            }
        }
    }
}