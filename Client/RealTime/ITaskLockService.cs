using System.Threading.Tasks;

namespace Client.RealTime
{
    public interface ITaskLockService
    {
        Task BeginUpdateAsync(int taskId);
        Task CancelUpdateAsync(int taskId);
    }
}
