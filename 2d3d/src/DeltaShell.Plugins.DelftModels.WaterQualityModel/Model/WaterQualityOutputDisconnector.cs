using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    /// <summary>
    /// Disconnects D-Water Quality model from the output
    /// </summary>
    public static class WaterQualityOutputDisconnector
    {
        /// <summary>
        /// Disconnects the specified <paramref name="model"/> from the output.
        /// </summary>
        /// <param name="model"> The model. </param>
        public static void Disconnect(WaterQualityModel model)
        {
            List<IDataItem> outputDataItems = model.AllDataItems
                                                   .Where(di => di.Role.HasFlag(DataItemRole.Output))
                                                   .ToList();

            DisconnectMapOutput(model, outputDataItems);
            DisconnectHistoryOutput(outputDataItems);
            DisconnectTextFiles(model, outputDataItems);
        }

        private static void DisconnectMapOutput(WaterQualityModel model, IReadOnlyCollection<IDataItem> outputDataItems)
        {
            model.MapFileFunctionStore.Path = null;
            outputDataItems.Select(di => di.Value)
                           .OfType<UnstructuredGridCellCoverage>()
                           .ForEach(c => c.ClearCoverage());

            outputDataItems.Select(di => di.Value)
                           .OfType<IFeatureCoverage>()
                           .ForEach(c =>
                           {
                               c.Filters.Clear();
                               c.Clear();
                           });
        }

        private static void DisconnectHistoryOutput(IEnumerable<IDataItem> outputDataItems)
        {
            outputDataItems.Select(di => di.Value)
                           .OfType<WaterQualityObservationVariableOutput>()
                           .ForEach(v => v.TimeSeriesList.ForEach(t => t.Clear()));
        }

        private static void DisconnectTextFiles(IModel model, IEnumerable<IDataItem> outputDataItems)
        {
            outputDataItems.Where(di => di.ValueType == typeof(TextDocument))
                           .ForEach(di => model.DataItems.Remove(di));
        }
    }
}