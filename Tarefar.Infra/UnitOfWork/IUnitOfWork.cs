using Tarefar.DB.Models;
using Tarefar.Infra.Repository;

namespace Tarefar.Infra.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Event> Events { get; }

        void Commit();
    }
}
