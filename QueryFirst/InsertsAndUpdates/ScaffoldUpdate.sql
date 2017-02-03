/* .sql query managed by QueryFirst add-in */
--QfDefaultConnection=Data Source=not-mobility;Initial Catalog=NORTHWND;Integrated Security=SSPI;
SET NOCOUNT ON;
DECLARE @nl varchar(5) = CHAR(13) + CHAR(10);
DECLARE @CloseComments varchar(50) = @nl + char(45) + '- endDesignTime' + @nl ;
/*designTime - put parameter declarations and design time initialization here


DECLARE @table_name varchar(776) = 'Orders';  		-- The table/view for which the INSERT statements will be generated using the existing data
endDesignTime*/
DECLARE @include_timestamp bit = 0; 		-- Specify 1 for this parameter, if you want to include the TIMESTAMP/ROWVERSION column's data in the INSERT statement
DECLARE @debug_mode bit = 0;			-- If @debug_mode is set to 1, the SQL statements constructed by this procedure will be printed for later examination
DECLARE @owner varchar(64) = NULL;		-- Use this parameter if you are not the owner of the table
DECLARE @ommit_images bit = 1;			-- Use this parameter to generate UPDATE statements by omitting the 'image' columns
DECLARE @ommit_computed_cols bit = 1;		-- When 1, computed columns will not be included in the INSERT statement
DECLARE @idColumnName varchar(128);

/***********************************************************************************************************
Procedure:	sp_generate_inserts  (Build 22) 
		(Copyright 2002 Narayana Vyas Kondreddi. All rights reserved.)
                                          
Purpose:	To generate INSERT statements from existing data. 
		These INSERTS can be executed to regenerate the data at some other location.
		This procedure is also useful to create a database setup, where in you can 
		script your data along with your table definitions.

Written by:	Narayana Vyas Kondreddi
	        http://vyaskn.tripod.com

Acknowledgements:
		Divya Kalra	-- For beta testing
		Mark Charsley	-- For reporting a problem with scripting uniqueidentifier columns with NULL values
		Artur Zeygman	-- For helping me simplify a bit of code for handling non-dbo owned tables
		Joris Laperre   -- For reporting a regression bug in handling text/ntext columns

Tested on: 	SQL Server 7.0 and SQL Server 2000

Date created:	January 17th 2001 21:52 GMT

Date modified:	May 1st 2002 19:50 GMT

Email: 		vyaskn@hotmail.com

NOTE:		This procedure may not work with tables with too many columns.
		Results can be unpredictable with huge text columns or SQL Server 2000's sql_variant data types
		Whenever possible, Use @include_column_list parameter to ommit column list in the INSERT statement, for better results
		IMPORTANT: This procedure is not tested with internation data (Extended characters or Unicode). If needed
		you might want to convert the datatypes of character variables in this procedure to their respective unicode counterparts
		like nchar and nvarchar
		

Example 1:	To generate INSERT statements for table 'titles':
		
		EXEC sp_generate_inserts 'titles'

Example 2: 	To ommit the column list in the INSERT statement: (Column list is included by default)
		IMPORTANT: If you have too many columns, you are advised to ommit column list, as shown below,
		to avoid erroneous results
		
		EXEC sp_generate_inserts 'titles', @include_column_list = 0

Example 3:	To generate INSERT statements for 'titlesCopy' table from 'titles' table:

		EXEC sp_generate_inserts 'titles', 'titlesCopy'

Example 4:	To generate INSERT statements for 'titles' table for only those titles 
		which contain the word 'Computer' in them:
		NOTE: Do not complicate the FROM or WHERE clause here. It's assumed that you are good with T-SQL if you are using this parameter

		EXEC sp_generate_inserts 'titles', @from = "from titles where title like '%Computer%'"

Example 5: 	To specify that you want to include TIMESTAMP column's data as well in the INSERT statement:
		(By default TIMESTAMP column's data is not scripted)

		EXEC sp_generate_inserts 'titles', @include_timestamp = 1

Example 6:	To print the debug information:
  
		EXEC sp_generate_inserts 'titles', @debug_mode = 1

Example 7: 	If you are not the owner of the table, use @owner parameter to specify the owner name
		To use this option, you must have SELECT permissions on that table

		EXEC sp_generate_inserts Nickstable, @owner = 'Nick'

Example 8: 	To generate INSERT statements for the rest of the columns excluding images
		When using this otion, DO NOT set @include_column_list parameter to 0.

		EXEC sp_generate_inserts imgtable, @ommit_images = 1

Example 11: 	To generate INSERT statements with only those columns you want:
		
		EXEC sp_generate_inserts titles, @cols_to_include = "'title','title_id','au_id'"


Example 14: 	To exclude computed columns from the INSERT statement:
		EXEC sp_generate_inserts MyTable, @ommit_computed_cols = 1
***********************************************************************************************************/

