namespace QueryFirst.IntegrationTests.Queries{
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using QueryFirst;
using System.Text.RegularExpressions;

using static InsertQfRepo;


using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

public interface IInsertQfRepo{

List<InsertQfDto> Execute(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary);
IEnumerable< InsertQfDto> Execute(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbConnection conn, IDbTransaction tx = null);
System.Int32? ExecuteScalar(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary);
System.Int32? ExecuteScalar(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbConnection conn, IDbTransaction tx = null);

InsertQfDto Create(IDataRecord record);
InsertQfDto GetOne(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary);
InsertQfDto GetOne(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbConnection conn, IDbTransaction tx = null);
int ExecuteNonQuery(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary);
int ExecuteNonQuery(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbConnection conn, IDbTransaction tx = null);
}
public partial class InsertQfRepo : IInsertQfRepo

{

void AppendExececutionMessage(string msg) { ExecutionMessages += msg + Environment.NewLine; }
public string ExecutionMessages { get; protected set; }
// constructor with connection factory injection
protected QueryFirst.QueryFirstConnectionFactory _connectionFactory;
public  InsertQfRepo(QueryFirst.QueryFirstConnectionFactory connectionFactory)
{
    _connectionFactory = connectionFactory;
}
private static IInsertQfRepo _inst;
private static IInsertQfRepo inst { get
{
if (_inst == null)
_inst = new InsertQfRepo(QueryFirstConnectionFactory.Instance);
return _inst;
}
}

#region Sync

public static int ExecuteNonQueryStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary)
=> inst.ExecuteNonQuery(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary);
public virtual int ExecuteNonQuery(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary)
{
using (IDbConnection conn = _connectionFactory.CreateConnection())
{
conn.Open();
return ExecuteNonQuery(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary, conn);
}
}

public static int ExecuteNonQueryStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbConnection conn, IDbTransaction tx = null)
=> inst.ExecuteNonQuery(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary, conn, tx);
public virtual int ExecuteNonQuery(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbConnection conn, IDbTransaction tx = null)
{

// this line will not compile in .net core unless you install the Microsoft.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });
using(IDbCommand cmd = conn.CreateCommand())
{
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary);
AddParameters(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary,  cmd);
var result = cmd.ExecuteNonQuery();

// Assign output parameters to instance properties. 
/*

*/
// only convert dbnull if nullable
return result;
}
}


#endregion

public string getCommandText(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary){
var queryText = $@"/* .sql query managed by QueryFirst add-in */
/*designTime
declare @MyBigint bigint;
declare @MyBit bit;
declare @MyDecimal decimal(18,9);
declare @MyInt int;
declare @MyMoney money;
declare @MySmallint smallint;
declare @MyTinyint tinyint;
declare @MyFloat float;
declare @MyReal real;
declare @MyDate date;
declare @MyDatetime2 datetime2(7);
declare @MyDatetime datetime;
declare @MyChar char(255);
declare @MyVarchar varchar(50);
declare @MyNchar nchar(255);
declare @MyNvarchar nvarchar(50);
declare @MyBinary binary(50);
declare @MyVarbinary varbinary(50);

endDesignTime*/

INSERT INTO EveryDatatype (
MyBigint,
MyBit,
MyDecimal,
MyInt,
MyMoney,
MySmallint,
MyTinyint,
MyFloat,
MyReal,
MyDate,
MyDatetime2,
MyDatetime,
MyChar,
MyVarchar,
MyNchar,
MyNvarchar,
MyBinary,
MyVarbinary
)
VALUES (
@MyBigint,
@MyBit,
@MyDecimal,
@MyInt,
@MyMoney,
@MySmallint,
@MyTinyint,
@MyFloat,
@MyReal,
@MyDate,
@MyDatetime2,
@MyDatetime,
@MyChar,
@MyVarchar,
@MyNchar,
@MyNvarchar,
@MyBinary,
@MyVarbinary
)

SELECT CAST(SCOPE_IDENTITY() AS INT) AS JustInsertedId
";
// QfExpandoParams

return queryText;
}
#region Sync


public static List<InsertQfDto> ExecuteStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary)
=> inst.Execute(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary);

public virtual List<InsertQfDto> Execute(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary)
{
using (IDbConnection conn = _connectionFactory.CreateConnection())
{
conn.Open();
var returnVal = Execute(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary, conn).ToList();
return returnVal;
}
}

