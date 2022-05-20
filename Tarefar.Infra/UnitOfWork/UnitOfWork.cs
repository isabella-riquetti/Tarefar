using System;
using System.Diagnostics.CodeAnalysis;
using Tarefar.DB;
using Tarefar.DB.Models;
using Tarefar.Infra.Repository;
using Tarefar.Infra.Repository.Implementation;

namespace Tarefar.Infra.UnitOfWork
{
    [ExcludeFromCodeCoverage]
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApiContext _context;

        public IRepository<Event> Events { get; private set; }

        public UnitOfWork(ApiContext context)
        {
            _context = context;
            
            Events = new Repository<Event>(context);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
