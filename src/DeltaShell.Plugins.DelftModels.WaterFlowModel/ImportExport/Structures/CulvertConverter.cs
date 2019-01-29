using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    /// <summary>
    /// This class is responsible for converting <see cref="IDelftIniCategory"/> objects into <see cref="Culvert"/> objects.
    /// </summary>
    /// <seealso cref="IStructureConverter" />
    public class CulvertConverter : IStructureConverter
    {
        /// <summary>
        /// Converts a <see cref="IDelftIniCategory"/> object into a <see cref="Pump"/> object.
        /// </summary>
        /// <param name="category">The data model for setting property values on the culvert.</param>
        /// <param name="branch">The branch on which the culvert should be added.</param>
        /// <returns>A <see cref="Culvert"/> object with properties set from <paramref name="category"/>.</returns>
        public IStructure1D ConvertToStructure1D(IDelftIniCategory category, IBranch branch)
        {
            var culvert = new Culvert();
            BasicStructuresOperations.ReadCommonRegionElements(category, branch, culvert);

            return culvert;
        }
    }
}