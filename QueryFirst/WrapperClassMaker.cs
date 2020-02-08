using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    public class WrapperClassMaker : IWrapperClassMaker
    {
        public virtual string Usings(State state)
        {
            return @"using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

";

        }
        public virtual string StartNamespace(State state)
        {
            if (!string.IsNullOrEmpty(state._2Namespace))
                return "namespace " + state._2Namespace + "{" + Environment.NewLine;
            else
                return "";
        }
        public virtual string StartClass(State state)
        {
            return "public partial class " + state._1BaseName + " : I" + state._1BaseName + "{" + Environment.NewLine;

        }
        public virtual string MakeExecuteWithoutConn(State state)
        {
            StringBuilder code = new StringBuilder();
            char[] spaceComma = new char[] { ',', ' ' };
            // Execute method, without connection
            code.AppendLine("public virtual List<" + state._2ResultInterfaceName + "> Execute(" + state._8MethodSignature.Trim(spaceComma) + "){");
            code.AppendLine("using (IDbConnection conn = QfRuntimeConnection.GetConnection())");
            code.AppendLine("{");
            code.AppendLine("conn.Open();");
            code.AppendLine("return Execute(" + state._8CallingArgs + " conn).ToList();");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }
        public virtual string MakeExecuteWithConn(State state)
        {
            StringBuilder code = new StringBuilder();
            // Execute method with connection
            code.AppendLine("public virtual IEnumerable<" + state._2ResultInterfaceName + "> Execute(" + state._8MethodSignature + "IDbConnection conn, IDbTransaction tx = null){");
            code.AppendLine("IDbCommand cmd = conn.CreateCommand();");
            code.AppendLine("if(tx != null)");
            code.AppendLine("cmd.Transaction = tx;");
            code.AppendLine("cmd.CommandText = getCommandText();");
            foreach (var qp in state._8QueryParams)
            {
                code.AppendLine("AddAParameter(cmd, \"" + qp.DbType + "\", \"" + qp.DbName + "\", " + qp.CSName + ", " + qp.Length + ", " + qp.Scale + ", " + qp.Precision + ");");
            }
            code.AppendLine("using (var reader = cmd.ExecuteReader())");
            code.AppendLine("{");
            code.AppendLine("while (reader.Read())");
            code.AppendLine("{");
            code.AppendLine("yield return Create(reader);");
            code.AppendLine("}");
            code.AppendLine("}");
            code.AppendLine("}"); //close Execute() method
            return code.ToString();
        }
        public virtual string MakeGetOneWithoutConn(State state)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            // GetOne without connection
            code.AppendLine("public virtual " + state._2ResultInterfaceName + " GetOne(" + state._8MethodSignature.Trim(spaceComma) + "){");
            code.AppendLine("using (IDbConnection conn = QfRuntimeConnection.GetConnection())");
            code.AppendLine("{");
            code.AppendLine("conn.Open();");
            code.AppendLine("return GetOne(" + state._8CallingArgs + " conn);");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();

        }
        public virtual string MakeGetOneWithConn(State state)
        {
            StringBuilder code = new StringBuilder();
            // GetOne() with connection
            code.AppendLine("public virtual " + state._2ResultInterfaceName + " GetOne(" + state._8MethodSignature + "IDbConnection conn, IDbTransaction tx = null)");
            code.AppendLine("{");
            code.AppendLine("var all = Execute(" + state._8CallingArgs + " conn,tx);");
            code.AppendLine("using (IEnumerator<" + state._2ResultInterfaceName + "> iter = all.GetEnumerator())");
            code.AppendLine("{");
            code.AppendLine("iter.MoveNext();");
            code.AppendLine("return iter.Current;");
            code.AppendLine("}");
            code.AppendLine("}"); // close GetOne() method
            return code.ToString();

        }
        public virtual string MakeExecuteScalarWithoutConn(State state)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            //ExecuteScalar without connection
            code.AppendLine("public virtual " + state._7ExecuteScalarReturnType + " ExecuteScalar(" + state._8MethodSignature.Trim(spaceComma) + "){");
            code.AppendLine("using (IDbConnection conn = QfRuntimeConnection.GetConnection())");
            code.AppendLine("{");
            code.AppendLine("conn.Open();");
            code.AppendLine("return ExecuteScalar(" + state._8CallingArgs + " conn);");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }
        public virtual string MakeExecuteScalarWithConn(State state)
        {
            StringBuilder code = new StringBuilder();
            // ExecuteScalar() with connection
            code.AppendLine("public virtual " + state._7ExecuteScalarReturnType + " ExecuteScalar(" + state._8MethodSignature + "IDbConnection conn, IDbTransaction tx = null){");
            code.AppendLine("IDbCommand cmd = conn.CreateCommand();");
            code.AppendLine("if(tx != null)");
            code.AppendLine("cmd.Transaction = tx;");
            code.AppendLine("cmd.CommandText = getCommandText();");
            foreach (var qp in state._8QueryParams)
            {
                code.AppendLine("AddAParameter(cmd, \"" + qp.DbType + "\", \"" + qp.DbName + "\", " + qp.CSName + ", " + qp.Length + ", " + qp.Scale + ", " + qp.Precision + ");");
            }
            code.AppendLine("var result = cmd.ExecuteScalar();");
            // only convert dbnull if nullable

            code.AppendLine("if( result == null || result == DBNull.Value)");
                code.AppendLine("return null;");
            code.AppendLine("else");
            code.AppendLine("return (" + state._7ExecuteScalarReturnType + ")result;");
            code.AppendLine("}");
            // close ExecuteScalar()
            return code.ToString();

        }
        public virtual string MakeExecuteNonQueryWithoutConn(State state)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            //ExecuteScalar without connection
            code.AppendLine("public virtual int ExecuteNonQuery(" + state._8MethodSignature.Trim(spaceComma) + "){");
            code.AppendLine("using (IDbConnection conn = QfRuntimeConnection.GetConnection())");
            code.AppendLine("{");
            code.AppendLine("conn.Open();");
            code.AppendLine("return ExecuteNonQuery(" + state._8CallingArgs + " conn);");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }
        public virtual string MakeExecuteNonQueryWithConn(State state)
        {
            StringBuilder code = new StringBuilder();
            // ExecuteScalar() with connection
            code.AppendLine("public virtual int ExecuteNonQuery(" + state._8MethodSignature + "IDbConnection conn, IDbTransaction tx = null){");
            code.AppendLine("IDbCommand cmd = conn.CreateCommand();");
            code.AppendLine("if(tx != null)");
            code.AppendLine("cmd.Transaction = tx;");
            code.AppendLine("cmd.CommandText = getCommandText();");
            foreach (var qp in state._8QueryParams)
            {
                code.AppendLine("AddAParameter(cmd, \"" + qp.DbType + "\", \"" + qp.DbName + "\", " + qp.CSName + ", " + qp.Length + ", " + qp.Scale + ", " + qp.Precision + ");");
            }
            code.AppendLine("return cmd.ExecuteNonQuery();");
            code.AppendLine("}");
            // close ExecuteScalar()
            return code.ToString();

        }

        public virtual string MakeCreateMethod(State state)
        {
            StringBuilder code = new StringBuilder();
            // Create() method
            code.AppendLine("public virtual " + state._2ResultInterfaceName + " Create(IDataRecord record)");
            code.AppendLine("{");
            code.AppendLine("var returnVal = CreatePoco(record);");
            int j = 0;
            foreach (var col in state._7ResultFields)
            {
                code.AppendLine("if(record[" + j + "] != null && record[" + j + "] != DBNull.Value)");
                code.AppendLine("returnVal." + col.CSColumnName + " =  (" + col.TypeCsShort + ")record[" + j++ + "];");
            }
            // call OnLoad method in user's half of partial class
            code.AppendLine("returnVal.OnLoad();");
            code.AppendLine("return returnVal;");

            code.AppendLine("}"); // close method;

            return code.ToString();
        }
        public virtual string MakeGetCommandTextMethod(State state)
        {
            StringBuilder code = new StringBuilder();
            // public load command text
            code.AppendLine("public string getCommandText(){");
            code.AppendLine("return @\"");
            code.Append(state._6FinalQueryTextForCode);
            code.AppendLine("\";");
            code.AppendLine("}"); // close method;
            return code.ToString();

        }
        public virtual string MakeOtherMethods(State state)
        {
            return "";
        }
        public virtual string CloseClass(State state)
        {
            return "}" + Environment.NewLine;
        }
        public virtual string CloseNamespace(State state)
        {
            if (!string.IsNullOrEmpty(state._2Namespace))
                return "}" + Environment.NewLine;
            else
                return "";
        }

        public string MakeInterface(State state)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            code.AppendLine("public interface I" + state._1BaseName + "{" + Environment.NewLine);
            if (state._7ResultFields != null && state._7ResultFields.Count > 0)
            {
                code.AppendLine("List<" + state._2ResultInterfaceName + "> Execute(" + state._8MethodSignature.Trim(spaceComma) + ");");
                code.AppendLine("IEnumerable<" + state._2ResultInterfaceName + "> Execute(" + state._8MethodSignature + "IDbConnection conn, IDbTransaction tx = null);");
                code.AppendLine("" + state._2ResultInterfaceName + " GetOne(" + state._8MethodSignature.Trim(spaceComma) + ");");
                code.AppendLine("" + state._2ResultInterfaceName + " GetOne(" + state._8MethodSignature + "IDbConnection conn, IDbTransaction tx = null);");
                code.AppendLine("" + state._7ExecuteScalarReturnType + " ExecuteScalar(" + state._8MethodSignature.Trim(spaceComma) + ");");
                code.AppendLine("" + state._7ExecuteScalarReturnType + " ExecuteScalar(" + state._8MethodSignature + "IDbConnection conn, IDbTransaction tx = null);");
                code.AppendLine("" + state._2ResultInterfaceName + " Create(IDataRecord record);");
            }
            code.AppendLine("int ExecuteNonQuery(" + state._8MethodSignature.Trim(spaceComma) + ");");
            code.AppendLine("int ExecuteNonQuery(" + state._8MethodSignature + "IDbConnection conn, IDbTransaction tx = null);");
            code.AppendLine("}"); // close interface;

            return code.ToString();
        }

        public string SelfTestUsings(State state)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine("using QueryFirst;");
            code.AppendLine("using Xunit;");
            return code.ToString();
        }

        public string MakeSelfTestMethod(State state)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();

            code.AppendLine("[Fact]");
            code.AppendLine("public void " + state._1BaseName + "SelfTest()");
            code.AppendLine("{");
            code.AppendLine("var queryText = getCommandText();");
            code.AppendLine("// we'll be getting a runtime version with the comments section closed. To run without parameters, open it.");
            code.AppendLine("queryText = queryText.Replace(\"/*designTime\", \"-- designTime\");");
            code.AppendLine("queryText = queryText.Replace(\"endDesignTime*/\", \"-- endDesignTime\");");
            // QfruntimeConnection will be used, but we still need to reference a provider, for the prepare parameters method.
            code.AppendLine($"var schema = new AdoSchemaFetcher().GetFields(QfRuntimeConnection.GetConnection(), \"{state._4Config.Provider}\", queryText);");
            code.Append("Assert.True(" + state._7ResultFields.Count + " <=  schema.Count,");
            code.AppendLine("\"Query only returns \" + schema.Count.ToString() + \" columns. Expected at least " + state._7ResultFields.Count + ". \");");
            for (int i = 0; i < state._7ResultFields.Count; i++)
            {
                var col = state._7ResultFields[i];
                code.Append("Assert.True(schema[" + i.ToString() + "].TypeDb == \"" + col.TypeDb + "\",");
                code.AppendLine("\"Result Column " + i.ToString() + " Type wrong. Expected " + col.TypeDb + ". Found \" + schema[" + i.ToString() + "].TypeDb + \".\");");
                code.Append("Assert.True(schema[" + i.ToString() + "].ColumnName == \"" + col.ColumnName + "\",");
                code.AppendLine("\"Result Column " + i.ToString() + " Name wrong. Expected " + col.ColumnName + ". Found \" + schema[" + i.ToString() + "].ColumnName + \".\");");
            }
            code.AppendLine("}");
            return code.ToString();
        }
    }
}
