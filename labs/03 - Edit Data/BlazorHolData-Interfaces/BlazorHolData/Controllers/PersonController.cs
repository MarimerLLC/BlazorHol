using BlazorHolData.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazorHolData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController(IPersonDal personDal) : ControllerBase
    {
        [HttpGet]
        public async Task<Person> GetPerson(int id)
        {
            return await personDal.GetPerson(id);
        }

        [HttpPost]
        public async Task<Person> SavePerson(Person person)
        {
            return await personDal.SavePerson(person);
        }
    }
}
