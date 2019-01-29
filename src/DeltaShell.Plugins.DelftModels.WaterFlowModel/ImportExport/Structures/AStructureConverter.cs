using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public abstract class AStructureConverter : IStructureConverter
    {
        /// <summary>
        /// Converts an <see cref="IDelftIniCategory"/> object to a <see cref="IStructure1D"/> object.
        /// </summary>
        /// <param name="category">The data model.</param>
        /// <param name="branch">The branch that the structure should be put on.</param>
        /// <returns></returns>
        public IStructure1D ConvertToStructure1D(IDelftIniCategory category, IBranch branch)
        {
            var structure = CreateNewStructure();
            BasicStructuresOperations.ReadCommonRegionElements(category, branch, structure);
            SetStructureProperties(structure, category);

            return structure;
        }

        protected abstract IStructure1D CreateNewStructure();

        /// <summary>
        /// Sets the structure properties that are on the <param name="category"></param> object.
        /// </summary>
        /// <param name="structure">The structure.</param>
        /// <param name="category">The data model for the structure.</param>
        protected abstract void SetStructureProperties(IStructure1D structure, IDelftIniCategory category);
    }

    public interface IStructureConverter
    {
        IStructure1D ConvertToStructure1D(IDelftIniCategory category, IBranch branch);
    }
}