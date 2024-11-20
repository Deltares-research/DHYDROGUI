using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekLateralSourcesDataImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekLateralSourcesDataImporter));

        private string displayName = "Lateral sources data";
        public override string DisplayName
        {
            get { return displayName; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

        protected override void PartialImport()
        {
            log.DebugFormat("Importing lateral source data ...");

            var waterFlowFMModel = GetModel<WaterFlowFMModel>();


            string lateralPath = GetFilePath(SobekFileNames.SobekLaterSourcesFileName);
            if (!File.Exists(lateralPath))
            {
                log.WarnFormat("Lateral condition file [{0}] not found; skipping...", lateralPath);
                return;
            }

            var lateralSources = HydroNetwork.LateralSources.ToDictionary(ls => ls.Name, ls => ls);
            var sobekLateralFlowReader = new SobekLateralFlowReader();
            var sobekLateralConditions = sobekLateralFlowReader.ReadLateralBoundaries(lateralPath);

            // reuse WaterFlowModel1DLateralSourceData object already in model.
            var lateralSourceDataMapping = new Dictionary<IFeature, Model1DLateralSourceData>();
            foreach (Model1DLateralSourceData flowModel1DLateralSourceData in waterFlowFMModel.LateralSourcesData)
            {
                lateralSourceDataMapping[flowModel1DLateralSourceData.Feature] = flowModel1DLateralSourceData;
            }

            foreach (var sobekLateralFlow in sobekLateralConditions)
            {
                if (lateralSources.ContainsKey(sobekLateralFlow.Id) && lateralSourceDataMapping.ContainsKey(lateralSources[sobekLateralFlow.Id]))
                {
                    var waterFlowModel1DLateralSourceData = lateralSourceDataMapping[lateralSources[sobekLateralFlow.Id]];
                    waterFlowModel1DLateralSourceData.Data.Clear();
                    ConvertToLateralSourceData(sobekLateralFlow, waterFlowModel1DLateralSourceData, SobekFileNames.SobekType == DeltaShell.Sobek.Readers.SobekType.SobekRE);
                }
                else
                {
                    log.WarnFormat("Lateral source {0} to add data has not been found.", sobekLateralFlow.Id);
                }
            }
        }

        /// <summary>
        /// Fill waterFlowModel1DLateralSourceData with values imported from sobekfile
        /// </summary>
        /// <param name="model1DLateralSourceData"></param>
        /// <param name="bSobekRE"></param>
        public static void ConvertToLateralSourceData(SobekLateralFlow sobekLateralFlow, Model1DLateralSourceData model1DLateralSourceData, bool bSobekRE = false)
        {
            if (bSobekRE)
            {
                ConvertToLateralSourceDataFromRE(sobekLateralFlow, model1DLateralSourceData);
            }
            else
            {
                ConvertToLateralSourceDataFrom212(sobekLateralFlow, model1DLateralSourceData);
            }
        }

        private static void ConvertToLateralSourceDataFromRE(SobekLateralFlow sobekLateralFlow, Model1DLateralSourceData model1DLateralSourceData)
        {
            if (sobekLateralFlow.IsConstantDischarge)
            {
                model1DLateralSourceData.DataType = Model1DLateralDataType.FlowConstant;
                model1DLateralSourceData.Flow = sobekLateralFlow.ConstantDischarge;
            }
            else if (sobekLateralFlow.FlowTimeTable != null)
            {

                model1DLateralSourceData.DataType = Model1DLateralDataType.FlowTimeSeries;
                DataTableHelper.SetTableToFunction(sobekLateralFlow.FlowTimeTable, model1DLateralSourceData.Data);
                model1DLateralSourceData.Data.Arguments[0].InterpolationType = sobekLateralFlow.InterpolationType;
                model1DLateralSourceData.Data.Arguments[0].ExtrapolationType = sobekLateralFlow.ExtrapolationType;
            }
            else if (sobekLateralFlow.LevelQhTable != null)
            {
                model1DLateralSourceData.DataType = Model1DLateralDataType.FlowWaterLevelTable;
                DataTableHelper.SetTableToFunction(sobekLateralFlow.LevelQhTable, model1DLateralSourceData.Data);
                model1DLateralSourceData.Data.Arguments[0].InterpolationType = InterpolationType.Linear;
                model1DLateralSourceData.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            }
            if (sobekLateralFlow.ExtrapolationType == ExtrapolationType.Periodic)
            {
                TimeSeriesHelper.SetPeriodicExtrapolationSobek(model1DLateralSourceData.Data, sobekLateralFlow.ExtrapolationPeriod);
            }

            if (!sobekLateralFlow.IsPointDischarge)
            {
                model1DLateralSourceData.Feature.Length = sobekLateralFlow.Length;
                UpdateFeatureGeometry(model1DLateralSourceData.Feature);
            }
        }

        private static void ConvertToLateralSourceDataFrom212(SobekLateralFlow sobekLateralFlow, Model1DLateralSourceData model1DLateralSourceData)
        {
            var isDiffuse = !sobekLateralFlow.IsPointDischarge;
            var length = sobekLateralFlow.Length;

            if (model1DLateralSourceData.Feature != null)
            {
                if (model1DLateralSourceData.Feature.IsDiffuse != isDiffuse)
                {
                    log.WarnFormat("Lateral source data for {0} does not match. Lateral source data is for type {1}, the lateral source is of type {2}",
                        sobekLateralFlow.Id,
                        (isDiffuse) ? "'diffuse'" : "'point'",
                        (model1DLateralSourceData.Feature.IsDiffuse) ? "'diffuse'" : "'point'"
                        );
                    return;
                }
                length = model1DLateralSourceData.Feature.Length;
            }

            if (length == 0.0)
            {
                length = 1.0;
            }

            if (sobekLateralFlow.IsConstantDischarge)
            {
                model1DLateralSourceData.DataType = Model1DLateralDataType.FlowConstant;

                if (isDiffuse)
                {
                    log.WarnFormat("The data of diffuse lateral source {0} has been changed from m2/s to m3/s (* length {1})", sobekLateralFlow.Id, length);
                    model1DLateralSourceData.Flow = sobekLateralFlow.ConstantDischarge * length;
                }
                else
                {
                    model1DLateralSourceData.Flow = sobekLateralFlow.ConstantDischarge;
                }
            }
            else if (sobekLateralFlow.FlowTimeTable != null)
            {

                model1DLateralSourceData.DataType = Model1DLateralDataType.FlowTimeSeries;
                DataTableHelper.SetTableToFunction(sobekLateralFlow.FlowTimeTable, model1DLateralSourceData.Data);

                if (isDiffuse)
                {
                    log.WarnFormat("The data of diffuse lateral source {0} has been changed from m2/s to m3/s (* length {1})", sobekLateralFlow.Id, length);
                    var values = model1DLateralSourceData.Data.Components[0].GetValues<double>();
                    for (int i = 0; i < values.Count; i++)
                    {
                        model1DLateralSourceData.Data.Components[0].Values[i] = values[i] * length;
                    }
                }
                model1DLateralSourceData.Data.Arguments[0].InterpolationType = sobekLateralFlow.InterpolationType;
                model1DLateralSourceData.Data.Arguments[0].ExtrapolationType = sobekLateralFlow.ExtrapolationType;
            }
            else if (sobekLateralFlow.LevelQhTable != null)
            {
                model1DLateralSourceData.DataType = Model1DLateralDataType.FlowWaterLevelTable;
                DataTableHelper.SetTableToFunction(sobekLateralFlow.LevelQhTable, model1DLateralSourceData.Data);

                if (isDiffuse)
                {
                    log.WarnFormat("The data of diffuse lateral source {0} has been changed from m2/s to m3/s (* length {1})", sobekLateralFlow.Id, length);
                    var values = model1DLateralSourceData.Data.Arguments[0].GetValues<double>();
                    for (int i = 0; i < values.Count; i++)
                    {
                        model1DLateralSourceData.Data.Arguments[0].Values[i] = values[i] * length;
                    }
                }

                model1DLateralSourceData.Data.Arguments[0].InterpolationType = InterpolationType.Linear;
                model1DLateralSourceData.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            }
            if (sobekLateralFlow.ExtrapolationType == ExtrapolationType.Periodic)
            {
                TimeSeriesHelper.SetPeriodicExtrapolationSobek(model1DLateralSourceData.Data, sobekLateralFlow.ExtrapolationPeriod);
            }
        }

        private static void UpdateFeatureGeometry(LateralSource lateralSource)
        {
            var lengthIndexedLine = new LengthIndexedLine(lateralSource.Branch.Geometry);
            lateralSource.Geometry = lengthIndexedLine.ExtractLine(lateralSource.Chainage, lateralSource.Chainage + lateralSource.Length);
        }
    }
}
