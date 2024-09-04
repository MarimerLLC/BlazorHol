# Creating an App to Edit Data

## Create a solution

1. Open Visual Studio
2. Click on Create a new project
3. Select Blazor Web App
4. Click Next
5. Enter the project name: `BlazorHolData`
6. Click Next
7. Use the following options:
   - Framework: .NET 8.0
   - Authentication Type: None
   - Configure for HTTPS: Checked
   - Interactive render mode: Server
   - Interactivity location: Global
   - Include sample pages: Checked
8. Click Create

## Adding some mock data

1. Add a `Data` folder to the `BlazorHolData` project
2. Add a new class file to the `Data` folder named `Database.cs`:

```csharp
namespace BlazorHolData.Data;

public static class Database
{
    public static List<Person> People = new List<Person>
    {
        new Person { Id = 1, FirstName = "John", LastName = "Doe", Age = 30 },
        new Person { Id = 2, FirstName = "Jane", LastName = "Smith", Age = 25 },
        new Person { Id = 3, FirstName = "Bob", LastName = "Johnson", Age = 40 }
    };
}

public class Person
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int Age { get; set; }
}
```

This is your mock data.

## Displaying the Data

1. Open the `Home.razor` file.
2. Add a table to list the people:

```html
@page "/"

<PageTitle>Home</PageTitle>

<h1>Hello, world!</h1>

Welcome to your new app.

<table>
    <thead>
        <tr>
            <th>Id</th>
            <th>First name</th>
            <th>Last name</th>
            <th>Age</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var person in Data.Database.People)
        {
            <tr>
                <td style="min-width:50px">@person.Id</td>
                <td>@person.FirstName</td>
                <td>@person.LastName</td>
                <td>@person.Age</td>
                <td><a href="EditPerson/id=@person.Id">Edit</a></td>
            </tr>
        }
    </tbody>
</table>
```

