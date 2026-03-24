using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;
using TaskManagerAPI.Models.Requests;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _repository;
        public TasksController(ITaskRepository repository)
        {
            _repository = repository;
        }

        // GET: api/tasks
        [HttpGet]
        public ActionResult<object> GetAllTasks(
            [FromQuery] string? category = null,
            [FromQuery] int? priority = null,
            [FromQuery] bool? isCompleted = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            // Получаем все задачи из репозитория
            var tasks = _repository.GetAllTasksAsync().Result;

            // Применяем фильтрацию
            var query = tasks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            if (isCompleted.HasValue)
            {
                query = query.Where(t => t.IsCompleted == isCompleted.Value);
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
              
                string order = string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder;

                query = sortBy.ToLower() switch
                {
                    "priority" => order.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.Priority)
                        : query.OrderBy(t => t.Priority),
                    "createdat" => order.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.CreatedAt)
                        : query.OrderBy(t => t.CreatedAt),
                    "title" => order.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.Title)
                        : query.OrderBy(t => t.Title),
                    _ => query.OrderBy(t => t.Id)
                };
            }
            else
            {
                query = query.OrderBy(t => t.Id);
            }

            // Пагинация
            var totalCount = query.Count();
            pageSize = Math.Clamp(pageSize, 1, 50);
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new
            {
                items,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTask(int id)
        {
            var task = await _repository.GetTaskByIdAsync(id);

            if (task == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "TaskNotFound",
                    Message = $"Задача с ID {id} не найдена.",
                    StatusCode = 404
                });
            }

            return Ok(new { data = task });
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<ActionResult<object>> CreateTask([FromBody] CreateTaskRequest request)
        {
            // Валидация выполняется автоматически благодаря [ApiController]
            // Но можно добавить дополнительную проверку
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "ValidationError",
                    Message = "Ошибки валидации входных данных",
                    StatusCode = 400,
                    Details = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var task = new Models.Task
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                Category = request.Category,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdTask = await _repository.CreateTaskAsync(task);

            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id },
                new { data = createdTask, message = "Задача успешно создана" });
        }

        // PUT: api/tasks/5
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "ValidationError",
                    Message = "Ошибки валидации входных данных",
                    StatusCode = 400,
                    Details = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            if (id != request.Id)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "IdMismatch",
                    Message = "ID в URL не совпадает с ID в теле запроса.",
                    StatusCode = 400
                });
            }

            var task = await _repository.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "TaskNotFound",
                    Message = $"Задача с ID {id} не найдена.",
                    StatusCode = 404
                });
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.IsCompleted = request.IsCompleted;
            task.Priority = request.Priority;
            task.Category = request.Category;

            if (task.IsCompleted && task.CompletedAt == null)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            else if (!task.IsCompleted)
            {
                task.CompletedAt = null;
            }

            var updatedTask = await _repository.UpdateTaskAsync(id, task);

            return Ok(new { data = updatedTask, message = "Задача успешно обновлена" });
        }

        // DELETE: api/tasks/5
        [HttpDelete("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> DeleteTask(int id)
        {
            var deleted = await _repository.DeleteTaskAsync(id);
            if (!deleted)
            {
                return NotFound($"Задача с ID {id} не найдена.");
            }
            return NoContent();
        }
    }
}
