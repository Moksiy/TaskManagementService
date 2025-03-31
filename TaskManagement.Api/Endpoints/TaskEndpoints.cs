using TaskManagement.Application.Services.Interfaces;
using TaskManagement.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace TaskManagement.Api.Endpoints
{
    /// <summary>
    /// Defines API endpoints for task management
    /// </summary>
    public class TaskEndpoints
    {
        /// <summary>
        /// Configures API routes for task management
        /// </summary>
        /// <param name="app">Web application instance to configure routes</param>
        public void Configure(WebApplication app)
        {
            app.MapGet("/api/tasks", GetAllTasks)
                .WithName("GetAllTasks")
                .WithOpenApi()
                .WithTags("Tasks");

            app.MapGet("/api/tasks/{id}", GetTaskById)
                .WithName("GetTaskById")
                .WithOpenApi()
                .WithTags("Tasks");

            app.MapPost("/api/tasks", CreateTask)
                .WithName("CreateTask")
                .WithOpenApi()
                .WithTags("Tasks")
                .AddEndpointFilter<ValidationFilter<CreateTaskDto>>();

            app.MapPut("/api/tasks/{id}", UpdateTask)
                .WithName("UpdateTask")
                .WithOpenApi()
                .WithTags("Tasks")
                .AddEndpointFilter<ValidationFilter<UpdateTaskDto>>();

            app.MapDelete("/api/tasks/{id}", DeleteTask)
                .WithName("DeleteTask")
                .WithOpenApi()
                .WithTags("Tasks");
        }

        /// <summary>
        /// Retrieves a list of all tasks
        /// </summary>
        /// <param name="taskService">Task management service</param>
        /// <returns>List of all tasks or an error if the operation fails</returns>
        private async Task<IResult> GetAllTasks(
            [FromServices] ITaskService taskService)
        {
            var tasks = await taskService.GetAllTasksAsync();
            return Results.Ok(tasks);
        }

        /// <summary>
        /// Retrieves a task by its identifier
        /// </summary>
        /// <param name="id">Task identifier</param>
        /// <param name="taskService">Task management service</param>
        /// <returns>Task with the specified identifier or 404 if not found</returns>
        private async Task<IResult> GetTaskById(
            Guid id,
            [FromServices] ITaskService taskService)
        {
            var task = await taskService.GetTaskByIdAsync(id);
            return task != null ? Results.Ok(task) : Results.NotFound();
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        /// <param name="createTaskDto">Task creation data</param>
        /// <param name="taskService">Task management service</param>
        /// <returns>Created task with status code 201 or an error if the operation fails</returns>
        private async Task<IResult> CreateTask(
            [FromBody] CreateTaskDto createTaskDto,
            [FromServices] ITaskService taskService)
        {
            var task = await taskService.CreateTaskAsync(createTaskDto);
            return Results.Created($"/api/tasks/{task.Id}", task);
        }

        /// <summary>
        /// Updates an existing task
        /// </summary>
        /// <param name="id">Task identifier</param>
        /// <param name="updateTaskDto">Task update data</param>
        /// <param name="taskService">Task management service</param>
        /// <returns>Updated task or 404 if the task is not found</returns>
        private async Task<IResult> UpdateTask(
            Guid id,
            [FromBody] UpdateTaskDto updateTaskDto,
            [FromServices] ITaskService taskService)
        {
            var task = await taskService.UpdateTaskAsync(id, updateTaskDto);
            return task != null ? Results.Ok(task) : Results.NotFound();
        }

        /// <summary>
        /// Deletes a task by its identifier
        /// </summary>
        /// <param name="id">Task identifier</param>
        /// <param name="taskService">Task management service</param>
        /// <returns>Status code 204 (No Content) if successful or 404 if the task is not found</returns>
        private async Task<IResult> DeleteTask(
            Guid id,
            [FromServices] ITaskService taskService)
        {
            var result = await taskService.DeleteTaskAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        }
    }
}