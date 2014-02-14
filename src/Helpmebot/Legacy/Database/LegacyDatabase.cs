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
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Net.Sockets;
    using System.Threading;

    using Castle.Core.Logging;

    using Helpmebot.Configuration;
    using Helpmebot.Configuration.XmlSections.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    using MySql.Data.MySqlClient;

    /// <summary>
    ///     Database access class
    /// </summary>
    public class LegacyDatabase : IDisposable, ILegacyDatabase
    {
        #region Static Fields

        /// <summary>
        ///     The _singleton.
        /// </summary>
        private static LegacyDatabase singleton;

        #endregion

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
        ///     Singletons the specified host.
        /// </summary>
        /// <returns>the instance</returns>
        public static LegacyDatabase Singleton()
        {
            return singleton
                   ?? (singleton =
                       new LegacyDatabase(
                           ServiceLocator.Current.GetInstance<ILogger>(), 
                           ServiceLocator.Current.GetInstance<IConfigurationHelper>()));
        }

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
                    var csb = new MySqlConnectionStringBuilder
                                  {
                                      Database = privateConfiguration.Schema, 
                                      Password = privateConfiguration.Password, 
                                      Server = privateConfiguration.Hostname, 
                                      UserID = privateConfiguration.Username, 
                                      Port = (uint)privateConfiguration.Port
                                  };

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

        /// <summary>
        ///     The where conditions.
        /// </summary>
        public struct WhereConds
        {
            #region Fields

            /// <summary>
            ///     The _comparer.
            /// </summary>
            private readonly string comparer;

            /// <summary>
            ///     The _a.
            /// </summary>
            private readonly string left;

            /// <summary>
            ///     The _quote a.
            /// </summary>
            private readonly bool quoteLeft;

            /// <summary>
            ///     The _quote b.
            /// </summary>
            private readonly bool quoteRight;

            /// <summary>
            ///     The _b.
            /// </summary>
            private readonly string right;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initialises a new instance of the <see cref="WhereConds"/> struct.
            /// </summary>
            /// <param name="leftNeedsQuoting">
            /// The a needs quoting.
            /// </param>
            /// <param name="left">
            /// The a.
            /// </param>
            /// <param name="comparer">
            /// The comparer.
            /// </param>
            /// <param name="rightNeedsQuoting">
            /// The b needs quoting.
            /// </param>
            /// <param name="right">
            /// The b.
            /// </param>
            public WhereConds(bool leftNeedsQuoting, string left, string comparer, bool rightNeedsQuoting, string right)
            {
                this.quoteLeft = leftNeedsQuoting;
                this.quoteRight = rightNeedsQuoting;
                this.left = left;
                this.right = right;
                this.comparer = comparer;
            }

            /// <summary>
            /// Initialises a new instance of the <see cref="WhereConds"/> struct.
            /// </summary>
            /// <param name="column">
            /// The column.
            /// </param>
            /// <param name="value">
            /// The value.
            /// </param>
            public WhereConds(string column, string value)
            {
                this.quoteLeft = false;
                this.quoteRight = true;
                this.left = column;
                this.right = value;
                this.comparer = "=";
            }

            /// <summary>
            /// Initialises a new instance of the <see cref="WhereConds"/> struct.
            /// </summary>
            /// <param name="column">
            /// The column.
            /// </param>
            /// <param name="value">
            /// The value.
            /// </param>
            public WhereConds(string column, int value)
            {
                this.quoteLeft = false;
                this.quoteRight = true;
                this.left = column;
                this.right = value.ToString(CultureInfo.InvariantCulture);
                this.comparer = "=";
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            ///     The to string.
            /// </summary>
            /// <returns>
            ///     The <see cref="string" />.
            /// </returns>
            public override string ToString()
            {
                string actualA = (this.quoteLeft ? "\"" : string.Empty) + MySqlHelper.EscapeString(this.left)
                                 + (this.quoteLeft ? "\"" : string.Empty);
                string actualB = (this.quoteRight ? "\"" : string.Empty) + MySqlHelper.EscapeString(this.right)
                                 + (this.quoteRight ? "\"" : string.Empty);
                string actualComp = MySqlHelper.EscapeString(this.comparer);
                return actualA + " " + actualComp + " " + actualB;
            }

            #endregion
        }

        /// <summary>
        ///     Class encapsulating a SELECT statement
        /// </summary>
        public class Select
        {
            #region Fields

            /// <summary>
            ///     The _fields.
            /// </summary>
            private readonly string[] fields;

            /// <summary>
            ///     The _groups.
            /// </summary>
            private readonly LinkedList<string> groups;

            /// <summary>
            ///     The having clauses.
            /// </summary>
            private readonly LinkedList<WhereConds> havings;

            /// <summary>
            ///     The _joins.
            /// </summary>
            private readonly LinkedList<Join> joins = new LinkedList<Join>();

            /// <summary>
            ///     The _orders.
            /// </summary>
            private readonly LinkedList<Order> orders;

            /// <summary>
            ///     The where clauses.
            /// </summary>
            private readonly LinkedList<WhereConds> wheres;

            /// <summary>
            ///     The _from.
            /// </summary>
            private string fromTable;

            /// <summary>
            ///     The _limit.
            /// </summary>
            private int limit;

            /// <summary>
            ///     The _offset.
            /// </summary>
            private int offset;

            /// <summary>
            ///     The _shall i escape selects.
            /// </summary>
            private bool shallIEscapeSelects = true;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initialises a new instance of the <see cref="Select"/> class.
            ///     Initializes a new instance of the <see cref="Select"/> class.
            /// </summary>
            /// <param name="fields">
            /// The fields to return.
            /// </param>
            public Select(params string[] fields)
            {
                this.fields = fields;
                this.fromTable = string.Empty;
                this.limit = this.offset = 0;
                this.joins = new LinkedList<Join>();
                this.wheres = new LinkedList<WhereConds>();
                this.groups = new LinkedList<string>();
                this.orders = new LinkedList<Order>();
                this.havings = new LinkedList<WhereConds>();
            }

            #endregion

            #region Enums

            /// <summary>
            ///     The join types.
            /// </summary>
            public enum JoinTypes
            {
                /// <summary>
                ///     The inner.
                /// </summary>
                Inner, 

                /// <summary>
                ///     The left.
                /// </summary>
                Left, 

                /// <summary>
                ///     The right.
                /// </summary>
                Right, 

                /// <summary>
                ///     The full outer.
                /// </summary>
                FullOuter
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// Adds a grouping.
            /// </summary>
            /// <param name="field">
            /// The field.
            /// </param>
            public void AddGroup(string field)
            {
                this.groups.AddLast(field);
            }

            /// <summary>
            /// Adds a having clause.
            /// </summary>
            /// <param name="conditions">
            /// The conditions.
            /// </param>
            public void AddHaving(WhereConds conditions)
            {
                this.havings.AddLast(conditions);
            }

            /// <summary>
            /// Adds a JOIN clause.
            /// </summary>
            /// <param name="table">
            /// The table.
            /// </param>
            /// <param name="joinType">
            /// Type of the join.
            /// </param>
            /// <param name="conditions">
            /// The conditions.
            /// </param>
            public void AddJoin(string table, JoinTypes joinType, WhereConds conditions)
            {
                this.joins.AddLast(new Join(joinType, table, conditions));
            }

            /// <summary>
            /// Adds a limit.
            /// </summary>
            /// <param name="lim">
            /// The limit.
            /// </param>
            /// <param name="off">
            /// The offset.
            /// </param>
            public void AddLimit(int lim, int off)
            {
                this.limit = lim;
                this.offset = off;
            }

            /// <summary>
            /// Adds the order.
            /// </summary>
            /// <param name="order">
            /// The order.
            /// </param>
            public void AddOrder(Order order)
            {
                this.orders.AddLast(order);
            }

            /// <summary>
            /// Adds a where clause.
            /// </summary>
            /// <param name="conditions">
            /// The conditions.
            /// </param>
            public void AddWhere(params WhereConds[] conditions)
            {
                foreach (WhereConds condition in conditions)
                {
                    this.wheres.AddLast(condition);
                }
            }

            /// <summary>
            /// Escape the selects?
            /// </summary>
            /// <param name="escape">
            /// if set to <c>true</c> [escape].
            /// </param>
            public void EscapeSelects(bool escape)
            {
                this.shallIEscapeSelects = escape;
            }

            /// <summary>
            /// Sets the from clause
            /// </summary>
            /// <param name="from">
            /// The table to pull from
            /// </param>
            /// <returns>
            /// The <see cref="Select"/>.
            /// </returns>
            public Select From(string from)
            {
                this.SetFrom(from);
                return this;
            }

            /// <summary>
            /// Sets from.
            /// </summary>
            /// <param name="from">
            /// The table to pull from .
            /// </param>
            public void SetFrom(string from)
            {
                this.fromTable = from;
            }

            /// <summary>
            ///     Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            ///     A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                string query = "SELECT ";
                bool firstField = true;
                foreach (string f in this.fields)
                {
                    if (!firstField)
                    {
                        query += ", ";
                    }

                    string fok = MySqlHelper.EscapeString(f);
                    if (!this.shallIEscapeSelects)
                    {
                        fok = f;
                    }

                    firstField = false;

                    query += fok;
                }

                if (this.fromTable != string.Empty)
                {
                    query += " FROM " + "`" + MySqlHelper.EscapeString(this.fromTable) + "`";
                }

                if (this.joins.Count != 0)
                {
                    foreach (Join item in this.joins)
                    {
                        switch (item.JoinType)
                        {
                            case JoinTypes.Inner:
                                query += " INNER JOIN ";
                                break;
                            case JoinTypes.Left:
                                query += " LEFT OUTER JOIN ";
                                break;
                            case JoinTypes.Right:
                                query += " RIGHT OUTER JOIN ";
                                break;
                            case JoinTypes.FullOuter:
                                query += " FULL OUTER JOIN ";
                                break;
                        }

                        query += "`" + MySqlHelper.EscapeString(item.Table) + "`";

                        query += " ON " + item.JoinConditions;
                    }
                }

                if (this.wheres.Count > 0)
                {
                    query += " WHERE ";

                    bool first = true;

                    foreach (WhereConds w in this.wheres)
                    {
                        if (!first)
                        {
                            query += " AND ";
                        }

                        first = false;
                        query += w.ToString();
                    }
                }

                if (this.groups.Count != 0)
                {
                    query += " GROUP BY ";
                    bool first = true;
                    foreach (string group in this.groups)
                    {
                        if (!first)
                        {
                            query += ", ";
                        }

                        first = false;
                        query += MySqlHelper.EscapeString(group);
                    }
                }

                if (this.orders.Count > 0)
                {
                    query += " ORDER BY ";

                    bool first = true;
                    foreach (Order order in this.orders)
                    {
                        if (!first)
                        {
                            query += ", ";
                        }

                        first = false;
                        query += order.ToString();
                    }
                }

                if (this.havings.Count > 0)
                {
                    query += " HAVING ";

                    bool first = true;

                    foreach (WhereConds w in this.havings)
                    {
                        if (!first)
                        {
                            query += " AND ";
                        }

                        first = false;
                        query += w.ToString();
                    }
                }

                if (this.limit != 0)
                {
                    query += " LIMIT " + this.limit;
                }

                if (this.offset != 0)
                {
                    query += " OFFSET " + this.offset;
                }

                query += ";";
                return query;
            }

            /// <summary>
            /// The where.
            /// </summary>
            /// <param name="conditions">
            /// The conditions.
            /// </param>
            /// <returns>
            /// The <see cref="Select"/>.
            /// </returns>
            public Select Where(params WhereConds[] conditions)
            {
                this.AddWhere(conditions);
                return this;
            }

            #endregion

            /// <summary>
            ///     The order.
            /// </summary>
            public struct Order
            {
                #region Fields

                /// <summary>
                ///     States whether the order is in ascending order.
                /// </summary>
                private readonly bool asc;

                /// <summary>
                ///     The _column.
                /// </summary>
                private readonly string column;

                /// <summary>
                ///     The _escape.
                /// </summary>
                private readonly bool escape;

                #endregion

                #region Constructors and Destructors

                /// <summary>
                /// Initialises a new instance of the <see cref="Order"/> struct.
                /// </summary>
                /// <param name="column">
                /// The column.
                /// </param>
                /// <param name="asc">
                /// The ascending order.
                /// </param>
                public Order(string column, bool asc)
                {
                    this.column = column;
                    this.asc = asc;
                    this.escape = true;
                }

                /// <summary>
                /// Initialises a new instance of the <see cref="Order"/> struct.
                /// </summary>
                /// <param name="column">
                /// The column.
                /// </param>
                /// <param name="asc">
                /// The ascending order.
                /// </param>
                /// <param name="escape">
                /// The escape.
                /// </param>
                public Order(string column, bool asc, bool escape)
                {
                    this.column = column;
                    this.asc = asc;
                    this.escape = escape;
                }

                #endregion

                #region Public Methods and Operators

                /// <summary>
                ///     The to string.
                /// </summary>
                /// <returns>
                ///     The <see cref="string" />.
                /// </returns>
                public override string ToString()
                {
                    return "`" + (this.escape ? MySqlHelper.EscapeString(this.column) : this.column) + "` "
                           + (this.asc ? "ASC" : "DESC");
                }

                #endregion
            }

            /// <summary>
            ///     The join.
            /// </summary>
            private struct Join
            {
                #region Fields

                /// <summary>
                ///     The join conditions.
                /// </summary>
                public readonly WhereConds JoinConditions;

                /// <summary>
                ///     The join type.
                /// </summary>
                public readonly JoinTypes JoinType;

                /// <summary>
                ///     The table.
                /// </summary>
                public readonly string Table;

                #endregion

                #region Constructors and Destructors

                /// <summary>
                /// Initialises a new instance of the <see cref="Join"/> struct.
                /// </summary>
                /// <param name="type">
                /// The type.
                /// </param>
                /// <param name="table">
                /// The table.
                /// </param>
                /// <param name="conditions">
                /// The conditions.
                /// </param>
                public Join(JoinTypes type, string table, WhereConds conditions)
                {
                    this.JoinType = type;
                    this.Table = table;
                    this.JoinConditions = conditions;
                }

                #endregion
            }
        }
    }
}