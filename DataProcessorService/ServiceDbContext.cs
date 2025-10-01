using DataProcessorService.Entries;
using Microsoft.EntityFrameworkCore;

namespace DataProcessorService
{
    /// <summary>
    /// Database context for the DataProcessorService.
    /// Provides access to ModuleStatuses table and ensures database creation.
    /// </summary>
    public class ServiceDbContext : DbContext
    {
        private readonly string _dbPath;

        /// <summary>
        /// Initializes a new instance of <see cref="ServiceDbContext"/> and ensures the database exists.
        /// </summary>
        /// <param name="dbPath">Path to the SQLite database file.</param>
        public ServiceDbContext(string dbPath)
        {
            _dbPath = dbPath;

            // Create the database if it does not exist
            Database.EnsureCreated();
        }

        /// <summary>
        /// Represents the ModuleStatuses table in the database.
        /// </summary>
        public DbSet<ModuleStatus> ModuleStatuses { get; set; } = null!;

        /// <summary>
        /// Configures the database provider and connection string.
        /// </summary>
        /// <param name="optionsBuilder">The options builder to configure the context.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbPath}");
        }
    }
}
