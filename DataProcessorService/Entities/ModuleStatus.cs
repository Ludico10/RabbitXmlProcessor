using System.ComponentModel.DataAnnotations;

namespace DataProcessorService.Entries
{
    public class ModuleStatus
    {
        [Key]
        public string ModuleCategoryID { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public ModuleStatus() { }

        public ModuleStatus(string moduleCategoryID, string state)
        {
            ModuleCategoryID = moduleCategoryID;
            State = state;
        }
    }
}
