using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.Utils;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.Properties;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekCrossSectionsImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekCrossSectionsImporter));

        private Dictionary<string, IBranch> branches;
        
        /// <summary>
        /// crossSectionUsage helps to set CRFR records to all cross sections that use a crossSectionDefinition
        /// </summary>
        private readonly Dictionary<string, IList<ICrossSection>> crossSectionUsage = new Dictionary<string, IList<ICrossSection>>();

        /// <summary>
        /// Maps crosssection to used crossSectionDefinition. Used to map BDFR records where the BDFR record holds friction
        /// for floodplain but floodplain dimensions are stored in crossSectionDefinition
        /// </summary>
        private readonly Dictionary<ICrossSection, SobekCrossSectionDefinition> crossSection2Definition = new Dictionary<ICrossSection, SobekCrossSectionDefinition>();

        private string displayName = "Cross-sections";
        public override string DisplayName
        {
            get { return displayName; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

        protected override void PartialImport()
        {
            log.DebugFormat("Importing cross-sections ...");

            var flowFmModel = TryGetModel<WaterFlowFMModel>();
            if (flowFmModel != null)
            { 
                flowFmModel.UnSubscribeFromNetwork(flowFmModel.Network); // performance optimization, no need to listen to network events
            }

            AddCrossSections();

            log.DebugFormat("Importing friction section types ...");
            AddFrictionSectionTypes();

            if (flowFmModel != null)
            {
                flowFmModel.SubscribeToNetwork(flowFmModel.Network);
                flowFmModel.UpdateRoughnessSections();
            }
        }

        private void AddCrossSections()
        {
            if (HydroNetwork.Branches.Count == 0)
            {
                log.Error("Network has no branches; can not import cross sections.");
                return;
            }

            // network.cr: contains locations of the several cross sections
            var locationsPath = GetFilePath(SobekFileNames.SobekNetworkLocationsFileName);
            IList<SobekBranchLocation> locations = new SobekCrossSectionsReader().Read(locationsPath).ToList();

            // profile.def: geometry of the cross section
            // four types currently supported:
            // ZW, YZ, Standard (Rectangle, Round)
            var crossSectionDefinitionReader = new CrossSectionDefinitionReader();

            //profile.dat: ref level and geometry of cross section at locaton
            List<SobekCrossSectionMapping> mappings = new SobekProfileDatFileReader().Read(GetFilePath(SobekFileNames.SobekProfileDataFileName)).ToList();

            //create lookup dictionaries
            Dictionary<string, SobekBranchLocation> locationLookup = locations.ToDictionaryWithErrorDetails(locationsPath, l => l.ID);
            // cleanup locations to preserve space
            locations.Clear();
            var crossSectionLookup = HydroNetwork.CrossSections.ToDictionary(cs => cs.Name);
            var defPath = GetFilePath(SobekFileNames.SobekProfileDefinitionsFileName);
            var crossSectionDefinitionsLookup = crossSectionDefinitionReader.Read(defPath)
                                                        .ToDictionaryWithDuplicateLogging(defPath, d => d.ID);

            if (mappings.Count > 0)
            {
                branches = HydroNetwork.Branches.ToDictionary(b => b.Name);
            }

            Dictionary<string, ICrossSectionDefinition> definitionIDToDefinition = GetDefinitionIDToDefinitionDictionary(mappings, crossSectionDefinitionsLookup, HydroNetwork);

            var initiatedEditing = false;
            var errorList = new Dictionary<string, IList<string>>();
            var warningList = new Dictionary<string, IList<string>>();
            
            void LogError(string key, string value)
            {
                errorList.AddToList(key, value);
            }

            void LogWarning(string key, string value)
            {
                warningList.AddToList(key, value);
            }

            try
            {
                if (!HydroNetwork.IsEditing)
                {
                    initiatedEditing = true;
                    HydroNetwork.BeginEdit(new DefaultEditAction("Importing cross sections"));
                }

                foreach (var sobekCrossSectionMapping in mappings)
                {
                    if (!locationLookup.ContainsKey(sobekCrossSectionMapping.LocationId))
                    {
                        LogWarning("No locations found with the following id's", sobekCrossSectionMapping.LocationId);
                        continue;
                    }

                    var location = locationLookup[sobekCrossSectionMapping.LocationId];

                    if (!branches.ContainsKey(location.BranchID))
                    {
                        LogError("Could not import cross-sections because branch doesn't exist.", $"id \"{sobekCrossSectionMapping.LocationId}\", branch \"{location.BranchID}\"");
                        continue;
                    }

                    var branch = branches[location.BranchID];

                    if (!crossSectionDefinitionsLookup.ContainsKey(sobekCrossSectionMapping.DefinitionId))
                    {
                        // definition not found or not supported type; ignore and do not use default type which is misleading.
                        // For pipe & sewer connections set defaults
                        LogWarning(Resources.SobekCrossSectionsImporter_AddCrossSections_Definition_with_the_following_ids_were_not_found__ignored__Using_default, sobekCrossSectionMapping.DefinitionId);
                        branch.GenerateDefaultProfileForSewerConnections();
                        continue;
                    }

                    var definition = definitionIDToDefinition[sobekCrossSectionMapping.DefinitionId];

                    if (definition == null)
                    {
                        continue;
                    }

                    var offset = location.Offset;

                    if (offset > branch.Length)
                    {
                        LogError(
                            "The chainage of cross-section is out of the branch length. The chainage has been set from to branch length.",
                            $"loc id \"{sobekCrossSectionMapping.LocationId}\", chainage \"{sobekCrossSectionMapping.LocationId}\", branch length \"{branch.Length}\"");

                        offset = branch.Length;
                    }

                    if (branch is ISewerConnection sewerConnection)
                    {
                        SetSewerConnectionProperties(sewerConnection, definition, sobekCrossSectionMapping);
                    }
                    else
                    {
                        //since during creation the cs is added to the network immediately, delete existing cs first
                        if (crossSectionLookup.ContainsKey(location.ID))
                        {
                            var existingBranch = HydroNetwork.Branches.FirstOrDefault(b => b.Name == branch.Name);
                            if (existingBranch != null)
                            {
                                existingBranch.BranchFeatures.Remove(crossSectionLookup[location.ID]);
                            }
                        }

                        var definitionToUse = (ICrossSectionDefinition)definition.Clone();

                        ICrossSection crossSection = new CrossSection(definitionToUse);
                        NetworkHelper.AddBranchFeatureToBranch(crossSection, branch, offset);

                        crossSection.SetNameWithoutUpdatingDefinition(location.ID);
                        crossSection.LongName = location.Name;
                        crossSection2Definition[crossSection] =
                            crossSectionDefinitionsLookup[sobekCrossSectionMapping.DefinitionId];

                        // Reference level as used in sobek is not stored in cross section; correct z values.
                        if (crossSection.Definition.IsProxy)
                        {
                            ((CrossSectionDefinitionProxy)crossSection.Definition).LevelShift =
                                sobekCrossSectionMapping.RefLevel1;
                        }
                        else if (sobekCrossSectionMapping.RefLevel1 != 0.0)
                        {
                            crossSection.Definition.ShiftLevel(sobekCrossSectionMapping.RefLevel1);
                        }

                        // link the cross section to the definition so we can later update the friction
                        crossSectionUsage.AddToList(sobekCrossSectionMapping.DefinitionId, crossSection);
                    }
                }

                ClearUnusedCrossSectionSectionTypes();//but why???
            }
            finally
            {
                foreach (var warning in warningList)
                {
                    log.Warn($"{warning.Key} {Environment.NewLine} {string.Join(Environment.NewLine, warning.Value)}");
                }

                foreach (var error in errorList)
                {
                    log.Error($"{error.Key} {Environment.NewLine} {string.Join(Environment.NewLine, error.Value)}");
                }

                if (initiatedEditing)
                {
                    HydroNetwork.EndEdit();
                }
            }
        }

        private void SetSewerConnectionProperties(ISewerConnection sewerConnection, ICrossSectionDefinition definition, SobekCrossSectionMapping sobekCrossSectionMapping)
        {
            if (definition.Sections != null && !definition.Sections.Any())
            {
                var sewerCrossSectionType = HydroNetwork.CrossSectionSectionTypes.FirstOrDefault(csst => csst.Name.Equals(RoughnessDataSet.SewerSectionTypeName));
                if (sewerCrossSectionType == null) return;
                if (definition.IsProxy)
                {
                    ((CrossSectionDefinitionProxy)definition).InnerDefinition.AddSection(sewerCrossSectionType, ((CrossSectionDefinitionProxy)definition).InnerDefinition.FlowWidth());
                }
                else
                {
                    definition.AddSection(sewerCrossSectionType, definition.FlowWidth());
                }
            }

            sewerConnection.CrossSection = new CrossSection(definition);
            sewerConnection.LevelSource = sobekCrossSectionMapping.RefLevel2;
            sewerConnection.LevelTarget = sobekCrossSectionMapping.RefLevel1;

        }

        private static Dictionary<string, ICrossSectionDefinition> GetDefinitionIDToDefinitionDictionary(IEnumerable<SobekCrossSectionMapping> mappings, Dictionary<string, SobekCrossSectionDefinition> sobekCrossSectionDefinitionsLookup, IHydroNetwork hydroNetwork)
        {
            var definitionIDToDefinition = new Dictionary<string, ICrossSectionDefinition>();

            Dictionary<string, int> definitionCount = mappings.GroupBy(m => m.DefinitionId)
                                                              .ToDictionaryWithErrorDetails("sobek cross section mapping", m => m.Key, m => m.Count());

            foreach (KeyValuePair<string, SobekCrossSectionDefinition> definitionMapping in sobekCrossSectionDefinitionsLookup)
            {
                string definitionID = definitionMapping.Key;

                int usageCount;
                if (!definitionCount.TryGetValue(definitionID, out usageCount)) //unused definition, don't import
                {
                    continue; //unused definition, don't import
                }

                ICrossSectionDefinition crossSectionDefinition = CreateDefinition(definitionMapping.Value);

                if (crossSectionDefinition != null)
                {
                    SetSectionsToDefinition(crossSectionDefinition, definitionMapping.Value, hydroNetwork);
                }
                else
                {
                    usageCount = 1;
                }
                
                if (usageCount == 1)
                {
                    //make local definition
                    definitionIDToDefinition.Add(definitionMapping.Key, crossSectionDefinition);
                }
                else
                {
                    //make global definition
                    hydroNetwork.SharedCrossSectionDefinitions.Add(crossSectionDefinition);
                    definitionIDToDefinition.Add(definitionMapping.Key, new CrossSectionDefinitionProxy(crossSectionDefinition));
                }
            }
            return definitionIDToDefinition;
        }

        private static void SetSectionsToDefinition(ICrossSectionDefinition definition, SobekCrossSectionDefinition sobekDefinition, IHydroNetwork hydroNetwork)
        {
            if (definition.CrossSectionType == CrossSectionType.ZW)
            {
                var main = GetCrossSectionSectionType(RoughnessDataSet.MainSectionTypeName, hydroNetwork);

                if (sobekDefinition.IsRiverProfile)
                {
                    var floodPlain1 = GetCrossSectionSectionType(RoughnessDataSet.Floodplain1SectionTypeName, hydroNetwork);
                    var floodPlain2 = GetCrossSectionSectionType(RoughnessDataSet.Floodplain2SectionTypeName, hydroNetwork);
                    SetBedFrictionToTabulatedProfile(main, floodPlain1, floodPlain2,
                                                        definition as CrossSectionDefinitionZW,
                                                        sobekDefinition);
                }
                else
                {
                    //only a main section:
                    var width = definition.Width/2.0;
                    var offset = 0.0;
                    AddFriction(definition, true, main, ref offset, width);
                }
            }
        }

        private static ICrossSectionDefinition CreateDefinition(SobekCrossSectionDefinition sobekCrossSectionDefinition)
        {
            switch (sobekCrossSectionDefinition.Type)
            {
                case SobekCrossSectionDefinitionType.Yztable:
                    return GetCrossSectionDefinitionYZ(sobekCrossSectionDefinition);
                case SobekCrossSectionDefinitionType.Tabulated:
                    return GetCrossSectionDefinitionZW(sobekCrossSectionDefinition);
                case SobekCrossSectionDefinitionType.AsymmetricalTrapezoidal:
                    return GetCrossSectionDefinitionYZ(sobekCrossSectionDefinition);
                case SobekCrossSectionDefinitionType.ClosedCircle:
                    return GetCrossSectionDefinitionClosedCircle(sobekCrossSectionDefinition);
                case SobekCrossSectionDefinitionType.EggShapedWidth:
                    return GetCrossSectionDefinitionEggShape(sobekCrossSectionDefinition);
                case SobekCrossSectionDefinitionType.Trapezoidal:
                    return GetCrossSectionDefinitionTrapezoidal(sobekCrossSectionDefinition);
                default:
                    log.ErrorFormat(@"Cross section of type {0} for id {1} not supported; ignored",
                                    sobekCrossSectionDefinition.Type, sobekCrossSectionDefinition.ID);
                    return null;
            }
        }
        
        private static ICrossSectionDefinition GetCrossSectionDefinitionTrapezoidal(SobekCrossSectionDefinition sobekCrossSectionDefinition)
        {
            if (sobekCrossSectionDefinition.BedWidth < 0 || sobekCrossSectionDefinition.Slope < 0 || sobekCrossSectionDefinition.MaxFlowWidth < 0)
            {
                return GetCrossSectionDefinitionZW(sobekCrossSectionDefinition);
            }

            var crossSectionStandardShapeTrapezium = new CrossSectionStandardShapeTrapezium
            {
                BottomWidthB = sobekCrossSectionDefinition.BedWidth,
                MaximumFlowWidth = sobekCrossSectionDefinition.MaxFlowWidth,
                Slope = sobekCrossSectionDefinition.Slope
            };
            var crossSectionDefinitionTrapezium = new CrossSectionDefinitionStandard(crossSectionStandardShapeTrapezium)
            {
                Name = sobekCrossSectionDefinition.Name
            };

            CrossSectionHelper.SetDefaultThalweg(crossSectionDefinitionTrapezium);

            return crossSectionDefinitionTrapezium;
        }

        private static ICrossSectionDefinition GetCrossSectionDefinitionEggShape(SobekCrossSectionDefinition sobekCrossSectionDefinition)
        {
            if (sobekCrossSectionDefinition.Width < 0)
            {
                return GetCrossSectionDefinitionZW(sobekCrossSectionDefinition);
            }

            var crossSectionStandardShapeEgg = new CrossSectionStandardShapeEgg
            {
                Width = sobekCrossSectionDefinition.Width
            };
            var crossSectionDefinitionEgg = new CrossSectionDefinitionStandard(crossSectionStandardShapeEgg)
            {
                Name = sobekCrossSectionDefinition.Name
            };

            CrossSectionHelper.SetDefaultThalweg(crossSectionDefinitionEgg);

            return crossSectionDefinitionEgg;
        }

        private static ICrossSectionDefinition GetCrossSectionDefinitionClosedCircle(SobekCrossSectionDefinition sobekCrossSectionDefinition)
        {
            if (sobekCrossSectionDefinition.Radius < 0)
            {
                return GetCrossSectionDefinitionZW(sobekCrossSectionDefinition);
            }

            var crossSectionStandardShapeRound = new CrossSectionStandardShapeCircle
                                                     {
                                                         Diameter = sobekCrossSectionDefinition.Radius*2
                                                     };
            var crossSectionDefinitionClosedCircle = new CrossSectionDefinitionStandard(crossSectionStandardShapeRound)
            {
                Name = sobekCrossSectionDefinition.Name
            };

            CrossSectionHelper.SetDefaultThalweg(crossSectionDefinitionClosedCircle);

            return crossSectionDefinitionClosedCircle;
        }
        
        private static ICrossSectionDefinition GetCrossSectionDefinitionZW(SobekCrossSectionDefinition sobekCrossSectionDefinition)
        {
            ICrossSectionStandardShape scs = AttemptToInferStandardType(sobekCrossSectionDefinition);
            if (scs != null)
            {
                var cs = new CrossSectionDefinitionStandard(scs) {Name = sobekCrossSectionDefinition.ID};
                CrossSectionHelper.SetDefaultThalweg(cs);
                return cs;
            }

            var crossSectionDefinitionZW = new CrossSectionDefinitionZW
                                     {
                                         Name = sobekCrossSectionDefinition.ID
                                     };

            crossSectionDefinitionZW.BeginEdit(new DefaultEditAction("Create new cross section definition"));
            
            var table = new FastZWDataTable();
            
            table.BeginLoadData();
            foreach (var row in sobekCrossSectionDefinition.TabulatedProfile)
            {
                double storageWidth = row.TotalWidth - row.FlowWidth;

                if (storageWidth < 0.0)
                {
                    log.WarnFormat("FlowWidth exceeds TotalWidth for cross section definition {0}. The FlowWidth has been set to the TotalWidth.", crossSectionDefinitionZW.Name);
                    storageWidth = 0.0;
                }

                table.AddCrossSectionZWRow(row.Height, row.TotalWidth, storageWidth);
            }
            table.EndLoadData();

            crossSectionDefinitionZW.ZWDataTable = table;

            crossSectionDefinitionZW.EndEdit();
            
            CrossSectionHelper.SetDefaultThalweg(crossSectionDefinitionZW);

            //summerdike data
            if (sobekCrossSectionDefinition.SummerDikeActive)
            {
                var summerDike = new SummerDike
                                     {
                                         Active = true,
                                         CrestLevel = sobekCrossSectionDefinition.CrestLevel,
                                         FloodPlainLevel = sobekCrossSectionDefinition.FloodPlainLevel,
                                         FloodSurface = sobekCrossSectionDefinition.FlowArea,
                                         TotalSurface = sobekCrossSectionDefinition.TotalArea
                                     };

                crossSectionDefinitionZW.SummerDike = summerDike;
            }
            return crossSectionDefinitionZW;
        }

        /// <summary>
        /// Function tries to infer cross section standard type from named fields that was extracted by
        /// the sobek cross section definition reader. For this to work:
        /// - the name must start with an identifier, and
        /// - the named fields must provide values for the type
        /// </summary>
        /// <param name="sobekCrossSectionDefinition"></param>
        /// <param name="crossSectionDefinitionZW"></param>
        /// <returns></returns>
        private static ICrossSectionStandardShape AttemptToInferStandardType(SobekCrossSectionDefinition sobekCrossSectionDefinition)
        {
            if (sobekCrossSectionDefinition.Name.Length < 2)
                return null;

            switch (sobekCrossSectionDefinition.Name.Substring(0, 2))
            {
                case "a_": // arch
                    if (!(sobekCrossSectionDefinition.ArcHeight < 0 || sobekCrossSectionDefinition.Height < 0 || sobekCrossSectionDefinition.Width < 0))
                    {
                        return new CrossSectionStandardShapeArch
                                   {
                                       ArcHeight = sobekCrossSectionDefinition.ArcHeight,
                                       Height = sobekCrossSectionDefinition.Height,
                                       Width = sobekCrossSectionDefinition.Width
                                   };
                    }
                    break;
                case "c_": // cunette
                    if (!(sobekCrossSectionDefinition.Width < 0))
                    {
                        return new CrossSectionStandardShapeCunette
                                   {
                                       Height = sobekCrossSectionDefinition.Height,
                                       Width = sobekCrossSectionDefinition.Width
                                   };
                    }
                    break;
                case "e_": // elliptical
                    if (!(sobekCrossSectionDefinition.Height < 0 || sobekCrossSectionDefinition.Width < 0))
                    {
                        return new CrossSectionStandardShapeElliptical
                                   {

                                       Height = sobekCrossSectionDefinition.Height,
                                       Width = sobekCrossSectionDefinition.Width
                                   };
                    }
                    break;
                case "r_": // rectangle
                    if (!(sobekCrossSectionDefinition.Height < 0 || sobekCrossSectionDefinition.Width < 0))
                    {
                        return new CrossSectionStandardShapeRectangle
                                   {
                                       Height = sobekCrossSectionDefinition.Height,
                                       Width = sobekCrossSectionDefinition.Width,
                                       Closed = sobekCrossSectionDefinition.IsTabulatedProfileClosedRectangularShape
                                   };
                    }
                    break;
                case "s_": // steel cunette
                    if (!(sobekCrossSectionDefinition.Height < 0 || sobekCrossSectionDefinition.RadiusR < 0 || sobekCrossSectionDefinition.RadiusR1 < 0 || 
                        sobekCrossSectionDefinition.RadiusR2 < 0 || sobekCrossSectionDefinition.RadiusR3 < 0 || sobekCrossSectionDefinition.AngleA < 0 || 
                        sobekCrossSectionDefinition.AngleA1 < 0))
                    {
                        return new CrossSectionStandardShapeSteelCunette
                                   {
                                       AngleA = sobekCrossSectionDefinition.AngleA,
                                       AngleA1 = sobekCrossSectionDefinition.AngleA1,
                                       Height = sobekCrossSectionDefinition.Height,
                                       RadiusR = sobekCrossSectionDefinition.RadiusR,
                                       RadiusR1 = sobekCrossSectionDefinition.RadiusR1,
                                       RadiusR2 = sobekCrossSectionDefinition.RadiusR2,
                                       RadiusR3 = sobekCrossSectionDefinition.RadiusR3
                                   };
                    }
                    break;
            }
            return null;
        }

        private static ICrossSectionDefinition GetCrossSectionDefinitionYZ(SobekCrossSectionDefinition sobekCrossSectionDefinition)
        {
            var crossSectionDefinitionYZ = new CrossSectionDefinitionYZ
                                               {
                                                   Name = sobekCrossSectionDefinition.Name  
                                               };
            

            crossSectionDefinitionYZ.BeginEdit(new DefaultEditAction("Set YZ data"));


            var newYZDataTable = new FastYZDataTable();
            newYZDataTable.BeginLoadData();

            foreach (var coordinate in sobekCrossSectionDefinition.YZ)
            {
                newYZDataTable.AddCrossSectionYZRow(coordinate.X, coordinate.Y);
            }

            newYZDataTable.EndLoadData();
            crossSectionDefinitionYZ.YZDataTable = newYZDataTable;
            
            crossSectionDefinitionYZ.EndEdit();

            CrossSectionHelper.SetDefaultThalweg(crossSectionDefinitionYZ);

            return crossSectionDefinitionYZ;
        }

        private void AddFrictionSectionTypes()
        {
            var frictionFile = GetFilePath(SobekFileNames.SobekFrictionFileName);
            if (!File.Exists(frictionFile))
            {
                log.WarnFormat("Friction file [{0}] not found; skipping...", frictionFile);
                return;
            }

            var sobekFriction = new SobekFrictionDatFileReader().ReadSobekFriction(frictionFile);

            if (HydroNetwork.CrossSections.Count(cs => cs.CrossSectionType == CrossSectionType.ZW) > 0)
            {
                //make sure these types are added to the network
                GetCrossSectionSectionType(RoughnessDataSet.MainSectionTypeName, HydroNetwork);
                GetCrossSectionSectionType(RoughnessDataSet.Floodplain1SectionTypeName, HydroNetwork);
                GetCrossSectionSectionType(RoughnessDataSet.Floodplain2SectionTypeName, HydroNetwork);
            }

            // process CRFR (Cross Section Friction) records
            // In Sobek friction for yz profiles can be defined in
            // CRFR Cross section friction -> RS00, RS01, ..
            // BDFR bed friction, Main, Floodplain1, Floodplain2
            // GLFR global friction, Main, Floodplain1, Floodplain2
            var crossSectionsPerBranch = new Dictionary<IBranch, IList<DelftTools.Utils.Tuple<ICrossSection, SobekCrossSectionFriction>>>();
            foreach (var sobekCrossSectionFriction in sobekFriction.CrossSectionFrictionList)
            {
                if (!crossSectionUsage.ContainsKey(sobekCrossSectionFriction.CrossSectionID))
                {
                    log.WarnFormat("Friction CRFR {0} is linked to non existing or non used cross section definition {1}; ignored.",
                                   sobekCrossSectionFriction.ID, sobekCrossSectionFriction.CrossSectionID);
                    continue;
                }
                // a CRFR record can be used by >1 cross sections
                var crossSections = crossSectionUsage[sobekCrossSectionFriction.CrossSectionID];
                foreach (var crossSection in crossSections)
                {
                    crossSectionsPerBranch.AddToList(crossSection.Branch, new DelftTools.Utils.Tuple<ICrossSection, SobekCrossSectionFriction>(crossSection, sobekCrossSectionFriction));
                }
            }

            // now we have per branch a list with cross secion and imported CRFR record
            foreach (var crossSections in crossSectionsPerBranch)
            {
                var roughnessTypePerBranchSection = new Dictionary<string, RoughnessType>();
                foreach (var tuple in crossSections.Value.OrderBy(t => t.First.Chainage))
                {
                    SetFrictionToCrossSection(roughnessTypePerBranchSection, tuple.First.Definition, tuple.Second);
                }
            }

            // all friction data has been read.
            // tabulated will have CrossSectionSection based on main, floodplain1 and floodplain2
            // yz with crfr record present will have RS00, RS01, ...
            // The Main section will be used there the roughness values will be set from the main roughness of the branches in the SobekRoughnessImporter
            var unhandledProfiles =
                HydroNetwork.CrossSections.Where(
                    cs => cs.Definition.Sections.Count == 0);

            if (unhandledProfiles.Any())
            {
                var crossSectionSectionType = GetCrossSectionSectionType(RoughnessDataSet.MainSectionTypeName, HydroNetwork); 

                foreach (var crossSection in unhandledProfiles)
                {
                    crossSection.Definition.Sections.Add(new CrossSectionSection
                                                             {
                                                                 MinY = crossSection.Definition.Left,
                                                                 MaxY =
                                                                     crossSection.Definition.Left +
                                                                     crossSection.Definition.Width,
                                                                 SectionType = crossSectionSectionType
                                                             });
                }
            }
        }

        private void ClearUnusedCrossSectionSectionTypes()
        {
            foreach(var crossSectionSectionType in HydroNetwork.CrossSectionSectionTypes.ToList())
            {
                var used = HydroNetwork.CrossSections.Select(crossSection => crossSection.Definition.Sections.FirstOrDefault(s => s.SectionType.Name == crossSectionSectionType.Name)).Any(section => section != null);
                if(!used)
                {
                    HydroNetwork.CrossSectionSectionTypes.Remove(crossSectionSectionType);
                }
            }
        }

        /// <summary>
        /// Add a roughness section to the cross section given the input parameters and increments the
        /// offset (in the cross section) the width of the section.
        /// </summary>
        /// <param name="crossSectionDefinition"></param>
        /// <param name="add"></param>
        /// <param name="crossSectionSectionType"></param>
        /// <param name="offset"></param>
        /// <param name="sectionWidth"></param>
        private static void AddFriction(ICrossSectionDefinition crossSectionDefinition, bool add, CrossSectionSectionType crossSectionSectionType,
            ref double offset, double sectionWidth)
        {
            if (!add)
            {
                return;
            }

            crossSectionDefinition.Sections.Add(new CrossSectionSection
            {
                MinY = offset,
                MaxY = offset + sectionWidth,
                SectionType = crossSectionSectionType
            });
            offset += sectionWidth;
        }

        private static void SetBedFrictionToTabulatedProfile(CrossSectionSectionType main, CrossSectionSectionType floodPlain1, CrossSectionSectionType floodPlain2, CrossSectionDefinitionZW crossSectionDefinition, SobekCrossSectionDefinition sobekCrossSectionDefinition)
        {
            var mainWidth = sobekCrossSectionDefinition.MainChannelWidth;
            var fpl1Width = sobekCrossSectionDefinition.FloodPlain1Width;
            var fpl2Width = sobekCrossSectionDefinition.FloodPlain2Width;
            var width = mainWidth + fpl1Width + fpl2Width;
            
            crossSectionDefinition.AddSection(main, mainWidth);
            if (fpl1Width > 0.0) crossSectionDefinition.AddSection(floodPlain1, fpl1Width);
            if (fpl2Width > 0.0) crossSectionDefinition.AddSection(floodPlain2, fpl2Width);

            var flowWidth = crossSectionDefinition.FlowWidth();
            if (width < flowWidth && Math.Abs(width - flowWidth) > 1e-10)
            {
                crossSectionDefinition.RefreshSectionsWidths();
            }
        }

        /// <summary>
        /// Set Sobek Cross section friction (CRFR) to imported cross section.
        /// 1 - create segments (start, end, CrossSectionsectionType) in cross section
        /// 2 - set the appropriate roughness value and type to the corresponding roughness coverage
        /// </summary>
        /// <param name="crossSectionDefinition"></param>
        /// <param name="sobekCrossSectionFriction"></param>
        private void SetFrictionToCrossSection(IDictionary<string, RoughnessType> roughnessTypePerBranchSection,
            ICrossSectionDefinition crossSectionDefinition, SobekCrossSectionFriction sobekCrossSectionFriction)
        {
            var usedFriction = new Dictionary<DelftTools.Utils.Tuple<RoughnessType, double>, string>();

            crossSectionDefinition.Sections.Clear();
            crossSectionDefinition.Sections.AddRange(sobekCrossSectionFriction.Segments
                .Select(s =>
                    {
                        var name = GetSectionTypeName(roughnessTypePerBranchSection, usedFriction, new DelftTools.Utils.Tuple<RoughnessType, double>(s.FrictionType, s.Friction));
                        return new CrossSectionSection
                            {
                                MinY = s.Start,
                                MaxY = s.End,
                                SectionType = GetCrossSectionSectionType(name, HydroNetwork)
                            };
                    }));
        }

        private static CrossSectionSectionType GetCrossSectionSectionType(string sectionTypeName, IHydroNetwork hydroNetwork)
        {
            var crossSectionSectionType = hydroNetwork.CrossSectionSectionTypes.FirstOrDefault(cst => cst.Name == sectionTypeName);
            if (crossSectionSectionType == null)
            {
                crossSectionSectionType = new CrossSectionSectionType { Name = sectionTypeName };
                hydroNetwork.CrossSectionSectionTypes.Add(crossSectionSectionType);
            }
            return crossSectionSectionType;
        }

        /// <summary>
        /// Returns the name of the section from usedFriction that has exactly the same type and value.
        /// </summary>
        /// <param name="roughnessTypePerBranchSection"></param>
        /// <param name="usedFriction">
        /// dictionary with pairs of frictiontype and frictionvalue 
        /// </param>
        /// <param name="roughness">
        /// pair of frictiontype and frictionvalue to be checked
        /// </param>
        /// <returns></returns>
        private static string GetSectionTypeName(IDictionary<string, RoughnessType> roughnessTypePerBranchSection,
            IDictionary<DelftTools.Utils.Tuple<RoughnessType, double>, string> usedFriction,
            DelftTools.Utils.Tuple<RoughnessType, double> roughness)
        {
            if (usedFriction.ContainsKey(roughness))
            {
                // combination has been used reuse section
                return usedFriction[roughness];
            }
            // combination of type value not found get next free sectionname. 
            // Extra requirement is the section may not be used for the current branch with another roughness type
            var count = usedFriction.Count;
            while (true)
            {
                if (roughnessTypePerBranchSection.ContainsKey(GetSectionName(count)))
                {
                    if ((roughnessTypePerBranchSection[GetSectionName(count)] == roughness.First)
                        && (!usedFriction.Values.Contains(GetSectionName(count))))
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
                count++;
            }
            
            var newSection = GetSectionName(count);
            usedFriction[roughness] = newSection;
            roughnessTypePerBranchSection[newSection] = roughness.First;
            return newSection;
        }

        private static string GetSectionName(int count)
        {
            return string.Format(DelftTools.Hydro.HydroNetwork.CrossSectionSectionFormat, count);
        }
    }
}
