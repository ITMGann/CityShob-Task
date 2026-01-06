using Common.Models;
using Microsoft.AspNet.SignalR;
using NLog;
using Server.Data;
using Server.Hubs;
using Server.TaskLocker;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Server.Controllers
{
    [RoutePrefix("api/tasks")]
    public class TasksController : ApiController
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskLocker _taskLocker;
        private readonly IHubContext _taskLockHubContext;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public TasksController(ITaskRepository taskRepository, ITaskLocker taskLocker)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _taskLocker = taskLocker ?? throw new ArgumentNullException(nameof(taskLocker));

            _taskLockHubContext = GlobalHost.ConnectionManager.GetHubContext<TaskLockHub>();
        }

        // GET: api/tasks
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAllTasks([FromUri] int pageNumber = 1, [FromUri] int pageSize = 10)
        {
            try
            {
                List<TaskDto> tasks = await _taskRepository.GetAllTasksAsync(pageNumber, pageSize);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error retrieving tasks for page {pageNumber} with pageSize {pageSize}.");
                return InternalServerError(ex);
            }
        }

        // GET: api/tasks/{id}}
        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(TaskModel))]
        public async Task<IHttpActionResult> GetTaskById(int id)
        {
            try
            {
                TaskModel taskModel = await _taskRepository.GetTaskByIdAsync(id);
                if (taskModel == null)
                {
                    return NotFound();
                }
                return Ok(taskModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error retrieving task by ID {id}.");
                return InternalServerError(ex);
            }
        }

        // POST: api/tasks
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> CreateTask([FromBody] TaskModel taskModel)
        {
            if (taskModel == null)
            {
                return BadRequest("Task data is required.");
            }

            try
            {
                // createdTaskModel will include the automatically assigned ID.
                TaskModel createdTaskModel = await _taskRepository.CreateTaskAsync(taskModel);
                return CreatedAtRoute("DefaultApi", new { id = createdTaskModel.Id }, createdTaskModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error creating task.");
                return InternalServerError(ex);
            }
        }

        // PUT: api/tasks/{id}
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> UpdateTask(int id, [FromBody] TaskModel taskModel)
        {
            if (taskModel == null)
            {
                return BadRequest("Task data is required.");
            }

            if (taskModel.Id != id)
            {
                return BadRequest($"Task ID mismatch between the requested ID ({id}) and the ID in the given task data ({taskModel.Id}).");
            }

            try
            {
                // TODO : Check if task is locked by another user and if so, don't allow it (the task will necessarily be locked,
                //        but we need to verify by whom and only allow the user who started the edit to call update).
                //        In any case, if the users use our client, the client itself will prevent users who haven't started the
                //        editing from updating.
                // TODO : Actually, if we use applications such as Postman, we will receive this PUT HTTP method when the task
                //        hasn't necessarily been previously locked. We need to decide how we want to approach this scenario.
                //if (_taskLocker.IsLocked(id) && <Check for the locking user>)
                //{
                //    return Conflict();
                //}

                TaskModel updatedTaskModel = await _taskRepository.UpdateTaskAsync(taskModel);
                if (updatedTaskModel == null)
                {
                    return NotFound();
                }

                return Ok(updatedTaskModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error updating task with ID {id}.");
                return InternalServerError(ex);
            }
        }

        // DELETE: api/tasks/{id}
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteTask(int id)
        {
            try
            {
                if (_taskLocker.IsLocked(id))
                {
                    return Conflict();
                }

                bool isDeleted = await _taskRepository.DeleteTaskAsync(id);
                if (isDeleted == false)
                {
                    return NotFound();
                }

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error deleting task with ID {id}.");
                return InternalServerError(ex);
            }
        }
    }
}
