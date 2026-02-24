namespace TaskManagerAPI.Data
{
    public class TaskRepository : ITaskRepository
    {
        private readonly List<Models.Task> _tasks = new List<Models.Task>();
        private int _nextId = 1;

        public System.Threading.Tasks.Task<List<Models.Task>> GetAllTasksAsync()
        {
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
