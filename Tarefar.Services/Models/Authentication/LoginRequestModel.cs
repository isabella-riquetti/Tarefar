using System.ComponentModel.DataAnnotations;

namespace Tarefar.Services.Models.Authentication
{
    public class LoginRequestModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
