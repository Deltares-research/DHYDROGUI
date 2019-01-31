using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using System;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class OrificeConverter : StructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new GatedWeirFormula()
            };
        }

        protected override void SetStructureProperties()
        {
            var weir = Structure as Weir;
            var weirFormula = weir.WeirFormula as GatedWeirFormula;

            weir.CrestLevel = Category.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = Category.ReadProperty<double>(StructureRegion.CrestWidth.Key);
            weir.FlowDirection =
                (FlowDirection)Category.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);

            weirFormula.ContractionCoefficient =
                Category.ReadProperty<double>(StructureRegion.ContractionCoeff.Key);
            weirFormula.LateralContraction =
                Category.ReadProperty<double>(StructureRegion.LatContrCoeff.Key);
            weirFormula.GateOpening = Category.ReadProperty<double>(StructureRegion.OpenLevel.Key) -
                                      weir.CrestLevel;
            weirFormula.UseMaxFlowPos =
                Convert.ToBoolean(Category.ReadProperty<int>(StructureRegion.UseLimitFlowPos.Key));
            weirFormula.UseMaxFlowNeg =
                Convert.ToBoolean(Category.ReadProperty<int>(StructureRegion.UseLimitFlowNeg.Key));
            weirFormula.MaxFlowPos = Category.ReadProperty<double>(StructureRegion.LimitFlowPos.Key);
            weirFormula.MaxFlowNeg = Category.ReadProperty<double>(StructureRegion.LimitFlowNeg.Key);
        }
    }
}