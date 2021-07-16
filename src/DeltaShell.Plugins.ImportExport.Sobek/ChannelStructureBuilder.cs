using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.ImportExport.Sobek.Builders;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    /// <summary>
    /// This class is responsible for the construction of structures and adding them to the network channels
    /// </summary>
    public class ChannelStructureBuilder
    {
        private static ILog log = LogManager.GetLogger(typeof(ChannelStructureBuilder));
        protected static readonly ILog Log = LogManager.GetLogger(typeof(ChannelStructureBuilder));
        private readonly IList<IBranchStructureBuilder> structurebuilders;
        private readonly IDictionary<string, IChannel> channels;
        private readonly IDictionary<string, SobekStructureDefinition> definitions;
        private readonly IEnumerable<SobekStructureLocation> locations;
        private readonly IDictionary<string, SobekStructureMapping> mappings; // structure mappings from STRUCT.DAT
        private readonly IDictionary<string, SobekCompoundStructure> compoundMapping;
        private readonly IList<SobekStructureFriction> sobekStructureFriction;
        private readonly IDictionary<string, SobekExtraResistance> sobekExtraFriction;

        #region Constructors



        public ChannelStructureBuilder(IDictionary<string, IChannel> channels,
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
                this.mappings[m.StructureId] = m;
            }
            //mlist.ForEach(m => this.mappings[m.StructureId] = m);

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
        /// Validates the mapping of the structure on the channel.
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
                    var channel = channels[location.BranchID];
                    var offset = location.Offset;

                    if (offset > channel.Length)
                    {
                        log.ErrorFormat("The chainage of structure '{0} - {1}' is out of the branch length. The chainage has been set from {2} to {3}.", location.ID, location.Name, offset, channel.Length);
                        offset = channel.Length;
                    }

                    IGeometry geometry = GeometryHelper.GetPointGeometry(channel, offset);

                    ICompositeBranchStructure compositeStructure = CreateCompositeStructureAndAddItToTheBranch(channel,
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

                            ProcesStruct(location, channel, definitionMapping, compositeStructure, existingStructures, structureName);
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

                        ProcesStruct(location, channel, definitionMapping, compositeStructure, existingStructures, structureName);
                    }

                    if (!compositeStructure.Structures.Any())
                    {
                        // no structures could be generated and the component structure should be removed again from the branch
                        channel.BranchFeatures.Remove(compositeStructure);
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

        private void ProcesStruct(SobekStructureLocation location, IChannel channel, SobekStructureMapping definitionMapping, ICompositeBranchStructure compositeStructure, IEnumerable<IStructure1D> existingStructures, string structureName)
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

                if (structure is IWeir)
                {
                    SetWeirOffSetY((IWeir)structure, channel, location);
                }
                if (structure is IBridge)
                {
                    SetBridgeFriction((IBridge)structure, structureFriction);
                }
                if (structure is ICulvert)
                {
                    SetCulvertFriction((ICulvert)structure, structureFriction);
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
            bridge.FrictionType = (BridgeFrictionType)structureFriction.MainFrictionType;
            bridge.Friction = structureFriction.MainFrictionConst;
            if (structureFriction.GroundLayerFrictionType != structureFriction.MainFrictionType)
            {
                if (bridge.GroundLayerEnabled)
                {
                    //should be the same
                    log.WarnFormat(
                        "Bridge '{2}': Bed friction type (={0}) and groundlayer friction type (={1}) should be the same. Groundlayer roughness was set to 0.",
                        (CulvertFrictionType) structureFriction.MainFrictionType,
                        (CulvertFrictionType) structureFriction.GroundLayerFrictionType,
                        bridge.Name);
                    bridge.GroundLayerRoughness = 0.0;
                }
            }
            else
            {
                bridge.GroundLayerRoughness = structureFriction.GroundLayerFrictionValue;
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
                // if channel has cross section, find nearest and use it to update 
                // the y-coordinate related values
                //throw new InvalidOperationException("Does not look OK.Use Abs");
                var nearestCrossSection = channel.CrossSections.OrderBy(o => Math.Abs( o.Chainage - location.Offset)).First();
                crossSectionWidth = nearestCrossSection.Definition.Width;
                crossSectionOffset = nearestCrossSection.Definition.Left;
            }
            if (!(weir.WeirFormula is FreeFormWeirFormula))
            {
                weir.OffsetY = crossSectionOffset + (crossSectionWidth / 2) - (weir.CrestWidth / 2);
            }
        }


        public static ICompositeBranchStructure CreateCompositeStructureAndAddItToTheBranch(IChannel channel, double offset, IGeometry geometry)
        {
            var compositeStructure = new CompositeBranchStructure
                                         {
                                             Network = channel.Network,
                                             Chainage = offset,
                                             Geometry = geometry
                                         };

            NetworkHelper.AddBranchFeatureToBranch(compositeStructure, channel, compositeStructure.Chainage);
            return compositeStructure;
        }

        private static void GetCrossSectionDimensions(ICrossSectionDefinition crossSectionDefinition, ref float crossSectionOffset, ref float crossSectionWidth)
        {
            var yzValues = crossSectionDefinition.Profile;
            if (yzValues.Count() <= 0)
            {
                return;
            }
            crossSectionWidth = (float)(yzValues.Max(yz => yz.X) - yzValues.Min(yz => yz.X));
            crossSectionOffset = (float)yzValues.Min(yz => yz.X);
        }
    }
}