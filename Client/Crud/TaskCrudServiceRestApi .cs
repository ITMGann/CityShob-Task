using Client.Utils;
using Common.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity;

namespace Client.Crud
{
    public class TaskCrudServiceRestApi : ITaskCrudService
    {
        private readonly TaskRestApiClient _taskRestApiClient;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        //public TaskCrudServiceRestApi(string baseUrl)
        public TaskCrudServiceRestApi([Dependency("TaskCrudServiceRestApiBaseUrl")] string baseUrl)
        {
            _taskRestApiClient = new TaskRestApiClient(baseUrl);
        }

        public async Task<List<TaskDto>> GetTaskDtosAsync(int pageNumber, int pageSize)
        {
            try
            {
                List<TaskDto> taskDtos = await _taskRestApiClient.GetTaskDtosAsync(pageNumber, pageSize);
                return taskDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting task DTOs.");
                throw;
            }
        }

        public async Task<TaskModel> GetTaskByIdAsync(int taskId)
        {
            try
            {
                TaskModel taskModel = await _taskRestApiClient.GetTaskByIdAsync(taskId);
                return taskModel;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error getting task by ID = {taskId}.");
                throw;
            }
        }

        public async Task<TaskModel> CreateTaskAsync(TaskModel taskModel)
        {
            try
            {
                TaskModel createdTask = await _taskRestApiClient.CreateTaskAsync(taskModel);
                return createdTask;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating task.");
                throw;
            }
        }

        public async Task<TaskModel> UpdateTaskAsync(TaskModel taskModel)
        {
            try
            {
                TaskModel updatedTask = await _taskRestApiClient.UpdateTaskAsync(taskModel);
                return updatedTask;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error updating task with ID = {taskModel.Id}.");
                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            try
            {
                bool isDeleted = await _taskRestApiClient.DeleteTaskAsync(taskId);
                return isDeleted;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error deleting task with ID = {taskId}.");
                throw;
            }
        }
    }
}
