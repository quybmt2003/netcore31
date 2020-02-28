using Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository.Interfaces
{
    public interface IUnitOfWork
    {
        void Save();
        IRepository<T, TKey> Repository<T, TKey>()
            where T : EntityBase<TKey>;
    }
}
