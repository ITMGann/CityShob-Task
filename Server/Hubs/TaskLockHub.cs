using Common;
using Microsoft.AspNet.SignalR;
using NLog;
using Server.TaskLocker;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Hubs
{
    public class TaskLockHub : Hub
    {
        private readonly ITaskLocker _taskLocker;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public TaskLockHub(ITaskLocker taskLocker)
        {
            _taskLocker = taskLocker ?? throw new ArgumentNullException(nameof(taskLocker));
        }

        public async Task BeginUpdateAsync(int taskId)
        {
            try
            {
                _taskLocker.Lock(taskId);
                await Clients.All.InvokeAsync(SignalRConstants.TaskLockedMethod, taskId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error locking task.");
                throw new ApplicationException("An error occurred while locking the task.", ex);
            }
        }

        public async Task CancelUpdateAsync(int taskId)
        {
            try
            {
                _taskLocker.Unlock(taskId);
                await Clients.All.InvokeAsync(SignalRConstants.TaskUnlockedMethod, taskId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error unlocking task.");
                throw new ApplicationException("An error occurred while unlocking the task.", ex);
            }
        }

        public override async Task OnConnected()
        {
            try
            {
                // Send the locked tasks to the client that has just connected
                //List<int> lockedTasks = _taskLocker.GetLockedIds();
                ICollection<int> lockedTasks = _taskLocker.GetLockedIds();
                await Clients.Caller.InvokeAsync(SignalRConstants.SendLockedTasksOnConnectMethod, lockedTasks);

                _logger.Info($"Client connected: {Context.ConnectionId}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error upon client connecting.");
                throw new ApplicationException("An error occurred upon client connecting.", ex);
            }

            await base.OnConnected();
        }
    }
}
