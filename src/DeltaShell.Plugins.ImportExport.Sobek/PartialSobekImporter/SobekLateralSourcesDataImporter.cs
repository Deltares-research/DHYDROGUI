using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Sobek.Readers;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekLateralSourcesDataImporter: PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekLateralSourcesDataImporter));

        private string displayName = "Lateral sources data";
        public override string DisplayName
        {
            get { return displayName; }
        }

        protected override void PartialImport()
        {
            log.DebugFormat("Importing lateral source data ...");

            var waterFlowModel1D = GetModel<WaterFlowModel1D>();

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
            var lateralSourceDataMapping = new Dictionary<IFeature, WaterFlowModel1DLateralSourceData>();
            foreach (WaterFlowModel1DLateralSourceData flowModel1DLateralSourceData in waterFlowModel1D.LateralSourceData)
            {
                lateralSourceDataMapping[flowModel1DLateralSourceData.Feature] = flowModel1DLateralSourceData;
            }

            foreach (var sobekLateralFlow in sobekLateralConditions)
            {
                if (lateralSources.ContainsKey(sobekLateralFlow.Id) && lateralSourceDataMapping.ContainsKey(lateralSources[sobekLateralFlow.Id]))
                {
                    var waterFlowModel1DLateralSourceData = lateralSourceDataMapping[lateralSources[sobekLateralFlow.Id]];
                    waterFlowModel1DLateralSourceData.Data.Clear();
                    ConvertToLateralSourceData(sobekLateralFlow, waterFlowModel1DLateralSourceData, SobekFileNames.SobekType == SobekType.SobekRE);
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
        /// <param name="waterFlowModel1DLateralSourceData"></param>
        /// <param name="bSobekRE"></param>
        public static void ConvertToLateralSourceData(SobekLateralFlow sobekLateralFlow, WaterFlowModel1DLateralSourceData waterFlowModel1DLateralSourceData, bool bSobekRE = false)
        {
            if (bSobekRE)
            {
                ConvertToLateralSourceDataFromRE(sobekLateralFlow, waterFlowModel1DLateralSourceData);
            }
            else
            {
                ConvertToLateralSourceDataFrom212(sobekLateralFlow, waterFlowModel1DLateralSourceData); 
            }
        }

        private static void ConvertToLateralSourceDataFromRE(SobekLateralFlow sobekLateralFlow, WaterFlowModel1DLateralSourceData waterFlowModel1DLateralSourceData)
        {
            if (sobekLateralFlow.IsConstantDischarge)
            {
                waterFlowModel1DLateralSourceData.DataType = WaterFlowModel1DLateralDataType.FlowConstant;
                waterFlowModel1DLateralSourceData.Flow = sobekLateralFlow.ConstantDischarge;
            }
            else if (sobekLateralFlow.FlowTimeTable != null)
            {

                waterFlowModel1DLateralSourceData.DataType = WaterFlowModel1DLateralDataType.FlowTimeSeries;
                DataTableHelper.SetTableToFunction(sobekLateralFlow.FlowTimeTable, waterFlowModel1DLateralSourceData.Data);
                //ConvertTableToTimeFunction(FlowTimeTable, waterFlowModel1DLateralSourceData);
                waterFlowModel1DLateralSourceData.Data.Arguments[0].InterpolationType = sobekLateralFlow.InterpolationType;
                waterFlowModel1DLateralSourceData.Data.Arguments[0].ExtrapolationType = sobekLateralFlow.ExtrapolationType;
            }
            else if (sobekLateralFlow.LevelQhTable != null)
            {
                waterFlowModel1DLateralSourceData.DataType = WaterFlowModel1DLateralDataType.FlowWaterLevelTable;
                DataTableHelper.SetTableToFunction(sobekLateralFlow.LevelQhTable, waterFlowModel1DLateralSourceData.Data);
                waterFlowModel1DLateralSourceData.Data.Arguments[0].InterpolationType = InterpolationType.Linear;
                waterFlowModel1DLateralSourceData.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            }
            if (sobekLateralFlow.ExtrapolationType == ExtrapolationType.Periodic)
            {
                TimeSeriesHelper.SetPeriodicExtrapolationSobek(waterFlowModel1DLateralSourceData.Data, sobekLateralFlow.ExtrapolationPeriod);
            }

            if (!sobekLateralFlow.IsPointDischarge)
            {
                waterFlowModel1DLateralSourceData.Feature.Length = sobekLateralFlow.Length;
                UpdateFeatureGeometry(waterFlowModel1DLateralSourceData.Feature);
            }
        }

        private static void ConvertToLateralSourceDataFrom212(SobekLateralFlow sobekLateralFlow, WaterFlowModel1DLateralSourceData waterFlowModel1DLateralSourceData)
        {
            var isDiffuse = !sobekLateralFlow.IsPointDischarge;
            var length = sobekLateralFlow.Length;

            if (waterFlowModel1DLateralSourceData.Feature != null)
            {
                if (waterFlowModel1DLateralSourceData.Feature.IsDiffuse != isDiffuse)
                {
                    log.WarnFormat("Lateral source data for {0} does not match. Lateral source data is for type {1}, the lateral source is of type {2}",
                        sobekLateralFlow.Id,
                        (isDiffuse) ? "'diffuse'" : "'point'",
                        (waterFlowModel1DLateralSourceData.Feature.IsDiffuse) ? "'diffuse'" : "'point'"
                        );
                    return;
                }
                length = waterFlowModel1DLateralSourceData.Feature.Length;
            }

            if (length == 0.0)
            {
                length = 1.0;
            }

            if (sobekLateralFlow.IsConstantDischarge)
            {
                waterFlowModel1DLateralSourceData.DataType = WaterFlowModel1DLateralDataType.FlowConstant;

                if (isDiffuse)
                {
                    log.WarnFormat("The data of diffuse lateral source {0} has been changed from m2/s to m3/s (* length {1})", sobekLateralFlow.Id, length);
                    waterFlowModel1DLateralSourceData.Flow = sobekLateralFlow.ConstantDischarge * length;
                }
                else
                {
                    waterFlowModel1DLateralSourceData.Flow = sobekLateralFlow.ConstantDischarge;
                }
            }
            else if (sobekLateralFlow.FlowTimeTable != null)
            {

                waterFlowModel1DLateralSourceData.DataType = WaterFlowModel1DLateralDataType.FlowTimeSeries;
                DataTableHelper.SetTableToFunction(sobekLateralFlow.FlowTimeTable, waterFlowModel1DLateralSourceData.Data);

                if (isDiffuse)
                {
                    log.WarnFormat("The data of diffuse lateral source {0} has been changed from m2/s to m3/s (* length {1})", sobekLateralFlow.Id, length);
                    var values = waterFlowModel1DLateralSourceData.Data.Components[0].GetValues<double>();
                    for (int i = 0; i < values.Count; i++)
                    {
                        waterFlowModel1DLateralSourceData.Data.Components[0].Values[i] = values[i] * length;
                    }
                }
                waterFlowModel1DLateralSourceData.Data.Arguments[0].InterpolationType = sobekLateralFlow.InterpolationType;
                waterFlowModel1DLateralSourceData.Data.Arguments[0].ExtrapolationType = sobekLateralFlow.ExtrapolationType;
            }
            else if (sobekLateralFlow.LevelQhTable != null)
            {
                waterFlowModel1DLateralSourceData.DataType = WaterFlowModel1DLateralDataType.FlowWaterLevelTable;
                DataTableHelper.SetTableToFunction(sobekLateralFlow.LevelQhTable, waterFlowModel1DLateralSourceData.Data);

                if (isDiffuse)
                {
                    log.WarnFormat("The data of diffuse lateral source {0} has been changed from m2/s to m3/s (* length {1})", sobekLateralFlow.Id, length);
                    var values = waterFlowModel1DLateralSourceData.Data.Arguments[0].GetValues<double>();
                    for (int i = 0; i < values.Count; i++)
                    {
                        waterFlowModel1DLateralSourceData.Data.Arguments[0].Values[i] = values[i] * length;
                    }
                }

                waterFlowModel1DLateralSourceData.Data.Arguments[0].InterpolationType = InterpolationType.Linear;
                waterFlowModel1DLateralSourceData.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            }
            if (sobekLateralFlow.ExtrapolationType == ExtrapolationType.Periodic)
            {
                TimeSeriesHelper.SetPeriodicExtrapolationSobek(waterFlowModel1DLateralSourceData.Data, sobekLateralFlow.ExtrapolationPeriod);
            }
        }

        private static void UpdateFeatureGeometry(LateralSource lateralSource)
        {
            var lengthIndexedLine = new LengthIndexedLine(lateralSource.Branch.Geometry);
            lateralSource.Geometry = lengthIndexedLine.ExtractLine(lateralSource.Chainage, lateralSource.Chainage + lateralSource.Length);
        }
    }
}
