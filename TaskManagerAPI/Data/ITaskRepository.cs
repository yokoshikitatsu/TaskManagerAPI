using TaskManagerAPI.Models;

namespace TaskManagerAPI.Data
{
    public interface ITaskRepository
    {
        System.Threading.Tasks.Task<List<Models.Task>> GetAllTasksAsync();
        System.Threading.Tasks.Task<Models.Task?> GetTaskByIdAsync(int id);
        System.Threading.Tasks.Task<Models.Task> CreateTaskAsync(Models.Task task);
        System.Threading.Tasks.Task<Models.Task?> UpdateTaskAsync(int id, Models.Task task);
        System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id);
    }
}
