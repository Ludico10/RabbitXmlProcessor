using DataProcessorService.Entries;
using Microsoft.Extensions.Logging;
using Shared.Model;

namespace DataProcessorService.Core
{
    /// <summary>
    /// Handles saving instrument/device statuses to the database.
    /// </summary>
    public class StatusProcessor(string dbPath, ILoggerFactory loggerFactory)
    {
        private readonly ILogger<StatusProcessor> _logger = loggerFactory.CreateLogger<StatusProcessor>();

        private readonly string _dbPath = dbPath;

        /// <summary>
        /// Saves the provided <see cref="InstrumentStatus"/> to the database.
        /// Updates existing entries or creates new ones as necessary.
        /// </summary>
        /// <param name="status">The instrument status containing device/module information.</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        public async Task SaveToDbAsync(InstrumentStatus status, CancellationToken ct = default)
        {
            await using var db = new ServiceDbContext(_dbPath);

            foreach (var module in status.Devices)
            {

                var moduleStatus = await db.ModuleStatuses.FindAsync(module.ModuleCategoryID, ct);
                if (moduleStatus is not null)
                {
                    moduleStatus.State = module.ModuleState.ToString();
                    db.ModuleStatuses.Update(moduleStatus);
                    _logger.LogInformation($"{module.ModuleCategoryID} updated.");
                }
                else
                {
                    await db.ModuleStatuses.AddAsync(new ModuleStatus(module.ModuleCategoryID, module.ModuleState.ToString()), ct);
                    _logger.LogInformation($"{module.ModuleCategoryID} created.");
                }
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
