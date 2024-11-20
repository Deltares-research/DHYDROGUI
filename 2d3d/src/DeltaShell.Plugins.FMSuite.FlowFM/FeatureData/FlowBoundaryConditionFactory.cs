using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public class FlowBoundaryConditionFactory : BoundaryConditionFactory
    {
        private const string MorphologyBedloadTransport = "MorphologyBedLoadTransport";

        public override bool SupportsMultipleConditionsPerSet => true;

        public WaterFlowFMModel Model { set; private get; }

        public static IBoundaryCondition CreateBoundaryCondition(Feature2D feature2D)
        {
            return CreateBoundaryCondition(feature2D, FlowBoundaryQuantityType.WaterLevel,
                                           BoundaryConditionDataType.TimeSeries, null, null);
        }

        public override IBoundaryCondition CreateBoundaryCondition(Feature2D feature2D,
                                                                   string quantity,
                                                                   BoundaryConditionDataType dataType,
                                                                   string quantityType = null)
        {
            var fractionList = new List<string>();
            if (Model != null)
            {
                fractionList = FilterSedimentFractions(quantity, Model.SedimentFractions)
                               .Select(sf => sf.Name).ToList();
            }

            // try to parse the sediment concentration name and the quantity type of the boundary condition (special case)
            if (quantityType != null &&
                FlowBoundaryQuantityType.SedimentConcentration.GetDescription().Equals(quantityType) &&
                fractionList.Count > 0 &&
                fractionList.Contains(quantity))
            {
                string fractionName = quantity;
                return CreateBoundaryCondition(feature2D,
                                               FlowBoundaryQuantityType.SedimentConcentration,
                                               dataType,
                                               fractionName,
                                               fractionList);
            }

            // try to parse the tracer name and the quantity type of the boundary condition (special case)
            string tracerName = null;
            if (Model != null && Model.TracerDefinitions.Contains(quantity))
            {
                tracerName = quantity;
            }

            if (tracerName != null)
            {
                return CreateBoundaryCondition(feature2D, FlowBoundaryQuantityType.Tracer, dataType, tracerName,
                                               fractionList);
            }

            // try to parse regular boundary condition
            if (quantity != FlowBoundaryQuantityType.Tracer.ToString()
                && quantity != FlowBoundaryQuantityType.SedimentConcentration.ToString()
                && Enum.TryParse(quantity, out FlowBoundaryQuantityType flowBoundaryQuantityType))
            {
                return CreateBoundaryCondition(feature2D,
                                               flowBoundaryQuantityType,
                                               dataType,
                                               null,
                                               fractionList);
            }

            return null;
        }

        private static IBoundaryCondition CreateBoundaryCondition(Feature2D feature2D,
                                                                  FlowBoundaryQuantityType flowBoundaryQuantityType,
                                                                  BoundaryConditionDataType dataType,
                                                                  string tracerName,
                                                                  List<string> sedimentFractionNames)
        {
            var result = new FlowBoundaryCondition(flowBoundaryQuantityType, dataType)
            {
                Feature = feature2D,
                SedimentFractionNames = sedimentFractionNames
            };
            if (flowBoundaryQuantityType == FlowBoundaryQuantityType.SedimentConcentration)
            {
                result.SedimentFractionName = tracerName;
            }

            if (flowBoundaryQuantityType == FlowBoundaryQuantityType.Tracer)
            {
                result.TracerName = tracerName;
            }

            result.Name = feature2D.Name + "-" + result.VariableDescription;

            if (result.IsHorizontallyUniform)
            {
                result.AddPoint(0);
            }

            return result;
        }

        private static IList<ISedimentFraction> FilterSedimentFractions(
            string variable, IEventedList<ISedimentFraction> modelSedimentFractions)
        {
            IList<ISedimentFraction> filteredSedimentFractions = null;
            if (variable == MorphologyBedloadTransport)
            {
                filteredSedimentFractions =
                    modelSedimentFractions.Where(sf => sf.CurrentSedimentType.Key != "mud").AsList();
            }

            return filteredSedimentFractions ?? modelSedimentFractions;
        }
    }
}