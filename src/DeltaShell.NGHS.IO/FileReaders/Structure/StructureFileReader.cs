using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileReaders.BackwardCompatibility;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini.BackwardCompatibility;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders.Structure
{
    public static class StructureFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(StructureFileReader));
        private static readonly IniBackwardsCompatibilityHelper backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(new StructureFileBackwardsCompatibilityConfigurationValues());

        public static void ReadFile(string structureFilename, 
                                    ICrossSectionDefinition[] crossSectionDefinitions, 
                                    IHydroNetwork network,
                                    DateTime referenceDateTime)
        {
            var fileReadingExceptions = new List<FileReadingException>();

            var structuresIniSections = ReadStructureIniSections(structureFilename);

            IList<IStructure1D> structures = GetAllStructuresFromIniSections(structuresIniSections, 
                                                                            crossSectionDefinitions, 
                                                                            network, 
                                                                            structureFilename, 
                                                                            fileReadingExceptions,
                                                                            referenceDateTime);
            if (fileReadingExceptions.Count > 0)
            {
                //Do not throw because we want to add the successful structures to the model
                var errors = string.Join(Environment.NewLine, fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine));
                log.Error(string.Format(Resources.StructureFileReader_Errors_while_creating_structures, Environment.NewLine, errors));
                fileReadingExceptions.Clear();
            }

            // do not add crossSectionDefinitions => already added
            AddStructuresToNetwork(structures);
            
            // Set defaults profiles for sewer connections without profile from file.
            network.SewerConnections.Where(sc => sc.CrossSection == null).ForEach(sewerConnection =>
            {
                sewerConnection.GenerateDefaultProfileForSewerConnections();
            });

            if (fileReadingExceptions.Count <= 0) return;

            var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
            throw new FileReadingException($"While reading structures an error occured :{Environment.NewLine} {string.Join(Environment.NewLine, innerExceptionMessages)}");
        }

        private static void AddStructuresToNetwork(IList<IStructure1D> structures)
        {
            var compoundStructures = structures.Where(s => s.GetStructureType() == StructureType.CompositeBranchStructure);
            var nonCoumpoundStructures = structures.Where(s => s.GetStructureType() != StructureType.CompositeBranchStructure);

            AddCompositeStructuresToNetwork(compoundStructures);

            var grouping = nonCoumpoundStructures.GroupBy(s => s.Branch);

            foreach (var group in grouping)
            {
                var key = group.Key;
                group.ForEach(s =>
                {
                    if (key is ISewerConnection sewerConnection && sewerConnection.IsInternalConnection())
                    {
                        sewerConnection.AddStructureToBranch(s);
                    }
                    else
                    {
                        HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(s, key);
                    }

                });
            }
        }

        private static void AddCompositeStructuresToNetwork(IEnumerable<IStructure1D> compoundStructures)
        {
            compoundStructures.ForEach(c =>
            {
                c.Chainage = c.Branch.GetBranchSnappedChainage(c.Chainage);
                c.Geometry = HydroNetworkHelper.GetStructureGeometry(c.Branch, c.Chainage);
                c.Branch.BranchFeatures.Add(c);
            });
        }

        private static IList<IniSection> ReadStructureIniSections(string structureFilename)
        {
            if (!File.Exists(structureFilename))
                throw new FileReadingException(string.Format(Resources.Could_not_read_file_0_properly_it_doesnt_exist, structureFilename));

            var structuresIniSections = new IniReader().ReadIniFile(structureFilename);
            if (structuresIniSections.Count == 0)
                throw new FileReadingException(string.Format(Resources.Could_not_read_file_0_properly_it_seems_empty, structureFilename));

            return ApplyBackwardCompatibility(structuresIniSections).ToList();
        }
     
        private static IEnumerable<IniSection> ApplyBackwardCompatibility(IEnumerable<IniSection> iniSections)
        {
            foreach (IniSection iniSection in iniSections)
            {
                IniProperty[] propertiesWithUnsupportedValue = GetPropertiesWithUnsupportedValues(iniSection).ToArray();
                if (!propertiesWithUnsupportedValue.Any())
                {
                    yield return iniSection;
                }

                foreach (IniProperty property in propertiesWithUnsupportedValue)
                {
                    log.WarnFormat(Resources.Property_0_contains_unsupported_value_1_2_at_line_number_3_will_be_skipped, 
                                   property.Key, 
                                   property.Value, 
                                   iniSection.Name, 
                                   iniSection.LineNumber);
                }
            }
        }

        private static IEnumerable<IniProperty> GetPropertiesWithUnsupportedValues(IniSection iniSection)
        {
            return iniSection.Properties.Where(p => backwardsCompatibilityHelper.IsUnsupportedPropertyValue(iniSection.Name,
                                                                                                          p.Key,
                                                                                                          p.Value));
        }

        private static IList<IStructure1D> GetAllStructuresFromIniSections(IList<IniSection> structuresIniSections, 
                                                                          IList<ICrossSectionDefinition> crossSectionDefinitions, 
                                                                          IHydroNetwork network, 
                                                                          string structuresFilePath, 
                                                                          IList<FileReadingException> fileReadingExceptions,
                                                                          DateTime referenceDateTime)
        {
            IList<IStructure1D> structure1Ds = new List<IStructure1D>();
            var branchLookup = network.Branches.Where(b => !string.IsNullOrEmpty(b.Name)).ToDictionary(b => b.Name);
            var structureNameLookup = new HashSet<string>();

            LogHandler timeSeriesFileReaderLogHandler = new LogHandler(Resources.BcSpecificTimeSeriesReader_logHandler_reading_structures_from__bc_file);
            
            ITimeSeriesFileReader timeSeriesFileReader = new TimeSeriesFileReader(new TimSpecificTimeSeriesReader(new TimFile()),
                                                                                  new BcSpecificTimeSeriesReader(new BcReader(), 
                                                                                                                 new BcSectionParser(new LogHandler(nameof(StructureFileReader))),
                                                                                                                 timeSeriesFileReaderLogHandler));

            foreach (var structureDefinitionIniSection in structuresIniSections.Where(iniSection => string.Equals(iniSection.Name, StructureRegion.Header, StringComparison.InvariantCultureIgnoreCase)))
            {
                try
                {
                    var branchId = structureDefinitionIniSection.ReadProperty<string>(StructureRegion.BranchId.Key, true);
                    if (string.IsNullOrWhiteSpace(branchId) || !branchLookup.TryGetValue(branchId, out var branch))
                    {
                        continue;
                    }
                    
                    var type = structureDefinitionIniSection.ReadProperty<string>(StructureRegion.DefinitionType.Key);
                    var structure1D = structureDefinitionIniSection.ReadStructure(crossSectionDefinitions, 
                                                                                branch, 
                                                                                type, 
                                                                                structuresFilePath, 
                                                                                referenceDateTime,
                                                                                timeSeriesFileReader);
                    if (structure1D == null)
                    {
                        continue;
                    }

                    if (structureNameLookup.Contains(structure1D.Name))
                    {
                        throw new FileReadingException(string.Format(
                            "Structure with id {0} is already read, id's CAN NOT be duplicates!",
                            structure1D.Name));
                    }

                    structureNameLookup.Add(structure1D.Name);
                    structure1Ds.Add(structure1D);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException(Resources.StructureFileReader_Could_not_read_structure,
                        fileReadingException));
                }
            }
            
            timeSeriesFileReaderLogHandler.LogReport();

            return structure1Ds;
        }
    }
}
