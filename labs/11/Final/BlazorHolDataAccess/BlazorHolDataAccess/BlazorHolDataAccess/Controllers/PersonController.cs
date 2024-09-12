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
            return await Put(person);
        }
        else
        {
            await personDal.UpdatePersonAsync(person);
            return person;
        }
    }

    [HttpPut]
    public async Task<PersonEntity> Put(PersonEntity person)
    {
        var newId = await personDal.AddPersonAsync(person);
        person.Id = newId;
        return person;
    }

    [HttpDelete("{id}")]
    public async Task Delete(int id)
    {
        await personDal.DeletePersonAsync(id);
    }
}