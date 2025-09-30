using System.Data;
using Npgsql;

namespace Laba.API.Infrastruction;

public class NpsqlConnectionFactory(string connectionString)
{
    public IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        return connection;
    }
}