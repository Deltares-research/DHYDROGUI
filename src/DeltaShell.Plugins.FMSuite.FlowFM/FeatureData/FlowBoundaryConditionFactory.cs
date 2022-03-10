using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public class FlowBoundaryConditionFactory: BoundaryConditionFactory
    {
        private const string MorphologyBedloadTransport = "MorphologyBedLoadTransport";

        public override bool SupportsMultipleConditionsPerSet
        {
            get { return true; }
        }

        public WaterFlowFMModel Model { set; private get; }

        public override IBoundaryCondition CreateBoundaryCondition(Feature2D feature, string variable, BoundaryConditionDataType dataType, string quantityType = null)
        {
            var fractionList = new List<string>();
            if (Model != null)
            {
                fractionList = FilterSedimentFractions(variable, Model.SedimentFractions).Select(sf => sf.Name).ToList();
            }

            // try to parse the sediment concentration name and the quantity type of the boundary condition (special case)
            if (quantityType != null && 
                FlowBoundaryQuantityType.SedimentConcentration.GetDescription().Equals(quantityType) && fractionList.Count > 0 && 
                fractionList.Contains(variable))
            {
                var fractionName = variable;
                return CreateBoundaryCondition(feature, FlowBoundaryQuantityType.SedimentConcentration, dataType, fractionName, fractionList);
            }

            // try to parse the tracer name and the quantity type of the boundary condition (special case)
            string tracerName = null;
            if (Model != null && Model.TracerDefinitions.Contains(variable))
            {
                tracerName = variable;
            }

            if (tracerName != null)
            {
                return CreateBoundaryCondition(feature, FlowBoundaryQuantityType.Tracer, dataType, tracerName, fractionList);
            }

            // try to parse regular boundary condition
            if (variable != FlowBoundaryQuantityType.Tracer.ToString()
                && variable != FlowBoundaryQuantityType.SedimentConcentration.ToString()
                && Enum.TryParse(variable, out FlowBoundaryQuantityType flowBoundaryQuantityType))
            {
                return CreateBoundaryCondition(feature, flowBoundaryQuantityType, dataType, null, fractionList);
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
            {
                result.SedimentFractionName = tracerName;
            }

            if (flowBoundaryQuantityType == FlowBoundaryQuantityType.Tracer)
            {
                result.TracerName = tracerName;
            }

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

        private static IList<ISedimentFraction> FilterSedimentFractions(string variable, IEventedList<ISedimentFraction> modelSedimentFractions)
        {
            IList<ISedimentFraction> filteredSedimentFractions = null;
            if (variable == MorphologyBedloadTransport) filteredSedimentFractions = modelSedimentFractions.Where(sf => sf.CurrentSedimentType.Key != "mud").AsList();
            return filteredSedimentFractions ?? modelSedimentFractions;
        }
    }
}
