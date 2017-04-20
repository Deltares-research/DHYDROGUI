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
                reportProgress(String.Format("Reading filenames from {0} file.", Path.GetFileName(modelFilename)), 1, totalSteps);
                var fileName = new ModelFileNames(modelFilename);
                
                reportProgress(String.Format("Reading network from {0} file.", fileName.Network), 2, totalSteps);
                NetworkAndGridReader.ReadFile(fileName.Network, model.Network, model.NetworkDiscretization);
                
                reportProgress(String.Format("Reading lateral discharge locations from {0} file.", fileName.LateralDischarge), 3, totalSteps); 
                LocationFileReader.ReadFileLateralDischargeLocations(fileName.LateralDischarge, model.Network);
                
                reportProgress(String.Format("Reading boundary conditions and lateral sources from {0} file.", fileName.BoundaryConditions), 4, totalSteps); 
                BoundaryFileReader.ReadFile(fileName.BoundaryConditions, model);

                reportProgress(String.Format("Reading observation points from {0} file.", fileName.ObservationPoints), 5, totalSteps);
                LocationFileReader.ReadFileObservationPointLocations(fileName.ObservationPoints, model.Network);

                var totalRoughnessFiles = fileName.RoughnessFiles.Count;
                var i = 1;
                if(totalRoughnessFiles > 0)
                    model.RoughnessSections.Clear();
                foreach (var roughnessFile in fileName.RoughnessFiles)
                {
                    reportProgress(String.Format("Reading roughness section from {0} file. (reading roughness file {1} of {2})", roughnessFile, i, totalRoughnessFiles), 6, totalSteps);
                    i++;
                    RoughnessDataFileReader.ReadFile(roughnessFile, model.Network, model.RoughnessSections);    
                }
                reportProgress(String.Format("Reading cross sections from {0} file and {1}.", fileName.CrossSectionLocations, fileName.CrossSectionDefinitions), 7, totalSteps);
                CrossSectionFileReader.ReadFile(fileName.CrossSectionLocations, fileName.CrossSectionDefinitions, model);
            }
            catch (FileReadingException fileReadingException)
            {
                Log.Error(fileReadingException.Message);
                return null;
            }
            return model;
        }
    }
}
