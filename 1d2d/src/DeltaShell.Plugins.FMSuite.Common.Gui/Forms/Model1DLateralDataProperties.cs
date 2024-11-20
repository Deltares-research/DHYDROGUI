using System;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    [ResourcesDisplayName(typeof(Resources), "Model1DLateralDataProperties_DisplayName")]
    public class Model1DLateralDataProperties : ObjectProperties<Model1DLateralSourceData>
    {
        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DLateralDataProperties_Name_Description")]
        public string Name
        {
            get { return data.Name; }
        }

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DLateralDataProperties_Type_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DLateralDataProperties_Type_Description")]
        public Model1DLateralDataType LateralType
        {
            get { return data.DataType; }
            set
            {
                if (data.DataType != value)
                {
                    data.DataType = value;

                    if (data.DataType == Model1DLateralDataType.FlowWaterLevelTable && data.Data != null)
                    {
                        data.Data.Arguments[0].InterpolationType = InterpolationType.Linear;
                    }
                }
            }
        }

        [PropertyOrder(3)]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DLateralDataProperties_InterpolationTypeQt_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DLateralDataProperties_InterpolationTypeQt_Description")]
        public InterpolationTypeBoundaryTime InterpolationTypeQt
        {
            get
            {
                var type = (InterpolationTypeBoundaryTime) data.Data.Arguments[0].InterpolationType;

                return Enum.IsDefined(typeof(InterpolationTypeBoundaryTime), type)
                           ? type
                           : InterpolationTypeBoundaryTime.Constant;
            }
            set
            {
                data.Data.Arguments[0].InterpolationType = (InterpolationType) value;
            }
        }

        [PropertyOrder(4)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DBoundaryNodeDataProperties_InterpolationTypeQh_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DBoundaryNodeDataProperties_ExtrapolationTypeQh_Description")]
        public InterpolationTypeBoundaryQh InterpolationTypeQh
        {
            get
            {
                var type = (InterpolationTypeBoundaryQh) data.Data.Arguments[0].InterpolationType;

                return Enum.IsDefined(typeof(InterpolationTypeBoundaryQh), type)
                           ? type
                           : InterpolationTypeBoundaryQh.Linear;
            }
        }

        [PropertyOrder(5)]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DLateralDataProperties_ExtrapolationTypeQt_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DLateralDataProperties_ExtrapolationTypeQt_Description")]
        public ExtrapolationType ExtrapolationTypeQt
        {
            get { return data.Data.Arguments[0].ExtrapolationType; }
            set { data.Data.Arguments[0].ExtrapolationType = value; }
        }

        [PropertyOrder(6)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DBoundaryNodeDataProperties_ExtrapolationTypeQh_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DBoundaryNodeDataProperties_ExtrapolationTypeQh_Description")]
        public ExtrapolationTypeQh ExtrapolationTypeQh
        {
            get
            {
                var type = (ExtrapolationTypeQh) data.Data.Arguments[0].ExtrapolationType;

                return Enum.IsDefined(typeof(ExtrapolationTypeQh), type)
                           ? type
                           : ExtrapolationTypeQh.Constant;
            }
        }

        [PropertyOrder(7)]
        [DynamicVisible]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DLateralDataProperties_Compartment_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DLateralDataProperties_Compartment_Description")]
        public string Compartment
        {
            get
            {
                if (data.Compartment != null) return data.Compartment.ToString();
                return string.Empty;
            }
        }

        [DynamicVisibleValidationMethod]
        public bool DynamicVisibleValidationMethod(string propertyName)
        {
            if (propertyName.Equals(nameof(Model1DLateralSourceData.Compartment), StringComparison.InvariantCultureIgnoreCase))
            {
                return data.Compartment != null;
            }

            return true;
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == "InterpolationTypeQt")
            {
                return LateralType != Model1DLateralDataType.FlowTimeSeries;
            }

            if (propertyName == "ExtrapolationTypeQt")
            {
                return LateralType != Model1DLateralDataType.FlowTimeSeries;
            }

            return true;
        }
    }
}
