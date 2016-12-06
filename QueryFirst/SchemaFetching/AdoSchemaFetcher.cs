using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Configuration;


namespace QueryFirst
{

    public class AdoSchemaFetcher : ISchemaFetcher
    {

        public List<ResultFieldDetails> GetFields(ConnectionStringSettings ConnectionString, string Query)
        {
            DataTable dt = new DataTable();
            var SchemaTable = GetQuerySchema(ConnectionString, Query);

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
                                    qf.ColumnName = SchemaTable.Rows[i].Field<string>(j);
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
                                if (ConnectionString.ProviderName == "System.Data.SqlClient")
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
                                if (ConnectionString.ProviderName == "System.Data.SqlClient")
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

        public AdoSchemaFetcher()
        {
        }

        //Perform the query, extract the results
        private DataTable GetQuerySchema(ConnectionStringSettings strconn, string strSQL)
        {
            //Returns a DataTable filled with the results of the query
            //Function returns the count of records in the datatable
            //----- dt (datatable) needs to be empty & no schema defined
            var provider = TinyIoC.TinyIoCContainer.Current.Resolve<IProvider>(strconn.ProviderName);

            using (var connection = provider.GetConnection(strconn))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = strSQL;
                    provider.PrepareParametersForSchemaFetching(command);
                    using (var srdrQuery = command.ExecuteReader(CommandBehavior.SchemaOnly))
                    {
                        var dtSchema = srdrQuery.GetSchemaTable();
                        return dtSchema;
                    }
                }
            }  // the connection will be closed & disposed here
        }
    }
}