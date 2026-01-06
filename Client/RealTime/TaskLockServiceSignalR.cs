using Common;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Client.RealTime
{
    public class TaskLockServiceSignalR : ITaskLockService
    {
        private readonly IHubProxy _taskHubProxy;
        private readonly HubConnection _connection;

        public TaskLockServiceSignalR(HubConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _taskHubProxy = _connection.CreateHubProxy("TaskLockHub");
        }

        public async Task StartConnectionAsync()
        {
            await _connection.Start();
        }

        public async Task BeginUpdateAsync(int taskId)
        {
            await _taskHubProxy.Invoke(SignalRConstants.BeginUpdateMethod, taskId);
        }

        public async Task CancelUpdateAsync(int taskId)
        {
            await _taskHubProxy.Invoke(SignalRConstants.CancelUpdateMethod, taskId);
        }

        public void StopConnection()
        {
            _connection.Stop();
        }
    }
}
