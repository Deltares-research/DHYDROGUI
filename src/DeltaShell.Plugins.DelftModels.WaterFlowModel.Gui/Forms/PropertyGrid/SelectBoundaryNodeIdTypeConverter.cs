using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    public class SelectSalinityEstuaryMouthNodeIdTypeConverter : TypeConverter
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var properties = (WaterFlowModel1DSalinityProperties) TypeUtils.GetField(context.Instance, "propertyObject");
            return new StandardValuesCollection(properties.GetSalinityEstuaryMouthNodeIds().ToArray());
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}