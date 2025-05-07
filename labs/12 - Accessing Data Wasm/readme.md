# Accessing Databases from Blazor WebAssembly

## Using the Solution

This lab uses the same solution as the previous lab. If you have not completed the previous lab, you can find the solution in the `labs/11/Final` folder.

1. Open Visual Studio
1. Open the `BlazorHolDataAccess` solution

## Enabling Controllers

1. Open the `Program.cs` file in the server project
1. Add the following code to the class:

```csharp
builder.Services.AddControllers();
```

1. Add the following code to the class:

```csharp
app.MapControllers();
```

## Implementing the Controller

1. Add a new folder to the server project called `Controllers`
1. Add a new class to the `Controllers` folder called `PersonController`
1. Add the following code to the `PersonController` class:

```csharp
using BlazorHolDataAccess.Data;
using Microsoft.AspNetCore.Mvc;

namespace BlazorHolDataAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonController(IPersonDal personDal) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<PersonEntity>> Get()
    {
        return await personDal.GetPeopleAsync();
    }

    [HttpGet("{id}")]
    public async Task<PersonEntity?> GetPerson(int id)
    {
        return await personDal.GetPersonAsync(id);
    }

    [HttpPost]
    public async Task<PersonEntity> Post(PersonEntity person)
    {
        if (person.Id == 0)
        {
            var newId = await Put(person);
            person.Id = newId;
            return person;
        }
        else
        {
            await personDal.UpdatePersonAsync(person);
            return person;
        }
    }

    [HttpPut]
    public async Task<int> Put(PersonEntity person)
    {
        return await personDal.AddPersonAsync(person);
    }

    [HttpDelete("{id}")]
    public async Task Delete(int id)
    {
        await personDal.DeletePersonAsync(id);
    }
}
```

This code creates a controller that will handle requests for people data. The controller uses the `IPersonDal` interface to interact with the database. The controller has methods to get all people, get a single person, add a person, update a person, and delete a person.

## Implementing the Client-Side Data Access Layer

1. Add a `PersonDal` class to the `Data` folder in the cient project
1. Add the following code to the `PersonDal` class:

```csharp
using BlazorHolDataAccess.Data;
using System.Net.Http.Json;

namespace BlazorHolDataAccess.Client.Data;

public class PersonDal(HttpClient httpClient) : IPersonDal
{
    public async Task<int> AddPersonAsync(PersonEntity person)
    {
        var result = await httpClient.PutAsJsonAsync("api/person", person);
        return await result.Content.ReadFromJsonAsync<int>();
    }

    public Task DeletePersonAsync(int id)
    {
        return httpClient.DeleteAsync($"api/person/{id}");
    }

    public Task<IEnumerable<PersonEntity>> GetPeopleAsync()
    {
        var result = httpClient.GetFromJsonAsync<IEnumerable<PersonEntity>>("api/person");
        if (result == null)
            return Task.FromResult(Enumerable.Empty<PersonEntity>());
        else
            return result!;
    }

    public Task<PersonEntity?> GetPersonAsync(int id)
    {
        return httpClient.GetFromJsonAsync<PersonEntity>($"api/person/{id}");
    }

    public Task UpdatePersonAsync(PersonEntity person)
    {
        return httpClient.PostAsJsonAsync("api/person", person);
    }
}
```

This code implements the `IPersonDal` interface using an `HttpClient` to make requests to the server.

## Registering the Data Access Layer

1. Open the `Program.cs` file in the client project
1. Add the following code to the class:

```csharp
builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IPersonDal, BlazorHolDataAccess.Client.Data.PersonDal>();
```

This code registers the client-side `PersonDal` class with the dependency injection container. This is only registered for the client project, so the server project will still use the server-side `PersonDal` class.

An `HttpClient` service is also registered with the dependency injection container. This service is used by the `PersonDal` class to make requests to the server. Notice how the `BaseAddress` of the `HttpClient` is set to the web server from which the Blazor WebAssembly app is served.

## Make the Edit Pages Run in WebAssembly

1. Open the `PersonEdit.razor` file in the `Pages` folder of the client project
1. Change the `@renderMode` of the page

```csharp
@rendermode InteractiveAuto
```

3. Do the same for the `RemovePerson.razor` file

These pages will now run on the server and will switch to WebAssembly once the DLLs have been downloaded to the browser.

## Running the Application

1. Run the application
1. Navigate to the People List page
1. Add or edit a person
1. Navigate to the Home page
1. Navigate back to the People List page
1. Add or edit a person

The second time you add or edit a person, the page will switch to WebAssembly and the changes will be made on the server via the API.

Notice how the _page_ code is the same for both server and WebAssembly. The only difference is the data access layer, which is implemented differently for each platform.
