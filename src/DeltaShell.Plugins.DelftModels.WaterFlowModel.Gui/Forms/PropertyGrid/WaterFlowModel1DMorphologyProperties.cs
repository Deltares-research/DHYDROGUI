using System.ComponentModel;
using System.Drawing.Design;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Morphology properties")]
    [Description("Morphology properties")]
    public class WaterFlowModel1DMorphologyProperties
    {
        private WaterFlowModel1D data;
        public WaterFlowModel1DMorphologyProperties(WaterFlowModel1D data)
        {
            this.data = data;
        }

        [PropertyOrder(1)]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_UseMorphology_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_UseMorphology_Description")]
        public bool UseMorphology
        {
            get { return data.UseMorphology; }
            set { data.UseMorphology = value; }
        }

        [PropertyOrder(2)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_AdditionalOutput_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_AdditionalOutput_Description")]
        public bool AdditionalOutput
        {
            get { return data.AdditionalMorphologyOutput; }
            set { data.AdditionalMorphologyOutput = value; }
        }

        [PropertyOrder(3)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_MorphologyPath_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_MorphologyPath_Description")]
        [Editor(typeof(PathEditor), typeof(UITypeEditor))]
        public string MorphologyFile
        {
            get { return data.MorphologyPath; }
            set
            {
                if(value != null) data.MorphologyPath = value;
            }
        }

        [PropertyOrder(4)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_BcmPath_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_BcmPath_Description")]
        [Editor(typeof(PathEditor), typeof(UITypeEditor))]
        public string BcmFile
        {
            get { return data.BcmPath; }
            set
            {
                if (value != null) data.BcmPath = value;
            }
        }

        [PropertyOrder(5)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_SedimentPath_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_SedimentPath_Description")]
        [Editor(typeof(PathEditor), typeof(UITypeEditor))]
        public string SedimentFile
        {
            get { return data.SedimentPath; }
            set { if (value != null) data.SedimentPath = value; }
        }

        [PropertyOrder(6)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_TraPath_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_TraPath_Description")]
        [Editor(typeof(PathEditor), typeof(UITypeEditor))]
        public string TraFile
        {
            get { return data.TraPath; }
            set { if (value != null) data.TraPath = value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool ValidateDynamicAttributes(string propertyName)
        {
            if (propertyName.Equals("MorphologyFile") ||
                propertyName.Equals("BcmFile") ||
                propertyName.Equals("SedimentFile") ||
                propertyName.Equals("TraFile"))
            {
                return !data.UseMorphology;
            }

            return false;
        }

        /* Override needed to avoid displaying the whole class name in the property grid */
        public override string ToString()
        {
            return "";
        }
    }
}