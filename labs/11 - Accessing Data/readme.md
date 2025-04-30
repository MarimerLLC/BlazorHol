# Accessing Databases from Blazor Server

## Opening the Solution

1. Open Visual Studio
2. Open the `labs/11/BlazorHolDataAccess.sln` solution

This is a Blazor Web App solution that you will finish in this lab.

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

3. There is already a `Data` folder to your server project.
4. Add a `Database` class to the `Data` folder.

```csharp
using Microsoft.Data.Sqlite;

namespace BlazorHolDataAccess.Data
{
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
}
```

This class contains the `InitializeDatabaseAsync` method that creates a `People` table in the database. This method will be called when the application starts.

5. Register the `Database` service in the `Program.cs` file:

```csharp
builder.Services.AddScoped<Database>();
```

6. Open the `Home.razor` file in the `Pages` folder of the server project.
7. Inject the `Database` service in the `Home` component:

```csharp
@inject Data.Database Database
```

8. Call the `InitializeDatabaseAsync` method in the `OnInitializedAsync` method:

```csharp
@code
{
    protected override async Task OnInitializedAsync()
    {
        await Database.InitializeDatabaseAsync();
    }
}
```

This will create the `People` table in the database when the application starts. If the table already exists the method will do nothing.

## The Data Model

The solution already contains a `PersonEntity` class in the `Data` folder. The class is in the _client_ project so the type is available to the server and client code.

This class represents a person in the database.

```csharp
using System.ComponentModel.DataAnnotations;

namespace BlazorHolDataAccess.Data;

public class PersonEntity
{
    public int Id { get; set; }
    [Required]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }
    [Required]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }
    public int Age { get; set; }
}
```

## The Data Access Service

The client `Data` folder contains an `IPersonDal` interface and a `PersonDal` class. The `PersonDal` class implements the `IPersonDal` interface.

The `IPersonDal` interface defines the methods that the `PersonDal` class must implement.

```csharp
namespace BlazorHolDataAccess.Data;

public interface IPersonDal
{
    Task<IEnumerable<PersonEntity>> GetPeopleAsync();
    Task<PersonEntity?> GetPersonAsync(int id);
    Task<int> AddPersonAsync(PersonEntity person);
    Task UpdatePersonAsync(PersonEntity person);
    Task DeletePersonAsync(int id);
}
```

This type is also in the client project so it is available to the server and client code. In the _next_ lab it will be important that this type be available in the client project.

1. Right now, add a `PersonDal` class to the `Data` folder in the server project.

```csharp
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
```

Notice how this class uses the `SqliteConnection` to interact with the database. The `PersonDal` class implements the `IPersonDal` interface. The methods are implemented using basic ADO.NET capabilities.

> ℹ️ You can use Entity Framework with Sqlite, and you can also use frameworks like Dapper. This lab uses ADO.NET to show you how to interact with a database without extra dependencies.

2. Register the `IPersonDal` service in the server project's `Program.cs` file:

```csharp
builder.Services.AddScoped<IPersonDal, PersonDal>();
```

This registers the `PersonDal` class as the implementation of the `IPersonDal` interface. The `PersonDal` class will be injected into the components that need it.

## The PersonList Component

1. In the server project, add a `PersonList.razor` file to the `Pages` folder.

```html
@page "/personlist"

@using Microsoft.AspNetCore.Components.QuickGrid

@inject Data.IPersonDal personDal
@inject NavigationManager NavigationManager

<h3>PersonList</h3>

@if (persons == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <p><a href="/personedit">Add person</a></p>

    <QuickGrid Items="@persons.AsQueryable()">
        <PropertyColumn Property="@(_ => _.Id)" />
        <PropertyColumn Property="@(_ => _.FirstName)" Sortable="true" />
        <PropertyColumn Property="@(_ => _.LastName)" Sortable="true" />
        <PropertyColumn Property="@(_ => _.Age)" Sortable="true" />
        <TemplateColumn>
            <a href="personedit/@context.Id">Edit</a>
        </TemplateColumn>
        <TemplateColumn>
            <a href="removeperson/@context.Id">Remove</a>
        </TemplateColumn>
    </QuickGrid>
}

@code {

    private List<Data.PersonEntity>? persons;

    protected override async Task OnInitializedAsync()
    {
        persons = (await personDal.GetPeopleAsync()).ToList();
    }
}
```

This component displays a list of people in the database. It uses the `QuickGrid` component to display the list. The `QuickGrid` component is part of the `Microsoft.AspNetCore.Components.QuickGrid` namespace.

