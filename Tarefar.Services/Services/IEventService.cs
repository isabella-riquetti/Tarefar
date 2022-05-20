using Tarefar.DB.Models;

namespace Tarefar.Services.Services
{
    public interface IEventService
    {
        BaseResponse IsValid(Event task);
    }
}
