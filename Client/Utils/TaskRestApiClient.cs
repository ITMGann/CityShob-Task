using Common.Models;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Client.Utils
{
    public class TaskRestApiClient
    {
        private const string PathApiTasks = "api/tasks";
        private const string MediaTypeJson = "application/json";

        private readonly HttpClient _httpClient;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public TaskRestApiClient(string baseUrl)
        {
            //UriBuilder uriBuilder = new UriBuilder(baseAddress);
            //uriBuilder.Path = PathApiTasks;
            //_httpClient = new HttpClient { BaseAddress = uriBuilder.Uri };
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeJson));
        }

        public async Task<List<TaskDto>> GetTaskDtosAsync(int pageNumber, int pageSize)
        {
            try
            {
                string url = $"{PathApiTasks}?pageNumber={pageNumber}&pageSize={pageSize}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode == false)
                {
                    _logger.Error($"Error fetching tasks from {url}. Status code: {response.StatusCode}");
                    throw new Exception("Failed to fetch tasks.");
                }

                string content = await response.Content.ReadAsStringAsync();
                List<TaskDto> taskDtos = JsonConvert.DeserializeObject<List<TaskDto>>(content);

                return taskDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while fetching tasks.");
                throw;
            }
        }

        public async Task<TaskModel> GetTaskByIdAsync(int id)
        {
            try
            {
                string url = $"{PathApiTasks}/{id}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error($"Error fetching task by ID = {id}. Status code: {response.StatusCode}");
                    throw new Exception("Failed to fetch task.");
                }

                string content = await response.Content.ReadAsStringAsync();
                TaskModel taskModel = JsonConvert.DeserializeObject<TaskModel>(content);

                return taskModel;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while fetching task by ID.");
                throw;
            }
        }

        public async Task<TaskModel> CreateTaskAsync(TaskModel taskModel)
        {
            try
            {
                string url = $"{PathApiTasks}";
                string content = JsonConvert.SerializeObject(taskModel);
                HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error($"Error creating task. Status code: {response.StatusCode}");
                    throw new Exception("Failed to create task.");
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                TaskModel createdTask = JsonConvert.DeserializeObject<TaskModel>(responseContent);

                return createdTask;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while creating task.");
                throw;
            }
        }

        public async Task<TaskModel> UpdateTaskAsync(TaskModel taskModel)
        {
            try
            {
                string url = $"{PathApiTasks}/{taskModel.Id}";
                string content = JsonConvert.SerializeObject(taskModel);
                HttpContent httpContent = new StringContent(content, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PutAsync(url, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error($"Error updating task with ID = {taskModel.Id}. Status code: {response.StatusCode}");
                    throw new Exception("Failed to update task.");
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                TaskModel updatedTask = JsonConvert.DeserializeObject<TaskModel>(responseContent);

                return updatedTask;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while updating task.");
                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                string url = $"{PathApiTasks}/{id}";
                HttpResponseMessage response = await _httpClient.DeleteAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error($"Error deleting task with ID = {id}. Status code: {response.StatusCode}");
                    throw new Exception("Failed to delete task.");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while deleting task.");
                throw;
            }
        }
    }
}
