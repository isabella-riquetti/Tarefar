using System;
using Tarefar.DB.Models;

namespace Tarefar.Services.Models.Events
{
    public class CreateEventModel : EventModel
    {
        public DateTime CreatedAt { get; set; }

        public static implicit operator Event(CreateEventModel eventModel)
        {
            return new Event()
            {
                CreatedAt = eventModel.CreatedAt,
                Name = eventModel.Name,
                StartDate = eventModel.AllDay
                    ? eventModel.StartDate.Date
                    : eventModel.StartDate,
                EndDate = eventModel.AllDay && eventModel.EndDate != null
                    ? eventModel.EndDate.Value.Date
                    : eventModel.EndDate,
                AllDay = eventModel.AllDay,
                ReocurrecyType = eventModel.ReocurrecyType,
                ReocurrencyFrequency =
                    eventModel.ReocurrecyType != ReocurrecyType.Never
                    && eventModel.ReocurrencyFrequency == null
                    ? 1 : eventModel.ReocurrencyFrequency,
                WeeklyReocurrencyType = eventModel.WeeklyReocurrencyType,
                MontlyReocurrencyType = eventModel.MontlyReocurrencyType
            };
        }
    }
}
