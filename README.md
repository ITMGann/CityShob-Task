# Task Management System

## Overview

This project is a task (ToDo item) management application that allows multiple clients to view and manage a shared list of tasks while preventing concurrent edits of the same task using a real-time locking mechanism.

---

## Core Features

- CRUD operations on tasks
- Paging support when displaying the tasks in a grid
- Real-time task locking to prevent concurrent edits
- Error handling and logging
- Extensible and testable architecture via abstraction and DI

---

## ⚠️ Important Note - The State of the Code

Unfortunately, the Client doesn't run. There's an exception somewhere not in my code. Working on it took a lot of the time I had, but to no avail.
Therefore, there are several features that I wanted to include for this version, but didn't have enough time or couldn't test and successfully add them. These are:
- **Client**:
  - I wanted to add a grid column that shows a lock icon for locked tasks. This isn't implemented and there's only some commented out XAML code for this column.<br />
  Note that the un/locking logic itself does exist. There's just no way to currently show it in the client (other than a MessageBox for debugging, which of course needs to be removed).
  - Improving the UI:
    - Adding better styling.
    - Adding a way to show error messages.

- **Server**:
  - I tested the server with Postman. All CRUD operations work. However, when creating a task using the HTTP POST method, it returns an error, but it does successfully create the task in the DB.
  - When a client connects, the server should send a list of all currently locked tasks.
  - When a client performs a CRUD operation, it isn't reflected in other clients.<br />
  I know it's part of the application's core, but I couldn't get to it and I had no way of testing it to make sure it works. However, all the technologies for implementing it are already being used (sending messages using SignalR, possibly have the client re-fetch the current page of tasks, etc.).

Note: There are also some theoretical improvements, listed at the end of this README, which were meant not to be included for this version, but were taken into account for scalability and general improvement.

---

## Architecture

### High-Level Overview

The system is composed of:
- **Server**: ASP.NET Web API (.NET Framework)
- **Client**: WPF application using MVVM (.NET Framework)
- **Common**: A class-library consisting of common models, DTOs, etc., used by both the server and the client

---

### Communication Protocols

- REST API is used for the task CRUD operations.
- SignalR is used for real-time communication, preventing the client from having to refresh or poll for information.

**SignalR is used for:** (Not all implemented because of the issues faced running the client)
- Client sending to the server when a user starts or cancels an edit (no need to notify the server when a user finishes an edit, because the server will get the relevant REST API HTTP method for updating), so that the server will know to lock and unlock that task.
- Server broadcasting lock state changes to all connected clients.
- Sending current lock state to newly connected clients.

**Why SignalR was chosen for notifying the server of starting or canceling an edit:**<br />
As opposed to the server broadcasting to the clients (which needs to be real-time so SignalR was the obvious choice between it and REST API), the client notifying the server of starting or canceling an edit could have been implemented using either SignalR or REST API.<br />
SignalrR was chosen, for these reasons:
- Lower latency so that tasks are locked for other clients faster, reducing the chance for the edge case where two clients try to start editing at (almost) the same time.
- Avoids the overhead of sending many HTTP requests (keeping scalability in mind) and instead, using a connection that stays open.

---

## Server Architecture

Several abstractions were used in order to keep the code flexibe, testable, and easy to extend. This allows swapping implemenations eaily needing to change only single files.

### Repository Pattern

- CRUD operations on the DB are abstracted using `ITaskRepository`.
- A concrete implementation `TaskRepositoryEf` uses Entity Framework.
- Enables future replacement or mocking.

### Task Locking Abstraction

- `ITaskLocker` abstraction
- `TaskLockerCache` implementation using caching (which in turn is also abstracted) storing the currently locked tasks.

### Caching Abstraction

- Implemented by an in-memory cache for a simpler implemenation considerting the scope and time frame.
- Can be replaced with Redis or distributed cache for scalability.

---

### Dependency Injection

- **Unity** is used for Dependency Injection
- Used in both client and server

---

### Using DTOs

