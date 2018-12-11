using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// IWaterFlowModelCategoryPropertySetter provides an interface to set property
    /// values described in the DelftIniCategory on the WaterFlowModel1D.
    /// </summary>
    /// <remarks>
    /// These interfaces are used within the ModelDefinitionFileReader to set the
    /// different kind of properties described in a .md1d file.
    /// </remarks>
    public interface IWaterFlowModelCategoryPropertySetter
    {
        /// <summary>
        /// Sets the properties of the <paramref name="model"/> to the values described
        /// in <paramref name="category"/>.
        /// </summary>
        /// <param name="category">A category describing a region of the md1d file.</param>
        /// <param name="model">The model.</param>
        /// <param name="errorMessages"> A list of error messages to which new encountered error messages are added.</param>
        void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages);
    }
}