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
    public class UniversalWeirConverter : StructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new FreeFormWeirFormula()
            };
        }

        protected override void SetStructurePropertiesFromCategory(IList<string> warningMessages)
        {
            if (!(Structure is IWeir weir)) return;
            if (!(weir.WeirFormula is FreeFormWeirFormula weirFormula)) return;

            weir.FlowDirection = (FlowDirection)Category.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);
            var yValues = Category.ReadPropertiesToListOfType<double>(StructureRegion.YValues.Key);
            var zValues = Category.ReadPropertiesToListOfType<double>(StructureRegion.ZValues.Key);
            var crestLevel = Category.ReadProperty<double>(StructureRegion.CrestLevel.Key);

            if (crestLevel - zValues.Min() > double.Epsilon)
            {
                throw new Exception($"For universal weir {weir.Name} the value for the crestlevel should be the same as the minimum value of the ZValues");
            }
            if (yValues.Count < zValues.Count)
            {
                throw new Exception("There are more values for the Z coordinate for universal weir");
            }

            if (yValues.Count > zValues.Count)
            {
                throw new Exception("There are more values for the Y coordinate for universal weir");
            }

            weirFormula.SetShape(yValues.ToArray(), zValues.ToArray());
            var counterCheck = Category.ReadProperty<int>(StructureRegion.LevelsCount.Key);

            if (counterCheck != weirFormula.Y.Count())
            {
                throw new Exception("There are more YZ coordinates given than mentioned in the levelsCount parameter");
            }

            weirFormula.DischargeCoefficient =
                Category.ReadProperty<double>(StructureRegion.DischargeCoeff.Key);
        }
    }
}
    