SET NOCOUNT ON

--Checking to see if the database name is specified along wih the table name
--Your database context should be local to the table for which you want to generate INSERT statements
--specifying the database name is not allowed
IF (PARSENAME(@table_name,3)) IS NOT NULL
	BEGIN
		RAISERROR('Do not specify the database name. Be in the required database and just specify the table name.',16,1)
		--RETURN -1 --Failure. Reason: Database name is specified along with the table name, which is not allowed
	END

--Checking for the existence of 'user table' or 'view'
--This procedure is not written to work on system tables
--To script the data in system tables, just create a view on the system tables and script the view instead

IF @owner IS NULL
	BEGIN
		IF ((OBJECT_ID(@table_name,'U') IS NULL) AND (OBJECT_ID(@table_name,'V') IS NULL)) 
			BEGIN
				RAISERROR('User table or view not found.',16,1)
				PRINT 'You may see this error, if you are not the owner of this table or view. In that case use @owner parameter to specify the owner name.'
				PRINT 'Make sure you have SELECT permission on that table or view.'
				--RETURN -1 --Failure. Reason: There is no user table or view with this name
			END
	END
ELSE
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @table_name AND (TABLE_TYPE = 'BASE TABLE' OR TABLE_TYPE = 'VIEW') AND TABLE_SCHEMA = @owner)
			BEGIN
				RAISERROR('User table or view not found.',16,1)
				PRINT 'You may see this error, if you are not the owner of this table. In that case use @owner parameter to specify the owner name.'
				PRINT 'Make sure you have SELECT permission on that table or view.'
				--RETURN -1 --Failure. Reason: There is no user table or view with this name		
			END
	END

--Variable declarations
DECLARE		@Column_ID int, 		
		@Column_List varchar(8000), 
		@Column_Name varchar(128), 
		@Start_Insert varchar(786), 
		@Data_Type varchar(128), 
		@Actual_Values varchar(8000),	--This is the string that will be finally executed to generate INSERT statements
		@Declarations varchar(8000) = ''  --SBY

--Variable Initialization
SET @Column_ID = 0
SET @Column_Name = ''
SET @Column_List = ''
SET @Actual_Values = ''

--Start Update
IF @owner IS NULL 
	BEGIN
		SET @Start_Insert = 'UPDATE ' + '[' + RTRIM(@table_name) + '] SET ' 
	END
ELSE
	BEGIN
		SET @Start_Insert = 'UPDATE ' + '[' + LTRIM(RTRIM(@owner)) + '].' + '[' + RTRIM(@table_name) + '] SET ' 		
	END

--To get the first column's ID

SELECT	@Column_ID = MIN(ORDINAL_POSITION) 	
FROM	INFORMATION_SCHEMA.COLUMNS (NOLOCK) 
WHERE 	TABLE_NAME = @table_name AND
(@owner IS NULL OR TABLE_SCHEMA = @owner)



