using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;

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

        // GET: api/tasks/5
        [HttpGet("{id}")]
        public async System.Threading.Tasks.Task<ActionResult<Models.Task>> GetTask(int id)
        {
            var task = await _repository.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound($"Задача с ID {id} не найдена.");
            }
            return Ok(task);
        }

        // POST: api/tasks
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult<Models.Task>> CreateTask(Models.Task task)
        {
            if (string.IsNullOrWhiteSpace(task.Title))
            {
                return BadRequest("Название задачи не может быть пустым.");
            }
            var createdTask = await _repository.CreateTaskAsync(task);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
        }

        // PUT: api/tasks/5
        [HttpPut("{id}")]
        public async System.Threading.Tasks.Task<ActionResult<Models.Task>> UpdateTask(int id, Models.Task task)
        {
            if (id != task.Id)
            {
                return BadRequest("ID в URL не совпадает с ID в теле запроса.");
            }
            var updatedTask = await _repository.UpdateTaskAsync(id, task);
            if (updatedTask == null)
            {
                return NotFound($"Задача с ID {id} не найдена.");
            }
            return Ok(updatedTask);
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
