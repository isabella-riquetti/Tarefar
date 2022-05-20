using System;
using Tarefar.DB.Models;
using Tarefar.Infra.UnitOfWork;
using Tarefar.Services.Models.Events;

namespace Tarefar.Services.Services.Implementations
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EventService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

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

        public BaseResponse Create(CreateEventModel newEvent)
        {
            var newEventValidation = IsValid(newEvent);
            if (!newEventValidation.Success)
                return newEventValidation;

            _unitOfWork.Events.Add(newEvent);

            if(newEvent.ReocurrecyType != ReocurrecyType.Never)
                _AddReocurrencies(newEvent);

            _unitOfWork.Commit();
            return BaseResponse.CreateSuccess();
        }

        private void _AddReocurrencies(Event newEvent)
        {
            if(newEvent.ReocurrecyType == ReocurrecyType.Day)
                _AddDailyReocurrence(newEvent);
        }

        private void _AddDailyReocurrence(Event newEvent)
        {
            Event child = newEvent.Clone();
            child.ParentId = newEvent.Id;

            DateTime finalDate = newEvent.StartDate.AddYears(1);
            DateTime currentStartDate = newEvent.StartDate;
            DateTime? currentEndDate = newEvent.EndDate;

            while (currentStartDate < finalDate)
            {
                currentStartDate = currentStartDate.AddDays((int)newEvent.ReocurrencyFrequency);
                currentEndDate = currentEndDate?.AddDays((int)newEvent.ReocurrencyFrequency);

                var newChildEvent = newEvent.Clone();
                newChildEvent.StartDate = currentStartDate;
                newChildEvent.EndDate = currentEndDate;

                _unitOfWork.Events.Add(newChildEvent);
            }
        }
    }
}
