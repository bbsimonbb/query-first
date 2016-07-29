using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    class WrapperClassMaker : IWrapperClassMaker
    {
        public virtual string Usings(CodeGenerationContext ctx)
        {
            return @"using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;";

        }
        public virtual string StartNamespace(CodeGenerationContext ctx)
        {
            if (!string.IsNullOrEmpty(ctx.Namespace))
                return "namespace " + ctx.Namespace + "{" + Environment.NewLine;
            else
                return "";
        }
        public virtual string StartClass(CodeGenerationContext ctx)
        {
            return "public class " + ctx.BaseName + "{" + Environment.NewLine;

        }
        public virtual string MakeExecuteWithoutConn(CodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            char[] spaceComma = new char[] { ',', ' ' };
            // Execute method, without connection
            code.AppendLine("public virtual List<" + ctx.ResultClassName + "> Execute(" + ctx.MethodSignature.Trim(spaceComma) + "){");
            code.AppendLine("using (SqlConnection conn = new SqlConnection(QfRuntimeConnection.GetConnectionString()))");
            code.AppendLine("{");
            code.AppendLine("conn.Open();");
            code.AppendLine("return Execute(" + ctx.CallingArgs + ").ToList();");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }
        public virtual string MakeExecuteWithConn(CodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            // Execute method with connection
            code.AppendLine("public virtual IEnumerable<" + ctx.ResultClassName + "> Execute(" + ctx.MethodSignature + "SqlConnection conn){");
            code.AppendLine("SqlCommand cmd = conn.CreateCommand();");
            code.AppendLine("loadCommandText(cmd);");
            code.Append(MakeParamLoadingCode(ctx));
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
        public virtual string MakeGetOneWithoutConn(CodeGenerationContext ctx)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            // GetOne without connection
            code.AppendLine("public virtual " + ctx.ResultClassName + " GetOne(" + ctx.MethodSignature.Trim(spaceComma) + "){");
            code.AppendLine("using (SqlConnection conn = new SqlConnection(QfRuntimeConnection.GetConnectionString()))");
            code.AppendLine("{");
            code.AppendLine("conn.Open();");
            code.AppendLine("return GetOne(" + ctx.CallingArgs + ");");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();

        }
        public virtual string MakeGetOneWithConn(CodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            // GetOne() with connection
            code.AppendLine("public virtual " + ctx.ResultClassName + " GetOne(" + ctx.MethodSignature + "SqlConnection conn)");
            code.AppendLine("{");
            code.AppendLine("var all = Execute(" + ctx.CallingArgs + ");");
            code.AppendLine("using (IEnumerator<" + ctx.ResultClassName + "> iter = all.GetEnumerator())");
            code.AppendLine("{");
            code.AppendLine("iter.MoveNext();");
            code.AppendLine("return iter.Current;");
            code.AppendLine("}");
            code.AppendLine("}"); // close GetOne() method
            return code.ToString();

        }
        public virtual string MakeExecuteScalarWithoutConn(CodeGenerationContext ctx)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            //ExecuteScalar without connection
            code.AppendLine("public virtual " + ctx.QueryFields[0].DataType + " ExecuteScalar(" + ctx.MethodSignature.Trim(spaceComma) + "){");
            code.AppendLine("using (SqlConnection conn = new SqlConnection(QfRuntimeConnection.GetConnectionString()))");
            code.AppendLine("{");
            code.AppendLine("conn.Open();");
            code.AppendLine("return ExecuteScalar(" + ctx.CallingArgs + ");");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }
        public virtual string MakeExecuteScalarWithConn(CodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            // ExecuteScalar() with connection
            code.AppendLine("public virtual " + ctx.QueryFields[0].DataType + " ExecuteScalar(" + ctx.MethodSignature + "SqlConnection conn){");
            code.AppendLine("SqlCommand cmd = conn.CreateCommand();");
            code.AppendLine("loadCommandText(cmd);");
            code.Append(MakeParamLoadingCode(ctx));
            code.AppendLine("return (" + ctx.QueryFields[0].DataType + ")cmd.ExecuteScalar();");
            code.AppendLine("}");
            // close ExecuteScalar()
            return code.ToString();

        }
        public virtual string MakeExecuteNonQueryWithoutConn(CodeGenerationContext ctx)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            //ExecuteScalar without connection
            code.AppendLine("public virtual int ExecuteNonQuery(" + ctx.MethodSignature.Trim(spaceComma) + "){");
            code.AppendLine("using (SqlConnection conn = new SqlConnection(QfRuntimeConnection.GetConnectionString()))");
            code.AppendLine("{");
            code.AppendLine("conn.Open();");
            code.AppendLine("return ExecuteNonQuery(" + ctx.CallingArgs + ");");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }
        public virtual string MakeExecuteNonQueryWithConn(CodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            // ExecuteScalar() with connection
            code.AppendLine("public virtual int ExecuteNonQuery(" + ctx.MethodSignature + "SqlConnection conn){");
            code.AppendLine("SqlCommand cmd = conn.CreateCommand();");
            code.AppendLine("loadCommandText(cmd);");
            code.Append(MakeParamLoadingCode(ctx));
            code.AppendLine("return cmd.ExecuteNonQuery();");
            code.AppendLine("}");
            // close ExecuteScalar()
            return code.ToString();

        }
        protected virtual string MakeParamLoadingCode(CodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            foreach (var qp in ctx.Query.QueryParams)
            {
                code.AppendLine("cmd.Parameters.AddWithValue(\"@" + qp.Name + "\", " + qp.Name + ");");
            }
            return code.ToString();

        }
        public virtual string MakeCreateMethod(CodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            // Create() method
            code.AppendLine("public virtual " + ctx.ResultClassName + " Create(IDataRecord record)");
            code.AppendLine("{");
            code.AppendLine("var returnVal = new " + ctx.ResultClassName + "();");
            int j = 0;
            foreach (var col in ctx.QueryFields)
            {
                code.AppendLine("if(record[" + j + "] != null && record[" + j + "] != DBNull.Value)");
                code.AppendLine("returnVal." + col.ColumnName + " =  (" + col.CSType + ")record[" + j++ + "];");
            }
            // call OnLoad method in user's half of partial class
            code.AppendLine("returnVal.OnLoad();");
            code.AppendLine("return returnVal;");

            code.AppendLine("}"); // close method;

            return code.ToString();
        }
        public virtual string MakeLoadCommandTextMethod(CodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            // private load command text
            code.AppendLine("private void loadCommandText(SqlCommand cmd){");
            code.AppendLine("Stream strm = typeof(" + ctx.ResultClassName + ").Assembly.GetManifestResourceStream(\"" + ctx.NameAndPathForManifestStream + "\");");
            code.AppendLine("string queryText = new StreamReader(strm).ReadToEnd();");
            code.AppendLine("#if DEBUG");
            code.AppendLine("//Comments inverted at runtime in debug, pre-build in release");
            code.AppendLine("queryText = queryText.Replace(\"--designTime\", \"/*designTime\");");
            code.AppendLine("queryText = queryText.Replace(\"--endDesignTime\", \"endDesignTime*/\");");
            code.AppendLine("#endif");
            code.AppendLine("cmd.CommandText = queryText;");
            code.AppendLine("}"); // close method;
            return code.ToString();

        }
        public virtual string MakeOtherMethods(CodeGenerationContext ctx)
        {
            return "";
        }
        public virtual string CloseClass(CodeGenerationContext ctx)
        {
            return "}" + Environment.NewLine;
        }
        public virtual string CloseNamespace(CodeGenerationContext ctx)
        {
            if (!string.IsNullOrEmpty(ctx.Namespace))
                return "}"+ Environment.NewLine;
            else
                return "";
        }
    }
}
