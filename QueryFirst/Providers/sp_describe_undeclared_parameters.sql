/* .sql query managed by QueryFirst add-in */
-- not detecting schema returned by procedure ??

/*designTime - put parameter declarations and design time initialization here

declare @tsql varchar = 'select * from orders';
endDesignTime*/
exec sp_describe_undeclared_parameters @tsql

--QfDefaultConnection=Data Source=not-mobility;Initial Catalog=NORTHWND;Integrated Security=SSPI;
