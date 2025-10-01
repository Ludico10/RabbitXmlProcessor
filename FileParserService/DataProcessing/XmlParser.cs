using Shared.Model;
using System.Xml;
using System.Xml.Serialization;

namespace FileParserService.DataProcessing
{
    /// <summary>
    /// Provides XML deserialization utilities for reading objects from files.
    /// </summary>
    public static class XmlParser
    {
        /// <summary>
        /// Asynchronously deserializes an XML file into an object of type T/>.
        /// Returns null if the file cannot be deserialized.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="path">The path to the XML file.</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>An instance of type T if deserialization succeeds; otherwise, null.</returns>
        public static async Task<T?> DeserializeAsync<T>(string path, CancellationToken ct = default) where T : class
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                await using var stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true);

                return serializer.Deserialize(stream) as T;
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Determines the module state from a <see cref="DeviceStatus"/> object.
        /// </summary>
        /// <param name="rapidControlStatus">XML is present, attempts to parse the ModuleState value.</param>
        /// <returns>The determined or randomly generated <see cref="ModuleState"/>.</returns>
        public static ModuleState? GetModuleState(string rapidControlStatus)
        {
            var innerDoc = new XmlDocument();
            innerDoc.LoadXml(rapidControlStatus);
            var node = innerDoc.SelectSingleNode("//ModuleState");
            if (Enum.TryParse(node?.InnerText, ignoreCase: true, out ModuleState state))
            {
                return state;
            }

            return null;
        }
    }
}
