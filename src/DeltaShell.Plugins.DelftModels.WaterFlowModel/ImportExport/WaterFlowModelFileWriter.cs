using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using DeltaShell.Plugins.NetworkEditor;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class WaterFlowModel1DFileWriter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModel1DFileWriter));
        
        public static void Write(string targetModelFile, WaterFlowModel1D waterFlowModel1D)
        {
            FileUtils.DeleteIfExists(targetModelFile);
            var fileName = new ModelFileNames(targetModelFile);

            FileUtils.CreateDirectoryIfNotExists(fileName.TargetPath);

            ThrowIfFileNotExists(fileName.CrossSectionDefinitions, fileName.TargetPath, p => CrossSectionDefinitionFileWriter.WriteFile(p, waterFlowModel1D));
            ThrowIfFileNotExists(fileName.CrossSectionLocations, fileName.TargetPath, p => LocationFileWriter.WriteFileCrossSectionLocations(p, waterFlowModel1D.Network.CrossSections));
            ThrowIfFileNotExists(fileName.ObservationPoints, fileName.TargetPath, p => LocationFileWriter.WriteFileObservationPointLocations(p, waterFlowModel1D.Network.ObservationPoints));
            ThrowIfFileNotExists(fileName.LateralDischarge, fileName.TargetPath, p => LocationFileWriter.WriteFileLateralDischargeLocations(p, waterFlowModel1D.Network.LateralSources));
            ThrowIfFileNotExists(fileName.BoundaryLocations, fileName.TargetPath, p => BoundaryLocationFileWriter.WriteFile(p, waterFlowModel1D));
            ThrowIfFileNotExists(fileName.Structures, fileName.TargetPath, p => StructureFileWriter.WriteFile(p, waterFlowModel1D));
            ThrowIfFileNotExists(fileName.Network, fileName.TargetPath, p => NetworkAndGridWriter.WriteFile(p, waterFlowModel1D.Network, waterFlowModel1D.NetworkDiscretization));

            #region Write network and computational grid in ugrid

            UGridGlobalMetaData metaData = new UGridGlobalMetaData("NetworkGeneratedInWaterFlowModel1D", Properties.Resources.WaterFlowModel1DApplicationPlugin_DisplayName_D_Flow1D_Plugin, typeof(WaterFlowModel1D).Assembly.GetName().Version.ToString());

            UGridToNetworkAdapter.SaveNetwork(waterFlowModel1D.Network, fileName.NetCdf, metaData);
            UGridToNetworkAdapter.SaveNetworkDiscretisation(waterFlowModel1D.NetworkDiscretization, fileName.NetCdf);

            #endregion

            waterFlowModel1D.WriteSpatialData(fileName.TargetPath);

            ThrowIfFileNotExists(fileName.BoundaryConditions, fileName.TargetPath, p => WaterFlowModel1DBoundaryFileWriter.WriteFile(p, waterFlowModel1D));

            var writtenRoughessFiles = new List<string>();
            
            foreach (var roughnessSection in waterFlowModel1D.RoughnessSections)
            {
                var filename = "roughness-" + roughnessSection.Name + ".ini";
                var roughnessFilename = Path.Combine(fileName.TargetPath, filename);

                ThrowIfFileNotExists(roughnessFilename, fileName.TargetPath, p => RoughnessDataFileWriter.WriteFile(p, roughnessSection));//Add subPath!!
                writtenRoughessFiles.Add(filename);
            }

            ModelDefinitionFileWriter.RoughnessFiles = string.Join(";", writtenRoughessFiles);
            WriteSobekSimIni(fileName.SobekSim);

            RetentionFileWriter.WriteFile(fileName.Retention, waterFlowModel1D.Network.Retentions);

            ThrowIfFileNotExists(targetModelFile, fileName.TargetPath, p => ModelDefinitionFileWriter.WriteFile(p, waterFlowModel1D));

            if (waterFlowModel1D.SalinityValidNonConstantFormulation)
            {
                var salinityFilename = Path.GetFileName(waterFlowModel1D.SalinityPath);
                if (!File.Exists(waterFlowModel1D.SalinityPath) || string.IsNullOrEmpty(salinityFilename))
                    throw new FileWritingException(string.Format("{0} is not at location {1}.", salinityFilename, waterFlowModel1D.SalinityPath));

                fileName.Salinity = salinityFilename;

                ThrowIfFileNotExists(waterFlowModel1D.SalinityPath, waterFlowModel1D.SalinityPath,
                    p => WaterFlowModel1DSalinityIniWriter.WriteFile(p, fileName.Salinity,waterFlowModel1D.DispersionFormulationType));
            }

            if (waterFlowModel1D.UseMorphology)
            {
                CopyMorphologyFilesToRunDir(waterFlowModel1D, fileName.TargetPath);
            }

            FileUtils.CreateDirectoryIfNotExists(Path.Combine(fileName.TargetPath, "output"), true);
        }

        private static void ThrowIfFileNotExists(string filePath, string fileNameTargetPath, Action<string> writeAction)
        {
            writeAction(filePath);

            if (File.Exists(filePath)) return;
            throw new FileWritingException(string.Format("{0} is not written at location {1}.", Path.GetFileName(filePath), fileNameTargetPath));
        }

        private static void CopyMorphologyFilesToRunDir(WaterFlowModel1D waterFlowModel1D, string targetPath)
        {
            if (File.Exists(waterFlowModel1D.MorphologyPath))
            {
                // morphologyFile
                var morphologyFileInfo = new FileInfo(waterFlowModel1D.MorphologyPath);
                FileUtils.CopyFile(waterFlowModel1D.MorphologyPath, Path.Combine(targetPath, morphologyFileInfo.Name));

                // bcmFile
                if (File.Exists(waterFlowModel1D.BcmPath))
                {
                    var morphologyFilePathUri = new Uri(waterFlowModel1D.MorphologyPath);
                    var bcmFilePathUri = new Uri(waterFlowModel1D.BcmPath);
                    var relativePathToBcmFile = morphologyFilePathUri.MakeRelativeUri(bcmFilePathUri).OriginalString;

                    var copyBcmFileInfo = new FileInfo(Path.Combine(targetPath, relativePathToBcmFile));
                    FileUtils.CreateDirectoryIfNotExists(copyBcmFileInfo.DirectoryName);
                    FileUtils.CopyFile(waterFlowModel1D.BcmPath, copyBcmFileInfo.FullName);
                }
                else
                {
                    Log.WarnFormat("Could not copy .BCM File (morphology) to run directory, file does not exist");
                }
            }
            
            if (File.Exists(waterFlowModel1D.SedimentPath))
            {
                // sedimentFile
                var sedimentFileInfo = new FileInfo(waterFlowModel1D.SedimentPath);
                FileUtils.CopyFile(waterFlowModel1D.SedimentPath, Path.Combine(targetPath, sedimentFileInfo.Name));

                // traFile
                if (File.Exists(waterFlowModel1D.TraPath))
                {
                    var sedimentFilePathUri = new Uri(waterFlowModel1D.SedimentPath);
                    var traFilePathUri = new Uri(waterFlowModel1D.TraPath);
                    var relativePathToTraFile = sedimentFilePathUri.MakeRelativeUri(traFilePathUri).OriginalString;

                    var copyTraFileInfo = new FileInfo(Path.Combine(targetPath, relativePathToTraFile));
                    FileUtils.CreateDirectoryIfNotExists(copyTraFileInfo.DirectoryName);
                    FileUtils.CopyFile(waterFlowModel1D.TraPath, copyTraFileInfo.FullName);
                }
                else
                {
                    Log.WarnFormat("Could not copy .TRA File (morphology) to run directory, file does not exist");
                }
            }
        }

        private static void WriteSobekSimIni(string sobekSimTargetFile)
        {
            var assemblyLocation = typeof(WaterFlowModel1D).Assembly.Location;
            Log.DebugFormat("WaterFlowModel1D plugin path: '{0}'", assemblyLocation);
            var directoryInfo = new FileInfo(assemblyLocation).Directory;

            if (directoryInfo != null)
            {
                var path = directoryInfo.FullName;
                var dataZipFile = Path.Combine(path, "template.zip");
                // copy template model files
                if (!File.Exists(dataZipFile))
                    throw new IOException(String.Format("Can't find model template file {0}", dataZipFile));
                var unzipPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                FileUtils.CreateDirectoryIfNotExists(unzipPath);
                ZipFileUtils.Extract(dataZipFile, unzipPath);
                var sobekSimIniFile = Path.Combine(unzipPath, "work", "sobeksim.ini");
                if (!File.Exists(sobekSimIniFile))
                    throw new IOException(String.Format("Can't find sobeksim ini file {0}", sobekSimIniFile));
                FileUtils.CopyFile(sobekSimIniFile, sobekSimTargetFile);
                if (!File.Exists(sobekSimTargetFile))
                    throw new FileWritingException(String.Format("Can't copy sobeksim ini file from {0} to {1}", sobekSimIniFile, sobekSimTargetFile));
                var sobekSimFnmFile = Path.Combine(unzipPath, "work", "sobeksim.fnm");
                if (!File.Exists(sobekSimFnmFile))
                    throw new IOException(String.Format("Can't find sobeksim fnm file {0}", sobekSimFnmFile));
                var sobekSimFnmTargetFile = Path.Combine(Path.GetDirectoryName(sobekSimTargetFile), "sobeksim.fnm");
                FileUtils.CopyFile(sobekSimFnmFile, sobekSimFnmTargetFile);
                if (!File.Exists(sobekSimTargetFile))
                    throw new FileWritingException(String.Format("Can't copy sobeksim fnm file from {0} to {1}", sobekSimFnmFile, sobekSimFnmTargetFile));
                FileUtils.DeleteIfExists(unzipPath);
            }
            
        }

        private static void WriteSpatialData(this WaterFlowModel1D waterFlowModel1D, string targetPath)
        {
            switch (waterFlowModel1D.InitialConditionsType)
            {
                case InitialConditionsType.WaterLevel:
                {
                    var filename = Path.Combine(targetPath, SpatialDataFileNames.InitialWaterLevel);
                    SpatialDataFileWriter.WriteFile(filename, SpatialDataQuantity.InitialWaterLevel, waterFlowModel1D.InitialConditions);
                    if (!File.Exists(filename))
                        throw new FileWritingException(string.Format("{0} is not written at location {1}.", filename,
                            targetPath));
                    break;
                }
                case InitialConditionsType.Depth:
                {
                    var filename = Path.Combine(targetPath, SpatialDataFileNames.InitialWaterDepth);
                    SpatialDataFileWriter.WriteFile(filename, SpatialDataQuantity.InitialWaterDepth, waterFlowModel1D.InitialConditions);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (waterFlowModel1D.UseSalt) //TODO:For enkf optimisation -> useSaltInComputation ??
            {
                var saltFilename = Path.Combine(targetPath, SpatialDataFileNames.InitialSalinity);
                SpatialDataFileWriter.WriteFile(saltFilename, SpatialDataQuantity.InitialSalinity, waterFlowModel1D.InitialSaltConcentration);
                if (!File.Exists(saltFilename))
                    throw new FileWritingException(string.Format("{0} is not written at location {1}.", saltFilename, targetPath));

                var dispersionFilename = Path.Combine(targetPath, SpatialDataFileNames.Dispersion);
                SpatialDataFileWriter.WriteFile(dispersionFilename, SpatialDataQuantity.Dispersion, waterFlowModel1D.DispersionCoverage);
                if (!File.Exists(dispersionFilename))
                    throw new FileWritingException(string.Format("{0} is not written at location {1}.", dispersionFilename, targetPath));

                if (waterFlowModel1D.DispersionFormulationType != DispersionFormulationType.Constant)
                {
                    var dispersionF3Filename = Path.Combine(targetPath, SpatialDataFileNames.DispersionF3);
                    SpatialDataFileWriter.WriteFile(dispersionF3Filename, SpatialDataQuantity.Dispersion, waterFlowModel1D.DispersionF3Coverage);
                    if (!File.Exists(dispersionFilename))
                        throw new FileWritingException(string.Format("{0} is not written at location {1}.", dispersionF3Filename, targetPath));

                    var dispersionF4Filename = Path.Combine(targetPath, SpatialDataFileNames.DispersionF4);
                    SpatialDataFileWriter.WriteFile(dispersionF4Filename, SpatialDataQuantity.Dispersion, waterFlowModel1D.DispersionF4Coverage);
                    if (!File.Exists(dispersionFilename))
                        throw new FileWritingException(string.Format("{0} is not written at location {1}.", dispersionF4Filename, targetPath));
                }
            }

            var dischargeFilename = Path.Combine(targetPath, SpatialDataFileNames.InitialDischarge);
            SpatialDataFileWriter.WriteFile(dischargeFilename, SpatialDataQuantity.InitialDischarge, waterFlowModel1D.InitialFlow);
            if (!File.Exists(dischargeFilename))
                throw new FileWritingException(string.Format("{0} is not written at location {1}.", dischargeFilename, targetPath));

            var windShieldingFilename = Path.Combine(targetPath, SpatialDataFileNames.WindShielding);
            SpatialDataFileWriter.WriteFile(windShieldingFilename, SpatialDataQuantity.WindShielding, waterFlowModel1D.WindShielding);
            if (!File.Exists(windShieldingFilename))
                throw new FileWritingException(string.Format("{0} is not written at location {1}.", windShieldingFilename, targetPath));

            var initialTemperatureFilename = Path.Combine(targetPath, SpatialDataFileNames.InitialTemperature);
            SpatialDataFileWriter.WriteFile(initialTemperatureFilename, SpatialDataQuantity.InitialTemperature, waterFlowModel1D.InitialTemperature);
            if (!File.Exists(initialTemperatureFilename))
                throw new FileWritingException(string.Format("{0} is not written at location {1}.", initialTemperatureFilename, targetPath));

            if (waterFlowModel1D.UseRestart)
            {
                ZipFileUtils.Extract(waterFlowModel1D.RestartInput.Path, targetPath);
            }
        }
    }
}
