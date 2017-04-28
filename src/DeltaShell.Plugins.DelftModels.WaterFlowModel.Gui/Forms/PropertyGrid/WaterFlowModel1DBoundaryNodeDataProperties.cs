using System;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_DisplayName")]
    public class WaterFlowModel1DBoundaryNodeDataProperties : ObjectProperties<WaterFlowModel1DBoundaryNodeData>
    {
        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_Name_Description")]
        public string Name
        {
            get { return data.Name; }
        }

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_Type_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_Type_Description")]
        public WaterFlowModel1DBoundaryNodeDataType Type
        {
            get { return data.DataType; }
            set { data.DataType = value; }
        }

        [PropertyOrder(3)]
        [DynamicReadOnly] // TODO: make this property invisible if readonly
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_InterpolationTypeT_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_InterpolationTypeT_Description")]
        public InterpolationTypeBoundaryTime InterpolationTypeT
        {
            get
            {
                if (!SourceHasFunctionData())
                {
                    return InterpolationTypeBoundaryTime.Constant;
                }

                var type = (InterpolationTypeBoundaryTime) data.Data.Arguments[0].InterpolationType;

                return Enum.IsDefined(typeof(InterpolationTypeBoundaryTime), type)
                           ? type
                           : InterpolationTypeBoundaryTime.Constant;
            }
            set { data.Data.Arguments[0].InterpolationType = (InterpolationType) value; }
        }

        [PropertyOrder(4)] // TODO: make this property invisible if not relevant
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_InterpolationTypeQh_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_InterpolationTypeQh_Description")]
        public InterpolationTypeBoundaryQh InterpolationTypeQh
        {
            get
            {
                if (!SourceHasFunctionData())
                {
                    return InterpolationTypeBoundaryQh.Linear;
                }

                var type = (InterpolationTypeBoundaryQh) data.Data.Arguments[0].InterpolationType;

                return Enum.IsDefined(typeof(InterpolationTypeBoundaryQh), type)
                           ? type
                           : InterpolationTypeBoundaryQh.Linear;
            }
        }

        [PropertyOrder(5)]
        [DynamicReadOnly] // TODO: make this property invisible if readonly
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_ExtrapolationTypeT_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_ExtrapolationTypeT_Description")]
        public ExtrapolationType ExtrapolationTypeT
        {
            get
            {
                return !SourceHasFunctionData()
                           ? ExtrapolationType.None
                           : data.Data.Arguments[0].ExtrapolationType;
            }
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
                if (!SourceHasFunctionData())
                {
                    return ExtrapolationTypeQh.Constant;
                }

                var type = (ExtrapolationTypeQh) data.Data.Arguments[0].ExtrapolationType;

                return Enum.IsDefined(typeof (ExtrapolationTypeQh), type)
                           ? type
                           : ExtrapolationTypeQh.Constant;
            }
        }

        [PropertyOrder(7)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_NodeName_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DBoundaryNodeDataProperties_NodeName_Description")]
        public string NodeName
        {
            get { return data.Node.Name; }
            set { data.Node.Name = value; }
        }

        /// <summary>
        /// Returns whether or not the BC has data in a function. If so, interpolation and extrapolation can be set and read
        /// </summary>
        private bool SourceHasFunctionData()
        {
            return (data.DataType == WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries
                    || data.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable
                    || data.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries)
                   && data.Data != null;
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == "InterpolationTypeT")
            {
                return !(Type == WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries || Type == WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries);
            }

            if (propertyName == "ExtrapolationTypeT")
            {
                return !(Type == WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries || Type == WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries);
            }

            return true;
        }
    }
}