using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Models.Requests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 символов")]
        public string Password { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
