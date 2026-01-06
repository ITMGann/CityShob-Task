using Client.Commands;
using Client.Crud;
using Client.RealTime;
using Common;
using Common.Models;
using Microsoft.AspNet.SignalR.Client;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private const int PageSize = 10;

        private readonly ITaskCrudService _taskCrudService;
        private readonly ITaskLockService _taskLockService;
        private readonly SignalRConnection _signalRConnection;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private List<TaskDto> _taskDtos;
        private TaskDto _selectedTaskDto;
        private string _errorMessage;

        private int _pageMax = 0;

        private bool _isConnectToServerButtonEnabled;
        private bool _isAddButtonEnabled;
        private bool _isUpdateButtonEnabled;
        private bool _isDeleteButtonEnabled;
        private bool _isSaveButtonEnabled;
        private bool _isCancelButtonEnabled;

        public ICommand ConnectToServerCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand BeginEditCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }

        public MainWindowViewModel(ITaskCrudService taskCrudService, ITaskLockService taskLockService, SignalRConnection signalRConnection)
        {
            _taskCrudService = taskCrudService;
            _taskLockService = taskLockService;
            _signalRConnection = signalRConnection;

            _signalRConnection.HubConnectionStateChanged += SignalRConnection_HubConnectionStateChanged;

            _signalRConnection.TaskLocked += SignalRConnection_TaskLocked;
            _signalRConnection.TaskUnlocked += SignalRConnection_TaskUnlocked;

            ConnectToServerCommand = new RelayCommand(OnConnectToServer);
            AddTaskCommand = new RelayCommand(AddTask, CanAddTask);
            BeginEditCommand = new RelayCommand(BeginEdit, CanBeginEdit);
            DeleteTaskCommand = new RelayCommand(DeleteTask, CanDeleteTask);
            SaveTaskCommand = new RelayCommand(SaveTask, CanSaveTask);
            CancelEditCommand = new RelayCommand(CancelEdit, CanCancelEdit);
            PreviousPageCommand = new RelayCommand(OnPreviousPage, CanFetchPreviousPage);
            // TODO : Add checking whether the next page should be disabled. For that, we need to add sending from
            //        the server to the client the total number of tasks as well whenver sending a page (we can't send
            //        it only when connecting because the number will change as the clients add and remove tasks),
            //        and then the client can calculate how many pages in total there are and disable the Next button
            //        when on the last one.
            NextPageCommand = new RelayCommand(OnNextPage, CanFetchNextPage);

            // TODO : Setting IsConnectToServerButtonEnabled to true assumes the connection to the server is always in
            //        disconnected state here. We could refactor setting the connect button's IsEnabled into a method
            //        which gets a connection state and use it here and in SignalRConnection_HubConnectionStateChanged(...).
            IsConnectToServerButtonEnabled = true;
            IsAddButtonEnabled = true;
            IsUpdateButtonEnabled = false;
            IsDeleteButtonEnabled = false;
            IsSaveButtonEnabled = false;
            IsCancelButtonEnabled = false;
        }

        private void SignalRConnection_TaskLocked(int taskId)
        {
            // TODO : Of course this is for debugging only. Add a way to visually show that a task is locked.
            MessageBox.Show($"Task with ID {taskId} is now locked.");
        }

        private void SignalRConnection_TaskUnlocked(int taskId)
        {
            // TODO : Of course this is for debugging only. Remove the visual cue which signified that this task was locked.
            MessageBox.Show($"Task with ID {taskId} is now unlocked.");
        }

        private async void SignalRConnection_HubConnectionStateChanged(StateChange stateChange)
        {
            ConnectionState connectionStateNew = stateChange.NewState;
            IsConnectToServerButtonEnabled = connectionStateNew == ConnectionState.Disconnected;

            if (connectionStateNew == ConnectionState.Connected)
            {
                await LoadTasks(pageNumber: 1);
            }
        }

        public List<TaskDto> TaskDtos
        {
            get => _taskDtos;
            set
            {
                _taskDtos = value;
                OnPropertyChanged();
            }
        }

        private TaskModel _selectedTaskModel = null;
        public TaskModel SelectedTaskModel
        {
            get => _selectedTaskModel;
            set
            {
                _selectedTaskModel = value;
                OnPropertyChanged();
            }
        }

        public TaskDto SelectedTaskDto
        {
            get => _selectedTaskDto;
            set
            {
                if (_selectedTaskDto == value)
                {
                    return;
                }

                _selectedTaskDto = value;
                OnPropertyChanged();

                bool isTaskSelected = _selectedTaskDto != null;
                IsUpdateButtonEnabled = isTaskSelected;
                IsDeleteButtonEnabled = isTaskSelected;
                if (isTaskSelected)
                {
                    Task.Run(async () => 
                    {
                        _selectedTaskModel = await FetchTaskDetails(_selectedTaskDto);
                        PopulateTaskFields(_selectedTaskModel);
                    });
                }
                else
                {
                    _selectedTaskModel = null;
                    PopulateTaskFields(null);
                }
            }
        }

        // TODO : For future handling of and showing error messages.
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        // These properties are used to populate the task fields on the right of the grid, as well as fetching their values for adding or updating a task.
        // TODO : I wanted to test binding all the UI widgets to the fields of SelectedTaskModel.
        //        However, I wasn't sure exactly on what I'd need to call OnPropertyChanged and how to make the UI
        //        update automatically correctly. Using separate properties for each value was used as a safe fallback.

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _dueDate;
        public DateTime? DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                OnPropertyChanged();
            }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                OnPropertyChanged();
            }
        }

        private TaskPriorityEnum _priority;
        public TaskPriorityEnum Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged();
            }
        }

        private DateTime _creationDate;
        public DateTime CreationDate
        {
            get => _creationDate;
            set
            {
                _creationDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _lastUpdateDate;
        public DateTime LastUpdateDate
        {
            get => _lastUpdateDate;
            set
            {
                _lastUpdateDate = value;
                OnPropertyChanged();
            }
        }

        public TaskPriorityEnum[] Priorities => (TaskPriorityEnum[])Enum.GetValues(typeof(TaskPriorityEnum));

        private int _pageCurrent;

        public int PageCurrent
        {
            get => _pageCurrent;
            set
            {
                _pageCurrent = value;
                OnPropertyChanged();
            }
        }

        public bool IsAddMode  { get; set; }
        public bool IsEditMode { get; set; }

        // TODO : Here Editing refers to being able to change field values, i.e., being in either Add or Edit mode.
        //        To aviod confusion, rename EditMode to UpdateMode and all other relevant "Edit" -> "Update"
        private bool isEditingFields;
        public bool IsEditingFields
        {
            get => isEditingFields;
            set
            {
                isEditingFields = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnectToServerButtonEnabled
        {
            get => _isConnectToServerButtonEnabled;
            set
            {
                _isConnectToServerButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        // TODO : I started with updating button states explicitly, but it would be good to revisit which properties are needed
        //        explicitly and which button states can rely on other properties. For example, instead of managing separately
        //        the state for the Save and Cancel buttons in designated properties, they can rely on IsEditingFields, and so on.
        public bool IsAddButtonEnabled
        {
            get => _isAddButtonEnabled;
            set
            {
                _isAddButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsUpdateButtonEnabled
        {
            get => _isUpdateButtonEnabled;
            set
            {
                _isUpdateButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsDeleteButtonEnabled
        {
            get => _isDeleteButtonEnabled;
            set
            {
                _isDeleteButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsSaveButtonEnabled
        {
            get => _isSaveButtonEnabled;
            set
            {
                _isSaveButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsCancelButtonEnabled
        {
            get => _isCancelButtonEnabled;
            set
            {
                _isCancelButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        private async void OnConnectToServer()
        {
            await _signalRConnection.StartConnectionAsync();
        }

        private void AddTask()
        {
            //IsEditMode = false;
            IsAddMode = true;

            IsSaveButtonEnabled = true;
            IsCancelButtonEnabled = true;

            _logger.Info("Started adding a task.");
        }

        private bool CanAddTask()
        {
            // TODO : All the fields should also be disabled when not connected to the server. This needs to be properly added in a global manner.
            return CanSaveTask() == false;
        }

        private async Task LoadTasks(int pageNumber, int pageSize = PageSize)
        {
            try
            {
                // Fetch task list (paginated)
                List<TaskDto> taskDtos = await _taskCrudService.GetTaskDtosAsync(pageNumber, pageSize);
                TaskDtos = taskDtos;

                PageCurrent = pageNumber;

                // TODO : Add having the server send also the total number of tasks in addition to the requested page, so that we'll be able to calculate
                //        the real maximum page number. This number will also need to udpate is we add or delete tasks.
                _pageMax = 5;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading tasks.");
                _errorMessage = "An error occurred while loading tasks. Please try again later.";
            }
        }

        private async Task<TaskModel> FetchTaskDetails(TaskDto taskDto)
        {
            if (taskDto == null)
            {
                return null;
            }

            try
            {
                TaskModel selectedTaskModel = await _taskCrudService.GetTaskByIdAsync(taskDto.Id);

                return selectedTaskModel;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error loading task details width ID = {SelectedTaskDto.Id}.");
                _errorMessage = "An error occurred while loading the task details.";

                return null;
            }
        }

        private void PopulateTaskFields(TaskModel selectedTaskModel)
        {
            if (selectedTaskModel == null)
            {
                ClearTaskFields();

                return;
            }

            Title          = selectedTaskModel.Title;
            Description    = selectedTaskModel.Description;
            DueDate        = selectedTaskModel.DueDate;
            Priority       = selectedTaskModel.Priority;
            IsCompleted    = selectedTaskModel.IsCompleted;
            CreationDate   = selectedTaskModel.CreationDate;
            LastUpdateDate = selectedTaskModel.LastUpdateDate ?? selectedTaskModel.CreationDate;
        }

        private TaskModel CreateTaskModelFromFields(int? taskId = null)
        {
            TaskModel taskModel = new TaskModel
            {
                Title = Title,
                Description = Description,
                DueDate = DueDate,
                IsCompleted = IsCompleted,
                Priority = Priority,
                CreationDate = CreationDate,
                LastUpdateDate = LastUpdateDate
            };

            if (taskId.HasValue)
            {
                taskModel.Id = taskId.Value;
            }

            return taskModel;
        }

        private async void SaveTask()
        {
            if (IsAddMode)
            {
                // Add new task
                TaskModel taskModelNew = CreateTaskModelFromFields();

                try
                {
                    TaskModel createdTask = await _taskCrudService.CreateTaskAsync(taskModelNew);

                    // TODO : When there's a change to the tasks (one is added, updated, or deleted), the page that the user is currently
                    //        viewing may be out of sync. For example, the newly created task should be added to the current page or the
                    //        deleted one should be removed.
                    //        If I could have finish implementing everything, the server would have broadcasted to all clients that
                    //        a such a change happened and then each client would have refetch the tasks for its current page.
                    //        Here, I fall back to explicitly fetch the tasks now, after the change.
                    await LoadTasks(PageCurrent);

                    _logger.Info($"Task created successfully: {createdTask.Title}");
                    CancelEdit();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error saving and creating the new task.");
                    _errorMessage = "An error occurred while saving and creating the task. Please try again later.";
                }
            }
            else if (IsEditMode)
            {
                TaskModel taskModelUpdated = CreateTaskModelFromFields(SelectedTaskDto.Id);

                try
                {
                    TaskModel updatedTask = await _taskCrudService.UpdateTaskAsync(taskModelUpdated);

                    // TODO : If I could have implemented the server broadcasting that a change was made, the client would have
                    //        refetched the tasks on the current page. Here, it's enough to only update the relevant row in the
                    //        grid, instead of refetching the entire page.
                    SelectedTaskDto.Title = updatedTask.Title;
                    SelectedTaskDto.DueDate = updatedTask.DueDate;
                    SelectedTaskDto.IsCompleted = updatedTask.IsCompleted;
                    SelectedTaskDto.Priority = updatedTask.Priority;

                    _logger.Info($"Task updated successfully: {updatedTask.Title}");
                    CancelEdit();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error updating task.");
                    _errorMessage = "An error occurred while updating the task. Please try again later.";
                }
            }
        }

        private bool CanSaveTask()
        {
            // TODO : Add check whether all mandatory fields have a valid value.
            //return string.IsNullOrEmpty(Title) == false;

            return IsAddMode || IsEditMode;
        }

        private void CancelEdit()
        {
            IsAddMode = false;
            IsEditMode = false;

            PopulateTaskFields(_selectedTaskModel);

            // Disable the Save and Cancel buttons as we're no longer in the middle of Adding or Editing a task.
            IsSaveButtonEnabled = false;
            IsCancelButtonEnabled = false;
        }

        private bool CanCancelEdit()
        {
            // The Save and Cancel buttons will always be Enabled or Diabled together so avoid duplicating the logic to check is enabled.
            return CanSaveTask();
        }

        private void ClearTaskFields()
        {
            Title = string.Empty;
            Description = string.Empty;
            DueDate = null;
            Priority = TaskPriorityEnum.None;
            IsCompleted = false;
            // Setting to Now only because we can't make the value not show.
            CreationDate = DateTime.Now;
            LastUpdateDate = DateTime.Now;
        }

        private async void BeginEdit()
        {
            if (SelectedTaskDto == null)
            {
                _logger.Info($"Can't start editing because no task is selected.");
                return;
            }

            try
            {
                await _taskLockService.BeginUpdateAsync(SelectedTaskDto.Id);

                // TODO : Add getting an answer from the server whether an update was approved.

                IsEditMode = true;
                //IsAddMode = false;

                // Enable the Save and Cancel buttons as we're entering Edit mode.
                IsSaveButtonEnabled = true;
                IsCancelButtonEnabled = true;

                _logger.Info($"Task {SelectedTaskDto.Title} locked for editing.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error beginning task edit.");
                _errorMessage = "An error occurred while locking the task for editing.";
            }
        }

        private bool CanBeginEdit()
        {
            return SelectedTaskDto != null;
        }

        private async void DeleteTask()
        {
            if (SelectedTaskDto == null)
            {
                _logger.Info("There's no selected task to delete.");
                return;
            }

            try
            {
                await _taskCrudService.DeleteTaskAsync(SelectedTaskDto.Id);

                // TODO : When there's a change to the tasks (one is added, updated, or deleted), the page that the user is currently
                //        viewing may be out of sync. For example, the newly created task should be added to the current page or the
                //        deleted one should be removed.
                //        If I could have finish implementing everything, the server would have broadcasted to all clients that
                //        a such a change happened and then each client would have refetch the tasks for its current page.
                //        Here, I fall back to explicitly fetch the tasks now, after the change.
                await LoadTasks(PageCurrent);

                _logger.Info($"Task \"{SelectedTaskDto.Title}\" (ID = {SelectedTaskDto.Id}) deleted successfully.");
                CancelEdit();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error deleting the task \"{SelectedTaskDto.Title}\" with ID = {SelectedTaskDto.Id}.");
                _errorMessage = "An unexpected error occurred while deleting the task.";
            }
        }

        private bool CanDeleteTask()
        {
            return SelectedTaskDto != null && CanSaveTask() == false;
        }

        private async void OnPreviousPage()
        {
            await LoadTasks(PageCurrent - 1);
        }

        private bool CanFetchPreviousPage()
        {
            return PageCurrent > 1;
        }

        private async void OnNextPage()
        {
            await LoadTasks(PageCurrent + 1);
        }
        private bool CanFetchNextPage()
        {
            return PageCurrent < _pageMax;
        }
    }
}
