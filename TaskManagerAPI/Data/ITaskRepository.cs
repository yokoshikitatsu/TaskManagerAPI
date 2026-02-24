namespace TaskManagerAPI.Data
{
    public interface ITaskRepository
    {
        public interface ITaskRepository
        {
            // System.Threading.Tasks.Task
            // List<Task> 
            System.Threading.Tasks.Task<List<Task>> GetAllTasksAsync();
            System.Threading.Tasks.Task<Task?> GetTaskByIdAsync(int id);
            System.Threading.Tasks.Task<Task> CreateTaskAsync(Task task);
            System.Threading.Tasks.Task<Task?> UpdateTaskAsync(int id, Task task);
            System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id);
        }
    }
}
