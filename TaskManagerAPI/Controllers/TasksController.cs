using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;
using TaskManagerAPI.Models.Requests;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _repository;
        private readonly APISettings _apiSettings;
        private readonly ILogger<TasksController> _logger;

        public TasksController(
            ITaskRepository repository,
            IOptions<APISettings> apiSettings,
            ILogger<TasksController> logger)
        {
            _repository = repository;
            _apiSettings = apiSettings.Value;
            _logger = logger;
        }

        [HttpGet("test-error")]
        public ActionResult<object> TestError()
        {
            _logger.LogWarning("Вызван тестовый эндпоинт для генерации исключения");
            throw new InvalidOperationException("Тест глобального обработчика исключений!");
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetAllTasks(
            [FromQuery] string? category = null,
            [FromQuery] int? priority = null,
            [FromQuery] bool? isCompleted = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation(
                "Запрос на получение задач: category={Category}, priority={Priority}, isCompleted={IsCompleted}, sortBy={SortBy}, page={Page}, pageSize={PageSize}",
                category, priority, isCompleted, sortBy, page, pageSize);

            var tasks = await _repository.GetAllTasksAsync();
            _logger.LogDebug("Получено {Count} задач из репозитория", tasks.Count);

            var query = tasks.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t =>
                    t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
                _logger.LogDebug("Применена фильтрация по категории: {Category}", category);
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
                _logger.LogDebug("Применена фильтрация по приоритету: {Priority}", priority.Value);
            }

            if (isCompleted.HasValue)
            {
                query = query.Where(t => t.IsCompleted == isCompleted.Value);
                _logger.LogDebug("Применена фильтрация по статусу: {IsCompleted}", isCompleted.Value);
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                string order = string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder;
                _logger.LogDebug("Применена сортировка: {SortBy} {SortOrder}", sortBy, order);

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

            var totalCount = query.Count();
            pageSize = Math.Clamp(pageSize, 1, 50);
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            _logger.LogInformation(
                "Возвращено {ItemCount} задач из {TotalCount} (страница {Page} из {TotalPages})",
                items.Count, totalCount, page, (int)Math.Ceiling((double)totalCount / pageSize));

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
            _logger.LogInformation("Запрос на получение задачи по ID: {Id}", id);

            var task = await _repository.GetTaskByIdAsync(id);

            if (task == null)
            {
                _logger.LogWarning("Задача с ID {Id} не найдена", id);
                return NotFound(new ErrorResponse
                {
                    Error = "TaskNotFound",
                    Message = $"Задача с ID {id} не найдена.",
                    StatusCode = 404
                });
            }

            _logger.LogDebug("Задача найдена: ID={Id}, Title={Title}", task.Id, task.Title);
            return Ok(new { data = task });
        }
        [HttpPost]
        public async Task<ActionResult<object>> CreateTask([FromBody] CreateTaskRequest request)
        {
            _logger.LogInformation("Попытка создания задачи: Title={Title}, Category={Category}, Priority={Priority}",
                request.Title, request.Category, request.Priority);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Ошибка валидации при создании задачи: {Errors}", string.Join("; ", errors));

                return BadRequest(new ErrorResponse
                {
                    Error = "ValidationError",
                    Message = "Ошибки валидации входных данных",
                    StatusCode = 400,
                    Details = errors
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

            _logger.LogInformation("Задача успешно создана: Id={Id}, Title={Title}", createdTask.Id, createdTask.Title);

            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id },
                new { data = createdTask, message = "Задача успешно создана" });
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
        {
            _logger.LogInformation("Попытка обновления задачи: Id={Id}, Title={Title}", id, request.Title);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Ошибка валидации при обновлении задачи: {Errors}", string.Join("; ", errors));

                return BadRequest(new ErrorResponse
                {
                    Error = "ValidationError",
                    Message = "Ошибки валидации входных данных",
                    StatusCode = 400,
                    Details = errors
                });
            }

            if (id != request.Id)
            {
                _logger.LogWarning("Несоответствие ID: URL={UrlId}, Body={BodyId}", id, request.Id);
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
                _logger.LogWarning("Задача с ID {Id} не найдена для обновления", id);
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
                _logger.LogDebug("Установлена CompletedAt={CompletedAt} для задачи {Id}", task.CompletedAt, task.Id);
            }
            else if (!task.IsCompleted)
            {
                task.CompletedAt = null;
                _logger.LogDebug("Очищена CompletedAt для задачи {Id}", task.Id);
            }

            var updatedTask = await _repository.UpdateTaskAsync(id, task);

            _logger.LogInformation("Задача успешно обновлена: Id={Id}", updatedTask.Id);

            return Ok(new { data = updatedTask, message = "Задача успешно обновлена" });
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteTask(int id)
        {
            _logger.LogInformation("Попытка удаления задачи: Id={Id}", id);

            var deleted = await _repository.DeleteTaskAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Задача с ID {Id} не найдена для удаления", id);
                return NotFound(new ErrorResponse
                {
                    Error = "TaskNotFound",
                    Message = $"Задача с ID {id} не найдена.",
                    StatusCode = 404
                });
            }

            _logger.LogInformation("Задача успешно удалена: Id={Id}", id);
            return Ok(new { message = "Задача успешно удалена", id = id });
        }
    }
}
