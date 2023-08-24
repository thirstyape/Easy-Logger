using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Easy_Logger_Sql
{
    /// <summary>
    /// Facilitates the connection to SQL Server
    /// </summary>
    internal class SqlConnectionService
    {
        private readonly SqlConnectionStringBuilder ConnectionString;
        
        public SqlConnectionService(SqlConnectionStringBuilder connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Sends the provided SQL text to the database in an ExecuteNonQuery command
        /// </summary>
        /// <param name="sql">The query text to run</param>
        /// <param name="parameters">Parameters to apply to the query</param>
        /// <param name="commandType">The type of query to run</param>
        /// <exception cref="DataException"></exception>
        public int ExecuteNonQuery(string sql, IEnumerable<SqlParameter>? parameters = null, CommandType commandType = CommandType.Text)
        {
            SqlCommand? command = null;
            var rowCount = -1;

            try
            {
                command = new SqlCommand()
                {
                    CommandText = sql,
                    CommandType = commandType,
                    Connection = new SqlConnection(ConnectionString.ToString())
                };

                if (parameters != null && parameters.Any())
                    command.Parameters.AddRange(parameters.ToArray());

                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();

                command.Transaction = command.Connection.BeginTransaction(IsolationLevel.ReadCommitted);
                rowCount = command.ExecuteNonQuery();
                command.Transaction.Commit();
            }
            catch (Exception e)
            {
                try
                {
                    command?.Transaction?.Rollback();
                }
                catch
                {
                    throw new DataException("Unhandled exception running transaction on database, rollback failed.", e);
                }

                if (command?.Transaction != null)
                    throw new DataException("Unhandled exception running transaction on database, rollback succeeded.", e);
                else
                    throw new DataException("Unhandled exception running transaction on database, rollback unavailable.", e);
            }
            finally
            {
                if (command != null)
                {
                    if (command.Connection.State == ConnectionState.Open)
                        command.Connection.Close();

                    if (parameters != null && parameters.Any())
                        command.Parameters.Clear();

                    command.Transaction?.Dispose();
                    command.Dispose();
                }
            }

            return rowCount;
        }
    }
}
