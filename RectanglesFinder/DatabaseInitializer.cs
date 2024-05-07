using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public static class DatabaseInitializer
{
    public static void EnsureDatabaseCreated(IConfiguration configuration)
    {
        string databaseName = "RectanglesFinderDB";


        var connectionString = configuration["MasterDatabase"];

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Query to check if the database exists
            var checkDbQuery = "SELECT database_id FROM sys.databases WHERE Name = @DatabaseName";
            using (var checkDbCommand = new SqlCommand(checkDbQuery, connection))
            {
                checkDbCommand.Parameters.AddWithValue("@DatabaseName", databaseName);
                var result = checkDbCommand.ExecuteScalar();

                // If the database does not exist, create it
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