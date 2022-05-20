using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tarefar.DB;
using Tarefar.DB.Models;

namespace Tarefar.Infra.Repository.Implementation
{
    [ExcludeFromCodeCoverage]
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApiContext _context;
        private DbSet<T> _dbSet;
        
        public Repository(ApiContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate != null)
                return _dbSet.Where(predicate);

            return _dbSet;
        }

        public async Task<T> Get(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task Add(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<int> Count()
        {
            return await _dbSet.CountAsync();
        }
    }
}
