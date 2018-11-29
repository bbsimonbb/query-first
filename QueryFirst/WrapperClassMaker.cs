using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    public class WrapperClassMaker : IWrapperClassMaker
    {
        public virtual string Usings(ICodeGenerationContext ctx)
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
        public virtual string StartNamespace(ICodeGenerationContext ctx)
        {
            if (!string.IsNullOrEmpty(ctx.Namespace))
                return "namespace " + ctx.Namespace + "{" + Environment.NewLine;
            else
                return "";
        }
        public virtual string StartClass(ICodeGenerationContext ctx)
        {
            return "public partial class " + ctx.BaseName + " : I" + ctx.BaseName + "{" + Environment.NewLine;

        }
        public virtual string MakeExecuteWithoutConn(ICodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            char[] spaceComma = new char[] { ',', ' ' };
            // Execute method, without connection
            code.AppendLine("public virtual List<" + ctx.ResultClassName + "> Execute(" + ctx.MethodSignature.Trim(spaceComma) + "){");
            code.AppendLine("using (IDbConnection conn = QfRuntimeConnection.GetConnection())");
            code.AppendLine("{");
            code.AppendLine("conn.Open();");
            code.AppendLine("return Execute(" + ctx.CallingArgs + " conn).ToList();");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }
        public virtual string MakeExecuteWithConn(ICodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            // Execute method with connection
            code.AppendLine("public virtual IEnumerable<" + ctx.ResultClassName + "> Execute(" + ctx.MethodSignature + "IDbConnection conn, IDbTransaction tx = null){");
            code.AppendLine("IDbCommand cmd = conn.CreateCommand();");
            code.AppendLine("if(tx != null)");
            code.AppendLine("cmd.Transaction = tx;");
            code.AppendLine("cmd.CommandText = getCommandText();");
            foreach (var qp in ctx.Query.QueryParams)
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
        public virtual string MakeGetOneWithoutConn(ICodeGenerationContext ctx)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            // GetOne without connection
            code.AppendLine("public virtual " + ctx.ResultClassName + " GetOne(" + ctx.MethodSignature.Trim(spaceComma) + "){");
            code.AppendLine("using (IDbConnection conn = QfRuntimeConnection.GetConnection())");
            code.AppendLine("{");
            code.AppendLine("conn.Open();");
            code.AppendLine("return GetOne(" + ctx.CallingArgs + " conn);");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();

        }
        public virtual string MakeGetOneWithConn(ICodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            // GetOne() with connection
            code.AppendLine("public virtual " + ctx.ResultClassName + " GetOne(" + ctx.MethodSignature + "IDbConnection conn, IDbTransaction tx = null)");
            code.AppendLine("{");
            code.AppendLine("var all = Execute(" + ctx.CallingArgs + " conn,tx);");
            code.AppendLine("using (IEnumerator<" + ctx.ResultClassName + "> iter = all.GetEnumerator())");
            code.AppendLine("{");
            code.AppendLine("iter.MoveNext();");
            code.AppendLine("return iter.Current;");
            code.AppendLine("}");
            code.AppendLine("}"); // close GetOne() method
            return code.ToString();

        }
        public virtual string MakeExecuteScalarWithoutConn(ICodeGenerationContext ctx)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            //ExecuteScalar without connection
            code.AppendLine("public virtual " + ctx.ExecuteScalarReturnType + " ExecuteScalar(" + ctx.MethodSignature.Trim(spaceComma) + "){");
            code.AppendLine("using (IDbConnection conn = QfRuntimeConnection.GetConnection())");
            code.AppendLine("{");
            code.AppendLine("conn.Open();");
            code.AppendLine("return ExecuteScalar(" + ctx.CallingArgs + " conn);");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }
        public virtual string MakeExecuteScalarWithConn(ICodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            // ExecuteScalar() with connection
            code.AppendLine("public virtual " + ctx.ExecuteScalarReturnType + " ExecuteScalar(" + ctx.MethodSignature + "IDbConnection conn, IDbTransaction tx = null){");
            code.AppendLine("IDbCommand cmd = conn.CreateCommand();");
            code.AppendLine("if(tx != null)");
            code.AppendLine("cmd.Transaction = tx;");
            code.AppendLine("cmd.CommandText = getCommandText();");
            foreach (var qp in ctx.Query.QueryParams)
            {
                code.AppendLine("AddAParameter(cmd, \"" + qp.DbType + "\", \"" + qp.DbName + "\", " + qp.CSName + ", " + qp.Length + ", " + qp.Scale + ", " + qp.Precision + ");");
            }
            code.AppendLine("var result = cmd.ExecuteScalar();");
            // only convert dbnull if nullable

            code.AppendLine("if( result == null || result == DBNull.Value)");
                code.AppendLine("return null;");
            code.AppendLine("else");
            code.AppendLine("return (" + ctx.ExecuteScalarReturnType + ")result;");
            code.AppendLine("}");
            // close ExecuteScalar()
            return code.ToString();

        }
        public virtual string MakeExecuteNonQueryWithoutConn(ICodeGenerationContext ctx)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            //ExecuteScalar without connection
            code.AppendLine("public virtual int ExecuteNonQuery(" + ctx.MethodSignature.Trim(spaceComma) + "){");
            code.AppendLine("using (IDbConnection conn = QfRuntimeConnection.GetConnection())");
            code.AppendLine("{");
            code.AppendLine("conn.Open();");
            code.AppendLine("return ExecuteNonQuery(" + ctx.CallingArgs + " conn);");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }
        public virtual string MakeExecuteNonQueryWithConn(ICodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            // ExecuteScalar() with connection
            code.AppendLine("public virtual int ExecuteNonQuery(" + ctx.MethodSignature + "IDbConnection conn, IDbTransaction tx = null){");
            code.AppendLine("IDbCommand cmd = conn.CreateCommand();");
            code.AppendLine("if(tx != null)");
            code.AppendLine("cmd.Transaction = tx;");
            code.AppendLine("cmd.CommandText = getCommandText();");
            foreach (var qp in ctx.Query.QueryParams)
            {
                code.AppendLine("AddAParameter(cmd, \"" + qp.DbType + "\", \"" + qp.DbName + "\", " + qp.CSName + ", " + qp.Length + ", " + qp.Scale + ", " + qp.Precision + ");");
            }
            code.AppendLine("return cmd.ExecuteNonQuery();");
            code.AppendLine("}");
            // close ExecuteScalar()
            return code.ToString();

        }

        public virtual string MakeCreateMethod(ICodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            // Create() method
            code.AppendLine("public virtual " + ctx.ResultClassName + " Create(IDataRecord record)");
            code.AppendLine("{");
            code.AppendLine("var returnVal = CreatePoco(record);");
            int j = 0;
            foreach (var col in ctx.ResultFields)
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
        public virtual string MakeGetCommandTextMethod(ICodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            // public load command text
            code.AppendLine("public string getCommandText(){");
            code.AppendLine("return @\"");
            code.Append(ctx.Query.FinalTextForCode);
            code.AppendLine("\";");
            code.AppendLine("}"); // close method;
            return code.ToString();

        }
        public virtual string MakeOtherMethods(ICodeGenerationContext ctx)
        {
            return "";
        }
        public virtual string CloseClass(ICodeGenerationContext ctx)
        {
            return "}" + Environment.NewLine;
        }
        public virtual string CloseNamespace(ICodeGenerationContext ctx)
        {
            if (!string.IsNullOrEmpty(ctx.Namespace))
                return "}" + Environment.NewLine;
            else
                return "";
        }

        public string MakeInterface(ICodeGenerationContext ctx)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            code.AppendLine("public interface I" + ctx.BaseName + "{" + Environment.NewLine);
            if (ctx.ResultFields != null && ctx.ResultFields.Count > 0)
            {
                code.AppendLine("List<" + ctx.ResultClassName + "> Execute(" + ctx.MethodSignature.Trim(spaceComma) + ");");
                code.AppendLine("IEnumerable<" + ctx.ResultClassName + "> Execute(" + ctx.MethodSignature + "IDbConnection conn, IDbTransaction tx = null);");
                code.AppendLine("" + ctx.ResultClassName + " GetOne(" + ctx.MethodSignature.Trim(spaceComma) + ");");
                code.AppendLine("" + ctx.ResultClassName + " GetOne(" + ctx.MethodSignature + "IDbConnection conn, IDbTransaction tx = null);");
                code.AppendLine("" + ctx.ExecuteScalarReturnType + " ExecuteScalar(" + ctx.MethodSignature.Trim(spaceComma) + ");");
                code.AppendLine("" + ctx.ExecuteScalarReturnType + " ExecuteScalar(" + ctx.MethodSignature + "IDbConnection conn, IDbTransaction tx = null);");
                code.AppendLine("" + ctx.ResultClassName + " Create(IDataRecord record);");
            }
            code.AppendLine("int ExecuteNonQuery(" + ctx.MethodSignature.Trim(spaceComma) + ");");
            code.AppendLine("int ExecuteNonQuery(" + ctx.MethodSignature + "IDbConnection conn, IDbTransaction tx = null);");
            code.AppendLine("}"); // close interface;

            return code.ToString();
        }

        public string SelfTestUsings(ICodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine("using QueryFirst;");
            code.AppendLine("using Xunit;");
            return code.ToString();
        }

        public string MakeSelfTestMethod(ICodeGenerationContext ctx)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();

            code.AppendLine("[Fact]");
            code.AppendLine("public void " + ctx.BaseName + "SelfTest()");
            code.AppendLine("{");
            code.AppendLine("var queryText = getCommandText();");
            code.AppendLine("// we'll be getting a runtime version with the comments section closed. To run without parameters, open it.");
            code.AppendLine("queryText = queryText.Replace(\"/*designTime\", \"-- designTime\");");
            code.AppendLine("queryText = queryText.Replace(\"endDesignTime*/\", \"-- endDesignTime\");");
            // QfruntimeConnection will be used, but we still need to reference a provider, for the prepare parameters method.
            code.AppendLine($"var schema = new AdoSchemaFetcher().GetFields(QfRuntimeConnection.GetConnection(), \"{ctx.Config.Provider}\", queryText);");
            code.Append("Assert.True(" + ctx.ResultFields.Count + " <=  schema.Count,");
            code.AppendLine("\"Query only returns \" + schema.Count.ToString() + \" columns. Expected at least " + ctx.ResultFields.Count + ". \");");
            for (int i = 0; i < ctx.ResultFields.Count; i++)
            {
                var col = ctx.ResultFields[i];
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
