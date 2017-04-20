using System.ComponentModel;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [ResourcesDisplayName(typeof(Resources), "ModelApiParameterProperties_DisplayName")]
    public class ModelApiParameterProperties
    {
        private readonly ModelApiParameter parameter;

        public ModelApiParameterProperties(ModelApiParameter parameter)
        {
            this.parameter = parameter;
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "ModelApiParameterProperties_Name_Description")]
        public string Name
        {
            get { return parameter.Name; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "ModelApiParameterProperties_Category_DisplayName")]
        [ResourcesDescription(typeof(Resources), "ModelApiParameterProperties_Category_Description")]
        public ParameterCategory Category
        {
            get { return parameter.Category; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "ModelApiParameterProperties_Description_DisplayName")]
        [ResourcesDescription(typeof(Resources), "ModelApiParameterProperties_Description_Description")]
        public string Description
        {
            get { return parameter.Description; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "ModelApiParameterProperties_Type_DisplayName")]
        [ResourcesDescription(typeof(Resources), "ModelApiParameterProperties_Type_Description")]
        public string Type
        {
            get { return parameter.Type; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "ModelApiParameterProperties_Value_DisplayName")]
        [ResourcesDescription(typeof(Resources), "ModelApiParameterProperties_Value_Description")]
        public string Value
        {
            get { return parameter.Value; }
            set { parameter.Value = value; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "ModelApiParameterProperties_Visible_DisplayName")]
        [ResourcesDescription(typeof(Resources), "ModelApiParameterProperties_Visible_Description")]
        public bool Visible
        {
            get { return parameter.Visible; }
        }

        public override string ToString()
        {
            return string.Format("{0} : ({1})", Name, Value);
        }
    }
}