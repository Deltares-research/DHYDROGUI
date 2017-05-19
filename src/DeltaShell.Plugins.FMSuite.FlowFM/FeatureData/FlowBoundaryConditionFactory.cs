using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public class FlowBoundaryConditionFactory: BoundaryConditionFactory
    {
        public override bool SupportsMultipleConditionsPerSet
        {
            get { return true; }
        }

        public WaterFlowFMModel Model { set; private get; }

        public override IBoundaryCondition CreateBoundaryCondition(Feature2D feature, string variable, BoundaryConditionDataType dataType, string quantityType = null)
        {
            FlowBoundaryQuantityType flowBoundaryQuantityType;
            var fractionList = new List<string>();
            if (Model != null)
            {
                fractionList = Model.SedimentFractions.Where(sf => sf.CurrentSedimentType.Name != "Mud").Select(sf => sf.Name).ToList();
            }
            if (variable != FlowBoundaryQuantityType.Tracer.ToString()
                && variable != FlowBoundaryQuantityType.SedimentConcentration.ToString()
                && Enum.TryParse(variable, out flowBoundaryQuantityType))
            {
                return CreateBoundaryCondition(feature, flowBoundaryQuantityType, dataType, null, fractionList);
            }

            if (quantityType != null && EnumDescriptionAttributeTypeConverter.GetEnumDescription(FlowBoundaryQuantityType.SedimentConcentration).Equals(quantityType))
            {
                string fractionName = null;
                if (fractionList.Count > 0 && fractionList.Contains(variable))
                {
                    fractionName = variable;
                    return CreateBoundaryCondition(feature, FlowBoundaryQuantityType.SedimentConcentration, dataType, fractionName, fractionList);
                }
            }

            // parse the tracer name and the quantity type of the boundary condition
            string tracerName = null;
            if (Model != null && Model.TracerDefinitions.Contains(variable))
            {
                tracerName = variable;
            }

            if (tracerName != null)
            {
                return CreateBoundaryCondition(feature, FlowBoundaryQuantityType.Tracer, dataType, tracerName, fractionList);
            }
            return null;
        }

        private static IBoundaryCondition CreateBoundaryCondition(Feature2D feature, 
                                                                  FlowBoundaryQuantityType flowBoundaryQuantityType,
                                                                  BoundaryConditionDataType dataType,
                                                                  string tracerName,
                                                                  List<string> sedimentFractionNames )
        {
            var result = new FlowBoundaryCondition(flowBoundaryQuantityType, dataType) {Feature = feature, SedimentFractionNames = sedimentFractionNames };
            if (flowBoundaryQuantityType == FlowBoundaryQuantityType.SedimentConcentration)
                result.SedimentFractionName = tracerName;
            if (flowBoundaryQuantityType == FlowBoundaryQuantityType.Tracer)
                result.TracerName = tracerName;

                result.Name = feature.Name + "-" + result.VariableDescription;

            if (result.IsHorizontallyUniform)
            {
                result.AddPoint(0);
            }

            return result;
        }

        public static IBoundaryCondition CreateBoundaryCondition(Feature2D feature2D)
        {
            return CreateBoundaryCondition(feature2D, FlowBoundaryQuantityType.WaterLevel,
                                           BoundaryConditionDataType.TimeSeries, null, null);
        }
    }
}
