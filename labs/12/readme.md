# Accessing Databases from Blazor WebAssembly

## Using the Solution

This lab uses the same solution as the previous lab. If you have not completed the previous lab, you can find the solution in the `labs/11` folder.

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
1. Add a new class to the `Controllers` folder called `PeopleController`
1. Add the following code to the `PeopleController` class:

```csharp
using BlazorHolDataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorHolDataAccess.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PeopleController : ControllerBase
    {
        private readonly DataAccess _dataAccess;

        public PeopleController(DataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        [HttpGet]
        public async Task<IEnumerable<Person>> Get()
        {
            return await _dataAccess.GetPeopleAsync();
        }

        [HttpPost]
        public async Task Post(Person person)
        {
            await _dataAccess.AddPersonAsync(person);
        }

        [HttpPut]
        public async Task Put(Person person)
        {
            await _dataAccess.UpdatePersonAsync(person);
        }

        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            await _dataAccess.DeletePersonAsync(id);
        }
    }
}
```
