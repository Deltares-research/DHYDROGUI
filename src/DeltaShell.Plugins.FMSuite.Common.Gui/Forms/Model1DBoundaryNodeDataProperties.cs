using System;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    [ResourcesDisplayName(typeof(Resources), "Model1DBoundaryNodeDataProperties_DisplayName")]
    public class Model1DBoundaryNodeDataProperties : ObjectProperties<Model1DBoundaryNodeData>
    {
        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DBoundaryNodeDataProperties_Name_Description")]
        public string Name
        {
            get { return data.Name; }
        }
        
        [PropertyOrder(2)]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DBoundaryNodeDataProperties_Type_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DBoundaryNodeDataProperties_Type_Description")]
        public Model1DBoundaryNodeDataType Type
        {
            get { return data.DataType; }
            set { data.DataType = value; }
        }
        
        [PropertyOrder(3)]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DBoundaryNodeDataProperties_InterpolationTypeT_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DBoundaryNodeDataProperties_InterpolationTypeT_Description")]
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

        [PropertyOrder(4)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DBoundaryNodeDataProperties_InterpolationTypeQh_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DBoundaryNodeDataProperties_InterpolationTypeQh_Description")]
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
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DBoundaryNodeDataProperties_ExtrapolationTypeT_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DBoundaryNodeDataProperties_ExtrapolationTypeT_Description")]
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
        
        [PropertyOrder(6)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DBoundaryNodeDataProperties_ExtrapolationTypeQh_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DBoundaryNodeDataProperties_ExtrapolationTypeQh_Description")]
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
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DBoundaryNodeDataProperties_NodeName_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DBoundaryNodeDataProperties_NodeName_Description")]
        public string NodeName
        {
            get { return data.Node.Name; }
            set { data.Node.Name = value; }
        }

        [PropertyOrder(8)]
        [DynamicVisible]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Model1DBoundaryNodeDataProperties_OutletCompartment_DisplayName")]
        [ResourcesDescription(typeof(Resources), "Model1DBoundaryNodeDataProperties_OutletCompartment_Description")]
        public OutletCompartment OutletCompartment
        {
            get { return data.OutletCompartment; }
        }
        /// <summary>
        /// Returns whether or not the BC has data in a function. If so, interpolation and extrapolation can be set and read
        /// </summary>
        private bool SourceHasFunctionData()
        {
            return (data.DataType == Model1DBoundaryNodeDataType.WaterLevelTimeSeries
                    || data.DataType == Model1DBoundaryNodeDataType.FlowWaterLevelTable
                    || data.DataType == Model1DBoundaryNodeDataType.FlowTimeSeries)
                   && data.Data != null;
        }

        [DynamicVisibleValidationMethod]
        public bool DynamicVisibleValidationMethod(string propertyName)
        {
            if (propertyName.Equals(nameof(Model1DBoundaryNodeData.OutletCompartment), StringComparison.InvariantCultureIgnoreCase))
            {
                return data.OutletCompartment != null;
            }

            return true;
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == "InterpolationTypeT")
            {
                return !(Type == Model1DBoundaryNodeDataType.FlowTimeSeries || Type == Model1DBoundaryNodeDataType.WaterLevelTimeSeries);
            }

            if (propertyName == "ExtrapolationTypeT")
            {
                return !(Type == Model1DBoundaryNodeDataType.FlowTimeSeries || Type == Model1DBoundaryNodeDataType.WaterLevelTimeSeries);
            }

            if (propertyName == nameof(NodeName) || propertyName == nameof(Type))
            {
                return data.Node is IManhole;
            }

            return true;
        }
        
    }
}