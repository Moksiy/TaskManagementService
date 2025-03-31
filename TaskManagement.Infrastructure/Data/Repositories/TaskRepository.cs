using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Infrastructure.Data.Context;
using TaskManagement.Infrastructure.Data.Repositories.Interfaces;

namespace TaskManagement.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Implementation of task repository
    /// </summary>
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskManagementDbContext _dbContext;
        private readonly ILogger<TaskRepository> _logger;

        /// <summary>
        /// Creates a new TaskRepository
        /// </summary>
        /// <param name="dbContext">Database context</param>
        /// <param name="logger">Logger</param>
        public TaskRepository(TaskManagementDbContext dbContext, ILogger<TaskRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Domain.Entities.Task>> GetAllAsync()
        {
            _logger.LogDebug("Getting all tasks from database");
            return await _dbContext.Tasks
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Domain.Entities.Task> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Getting task with ID: {TaskId} from database", id);
            return await _dbContext.Tasks.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task AddAsync(Domain.Entities.Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            _logger.LogDebug("Adding new task with ID: {TaskId} to database", task.Id);
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Domain.Entities.Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            _logger.LogDebug("Updating task with ID: {TaskId} in database", task.Id);
            _dbContext.Tasks.Update(task);
            await _dbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid id)
        {
            _logger.LogDebug("Deleting task with ID: {TaskId} from database", id);

            var task = await _dbContext.Tasks.FindAsync(id);
            if (task != null)
            {
                _dbContext.Tasks.Remove(task);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}