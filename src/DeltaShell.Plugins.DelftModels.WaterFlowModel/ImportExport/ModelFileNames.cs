using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class ModelFileNames
    {
        private readonly HashSet<string> knownPropertyNames = new HashSet<string>
        {
            ModelDefinitionsRegion.NetworkFile.Key,
            ModelDefinitionsRegion.StructuresFile.Key,
            ModelDefinitionsRegion.ObservationPointsFile.Key,
            ModelDefinitionsRegion.LateralDischargeLocationsFile.Key,
            ModelDefinitionsRegion.BoundaryConditionsFile.Key,
            ModelDefinitionsRegion.BoundaryLocationsFile.Key,
            ModelDefinitionsRegion.CrossSectionDefinitionsFile.Key,
            ModelDefinitionsRegion.CrossSectionLocationsFile.Key,
            ModelDefinitionsRegion.RetentionFile.Key,
            ModelDefinitionsRegion.SobekSimIniFile.Key,
            ModelDefinitionsRegion.InitialDischargeFile.Key,
            ModelDefinitionsRegion.InitialSalinityFile.Key,
            ModelDefinitionsRegion.InitialTemperatureFile.Key,
            ModelDefinitionsRegion.InitialWaterLevelFile.Key,
            ModelDefinitionsRegion.InitialWaterDepthFile.Key,
            ModelDefinitionsRegion.DispersionFile.Key,
            ModelDefinitionsRegion.DispersionF3File.Key,
            ModelDefinitionsRegion.DispersionF4File.Key,
            ModelDefinitionsRegion.WindShieldingFile.Key,
            ModelDefinitionsRegion.LogFile.Key,
            ModelDefinitionsRegion.SalinityParametersFile.Key,
            ModelDefinitionsRegion.RoughnessFile.Key
        };

        private readonly string targetPath;
        private string network;
        private string observationPoint;
        private string initialDischarge;
        private string initialSalinity;
        private string initialTemperature;
        private string initialWaterLevel;
        private string initialWaterDepth;
        private string dispersion;
        private string dispersionF3;
        private string dispersionF4;
        private string windShielding;
        private string lateralDischarge;
        private string salinity;
        private string boundaryConditions;
        private string crossSectionDefinitions;
        private string crossSectionLocations;
        private string sobekSim;
        private string retention;
        private string logFile;
        private string boundaryLocations;
        private string structures;
        
        private const string CrossSectionDefinitionFilename = "CrossSectionDefinitions.ini";
        private const string CrossSectionLocationFilename = "CrossSectionLocations.ini";
        private const string ObservationPointFilename = "ObservationPoints.ini";
        private const string InitialDischargeFilename = "InitialDischarge.ini";
        private const string InitialSalinityFilename = "InitialSalinity.ini";
        private const string InitialTemperatureFilename = "InitialTemperature.ini";
        private const string InitialWaterLevelFilename = "InitialWaterLevel.ini";
        private const string InitialWaterDepthFilename = "InitialWaterDepth.ini";
        private const string DispersionFilename = "Dispersion.ini";
        private const string DispersionF3Filename = "DispersionF3.ini";
        private const string DispersionF4Filename = "DispersionF4.ini";
        private const string WindShieldingFilename = "WindShielding.ini";
        private const string LateralDischargeFilename = "LateralDischargeLocations.ini";
        private const string SalinityFilename = "Salinity.ini";
        private const string BoundaryLocationFilename = "BoundaryLocations.ini";
        private const string StructureFilename = "Structures.ini";
        private const string NetworkFilename = "NetworkDefinition.ini";
        private const string SobekSimFilename = "SobekSim.ini";
        private const string RetentionFilename = "Retention.ini";
        private const string LogFileName = "sobek.log";
        private const string BoundaryConditionsFilename = "BoundaryConditions.bc"; 
        public const string ModelDefinitionFilename = "ModelDefinition.md1d";
        public const string ModelFilenameExtension = ".md1d";
        
        public IEventedList<string> RoughnessFiles { get; private set; }
        
        public ModelFileNames(string modelFilename, Action<string, IList<string>> createAndAddErrorReport = null) : this()
        {
            if (!File.Exists(modelFilename))
            {
                if (Path.GetExtension(modelFilename) == ModelFilenameExtension)
                {
                    targetPath = Path.GetDirectoryName(modelFilename);
                    FileUtils.CreateDirectoryIfNotExists(targetPath);
                    return;
                }
                //see if it is just a directory
                try
                {
                    if ((File.GetAttributes(modelFilename) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        targetPath = modelFilename;
                        return;
                    }
                }
                catch (FileNotFoundException)
                {
                    FileUtils.CreateDirectoryIfNotExists(modelFilename);
                    targetPath = modelFilename;
                    return;
                }
                catch
                {
                    return;
                }
            }
            else
            {
                if ((File.GetAttributes(modelFilename) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    targetPath = modelFilename;
                    return;
                }
            }

            targetPath = Path.GetDirectoryName(modelFilename);
            modelFilename = Path.GetFileName(modelFilename);

            if (targetPath == null || modelFilename == null) return;

            var modelDefinitionFileName = Path.Combine(targetPath, modelFilename);
            if (!File.Exists(modelDefinitionFileName))
                throw new FileReadingException(string.Format("Could not read file {0} properly, it doesn't exist.",
                    modelDefinitionFileName));
            var categories = new DelftIniReader().ReadDelftIniFile(modelDefinitionFileName);
            if (categories.Count == 0)
                throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty",
                    modelDefinitionFileName));
            var fileSection = categories.Where(category => category.Name == ModelDefinitionsRegion.FilesIniHeader).ToArray();
            if (fileSection.Length != 1)
                throw new FileReadingException(string.Format("Could not read files section {0} properly", modelDefinitionFileName));

            try
            {
                var filesNamesCategory = fileSection.FirstOrDefault();
                ReadMandatoryFileNames(filesNamesCategory);
                ReadOptionalFileNames(filesNamesCategory);
                ValidateFileNames(filesNamesCategory, modelDefinitionFileName, createAndAddErrorReport);
            }
            catch (PropertyNotFoundInFileException e)
            {
                var errorMessage = string.Join(" ", e.Message, $"'{modelDefinitionFileName}' under category '{ModelDefinitionsRegion.FilesIniHeader}'.");
                throw new PropertyNotFoundInFileException(errorMessage);
            }
        }

        private void ValidateFileNames(IDelftIniCategory filesNamesCategory, string modelFilename, Action<string, IList<string>> createAndAddErrorReport)
        {
            if(createAndAddErrorReport == null) return;

            var warningMessages = new List<string>();
            var unsupportedProperties = filesNamesCategory.Properties.Where(p => !knownPropertyNames.Contains(p.Name));
            unsupportedProperties.ForEach(property =>
            {
                var warningMessage = 
                    string.Format(Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                        property.LineNumber, property.Name);
                warningMessages.Add(warningMessage);
            });

            createAndAddErrorReport.Invoke($"While reading the file names from file '{modelFilename}', the following warnings occured", warningMessages);
        }

        private void ReadMandatoryFileNames(IDelftIniCategory filesNamesCategory)
        {
            Network = filesNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.NetworkFile.Key);
            Structures = filesNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.StructuresFile.Key);
            ObservationPoints = filesNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.ObservationPointsFile.Key);
            LateralDischarge = filesNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.LateralDischargeLocationsFile.Key);
            BoundaryConditions = filesNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.BoundaryConditionsFile.Key);
            BoundaryLocations = filesNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.BoundaryLocationsFile.Key);
            CrossSectionDefinitions = filesNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.CrossSectionDefinitionsFile.Key);
            CrossSectionLocations = filesNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.CrossSectionLocationsFile.Key);
            Retention = filesNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.RetentionFile.Key);
            SobekSim = filesNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.SobekSimIniFile.Key);
        }

        private void ReadOptionalFileNames(IDelftIniCategory fileNamesCategory)
        {
            InitialDischarge = fileNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.InitialDischargeFile.Key, true);
            InitialSalinity = fileNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.InitialSalinityFile.Key, true);
            InitialTemperature = fileNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.InitialTemperatureFile.Key, true);
            InitialWaterLevel = fileNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.InitialWaterLevelFile.Key, true);
            InitialWaterDepth = fileNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.InitialWaterDepthFile.Key, true);
            Dispersion = fileNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.DispersionFile.Key, true);
            DispersionF3 = fileNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.DispersionF3File.Key, true);
            DispersionF4 = fileNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.DispersionF4File.Key, true);
            WindShielding = fileNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.WindShieldingFile.Key, true);
            LogFile = fileNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.LogFile.Key, true);
            Salinity = fileNamesCategory.ReadProperty<string>(ModelDefinitionsRegion.SalinityParametersFile.Key, true);

            var readRoughnessFiles = fileNamesCategory.ReadPropertiesToListOfType<string>(ModelDefinitionsRegion.RoughnessFile.Key, separator: ';', isOptional: true);
            if (readRoughnessFiles == null) return;
            foreach (var file in readRoughnessFiles)
            {
                RoughnessFiles.Add(file);
            }
        }

        public ModelFileNames()
        {
            targetPath = String.Empty;
            Network = NetworkFilename;
            ObservationPoints = ObservationPointFilename;
            InitialDischarge = InitialDischargeFilename;
            InitialSalinity = InitialSalinityFilename;
            InitialTemperature = InitialTemperatureFilename;
            InitialWaterLevel = InitialWaterLevelFilename;
            InitialWaterDepth = InitialWaterDepthFilename;
            Dispersion = DispersionFilename;
            DispersionF3 = DispersionF3Filename;
            DispersionF4 = DispersionF4Filename;
            WindShielding = WindShieldingFilename;
            LateralDischarge = LateralDischargeFilename;
            Salinity = SalinityFilename;
            BoundaryConditions = BoundaryConditionsFilename;
            BoundaryLocations = BoundaryLocationFilename;
            CrossSectionDefinitions = CrossSectionDefinitionFilename;
            CrossSectionLocations = CrossSectionLocationFilename;
            SobekSim = SobekSimFilename;
            Retention = RetentionFilename;
            LogFile = LogFileName;
            Structures = StructureFilename;
            RoughnessFiles = new EventedList<string>();
            RoughnessFiles.CollectionChanged += RoughnessFiles_CollectionChanged;
        }

        private void RoughnessFiles_CollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Add)
            {
                var files = sender as IList<string>;
                if (files == null) 
                    throw new FileReadingException("Reading roughness files list from md1d file went wrong.");
                files[files.Count-1] = Path.Combine(targetPath, e.Item.ToString());
            }
        }
        
        public string Network
        {
            get { return Path.Combine(targetPath, network); }
            private set { network = value; }
        }

        public string ObservationPoints
        {
            get { return Path.Combine(targetPath, observationPoint); }
            private set { observationPoint = value; }
        }

        public string InitialDischarge
        {
            get
            {
                return initialDischarge == null ? null : Path.Combine(targetPath, initialDischarge);
            }
            private set { initialDischarge = value; }
        }

        public string InitialSalinity
        {
            get
            {
                return initialSalinity == null ? null : Path.Combine(targetPath, initialSalinity);
            }
            private set { initialSalinity = value; }
        }

        public string InitialTemperature
        {
            get
            {
                return initialTemperature == null ? null : Path.Combine(targetPath, initialTemperature);
            }
            private set { initialTemperature = value; }
        }

        public string InitialWaterLevel
        {
            get
            {
                return initialWaterLevel == null ? null : Path.Combine(targetPath, initialWaterLevel);
            }
            private set { initialWaterLevel = value; }
        }
        public string InitialWaterDepth
        {
            get
            {
                return initialWaterDepth == null ? null : Path.Combine(targetPath, initialWaterDepth);
            }
            private set { initialWaterDepth = value; }
        }

        public string Dispersion
        {
            get
            {
                return dispersion == null ? null : Path.Combine(targetPath, dispersion);
            }
            private set { dispersion = value; }
        }

        public string DispersionF3
        {
            get
            {
                return dispersionF3 == null ? null : Path.Combine(targetPath, dispersionF3);
            }
            private set { dispersionF3 = value; }
        }

        public string DispersionF4
        {
            get
            {
                return dispersionF4 == null ? null : Path.Combine(targetPath, dispersionF4);
            }
            private set { dispersionF4 = value; }
        }
        public string WindShielding
        {
            get
            {
                return windShielding == null ? null : Path.Combine(targetPath, windShielding);
            }
            private set { windShielding = value; }
        }

        public string LateralDischarge
        {
            get { return Path.Combine(targetPath, lateralDischarge); }
            private set { lateralDischarge = value; }
        }

        public string Salinity
        {
            get { return salinity == null ? null : Path.Combine(targetPath, salinity); }
            set { salinity = value; }
        }

        public string BoundaryConditions
        {
            get { return Path.Combine(targetPath, boundaryConditions); }
            private set { boundaryConditions = value; }
        }

        public string CrossSectionDefinitions
        {
            get { return Path.Combine(targetPath, crossSectionDefinitions); }
            private set { crossSectionDefinitions = value; }
        }

        public string CrossSectionLocations
        {
            get { return Path.Combine(targetPath, crossSectionLocations); }
            private set { crossSectionLocations = value; }
        }

        public string SobekSim
        {
            get { return Path.Combine(targetPath, sobekSim); }
            private set { sobekSim = value; }
        }
        public string Retention
        {
            get { return Path.Combine(targetPath, retention); }
            private set { retention = value; }
        }

        public string LogFile
        {
            get { return Path.Combine(targetPath, logFile); }
            private set { logFile = value; }
        }

        public string BoundaryLocations
        {
            get { return Path.Combine(targetPath, boundaryLocations); }
            private set { boundaryLocations = value; }
        }

        public string Structures
        {
            get { return Path.Combine(targetPath, structures); }
            private set { structures = value; }
        }

        public string TargetPath { get { return targetPath; } }
    }
}