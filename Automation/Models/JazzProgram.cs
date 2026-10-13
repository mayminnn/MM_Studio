using System.Collections.Generic;

namespace Automation.Models
{
    public class JazzProgram
    {
        public string Name { get; set; } = "";
        public List<JazzStepDef> Steps { get; set; } = new List<JazzStepDef>();
    }
}