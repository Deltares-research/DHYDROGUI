using System;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DLateralDataProperties_DisplayName")]
    public class WaterFlowModel1DLateralDataProperties : ObjectProperties<WaterFlowModel1DLateralSourceData>
    {
        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DLateralDataProperties_Name_Description")]
        public string Name
        {
            get { return data.Name; }
        }

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DLateralDataProperties_Type_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DLateralDataProperties_Type_Description")]
        public WaterFlowModel1DLateralDataType LateralType
        {
            get { return data.DataType; }
            set
            {
                if (data.DataType != value)
                {
                    data.DataType = value;

                    if (data.DataType == WaterFlowModel1DLateralDataType.FlowWaterLevelTable && data.Data != null)
                    {
                        data.Data.Arguments[0].InterpolationType = InterpolationType.Linear;
                    }
                }
            }
        }

        [PropertyOrder(3)]
        [DynamicReadOnly] // TODO: make this property invisible if readonly
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DLateralDataProperties_InterpolationTypeQt_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DLateralDataProperties_InterpolationTypeQt_Description")]
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

        [PropertyOrder(4)] // TODO: make this property invisible if not relevant
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_InterpolationTypeQh_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_ExtrapolationTypeQh_Description")]
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
        [DynamicReadOnly] // TODO: make this property invisible if readonly
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DLateralDataProperties_ExtrapolationTypeQt_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DLateralDataProperties_ExtrapolationTypeQt_Description")]
        public ExtrapolationType ExtrapolationTypeQt
        {
            get { return data.Data.Arguments[0].ExtrapolationType; }
            set { data.Data.Arguments[0].ExtrapolationType = value; }
        }

        [PropertyOrder(6)] // TODO: make this property invisible if not relevant
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_ExtrapolationTypeQh_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_ExtrapolationTypeQh_Description")]
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

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == "InterpolationTypeQt")
            {
                return LateralType != WaterFlowModel1DLateralDataType.FlowTimeSeries;
            }

            if (propertyName == "ExtrapolationTypeQt")
            {
                return LateralType != WaterFlowModel1DLateralDataType.FlowTimeSeries;
            }

            return true;
        }
    }
}
