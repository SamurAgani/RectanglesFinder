using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public static class DatabaseInitializer
{
    public static void EnsureDatabaseCreated(IConfiguration configuration)
    {
        var connectionString = configuration["MasterDatabase"];

        var databaseName = "RectangleDB";
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            var checkDbQuery = $"SELECT database_id FROM sys.databases WHERE Name = '{databaseName}'";
            using (var checkDbCommand = new SqlCommand(checkDbQuery, connection))
            {
                var result = checkDbCommand.ExecuteScalar();

                // If database does not exist, create it
                if (result == null)
                {
                    var createDbQuery = $"CREATE DATABASE [{databaseName}]";
                    using (var createDbCommand = new SqlCommand(createDbQuery, connection))
                    {
                        createDbCommand.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