public static IEnumerable<InsertQfDto> ExecuteStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbConnection conn, IDbTransaction tx = null)
=> inst.Execute(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary, conn, tx);

public virtual IEnumerable<InsertQfDto> Execute(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbConnection conn, IDbTransaction tx = null)
{
// this line will not compile in .net core unless you install the Microsoft.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });
using(IDbCommand cmd = conn.CreateCommand())
{
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary);
AddParameters(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary,  cmd);
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


public static InsertQfDto GetOneStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary)
=> inst.GetOne(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary);
public virtual InsertQfDto GetOne(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary)
{
using (IDbConnection conn = _connectionFactory.CreateConnection())
{
conn.Open();
var returnVal = GetOne(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary, conn);
return returnVal;
}
}
public static InsertQfDto GetOneStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbConnection conn, IDbTransaction tx = null)
=> inst.GetOne(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary, conn, tx);
public virtual InsertQfDto GetOne(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbConnection conn, IDbTransaction tx = null)
{
// this line will not compile in .net core unless you install the Microsoft.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });
{
var all = Execute(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary,  conn,tx);
InsertQfDto returnVal;
using (IEnumerator<InsertQfDto> iter = all.GetEnumerator())
{
iter.MoveNext();
returnVal = iter.Current;
}
return returnVal;
}
}

public static System.Int32? ExecuteScalarStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary)
=> inst.ExecuteScalar(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary);
public virtual System.Int32? ExecuteScalar(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary)
{
using (IDbConnection conn = _connectionFactory.CreateConnection())
{
conn.Open();
var returnVal = ExecuteScalar(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary, conn);
/*
;
*/
return returnVal;
}
}


public static System.Int32? ExecuteScalarStatic(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbConnection conn, IDbTransaction tx = null)
=> inst.ExecuteScalar(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary, conn, tx);
public virtual System.Int32? ExecuteScalar(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbConnection conn, IDbTransaction tx = null)
{
// this line will not compile in .net core unless you install the Microsoft.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });
using(IDbCommand cmd = conn.CreateCommand())
{
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary);
AddParameters(myBigint, myBit, myDecimal, myInt, myMoney, mySmallint, myTinyint, myFloat, myReal, myDate, myDatetime2, myDatetime, myChar, myVarchar, myNchar, myNvarchar, myBinary, myVarbinary,  cmd);
var result = cmd.ExecuteScalar();

// only convert dbnull if nullable
// Assign output parameters to instance properties.
/*

*/
if( result == null || result == DBNull.Value)
return null;
else
return (System.Int32?)result;
}
}


#endregion

public virtual InsertQfDto Create(IDataRecord record)
{
var returnVal = CreatePoco(record);

    if(record[0] != null && record[0] != DBNull.Value)
    returnVal.JustInsertedId =  (int?)record[0];

// provide a hook to override
returnVal.OnLoad();
return returnVal;
}
protected virtual InsertQfDto CreatePoco(System.Data.IDataRecord record)
{
    return new InsertQfDto();
}
protected void AddParameters(long? myBigint, bool? myBit, decimal? myDecimal, int? myInt, decimal? myMoney, short? mySmallint, byte? myTinyint, double? myFloat, float? myReal, DateTime? myDate, DateTime? myDatetime2, DateTime? myDatetime, string myChar, string myVarchar, string myNchar, string myNvarchar, System.Byte[] myBinary, System.Byte[] myVarbinary, IDbCommand cmd)
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
myParam.ParameterName = "@MyDatetime2";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "DateTime2");
myParam.Value = (object)myDatetime2 ?? DBNull.Value;

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
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyBinary";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "Binary");
myParam.Value = (object)myBinary ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}
{
var myParam = cmd.CreateParameter();
myParam.Direction = ParameterDirection.Input;
myParam.ParameterName = "@MyVarbinary";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), "Binary");
myParam.Value = (object)myVarbinary ?? DBNull.Value;

cmd.Parameters.Add(myParam);
}}
}
public partial class InsertQfDto  {
protected int? _JustInsertedId; // int(4) null
public int? JustInsertedId{
get{return _JustInsertedId;}
set{_JustInsertedId = value;}
}
protected internal virtual void OnLoad(){}
}
}
