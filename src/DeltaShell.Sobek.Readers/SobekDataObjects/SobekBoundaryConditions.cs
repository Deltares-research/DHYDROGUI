using System.Collections.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekBoundaryConditions
    {
        private readonly IList<SobekFlowBoundaryCondition> flowConditions;

        public SobekBoundaryConditions()
        {
            flowConditions = new List<SobekFlowBoundaryCondition>();
        }

        public IList<SobekFlowBoundaryCondition> FlowConditions
        {
            get { return flowConditions; }
        }
    }
}