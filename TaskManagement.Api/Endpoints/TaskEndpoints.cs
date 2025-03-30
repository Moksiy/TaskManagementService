using TaskManagement.Application.Services.Interfaces;
using TaskManagement.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;

namespace TaskManagement.Api.Endpoints
{
    public class TaskEndpoints
    {
        public void Configure(WebApplication app)
        {
            app.MapGet("~/api/tasks", GetAllTasks)
                .WithName("GetAllTasks")
                .WithOpenApi()
                .WithTags("Tasks");

            app.MapGet("~/api/tasks/{id}", GetTaskById)
                .WithName("GetTaskById")
                .WithOpenApi()
                .WithTags("Tasks");

            app.MapPost("~/api/tasks", CreateTask)
                .WithName("CreateTask")
                .WithOpenApi()
                .WithTags("Tasks");

            app.MapPut("~/api/tasks/{id}", UpdateTask)
                .WithName("UpdateTask")
                .WithOpenApi()
                .WithTags("Tasks");

            app.MapDelete("~/api/tasks/{id}", DeleteTask)
                .WithName("DeleteTask")
                .WithOpenApi()
                .WithTags("Tasks");
        }

        private async Task<IResult> GetAllTasks(
            HttpContext httpContext,
            [FromServices] ITaskService taskService)
        {
            var tasks = await taskService.GetAllTasksAsync();
            return Results.Ok(tasks);
        }

        private async Task<IResult> GetTaskById(
            HttpContext httpContext,
            Guid id,
            [FromServices] ITaskService taskService)
        {
            var task = await taskService.GetTaskByIdAsync(id);
            return task != null ? Results.Ok(task) : Results.NotFound();
        }

        private async Task<IResult> CreateTask(
            HttpContext httpContext,
            [FromBody] CreateTaskDto createTaskDto,
            [FromServices] ITaskService taskService)
        {
            var task = await taskService.CreateTaskAsync(createTaskDto);
            return Results.Created($"/api/tasks/{task.Id}", task);
        }

        private async Task<IResult> UpdateTask(
            HttpContext httpContext,
            Guid id,
            [FromBody] UpdateTaskDto updateTaskDto,
            [FromServices] ITaskService taskService)
        {
            var task = await taskService.UpdateTaskAsync(id, updateTaskDto);
            return task != null ? Results.Ok(task) : Results.NotFound();
        }

        private async Task<IResult> DeleteTask(
            HttpContext httpContext,
            Guid id,
            [FromServices] ITaskService taskService)
        {
            var result = await taskService.DeleteTaskAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        }
    }
}
