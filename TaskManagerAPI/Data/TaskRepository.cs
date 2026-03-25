namespace TaskManagerAPI.Data
{
    public class TaskRepository : ITaskRepository
    {
        private readonly List<Models.Task> _tasks = new List<Models.Task>
        { 
        new Models.Task
        {
            Id = 1,
            Title = "Сделать лабораторную",
            Description = "ЛР 9, 10, 11",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Category = "Учёба",
            Priority = 5
        },
        new Models.Task
        {
            Id = 2,
            Title = "Проверить почту",
            Description = "",
            IsCompleted = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CompletedAt = DateTime.UtcNow,
            Category = "Работа",
            Priority = 3
        },
        new Models.Task
        {
            Id = 3,
            Title = "Купить продукты",
            Description = "Молоко, хлеб",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Category = "Дом",
            Priority = 2
        }
    };
        private int _nextId = 4;

        public System.Threading.Tasks.Task<List<Models.Task>> GetAllTasksAsync()
        {
            Console.WriteLine($"В репозитории {_tasks.Count} задач");
            foreach (var task in _tasks)
            {
                Console.WriteLine($"  - {task.Id}: {task.Title} ({task.Category})");
            }
            return System.Threading.Tasks.Task.FromResult(_tasks.ToList());
        }

        public System.Threading.Tasks.Task<Models.Task?> GetTaskByIdAsync(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            return System.Threading.Tasks.Task.FromResult(task);
        }

        public System.Threading.Tasks.Task<Models.Task> CreateTaskAsync(Models.Task task)
        {
            task.Id = _nextId++;
            task.CreatedAt = DateTime.UtcNow;
            _tasks.Add(task);
            return System.Threading.Tasks.Task.FromResult(task);
        }

        public System.Threading.Tasks.Task<Models.Task?> UpdateTaskAsync(int id, Models.Task task)
        {
            var existingTask = _tasks.FirstOrDefault(t => t.Id == id);
            if (existingTask == null)
                return System.Threading.Tasks.Task.FromResult<Models.Task?>(null);

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.IsCompleted = task.IsCompleted;

            if (task.IsCompleted && !existingTask.IsCompleted)
            {
                existingTask.CompletedAt = DateTime.UtcNow;
            }
            else if (!task.IsCompleted)
            {
                existingTask.CompletedAt = null;
            }

            return System.Threading.Tasks.Task.FromResult<Models.Task?>(existingTask);
        }

        public System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return System.Threading.Tasks.Task.FromResult(false);

            _tasks.Remove(task);
            return System.Threading.Tasks.Task.FromResult(true);
        }
    }
}
