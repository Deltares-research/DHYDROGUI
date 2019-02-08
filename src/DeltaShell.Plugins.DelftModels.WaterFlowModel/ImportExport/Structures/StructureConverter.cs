using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public abstract class StructureConverter : IStructureConverter
    {
        protected IStructure1D Structure;
        protected static IDelftIniCategory Category;

        /// <summary>
        /// Converts an <see cref="IDelftIniCategory"/> object to a <see cref="IStructure1D"/> object.
        /// </summary>
        /// <param name="category">The data model.</param>
        /// <param name="branch">The branch that the structure should be put on.</param>
        /// <returns></returns>
        public IStructure1D ConvertToStructure1D(IDelftIniCategory category, IBranch branch)
        {
            Category = category;
            Structure = CreateNewStructure();
            Structure.SetCommonRegionElementsFromCategory(category, branch);
            SetStructurePropertiesFromCategory();

            return Structure;
        }

        protected static double[] TransformToDoubleArray(string valuesString)
        {
            return valuesString.Split(' ').Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToArray();
        }

        protected abstract IStructure1D CreateNewStructure();

        /// <summary>
        /// Sets the structure properties on <see cref="Structure"/> that are on the <see cref="Category"/> data model.
        /// </summary>
        protected abstract void SetStructurePropertiesFromCategory();
    }
}