using Entity;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Repositories;
using System;
using System.Collections.Generic;

namespace Repository
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private DbContext _dbContext;
        private IDictionary<string, object> _repositories;
        public UnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Dictionary<string, object>();
        }

        public IRepository<T, TKey> Repository<T, TKey>() where T : EntityBase<TKey>
        {
            if (_repositories.ContainsKey(nameof(T)))
                return _repositories[nameof(T)] as IRepository<T, TKey>;

            var repository = new Repository<T, TKey>(_dbContext);
            _repositories.Add(nameof(T), repository);
            return repository;
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
