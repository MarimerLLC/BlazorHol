#nullable disable

namespace BlazorHolData.Data
{
    public class PersonDalClient(HttpClient client) : IPersonDal
    {
        public async Task<Person> GetPerson(int id)
        {
            return await client.GetFromJsonAsync<Person>($"api/person/{id}");
        }

        public async Task<Person> SavePerson(Person person)
        {
            var response = await client.PostAsJsonAsync<Person>("api/person", person);
            return person;
        }
    }
}
