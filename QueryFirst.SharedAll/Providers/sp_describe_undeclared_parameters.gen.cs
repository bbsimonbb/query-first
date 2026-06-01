using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;

namespace QueryFirst.Providers
{
    public interface Isp_describe_undeclared_parameters
    {

        int ExecuteNonQuery(string tsql);
        int ExecuteNonQuery(string tsql, SqlConnection conn);
    }
    public class sp_describe_undeclared_parameters : Isp_describe_undeclared_parameters
    {
        public virtual int ExecuteNonQuery(string tsql)
        {
            using (SqlConnection conn = new SqlConnection(QfRuntimeConnection.GetConnectionString()))
            {
                conn.Open();
                return ExecuteNonQuery(tsql, conn);
            }
        }
        public virtual int ExecuteNonQuery(string tsql, SqlConnection conn)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = getCommandText();
            cmd.Parameters.Add("@tsql", SqlDbType.VarChar, 0).Value = tsql != null ? (object)tsql : DBNull.Value;
            return cmd.ExecuteNonQuery();
        }
        private string getCommandText()
        {
            Stream strm = typeof(sp_describe_undeclared_parametersResults).Assembly.GetManifestResourceStream("QueryFirst.Providers.sp_describe_undeclared_parameters.sql");
            string queryText = new StreamReader(strm).ReadToEnd();
            //Comments inverted at runtime in debug, pre-build in release
            queryText = queryText.Replace("-- designTime", "/*designTime");
            queryText = queryText.Replace("-- endDesignTime", "endDesignTime*/");
            return queryText;
        }
    }
}
