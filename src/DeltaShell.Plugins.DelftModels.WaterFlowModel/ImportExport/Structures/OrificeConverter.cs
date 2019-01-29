using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using System;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class OrificeConverter : AStructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new GatedWeirFormula()
            };
        }

        protected override void SetStructureProperties(IStructure1D structure, IDelftIniCategory category)
        {
            var weir = structure as Weir;
            var weirFormula = weir.WeirFormula as GatedWeirFormula;

            weir.CrestLevel = category.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = category.ReadProperty<double>(StructureRegion.CrestWidth.Key);
            weir.FlowDirection =
                (FlowDirection)category.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);

            weirFormula.ContractionCoefficient =
                category.ReadProperty<double>(StructureRegion.ContractionCoeff.Key);
            weirFormula.LateralContraction =
                category.ReadProperty<double>(StructureRegion.LatContrCoeff.Key);
            weirFormula.GateOpening = category.ReadProperty<double>(StructureRegion.OpenLevel.Key) -
                                      weir.CrestLevel;
            weirFormula.UseMaxFlowPos =
                Convert.ToBoolean(category.ReadProperty<int>(StructureRegion.UseLimitFlowPos.Key));
            weirFormula.UseMaxFlowNeg =
                Convert.ToBoolean(category.ReadProperty<int>(StructureRegion.UseLimitFlowNeg.Key));
            weirFormula.MaxFlowPos = category.ReadProperty<double>(StructureRegion.LimitFlowPos.Key);
            weirFormula.MaxFlowNeg = category.ReadProperty<double>(StructureRegion.LimitFlowNeg.Key);
        }
    }
}