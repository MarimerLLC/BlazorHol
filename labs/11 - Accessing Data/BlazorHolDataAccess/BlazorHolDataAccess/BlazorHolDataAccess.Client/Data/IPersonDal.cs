namespace BlazorHolDataAccess.Data;

public interface IPersonDal
{
    Task<IEnumerable<PersonEntity>> GetPeopleAsync();
    Task<PersonEntity?> GetPersonAsync(int id);
    Task<int> AddPersonAsync(PersonEntity person);
    Task UpdatePersonAsync(PersonEntity person);
    Task DeletePersonAsync(int id);
}
