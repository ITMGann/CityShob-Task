using Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Data
{
    public interface ITaskRepository
    {
        Task<List<TaskDto>> GetAllTasksAsync(int pageNumber, int pageSize);
        Task<TaskModel> GetTaskByIdAsync(int id);
        Task<TaskModel> CreateTaskAsync(TaskModel taskModel);
        Task<TaskModel> UpdateTaskAsync(TaskModel taskModel);
        Task<bool> DeleteTaskAsync(int id);
    }
}
