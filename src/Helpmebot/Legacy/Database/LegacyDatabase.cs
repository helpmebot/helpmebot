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
    using Helpmebot.Configuration.XmlSections.Interfaces;

    using MySql.Data.MySqlClient;

    /// <summary>
    ///     Database access class
    /// </summary>
    public class LegacyDatabase : IDisposable, ILegacyDatabase
    {
        #region Fields

        /// <summary>
        ///     The configuration helper.
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        ///     The connection.
        /// </summary>
        private MySqlConnection connection;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="LegacyDatabase"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="configurationHelper">
        /// The configuration Helper.
        /// </param>
        public LegacyDatabase(ILogger logger, IConfigurationHelper configurationHelper)
        {
            this.configurationHelper = configurationHelper;
            this.Log = logger.CreateChildLogger("Helpmebot.Legacy.Database.LegacyDatabase");
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Connects this instance to the database.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool Connect()
        {
            IPrivateConfiguration privateConfiguration = this.configurationHelper.PrivateConfiguration;

            try
            {
                lock (this)
                {
                    this.Log.Info("Opening database connection...");
                    var csb = privateConfiguration.ConnectionString;

                    this.connection = new MySqlConnection(csb.ConnectionString);
                    this.connection.Open();
                }

                return true;
            }
            catch (MySqlException ex)
            {
                this.Log.Error(ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The execute command.
        /// </summary>
        /// <param name="command">
        /// The delete command.
        /// </param>
        public void ExecuteCommand(MySqlCommand command)
        {
            this.ExecuteNonQuery(ref command);
        }

        /// <summary>
        /// The execute scalar select.
        /// </summary>
        /// <param name="cmd">
        /// The command.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ExecuteScalarSelect(MySqlCommand cmd)
        {
            ArrayList al = this.ExecuteSelect(cmd);
            return al.Count > 0 ? ((object[])al[0])[0].ToString() : string.Empty;
        }

        /// <summary>
        /// The execute select.
        /// </summary>
        /// <param name="cmd">
        /// The command.
        /// </param>
        /// <returns>
        /// The <see cref="ArrayList"/>.
        /// </returns>
        public ArrayList ExecuteSelect(MySqlCommand cmd)
        {
            var resultSet = new ArrayList();
            lock (this)
            {
                MySqlDataReader result = null;

                this.Log.Debug("Executing (reader)query: " + cmd.CommandText);

                try
                {
                    this.RunConnectionTest();
                    cmd.Connection = this.connection;

                    result = cmd.ExecuteReader();
                    this.Log.Debug("Done executing (reader)query: " + cmd.CommandText);
                }
                catch (Exception ex)
                {
                    this.Log.Error("Problem executing (reader)query", ex);
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
                    this.Log.Error(ex.Message, ex);
                    throw;
                }
                finally
                {
                    result.Close();
                }
            }

            return resultSet;
        }

        /// <summary>
        /// Call the HMB_GET_LOCAL_OPTION stored procedure.
        /// </summary>
        /// <param name="option">
        /// The option.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ProcHmbGetLocalOption(string option, string channel)
        {
            try
            {
                lock (this)
                {
                    this.RunConnectionTest();

                    var cmd = new MySqlCommand
                                  {
                                      Connection = this.connection, 
                                      CommandType = CommandType.StoredProcedure, 
                                      CommandText = "HMB_GET_LOCAL_OPTION"
                                  };

                    cmd.Parameters.AddWithValue("@optionName", option);
                    cmd.Parameters["@optionName"].Direction = ParameterDirection.Input;

                    cmd.Parameters.AddWithValue("@channel", channel);
                    cmd.Parameters["@channel"].Direction = ParameterDirection.Input;

                    cmd.Parameters.AddWithValue("@optionValue", string.Empty);
                    cmd.Parameters["@optionValue"].Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    return (string)cmd.Parameters["@optionValue"].Value;
                }
            }
            catch (FormatException ex)
            {
                this.Log.Error(ex.Message, ex);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                this.Log.Error(ex.Message, ex);
            }

            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.connection.Dispose();
            }
        }

        /// <summary>
        /// The execute non query.
        /// </summary>
        /// <param name="cmd">
        /// The command.
        /// </param>
        private void ExecuteNonQuery(ref MySqlCommand cmd)
        {
            lock (this)
            {
                this.Log.Debug(string.Format("Executing non-query: {0}", cmd.CommandText));
                try
                {
                    this.RunConnectionTest();
                    cmd.Connection = this.connection;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex.Message, ex);
                }

                this.Log.Debug("Done executing query");
            }
        }

        /// <summary>
        ///     The run connection test.
        /// </summary>
        private void RunConnectionTest()
        {
            // ok, first let's assume the connection is dead.
            bool connectionOk = false;

            // first time through, skip the connection attempt
            bool firstTime = true;

            int sleepTime = 1000;

            int totalTimeSlept = 0;

            while (!connectionOk || totalTimeSlept >= 180 /*seconds*/ * 1000 /*transform to milliseconds*/)
            {
                if (!firstTime)
                {
                    this.Log.Warn("Reconnecting to database...");

                    this.Connect();

                    Thread.Sleep(sleepTime);
                    totalTimeSlept += sleepTime;

                    sleepTime = (int)(sleepTime * 1.5) > int.MaxValue ? sleepTime : (int)(sleepTime * 1.5);
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

        #endregion
    }
}