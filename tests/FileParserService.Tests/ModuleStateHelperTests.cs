using FileParserService.DataProcessing;
using Shared.Model;

namespace FileParserService.Tests
{
    public class ModuleStateHelperTests
    {
        /// <summary>
        /// GenerateModuleState always returns a valid enum value.
        /// </summary>
        [Fact]
        public void GenerateModuleStateTest()
        {
            var state = ModuleStateHelper.GenerateModuleState();

            Assert.Contains(state, Enum.GetValues<ModuleState>());
        }
    }
}
