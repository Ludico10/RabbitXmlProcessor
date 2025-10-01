using DataProcessorService.Core;
using DataProcessorService.Entries;
using Microsoft.Extensions.Logging;
using Shared.Model;

namespace DataProcessorService.Tests
{
    public class StatusProcessorTests
    {
        private const string dbName = "TestDb";
        /// <summary>
        /// Test that a new module status is added to the database.
        /// </summary>
        [Fact]
        public async Task SaveNewModuleTest()
        {
            await using var db = new ServiceDbContext(dbName);
            db.Database.EnsureDeleted();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var processor = new StatusProcessor(dbName, loggerFactory);

            var status = new InstrumentStatus
            {
                PackageID = "123",
                Devices =
                [
                    new() { ModuleCategoryID = "M1", ModuleState = ModuleState.Run }
                ]
            };

            await processor.SaveToDbAsync(status);

            var module = await db.ModuleStatuses.FindAsync("M1");
            Assert.NotNull(module);
            Assert.Equal("Run", module.State);
        }

        /// <summary>
        /// Test that existing module status is updated instead of duplicated.
        /// </summary>
        [Fact]
        public async Task UpdateModuleTest()
        {
            var db = new ServiceDbContext(dbName);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            await db.ModuleStatuses.AddAsync(new ModuleStatus("mod1", "NotReady"));
            await db.SaveChangesAsync();

            var processor = new StatusProcessor(dbName, loggerFactory);
            var status = new InstrumentStatus
            {
                PackageID = "pkg1",
                Devices = { new DeviceStatus { ModuleCategoryID = "mod1", ModuleState = ModuleState.Run } }
            };

            await processor.SaveToDbAsync(status);

            //new context needed, because old stores value in cache
            db = new ServiceDbContext(dbName);
            var module = await db.ModuleStatuses.FindAsync("mod1");
            Assert.NotNull(module);
            Assert.Equal("Run", module.State);
        }
    }
}
