using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Boundary
{
    /// <summary>
    /// Class to get the boundaries for the structure file writers.
    /// </summary>
    public class StructureBoundaryGenerator : BoundaryFileWriter, IStructureBoundaryGenerator
    {
        private const string boundaryHeader = "forcing";

        public IEnumerable<IDelftIniCategory> GenerateBoundaries(IEnumerable<IStructureTimeSeries> structureData, DateTime startTime)
        {
            Ensure.NotNull(structureData, nameof(structureData));

            var categories = new List<IDelftIniCategory>();

            foreach (IStructureTimeSeries structureTimeSeries in structureData)
            {
                IStructure1D structure = structureTimeSeries.Structure;
                ITimeSeries timeSeries = structureTimeSeries.TimeSeries;
                
                List<IVariable> boundaryNodeData = timeSeries.Components.ToList();
                categories.AddRange(boundaryNodeData.Select(data => GenerateBoundaryConditionDefinition(structure.Name, startTime, data, QuantityHelper.GetQuantity(structure, timeSeries.Name))));
            }

            return categories;
        }
        
        public IEnumerable<IDelftIniCategory> GenerateBoundary(string structureName, ITimeSeries structureData, DateTime startTime)
        {
            Ensure.NotNull(structureName, nameof(structureName));
            Ensure.NotNull(structureData, nameof(structureData));
            
            var categories = new List<IDelftIniCategory>
            {
                GenerateBoundaryConditionDefinition(structureName, startTime, structureData, string.Empty)
            };

            return categories;
        }

        private static IDelftBcCategory GenerateBoundaryConditionDefinition(string name, DateTime startTime, IFunction boundaryNodeData, string quantity)
        {
            string periodic = GetTimeSeriesIsPeriodicProperty(boundaryNodeData);

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(boundaryHeader);

            IDelftBcCategory boundaryDefinition = definitionGenerator.CreateRegion(name,
                                                                                   BoundaryRegion.FunctionStrings.TimeSeries,
                                                                                   BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate,
                                                                                   periodic);

            IDictionary<string, string> quantityAndUnitData = QuantityHelper.GetQuantityAndUnit(boundaryNodeData, quantity);
            boundaryDefinition.Table = GenerateTableForTimeSeriesData(quantityAndUnitData, boundaryNodeData, startTime);

            return boundaryDefinition;
        }
    }
}