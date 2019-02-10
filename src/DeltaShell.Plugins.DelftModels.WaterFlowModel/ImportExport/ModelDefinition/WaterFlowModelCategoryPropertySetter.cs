using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelCategoryPropertySetter provides an interface to set property
    /// values described in the DelftIniCategory on the WaterFlowModel1D.
    /// </summary>
    /// <remarks>
    /// These interfaces are used within the ModelDefinitionFileReader to set the
    /// different kind of properties described in a .md1d file.
    /// </remarks>
    public abstract class WaterFlowModelCategoryPropertySetter
    {
        /// <summary>
        /// Sets the properties of the <paramref name="model"/> to the values described
        /// in <paramref name="category"/>.
        /// </summary>
        /// <param name="category">A category describing a region of the md1d file.</param>
        /// <param name="model">The model.</param>
        /// <param name="errorMessages"> A list of error messages to which new encountered error messages are added.</param>
        public abstract void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages);

        protected static string GetUnsupportedPropertyWarningMessage(IDelftIniProperty property)
        {
            return string.Format(Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                property.LineNumber, property.Name);
        }
    }
}