using AirportTrafficControlTower.Data.Model;

namespace AirportTrafficControlTower.Data.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        T? GetById(int id);
        IQueryable<T> GetAll();
        bool Delete(int id);
        bool Update(T entity);
        void Create(T entity);
        Task SaveChangesAsync();
        void SaveChanges();
    }
}