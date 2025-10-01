using Microsoft.Extensions.Logging;
using Shared.Model;
using System.Xml;

namespace FileParserService.DataProcessing
{
    /// <summary>
    /// Provides helper methods for working with <see cref="ModuleState"/>.
    /// </summary>
    public class ModuleStateHelper(ILoggerFactory loggerFactory)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<ModuleStateHelper>();

        /// <summary>
        /// Assigns a generated module state to each device in the instrument.
        /// </summary>
        /// <param name="status">The instrument status whose device states will be enriched.</param>
        public void Enrich(InstrumentStatus status)
        {
            foreach (DeviceStatus device in status.Devices)
            {
                if (device.RapidControlStatus is null)
                    return;

                var state = XmlParser.GetModuleState(device.RapidControlStatus);
                //Randomize
                device.ModuleState = GenerateModuleState();
                _logger.LogDebug($"Module state changed to {device.ModuleState}.");
            }
        }

        /// <summary>
        /// Generates a random <see cref="ModuleState"/>.
        /// </summary>
        /// <returns>A randomly selected <see cref="ModuleState"/> value.</returns>
        public static ModuleState GenerateModuleState()
        {
            var random = new Random();
            var values = Enum.GetValues<ModuleState>();
            return values[random.Next(values.Length)];
        }
    }
}
