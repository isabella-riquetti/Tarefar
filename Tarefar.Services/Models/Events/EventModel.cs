using System;
using System.ComponentModel.DataAnnotations;
using Tarefar.DB.Models;

namespace Tarefar.Services.Models.Events
{
    public class EventModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool AllDay { get; set; } = true;
        public ReocurrecyType ReocurrecyType { get; set; } = ReocurrecyType.Never;
        public int? ReocurrencyFrequency { get; set; }
        public WeeklyReocurrencyType? WeeklyReocurrencyType { get; set; }
        public MontlyReocurrencyType? MontlyReocurrencyType { get; set; }

        public string Description { get; set; }
    }
}
