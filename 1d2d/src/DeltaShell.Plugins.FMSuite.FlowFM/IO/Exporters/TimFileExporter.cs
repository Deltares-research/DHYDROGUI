using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
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
        public string Description { get { return Name; } }

        public string Category { get { return "General"; } }

        public bool Export(object item, string path)
        {
            IFunction data = null;
            DateTime? refDate = null;

            var boundaryCondition = item as IBoundaryCondition;
            if (boundaryCondition != null && boundaryCondition.DataType == BoundaryConditionDataType.TimeSeries)
            {
                data = SeriesToExport(boundaryCondition);
                refDate = ModelReferenceDate;
            }

            var sourceAndSink = item as SourceAndSink;
            if (sourceAndSink != null)
            {
                var function = sourceAndSink.Function;
                if (function == null)
                {
                    Log.ErrorFormat(Resources.Could_not_export_data_for_SourceAndSink___0___no_Function_was_found, sourceAndSink.Name);
                    return false;
                }

                data = (IFunction)function.Clone(true);

                var model = GetModelForSourceAndSink(sourceAndSink);
                if (model != null)
                {
                    refDate = model.ReferenceTime;
                    
                    if (!model.UseSalinity) data.RemoveComponentByName(SourceAndSink.SalinityVariableName);
                    if (!model.UseTemperature) data.RemoveComponentByName(SourceAndSink.TemperatureVariableName);
                    if (!model.UseMorSed) sourceAndSink.SedimentFractionNames.ForEach(sfn => data.RemoveComponentByName(sfn));
                    if (!model.UseSecondaryFlow) data.RemoveComponentByName(SourceAndSink.SecondaryFlowVariableName);
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
                writer.Write(path, data, refDate);
                return true;
            }
            catch (Exception e)
            {
                Log.ErrorFormat(Resources.TimFileExporter_Export_Failed_to_export_data_to__0____1_, path, e.Message);
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

        [ExcludeFromCodeCoverage]
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
