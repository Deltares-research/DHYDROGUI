using System;
using System.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    // TODO: this needs to be called from an integration test

    public static class WaterFlowModel1DFileReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModel1DFileReader));
        
        public static WaterFlowModel1D Read(string modelFilename, Action<string, int, int> reportProgress = null)
        {
            reportProgress = reportProgress ?? ((s, c, t) => { });
            WaterFlowModel1D model = new WaterFlowModel1D();
            
            try
            {
                const int totalSteps = 7;
                reportProgress($"Reading filenames from {Path.GetFileName(modelFilename)} file.", 1, totalSteps);
                var fileName = new ModelFileNames(modelFilename);

                var networkDefinitionFilePath = fileName.Network;
                reportProgress($"Reading network from {networkDefinitionFilePath} file.", 2, totalSteps);
                ReadNetworkDefinitionFile(networkDefinitionFilePath, model);

                reportProgress($"Reading lateral discharge locations from {fileName.LateralDischarge} file.", 3, totalSteps); 
                LocationFileReader.ReadFileLateralDischargeLocations(fileName.LateralDischarge, model.Network);
                
                reportProgress($"Reading boundary conditions and lateral sources from {fileName.BoundaryConditions} file.", 4, totalSteps); 
                BoundaryFileReader.ReadFile(fileName.BoundaryConditions, model);

                reportProgress($"Reading observation points from {fileName.ObservationPoints} file.", 5, totalSteps);
                LocationFileReader.ReadFileObservationPointLocations(fileName.ObservationPoints, model.Network);

                var totalRoughnessFiles = fileName.RoughnessFiles.Count;
                var i = 1;
                if(totalRoughnessFiles > 0)
                    model.RoughnessSections.Clear();

                foreach (var roughnessFile in fileName.RoughnessFiles)
                {
                    reportProgress($"Reading roughness section from {roughnessFile} file. (reading roughness file {i} of {totalRoughnessFiles})", 6, totalSteps);
                    i++;
                    RoughnessDataFileReader.ReadFile(roughnessFile, model.Network, model.RoughnessSections);
                }
                reportProgress($"Reading cross sections from {fileName.CrossSectionLocations} file and {fileName.CrossSectionDefinitions}.", 7, totalSteps);
                CrossSectionFileReader.ReadFile(fileName.CrossSectionLocations, fileName.CrossSectionDefinitions, model);
            }
            catch (FileReadingException fileReadingException)
            {
                Log.Error(fileReadingException.Message);
                return null;
            }
            return model;
        }

        private static void ReadNetworkDefinitionFile(string networkDefinitionFilePath, WaterFlowModel1D model)
        {
            var networkDefinitionFileReader = new NetworkDefinitionFileReader();
            var nodes = networkDefinitionFileReader.ReadHydroNodes(networkDefinitionFilePath);
            model.Network.Nodes.AddRange(nodes);

            var branches = networkDefinitionFileReader.ReadBranches(networkDefinitionFilePath, model.Network);
            model.Network.Branches.AddRange(branches);

            var readNetworkLocations = networkDefinitionFileReader.ReadNetworkLocations(networkDefinitionFilePath, model.Network);
            model.NetworkDiscretization.Locations.Values.Add(readNetworkLocations);
        }
    }
}
