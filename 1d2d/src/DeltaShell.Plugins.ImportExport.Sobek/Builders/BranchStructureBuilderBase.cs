using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek.Builders
{
    public abstract class BranchStructureBuilderBase<T> : IBranchStructureBuilder where T : BranchStructure
    {
        /// <summary>
        /// rt = possible flow direction (relative to the branch direction):
        /// 0 : flow in both directions
        /// 1 : flow from begin node to end node (positive)
        /// 2 : flow from end node to begin node (negative)
        /// 3 : no flow
        /// Converts a direction r1 (sobek) to deltashell flowdirection.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected FlowDirection GetFlowDirection(int direction)
        {
            switch (direction)
            {
                case 0:
                    return FlowDirection.Both;
                case 1:
                    return FlowDirection.Positive;
                case 2:
                    return FlowDirection.Negative;
                case 3:
                    return FlowDirection.None;
            }
            throw new NotImplementedException("Invalid direction");
        }

        public abstract IEnumerable<T> GetBranchStructures(SobekStructureDefinition structure);
        
        IEnumerable<BranchStructure> IBranchStructureBuilder.GetBranchStructures(SobekStructureDefinition structure)
        {
            return GetBranchStructures(structure).Cast<BranchStructure>();
        }
    }
}