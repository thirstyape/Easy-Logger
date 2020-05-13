using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace Easy_Logger
{
    /// <summary>
    /// 
    /// </summary>
    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Class is known to allow unsafe sql, developer must use at own risk")]
    public class SqlServerService
    {
        private readonly SqlConnection connection;
        private SqlCommand command;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlServerService(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlServerService(SqlConnectionStringBuilder connectionString)
        {
            connection = new SqlConnection(connectionString.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="integratedSecurity"></param>
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
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
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
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
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
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
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
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
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
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
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
        /// 
        /// </summary>
        /// <param name="parameters"></param>
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
