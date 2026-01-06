using NLog;
using Server.Cache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.TaskLocker
{
    public class TaskLockerCache : ITaskLocker
    {
        private const string KeyPrefix = "TaskLocked_";
        private const string KeySuffix = "";

        private readonly ICache _cache;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TaskLockerCache(ICache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public void Lock(int taskId)
        {
            try
            {
                bool isLocked = IsLocked(taskId);
                if (isLocked)
                {
                    Logger.Warn($"Task {taskId} is already locked.");
                    return;
                }

                string key = createKeyByTaskId(taskId);
                _cache.Add(key, taskId);

                Logger.Info($"Task {taskId} locked.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error locking task with ID = {taskId}.");
                throw new ApplicationException("An error occurred while locking the task.", ex);
            }
        }

        public void Unlock(int taskId)
        {
            try
            {
                bool isLocked = IsLocked(taskId);
                if (isLocked == false)
                {
                    Logger.Warn($"Task with ID = {taskId} is not locked, so it can't be unlocked.");
                    return;
                }

                string key = createKeyByTaskId(taskId);
                _cache.Remove(key);
                Logger.Info($"Task with ID = {taskId} unlocked.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error unlocking task.");
                throw new ApplicationException("An error occurred while unlocking the task.", ex);
            }
        }

        public bool IsLocked(int taskId)
        {
            try
            {
                string key = createKeyByTaskId(taskId);

                return _cache.Get(key) != null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error checking if task with ID = {taskId} is locked.");
                throw new ApplicationException("An error occurred while checking if the task is locked.", ex);
            }
        }

        public ICollection<int> GetLockedIds()
        {
            ICollection<string> lockedTasks = _cache.GetAllKeys();

            return lockedTasks.Select(key => extractTaskIdFromKey(key))
                              .ToList();
        }

        private string createKeyByTaskId(int taskId)
        {
            return $"{KeyPrefix}{taskId}{KeySuffix}";
        }

        private int extractTaskIdFromKey(string key)
        {
            string taskIdString = key.Replace(KeyPrefix, string.Empty)
                                     .Replace(KeySuffix, string.Empty);

            return int.Parse(taskIdString);
        }
    }
}