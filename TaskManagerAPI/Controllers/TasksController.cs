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
        public async System.Threading.Tasks.Task<ActionResult<List<Models.Task>>> GetAllTasks()
        {
            var tasks = await _repository.GetAllTasksAsync();
            return Ok(tasks);
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