--Loop through all the columns of the table, to get the column names and their data types
WHILE @Column_ID IS NOT NULL
	BEGIN
		SELECT 	@Column_Name = COLUMN_NAME, 
		@Data_Type = DATA_TYPE 
		FROM 	INFORMATION_SCHEMA.COLUMNS (NOLOCK) 
		WHERE 	ORDINAL_POSITION = @Column_ID AND 
		TABLE_NAME = @table_name AND
		(@owner IS NULL OR TABLE_SCHEMA = @owner)


		--Making sure to output SET IDENTITY_INSERT ON/OFF in case the table has an IDENTITY column
		IF (SELECT COLUMNPROPERTY( OBJECT_ID(QUOTENAME(COALESCE(@owner,USER_NAME())) + '.' + @table_name),@Column_Name,'IsIdentity')) = 1 
		BEGIN
			SET @idColumnName = @Column_Name --SBY
			SET @Declarations = @Declarations + 'DECLARE @' + @Column_Name + ' ' + @Data_Type + ';' + CHAR(13)+CHAR(10)
			GOTO SKIP_LOOP			
		END
		
		--Making sure whether to output computed columns or not
		IF @ommit_computed_cols = 1
		BEGIN
			IF (SELECT COLUMNPROPERTY( OBJECT_ID(QUOTENAME(COALESCE(@owner,USER_NAME())) + '.' + @table_name),@Column_Name,'IsComputed')) = 1 
			BEGIN
				GOTO SKIP_LOOP					
			END
		END
		
		--Tables with columns of IMAGE data type are not supported for obvious reasons
		IF(@Data_Type in ('image','text', 'ntext'))
			BEGIN
				IF (@ommit_images = 0)
					BEGIN
						RAISERROR('Tables with image columns are not supported.',16,1)
						PRINT 'Use @ommit_images = 1 parameter to generate INSERTs for the rest of the columns.'
						PRINT 'DO NOT ommit Column List in the INSERT statements. If you ommit column list using @include_column_list=0, the generated INSERTs will fail.'
						--RETURN -1 --Failure. Reason: There is a column with image data type
					END
				ELSE
					BEGIN
					GOTO SKIP_LOOP
					END
			END

		--SBY here is where we generate the values part
		-- THIS LINE CAUSING ME A BIG PROBLEM IN SCHEMA RETRIEVAL MODE??? Fixed by losing square brackets, so no need to remove them
		SET @Actual_Values = @Actual_Values + char(9)  + @Column_Name + ' = @' + @Column_Name + ',' + @nl--SUBSTRING(@Column_Name,2,LEN(@Column_Name)-2) + ','
		-- Declarations
		SET @Declarations = @Declarations + 'DECLARE @' + @Column_Name + ' ' + @Data_Type + ';' + CHAR(13)+CHAR(10)
		--Generating the column list for the INSERT statement
		SET @Column_List = @Column_List + char(9) +  @Column_Name + ',' + @nl

		SKIP_LOOP: --The label used in GOTO

		SELECT 	@Column_ID = MIN(ORDINAL_POSITION) 
		FROM 	INFORMATION_SCHEMA.COLUMNS (NOLOCK) 
		WHERE 	TABLE_NAME = @table_name AND 
		ORDINAL_POSITION > @Column_ID AND
		(@owner IS NULL OR TABLE_SCHEMA = @owner)


	--Loop ends here!
	END

--To get rid of the extra characters that got concatenated during the last run through the loop
SET @Actual_Values = LEFT(@Actual_Values,len(@Actual_Values) - 3)
SET @Declarations = LEFT(@Declarations,len(@Declarations) - 2)

IF LTRIM(@Actual_Values) = '' 
	BEGIN
		RAISERROR('No columns to select. There should at least be one column to generate the output',16,1)
	END

--Forming the final string that will be executed, to output the INSERT statements

	BEGIN
		SET @Actual_Values = '/* .sql query managed by QueryFirst add-in */' + @nl + @nl + char(45) +
'-designTime - put parameter declarations and design time initialization here' + @nl + 
@Declarations +
@CloseComments +
RTRIM(@Start_Insert) + @nl +
@Actual_Values 
IF @idColumnName IS NOT NULL
SET @Actual_Values = @Actual_Values + @nl + 'WHERE ' + @idColumnName + ' = @' + @idColumnName 
	END


--Determining whether to ouput any debug information
IF @debug_mode =1
	BEGIN
		PRINT '/*****START OF DEBUG INFORMATION*****'
		PRINT 'Beginning of the INSERT statement:'
		PRINT @Start_Insert
		PRINT ''
		PRINT 'The column list:'
		PRINT @Column_List
		PRINT ''
		PRINT 'The SELECT statement executed to generate the INSERTs'
		PRINT @Actual_Values
		PRINT ''
		PRINT '*****END OF DEBUG INFORMATION*****/'
		PRINT ''
	END
		

SELECT (@Actual_Values) as MyUpdateStatement

PRINT 'PRINT ''Done'''
PRINT ''
