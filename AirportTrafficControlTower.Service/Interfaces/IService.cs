using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportTrafficControlTower.Service.Interfaces
{
    public interface IService<T> where T:class
    {
        Task Create(T entity);
        Task<List<T>> GetAll();
        Task<T?> Get(int id);
        Task<bool> Update (T entity);
    }
}
