using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using QueryFirst.Providers;

namespace QueryFirst
{

    public class AdoSchemaFetcher : ISchemaFetcher
    {
        /// <summary>
        /// At design time, our provider implementation will create us a connection.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        /// <param name="Query"></param>
        /// <returns></returns>
        public List<ResultFieldDetails> GetFields(string connectionString, string provider, string Query)
        {
            var prov = TinyIoC.TinyIoCContainer.Current.Resolve<IProvider>(provider);

            using (var connection = prov.GetConnection(connectionString))
            {
                connection.Open();
                return GetFields(connection, prov, Query);
            }
        }
        /// <summary>
        /// For SelfTest, the user's QfRuntimeConnection will be called by the generated code. We still need the provider
        /// </summary>
        /// <param name="connection">Must be open</param>
        /// <param name="provider">Must be a built-in provider. No DI for self-test.</param>
        /// <param name="Query">The query text</param>
        /// <returns></returns>
        public List<ResultFieldDetails> GetFields(IDbConnection connection, string provider, string Query)
        {
            // how do I be registering with tiny inside someone else's program ?
            IProvider prov = null;
            switch (provider)
            {
                case "System.Data.SqlClient":
                    prov = new SqlClient();
                    break;
                case "Npgsql":
                    prov = new Providers.Npgsql();
                    break;
                case "MySql.Data.MySqlClient":
                    prov = new MySqlClient();
                    break;
            }
            if(connection.State != ConnectionState.Open)
                connection.Open();
            return GetFields(connection, prov, Query);
        }


        private List<ResultFieldDetails> GetFields(IDbConnection connection, IProvider provObj, string Query)
        {
            DataTable dt = new DataTable();
            var SchemaTable = GetQuerySchema(connection, provObj, Query);

            List<ResultFieldDetails> result = new List<ResultFieldDetails>();
            if (SchemaTable == null)
                return result;


            for (int i = 0; i <= SchemaTable.Rows.Count - 1; i++)
            {
                var qf = new ResultFieldDetails();
                string properties = string.Empty;
                for (int j = 0; j <= SchemaTable.Columns.Count - 1; j++)
                {
                    properties += SchemaTable.Columns[j].ColumnName + (char)254 + SchemaTable.Rows[i].ItemArray[j].ToString();
                    if (j < SchemaTable.Columns.Count - 1)
                        properties += (char)255;

                    if (SchemaTable.Rows[i].ItemArray[j] != DBNull.Value)
                    {
                        switch (SchemaTable.Columns[j].ColumnName)
                        {
                            case "ColumnName":
                                // sby : ColumnName might be null, in which case it will be created from ordinal.
                                if (!string.IsNullOrEmpty(SchemaTable.Rows[i].Field<string>(j)))
                                {
                                    var colName = SchemaTable.Rows[i].Field<string>(j);
                                    qf.ColumnName = colName.StartsWith("JSON_") ? "Json" : colName; // rename column for 'FOR JSON' queries
                                }
                                break;
                            case "ColumnOrdinal":
                                qf.ColumnOrdinal = (int)SchemaTable.Rows[i].Field<int>(j);
                                if (string.IsNullOrEmpty(qf.ColumnName))
                                    qf.ColumnName = "col" + qf.ColumnOrdinal.ToString();
                                break;
                            case "ColumnSize":
                                qf.ColumnSize = (int)SchemaTable.Rows[i].Field<int>(j);
                                break;
                            case "NumericPrecision":
                                // Postgres choking
                                if (connection.GetType().Name == "SqlConnection")
                                {
                                    qf.NumericPrecision = (int)SchemaTable.Rows[i].Field<short>(j);
                                }
                                else
                                {
                                    qf.NumericPrecision = (int)SchemaTable.Rows[i].Field<int>(j);
                                }
                                break;
                            case "NumericScale":
                                // Postgres choking
                                if (connection.GetType().Name == "SqlConnection")
                                {
                                    qf.NumericScale = (int)SchemaTable.Rows[i].Field<short>(j);
                                }
                                else
                                {
                                    qf.NumericScale = (int)SchemaTable.Rows[i].Field<int>(j);
                                }
                                break;
                            case "IsUnique":
                                qf.IsUnique = SchemaTable.Rows[i].Field<bool>(j);
                                break;
                            case "BaseColumnName":
                                qf.BaseColumnName = SchemaTable.Rows[i].Field<string>(j);
                                break;
                            case "BaseTableName":
                                qf.BaseTableName = SchemaTable.Rows[i].Field<string>(j);
                                break;
                            case "DataType":
                                qf.TypeCs = SchemaTable.Rows[i].Field<System.Type>(j).FullName;
                                break;
                            case "AllowDBNull":
                                qf.AllowDBNull = SchemaTable.Rows[i].Field<bool>(j);
                                break;
                            case "ProviderType":
                                //qf.ProviderType = SchemaTable.Rows[i].Field<int>(j);
                                break;
                            case "IsIdentity":
                                qf.IsIdentity = SchemaTable.Rows[i].Field<bool>(j);
                                break;
                            case "IsAutoIncrement":
                                qf.IsAutoIncrement = SchemaTable.Rows[i].Field<bool>(j);
                                break;
                            case "IsRowVersion":
                                qf.IsRowVersion = SchemaTable.Rows[i].Field<bool>(j);
                                break;
                            case "IsLong":
                                qf.IsLong = SchemaTable.Rows[i].Field<bool>(j);
                                break;
                            case "IsReadOnly":
                                qf.IsReadOnly = SchemaTable.Rows[i].Field<bool>(j);
                                break;
                            case "ProviderSpecificDataType":
                                qf.ProviderSpecificDataType = SchemaTable.Rows[i].Field<System.Type>(j).FullName;
                                break;
                            case "DataTypeName":
                                qf.TypeDb = SchemaTable.Rows[i].Field<string>(j);
                                break;
                            case "UdtAssemblyQualifiedName":
                                qf.UdtAssemblyQualifiedName = SchemaTable.Rows[i].Field<string>(j);
                                break;
                            case "IsColumnSet":
                                qf.IsColumnSet = SchemaTable.Rows[i].Field<bool>(j);
                                break;
                            case "NonVersionedProviderType":
                                qf.NonVersionedProviderType = SchemaTable.Rows[i].Field<int>(j);
                                break;
                            default:
                                break;
                        }
                    }
                }
                qf.RawProperties = properties;
                result.Add(qf);
            }

            return result;
        }


        // Perform the query, extract the results
        private DataTable GetQuerySchema(IDbConnection connection, IProvider prov, string strSQL)
        {
            // Returns a DataTable filled with the results of the query
            // Function returns the count of records in the datatable
            // ----- dt (datatable) needs to be empty & no schema defined
            // we can't put provider in the constructor because we need the provider name to resolve.

            using (var command = connection.CreateCommand())
            {
                command.CommandText = strSQL;
                prov.PrepareParametersForSchemaFetching(command);
                using (var srdrQuery = command.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    var dtSchema = srdrQuery.GetSchemaTable();
                    return dtSchema;
                }
            }
        }
    }
}