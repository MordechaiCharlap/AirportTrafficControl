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
        List<T> GetAll();
        T? Get(int id);
        bool Update (T entity);
    }
}
