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

using static LoadsaParametersQfRepo;


using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

public interface ILoadsaParametersQfRepo{

List<LoadsaParametersQfDto> Execute(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar);
IEnumerable< LoadsaParametersQfDto> Execute(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbConnection conn, IDbTransaction tx = null);
System.String ExecuteScalar(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar);
System.String ExecuteScalar(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbConnection conn, IDbTransaction tx = null);

LoadsaParametersQfDto Create(IDataRecord record);
LoadsaParametersQfDto GetOne(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar);
LoadsaParametersQfDto GetOne(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbConnection conn, IDbTransaction tx = null);
int ExecuteNonQuery(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar);
int ExecuteNonQuery(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbConnection conn, IDbTransaction tx = null);
}
public partial class LoadsaParametersQfRepo : ILoadsaParametersQfRepo

{

void AppendExececutionMessage(string msg) { ExecutionMessages += msg + Environment.NewLine; }
public string ExecutionMessages { get; protected set; }
// constructor with connection factory injection
protected QueryFirst.QueryFirstConnectionFactory _connectionFactory;
public  LoadsaParametersQfRepo(QueryFirst.QueryFirstConnectionFactory connectionFactory)
{
    _connectionFactory = connectionFactory;
}
private static ILoadsaParametersQfRepo _inst;
private static ILoadsaParametersQfRepo inst { get
{
if (_inst == null)
_inst = new LoadsaParametersQfRepo(QueryFirstConnectionFactory.Instance);
return _inst;
}
}

#region Sync

public static int ExecuteNonQueryStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar)
=> inst.ExecuteNonQuery(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar);
public virtual int ExecuteNonQuery(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar)
{
using (IDbConnection conn = _connectionFactory.CreateConnection())
{
conn.Open();
return ExecuteNonQuery(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar, conn);
}
}

public static int ExecuteNonQueryStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbConnection conn, IDbTransaction tx = null)
=> inst.ExecuteNonQuery(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar, conn, tx);
public virtual int ExecuteNonQuery(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbConnection conn, IDbTransaction tx = null)
{

// this line will not compile in .net core unless you install the Microsoft.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });// hello from MyGroovyProvider
using(IDbCommand cmd = conn.CreateCommand())
{
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar);
AddParameters(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar,  cmd);
var result = cmd.ExecuteNonQuery();

// Assign output parameters to instance properties. 
/*

*/
// only convert dbnull if nullable
return result;
}
}


#endregion

public string getCommandText(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar){
var queryText = $@"/* .sql query managed by QueryFirst add-in */
/*designTime - put parameter declarations and design time initialization here
declare @MyBigint bigint;
declare @MyBit bit;
declare @MyDecimal decimal(18,0);
declare @MyInt int;
declare @MyMoney money;
declare @MySmallint smallint;
declare @MyTinyint tinyint;
declare @MyFloat float;
declare @MyReal real;
declare @MyDate date;
declare @MyDatetime datetime;
declare @MyDatetime2 datetime2(7);
declare @MyChar char(255);
declare @MyVarchar varchar(50);
declare @MyNchar nchar(255);
declare @MyNvarchar nvarchar(50);
endDesignTime*/
SELECT E.MyNchar FROM EveryDatatype E
WHERE 1=1
and MyBigint = @MyBigint
and MyBit = @MyBit
and MyDecimal = @MyDecimal
and MyInt = @MyInt
and MyMoney = @MyMoney
and MySmallint = @MySmallint
and MyTinyint = @MyTinyint
and MyFloat = @MyFloat
and MyReal = @MyReal
and MyDate <= @MyDate
and MyDatetime <= @MyDatetime
and MyDatetime2 <= @MyDatetime2
and MyChar = @MyChar
and MyVarchar = @MyVarchar
and MyNchar = @MyNchar
and MyNvarchar = @MyNvarchar
";
// QfExpandoParams

return queryText;
}
#region Sync


public static List<LoadsaParametersQfDto> ExecuteStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar)
=> inst.Execute(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar);

public virtual List<LoadsaParametersQfDto> Execute(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar)
{
using (IDbConnection conn = _connectionFactory.CreateConnection())
{
conn.Open();
var returnVal = Execute(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar, conn).ToList();
return returnVal;
}
}

public static IEnumerable<LoadsaParametersQfDto> ExecuteStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbConnection conn, IDbTransaction tx = null)
=> inst.Execute(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar, conn, tx);

