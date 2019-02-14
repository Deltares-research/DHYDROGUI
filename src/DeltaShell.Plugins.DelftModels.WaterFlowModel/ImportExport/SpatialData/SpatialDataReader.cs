using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.FileReaders.SpatialData;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.SpatialData
{
    public class SpatialDataReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SpatialDataReader));

        public static void ReadSpatialData(IEnumerable<string> filePaths, WaterFlowModel1D model,
            Action<string, IList<string>> createAndAddErrorReport)
        {
            foreach (var filePath in filePaths)
            {
                ReadNetworkCoverage(filePath, model, createAndAddErrorReport);
            }
        }

        private static void ReadNetworkCoverage(string filePath, WaterFlowModel1D model,
            Action<string, IList<string>> createAndAddErrorReport)
        {
            var networkCoverageFileReader = new NetworkCoverageFileReader(createAndAddErrorReport);
            var networkCoverage = networkCoverageFileReader.ReadSpatialFileData(filePath, model.Network.Channels.ToList());

            if(networkCoverage != null) SetModelSpatialDataOnModel(filePath, model, networkCoverage);
        }

        private static void SetModelSpatialDataOnModel(string filePath, WaterFlowModel1D model, IFunction spatialFileData)
        {
            switch (Path.GetFileName(filePath))
            {
                case SpatialDataFileNames.InitialDischarge:
                    CopySpatialFileDataToModel(model.InitialFlow, spatialFileData);
                    break;
                case SpatialDataFileNames.InitialSalinity:
                    CopySpatialFileDataToModel(model.InitialSaltConcentration, spatialFileData);
                    break;
                case SpatialDataFileNames.InitialTemperature:
                    CopySpatialFileDataToModel(model.InitialTemperature, spatialFileData);
                    break;
                case SpatialDataFileNames.InitialWaterLevel:
                    if (model.InitialConditionsType == InitialConditionsType.WaterLevel)
                        CopySpatialFileDataToModel(model.InitialConditions, spatialFileData);
                    break;
                case SpatialDataFileNames.InitialWaterDepth:
                    if (model.InitialConditionsType == InitialConditionsType.Depth)
                        CopySpatialFileDataToModel(model.InitialConditions, spatialFileData);
                    break;
                case SpatialDataFileNames.Dispersion:
                    CopySpatialFileDataToModel(model.DispersionCoverage, spatialFileData);
                    break;
                case SpatialDataFileNames.DispersionF3:
                    CopySpatialFileDataToModel(model.DispersionF3Coverage, spatialFileData);
                    break;
                case SpatialDataFileNames.DispersionF4:
                    CopySpatialFileDataToModel(model.DispersionF4Coverage, spatialFileData);
                    break;
                case SpatialDataFileNames.WindShielding:
                    CopySpatialFileDataToModel(model.WindShielding, spatialFileData);
                    break;
                default:
                    Log.Warn(string.Format(Resources.SpatialDataReader_SetModelSpatialDataOnModel_Could_not_find_any_spatial_data_to_set_on_the_model__The_file___0__does_not_have_a_correct_name_,
                        filePath));
                    break;
            }
        }

        private static void CopySpatialFileDataToModel(IFunction copyTo, IFunction copyFrom)
        {
            if (copyTo == null || copyFrom == null) return;
            copyTo.Arguments[0].SetValues(copyFrom.Arguments[0].Values);
            copyTo.Components[0].SetValues(copyFrom.Components[0].Values);
        }
    }
}