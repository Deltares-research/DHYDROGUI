using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Csv.Importer;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    public abstract class CrossSectionFromCsvFileImporterBase : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrossSectionFromCsvFileImporterBase));

        protected CrossSectionFromCsvFileImporterBase()
        {
            CsvSettings = new CsvSettings
                {
                    Delimiter = ',', 
                    FirstRowIsHeader = true, 
                    SkipEmptyLines = true
                };
            CrossSectionImportSettings = new CrossSectionImportSettings
                {
                    ImportChainages = true,
                    CreateIfNotFound = true
                };
        }

        public abstract string Name { get; }
        public virtual string Description { get { return Name; } }

        public virtual string Category { get; protected set; }

        public virtual Bitmap Image { get; protected set; }
        
        public bool OpenViewAfterImport { get { return false; } }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof (IHydroNetwork); }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel { get { return false; } }

        public string FileFilter { get { return "CSV Files (*.csv)|*.csv"; } }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target)
        {
            if (!(target is IHydroNetwork))
            {
                Log.Error("No network available to import the cross-sections.");
                return null;
            }

            if (path != null)
            {
                FilePath = path;
            }

            DataTable crossSectionDataTable;

            try
            {
                crossSectionDataTable = ImportDataTable();
            }
            catch (Exception e)
            {
                Log.Error("Reading data table failed, exception caught: " + e.Message);
                return target;
            }

            var dataTable = ReadDataTable(crossSectionDataTable);

            UpdateExistingAndCreateNewCrossSections((IHydroNetwork) target, dataTable);

            return target;
        }

        protected abstract CrossSectionType CrossSectionType { get; }

        protected abstract IList<CrossSectionCsvData> ReadDataTable(DataTable dataTable);

        [InvokeRequired]
        private void UpdateExistingAndCreateNewCrossSections(IHydroNetwork target, IList<CrossSectionCsvData> crossSectionCsvData)
        {
            if (!crossSectionCsvData.Any())
            {
                return;
            }

            var existingCrossSections = target.CrossSections.ToDictionary(cs => cs.Name, cs => cs);

            target.BeginEdit("ImportCrossSections");

            foreach (var data in crossSectionCsvData)
            {
                ICrossSection existing;
                existingCrossSections.TryGetValue(data.Name, out existing);
                try
                {
                    if (existing != null)
                    {
                        if (existing.CrossSectionType != CrossSectionType)
                        {
                            Log.ErrorFormat(
                                "Skipping update of cross section {0} of type {1}: importer can only update cross sections of type {2}",
                                existing.Name, existing.CrossSectionType, CrossSectionType);
                            continue;
                        }
                        if (CrossSectionImportSettings.ImportChainages)
                        {
                            bool importChainage = true;
                            if (existing.Branch.Name != data.Branch)
                            {
                                Log.Error("Branch of cross section " + data.Name + " does not match with branch in import -- skipping import of chainage");
                                importChainage = false;
                            }
                            if (double.IsNaN(data.Chainage))
                            {
                                Log.Error("Chainage of cross section" + data.Name + "was not a valid number -- skipping import of chainage");
                                importChainage = false;
                            }
                            if (importChainage)
                            {
                                SetChainage(existing, existing.Branch, data.Chainage);
                            }
                        }
                        UpdateCrossSection(existing, data, target);
                    }
                    else if (CrossSectionImportSettings.CreateIfNotFound)
                    {
                        var branch = target.Branches.FirstOrDefault(br => br.Name == data.Branch);

                        if (branch == null)
                        {
                            Log.Error("Creation of new cross section " + data.Name + "failed: branch " +
                                      data.Branch + " not found in network.");
                            continue;
                        }
                        if (double.IsNaN(data.Chainage))
                        {
                            Log.Error("Chainage of cross section" + data.Name + "was not a valid number,");
                            continue;
                        }

                        var crossSection = CreateNewCrossSection(data, target);

                        SetChainage(crossSection, branch, data.Chainage);

                        branch.BranchFeatures.Add(crossSection);

                        existingCrossSections.Add(crossSection.Name, crossSection);
                    }
                }
                catch (Exception e)
                {
                    Log.ErrorFormat(
                        "Import of cross section '{0}' failed with error: " + e.Message, data.Name);
                }
            }
            target.EndEdit();
        }

        private static void SetChainage(ICrossSection crossSection, IBranch branch, double chainage)
        {
            if (branch.Length < chainage)
            {
                Log.Warn("Cross section " + crossSection.Name +
                         " has a chainage larger than the length of branch " +
                         branch.Name + ": the cross section has been moved to the end of the branch");

                crossSection.Chainage = branch.Length;
            }
            else if (chainage < 0)
            {
                Log.Warn("Cross section " + crossSection.Name +
                         " has a chainage smaller than zero: the cross section has been moved to the beginning of the branch");

                crossSection.Chainage = 0;
            }
            else
            {
                crossSection.Chainage = chainage;
            }
        }

        protected virtual void UpdateCrossSection(ICrossSection crossSection, CrossSectionCsvData crossSectionCsvData, IHydroNetwork target)
        {
            ICrossSectionDefinition crossSectionDefinition;

            if (crossSection.Definition.IsProxy)
            {
                Log.WarnFormat(
                    "Cross section {0} is using a shared definition {1}.The definition will be updated.",
                    crossSection.Name, crossSection.Definition.Name);
                crossSectionDefinition =
                    ((CrossSectionDefinitionProxy)crossSection.Definition).InnerDefinition;
            }
            else
            {
                crossSectionDefinition = crossSection.Definition;
            }
            try
            {
                crossSectionDefinition.BeginEdit("Update cross section definition");
                UpdateCrossSectionDefinition(crossSectionDefinition, crossSectionCsvData, target);
                crossSectionDefinition.EndEdit();
            }
            catch (Exception e)
            {
                Log.Warn("During import of cross section " + crossSectionCsvData.Name +
                         " errors in the update of the profile were encountered: " + e.Message);
                throw;
            }
            
        }

        protected virtual ICrossSection CreateNewCrossSection(CrossSectionCsvData crossSectionCsvData, IHydroNetwork target)
        {
            var crossSectionDefinition = CreateCrossSectionDefinition();
            
            var crossSection = new CrossSection(crossSectionDefinition)
            {
                Name = crossSectionCsvData.Name,
                LongName = crossSectionCsvData.LongName,
            };

            try
            {
                crossSectionDefinition.BeginEdit("Create cross section definition");
                UpdateCrossSectionDefinition(crossSectionDefinition, crossSectionCsvData, target);
                crossSectionDefinition.EndEdit();
            }
            catch (Exception e)
            {
                Log.Warn("During import of cross section " + crossSectionCsvData.Name +
                         " errors in the creation of the profile were encountered: " + e.Message);
                throw;
            }

            CrossSectionHelper.SetDefaultThalweg(crossSectionDefinition);
            return crossSection;
        }
        
        protected abstract void UpdateCrossSectionDefinition(ICrossSectionDefinition crossSectionDefinition, CrossSectionCsvData crossSectionCsvData, IHydroNetwork target);

        protected virtual ICrossSectionDefinition CreateCrossSectionDefinition()
        {
            switch (CrossSectionType)
            {
                case CrossSectionType.ZW:
                    return new CrossSectionDefinitionZW();
                case CrossSectionType.YZ:
                    return new CrossSectionDefinitionYZ();
                case CrossSectionType.GeometryBased:
                    return new CrossSectionDefinitionXYZ();
                default:
                    return null;
            }
        }

        private DataTable ImportDataTable()
        {
            var importer = new CsvImporter {AllowEmptyCells = true};
            return importer.ImportCsv(FilePath,
                                      new CsvMappingData
                                          {
                                              FieldToColumnMapping = CsvFieldToColumnMapping,
                                              Settings = CsvSettings
                                          });
        }

        protected abstract IDictionary<CsvRequiredField, CsvColumnInfo> CsvFieldToColumnMapping { get; }

        public string FilePath { private get; set; }

        public CsvSettings CsvSettings { get; set; }
        
        public CrossSectionImportSettings CrossSectionImportSettings { get; set; }
    }
}
