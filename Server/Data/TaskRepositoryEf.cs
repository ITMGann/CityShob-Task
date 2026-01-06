using Common.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Data
{
    public class TaskRepositoryEf : ITaskRepository, IDisposable
    {
        private readonly TaskDbContext _dbContext;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public TaskRepositoryEf(TaskDbContext context)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<TaskDto>> GetAllTasksAsync(int pageNumber, int pageSize)
        {
            try
            {
                List<TaskDto> tasks = await _dbContext.Tasks
                    .OrderBy(taskModel => taskModel.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(taskModel => new TaskDto
                    {
                        Id          = taskModel.Id,
                        Title       = taskModel.Title,
                        Priority    = taskModel.Priority,
                        DueDate     = taskModel.DueDate,
                        IsCompleted = taskModel.IsCompleted
                    })
                    .ToListAsync();

                return tasks;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error retrieving tasks for page {pageNumber} with pageSize {pageSize}.");
                throw new ApplicationException("An error occurred while retrieving tasks.", ex);
            }
        }

        public async Task<TaskModel> GetTaskByIdAsync(int id)
        {
            try
            {
                TaskModel task = await _dbContext.Tasks.FindAsync(id);
                if (task == null)
                {
                    _logger.Warn($"Task with ID {id} not found.");
                    throw new KeyNotFoundException($"Task with ID {id} not found.");
                }

                return task;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error retrieving task by ID = {id}.");
                throw new ApplicationException($"An error occurred while retrieving the task by ID = {id}.", ex);
            }
        }

        public async Task<TaskModel> CreateTaskAsync(TaskModel taskModel)
        {
            try
            {
                if (taskModel == null)
                {
                    _logger.Error("Attempted to create a null task.");
                    throw new ArgumentNullException(nameof(taskModel), "Task cannot be null.");
                }

                TaskModel createdTaskModel = _dbContext.Tasks.Add(taskModel);
                await _dbContext.SaveChangesAsync();

                _logger.Info($"Task created: {createdTaskModel.Id}");

                return createdTaskModel;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating task.");
                throw new ApplicationException("An error occurred while creating the task.", ex);
            }
        }

        public async Task<TaskModel> UpdateTaskAsync(TaskModel taskModel)
        {
            try
            {
                if (taskModel == null)
                {
                    _logger.Error("Attempted to update a null task.");
                    throw new ArgumentNullException(nameof(taskModel), "Task cannot be null.");
                }

                TaskModel existingTask = await _dbContext.Tasks.FindAsync(taskModel.Id);
                if (existingTask == null)
                {
                    _logger.Warn($"Task not found for update: {taskModel.Id}");
                    throw new KeyNotFoundException($"Task with ID {taskModel.Id} not found.");
                }

                _dbContext.Entry(existingTask).CurrentValues.SetValues(taskModel);
                await _dbContext.SaveChangesAsync();

                _logger.Info($"Task updated: {taskModel.Id}");

                return taskModel;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating task.");
                throw new ApplicationException("An error occurred while updating the task.", ex);
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                TaskModel task = await _dbContext.Tasks.FindAsync(id);
                if (task == null)
                {
                    _logger.Warn($"Task not found for deletion: {id}");
                    return false;
                }

                _dbContext.Tasks.Remove(task);
                await _dbContext.SaveChangesAsync();

                _logger.Info($"Task deleted: {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting task.");
                throw new ApplicationException("An error occurred while deleting the task.", ex);
            }
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}