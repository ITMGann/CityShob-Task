using System.Collections.Generic;

namespace Server.TaskLocker
{
    public interface ITaskLocker
    {
        void Lock(int taskId);
        void Unlock(int taskId);
        bool IsLocked(int taskId);
        ICollection<int> GetLockedIds();
    }
}
