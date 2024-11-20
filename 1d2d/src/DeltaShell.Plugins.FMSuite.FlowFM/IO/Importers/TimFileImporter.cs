using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class TimFileImporter: BoundaryDataImporterBase, IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (TimFileImporter));

        public Func<SourceAndSink, WaterFlowFMModel> GetModelForSourceAndSink { private get; set; }

        public Func<HeatFluxModel, WaterFlowFMModel> GetModelForHeatFluxModel { private get; set; }

        public Func<UniformWindField, WaterFlowFMModel> GetModelForWindTimeSeries { private get; set; }

        public bool WindFileImporter { get; set; }

        #region IFileImporter

        public string Name
        {
            get { return WindFileImporter ? "Time series file" : "Time series .tim file"; }
        }
        public string Description { get { return Name; } }
        public string Category
        {
            get { return "Time series"; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.TimeSeries; }
        }
        
        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                if (WindFileImporter)
                {
                    yield return typeof (UniformWindField);
                }
                else
                {
                    yield return typeof(SourceAndSink);
                    yield return typeof(HeatFluxModel);                    
                }
            }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel { get { return false; } }

        public override string FileFilter
        {
            get
            {
                return WindFileImporter
                    ? "Time series file (*.tim)|*.tim|Wind file (*.wnd)|*.wnd"
                    : "Time series file (*.tim)|*.tim";
            }
        }

        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get { return true; } }
        
        public object ImportItem(string path, object target = null)
        {
            if (WindFileImporter)
            {
                var windItem = target as UniformWindField;

                if (windItem == null) return target;
                
                try
                {
                    InsertTimeSeries(path, windItem.Data, GetModelForWindTimeSeries(windItem).ReferenceTime);
                }
                catch (Exception e)
                {
                    Log.ErrorFormat(Resources.File_import_failed___0_, e.Message);
                    return windItem;
                }

                return target;
            }

            var boundaryCondition = target as IBoundaryCondition;

            if (boundaryCondition != null && boundaryCondition.DataType == BoundaryConditionDataType.TimeSeries)
            {
                try
                {
                    var seriesToFill = SeriesToFill(boundaryCondition).ToList();
                    if (seriesToFill.Any())
                    {
                        InsertTimeSeries(path, seriesToFill.First(), ModelReferenceDate);
                        foreach (var function in seriesToFill.Skip(1))
                        {
                            CopyTimeSeries(seriesToFill.First(), function);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.ErrorFormat(Resources.Tim_file_import_failed___0_, e.Message);
                    return boundaryCondition;
                }
            }

            var sourceAndSink = target as SourceAndSink;

            if (sourceAndSink != null)
            {
                try
                {
                    var model = GetModelForSourceAndSink(sourceAndSink);
                    if (model == null)
                    {
                        Log.ErrorFormat(Resources.Tim_file_import_failed__could_not_retrieve_model_for_SourceAndSink___0_, sourceAndSink.Name);
                        return sourceAndSink;
                    }
                    
                    var sourceAndSinkFunction = sourceAndSink.Function;
                    if (sourceAndSinkFunction == null)
                    {
                        Log.ErrorFormat(Resources.Tim_file_import_failed__could_not_retrieve_function_for_SourceAndSink___0_, sourceAndSink.Name);
                        return sourceAndSink;
                    }

                    var readFunction = new TimFile().Read(path, model.ReferenceTime);
                    sourceAndSink.CopyValuesFromFileToSourceAndSinkAttributes(readFunction);
                    
                    var componentSettings = new Dictionary<string, bool>()
                    {
                        {SourceAndSink.SalinityVariableName, model.UseSalinity},
                        {SourceAndSink.TemperatureVariableName, model.UseTemperature},
                        {SourceAndSink.SecondaryFlowVariableName, model.UseSecondaryFlow }
                    };

                    sourceAndSink.SedimentFractionNames.ForEach(sfn => componentSettings.Add(sfn, model.UseMorSed));
                    sourceAndSink.TracerNames.ForEach(tn => componentSettings.Add(tn, true));
                    sourceAndSink.PopulateFunctionValuesFromAttributes(componentSettings);

                    return sourceAndSink;
                }
                catch (Exception e)
                {
                    Log.ErrorFormat(Resources.Tim_file_import_failed___0_, e.Message);
                    return sourceAndSink;
                }
            }

            var heatFluxModel = target as HeatFluxModel;

            if (heatFluxModel != null)
            {
                try
                {
                    InsertTimeSeries(path, heatFluxModel.MeteoData, GetModelForHeatFluxModel(heatFluxModel).ReferenceTime);
                }
                catch (Exception e)
                {
                    Log.ErrorFormat(Resources.Tim_file_import_failed___0_, e.Message);
                    return heatFluxModel;
                }
            }

            return target;
        }

        [InvokeRequired]
        private static void InsertTimeSeries(string path, IFunction data, DateTime? refDate)
        {
            new TimFile().Read(path, data, refDate);
        }

        [InvokeRequired]
        private static void CopyTimeSeries(IFunction functionFrom, IFunction functionTo)
        {
            functionTo.BeginEdit("Copying time series from file");
            functionTo.Clear();
            FunctionHelper.SetValuesRaw<DateTime>(functionTo.Arguments[0], functionFrom.Arguments[0].GetValues<DateTime>());
            for (var i = 0; i < functionTo.Components.Count; ++i)
            {
                FunctionHelper.SetValuesRaw<double>(functionTo.Components[i],
                    functionFrom.Components[i].GetValues<double>());
            }
            functionTo.EndEdit();
        }

        #endregion

        public override IEnumerable<BoundaryConditionDataType> ForcingTypes
        {
            get { yield return BoundaryConditionDataType.TimeSeries; }
        }

        public override void Import(string fileName, FlowBoundaryCondition boundaryCondition)
        {
            ImportItem(fileName, boundaryCondition);
        }

        public override bool CanImportOnBoundaryCondition(FlowBoundaryCondition boundaryCondition)
        {
            return ForcingTypes.Contains(boundaryCondition.DataType);
        }
    }
}
