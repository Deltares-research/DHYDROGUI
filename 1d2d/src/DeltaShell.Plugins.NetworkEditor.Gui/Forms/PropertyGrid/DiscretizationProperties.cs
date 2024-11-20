using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "DiscretizationProperties_DisplayName")]
    public class DiscretizationProperties : ObjectProperties<Discretization>
    {
        [Category("General")]
        [Description("Name of the discretization")]
        [DynamicReadOnly]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Category("General")]
        [Description("Method used to generate segments from location")]
        public SegmentGenerationMethod SegmentMethod
        {
            get { return data.SegmentGenerationMethod; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool ValidateDynamicAttributes(string propertyName)
        {
            if (propertyName.Equals("Name"))
            {
                return !data.IsEditable;
            }

            return false;
        }
    }
}