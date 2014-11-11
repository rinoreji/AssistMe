
using AssistMe.Data;
namespace AssistMe.Models
{
    public class SidebarNode
    {
        public string iconClass { get; set; }
        public AFileInfo AFileInfo { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public SidebarNode()
        {
            iconClass = "fa-dashboard";
        }
    }
}