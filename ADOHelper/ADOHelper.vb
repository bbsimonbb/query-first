Imports System.Data
Imports System.Data.Sql
Imports System.Data.SqlClient
Imports System.Text.RegularExpressions


Public Class QueryField
    Public Property ColumnName As String
    Public Property ColumnOrdinal As Integer
    Public Property ColumnSize As Integer
    Public Property NumericPrecision As Integer
    Public Property NumericScale As Integer
    Public Property IsUnique As Boolean
    Public Property BaseColumnName As String
    Public Property BaseTableName As String
    Public Property DataType As String
    Public Property AllowDBNull As Boolean
    Public Property ProviderType As String
    Public Property IsIdentity As Boolean
    Public Property IsAutoIncrement As Boolean
    Public Property IsRowVersion As Boolean
    Public Property IsLong As Boolean
    Public Property IsReadOnly As Boolean
    Public Property ProviderSpecificDataType As String
    Public Property DataTypeName As String
    Public Property UdtAssemblyQualifiedName As String
    Public Property NewVersionedProviderType As Integer
    Public Property IsColumnSet As String
    Public Property RawProperties As String
    Public Property NonVersionedProviderType As String
End Class

Public Class ADOHelper

    Public Function GetFields(ConnectionString As String, Query As String, ByRef SchemaTable As DataTable) As List(Of QueryField)
        Dim dt As New DataTable

        If (Regex.IsMatch(Query, "^\s*select", RegexOptions.IgnoreCase Or RegexOptions.Multiline)) Then
            SchemaTable = GetQuerySchema(ConnectionString, Query)
        ElseIf (Regex.IsMatch(Query, "^\s*exec", RegexOptions.IgnoreCase Or RegexOptions.Multiline)) Then
            SchemaTable = GetSPSchema(Query, ConnectionString)
        Else
            Err.Raise(-1000, , "Unable to determine query or procedure. A line must start with SELECT or EXEC")
        End If
        Dim result As New List(Of QueryField)

        For i As Integer = 0 To SchemaTable.Rows.Count - 1
            Dim qf = New QueryField
            Dim properties As String = String.Empty
            For j = 0 To SchemaTable.Columns.Count - 1
                properties += SchemaTable.Columns(j).ColumnName & Chr(254) & SchemaTable.Rows(i).Item(j).ToString
                If j < SchemaTable.Columns.Count - 1 Then properties += Chr(255)

                If (IsDBNull(SchemaTable.Rows(i).Item(j)) = False) Then
                    Select Case SchemaTable.Columns(j).ColumnName
                        Case "ColumnName"
                            qf.ColumnName = SchemaTable.Rows(i).Item(j)
                        Case "ColumnOrdinal"
                            qf.ColumnOrdinal = SchemaTable.Rows(i).Item(j)
                        Case "ColumnSize"
                            qf.ColumnSize = SchemaTable.Rows(i).Item(j)
                        Case "NumericPrecision"
                            qf.NumericPrecision = SchemaTable.Rows(i).Item(j)
                        Case "NumericScale"
                            qf.NumericScale = SchemaTable.Rows(i).Item(j)
                        Case "IsUnique"
                            qf.IsUnique = SchemaTable.Rows(i).Item(j)
                        Case "BaseColumnName"
                            qf.BaseColumnName = SchemaTable.Rows(i).Item(j)
                        Case "BaseTableName"
                            qf.BaseTableName = SchemaTable.Rows(i).Item(j)
                        Case "DataType"
                            qf.DataType = CType(SchemaTable.Rows(i).Item(j), System.Type).FullName
                        Case "AllowDBNull"
                            qf.AllowDBNull = SchemaTable.Rows(i).Item(j)
                        Case "ProviderType"
                            qf.ProviderType = SchemaTable.Rows(i).Item(j)
                        Case "IsIdentity"
                            qf.IsIdentity = SchemaTable.Rows(i).Item(j)
                        Case "IsAutoIncrement"
                            qf.IsAutoIncrement = SchemaTable.Rows(i).Item(j)
                        Case "IsRowVersion"
                            qf.IsRowVersion = SchemaTable.Rows(i).Item(j)
                        Case "IsLong"
                            qf.IsLong = SchemaTable.Rows(i).Item(j)
                        Case "IsReadOnly"
                            qf.IsReadOnly = SchemaTable.Rows(i).Item(j)
                        Case "ProviderSpecificDataType"
                            qf.ProviderSpecificDataType = CType(SchemaTable.Rows(i).Item(j), System.Type).FullName
                        Case "DataTypeName"
                            qf.DataTypeName = SchemaTable.Rows(i).Item(j)
                        Case "UdtAssemblyQualifiedName"
                            qf.UdtAssemblyQualifiedName = SchemaTable.Rows(i).Item(j)
                        Case "IsColumnSet"
                            qf.IsColumnSet = SchemaTable.Rows(i).Item(j)
                        Case "NonVersionedProviderType"
                            qf.NonVersionedProviderType = SchemaTable.Rows(i).Item(j)
                        Case Else
                    End Select
                End If
            Next
            qf.RawProperties = properties
            result.Add(qf)
        Next

        Return result
    End Function

    Sub New()
    End Sub

    'Perform the query, extract the results
    Private Function GetQuerySchema(ByVal strconn As String, ByVal strSQL As String) As DataTable
        'Returns a DataTable filled with the results of the query
        'Function returns the count of records in the datatable
        '----- dt (datatable) needs to be empty & no schema defined

        Dim sconQuery As New SqlConnection
        Dim scmdQuery As New SqlCommand
        Dim srdrQuery As SqlDataReader = Nothing
        Dim intRowsCount As Integer = 0
        Dim dtSchema As New Data.DataTable

        Try

            'Open the SQL connnection to the SWO database
            sconQuery.ConnectionString = strconn
            sconQuery.Open()

            'Execute the SQL command against the database & return a resultset
            scmdQuery.Connection = sconQuery
            scmdQuery.CommandText = strSQL
            srdrQuery = scmdQuery.ExecuteReader(Data.CommandBehavior.SchemaOnly)

            dtSchema = srdrQuery.GetSchemaTable
        Catch ex As Exception
            Err.Raise(-1000, , "Error = '" & ex.Message & " ': sql = " & strSQL)
        Finally
            If Not IsNothing(srdrQuery) Then
                If Not srdrQuery.IsClosed Then srdrQuery.Close()
            End If
            scmdQuery.Dispose()
            sconQuery.Close()
            sconQuery.Dispose()
        End Try

        Return dtSchema
    End Function

    Public Function GenerateCodeVB(ByRef Columns As List(Of QueryField), ObjectName As String, Optional LinePrefix As String = "    ") As String()
        Dim result(Columns.Count + 2) As String
        result(0) = String.Format("Public Class {0}", ObjectName)
        For i As Integer = 0 To Columns.Count - 1
            Try
                If String.IsNullOrEmpty(Columns(i).ColumnName) Then
                    Columns(i).ColumnName = "Field" & i.ToString("000")
                End If
                If Char.IsNumber(Columns(i).ColumnName.Substring(0, 1)) Then
                    Columns(i).ColumnName = "_" & Columns(i).ColumnName
                End If

                Dim AllowNull As String = ", null"
                If Columns(i).AllowDBNull = False Then AllowNull = ", not null"

                Select Case Columns(i).DataTypeName

                    Case "bigint"
                        result(i + 1) = String.Format("{0}Public Property {1} as Long '(bigint{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "binary"
                        result(i + 1) = String.Format("{0}Public Property {1}() as Byte '(binary({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, AllowNull)

                    Case "bit"
                        result(i + 1) = String.Format("{0}Public Property {1} as Boolean '(bit{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "char"
                        result(i + 1) = String.Format("{0}Public Property {1} as String '(char({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, AllowNull)

                    Case "date"
                        result(i + 1) = String.Format("{0}Public Property {1} as DateTime '(date{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "datetime"
                        result(i + 1) = String.Format("{0}Public Property {1} as DateTime '(datetime{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "datetime2"
                        result(i + 1) = String.Format("{0}Public Property {1} as DateTime '(datetime2({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).NumericScale, AllowNull)

                    Case "datetimeoffset"
                        result(i + 1) = String.Format("{0}Public Property {1} as DateTimeOffset '(datetimeoffset{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "decimal"
                        result(i + 1) = String.Format("{0}Public Property {1} as Decimal '(decimal({2},{3}){4})", LinePrefix, Columns(i).ColumnName, Columns(i).NumericPrecision, Columns(i).NumericScale, AllowNull)

                    Case "float"
                        result(i + 1) = String.Format("{0}Public Property {1} as Double '(float{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "image"
                        result(i + 1) = String.Format("{0}Public Property {1}() as Byte '(image{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "int"
                        result(i + 1) = String.Format("{0}Public Property {1} as Integer '(int{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "money"
                        result(i + 1) = String.Format("{0}Public Property {1} as Decimal '(money{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "nchar"
                        If Columns(i).IsLong Then
                            result(i + 1) = String.Format("{0}Public Property {1} as String '(nchar(max){2})", LinePrefix, Columns(i).ColumnName, AllowNull)
                        Else
                            result(i + 1) = String.Format("{0}Public Property {1} as String '(nchar({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, AllowNull)
                        End If

                    Case "ntext"
                        result(i + 1) = String.Format("{0}Public Property {1} as String '(ntext{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "nvarchar"
                        If Columns(i).IsLong Then
                            result(i + 1) = String.Format("{0}Public Property {1} as String '(nvarchar(max){2})", LinePrefix, Columns(i).ColumnName, AllowNull)
                        Else
                            result(i + 1) = String.Format("{0}Public Property {1} as String '(nvarchar({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, AllowNull)
                        End If

                    Case "real"
                        result(i + 1) = String.Format("{0}Public Property {1} as Single '(real{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "smalldatetime"
                        result(i + 1) = String.Format("{0}Public Property {1} as DateTime '(smalldatetime{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "sql_variant"
                        result(i + 1) = String.Format("{0}Public Property {1} as Object '(sql_variant{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "text"
                        result(i + 1) = String.Format("{0}Public Property {1} as String '(text{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "time"
                        result(i + 1) = String.Format("{0}Public Property {1} as DateTime '(time({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).NumericScale, AllowNull)

                    Case "timestamp"
                        result(i + 1) = String.Format("{0}Public Property {1}() as Byte '(timestamp{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "tinyint"
                        result(i + 1) = String.Format("{0}Public Property {1}() as Byte '(tinyint{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "uniqueidentifier"
                        result(i + 1) = String.Format("{0}Public Property {1} as Guid '(uniqueidentifier{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "varbinary"
                        If Columns(i).IsLong Then
                            result(i + 1) = String.Format("{0}Public Property {1}() as Byte '(varbinary(max){2})", LinePrefix, Columns(i).ColumnName, AllowNull)
                        Else
                            result(i + 1) = String.Format("{0}Public Property {1}() as Byte '(varbinary({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, AllowNull)
                        End If

                    Case "varchar"
                        If Columns(i).IsLong Then
                            result(i + 1) = String.Format("{0}Public Property {1} as String '(varchar(max){2})", LinePrefix, Columns(i).ColumnName, AllowNull)
                        Else
                            result(i + 1) = String.Format("{0}Public Property {1} as String '(varchar({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, AllowNull)
                        End If

                    Case "xml"
                        result(i + 1) = String.Format("{0}Public Property {1} as String '(XML(.){2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case Else 'sql variant
                        Select Case Columns(i).DataType

                            Case "Microsoft.SqlServer.Types.SqlGeography" 'geography
                                result(i + 1) = String.Format("{0}Public Property {1} as Microsoft.SqlServer.Types.SqlGeography '({2}{3})", LinePrefix, Columns(i).ColumnName, Columns(i).DataTypeName, AllowNull)

                            Case "Microsoft.SqlServer.Types.SqlHierarchyId" 'heirarchyid
                                result(i + 1) = String.Format("{0}Public Property {1} as Microsoft.SqlServer.Types.SqlGeography '({2}{3})", LinePrefix, Columns(i).ColumnName, Columns(i).DataTypeName, AllowNull)

                            Case "Microsoft.SqlServer.Types.SqlGeometry" 'geometry
                                result(i + 1) = String.Format("{0}Public Property {1} as Microsoft.SqlServer.Types.SqlGeography '({2}{3})", LinePrefix, Columns(i).ColumnName, Columns(i).DataTypeName, AllowNull)

                        End Select
                End Select
            Catch ex As Exception
                Throw New Exception(ex.Message)
            End Try
        Next
        result(Columns.Count + 1) = "End Class"
        Return result
    End Function

    Public Function GenerateCodeCS(ByRef Columns As List(Of QueryField), ObjectName As String, Optional LinePrefix As String = "    ") As String()
        Dim result(Columns.Count + 2) As String
        result(0) = String.Format("public partial class {0} {{", ObjectName)
        For i As Integer = 0 To Columns.Count - 1
            Try

                If Char.IsNumber(Columns(i).ColumnName.Substring(0, 1)) Then
                    Columns(i).ColumnName = "_" & Columns(i).ColumnName
                End If

                Dim AllowNull As String = ", null"
                If Columns(i).AllowDBNull = False Then AllowNull = ", not null"

                Select Case Columns(i).DataTypeName

                    Case "bigint"
                        result(i + 1) = String.Format("{0}public long {1} {{ get; set; }} //(bigint{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "binary"
                        result(i + 1) = String.Format("{0}public byte[] {1} {{ get; set; }} //(binary({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, AllowNull)

                    Case "bit"
                        result(i + 1) = String.Format("{0}public bool {1} {{ get; set; }} //(bit{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "char"
                        result(i + 1) = String.Format("{0}public string {1} {{ get; set; }} //(char({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, AllowNull)

                    Case "date"
                        result(i + 1) = String.Format("{0}public DateTime {1} {{ get; set; }} //(date{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "datetime"
                        result(i + 1) = String.Format("{0}public DateTime {1} {{ get; set; }} //(datetime{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "datetime2"
                        result(i + 1) = String.Format("{0}public DateTime {1} {{ get; set; }} //(datetime2({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).NumericScale, AllowNull)

                    Case "datetimeoffset"
                        result(i + 1) = String.Format("{0}public DateTimeOffset {1} {{ get; set; }} //(datetimeoffset{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "decimal"
                        result(i + 1) = String.Format("{0}public decimal {1} {{ get; set; }} //(decimal({2},{3}){4})", LinePrefix, Columns(i).ColumnName, Columns(i).NumericPrecision, Columns(i).NumericScale, AllowNull)

                    Case "float"
                        result(i + 1) = String.Format("{0}public double {1} {{ get; set; }} //(float{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "image"
                        result(i + 1) = String.Format("{0}public byte[] {1} {{ get; set; }} //(image{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "int"
                        result(i + 1) = String.Format("{0}public int {1} {{ get; set; }} //(int{2})", LinePrefix, Columns(i).ColumnName, AllowNull)
                        'SBY
                    Case "smallint"
                        result(i + 1) = String.Format("{0}public int {1} {{ get; set; }} //(int{2})", LinePrefix, Columns(i).ColumnName, AllowNull)
                    Case "money"
                        result(i + 1) = String.Format("{0}public decimal {1} {{ get; set; }} //(money{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "nchar"
                        If Columns(i).IsLong Then
                            result(i + 1) = String.Format("{0}public string {1} {{ get; set; }} //(nchar(max){2})", LinePrefix, Columns(i).ColumnName, AllowNull)
                        Else
                            result(i + 1) = String.Format("{0}public string {1} {{ get; set; }} //(nchar({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, AllowNull)
                        End If

                    Case "ntext"
                        result(i + 1) = String.Format("{0}public string {1} {{ get; set; }} //(ntext{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "nvarchar"
                        If Columns(i).IsLong Then
                            result(i + 1) = String.Format("{0}public string {1} {{ get; set; }} //(nvarchar(max){2})", LinePrefix, Columns(i).ColumnName, AllowNull)
                        Else
                            result(i + 1) = String.Format("{0}public string {1} {{ get; set; }} //(nvarchar({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, AllowNull)
                        End If

                    Case "real"
                        result(i + 1) = String.Format("{0}public Single {1} {{ get; set; }} //(real({2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "smalldatetime"
                        result(i + 1) = String.Format("{0}public DateTime {1} {{ get; set; }} //(smalldatetime{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "sql_variant"
                        result(i + 1) = String.Format("{0}public object {1} {{ get; set; }} //(sql_variant{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "text"
                        result(i + 1) = String.Format("{0}public string {1} {{ get; set; }} //(text{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "time"
                        result(i + 1) = String.Format("{0}public DateTime {1} {{ get; set; }} //(time({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).NumericScale, AllowNull)

                    Case "timestamp"
                        result(i + 1) = String.Format("{0}public byte[] {1} {{ get; set; }} //(timestamp{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "tinyint"
                        result(i + 1) = String.Format("{0}public byte {1} {{ get; set; }} //(tinyint{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "uniqueidentifier"
                        result(i + 1) = String.Format("{0}public Guid {1} {{ get; set; }} //(uniqueidentifier{2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case "varbinary"
                        If Columns(i).IsLong Then
                            result(i + 1) = String.Format("{0}public byte[] {1} {{ get; set; }} //(varbinary(max){2})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, AllowNull)
                        Else
                            result(i + 1) = String.Format("{0}public byte[] {1} {{ get; set; }} //(varbinary({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, Columns(i).ColumnSize, AllowNull)
                        End If

                    Case "varchar"
                        If Columns(i).IsLong Then
                            result(i + 1) = String.Format("{0}public string {1} {{ get; set; }} //(varchar(max){2})", LinePrefix, Columns(i).ColumnName, AllowNull)
                        Else
                            result(i + 1) = String.Format("{0}public string {1} {{ get; set; }} //(varchar({2}){3})", LinePrefix, Columns(i).ColumnName, Columns(i).ColumnSize, AllowNull)
                        End If

                    Case "xml"
                        result(i + 1) = String.Format("{0}public string {1} {{ get; set; }} //(XML(.){2})", LinePrefix, Columns(i).ColumnName, AllowNull)

                    Case Else 'sql variant
                        Select Case Columns(i).DataType

                            Case "Microsoft.SqlServer.Types.SqlGeography" 'geography
                                result(i + 1) = String.Format("{0}public Microsoft.SqlServer.Types.SqlGeography {1} {{ get; set; }} //({2}{3})", LinePrefix, Columns(i).ColumnName, Columns(i).DataTypeName, AllowNull)

                            Case "Microsoft.SqlServer.Types.SqlHierarchyId" 'heirarchyid
                                result(i + 1) = String.Format("{0}public Microsoft.SqlServer.Types.SqlGeography {1} {{ get; set; }} //({2}{3})", LinePrefix, Columns(i).ColumnName, Columns(i).DataTypeName, AllowNull)

                            Case "Microsoft.SqlServer.Types.SqlGeometry" 'geometry
                                result(i + 1) = String.Format("{0}public Microsoft.SqlServer.Types.SqlGeography {1} {{ get; set; }} //({2}{3})", LinePrefix, Columns(i).ColumnName, Columns(i).DataTypeName, AllowNull)

                        End Select
                End Select
            Catch ex As Exception
                Throw New Exception(ex.Message)
            End Try
        Next
        result(Columns.Count + 1) = "}"
        Return result
    End Function

    Public Function StringArrayToText(lines() As String, Optional LineDelimiter As String = vbCrLf) As String
        'Determine number of characters needed in result string
        Dim charCount = 0
        For Each s In lines
            If Not IsNothing(s) Then
                charCount += s.Length + LineDelimiter.Length
            End If
        Next

        'Preallocate needed string space plus a little extra
        Dim sb = New System.Text.StringBuilder(charCount + lines.Count)
        For i As Integer = 0 To lines.Count - 1
            If Not IsNothing(lines(i)) Then
                sb.Append(lines(i) & LineDelimiter)
            End If
        Next
        Return sb.ToString
    End Function

    'Run a stored procedure with optional parameters and return as datatable of results
    Public Function GetSPSchema(ByVal spName As String, strConn As String) As DataTable

        'Dim the dataset to hold the result table
        Dim dtSchema As New DataTable

        'Return if missing important information (SP Name)
        If spName.Trim.Length = 0 Then Return dtSchema

        'Default the connection string to the public class variable if not specified
        If strConn.Length = 0 Then Return dtSchema

        'Create the connection to the database
        Dim sconSP As New SqlClient.SqlConnection
        Dim scmdSP As New SqlClient.SqlCommand
        Dim srdrSP As SqlDataReader = Nothing

        Try
            'Set the connection string on the connection object
            sconSP.ConnectionString = strConn
            sconSP.Open()

            'Set up the SqlCommand object
            scmdSP.CommandText = spName
            scmdSP.Connection = sconSP
            scmdSP.CommandType = CommandType.StoredProcedure

            If spName.Contains("|") Then
                Dim spParms = spName.Split("|")
                scmdSP.CommandText = spParms(0).Trim
                For i = 1 To spParms.Count - 1

                    Dim spFields = spParms(i).Replace("`,", vbTab).Replace("`", "").Split(vbTab)
                    scmdSP.Parameters.Add(New SqlClient.SqlParameter(spFields(0).Trim, spFields(1).Trim))
                Next
            End If

            srdrSP = scmdSP.ExecuteReader(CommandBehavior.SchemaOnly)
            dtSchema = srdrSP.GetSchemaTable
        Catch ex As Exception
            Err.Raise(-1000, , "Error = '" & ex.Message & " ': stored procedure = " & spName)
        Finally
            If Not IsNothing(srdrSP) Then
                If Not srdrSP.IsClosed Then srdrSP.Close()
            End If
            scmdSP.Dispose()
            sconSP.Close()
            sconSP.Dispose()
        End Try

        Return dtSchema
    End Function


End Class

'USE [Chinook]
'GO
'SET ANSI_NULLS ON
'GO
'SET QUOTED_IDENTIFIER ON
'GO
'CREATE PROCEDURE [dbo].[spSelectCustomer]
'AS
'BEGIN
'	SET NOCOUNT ON;
'	SELECT * FROM CUSTOMER
'END
'GO


