namespace QueryFirst{
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;public interface IScaffoldUpdate{

List<ScaffoldUpdateResults> Execute(string table_name);
IEnumerable<ScaffoldUpdateResults> Execute(string table_name, SqlConnection conn);
ScaffoldUpdateResults GetOne(string table_name);
ScaffoldUpdateResults GetOne(string table_name, SqlConnection conn);
System.String ExecuteScalar(string table_name);
System.String ExecuteScalar(string table_name, SqlConnection conn);
ScaffoldUpdateResults Create(IDataRecord record);
int ExecuteNonQuery(string table_name);
int ExecuteNonQuery(string table_name, SqlConnection conn);
}
public class ScaffoldUpdate : IScaffoldUpdate{
public virtual int ExecuteNonQuery(string table_name){
using (SqlConnection conn = new SqlConnection(QfRuntimeConnection.GetConnectionString()))
{
conn.Open();
return ExecuteNonQuery(table_name, conn);
}
}
public virtual int ExecuteNonQuery(string table_name, SqlConnection conn){
SqlCommand cmd = conn.CreateCommand();
cmd.CommandText = getCommandText();
cmd.Parameters.Add("@table_name", SqlDbType.VarChar,776).Value = table_name != null ? (object)table_name :DBNull.Value;
return cmd.ExecuteNonQuery();
}
private string getCommandText(){
Stream strm = typeof(ScaffoldUpdateResults).Assembly.GetManifestResourceStream("QueryFirst.InsertsAndUpdates.ScaffoldUpdate.sql");
string queryText = new StreamReader(strm).ReadToEnd();
#if DEBUG
//Comments inverted at runtime in debug, pre-build in release
queryText = queryText.Replace("-- designTime", "/*designTime");
queryText = queryText.Replace("-- endDesignTime", "endDesignTime*/");
#endif
return queryText;
}
public virtual List<ScaffoldUpdateResults> Execute(string table_name){
using (SqlConnection conn = new SqlConnection(QfRuntimeConnection.GetConnectionString()))
{
conn.Open();
return Execute(table_name, conn).ToList();
}
}
public virtual IEnumerable<ScaffoldUpdateResults> Execute(string table_name, SqlConnection conn){
SqlCommand cmd = conn.CreateCommand();
cmd.CommandText = getCommandText();
cmd.Parameters.Add("@table_name", SqlDbType.VarChar,776).Value = table_name != null ? (object)table_name :DBNull.Value;
using (var reader = cmd.ExecuteReader())
{
while (reader.Read())
{
yield return Create(reader);
}
}
}
public virtual ScaffoldUpdateResults GetOne(string table_name){
using (SqlConnection conn = new SqlConnection(QfRuntimeConnection.GetConnectionString()))
{
conn.Open();
return GetOne(table_name, conn);
}
}
public virtual ScaffoldUpdateResults GetOne(string table_name, SqlConnection conn)
{
var all = Execute(table_name, conn);
using (IEnumerator<ScaffoldUpdateResults> iter = all.GetEnumerator())
{
iter.MoveNext();
return iter.Current;
}
}
public virtual System.String ExecuteScalar(string table_name){
using (SqlConnection conn = new SqlConnection(QfRuntimeConnection.GetConnectionString()))
{
conn.Open();
return ExecuteScalar(table_name, conn);
}
}
public virtual System.String ExecuteScalar(string table_name, SqlConnection conn){
SqlCommand cmd = conn.CreateCommand();
cmd.CommandText = getCommandText();
cmd.Parameters.Add("@table_name", SqlDbType.VarChar,776).Value = table_name != null ? (object)table_name :DBNull.Value;
return (System.String)cmd.ExecuteScalar();
}
public virtual ScaffoldUpdateResults Create(IDataRecord record)
{
var returnVal = new ScaffoldUpdateResults();
if(record[0] != null && record[0] != DBNull.Value)
returnVal.MyUpdateStatement =  (string)record[0];
returnVal.OnLoad();
return returnVal;
}
}
public partial class ScaffoldUpdateResults {
public string MyUpdateStatement { get; set; } //(varchar null)
}
}
