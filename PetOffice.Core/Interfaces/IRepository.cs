using PetOffice.Core.Models;

namespace PetOffice.Core.Interfaces
{
    public interface IRepository<T> where T : OfficePet
    {
        void Add(T item);

        bool Remove(T item);

        IEnumerable<T> GetAll();

        T? Find(Func<T, bool> predicate);
    }
}