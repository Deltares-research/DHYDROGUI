using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses
{
    public class RainfallRunoffBoundaryDataProperties : ObjectProperties<RainfallRunoffBoundaryData>
    {      
        [DisplayName("Is constant")]
        public bool IsConstant
        {
            get { return data.IsConstant; }
            set { data.IsConstant = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Water level constant")]
        public double Value
        {
            get { return data.Value; }
            set { data.Value = value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (propertyName == "Value")
            {
                return !IsConstant;
            }

            return false;
        }
    }
}