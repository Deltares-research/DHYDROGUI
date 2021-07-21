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
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public static class StructureFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(StructureFileReader));
        public static void ReadFile(string structureFilename, ICrossSectionDefinition[] crossSectionDefinitions, IHydroNetwork network)
        {
            var fileReadingExceptions = new List<FileReadingException>();

            var structuresCategories = ReadStructureDelftIniCategories(structureFilename);
            if (structuresCategories.Count == 0)
                throw new FileReadingException($"Could not read file {structureFilename} properly, it seems empty");

            var structures = GetAllStructuresFromCategories(structuresCategories, crossSectionDefinitions, network, fileReadingExceptions);
            if (fileReadingExceptions.Count > 0)
            {
                //Do not throw because we want to add the successful structures to the model
                var errors = string.Join(Environment.NewLine, fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine));
                log.Warn($"While creating structures an error occured :{Environment.NewLine} {errors}");
                fileReadingExceptions.Clear();
            }

            // do not add crossSectionDefinitions => already added
            AddStructuresToNetwork(structures);

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
                c.Chainage = c.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(c.Chainage);
                c.Geometry = HydroNetworkHelper.GetStructureGeometry(c.Branch, c.Chainage);
                c.Branch.BranchFeatures.Add(c);
            });
        }

        private static IList<DelftIniCategory> ReadStructureDelftIniCategories(string structureFilename)
        {
            if (!File.Exists(structureFilename))
                throw new FileReadingException(string.Format("Could not read file {0} properly, it doesn't exist.", structureFilename));

            var structuresCategories = new DelftIniReader().ReadDelftIniFile(structureFilename);
            if (structuresCategories.Count == 0)
                throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty", structureFilename));

            return structuresCategories;
        }

        private static IList<IStructure1D> GetAllStructuresFromCategories(IList<DelftIniCategory> structuresCategories,
            IList<ICrossSectionDefinition> crossSectionDefinitions, IHydroNetwork network, IList<FileReadingException> fileReadingExceptions)
        {
            IList<IStructure1D> structure1Ds = new List<IStructure1D>();
            var branchLookup = network.Branches.Where(b => !string.IsNullOrEmpty(b.Name)).ToDictionary(b => b.Name);
            var structureNameLookup = new HashSet<string>();

            foreach (var structureDefinitionCategory in structuresCategories.Where(category => category.Name == StructureRegion.Header))
            {
                try
                {
                    var branchId = structureDefinitionCategory.ReadProperty<string>(StructureRegion.BranchId.Key, true);
                    if (string.IsNullOrWhiteSpace(branchId) || !branchLookup.TryGetValue(branchId, out var branch))
                    {
                        continue;
                    }

                    var type = structureDefinitionCategory.ReadProperty<string>(StructureRegion.DefinitionType.Key);
                    var structure1D = structureDefinitionCategory.ReadStructure(crossSectionDefinitions, branch, type);
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
                    fileReadingExceptions.Add(new FileReadingException("Could not structure.",
                        fileReadingException));
                }
            }

            return structure1Ds;
        }
    }
}