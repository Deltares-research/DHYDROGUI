using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class UniversalWeirConverter : IStructureConverter
    {
        public IStructure1D ConvertToStructure1D(IDelftIniCategory structureBranchCategory, IList<IChannel> channelsList)
        {
            var weirFormula = new FreeFormWeirFormula();

            var weir = new Weir
            {
                WeirFormula = weirFormula
            };

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, channelsList, weir);

            weir.FlowDirection =
                (FlowDirection) structureBranchCategory.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);
            var yValues = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.YValues.Key);
            var zValues = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.ZValues.Key);
            var crestLevel = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestLevel.Key);

            if (crestLevel - zValues.Min()>Double.Epsilon)
            {
                throw new Exception(string.Format("For universal weir {0} the value for the crestlevel should be the same as the minimum value of the ZValues", weir.Name));
            }
            if (yValues.Count < zValues.Count)
            {
                throw new Exception("There are more values for the Z coordinate for universal weir");
            }
            else if (yValues.Count > zValues.Count)
            {
                throw new Exception("There are more values for the Y coordinate for universal weir");
            }

            weirFormula.SetShape(yValues.ToArray(), zValues.ToArray());
            var counterCheck = structureBranchCategory.ReadProperty<int>(StructureRegion.LevelsCount.Key);

            if (counterCheck != weirFormula.Y.Count())
            {
                throw new Exception("There are more YZ coordinates given than mentioned in the levelsCount parameter");
            }

            weirFormula.DischargeCoefficient =
                structureBranchCategory.ReadProperty<double>(StructureRegion.DischargeCoeff.Key);

            return weir;
        }
    }
}
    