Instead of passing a list of the full data model for all the tasks being sent, a DTO class was created TaskDto. This only holds the information that is presented in the grid. The entire data is only ever needed for a single task, so the full data, represented with the TaskModel class, is only sent as needed. This reduces network communication and latency times.

---

## UI/UX Brekdown and Workflow

- UI structure:
  - Left side: Task grid (paged)
  - Right side: Task details for the selected task, or for when adding a task. Another option would have been to show only the grid and open anoter window for viewing, editing, or adding a task. I wanted to avoid having to open a window for every task the user wants to view.
- Selecting a task:
  - Sends a REST call to fetch the full task details and populates the fields to the right.
- Editing a task:
  - Because we need to notify the server when an editing is taking place, the Edit button couldn't be used to send the edited task itself to the server, meaning after updating the values. Instead, pressing the Edit button begins the editing process and the client notifies the server, which in turns notifies the other clients.
    - All UI fields are disabled until the user clicks the **Edit** button.
    - Once in Edit mode:
      - The client sends the BeginEdit SignalR message for real time syncing.
      - The Save and Cancel buttons will be enabled.
      - The server will mark the task as locked (using in-memory cache in this implementation).
      - When canceling, the client sends the server the CancelEdit SignalR. The server then unlocks the task and broadcasts this.
      - When saving, the updating client needn't send any special unlocking message in addition to the REST API request to update the task (the server will know it's unlocked after a successful update and will notify the clients).
- Adding a task:
  - Because the fields are normally disabled, the adding button will also be pressed at the start of adding a task, rather than the end.
  - Once in Add mode, the Save and Cancel buttons can be used similarly to in Edit mode.
- Deleting a task:
  - Select a task and press the Delete button.



## Shared Project

A class-library project containing the data types (Model and DTO) and enums used by both the client and the server.

---

## Error Handling & Logging

- **NLog** is used to log errors, info messages, unexpected exceptions, etc.
- Log files are written to a configurable file target.

---

## Database

- **MS SQL Server** is used (hosted via **Docker** during development).
- Accessed with Entity Framework and using the Repository design pattern.
- Migrations used to create/update schema.

---

## Setup Instructions

- MS SQL Server is required to be installed, either locally or using Docker.
- After opening the solution in Visual Studio, restore the NuGet packages:
  - Visual Studio should automatically prompt to do so.
  - If it doesn't, the packages can be restored either via:
    - Tools > NuGet Package Manager > Restore Packages<br />
    or
    - the .NET CLI using: `nuget restore CityShob-Task.slnx`

---

### Database Setup

1. Have SQL Server running (e.g., start the Docker container)
2. Update the connection string value under the name "TaskDbConnection" in the Server's `Web.config` file, particularly the password in the connection string.
3. Run EF migrations or create the DB and table manually.

---

## Design Patterns Used

- **MVVM** – Client UI architecture
- **Repository** – Database access abstraction
- **Dependency Injection** – For loose coupling and testability
- **Publish / Subscribe** – SignalR-based real-time updates

---

## Scalability & Future Improvements

Here are ideas for possible improvements, which deliberately weren't implemented either because of the scope or the time constraint of the assignment:
- Replace in-memory lock cache with Redis for better scalability.
- For greater volumes of clients and communications, we can change the server to only push requests to RabbitMQ queues, and add a service that will consume and handle them. That will allow adding multiple instances of this service for horizontal scaling.
- Full DI coverage across all classes.
- Retry and reconnection logic for SignalR.
- Unit and integration tests.
- Better configuration, e.g., for the server's URL.
- Use the AutoMapper package for DTO <-> Model mapping.
- When fetching all the tasks from the REST API, add sending also the total number of tasks, in addition to the tasks for the requested page, so that the client will be able to calculate the number of pages.
- Add error handling and logging to more areas of the code.
- Add a PATCH HTTP method (or replace the PUT method) to allow updating only the fields that were updated, thus reducing the data being transferred. This will increase complexity as the client will need to keep track of which fields changed during the update.
- There are also some TODO comment throughout the code with some potential improvements.
