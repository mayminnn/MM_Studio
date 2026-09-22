using System.Collections.Generic;

namespace Automation.Models
{
    public class FlowDefinition
    {
        public string FlowName { get; set; }

        public List<FlowTestDefinition> Tests { get; set; }

        public FlowDefinition()
        {
            Tests = new List<FlowTestDefinition>();
        }
    }
}