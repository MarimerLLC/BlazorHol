namespace BlazorHolData.Data
{
    public interface IPersonDal
    {
        Task<Person> GetPerson(int id);
        Task<Person> SavePerson(Person person);
    }
}
