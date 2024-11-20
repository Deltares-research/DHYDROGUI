using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.TestUtils
{
    public class ModelFileNames
    {
        private readonly string targetPath;
        private string network;
        private string observationPoint;
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
        private string netCdf;

        private const string CrossSectionDefinitionFilename = "CrossSectionDefinitions.ini";
        private const string CrossSectionLocationFilename = "CrossSectionLocations.ini";
        private const string ObservationPointFilename = "ObservationPoints.ini";
        private const string LateralDischargeFilename = "LateralDischargeLocations.ini";
        private const string SalinityFilename = "Salinity.ini";
        private const string BoundaryLocationFilename = "BoundaryLocations.ini";
        private const string StructureFilename = "Structures.ini";
        private const string NetworkFilename = "NetworkDefinition.ini";
        private const string NetCfdFilename = "NetworkDefinition.nc";
        private const string SobekSimFilename = "SobekSim.ini";
        private const string RetentionFilename = "Retention.ini";
        private const string LogFileName = "sobek.log";
        private const string BoundaryConditionsFilename = "BoundaryConditions.bc";
        public const string ModelDefinitionFilename = "ModelDefinition.md1d";
        public const string ModelFilenameExtension = ".md1d";


        public IEventedList<string> RoughnessFiles { get; private set; }

        public ModelFileNames(string modelFilename) : this()
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
                catch (FileNotFoundException exception)
                {
                    //targetPath = Path.GetDirectoryName(modelFilename);
                    //the directory doesn't exist
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

            var filename = Path.Combine(targetPath, modelFilename);
            if (!File.Exists(filename))
                throw new FileReadingException(String.Format("Could not read file {0} properly, it doesn't exist.",
                    filename));
            var iniSections = new IniReader().ReadIniFile(filename);
            if (iniSections.Count == 0)
                throw new FileReadingException(String.Format("Could not read file {0} properly, it seems empty",
                    filename));
            var fileSection =
                iniSections.Where(iniSection => iniSection.Name == ModelDefinitionsRegion.FilesIniHeader).ToList();
            if (fileSection.Count() > 1 && fileSection.Any())
                throw new FileReadingException(String.Format("Could not read files section {0} properly", filename));

            Network = fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.NetworkFile.Key);
            Structures = fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.StructuresFile.Key);
            ObservationPoints = fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.ObservationPointsFile.Key);
            LateralDischarge =
                fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.LateralDischargeLocationsFile.Key);
            BoundaryConditions =
                fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.BoundaryConditionsFile.Key);
            BoundaryLocations =
                fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.BoundaryLocationsFile.Key);
            CrossSectionDefinitions =
                fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.CrossSectionDefinitionsFile.Key);
            CrossSectionLocations =
                fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.CrossSectionLocationsFile.Key);
            Retention =
                fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.RetentionFile.Key);
            SobekSim =
                fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.SobekSimIniFile.Key);

            LogFile =
                fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.LogFile.Key);

            Salinity =
                fileSection[0].ReadProperty<string>(ModelDefinitionsRegion.SalinityParametersFile.Key, true);


            var readRoughnessFiles =
                fileSection[0].ReadPropertiesToListOfType<string>(ModelDefinitionsRegion.RoughnessFile.Key,
                    customSeparator: ';', isOptional: true);
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
            NetCdf = NetCfdFilename;
            RoughnessFiles = new EventedList<string>();
            RoughnessFiles.CollectionChanged += RoughnessFiles_CollectionChanged;
        }

        void RoughnessFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var files = sender as IList<string>;
                if (files == null)
                    throw new FileReadingException("Reading roughness files list from md1d file went wrong.");
                files[files.Count - 1] = Path.Combine(targetPath, e.GetRemovedOrAddedItem().ToString());
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

        public string LateralDischarge
        {
            get { return Path.Combine(targetPath, lateralDischarge); }
            private set { lateralDischarge = value; }
        }

        public string Salinity
        {
            get { return Path.Combine(targetPath, salinity); }
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

        public string NetCdf
        {
            get { return Path.Combine(targetPath, netCdf); }
            private set { netCdf = value; }
        }
    }
}