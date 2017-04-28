using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "WindFunctionProperties_DisplayName")]
    public class WindFunctionProperties : ObjectProperties<WindFunction>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WindFunctionProperties));

        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WindFunctionProperties_Name_Description")]
        public string Name
        {
            get { return data.Name; }
        }

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WindFunctionProperties_InterpolationType_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WindFunctionProperties_InterpolationType_Description")]
        public InterpolationType InterpolationType
        {
            get { return data.Arguments[0].InterpolationType; }
            set
            {
                if (data.Arguments[0].AllowSetInterpolationType)
                {
                    data.Arguments[0].InterpolationType = value;
                }
                else
                {
                    Log.ErrorFormat("Unable to set interpolation-type for locations, it is not allowed.");
                }
            }
        }

        [PropertyOrder(3)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WindFunctionProperties_Extrapolation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WindFunctionProperties_Extrapolation_Description")]
        public ExtrapolationType ExtrapolationType
        {
            get { return data.Arguments[0].ExtrapolationType; }
        }
    }
}
