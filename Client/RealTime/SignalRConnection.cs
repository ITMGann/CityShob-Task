using Common;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.RealTime
{
    // TODO : Find a less general name for the class.
    public class SignalRConnection
    {
        private readonly IHubProxy _taskHubProxy;
        private readonly HubConnection _connection;

        public event Action<int> TaskLocked;
        public event Action<int> TaskUnlocked;
        public event Action<HashSet<int>> LockedTasksReceived;

        public event Action<StateChange> HubConnectionStateChanged;

        public SignalRConnection(HubConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _taskHubProxy = _connection.CreateHubProxy("TaskLockHub");

            _connection.StateChanged += hubConnection_StateChanged;

            _taskHubProxy.On<int>(SignalRConstants.TaskLockedMethod, taskId =>
            {
                TaskLocked?.Invoke(taskId);
            });

            _taskHubProxy.On<int>(SignalRConstants.TaskUnlockedMethod, taskId =>
            {
                TaskUnlocked?.Invoke(taskId);
            });

            _taskHubProxy.On<HashSet<int>>(SignalRConstants.SendLockedTasksOnConnectMethod, lockedTasks =>
            {
                LockedTasksReceived?.Invoke(lockedTasks);
            });
        }

        private void hubConnection_StateChanged(StateChange stateChange)
        {
            HubConnectionStateChanged?.Invoke(stateChange);
        }

        public async Task StartConnectionAsync()
        {
            await _connection.Start();
        }

        public void StopConnection()
        {
            _connection.Stop();
        }


    }
}
