DECLARE @User AS varchar(50)
DECLARE @Password AS varchar(50)
DECLARE @UserSQL AS nvarchar(max)
DECLARE @GrantSQL AS nvarchar(max)

-- Set user and password here
SET @User = 'dfc-<env>-stax-editor-as-svc'
SET @Password = 'n0t-a-real-password'

-- Create user if it does not exist
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE NAME = @User) BEGIN
SET @UserSQL = 'CREATE USER [' + @User + '] WITH PASSWORD = ''' + @Password + ''''
EXECUTE(@UserSQL)END

-- Add user to db roles
EXEC sp_addrolemember 'db_datareader', @User
EXEC sp_addrolemember 'db_datawriter', @User
EXEC sp_addrolemember 'db_ddladmin', @User

-- Add Grants
SET @GrantSQL = 'GRANT EXECUTE TO [' + @User + ']'
EXEC(@GrantSQL)