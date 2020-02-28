using Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IRepository<T, TKey>
        where T : EntityBase<TKey>
    {
        Task<T> Get(TKey id);
        IEnumerable<T> Get(Func<T, bool> predicate);

        void Insert(T entity);

        void Update(T entity);

        Task Delete(TKey id);
    }
}
