using Automation.Models;

namespace Automation.Components
{
    public class FlowReader
    {
        private readonly Flow _flowPage;

        public FlowReader(Flow flowPage)
        {
            _flowPage = flowPage;
        }

        public FlowDefinition ReadFlow()
        {
            var result = new FlowDefinition();

            // Read spreadsheet here

            return result;
        }
    }
}