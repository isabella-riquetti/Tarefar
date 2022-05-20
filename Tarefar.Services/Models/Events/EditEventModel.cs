using System.ComponentModel.DataAnnotations;

namespace Tarefar.Services.Models.Events
{
    public class EditEventModel : EventModel
    {
        [Required]
        public long Id { get; set; }
    }
}
