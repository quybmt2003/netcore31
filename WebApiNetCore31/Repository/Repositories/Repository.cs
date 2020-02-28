using Entity;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class Repository<T, TKey> : IRepository<T, TKey>
        where T : EntityBase<TKey>
    {
        private DbContext _context;
        private DbSet<T> _dbSet;
        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task Delete(TKey id)
        {
            T entity = await Get(id);
            _dbSet.Remove(entity);
        }

        public async Task<T> Get(TKey id)
        {
            return await _dbSet.FindAsync(id);
        }

        public IEnumerable<T> Get(Func<T, bool> predicate)
        {
            return _dbSet.AsNoTracking().Where(predicate);
        }

        public void Insert(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Added;
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}
