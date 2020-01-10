using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DeltaShell.Sobek.Readers;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekStructuresImporter: PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRetentionImporter));

        private const string displayName = "Structures (weirs, bridges etc.)";
        public override string DisplayName
        {
            get { return displayName; }
        }

        protected override void PartialImport()
        {
            log.DebugFormat("Importing structures ...");

            var structureLocationPath = GetFilePath(SobekFileNames.SobekNetworkStructuresFileName);
            var structureMappingPath = GetFilePath(SobekFileNames.SobekStructureDatFileName);
            var structureDefinitionPath = GetFilePath(SobekFileNames.SobekStructureDefinitionFileName);
            var compoundStructurePath = GetFilePath(SobekFileNames.SobekCompoundStructureFileName);
            if (!ValidateStructureFilesExist(structureLocationPath, structureMappingPath, structureDefinitionPath))
            {
                return;
            }

            IEnumerable<SobekStructureDefinition> definitions = new SobekStructureDefFileReader(SobekType).Read(structureDefinitionPath);
            IEnumerable<SobekStructureMapping> mappings = (new SobekStructureDatFileReader().Read(structureMappingPath));
            IEnumerable<SobekCompoundStructure> compoundStructures = new SobekCompoundStructureReader().Read(compoundStructurePath);

            var sobekCrossSectionDefinitionsLookup = GetSobekCrossSectionDefinitionsLookup();
            var sobekFriction = GetSobekFriction();
            var sobekValveData = GetSobekValveData();
            var channels = HydroNetwork.Channels.ToDictionary(b => b.Name, b => b);

            if (channels.Count == 0)
            {
                log.Error("There are no branches. Structures can not be placed.");
                return;
            }

            var channelStructureBuilder = new ChannelStructureBuilder(
                channels,
                new SobekNetworkStructureReader().Read(structureLocationPath),
                definitions,
                mappings,
                compoundStructures,
                sobekCrossSectionDefinitionsLookup,
                sobekValveData,
                sobekFriction.StructureFrictionList,
                sobekFriction.SobekExtraFrictionList
                );

            channelStructureBuilder.SetStructuresOnChannels(HydroNetwork.Structures);

            if (SobekType == SobekType.SobekRE)
            {
                // SobekRe XRST records in deffrc.3 in 2.12 in friction.dat (already imported)
                var extraFrictionFile = GetFilePath(SobekFileNames.SobekExtraFrictionFileName);
                if (!File.Exists(extraFrictionFile))
                {
                    log.WarnFormat("Extra friction file [{0}] not found; skipping...", extraFrictionFile);

                }
                else
                {
                    var extraFrictions = new SobekReExtraFrictionDatFileReader().Read(extraFrictionFile);
                    var extraResistances = HydroNetwork.ExtraResistances.ToDictionary(er => er.Name, er => er);

                    foreach (var extraFriction in extraFrictions)
                    {
                        FindAndReplaceOrAddExtraFrictionToNetwork(extraFriction, channels, extraResistances);
                    }
                }
            }
        }

        private SobekFriction GetSobekFriction()
        {
            var sobekFriction = new SobekFriction();
            var frictionFile = GetFilePath(SobekFileNames.SobekFrictionFileName);
            if (!File.Exists(frictionFile))
            {
                log.WarnFormat("Friction file [{0}] not found; skipping...", frictionFile);
                return sobekFriction;
            }
            sobekFriction = new SobekFrictionDatFileReader().ReadSobekFriction(frictionFile);
            return sobekFriction;
        }

        private Dictionary<string, SobekCrossSectionDefinition> GetSobekCrossSectionDefinitionsLookup()
        {
            var filePath = GetFilePath(SobekFileNames.SobekProfileDefinitionsFileName);
            return new CrossSectionDefinitionReader()
                .Read(filePath)
                .ToDictionaryWithErrorDetails(filePath, csd => csd.ID);
        }

        private IList<SobekValveData> GetSobekValveData()
        {
            if (SobekType == SobekType.SobekRE)
            {
                // SobekRe has no culverts and thus no valves
                return new List<SobekValveData>();
            }
            string valveDataPath = GetFilePath(SobekFileNames.SobekValveDataFileName);
            if (!File.Exists(valveDataPath))
            {
                log.WarnFormat("Valve data file [{0}] not found; skipping...", valveDataPath);
                return new List<SobekValveData>();
            }
            return SobekValveDataReader.ReadValveData(valveDataPath);
        }

        private static bool ValidateStructureFilesExist(string structureLocationPath, string structureMappingPath, string structureDefinitionPath)
        {
            if (!File.Exists(structureLocationPath))
            {
                log.WarnFormat("Structure location file [{0}] not found; skipping...", structureLocationPath);
                return false;
            }
            if (!File.Exists(structureMappingPath))
            {
                log.WarnFormat("Structure mapping file [{0}] not found; skipping...", structureMappingPath);
                return false;
            }
            if (!File.Exists(structureDefinitionPath))
            {
                log.WarnFormat("Structure definition file [{0}] not found; skipping...", structureDefinitionPath);
                return false;
            }
            return true;
        }

        private void FindAndReplaceOrAddExtraFrictionToNetwork(SobekReExtraResistance extraFriction, Dictionary<string, IChannel> channels, Dictionary<string, IExtraResistance> extraResistances)
        {
            var channel = channels[extraFriction.BranchId];
            var offset = extraFriction.Chainage;
            var geometry = GeometryHelper.GetPointGeometry(channel, offset);

            if(extraResistances.ContainsKey(extraFriction.Id))
            {
                var extraResistance = extraResistances[extraFriction.Id];
                extraResistance.LongName = extraFriction.Name;
                extraResistance.FrictionTable.Clear();
                FunctionHelper.AddDataTableRowsToFunction(extraFriction.Table, extraResistance.FrictionTable);
                if(extraResistance.Branch != channel)
                {
                    extraResistance.Branch.BranchFeatures.Remove(extraResistance);
                    extraResistance.Branch = channel;
                    channel.BranchFeatures.Add(extraResistance);
                }
            }
            else
            {
                var compositeStructure = ChannelStructureBuilder.CreateCompositeStructureAndAddItToTheBranch(channel, offset, geometry);
                var extraResistance = new ExtraResistance
                {
                    Name = extraFriction.Id,
                    LongName = extraFriction.Name,
                };
                FunctionHelper.AddDataTableRowsToFunction(extraFriction.Table, extraResistance.FrictionTable);
                HydroNetworkHelper.AddStructureToComposite(compositeStructure, extraResistance);
            }
        }
    }
}
