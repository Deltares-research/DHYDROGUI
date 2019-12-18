using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Csv.Importer;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;
using PostSharp.Extensibility;

namespace DeltaShell.Plugins.ImportExport.Gwsw
{

    /// <summary>
    /// Importer for GWSW files
    /// </summary>
    /// <seealso cref="DelftTools.Shell.Core.IFileImporter" />
    public class GwswFileImporter : IFileImporter
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswFileImporter));

        private CsvSettings csvSettings;

        private class GwswImportManager
        {
            private int totalAmountOfImportSteps;
            private int currentImportStep;
            public int AmountOfImportStepsPerFile;

            public void CalculateTotalAmountOfImportSteps(IList<string> filesToImport)
            {
                var amountOfFiles = filesToImport.Count;
                totalAmountOfImportSteps = amountOfFiles * AmountOfImportStepsPerFile + 1;
            }

            public void ReportProgress(string message)
            {
                currentImportStep++;
                new GwswFileImporter().SetProgress(message, currentImportStep, totalAmountOfImportSteps);
            }

            public void JumpImportStepsForNextFile()
            {
                currentImportStep += AmountOfImportStepsPerFile;
            }
        }

        public GwswFileImporter()
        {
            FilesToImport = new List<string>();
            GwswAttributesDefinition = new EventedList<GwswAttributeType>();
            GwswDefaultFeatures = new Dictionary<string, List<string>>();
            ImportManager = new GwswImportManager();
            CsvDelimeter = ';'; //Default value, can be changed.
            LoadDefinitionFile();
        }

        /// <summary>
        /// Imports the given file as path. If it is null, then the list of files (FilesToImport) will be imported instead. 
        /// A Gwsw Definition file needs to be loaded beforehand with method LoadDefinitionFile.
        /// By default, all files referenced in the GwswDefinitionFile are selected to import.
        /// </summary>
        /// <param name="path">File to import. If this argument is missing then FilesToImport will be taken instead.</param>
        /// <param name="target"></param>
        /// <returns></returns>
        public object ImportItem(string path, object target = null)
        {
            if (GwswAttributesDefinition == null || !GwswAttributesDefinition.Any())
            {
                Log.ErrorFormat(Resources.GwswFileImporter_ImportItem_No_mapping_was_found_to_import_Gwsw_Files_);
                return null;
            }

            if (!string.IsNullOrEmpty(path)) FilesToImport = new EventedList<string> { path };

            Log.Info(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Importing_sub_files_);
            if (ShouldCancel)
                return null;
            var elementTypesList = ImportGwswElementsFromGwswFiles().ToList();

            var hydroModel = target is Project || target == null ? new HydroModelBuilder().BuildModel(ModelGroup.RHUModels) : target as HydroModel;
            
            
            var fmModel = hydroModel?.GetAllActivitiesRecursive<IWaterFlowFMModel>()?.FirstOrDefault() ?? target as IWaterFlowFMModel;
            if (fmModel != null)
            {
                ImportGwswNetworkInFmModel(elementTypesList, fmModel);
                if (hydroModel != null)
                {
                    //todo: Add workflow
                    //hydroModel.CurrentWorkflow = hydroModel.Workflows.First(w => w.Activities.Count == 1 && w.Activities.OfType<IWaterFlowFMModel>().Any());
                }
            }

            var rrModel = hydroModel?.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault() ?? target as RainfallRunoffModel;
            if (rrModel != null && fmModel?.Network != null)
            {
                ImportGwswNetworkInRrModel(elementTypesList, rrModel, fmModel?.Network);
                if (hydroModel != null)
                {
                    AddRRtoFMNwrwLinks(elementTypesList, rrModel, fmModel);
                    //todo: Add workflow
                    //if (fmModel != null) hydroModel.CurrentWorkflow = hydroModel.Workflows.First(w => w.Activities.Count == 2 && w.Activities.OfType<IWaterFlowFMModel>().Any() && w.Activities.OfType<RainfallRunoffModel>().Any());
                    //else hydroModel.CurrentWorkflow = hydroModel.Workflows.First(w => w.Activities.Count == 1 && w.Activities.OfType<RainfallRunoffModel>().Any());
                }
            }

            return (target is Project || target == null) && !ShouldCancel ? hydroModel : null;

        }

        private void AddRRtoFMNwrwLinks(List<KeyValuePair<SewerFeatureType, GwswElement>> elementTypesList, RainfallRunoffModel rrModel, IWaterFlowFMModel fmModel)
        {
            // TODO: create link
            foreach (var nwrwDischargeData in rrModel.NwrwDischargeData)
            {
                if (nwrwDischargeData.DischargeType == DischargeType.DryWeatherFlow)
                {
                    IBranch branch = fmModel.Network.Branches.FirstOrDefault(b => b.Name == nwrwDischargeData.Name);
                    if (branch == null)
                    {
                        INode node = fmModel.Network.Nodes.FirstOrDefault(b => b.Name == nwrwDischargeData.Name);
                        if (node != null) branch = node.IncomingBranches.FirstOrDefault();
                    }

                    if (branch != null)
                    {
                        // add lateral to branch
                        LateralSource lateralSource = new LateralSource { Branch = branch, Chainage = branch.Length, Name = nwrwDischargeData.Name };
                        branch.BranchFeatures.Add(lateralSource);

                        // at FM-side, create lateral data of type REALTIME
                        Model1DLateralSourceData model1DLateralSourceData = fmModel.ModelDefinition.LateralSourcesData.FirstOrDefault(lsd =>lsd.Feature == lateralSource); //new Model1DLateralSourceData {Feature = (LateralSource) lateralSource};
                        model1DLateralSourceData.Name = lateralSource.Name;
                        model1DLateralSourceData.DataType = Model1DLateralDataType.FlowRealTime;
                        model1DLateralSourceData.Flow = 0d;
                        //fmModel.ModelDefinition.LateralSourcesData.Add(model1DLateralSourceData);
                    }
                }
            }
        }

        private void ImportGwswNetworkInRrModel(IEnumerable<KeyValuePair<SewerFeatureType, GwswElement>> elementTypesList, RainfallRunoffModel rrModel, IHydroNetwork network)
        {
            var importedFeatureElements = ImportGwswDatabaseForRr(elementTypesList).ToList();
            if (rrModel == null || network == null || ShouldCancel) return;
            ReportProgress("Adding features to Rainfall Runoff Model.");
            AddNwrwFeaturesToRainfallRunoffModel(importedFeatureElements, rrModel, network);
        }

        private void AddNwrwFeaturesToRainfallRunoffModel(IEnumerable<INwrwFeature> importedFeatureElements, RainfallRunoffModel rrModel, IHydroNetwork network)
        {
            var featureElements = importedFeatureElements.ToList();
            var nrOfImportedFeatureElements = featureElements.Count;
            var stepSize = nrOfImportedFeatureElements / 20;

            var nodeNameLookup = new HashSet<string>(network.Nodes.Select(n => n.Name));
            var branchNameLookup = new HashSet<string>(network.Branches.Select(n => n.Name));

            var listOfErrors = new List<string>();
            for (int i = 0; i < featureElements.Count; i++)
            {
                var e = featureElements[i];
                try
                {
                    if (ShouldCancel)
                        return;

                    var indexOf = i;

                    if (stepSize != 0 && indexOf % stepSize == 0)
                        SetProgress($"Adding feature to Rainfall Runoff Model ({indexOf / (double)nrOfImportedFeatureElements:P0})", indexOf, nrOfImportedFeatureElements);

                    //todo: refactor
                    // only add gwsw data if a node/connection with same unique name already exists in the network, or when we are dealing with NwrwGlobalData or NwrwDryWeatherFlowDefinition
                    if (e is NwrwGlobalData || e is NwrwDryWeatherFlowDefinition || nodeNameLookup.Contains(e.Name) || branchNameLookup.Contains(e.Name))
                        e.AddNwrwCatchmentModelDataToModel(rrModel);
                }
                catch (Exception exception)
                {
                    listOfErrors.Add(exception.Message + Environment.NewLine);
                }
            }

            if (listOfErrors.Any())
                Log.ErrorFormat($"While adding GWSW features to Rainfall Runoff Model we encountered the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, listOfErrors)}");
        }

        [EditAction]
        private void ImportGwswNetworkInFmModel(IEnumerable<KeyValuePair<SewerFeatureType, GwswElement>> elementTypesList, IWaterFlowFMModel fmModel)
        {
            var network = fmModel?.Network;
            network?.BeginEdit(new DefaultEditAction("Importing GWSW database."));

            try
            {
                var importedFeatureElements = SewerFeatureFactory.CreateSewerEntities(elementTypesList, SetProgress, this).ToList();
                if (network != null && !ShouldCancel)
                {
                    ReportProgress("Adding features to network.");
                    AddSewerFeaturesToNetwork(importedFeatureElements, network);
                    AddBoundariesOfNetworkOutletCompartmentsToModelDefinition(fmModel);
                }
            }
            finally
            {
                network?.EndEdit();
            }
        }

        private GwswImportManager ImportManager { get; }

        private IEnumerable<INwrwFeature> ImportGwswDatabaseForRr(IEnumerable<KeyValuePair<SewerFeatureType, GwswElement>> elementTypesList)
        {
            // Surface types (oppervlak.csv)
            var surfaceTypes = elementTypesList.Where(k => k.Key == SewerFeatureType.Surface).Select(k => k.Value).ToList();
            if (surfaceTypes.Any())
            {
                var surfaceFeatures = CreateSurfaceFeatures(surfaceTypes);

                foreach (var surfaceFeature in surfaceFeatures)
                {
                    if (ShouldCancel)
                        yield break;
                    yield return surfaceFeature;
                }
            }

            // Runoff types (nwrw.csv)
            var runoffTypes = elementTypesList.Where(k => k.Key == SewerFeatureType.Runoff).Select(k => k.Value).ToList();
            if (runoffTypes.Any())
            {
                var nrOfRunoffs = runoffTypes.Count;
                foreach (var element in runoffTypes)
                {
                    if (ShouldCancel)
                        yield break;
                    var indexOf = runoffTypes.IndexOf(element);
                    var stepSize = nrOfRunoffs / 20;
                    if (stepSize != 0 && indexOf % stepSize == 0)
                    {
                        SetProgress($"Generating Rainfall Runoff features", runoffTypes.IndexOf(element), nrOfRunoffs);
                    }

                    yield return GwswNwrwGenerator.CreateNewNwrwRunoffData(element);
                }
            }

            // Distribution types (verloop.csv)
            var distributionTypes = elementTypesList.Where(k => k.Key == SewerFeatureType.Distribution).Select(k => k.Value).ToList();
            if (distributionTypes.Any())
            {
                var nrOfDistributions = distributionTypes.Count;
                foreach (var element in distributionTypes)
                {
                    if (ShouldCancel)
                        yield break;
                    var indexOf = distributionTypes.IndexOf(element);
                    var stepSize = nrOfDistributions / 20;
                    if (stepSize != 0 && indexOf % stepSize == 0)
                    {
                        SetProgress($"Generating Rainfall Runoff features", surfaceTypes.IndexOf(element), nrOfDistributions);
                    }

                    yield return GwswNwrwGenerator.CreateNewNwrwDistributionData(element);
                }
            }

            // Discharge types (debiet.csv)
            var dischargeTypes = elementTypesList.Where(k => k.Key == SewerFeatureType.Discharge).Select(k => k.Value).ToList();
            if (dischargeTypes.Any())
            {
                var nrOfDischargeTypes = dischargeTypes.Count;
                foreach (var element in dischargeTypes)
                {
                    if (ShouldCancel)
                        yield break;
                    var indexOf = dischargeTypes.IndexOf(element);
                    var stepSize = nrOfDischargeTypes / 20;
                    if (stepSize != 0 && indexOf % stepSize == 0)
                    {
                        SetProgress($"Generating Rainfall Runoff features", dischargeTypes.IndexOf(element), nrOfDischargeTypes);
                    }

                    yield return GwswNwrwGenerator.CreateNewNwrwDischargeData(element);
                }
            }
        }

        private IEnumerable<NwrwData> CreateSurfaceFeatures(IList<GwswElement> surfaceTypes)
        {
            var surfaceFeatures = new List<NwrwData>();
            var nrOfSurfaces = surfaceTypes.Count;
            foreach (var element in surfaceTypes)
            {
                if(ShouldCancel)
                    yield break;
                var indexOf = surfaceTypes.IndexOf(element);
                var stepSize = nrOfSurfaces / 20;
                if (stepSize != 0 && indexOf % stepSize == 0)
                {
                    SetProgress($"Generating Rainfall Runoff features", surfaceTypes.IndexOf(element), nrOfSurfaces);
                }

                var uniqueId = element.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId).ValueAsString.Trim();

                if (surfaceFeatures.Any(nwrwData => nwrwData.Name == uniqueId))
                {
                    var surfaceFeature = surfaceFeatures.FirstOrDefault(nwrwData => nwrwData.Name == uniqueId);
                    if (surfaceFeature != null)
                    {
                        var surfaceType = element.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceId)
                            .ValueAsString.Trim();

                        var surface = element.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Surface);
                        if (surface.TryGetValueAsDouble(out double auxDouble))
                        {
                            var nwrwSurfaceType = (NwrwSurfaceType) typeof(NwrwSurfaceType).GetEnumValueFromDescription(surfaceType);
                            surfaceFeature.SurfaceLevelDict[nwrwSurfaceType] = auxDouble;
                        }
                    }
                }
                else
                {
                    var newNwrwSurfaceData = GwswNwrwGenerator.CreateNewNwrwSurfaceData(element);
                    surfaceFeatures.Add(newNwrwSurfaceData);
                    yield return newNwrwSurfaceData;
                }
            }
        }

        private IEnumerable<KeyValuePair<SewerFeatureType, GwswElement>> ImportGwswElementsFromGwswFiles()
        {
            InitializeImportManager();
            foreach (var filePath in FilesToImport)
            {
                if (ShouldCancel)
                    yield break;
                if (!File.Exists(filePath))
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Could_not_find_file__0__, filePath);
                    ImportManager.JumpImportStepsForNextFile();
                    continue;
                }

                var gwswElements = ImportGwswElementList(filePath);
                
                foreach (var gwswElement in gwswElements)
                {
                    if(ShouldCancel)
                        yield break;
                    SewerFeatureType elementType;
                    if (!Enum.TryParse(gwswElement?.ElementTypeName, out elementType)) continue;

                    yield return new KeyValuePair<SewerFeatureType, GwswElement>(elementType, gwswElement);
                }
            }
        }

        private void AddSewerFeaturesToNetwork(IEnumerable<ISewerFeature> importedFeatureElements, IHydroNetwork network)
        {
            var featureElements = importedFeatureElements.ToList();
            var nrOfImportedFeatureElements = featureElements.Count;
            var stepSize = nrOfImportedFeatureElements / 20;
            var listOfErrors = new List<string>();
            var helper = new SewerImporterHelper();
            featureElements.ForEach(e =>
            {
                try
                {
                    if (ShouldCancel)
                        return;
                    var indexOf = featureElements.IndexOf(e);

                    if (stepSize != 0 && indexOf % stepSize == 0)
                        SetProgress($"Adding feature to network ({((double)((double)indexOf / (double)nrOfImportedFeatureElements)):P0})", indexOf, nrOfImportedFeatureElements);


                    e.AddToHydroNetwork(network, helper);
                }
                catch (Exception exception)
                {
                    listOfErrors.Add(exception.Message + Environment.NewLine);
                }
            });
            if (listOfErrors.Any())
                Log.ErrorFormat($"While adding GWSW features to network we encountered the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, listOfErrors)}");
        }

        private void AddBoundariesOfNetworkOutletCompartmentsToModelDefinition(IWaterFlowFMModel fmModel)
        {
            foreach (var outletCompartment in fmModel.Network.OutletCompartments)
            {
                if (ShouldCancel)
                    return;
                //var boundaryCondition = WaterFlowModel1DHelper.CreateDefaultBoundaryCondition(outletCompartment.ParentManhole, fmModel.UseSalinity, fmModel.UseTemperature);
                var boundaryCondition = fmModel.BoundaryConditions1D.FirstOrDefault(bc => bc.Node == outletCompartment.ParentManhole);
                if (boundaryCondition == null) continue;

                boundaryCondition.DataType = Model1DBoundaryNodeDataType.WaterLevelTimeSeries;
                boundaryCondition.Data[fmModel.StartTime] = -1000.0;
                boundaryCondition.Data[fmModel.StopTime] = -1000.0;

                /*
                fmModel.ModelDefinition.Boundaries.Add(outletCompartment.OutletCompartmentBoundaryFeature);

                var boundaryConditionSet = fmModel.BoundaryConditionSets.FirstOrDefault(bcs => bcs.Name.StartsWith(outletCompartment.Name));
                var boundaryCondition = CreateOutletCompartmentBoundaryCondition(fmModel, outletCompartment);
                boundaryConditionSet?.BoundaryConditions.Add(boundaryCondition);*/
                //fmModel
            }
        }

        private static FlowBoundaryCondition CreateOutletCompartmentBoundaryCondition(WaterFlowFMModel fmModel, OutletCompartment outletCompartment)
        {
            if (outletCompartment.OutletCompartmentBoundaryFeature.Geometry == null)
                outletCompartment.OutletCompartmentBoundaryFeature.Geometry = outletCompartment.Geometry;
            var boundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)
            {
                Feature = outletCompartment.OutletCompartmentBoundaryFeature
            };

            boundaryCondition.AddPoint(0);
            var dataAtZero = boundaryCondition.GetDataAtPoint(0);

            dataAtZero[fmModel.StartTime] = -1000.0;
            dataAtZero[fmModel.StopTime] = -1000.0;
            return boundaryCondition;
        }

        private void InitializeImportManager()
        {
            ImportManager.AmountOfImportStepsPerFile = 2;
            ImportManager.CalculateTotalAmountOfImportSteps(FilesToImport);
        }

        private void ReportProgress(string message)
        {
            ImportManager.ReportProgress(message);
        }

        /// <summary>
        /// Loads the feature files from a directory.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        public void LoadFeatureFiles(string directoryPath)
        {
            Log.InfoFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Attributes_mapped__0_,
                GwswAttributesDefinition.Count);

            try
            {
                GwswDefaultFeatures = CreateFileNameToViewDataDictionary(directoryPath);
            }
            catch (Exception)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_Not_possible_to_import__0_, directoryPath);
            }

            FilesToImport = new EventedList<string>(GwswDefaultFeatures?.Select(f => f.Value[2]));
        }

        /// <summary>
        /// Given a file path, it tries to import a CSV file and generate Gwsw elements out of the data on it.
        /// </summary>
        /// <param name="path">The location of the CSV file we want to transform into Gwsw elements.</param>
        /// <returns>List of GwswElements or null</returns>
        public IEnumerable<GwswElement> ImportGwswElementList(string path)
        {
            var importedDataTable = ImportFileAsDataTable(path); // TODO Sil -> invalid cast exception from this method
            if (importedDataTable == null)
                yield break;

            
            var elementTypeFound = GwswAttributesDefinition.FirstOrDefault(at => at.FileName.Equals(Path.GetFileName(path)));
            var elementTypeName = string.Empty;
            if (elementTypeFound != null)
            {
                elementTypeName = elementTypeFound.ElementName;
                Log.InfoFormat(Resources.GwswFileImporterBase_ImportItem_Mapping_file__0__as_element__1_, path, elementTypeName);
            }
            else
            {
                Log.InfoFormat(Resources.GwswFileImporterBase_ImportItem_Occurrences_on_file__0__will_not_be_mapped_to_any_element_, path);
                yield break;
            }

            if (!IsColumnMappingCorrect(path, importedDataTable))
            {
                yield break;
            }

            var nrOfRows = importedDataTable.Rows.Count;
            foreach (DataRow dataRow in importedDataTable.Rows)
            {
                if(ShouldCancel)
                    yield break;
                var lineNumber = importedDataTable.Rows.IndexOf(dataRow);
                var stepSize = (int)nrOfRows / 10;
                if (stepSize != 0 && lineNumber % stepSize == 0)
                    SetProgress($"Importing file {Path.GetFileName(path) ?? ("<unknown_file>")}", lineNumber, nrOfRows);
                var element = new GwswElement { ElementTypeName = elementTypeName };
                for (var i = 0; i < dataRow.ItemArray.Length; i++)
                {
                    var cell = dataRow.ItemArray[i];
                    var columnName = importedDataTable.Columns[i].ColumnName;
                    var attribute = new GwswAttribute
                    {
                        LineNumber = lineNumber,
                        ValueAsString = cell.ToString()
                    };
                    if (GwswAttributesDefinition != null)
                    {
                        var foundAttributeType = GwswAttributesDefinition.FirstOrDefault(attr =>
                            attr.ElementName.Equals(elementTypeName) && attr.Key.Equals(columnName));
                        attribute.GwswAttributeType = foundAttributeType;
                    }

                    element.GwswAttributeList.Add(attribute);
                }

                yield return element;
            }
        }

        private bool IsColumnMappingCorrect(string path, DataTable importedDataTable)
        {
            var result = true;
            string headerLineFile;
            using (var reader = new StreamReader(path))
            {
                headerLineFile = reader.ReadLine() ?? string.Empty;
            }
            var headersFile = headerLineFile.Split(CsvDelimeter).Distinct().ToArray();
            var fileAttributes = GwswAttributesDefinition.Where(at => at.FileName.Equals(Path.GetFileName(Path.GetFileName(path)))).ToList();
            for (var columnIndex = 0; columnIndex < importedDataTable.Columns.Count; columnIndex++)
            {
                var columnName = importedDataTable.Columns[columnIndex].ColumnName;
                var fileAttribute = fileAttributes.First(a => a.Key.Equals(columnName));
                var expectedHeader = fileAttribute.LocalKey;
                var headerName = headersFile[columnIndex];
                if (!expectedHeader.ToLower().Equals(headerName.ToLower().Trim()))
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_column__0__expectedcolumn__1__of_file__2__was_not_mapped_correctly__,
                        headerName, expectedHeader, path);
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Transforms a CSV data file, into tables that we can handle internally
        /// </summary>
        /// <param name="path">Location of the CSV file to import.</param>
        /// <param name="mappingData">Delimeters and properties for handling the CSV file.</param>
        /// <returns>DataTable with the content of the CSV file of <param name="path"/>.</returns>
        public DataTable ImportFileAsDataTable(string path, CsvMappingData mappingData = null)
        {
            if (mappingData == null)
                mappingData = GetCsvMappingDataForFileFromDefinition(path);

            if (mappingData == null)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__, path);
                return null;
            }

            var csvImporter = new CsvImporter { AllowEmptyCells = true };
            var importedCsv = new DataTable();
            try
            {
                importedCsv = csvImporter.ImportCsv(path, mappingData); // TODO Sil -> Invalid cast exception from this method
            }
            catch (Exception e)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_Could_not_import_file__0___Reason___1_, path, e.Message);
            }

            return importedCsv;
        }

        /// <summary>
        /// Gets or sets the CSV delimeter to split a line in a csv file.
        /// </summary>
        /// <value>
        /// The CSV delimeter.
        /// </value>
        public char CsvDelimeter { get; set; }

        /// <summary>
        /// Gets or sets the files to import.
        /// </summary>
        /// <value>
        /// The files to import.
        /// </value>
        public IList<string> FilesToImport { get; set; }

        private CsvSettings CsvSettingsSemiColonDelimeted
        {
            get
            {
                return csvSettings ?? (csvSettings = new CsvSettings
                {
                    Delimiter = CsvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                });
            }
        }
        public IEventedList<GwswAttributeType> GwswAttributesDefinition { get; private set; }

        /// <summary>
        /// Dictionary content:
        /// Key = Feature FileName.
        /// Value = List containing 3 strings:
        ///     [0] <string>ElementName</string>
        ///     [1] <string>SewerFeatureType (mapped value)</string>
        ///     [2] <string>Full path</string>
        /// </summary>
        public IDictionary<string, List<string>> GwswDefaultFeatures { get; private set; }

        private CsvMappingData CsvMappingData
        {
            get
            {
                var mappingData = new CsvMappingData
                {
                    Settings = new CsvSettings
                    {
                        Delimiter = CsvDelimeter,
                        FirstRowIsHeader = true,
                        SkipEmptyLines = true
                    },
                    FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                    {
                        {
                            new CsvRequiredField("Bestandsnaam", typeof(string)),
                            new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("ElementName", typeof(string)),
                            new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Kolomnaam", typeof(string)),
                            new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Code", typeof(string)),
                            new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Code_International", typeof(string)),
                            new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Definitie", typeof(string)),
                            new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Type", typeof(string)),
                            new CsvColumnInfo(6, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Eenheid", typeof(string)),
                            new CsvColumnInfo(7, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Verplicht", typeof(string)),
                            new CsvColumnInfo(8, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Standaardwaarde", typeof(string)),
                            new CsvColumnInfo(9, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Opmerking", typeof(string)),
                            new CsvColumnInfo(10, CultureInfo.InvariantCulture)
                        },
                    }
                };
                return mappingData;
            }
        }

#region IFileImporter

        public string Name
        {
            get { return "GWSW Feature File importer"; }
        }
        public string Description { get { return Name; } }
        public string Category
        {
            get { return "1D / 2D"; }
        }

        public Bitmap Image
        {
            get { return Resources.StructureFeatureSmall; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IWaterFlowFMModel);
                yield return typeof(HydroModel);
                yield return typeof(RainfallRunoffModel);
            }
        }

        public bool CanImportOnRootLevel { get { return true; } }
        public string FileFilter { get { return "GWSW Csv Files (*.csv)|*.csv"; } }
        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get { return false; } }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

#endregion

        private void SetProgress(string currentStepName, int currentStep, int totalSteps)
        {
            ProgressChanged?.Invoke(currentStepName, currentStep, totalSteps);
        }

        /// <summary>
        /// It loads a definition file into the dictionary GwswAttributeDefinition
        /// It also sets the initial FilesToImport
        /// </summary>
        /// <returns>DataTable describing contents of the CSV file</returns>
        private void LoadDefinitionFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = @"DeltaShell.Plugins.ImportExport.GWSW.Resources.GWSWDefinition.csv";
            var csvPreviousDelimeter = CsvDelimeter;
            CsvDelimeter = ',';
            DataTable importedTable;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_Not_possible_to_import__0_, resourceName);
                    CsvDelimeter = csvPreviousDelimeter;
                    return;
                }
                var mappingData = CsvMappingData;
                using (var streamReader = new StreamReader(stream))
                {
                    importedTable = ImportFileAsDataTable(streamReader, mappingData);
                    if (importedTable == null || importedTable.Rows.Count == 0)
                    {
                        Log.ErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_Not_possible_to_import__0_, resourceName);
                        CsvDelimeter = csvPreviousDelimeter;
                        return;
                    }
                }
            }

            //Load the related tables referred in the definition file.
            var attributeList = new EventedList<GwswAttributeType>();

            // Create new attributes for each occurrence.
            // Retrieve the files that need to be read.
            foreach (DataRow row in importedTable.Rows)
            {
                var attributeFile = row.ItemArray[0].ToString();
                var attributeElement = row.ItemArray[1].ToString();
                var attributeName = row.ItemArray[2].ToString();
                var attributeCode = row.ItemArray[3].ToString();
                var attributeCodeInt = row.ItemArray[4].ToString();
                var attributeDefinition = row.ItemArray[5].ToString();
                var attributeType = row.ItemArray[6].ToString();
                var attributeDefaultValue = row.ItemArray[9].ToString();

                var attribute = new GwswAttributeType
                {
                    Name = attributeName,
                    ElementName = attributeElement,
                    Definition = attributeDefinition,
                    FileName = attributeFile,
                    Key = attributeCodeInt,
                    LocalKey = attributeCode,
                    AttributeType = GwswAttributeType.TryGetParsedValueType(attributeName, attributeType, attributeDefinition, attributeFile, importedTable.Rows.IndexOf(row)),
                    DefaultValue = attributeDefaultValue
                };

                attributeList.Add(attribute);
            }

            //If some attributes have a different element from which they should, then we will show an error informing of such a difference.
            attributeList.GroupBy(el => el.FileName).ForEach(gr =>
            {
                var mismatchedElementNames = gr.Select(el => el.ElementName).Distinct().ToList();
                if (mismatchedElementNames.Count > 1)
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_There_is_a_mismatch_for_File_Name__0___currently_mapped_to_different_element_names__1__, gr.Key, string.Concat(mismatchedElementNames));
                }
            });

            GwswAttributesDefinition = attributeList;
            CsvDelimeter = csvPreviousDelimeter;
        }

        /// <summary>
        /// Transforms a CSV data file, into tables that we can handle internally
        /// </summary>
        /// <param name="streamReader">Stream of the CSV file to import.</param>
        /// <param name="mappingData">Delimeters and properties for handling the CSV file.</param>
        /// <returns>DataTable with the content of the CSV file of <param name="streamReader"/>.</returns>
        private DataTable ImportFileAsDataTable(StreamReader streamReader, CsvMappingData mappingData)
        {
            if (mappingData == null)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__, streamReader);
                return null;
            }

            var csvImporter = new CsvImporter { AllowEmptyCells = true };
            var importedCsv = new DataTable();
            try
            {
                importedCsv = csvImporter.Extract(csvImporter.SplitToTable(streamReader, mappingData.Settings), mappingData.FieldToColumnMapping, mappingData.Filters);
            }
            catch (Exception e)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_Could_not_import_file__0___Reason___1_, streamReader, e.Message);
            }

            return importedCsv;
        }

        private IDictionary<string, List<string>> CreateFileNameToViewDataDictionary(string directoryPath)
        {
            //Get the items to import
            var dictionary = GwswAttributesDefinition?.GroupBy(i => i.FileName)
                .ToDictionary(
                    grp => grp.Key,
                    grp => {
                        var valueList = new List<string>();
                        var elementName = grp.FirstOrDefault(g => !String.IsNullOrEmpty(g.ElementName))?.ElementName;
                        SewerFeatureType featureName;
                        Enum.TryParse(elementName, out featureName);
                        valueList.Add(elementName);
                        valueList.Add(featureName.ToString());
                        valueList.Add(Path.Combine(directoryPath, grp.Key));
                        return valueList;
                    });

            return dictionary;
        }

        private CsvMappingData GetCsvMappingDataForFileFromDefinition(string fileName)
        {
            //Import file elements based on their attributes
            if (GwswAttributesDefinition == null || !GwswAttributesDefinition.Any())
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__, fileName);
                return null;
            }

            var fileAttributes = GwswAttributesDefinition.Where(at => at.FileName.Equals(Path.GetFileName(fileName))).ToList();
            var fileColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>();
            //Create column mapping
            fileAttributes.ForEach(
                attr =>
                    fileColumnMapping.Add(
                        new CsvRequiredField(attr.Key, attr.AttributeType),
                        new CsvColumnInfo(fileAttributes.IndexOf(attr), CultureInfo.InvariantCulture)));

            var mapping = new CsvMappingData
            {
                Settings = CsvSettingsSemiColonDelimeted,
                FieldToColumnMapping = fileColumnMapping,
            };
            return mapping;
        }
    }
}
