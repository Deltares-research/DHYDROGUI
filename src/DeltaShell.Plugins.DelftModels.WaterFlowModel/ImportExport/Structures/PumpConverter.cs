using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class PumpConverter : IStructureConverter
    {
        public IStructure1D ConvertToStructure1D(IDelftIniCategory category, IBranch branch)
        {
            var pump = new Pump();
            BasicStructuresOperations.ReadCommonRegionElements(category, branch, pump);

            //pump.ControlDirection = (PumpControlDirection) category.ReadProperty<int>(StructureRegion.Direction.Key);

            return pump;
        }
    }
}
