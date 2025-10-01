using System.Xml;
using System.Xml.Serialization;

namespace Shared.Model
{
    //all possible values for modules state
    public enum ModuleState
    {
        Online,
        Run,
        NotReady,
        Offline
    }

    public class DeviceStatus
    {
        [XmlElement("ModuleCategoryID")]
        public string ModuleCategoryID { get; set; } = string.Empty;

        [XmlElement("IndexWithinRole")]
        public int IndexWithinRole { get; set; }

        [XmlElement("RapidControlStatus")]
        public string? RapidControlStatus { get; set; }

        public ModuleState ModuleState { get; set; }
    }
}
