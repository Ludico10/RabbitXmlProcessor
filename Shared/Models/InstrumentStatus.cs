using System.Xml.Serialization;

namespace Shared.Model
{
    [XmlRoot("InstrumentStatus")]
    public class InstrumentStatus
    {
        [XmlElement("PackageID")]
        public string? PackageID { get; set; }

        [XmlElement("DeviceStatus")]
        public List<DeviceStatus> Devices { get; set; } = [];
    }

}
