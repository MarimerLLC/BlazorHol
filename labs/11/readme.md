# Accessing Databases from Blazor Server

## Creating the Solution

1. Open Visual Studio
2. Click on Create a new project
3. Select Blazor Web App
4. Click Next
5. Enter the project name: `BlazorHolDataAccess`
6. Click Next
7. Use the following options:
   - Framework: .NET 8.0
   - Authentication Type: None
   - Configure for HTTPS: Checked
   - Interactive render mode: Auto (Server and WebAssembly)
   - Interactivity location: Per page/component
   - Include sample pages: Checked
8. Click Create

## Setting Up Sqlite

Sqlite is perhaps the easiest way to get started with using a database in .NET. It is a file-based database that is easy to set up and use. It is not as powerful as SQL Server, but it is a good choice for small applications or for learning purposes.

1. Add the `Microsoft.Data.Sqlite` package to your server project.
2. Register a database connection service in the `Program.cs` file:

```csharp
builder.Services.AddTransient<SqliteConnection>(sp =>
{
    var connection = new SqliteConnection("Data Source=BlazorHolData.db");
    connection.Open();
    return connection;
});
```

3. Add a `Data` folder to your server project.
4. Add a `DataAccess` class to the `Data` folder.

```csharp
using Microsoft.Data.Sqlite;

namespace BlazorHolData.Data
{
    public class DataAccess(SqliteConnection Connection)
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
}
```

Initially this class contains the `InitializeDatabaseAsync` method that creates a `People` table in the database. This method will be called when the application starts.

5. Add a `DataAccess` service to the `Program.cs` file:

```csharp
builder.Services.AddScoped<DataAccess>();
```
