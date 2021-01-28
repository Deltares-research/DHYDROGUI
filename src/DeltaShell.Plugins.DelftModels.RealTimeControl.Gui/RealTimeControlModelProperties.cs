using System.ComponentModel;
using System.Drawing.Design;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui
{
    [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_DisplayName")]
    public class RealTimeControlModelProperties : ObjectProperties<RealTimeControlModel>
    {
        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_Name_Description")]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_LimitMemory_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_LimitMemory_Description")]
        public bool LimitMemory
        {
            get { return data.LimitMemory; }
            set { data.LimitMemory = value; }
        }

        [PropertyOrder(3)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_CoordinateSystem_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_CoordinateSystem_Description")]
        [TypeConverter(typeof(CoordinateSystemStringTypeConverter))]
        [Editor(typeof(CoordinateSystemTypeEditor), typeof(UITypeEditor))]
        public ICoordinateSystem CoordinateSystem
        {
            get { return data.CoordinateSystem; }
            set { data.CoordinateSystem = value; }
        }
    }
}