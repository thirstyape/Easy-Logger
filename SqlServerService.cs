using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace Easy_Logger
{
    /// <summary>
    /// Provides methods to easily facilitate SQL functions
    /// </summary>
    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Class is known to allow unsafe sql, developer must use at own risk")]
    public class SqlServerService
    {
        private readonly SqlConnection connection;
        private SqlCommand command;

        /// <summary>
        /// Prepares the class to run SQL queries
        /// </summary>
        /// <param name="connectionString">The details to connect to the database with</param>
        public SqlServerService(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        /// <summary>
        /// Prepares the class to run SQL queries
        /// </summary>
        /// <param name="connectionString">The details to connect to the database with</param>
        public SqlServerService(SqlConnectionStringBuilder connectionString)
        {
            connection = new SqlConnection(connectionString.ToString());
        }

        /// <summary>
        /// Prepares the class to run SQL queries
        /// </summary>
        /// <param name="server">The DNS name or IP address of the server to connect to</param>
        /// <param name="database">The name of the database to run queries against</param>
        /// <param name="username">The username to access the server</param>
        /// <param name="password">The password to access the server</param>
        /// <param name="integratedSecurity">If true will ignore username and password and use Windows integrated security</param>
        public SqlServerService(string server, string database, string username, string password, bool integratedSecurity)
        {
            var connectionString = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                UserID = username,
                Password = password,
                IntegratedSecurity = integratedSecurity
            };

            connection = new SqlConnection(connectionString.ToString());
        }

        ~SqlServerService()
        {
            if (connection.State == ConnectionState.Open)
                connection.Close();
        }


        /// <summary>
        /// Sends the provided SQL text to the database in an ExecuteReader command
        /// </summary>
        /// <param name="sql">The query text to run</param>
        /// <param name="parameters">Parameters to apply to the query</param>
        /// <param name="commandType">The type of query to run</param>
        /// <exception cref="DataException"></exception>
        public IDataReader ExecuteReader(string sql, List<SqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            SqlDataReader dataReader = null;

            try
            {
                Prepare(sql, parameters, commandType);

                dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception e)
            {
                throw new DataException("Unhandled exception querying database", e);
            }
            finally
            {
                Cleanup(parameters);
            }

            return dataReader;
        }

        /// <summary>
        /// Sends the provided SQL text to the database in an ExecuteAdapter command
        /// </summary>
        /// <param name="sql">The query text to run</param>
        /// <param name="parameters">Parameters to apply to the query</param>
        /// <param name="commandType">The type of query to run</param>
        /// <exception cref="DataException"></exception>
        public DataTable ExecuteAdapter(string sql, List<SqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            SqlDataAdapter dataAdapter = null;

            var data = new DataTable();

            try
            {
                Prepare(sql, parameters, commandType);

                dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(data);
            }
            catch (Exception e)
            {
                throw new DataException("Unhandled exception querying database", e);
            }
            finally
            {
                if (dataAdapter != null)
                    dataAdapter.Dispose();

                Cleanup(parameters);
            }

            return data;
        }

        /// <summary>
        /// Sends the provided SQL text to the database in an ExecuteScalar command
        /// </summary>
        /// <param name="sql">The query text to run</param>
        /// <param name="parameters">Parameters to apply to the query</param>
        /// <param name="commandType">The type of query to run</param>
        /// <exception cref="DataException"></exception>
        public object ExecuteScalar(string sql, List<SqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            object data = null;

            try
            {
                Prepare(sql, parameters, commandType);

                data = command.ExecuteScalar();
            }
            catch (Exception e)
            {
                throw new DataException("Unhandled exception querying database", e);
            }
            finally
            {
                Cleanup(parameters);
            }

            return data;
        }

        /// <summary>
        /// Sends the provided SQL text to the database in an ExecuteNonQuery command
        /// </summary>
        /// <param name="sql">The query text to run</param>
        /// <param name="parameters">Parameters to apply to the query</param>
        /// <param name="commandType">The type of query to run</param>
        /// <exception cref="DataException"></exception>
        public int ExecuteNonQuery(string sql, List<SqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            SqlTransaction transaction = null;

            var rows = -1;

            try
            {
                Prepare(sql, parameters, commandType);

                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                command.Transaction = transaction;

                rows = command.ExecuteNonQuery();

                transaction.Commit();
            }
            catch (Exception e)
            {
                try
                {
                    if (transaction != null)
                        transaction.Rollback();
                }
                catch
                {
                    throw new DataException("Unhandled exception running transaction on database, rollback failed.", e);
                }

                if (transaction != null)
                    throw new DataException("Unhandled exception running transaction on database, rollback succeeded.", e);
                else
                    throw new DataException("Unhandled exception running transaction on database, rollback unavailable.", e);
            }
            finally
            {
                Cleanup(parameters);

                if (transaction != null)
                    transaction.Dispose();
            }

            return rows;
        }

        /// <summary>
        /// Prepares a query to be run on the database
        /// </summary>
        /// <param name="sql">The query text to run</param>
        /// <param name="parameters">Parameters to apply to the query</param>
        /// <param name="commandType">The type of query to run</param>
        private void Prepare(string sql, List<SqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            command = new SqlCommand()
            {
                CommandText = sql,
                CommandType = commandType,
                Connection = connection
            };

            if (parameters != null && parameters.Count > 0)
                command.Parameters.AddRange(parameters.ToArray());

            if (connection.State != ConnectionState.Open)
                connection.Open();
        }

        /// <summary>
        /// Disposes of active objects and clears provided parameters
        /// </summary>
        /// <param name="parameters">Parameters to apply to the query</param>
        private void Cleanup(List<SqlParameter> parameters = null)
        {
            if (connection.State == ConnectionState.Open)
                connection.Close();

            if (parameters != null && parameters.Count > 0)
                command.Parameters.Clear();

            if (command != null)
                command.Dispose();
        }
    }
}
