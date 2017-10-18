using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class ModelDefinitionFileWriter
    {
        public static string RoughnessFiles { get; set; }
       
        public static void WriteFile(string targetFilename, WaterFlowModel1D waterFlowModel1D)
        {
            var targetPath = Path.GetDirectoryName(targetFilename);
            if (targetPath == null) return;

            var useSalt = waterFlowModel1D != null && waterFlowModel1D.UseSalt;
            var useThatcherHarleman = waterFlowModel1D != null && waterFlowModel1D.DispersionFormulationType != DispersionFormulationType.Constant;

            var salinityPath = waterFlowModel1D != null && waterFlowModel1D.SalinityValidNonConstantFormulation 
                ? WaterFlowModel1D.SalinityFileName
                : null;

            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.ModelDefinitionsMajorVersion, 
                                                             GeneralRegion.ModelDefinitionsMinorVersion, 
                                                             GeneralRegion.FileTypeName.ModelDefinition),
                
                GenerateFilesRegion(useSalt, useThatcherHarleman, salinityPath)
            };

            if (waterFlowModel1D != null && waterFlowModel1D.OutputSettings != null)
            {
                var outputSettings = waterFlowModel1D.OutputSettings.GenerateAdvancedOptionsValues();
                if (outputSettings != null)
                {
                    outputSettings.ForEach(lc => lc.Properties = lc.Properties.OrderBy(p => p.Name).ToList());
                    categories.AddRange(outputSettings);   // Order the properties by name, just for consistency of the files. 
                }
            }

            Flow1DParameterGenerator parameterGenerator = new Flow1DParameterGenerator();
            IEnumerable<DelftIniCategory> parameterCategories = parameterGenerator.GenerateParameterCategories(waterFlowModel1D);
            if (parameterCategories != null)
            {
                categories.AddRange(parameterCategories);
            }

            MoveSalinityParameterToBranchResults(categories);
            
            var fileName = waterFlowModel1D == null ? Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename) : targetFilename;
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            new IniFileWriter().WriteIniFile(categories, fileName, true);
        }

        private static void MoveSalinityParameterToBranchResults(List<DelftIniCategory> categories)
        {
            /* CS2016: 
             * To avoid messing up with the internal definition and category of the parameters, we can here
             * reorder certain parameters as well as rename them, so that in the output file *.md1d they 
             * are displayed correctly. 
             * BC: But why not change the EngineParameter?? */
            
            var resultsNodes = categories.FirstOrDefault(sg => sg.Name.Equals("ResultsNodes"));
            var resultBranches = categories.FirstOrDefault(p => p.Name.Equals("ResultsBranches"));

            if (resultsNodes != null)
            {
                var salinityDisp = resultsNodes.Properties.FirstOrDefault(sgP => sgP.Name.Equals("Dispersion"));
                if (salinityDisp != null)
                {
                    if (resultBranches != null)
                    {
                        resultBranches.AddProperty(salinityDisp);
                        resultsNodes.RemoveProperty(salinityDisp);
                    }
                }
                
                //Change name of QTotal_1d2d to lateral1d2d
                var qTotal = resultsNodes.Properties.FirstOrDefault(p => p.Name.Equals("QTotal_1d2d"));
                if (qTotal != null)
                {
                    qTotal.Name = "Lateral1D2D";
                }
            }
        }
        
        private static DelftIniCategory GenerateFilesRegion(bool useSalt, bool useThatcherHarleman, string salinityPath)
        {
            var filesRegion = new DelftIniCategory(ModelDefinitionsRegion.FilesIniHeader);

            var modelFileNames = new ModelFileNames();
             
            filesRegion.AddProperty(ModelDefinitionsRegion.NetworkFile.Key, modelFileNames.Network, ModelDefinitionsRegion.NetworkFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.CrossSectionLocationsFile.Key, modelFileNames.CrossSectionLocations, ModelDefinitionsRegion.CrossSectionLocationsFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.CrossSectionDefinitionsFile.Key, modelFileNames.CrossSectionDefinitions, ModelDefinitionsRegion.CrossSectionDefinitionsFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.StructuresFile.Key, modelFileNames.Structures, ModelDefinitionsRegion.StructuresFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.ObservationPointsFile.Key, modelFileNames.ObservationPoints, ModelDefinitionsRegion.ObservationPointsFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.InitialWaterLevelFile.Key, SpatialDataFileNames.InitialWaterLevel, ModelDefinitionsRegion.InitialWaterLevelFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.InitialWaterDepthFile.Key, SpatialDataFileNames.InitialWaterDepth, ModelDefinitionsRegion.InitialWaterDepthFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.InitialDischargeFile.Key, SpatialDataFileNames.InitialDischarge, ModelDefinitionsRegion.InitialDischargeFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.InitialSalinityFile.Key, SpatialDataFileNames.InitialSalinity, ModelDefinitionsRegion.InitialSalinityFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.InitialTemperatureFile.Key, SpatialDataFileNames.InitialTemperature, ModelDefinitionsRegion.InitialTemperatureFile.Description);

            if (useSalt)
            {
                filesRegion.AddProperty(ModelDefinitionsRegion.DispersionFile.Key, SpatialDataFileNames.Dispersion, ModelDefinitionsRegion.DispersionFile.Description);

                if (useThatcherHarleman)
                {
                    filesRegion.AddProperty(ModelDefinitionsRegion.DispersionF3File.Key, SpatialDataFileNames.DispersionF3, ModelDefinitionsRegion.DispersionF3File.Description);
                    filesRegion.AddProperty(ModelDefinitionsRegion.DispersionF4File.Key, SpatialDataFileNames.DispersionF4, ModelDefinitionsRegion.DispersionF4File.Description);

                    if (!string.IsNullOrEmpty(salinityPath))
                    {
                        filesRegion.AddProperty(ModelDefinitionsRegion.SalinityParametersFile.Key, salinityPath, ModelDefinitionsRegion.SalinityParametersFile.Description);
                    }
                }
            }
            
            filesRegion.AddProperty(ModelDefinitionsRegion.WindShieldingFile.Key, SpatialDataFileNames.WindShielding, ModelDefinitionsRegion.WindShieldingFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.RoughnessFile.Key, RoughnessFiles ?? "", ModelDefinitionsRegion.RoughnessFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.BoundaryLocationsFile.Key, modelFileNames.BoundaryLocations, ModelDefinitionsRegion.BoundaryLocationsFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.LateralDischargeLocationsFile.Key, modelFileNames.LateralDischarge, ModelDefinitionsRegion.LateralDischargeLocationsFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.BoundaryConditionsFile.Key, modelFileNames.BoundaryConditions, ModelDefinitionsRegion.BoundaryConditionsFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.SobekSimIniFile.Key, modelFileNames.SobekSim, ModelDefinitionsRegion.SobekSimIniFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.RetentionFile.Key, modelFileNames.Retention, ModelDefinitionsRegion.RetentionFile.Description);
            filesRegion.AddProperty(ModelDefinitionsRegion.LogFile.Key, modelFileNames.LogFile, ModelDefinitionsRegion.LogFile.Description);
            
            return filesRegion;
        }
    }

    
}