public virtual IEnumerable<LoadsaParametersQfDto> Execute(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbConnection conn, IDbTransaction tx = null)
{
// this line will not compile in .net core unless you install the Microsoft.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });// hello from MyGroovyProvider
using(IDbCommand cmd = conn.CreateCommand())
{
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar);
AddParameters(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar,  cmd);
using (var reader = cmd.ExecuteReader())
{
while (reader.Read())
{
yield return Create(reader);
}
}

// Assign output parameters to instance properties. These will be available after this method returns.
// todo : make output parameters work in a threadsafe way. An output object with execution messages and output parameters?
/*

*/
}
}


public static LoadsaParametersQfDto GetOneStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar)
=> inst.GetOne(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar);
public virtual LoadsaParametersQfDto GetOne(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar)
{
using (IDbConnection conn = _connectionFactory.CreateConnection())
{
conn.Open();
var returnVal = GetOne(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar, conn);
return returnVal;
}
}
public static LoadsaParametersQfDto GetOneStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbConnection conn, IDbTransaction tx = null)
=> inst.GetOne(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar, conn, tx);
public virtual LoadsaParametersQfDto GetOne(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbConnection conn, IDbTransaction tx = null)
{
// this line will not compile in .net core unless you install the Microsoft.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });// hello from MyGroovyProvider
{
var all = Execute(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar,  conn,tx);
LoadsaParametersQfDto returnVal;
using (IEnumerator<LoadsaParametersQfDto> iter = all.GetEnumerator())
{
iter.MoveNext();
returnVal = iter.Current;
}
return returnVal;
}
}

public static System.String ExecuteScalarStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar)
=> inst.ExecuteScalar(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar);
public virtual System.String ExecuteScalar(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar)
{
using (IDbConnection conn = _connectionFactory.CreateConnection())
{
conn.Open();
var returnVal = ExecuteScalar(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar, conn);
/*
;
*/
return returnVal;
}
}


public static System.String ExecuteScalarStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbConnection conn, IDbTransaction tx = null)
=> inst.ExecuteScalar(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar, conn, tx);
public virtual System.String ExecuteScalar(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbConnection conn, IDbTransaction tx = null)
{
// this line will not compile in .net core unless you install the Microsoft.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });// hello from MyGroovyProvider
using(IDbCommand cmd = conn.CreateCommand())
{
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar);
AddParameters(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime, myDatetime2, myChar, myVarchar, myNchar, myNvarchar,  cmd);
var result = cmd.ExecuteScalar();

// only convert dbnull if nullable
// Assign output parameters to instance properties.
/*

*/
if( result == null || result == DBNull.Value)
return null;
else
return (System.String)result;
}
}


#endregion

public virtual LoadsaParametersQfDto Create(IDataRecord record)
{
var returnVal = CreatePoco(record);

    if(record[0] != null && record[0] != DBNull.Value)
    returnVal.MyNchar =  (string)record[0];

// provide a hook to override
returnVal.OnLoad();
return returnVal;
}
protected virtual LoadsaParametersQfDto CreatePoco(System.Data.IDataRecord record)
{
    return new LoadsaParametersQfDto();
}
protected void AddParameters(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime, DateTime? myDatetime2, string myChar, string myVarchar, string myNchar, string myNvarchar, IDbCommand cmd)
{

{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyBigint";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "Int64");
myParam.Value = (object)myBigint ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyBit";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "Boolean");
myParam.Value = (object)myBit ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyDecimal";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "Decimal");
myParam.Value = (object)myDecimal ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyInt";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "Int32");
myParam.Value = (object)myInt ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyMoney";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "Decimal");
myParam.Value = (object)myMoney ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MySmallint";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "Int16");
myParam.Value = (object)mySmallint ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyTinyint";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "Byte");
myParam.Value = (object)myTinyint ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyFloat";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "Double");
myParam.Value = (object)myFloat ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyReal";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "Single");
myParam.Value = (object)myReal ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyDate";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "Date");
myParam.Value = (object)myDate ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyDatetime";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "DateTime");
myParam.Value = (object)myDatetime ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyDatetime2";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "DateTime2");
myParam.Value = (object)myDatetime2 ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyChar";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "AnsiStringFixedLength");
myParam.Value = (object)myChar ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyVarchar";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "AnsiString");
myParam.Value = (object)myVarchar ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyNchar";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "StringFixedLength");
myParam.Value = (object)myNchar ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyNvarchar";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "String");
myParam.Value = (object)myNvarchar ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}}
}
public partial class LoadsaParametersQfDto  {
protected string _MyNchar; // nchar(50) null
public string MyNchar{
get{return _MyNchar;}
set{_MyNchar = value;}
}
protected internal virtual void OnLoad(){}
}
}
