using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.ImportExport.Sobek.Builders
{
    /// <summary>
    /// This class is responsible for the construction of structures and adding them to the network channels
    /// </summary>
    public class ChannelStructureBuilder
    {
        private static ILog log = LogManager.GetLogger(typeof(ChannelStructureBuilder));
        protected static readonly ILog Log = LogManager.GetLogger(typeof(ChannelStructureBuilder));
        private readonly IList<IBranchStructureBuilder> structurebuilders;
        private readonly IDictionary<string, IBranch> channels;
        private readonly IDictionary<string, SobekStructureDefinition> definitions;
        private readonly IEnumerable<SobekStructureLocation> locations;
        private readonly IDictionary<string, SobekStructureMapping> mappings; // structure mappings from STRUCT.DAT
        private readonly IDictionary<string, SobekCompoundStructure> compoundMapping;
        private readonly IList<SobekStructureFriction> sobekStructureFriction;
        private readonly IDictionary<string, SobekExtraResistance> sobekExtraFriction;

        #region Constructors



        public ChannelStructureBuilder(IDictionary<string, IBranch> channels,
                                        IEnumerable<SobekStructureLocation> locations,
                                        IEnumerable<SobekStructureDefinition> definitions,
                                        IEnumerable<SobekStructureMapping> mappings,
                                        IEnumerable<SobekCompoundStructure> compoundStructures,
                                        Dictionary<string, SobekCrossSectionDefinition> sobekCrossSectionDefinitions,
                                        IList<SobekValveData> sobekValveDataTables,
                                        IList<SobekStructureFriction> sobekStructureFriction,
                                        IList<SobekExtraResistance> sobekExtraFriction)
        {
            this.channels = channels;
            this.sobekStructureFriction = sobekStructureFriction;
            this.locations = locations;

            structurebuilders = new List<IBranchStructureBuilder>
                                    {
                                        new WeirBuilder(sobekCrossSectionDefinitions),
                                        new PumpBuilder(),
                                        new BridgeBuilder(sobekCrossSectionDefinitions),
                                        new CulvertBuilder(sobekCrossSectionDefinitions,sobekValveDataTables)
                                    };

            compoundMapping = new Dictionary<string, SobekCompoundStructure>();
            foreach (var sobekCompoundStructure in compoundStructures)
            {
                compoundMapping[sobekCompoundStructure.Id] = sobekCompoundStructure;
            }

            // Convert the list to lookup tables
            // Note: we could use the IEnumerable.ToDictionary(..) here but it's possible the lists contains duplicates

            var mlist = mappings as List<SobekStructureMapping> ?? new List<SobekStructureMapping>(mappings);
            this.mappings = new Dictionary<string, SobekStructureMapping>();
            foreach (var m in mlist)
            {
                if (this.mappings.ContainsKey(m.StructureId))
                {
                    log.Warn( $"Duplicate structure definition statements for " +
                              $"id = {m.StructureId}, " +
                              $"overwriting definition with latest values : " +
                              $"DefId = \"{m.DefinitionId ?? string.Empty}\", " +
                              $"Description = \"{m.Name ?? string.Empty}\", " +
                              $"Controller Ids = \"{(m.ControllerIDs != null ? string.Join(", ", m.ControllerIDs) : string.Empty)}\"");
                }

                this.mappings[m.StructureId] = m;
            }
            
            var dlist = definitions as List<SobekStructureDefinition> ?? new List<SobekStructureDefinition>(definitions);
            this.definitions = new Dictionary<string, SobekStructureDefinition>();
            dlist.ForEach(d => this.definitions[d.Id] = d);

            var elist = sobekExtraFriction as List<SobekExtraResistance> ?? new List<SobekExtraResistance>(sobekExtraFriction);
            this.sobekExtraFriction = new Dictionary<string, SobekExtraResistance>();
            elist.ForEach(ef => this.sobekExtraFriction[ef.Id] = ef);
        }

        #endregion

        /// <summary>
        /// Validates if the location found in network.st has a mapping in struct.dat. If not valid then it will be logged
        /// </summary>
        private bool CheckIfLocationHasStructureMapping(string locationId)
        {
            var isValid = !(mappings.ContainsKey(locationId) || compoundMapping.ContainsKey(locationId));

            if (isValid)
            {
                Log.WarnFormat("No mapping of structure found with id = {0}.", locationId);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the mapping of the structure on the branch.
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        private bool CheckIfLocationToDefinitionMappingIsValid(string locationId)
        {
            if (mappings.ContainsKey(locationId))
            {
                var definitionId = mappings[locationId].DefinitionId;
                if (!definitions.ContainsKey(definitionId))
                {
                    Log.WarnFormat("No definition with id = {0} for structure {1}.", definitionId, locationId);
                    return false;
                }
            }
            else
            {
                var compound = compoundMapping[locationId];
                // Sobek 2.* id found in struct.cmp is prefixed with id of compound -> 3##1 refers to definition ##1
                foreach (var mapping in compound.Structures)
                {
                    var definitionId = mappings[mapping].DefinitionId;
                    if (!definitions.ContainsKey(definitionId))
                    {
                        Log.WarnFormat("No definition with id = {0} for structure {1}.", definitionId, locationId);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Constructs the structures and associates them with the channels on the network
        /// </summary>
        public void SetStructuresOnChannels()
        {
            SetStructuresOnChannels(new List<IStructure1D>());
        }

        /// <summary>
        /// Constructs or replace the structures and associates them with the channels on the network
        /// </summary>
        public void SetStructuresOnChannels(IEnumerable<IStructure1D> existingStructures)
        {
            foreach (var location in locations)
            {
                bool hasMappingInStructDat = CheckIfLocationHasStructureMapping(location.ID);
                if (!hasMappingInStructDat)
                {
                    // Locations of extra resistances in sobek are stored in network.st but there is no related 
                    // data io either strcut.dat of strcut.def. The extra resistance is stored in friction.dat
                    HandleStructureNotInStructDat(location, existingStructures);
                }
                else
                {
                    if (!CheckIfLocationToDefinitionMappingIsValid(location.ID))
                    {
                        continue;
                    }
                    if (location.BranchID.Trim() == "-1")
                    {
                        // structure not linked to branch but part of compound structure
                        continue;
                    }
                    if (!channels.ContainsKey(location.BranchID))
                    {
                        log.ErrorFormat("Can not add structure {0} to branch; carrier id {1} not found.",
                                        location.ID, location.BranchID);
                        continue;
                    }
                    var branch = channels[location.BranchID];
                    var offset = branch is ISewerConnection sewerConnection && sewerConnection.IsInternalConnection() ? 0  : location.Offset;

                    if (offset > branch.Length)
                    {
                        log.ErrorFormat("The chainage of structure '{0} - {1}' is out of the branch length. The chainage has been set from {2} to {3}.", location.ID, location.Name, offset, branch.Length);
                        offset = branch.Length;
                    }

                    IGeometry geometry = GeometryHelper.GetPointGeometry(branch, offset);

                    ICompositeBranchStructure compositeStructure = CreateCompositeStructureAndAddItToTheBranch(branch,
                                                                           offset, geometry);

                    compositeStructure.Name = location.ID + " [compound]"; // to prevent duplicates with substructure
                    compositeStructure.LongName = location.Name;

                    // definition found, add to the network
                    if (location.IsCompound)
                    {
                        var compound = compoundMapping[location.ID];
                        foreach (var structure in compound.Structures)
                        {
                            var definitionMapping = mappings[structure];

                            // Sobek 2.12: Structures in a compound are not written to SobekNetworkStructuresFileName (NETWORK.ST)
                            // For ALL structures in a compound, names are written to SobekStructureDatFileName (STRUCT.DAT)
                            // Therefore name of definitionMapping is never empty for a structure in a compound

                            // Sobek RE: In SobekStructureDatFileName (DEFSTR.2) no names are written
                            // Therefore name of definitionMapping is always empty
                            // Use the names from SobekNetworkStructuresFileName (DEFSTR.1) ie the name of the sublocation

                            string structureName = "";
                            if (definitionMapping.Name == "")
                            {
                                var subLocation = locations.Where(lc => lc.ID == structure && !lc.IsCompound).FirstOrDefault(); 
                                // Note that in Sobek RE a compound structure can have the same ID as a structure that is not a compound
                                // Therefore searching by just ID is not enough
                                if (subLocation != null)
                                {
                                    structureName = subLocation.Name;
                                }
                            }
                            else
                            {
                                structureName = definitionMapping.Name;
                            }

                            ProcesStruct(location, branch, definitionMapping, compositeStructure, existingStructures, structureName);
                        }
                    }
                    else
                    {
                        var definitionMapping = mappings[location.ID];

                        // Sobek 2.12: For some structures like a River Weir the name is not written to SobekStructureDatFileName (STRUCT.DAT)
                        // Structures in a compound are not written to SobekNetworkStructuresFileName (NETWORK.ST)
                        // Use the name of the location when it is not a structure in a compound

                        // Sobek RE: In SobekStructureDatFileName (DEFSTR.2) no names are written
                        // ALL structures are written to SobekNetworkStructuresFileName (DEFSTR.1)
                        // Always use the name of the location

                        string structureName = location.Name;

                        ProcesStruct(location, branch, definitionMapping, compositeStructure, existingStructures, structureName);
                    }

                    if (!compositeStructure.Structures.Any())
                    {
                        // no structures could be generated and the component structure should be removed again from the branch
                        branch.BranchFeatures.Remove(compositeStructure);
                    }
                }
            }
        }

        private void HandleStructureNotInStructDat(SobekStructureLocation location, IEnumerable<IStructure1D> existingStructures)
        {
            if (sobekExtraFriction.ContainsKey(location.ID))
            {

                SobekExtraResistance sobekExtraResistance = sobekExtraFriction[location.ID];
                var channel = channels[location.BranchID];
                IGeometry geometry = GeometryHelper.GetPointGeometry(channel, location.Offset);
                ICompositeBranchStructure compositeStructure = CreateCompositeStructureAndAddItToTheBranch(channel,
                                                                                                           location.Offset, geometry);
                var extraResistance = new ExtraResistance
                                          {
                                              Name = location.ID,
                                          };
                FunctionHelper.AddDataTableRowsToFunction(sobekExtraResistance.Table, extraResistance.FrictionTable);

                AddOrReplaceStructure(extraResistance, compositeStructure, existingStructures);
            }
        }

        private void ProcesStruct(SobekStructureLocation location, IBranch branch, SobekStructureMapping definitionMapping, ICompositeBranchStructure compositeStructure, IEnumerable<IStructure1D> existingStructures, string structureName)
        {
            SobekStructureDefinition sobekStructureDefinition = definitions[definitionMapping.DefinitionId];

            var structures = structurebuilders.SelectMany(sb => sb.GetBranchStructures(sobekStructureDefinition));

            foreach (var structure in structures)
            {
                if(structure is IPump)
                {
                    //Import pumps with many start/stop levels results in more then one pump. The can not have the same name, otherwise they will override each other
                    //Structure name set by Pumpbuilder will be set as "1", "2","3" etc for each pump per start/stop level
                    structure.Name = definitionMapping.StructureId + structure.Name;
                }
                else
                {
                    structure.Name = definitionMapping.StructureId;
                }

                structure.LongName = structureName;

                var structureFriction = sobekStructureFriction.FirstOrDefault(c => c.StructureDefinitionID == definitionMapping.DefinitionId);

                switch (structure)
                {
                    case IWeir weir:
                    {
                        var channel = branch as Channel;
                        if (channel != null)
                        {
                            SetWeirOffSetY(weir, channel, location);
                        }
                        else
                        {
                            log.WarnFormat("Couldn't set the offset of weir {0} because its not a channel.", structure.Name);
                        }

                        break;
                    }
                    case IBridge bridge:
                        SetBridgeFriction(bridge, structureFriction);
                        break;
                    case ICulvert culvert:
                        SetCulvertFriction(culvert, structureFriction);
                        break;
                }

                AddOrReplaceStructure(structure, compositeStructure, existingStructures);
            }
        }

        private static void AddOrReplaceStructure(BranchStructure structure, ICompositeBranchStructure compositeStructure, IEnumerable<IStructure1D> existingStructures)
        {
            var existingStructure = existingStructures.Where(bf => bf.GetType() == structure.GetType() && bf.Name == structure.Name).FirstOrDefault();
            if (existingStructure != null)
            {
                existingStructure.CopyFrom(structure);
                if (existingStructure.ParentStructure != compositeStructure)
                {
                    existingStructure.ParentStructure.Structures.Remove(existingStructure);
                    existingStructure.Branch.BranchFeatures.Remove(existingStructure);
                    if (existingStructure.ParentStructure.Structures.Count == 0)
                    {
                        existingStructure.ParentStructure.Branch.BranchFeatures.Remove(existingStructure.ParentStructure);
                    }
                    HydroNetworkHelper.AddStructureToComposite(compositeStructure, existingStructure);
                }
            }
            else
            {
                HydroNetworkHelper.AddStructureToComposite(compositeStructure, structure);
            }
        }

        private static void SetBridgeFriction(IBridge bridge, SobekStructureFriction structureFriction)
        {
            if (structureFriction == null)
            {
                bridge.FrictionType = BridgeFrictionType.Chezy;
                bridge.Friction = 45.0;
                log.DebugFormat("Friction of bridge {0} not found in import file; set default type Chezy and value 45.",
                                bridge.Name);
                return;
            }
            if (structureFriction.MainFrictionFunctionType != SobekFrictionFunctionType.Constant)
            {
                bridge.FrictionType = BridgeFrictionType.Chezy;
                bridge.Friction = 45.0;
                log.DebugFormat("Only constant friction for structures supported." +
                                "{0} Friction of bridge {1} set default type Chezy and value 45.",
                                structureFriction.MainFrictionFunctionType, bridge.Name);
                return;
            }

            try
            {
                bridge.FrictionType = (BridgeFrictionType)structureFriction.MainFrictionType;
                bridge.Friction = structureFriction.MainFrictionConst;
                if (structureFriction.GroundLayerFrictionType != structureFriction.MainFrictionType)
                {
                    if (bridge.GroundLayerEnabled)
                    {
                        //should be the same
                        log.WarnFormat(
                            "Bridge '{2}': Bed friction type (={0}) and groundlayer friction type (={1}) should be the same. Groundlayer roughness was set to 0.",
                            (BridgeFrictionType)structureFriction.MainFrictionType,
                            (BridgeFrictionType)structureFriction.GroundLayerFrictionType,
                            bridge.Name);
                        bridge.GroundLayerRoughness = 0.0;
                    }
                }
                else
                {
                    bridge.GroundLayerRoughness = structureFriction.GroundLayerFrictionValue;
                }
            }
            catch (Exception e)
            {
                log.Warn($"Bridge '{bridge.Name}': Bed friction type (={structureFriction.MainFrictionType}) does not exits for bridges. Because of {e.Message}");
            }

        }

        private static void SetCulvertFriction(ICulvert culvert, SobekStructureFriction structureFriction)
        {
            if (structureFriction == null)
            {
                culvert.FrictionType = CulvertFrictionType.Chezy;
                culvert.Friction = 45.0;
                log.DebugFormat("Friction of culvert {0} not found in import file; set default type Chezy and value 45.",
                                culvert.Name);
                return;
            }

            if (structureFriction.MainFrictionFunctionType != SobekFrictionFunctionType.Constant)
            {
                culvert.FrictionType = CulvertFrictionType.Chezy;
                culvert.Friction = 45.0;
                log.DebugFormat("Only constant friction for structures supported." +
                                "{0} Friction of culvert {1} set default type Chezy and value 45.",
                                structureFriction.MainFrictionFunctionType, culvert.Name);
            }

            try
            {
                culvert.FrictionType = (CulvertFrictionType)structureFriction.MainFrictionType;
                culvert.Friction = structureFriction.MainFrictionConst;

                if (structureFriction.GroundLayerFrictionType != structureFriction.MainFrictionType)
                {
                    if (culvert.GroundLayerEnabled)
                    {
                        //should be the same
                        log.WarnFormat(
                            "Culvert '{2}': Bed friction type (={0}) and groundlayer friction type (={1}) should be the same. Groundlayer roughness was set to 0.",
                            (CulvertFrictionType)structureFriction.MainFrictionType,
                            (CulvertFrictionType)structureFriction.GroundLayerFrictionType,
                            culvert.Name);
                    }
                    culvert.GroundLayerRoughness = 0.0;
                }
                else
                {
                    culvert.GroundLayerRoughness = structureFriction.GroundLayerFrictionValue;
                }
            }
            catch (Exception e)
            {
                log.Warn($"Culvert:'{culvert.Name}': Bed friction type (={structureFriction.MainFrictionType}) does not exits for culverts. Because of {e.Message}");
            }
            
        }

        /// <summary>
        /// Tries to determine an optimal y offset for the structure to be used in views in DeltaShell. 
        /// This should not be done if the imported structure has the y values given in the source.
        /// - freeformweir
        /// </summary>
        /// <param name="weir"></param>
        /// <param name="channel"></param>
        /// <param name="location"></param>
        private static void SetWeirOffSetY(IWeir weir, IChannel channel, SobekBranchLocation location)
        {
            double crossSectionWidth = 300;
            double crossSectionOffset = 100;
            if (channel.CrossSections.Count() != 0)
            {
                // if branch has cross section, find nearest and use it to update 
                // the y-coordinate related values
                var nearestCrossSection = channel.CrossSections.OrderBy(o => Math.Abs( o.Chainage - location.Offset)).First();
                crossSectionWidth = nearestCrossSection.Definition.Width;
                crossSectionOffset = nearestCrossSection.Definition.Left;
            }
            if (!(weir.WeirFormula is FreeFormWeirFormula))
            {
                weir.OffsetY = crossSectionOffset + (crossSectionWidth / 2) - (weir.CrestWidth / 2);
            }
        }


        public static ICompositeBranchStructure CreateCompositeStructureAndAddItToTheBranch(IBranch branch, double offset, IGeometry geometry)
        {
            var compositeStructure = new CompositeBranchStructure
                                         {
                                             Network = branch.Network,
                                             Chainage = offset,
                                             Geometry = geometry
                                         };

            NetworkHelper.AddBranchFeatureToBranch(compositeStructure, branch, compositeStructure.Chainage);
            return compositeStructure;
        }
    }
}