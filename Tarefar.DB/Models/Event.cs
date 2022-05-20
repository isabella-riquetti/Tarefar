using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tarefar.DB.Models
{
    public class Event
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [DefaultValue("getdate()")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

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

        [Required]
        public long UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public long? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Event Parent { get; set; }
        public virtual ICollection<Event> Reocurrencies { get; set; }
    }

    public enum ReocurrecyType
    {
        Never = 0,
        Day = 1,
        Week = 2,
        Month = 3,
        Year = 4
    }

    [Flags]
    public enum WeeklyReocurrencyType
    {
        Weekday = Monday | Tuesday | Wednesday | Thursday | Friday,
        Weekend = Saturday | Sunday,
        Sunday = 1 << 0,
        Monday = 1 << 1,
        Tuesday = 1 << 2,
        Wednesday = 1 << 3,
        Thursday = 1 << 4,
        Friday = 1 << 5,
        Saturday = 1 << 6
    }

    public enum MontlyReocurrencyType
    {
        SameDay = 0,
        SameDayOfTheWeek = 1
    }
}
