using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
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

        protected override void SetStructureProperties(IStructure1D culvert, IDelftIniCategory category)
        {

        }
    }
}