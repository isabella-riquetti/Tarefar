using Tarefar.DB.Models;
using Tarefar.Services.Models.Events;

namespace Tarefar.Services.Services
{
    public interface IEventService
    {
        /// <summary>
        /// Extra validations that the data annotations are not validating
        /// </summary>
        /// <param name="obj">The event</param>
        /// <returns>If the event is valid</returns>
        BaseResponse IsValid(Event obj);

        /// <summary>
        /// Create a new event
        /// </summary>
        /// <param name="newEvent">New event to be created</param>
        /// <returns></returns>
        BaseResponse Create(CreateEventModel newEvent);
    }
}
