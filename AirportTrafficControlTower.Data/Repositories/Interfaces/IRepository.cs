namespace AirportTrafficControlTower.Data.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetById(int id);
        IQueryable<T> GetAll();
        Task<bool> Delete(int id);
        Task<bool> Update(T entity);
        void Create(T entity);
        Task SaveChangesAsync();
    }
}