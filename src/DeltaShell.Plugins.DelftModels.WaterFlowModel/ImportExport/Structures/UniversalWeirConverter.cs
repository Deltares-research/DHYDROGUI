using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class UniversalWeirConverter : AStructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new FreeFormWeirFormula()
            };
        }

        protected override void SetStructureProperties(IStructure1D structure, IDelftIniCategory category)
        {
            var weir = structure as Weir;
            var weirFormula = weir.WeirFormula as FreeFormWeirFormula;

            weir.FlowDirection =
                (FlowDirection)category.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);
            var yValues = category.ReadPropertiesToListOfType<double>(StructureRegion.YValues.Key);
            var zValues = category.ReadPropertiesToListOfType<double>(StructureRegion.ZValues.Key);
            var crestLevel = category.ReadProperty<double>(StructureRegion.CrestLevel.Key);

            if (crestLevel - zValues.Min() > double.Epsilon)
            {
                throw new Exception(string.Format("For universal weir {0} the value for the crestlevel should be the same as the minimum value of the ZValues", weir.Name));
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
            var counterCheck = category.ReadProperty<int>(StructureRegion.LevelsCount.Key);

            if (counterCheck != weirFormula.Y.Count())
            {
                throw new Exception("There are more YZ coordinates given than mentioned in the levelsCount parameter");
            }

            weirFormula.DischargeCoefficient =
                category.ReadProperty<double>(StructureRegion.DischargeCoeff.Key);
        }
    }
}
    
