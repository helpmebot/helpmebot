namespace Helpmebot.Legacy.Database
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IDAL
    {
        /// <summary>
        /// Connects this instance to the database.
        /// </summary>
        /// <returns></returns>
        bool connect();

        /// <summary>
        /// Inserts values the specified table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        long insert(string table, params string[] values);

        /// <summary>
        /// Deletes from the specified table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="conditions">The conditions.</param>
        bool delete(string table, int limit, params DAL.WhereConds[] conditions);

        /// <summary>
        /// Updates rows in the specified table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="items">The items.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="conditions">The conditions.</param>
        bool update(string table, Dictionary<string, string> items, int limit, params DAL.WhereConds[] conditions);

        /// <summary>
        /// Executes the select.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Arraylist of arrays. Each array is one row in the dataset.</returns>
        ArrayList executeSelect(DAL.Select query);

        /// <summary>
        /// Executes the select.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="columns">A list of column names</param>
        /// <returns>Arraylist of arrays. Each array is one row in the dataset.</returns>
        ArrayList executeSelect(DAL.Select query, out List<string> columns);

        /// <summary>
        /// Executes the scalar select.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>A single value as a string</returns>
        string executeScalarSelect(DAL.Select query);

        /// <summary>
        /// Executes the stored procedure.
        /// </summary>
        /// <param name="name">The procedure name.</param>
        /// <param name="args">The args.</param>
        void executeProcedure(string name, params string[] args);

        string proc_HMB_GET_LOCAL_OPTION(string option, string channel)
            // ReSharper restore InconsistentNaming
            ;

        string proc_HMB_GET_MESSAGE_CONTENT(string title)
            // ReSharper restore InconsistentNaming
            ;

        string proc_HMB_GET_IW_URL(string prefix)
            // ReSharper restore InconsistentNaming
            ;
    }
}