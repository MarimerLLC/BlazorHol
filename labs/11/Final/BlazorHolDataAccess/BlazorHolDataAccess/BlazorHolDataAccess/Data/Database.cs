using Microsoft.Data.Sqlite;

namespace BlazorHolDataAccess.Data;

public class Database(SqliteConnection Connection)
{
    public async Task InitializeDatabaseAsync()
    {
        using var command = Connection.CreateCommand();
        command.CommandText = @"
                CREATE TABLE IF NOT EXISTS People (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Age INTEGER NOT NULL
                );
            ";
        await command.ExecuteNonQueryAsync();
    }
}
