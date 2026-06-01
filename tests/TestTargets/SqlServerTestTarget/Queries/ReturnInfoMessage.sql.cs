namespace SqlServerTestTarget.Queries{
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using QueryFirst;
using System.Text.RegularExpressions;

using static ReturnInfoMessageQfRepo;


using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

public interface IReturnInfoMessageQfRepo{

int ExecuteNonQuery();
int ExecuteNonQuery(IDbConnection conn, IDbTransaction tx = null);
}
public partial class ReturnInfoMessageQfRepo : IReturnInfoMessageQfRepo

{

void AppendExececutionMessage(string msg) { ExecutionMessages += msg + Environment.NewLine; }
public string ExecutionMessages { get; protected set; }
// constructor with connection factory injection
protected QueryFirst.QueryFirstConnectionFactory _connectionFactory;
public  ReturnInfoMessageQfRepo(QueryFirst.QueryFirstConnectionFactory connectionFactory)
{
    _connectionFactory = connectionFactory;
}
private static IReturnInfoMessageQfRepo _inst;
private static IReturnInfoMessageQfRepo inst { get
{
if (_inst == null)
_inst = new ReturnInfoMessageQfRepo(QueryFirstConnectionFactory.Instance);
return _inst;
}
}

#region Sync

public static int ExecuteNonQueryStatic()
=> inst.ExecuteNonQuery();
public virtual int ExecuteNonQuery()
{
using (IDbConnection conn = _connectionFactory.CreateConnection())
{
conn.Open();
return ExecuteNonQuery(conn);
}
}

public static int ExecuteNonQueryStatic(IDbConnection conn, IDbTransaction tx = null)
=> inst.ExecuteNonQuery(conn, tx);
public virtual int ExecuteNonQuery(IDbConnection conn, IDbTransaction tx = null)
{

// this line will not compile in .net core unless you install the Microsoft.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });// hello from MyGroovyProvider
using(IDbCommand cmd = conn.CreateCommand())
{
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText();
AddParameters( cmd);
var result = cmd.ExecuteNonQuery();

// Assign output parameters to instance properties. 
/*

*/
// only convert dbnull if nullable
return result;
}
}


#endregion

public string getCommandText(){
var queryText = $@"-- use queryfirst
/*designTime

endDesignTime*/

PRINT 'info message for cobber'
";
// QfExpandoParams

return queryText;
}
protected void AddParameters(IDbCommand cmd)
{
}
}
}
