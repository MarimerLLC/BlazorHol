using Microsoft.Data.Sqlite;

namespace BlazorHolDataAccess.Data;

public class PersonDal(SqliteConnection connection) : IPersonDal
{
    public Task DeletePersonAsync(int id)
    {
        var sql = "DELETE FROM People WHERE Id = @Id";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        return command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<PersonEntity>> GetPeopleAsync()
    {
        var sql = "SELECT Id, FirstName, LastName, Age FROM People";
        using var command = new SqliteCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();
        var people = new List<PersonEntity>();
        while (await reader.ReadAsync())
        {
            people.Add(new PersonEntity
            {
                Id = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Age = reader.GetInt32(3)
            });
        }
        return people;
    }

    public async Task<PersonEntity?> GetPersonAsync(int id)
    {
        var sql = "SELECT Id, FirstName, LastName, Age FROM People WHERE Id = @Id";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new PersonEntity
            {
                Id = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Age = reader.GetInt32(3)
            };
        }
        return null;
    }

    public async Task<int> AddPersonAsync(PersonEntity person)
    {
        var sql = "INSERT INTO People (FirstName, LastName, Age) VALUES (@FirstName, @LastName, @Age)";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", person.FirstName);
        command.Parameters.AddWithValue("@LastName", person.LastName);
        command.Parameters.AddWithValue("@Age", person.Age);
        await command.ExecuteNonQueryAsync();
        using var idCommand = new SqliteCommand("SELECT last_insert_rowid()", connection);
        var newId = await idCommand.ExecuteScalarAsync() ?? 0;
        return Convert.ToInt32(newId);
    }

    public Task UpdatePersonAsync(PersonEntity person)
    {
        var sql = "UPDATE People SET FirstName = @FirstName, LastName = @LastName, Age = @Age WHERE Id = @Id";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", person.FirstName);
        command.Parameters.AddWithValue("@LastName", person.LastName);
        command.Parameters.AddWithValue("@Age", person.Age);
        command.Parameters.AddWithValue("@Id", person.Id);
        return command.ExecuteNonQueryAsync();
    }
}
