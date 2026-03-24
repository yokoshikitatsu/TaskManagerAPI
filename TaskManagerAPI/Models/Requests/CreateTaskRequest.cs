using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Models.Requests
{
    public class CreateTaskRequest
    {
        [Required(ErrorMessage = "Название задачи обязательно")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 100 символов")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание не может превышать 500 символов")]
        public string Description { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Приоритет должен быть от 1 до 5")]
        public int Priority { get; set; } = 1;

        [StringLength(50, ErrorMessage = "Категория не может превышать 50 символов")]
        public string Category { get; set; } = "General";
    }
}
