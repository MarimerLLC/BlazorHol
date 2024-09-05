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
using System.ComponentModel.DataAnnotations;

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
    [Required]
    public required string FirstName { get; set; }
    [Required]
    public required string LastName { get; set; }
    public int Age { get; set; }
}
```

This is your mock data, including the use of data annotations to validate the data.

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

## Using QuickGrid

1. Add the QuickGrid NuGet package to the project:

```bash
dotnet add package Microsoft.AspNetCore.Components.QuickGrid
```

2. Open the `Imports.razor` file and add the following line:

```csharp
@using Microsoft.AspNetCore.Components.QuickGrid
```

3. Replace the table in the `Home.razor` file with the following code:

```html
<div class="grid">
    <QuickGrid Items="@Data.Database.People.AsQueryable()">
        <PropertyColumn Property="@(_ => _.Id)" />
        <PropertyColumn Property="@(_ => _.FirstName)" Sortable="true" />
        <PropertyColumn Property="@(_ => _.LastName)" Sortable="true" />
        <PropertyColumn Property="@(_ => _.Age)" Sortable="true" />
        <TemplateColumn>
            <a href="EditPerson/@context.Id">Edit</a>
        </TemplateColumn>
    </QuickGrid>
</div>
```

4. Press `Ctrl+F5` to run the app.

## Adding an Edit Page

1. Add a new razor component to the `Pages` folder named `EditPerson.razor`:

```html
@page "/editperson/{id:int}"

@using BlazorHolData.Data

@inject NavigationManager NavigationManager

<h3>EditPerson</h3>

@if (Person == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <EditForm Model="@Person" OnValidSubmit="@HandleValidSubmit">
        <DataAnnotationsValidator />
        <ValidationSummary />

        <div class="form-group">
            <label for="FirstName">First Name</label>
            <InputText id="FirstName" class="form-control" @bind-Value="Person.FirstName" />
            <ValidationMessage For="@(() => Person.FirstName)" />
        </div>
        <div class="form-group">
            <label for="LastName">Last Name</label>
            <InputText id="LastName" class="form-control" @bind-Value="Person.LastName" />
            <ValidationMessage For="@(() => Person.LastName)" />
        </div>
        <div class="form-group">
            <label for="Age">Age</label>
            <InputNumber id="Age" class="form-control" @bind-Value="Person.Age" />
            <ValidationMessage For="@(() => Person.Age)" />
        </div>
        <button type="submit" class="btn btn-primary">Save</button>
    </EditForm>
}

@code {
    [Parameter]
    public int Id { get; set; }

    private Person? Person { get; set; }

    protected override void OnParametersSet()
    {
        if (Id > 0)
        {
            Person = Data.Database.People.Where(p => p.Id == Id).Select(_ => new Person
                { Id = _.Id, FirstName = _.FirstName, LastName = _.LastName, Age = _.Age }).First();
        }
    }

    private void HandleValidSubmit()
    {
        if (Id > 0)
        {
            var person = Data.Database.People.Where(p => p.Id == Id).FirstOrDefault();
            if (person != null && Person != null)
            {
                person.FirstName = Person.FirstName;
                person.LastName = Person.LastName;
                person.Age = Person.Age;
            }
        }
        NavigationManager.NavigateTo("/");
    }
}
```

Notice that the component operates on a _copy_ of the `Person` data, not the original data. This forces the user to click the Save button to commit the changes.

## Adding a New Person

1. Open the `Home.razor` file.
2. Add a link to the `EditPerson` page:

```html
<div>
    <a href="EditPerson">Add new person</a>
</div>
```

3. Open the `EditPerson.razor` file.
4. Add a new route to the `EditPerson` page:

```html
@page "/editperson"
@page "/editperson/{id:int}"
```

Now this page can be accessed by navigating to `/editperson` or `/editperson/0`.

5. Create a new `Person` object in the `EditPerson` component:

```csharp
    protected override void OnParametersSet()
    {
        if (Id > 0)
        {
            Person = Data.Database.People.Where(p => p.Id == Id).Select(_ => new Person
                { Id = _.Id, FirstName = _.FirstName, LastName = _.LastName, Age = _.Age }).First();
        }
        else
        {
            Person = new Person { Id = -1,  FirstName = string.Empty, LastName = string.Empty };
        }
    }
```

6. Update `HandleValidSubmit` to handle the creation of a new person:

```csharp
    private void HandleValidSubmit()
    {
        if (Person == null) return;
        if (Person.Id > 0)
        {
            var person = Data.Database.People.Where(p => p.Id == Id).FirstOrDefault();
            if (person != null)
            {
                person.FirstName = Person.FirstName;
                person.LastName = Person.LastName;
                person.Age = Person.Age;
            }
        }
        else
        {
            var newId = Data.Database.People.Max(p => p.Id) + 1;
            Person.Id = newId;
            Data.Database.People.Add(Person);
        }
        NavigationManager.NavigateTo("/");
    }
```

7. Press `Ctrl+F5` to run the app.
8. Add and edit some people.

## Deleting a Person

1. Open the `Home.razor` file.
2. Add a delete button to the QuickGrid:

```html
        <TemplateColumn>
            <a href="RemovePerson/@context.Id">Remove</a>
        </TemplateColumn>
```

3. Add a new `RemovePerson` page to the `Pages` folder:

```html
@page "/removeperson/{id:int}"

@using BlazorHolData.Data

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
    private Person? Person { get; set; }

    [Parameter]
    public int Id { get; set; }

    protected override void OnParametersSet()
    {
        if (Id > 0)
        {
            Person = Data.Database.People.Where(p => p.Id == Id).FirstOrDefault();
        }
        else
        {
            GoHome();
        }
    }

    private void Remove()
    {
        if (Person == null) return;
        Data.Database.People.Remove(Person);
        GoHome();
    }

    private void GoHome()
    {
        NavigationManager.NavigateTo("/");
    }
}
```

4. Press `Ctrl+F5` to run the app.
5. Add, edit, and remove some people.