using FileParserService.DataProcessing;
using Shared.Model;
using System.Xml.Serialization;

namespace FileParserService.Tests
{
    public class XmlParserTests
    {
        private const string TempFile = "test.xml";

        /// <summary>
        /// Returns correct ModuleState when XML contains a valid ModuleState node.
        /// </summary>
        [Fact]
        public void GetModuleStateTest()
        {
            var device = new DeviceStatus
            {
                RapidControlStatus = "<Device><ModuleState>Run</ModuleState></Device>"
            };

            var state = XmlParser.GetModuleState(device.RapidControlStatus);

            Assert.Equal(ModuleState.Run, state);
        }

        /// <summary>
        /// Returns null when XML is invalid or missing.
        /// </summary>
        [Fact]
        public void InvalidXmlTest()
        {
            var device = new DeviceStatus
            {
                RapidControlStatus = "<Device><SomeOtherNode>Value</SomeOtherNode></Device>"
            };

            var state = XmlParser.GetModuleState(device.RapidControlStatus);

            Assert.Null(state);
        }

        /// <summary>
        /// Ensures DeserializeAsync returns the correct object from valid XML.
        /// </summary>
        [Fact]
        public async Task DeserializeTest()
        {
            var obj = new TestClass { Id = 1, Name = "Test" };
            var serializer = new XmlSerializer(typeof(TestClass));
            await using (var stream = new FileStream(TempFile, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(stream, obj);
            }

            var result = await XmlParser.DeserializeAsync<TestClass>(TempFile);

            Assert.NotNull(result);
            Assert.Equal(obj.Id, result!.Id);
            Assert.Equal(obj.Name, result.Name);

            File.Delete(TempFile);
        }

        /// <summary>
        /// Returns null when XML content is invalid.
        /// </summary>
        [Fact]
        public async Task DeserializeInvalidTest()
        {
            await File.WriteAllTextAsync(TempFile, "<invalid<xml>");

            var result = await XmlParser.DeserializeAsync<TestClass>(TempFile);

            Assert.Null(result);

            File.Delete(TempFile);
        }

        /// <summary>
        /// Returns null when file does not exist.
        /// </summary>
        [Fact]
        public async Task FileNotFoundTest()
        {
            var result = await XmlParser.DeserializeAsync<TestClass>("nonexistent.xml");

            Assert.Null(result);
        }

        public class TestClass
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
