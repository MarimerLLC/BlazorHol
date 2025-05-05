namespace BlazorHolData.Data
{
    public class PersonDal : IPersonDal
    {
        public Task<Person> GetPerson(int id)
        {
            Person Person;
            if (id > 0)
            {
                Person = Data.Database.People.Where(p => p.Id == id).Select(_ => new Person
                { Id = _.Id, FirstName = _.FirstName, LastName = _.LastName, Age = _.Age }).First();
            }
            else
            {
                Person = new Person { Id = -1, FirstName = string.Empty, LastName = string.Empty };
            }
            return Task.FromResult(Person);
        }

        public Task<Person> SavePerson(Person person)
        {
            ArgumentNullException.ThrowIfNull(person);
            if (person.Id > 0)
            {
                var existingPerson = Data.Database.People.Where(p => p.Id == person.Id).First();
                existingPerson.FirstName = person.FirstName;
                existingPerson.LastName = person.LastName;
                existingPerson.Age = person.Age;
                person = existingPerson;
            }
            else
            {
                var newId = Data.Database.People.Max(p => p.Id) + 1;
                person.Id = newId;
                Data.Database.People.Add(person);
            }
            return Task.FromResult(person);
        }
    }
}
