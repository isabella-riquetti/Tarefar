using Tarefar.DB.Models;

namespace Tarefar.Services.Services.Implementations
{
    public class EventService : IEventService
    {
        /// <summary>
        /// Extra validations that the data annotations are not validating
        /// </summary>
        /// <param name="obj">The event</param>
        /// <returns>If the event is valid</returns>
        public BaseResponse IsValid(Event obj)
        {
            if (obj == null)
            {
                return BaseResponse.CreateError("Events cannot be empty");
            }
            if (obj.EndDate == null && !obj.AllDay)
            {
                return BaseResponse.CreateError("Events that don't last the entire day must have End Date");
            }
            if (obj.EndDate != null && obj.EndDate < obj.StartDate)
            {
                return BaseResponse.CreateError("Even End Date must be greater thatn Start Date");
            }
            if (obj.ReocurrecyType == ReocurrecyType.Week && obj.WeeklyReocurrencyType == null)
            {
                return BaseResponse.CreateError("Events with weekly reocurrence need to have weekdays");
            }
            if (obj.ReocurrecyType == ReocurrecyType.Month && obj.MontlyReocurrencyType == null)
            {
                return BaseResponse.CreateError("Events with monthly reocurrence must select the reocurrence type");
            }
            // May be extended

            return BaseResponse.CreateSuccess();
        }
    }
}
