using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tarefar.DB.Models;

namespace Tarefar.Infra.Repository
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll(Expression<Func<T, bool>> predicate = null);
        Task<T> Get(Expression<Func<T, bool>> predicate);
        Task Add(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task<int> Count();
    }
}
