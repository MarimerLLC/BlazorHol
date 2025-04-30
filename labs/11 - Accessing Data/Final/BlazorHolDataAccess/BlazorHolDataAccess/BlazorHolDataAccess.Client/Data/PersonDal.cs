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
