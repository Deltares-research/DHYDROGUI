using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    /// <summary>
    /// This class is responsible for converting <see cref="IDelftIniCategory" /> objects into <see cref="Culvert" /> objects.
    /// </summary>
    /// <seealso cref="AStructureConverter" />
    public class CulvertConverter : AStructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Culvert();
        }

        protected override void SetStructureProperties(IStructure1D structure, IDelftIniCategory category)
        {
            var culvert = structure as Culvert;

            culvert.FlowDirection = (FlowDirection) category.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);
            culvert.InletLevel = category.ReadProperty<double>(StructureRegion.LeftLevel.Key);
            culvert.OutletLevel = category.ReadProperty<double>(StructureRegion.RightLevel.Key);

            culvert.Length = category.ReadProperty<double>(StructureRegion.Length.Key);
            culvert.InletLossCoefficient = category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key);
            culvert.OutletLossCoefficient = category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key);
            culvert.GateInitialOpening = category.ReadProperty<double>(StructureRegion.IniValveOpen.Key);

            culvert.IsGated = Convert.ToBoolean(category.ReadProperty<int>(StructureRegion.ValveOnOff.Key));
        }
    }
}