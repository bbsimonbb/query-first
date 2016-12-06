0.6 MySql and Postgres BREAKING CHANGES

QfRuntimeConnection now returns a connection, not a connection string. Copy and customize this class...

class QfRuntimeConnection
{
    public static IDbConnection GetConnection()
    {
        return new MySqlConnection(ConfigurationManager.ConnectionStrings["QfDefaultConnection"].ConnectionString);
    }
}

If you use SelfTest, you additionally need to implement the interface IQfDefaultConnection. You can do that like this...

class QfRuntimeConnection : IQfDefaultConnection
{
    public static IDbConnection GetConnection()
    {
        return new SqlConnection(ConfigurationManager.ConnectionStrings["QfRuntimeConnection"].ConnectionString) ;
    }
    IDbConnection IQfDefaultConnection.GetConnection()
    {
        return QfRuntimeConnection.GetConnection();
    }
}

MySql requires a space after two hyphens for comments. For existing QueryFirst queries, do a global search and replace on 
"--designTime" --> "-- designTime"
"--endDesignTime" "-- endDesignTime"