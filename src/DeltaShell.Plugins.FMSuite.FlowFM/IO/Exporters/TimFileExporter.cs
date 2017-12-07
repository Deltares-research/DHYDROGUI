using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class TimFileExporter: BoundaryDataExporterBase, IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (TimFileExporter));

        public Func<SourceAndSink, WaterFlowFMModel> GetModelForSourceAndSink { private get; set; }

        public Func<HeatFluxModel, WaterFlowFMModel> GetModelForHeatFluxModel { private get; set; }

        #region IFileExporter

        public string Name { get { return "Time series to .tim file"; } }

        public string Category { get { return "General"; } }

        public bool Export(object item, string path)
        {
            IFunction data = null;
            DateTime? refDate = null;
            ICollection<int> componentIndexesToIgnore = null;

            var boundaryCondition = item as IBoundaryCondition;
            if (boundaryCondition != null && boundaryCondition.DataType == BoundaryConditionDataType.TimeSeries)
            {
                data = SeriesToExport(boundaryCondition);
                refDate = ModelReferenceDate;
            }

            var sourceAndSink = item as SourceAndSink;
            if (sourceAndSink != null)
            {
                data = sourceAndSink.Function;
                var model = GetModelForSourceAndSink(sourceAndSink);
                if (model != null)
                {
                    refDate = model.ReferenceTime;
                    componentIndexesToIgnore = new List<int>();

                    if (data != null)
                    {
                        if (!model.UseSalinity)
                        {
                            var salinityComponentIndex = data.GetComponentIndexByName(SourceAndSink.SalinityVariableName);
                            if (salinityComponentIndex >= 0)
                            {
                                componentIndexesToIgnore.Add(salinityComponentIndex);
                            }
                        }

                        if (!model.UseTemperature)
                        {
                            var temperatureComponentIndex = data.GetComponentIndexByName(SourceAndSink.TemperatureVariableName);
                            if (temperatureComponentIndex >= 0)
                            {
                                componentIndexesToIgnore.Add(temperatureComponentIndex);
                            }
                        }
                    }
                }
            }

            var heatFluxModel = item as HeatFluxModel;
            if (heatFluxModel != null)
            {                
                data = heatFluxModel.MeteoData;
                if (GetModelForHeatFluxModel != null)
                {
                    refDate = GetModelForHeatFluxModel(heatFluxModel).ReferenceTime;
                }
            }

            if (data == null) return false;
            
            try
            {
                var writer = new TimFile();
                writer.Write(path, data, refDate, componentIndexesToIgnore);
                return true;
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Failed to export data to {0}: {1}", path, e.Message);
                return false;
            }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof (SourceAndSink);
            yield return typeof (HeatFluxModel);
        }

        public string FileFilter
        {
            get { return "Time series file|*.tim"; }
        }

        public Bitmap Icon { get { return Properties.Resources.TextDocument; } }
        public bool CanExportFor(object item)
        {
            return true;
        }

        #endregion

        public override IEnumerable<BoundaryConditionDataType> ForcingTypes
        {
            get { yield return BoundaryConditionDataType.TimeSeries; }
        }
    }
}