The `PersonList` component injects the `IPersonDal` service and uses it to get the list of people from the database.

The `PersonList` component has a link to the `PersonEdit` component to add a new person. It also has links to edit and remove people.

Because this page does not have a `renderMode` attribute, it will be rendered using server-static rendering on the server.

2. Open the `NavMenu.razor` file in the `Layout` folder of the server project.
3. Add a link to the `PersonList` page:

```html
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="personlist">
                <span class="bi bi-plus-square-fill-nav-menu" aria-hidden="true"></span> List People
            </NavLink>
```

## The PersonEdit Component

1. In the client project, add a `PersonEdit.razor` file to the `Pages` folder.

```html
@page "/personedit"
@page "/personedit/{id:int}"
@rendermode InteractiveServer

@using BlazorHolDataAccess.Client.Shared
@using BlazorHolDataAccess.Data

@inject IPersonDal personDal
@inject NavigationManager NavigationManager

<h3>PersonEdit</h3>

<div class="alert-danger">@ErrorMessage</div>

@if (Person == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <EditForm Model="@Person" OnValidSubmit="@HandleValidSubmit">
        <DataAnnotationsValidator />
        <ValidationSummary />

        <TextEdit For="() => Person.FirstName" />
        <TextEdit For="() => Person.LastName" />
        <NumberEdit For="() => Person.Age" />
        <button type="submit" class="btn btn-primary">Save</button>
    </EditForm>
}


@code {
    [Parameter]
    public int Id { get; set; }

    private string ErrorMessage { get; set; } = null!;
    private PersonEntity? Person { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            if (Id > 0)
            {
                Person = await personDal.GetPersonAsync(Id);
            }
            else
            {
                Person = new PersonEntity();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private async Task HandleValidSubmit()
    {
        if (Person == null) return;
        try
        {
            if (Person.Id == 0)
            {
                await personDal.AddPersonAsync(Person);
            }
            else
            {
                await personDal.UpdatePersonAsync(Person);
            }
            NavigationManager.NavigateTo("/personlist");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.ToString();
        }
    }
}
```

This component allows you to edit a person. It uses the `EditForm` component to create a form for editing a person. The `EditForm` component is part of the Blazor framework.

The `PersonEdit` component injects the `IPersonDal` service and uses it to get a person from the database. If the `Id` parameter is 0, a new person is created. If the `Id` parameter is greater than 0, the person with that ID is retrieved from the database.

The `PersonEdit` component has a `HandleValidSubmit` method that is called when the form is submitted. If the person is new, the `AddPersonAsync` method is called. If the person already exists, the `UpdatePersonAsync` method is called.

The `PersonEdit` component has a `renderMode` attribute with the value `InteractiveServer`. This means that the component will be rendered using the server interactive render mode, so all the code runs on the server and the user has a highly interactive experience in the browser.

## The RemovePerson Component

1. In the client project, add a `RemovePerson.razor` file to the `Pages` folder.

```html
@page "/removeperson/{id:int}"
@rendermode InteractiveServer

@using BlazorHolDataAccess.Data

@inject IPersonDal personDal
@inject NavigationManager NavigationManager

<h3>Remove Person</h3>

@if (Person == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <div>
        <p>Are you sure you want to remove this person?</p>
        <p>First Name: @Person.FirstName</p>
        <p>Last Name: @Person.LastName</p>
        <p>Age: @Person.Age</p>
        <button class="btn btn-danger" @onclick="Remove">Remove</button>
        <button class="btn btn-primary" @onclick="GoHome">Cancel</button>
    </div>
}

@code {
    private PersonEntity? Person { get; set; }

    [Parameter]
    public int Id { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Id > 0)
        {
            Person = await personDal.GetPersonAsync(Id);
        }
        else
        {
            GoHome();
        }
    }

    private async Task Remove()
    {
        if (Person == null) return;
        await personDal.DeletePersonAsync(Person.Id);
        GoHome();
    }

    private void GoHome()
    {
        NavigationManager.NavigateTo("/personlist");
    }
}
```

This component allows you to remove a person. It uses the `IPersonDal` service to get the person from the database and to remove the person.

The `RemovePerson` component has a `renderMode` attribute with the value `InteractiveServer`. This means that the component will be rendered using the server interactive render mode, so all the code runs on the server and the user has a highly interactive experience in the browser.

## Running the Application

1. Run the application and select the `List People` link in the navigation menu.
2. Add a person by selecting the `Add person` link.
3. Edit a person by selecting the `Edit` link next to a person.
4. Remove a person by selecting the `Remove` link next to a person.
