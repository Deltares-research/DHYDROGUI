using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    /// <summary>
    /// This class is responsible for converting <see cref="IDelftIniCategory"/> objects into <see cref="Weir"/> objects with
    /// a <see cref="PierWeirFormula"/> object as WeirFormula.
    /// </summary>
    /// <seealso cref="StructureConverter" />
    public class AdvancedWeirConverter : StructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new PierWeirFormula()
            };
        }

        protected override void SetStructureProperties()
        {
            if (!(Structure is IWeir weir)) return;
            if (!(weir.WeirFormula is PierWeirFormula weirFormula)) return;

            weir.CrestLevel = Category.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = Category.ReadProperty<double>(StructureRegion.CrestWidth.Key);

            weirFormula.NumberOfPiers = Category.ReadProperty<int>(StructureRegion.NPiers.Key);

            weirFormula.UpstreamFacePos = Category.ReadProperty<double>(StructureRegion.PosHeight.Key);
            weirFormula.DesignHeadPos = Category.ReadProperty<double>(StructureRegion.PosDesignHead.Key);
            weirFormula.PierContractionPos = Category.ReadProperty<double>(StructureRegion.PosPierContractCoef.Key);
            weirFormula.AbutmentContractionPos =
                Category.ReadProperty<double>(StructureRegion.PosAbutContractCoef.Key);

            weirFormula.UpstreamFaceNeg = Category.ReadProperty<double>(StructureRegion.NegHeight.Key);
            weirFormula.DesignHeadNeg = Category.ReadProperty<double>(StructureRegion.NegDesignHead.Key);
            weirFormula.PierContractionNeg = Category.ReadProperty<double>(StructureRegion.NegPierContractCoef.Key);
            weirFormula.AbutmentContractionNeg =
                Category.ReadProperty<double>(StructureRegion.NegAbutContractCoef.Key);
        }
    }
}