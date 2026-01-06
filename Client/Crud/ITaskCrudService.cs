using Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Crud
{
    public interface ITaskCrudService
    {
        Task<List<TaskDto>> GetTaskDtosAsync(int pageNumber, int pageSize);
        Task<TaskModel> GetTaskByIdAsync(int id);
        Task<TaskModel> CreateTaskAsync(TaskModel taskModel);
        Task<TaskModel> UpdateTaskAsync(TaskModel taskModel);
        Task<bool> DeleteTaskAsync(int id);
    }
}
