using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.Utils;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRoughnessImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRoughnessImporter));
        private const RoughnessType DefaultRoughnessType = RoughnessType.Chezy;
        private const double DefaultRoughnessValue = 45.0;
        
        IDictionary<string, IDictionary<NetworkLocation, DelftTools.Utils.Tuple<double, int>>> sectionTypeLocations = new Dictionary<string, IDictionary<NetworkLocation, DelftTools.Utils.Tuple<double, int>>>();
        private string displayName = "Friction (roughness)";

        public override string DisplayName
        {
            get { return displayName; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

        protected override void PartialImport()
        {

            log.DebugFormat("Importing roughness data ...");

            var frictionFile = GetFilePath(SobekFileNames.SobekFrictionFileName);
            if (!File.Exists(frictionFile))
            {
                log.WarnFormat("Friction file [{0}] not found; skipping...", frictionFile);
                return;
            }

            var fmModel = GetModel<WaterFlowFMModel>();
            if (fmModel == null)
            {
                throw new InvalidOperationException();
            }
            
            var main = GetCrossSectionSectionType(RoughnessDataSet.MainSectionTypeName);
            var floodPlain1 = GetCrossSectionSectionType(RoughnessDataSet.Floodplain1SectionTypeName);
            var floodPlain2 = GetCrossSectionSectionType(RoughnessDataSet.Floodplain2SectionTypeName);
            var sewerFriction = GetCrossSectionSectionType(RoughnessDataSet.SewerSectionTypeName);

            if (SobekFileNames.SobekGlobalFrictionFileName != "")
            {
                // In sobekRE the global friction record GLFR is stored in DEFFRC.4; other friction (BDFR, CRFR, STFR) in DEFFRC.1
                var globalfrictionFile = GetFilePath(SobekFileNames.SobekGlobalFrictionFileName);
                if (!File.Exists(globalfrictionFile))
                {
                    log.WarnFormat("Friction file [{0}] not found; skipping...", globalfrictionFile);
                    return;
                }
            }

            var sobekFriction = new SobekFrictionDatFileReader().ReadSobekFriction(frictionFile);
            
            if (main != null && floodPlain1 != null && floodPlain2 != null)
            {
                SetMainAndFloodPlainRoughness(main, floodPlain1, floodPlain2, sobekFriction);
            }

            SetSewerRoughness(sewerFriction, sobekFriction);

            if (ShouldCancel)
            {
                return;
            }

            SetCrossSectionFrictionsToRoughnessCoverages(sobekFriction);

            var converter = new SobekToWaterFlowFMRoughnessConverter();
            converter.ConvertSobekRoughnessToWaterFlowFmRoughness(fmModel.ChannelFrictionDefinitions,
                fmModel.RoughnessSections.First(rs => rs.CrossSectionSectionType.Name == RoughnessDataSet.MainSectionTypeName),
                fmModel.Network);
        }

        private void SetCrossSectionFrictionsToRoughnessCoverages(SobekFriction sobekFriction)
        {
            Dictionary<IBranch, IList<DelftTools.Utils.Tuple<INetworkLocation, SobekCrossSectionFriction>>> crossSectionRoughnessPerBranch = GetCrossSectionRoughnessPerBranch(sobekFriction);

            // now we have per branch a list with cross section and imported CRFR record
            foreach (var crossSections in crossSectionRoughnessPerBranch)
            {
                if (ShouldCancel)
                {
                    return;
                }

                var roughnessTypePerBranchSection = new Dictionary<string, RoughnessType>();
                foreach (var tuple in crossSections.Value.OrderBy(t => t.First.Chainage))
                {
                    SetFrictionToCrossSectionLocation(roughnessTypePerBranchSection, tuple.First, tuple.Second);
                }
            }

            // set values to coverages
            for (int i = 0; i < sectionTypeLocations.Count; i++)
            {
                if (ShouldCancel)
                {
                    return;
                }

                var d =
                    new SortedDictionary<NetworkLocation, DelftTools.Utils.Tuple<double, int>>(sectionTypeLocations.Values.ElementAt(i));

                var roughnessSection = GetRoughnessSection(sectionTypeLocations.Keys.ElementAt(i));

                if (roughnessSection == null)
                {
                    continue;
                }

                var coverage = roughnessSection.RoughnessNetworkCoverage;

                if (coverage == null)
                {
                    continue;
                }

                try
                {
                    coverage.SkipInterpolationForNewLocation = true;
                    foreach (var key in d.Keys)
                    {
                        var v = d[key];
                        coverage[key] = new[] { v.First, v.Second };

                        if (ShouldCancel)
                        {
                            return;
                        }
                    }
                }
                finally
                {
                    coverage.SkipInterpolationForNewLocation = false;
                }
            }
        }

        private Dictionary<IBranch, IList<DelftTools.Utils.Tuple<INetworkLocation, SobekCrossSectionFriction>>> GetCrossSectionRoughnessPerBranch(SobekFriction sobekFriction)
        {
            var profilePath = GetFilePath(SobekFileNames.SobekProfileDataFileName);
            var locationPath = GetFilePath(SobekFileNames.SobekNetworkLocationsFileName);
            var sobekCrossSectionMapping = new SobekProfileDatFileReader().Read(profilePath).ToDictionaryWithErrorDetails(profilePath, def => def.LocationId, def => def.DefinitionId);
            var sobekCrossSectionLocations = new SobekCrossSectionsReader().Read(GetFilePath(SobekFileNames.SobekNetworkLocationsFileName)).ToDictionaryWithErrorDetails(locationPath, l => l.ID, l => l);

            return GetCrossSectionRoughnessPerBranch(sobekCrossSectionLocations,
                                                    sobekCrossSectionMapping,
                                                    sobekFriction,
                                                    HydroNetwork.Branches.ToDictionary(b => b.Name, b => b),
                                                    GetFilePath(SobekFileNames.SobekProfileDefinitionsFileName));
        }

        private void SetMainAndFloodPlainRoughness(CrossSectionSectionType main, CrossSectionSectionType floodPlain1, CrossSectionSectionType floodPlain2, SobekFriction sobekFriction)
        {
            var waterFlowFMModel = GetModel<WaterFlowFMModel>();

            var sectionMain = GetRoughnessSection(main.Name);
            var sectionFloodplain1 = GetRoughnessSection(floodPlain1.Name);
            var sectionFloodplain2 = GetRoughnessSection(floodPlain2.Name);

            var channels = HydroNetwork.Channels.ToDictionary(b => b.Name, b => b);

            if (SobekBedFrictionContainsReverseRoughness(sobekFriction))
            {
                waterFlowFMModel.UseReverseRoughness = true;
            }

            var globalSobekFriction = sobekFriction;

            if (SobekFileNames.SobekGlobalFrictionFileName != "")
            {
                var globalfrictionFile = GetFilePath(SobekFileNames.SobekGlobalFrictionFileName);
                globalSobekFriction = new SobekFrictionDatFileReader().ReadSobekFriction(globalfrictionFile);

            }
            SetGlobalFrictionToRoughnessCoverages(globalSobekFriction, sectionMain, sectionFloodplain1,
                                                      sectionFloodplain2, true);

            if (waterFlowFMModel.UseReverseRoughness)
            {
                var reverseMain = waterFlowFMModel.RoughnessSections.GetApplicableReverseRoughnessSection(sectionMain);
                var reverseFp1 = waterFlowFMModel.RoughnessSections.GetApplicableReverseRoughnessSection(sectionFloodplain1);
                var reverseFp2 = waterFlowFMModel.RoughnessSections.GetApplicableReverseRoughnessSection(sectionFloodplain2);
                SetGlobalFrictionToRoughnessCoverages(globalSobekFriction, reverseMain, reverseFp1,
                                                      reverseFp2, false);
            }

            var warningList = new Dictionary<string, IList<string>>();

            void LogWarning(string key, string value)
            {
                warningList.AddToList(key, value);
            }

            // process all BDFR records
            foreach (var sobekBedFriction in sobekFriction.SobekBedFrictionList)
            {
                if (ShouldCancel)
                {
                    return;
                }

                if (!channels.ContainsKey(sobekBedFriction.BranchId))
                {
                    LogWarning("Friction BDFR is linked to branch that does not exist; ignored.", $"{sobekBedFriction.Id}, branch {sobekBedFriction.BranchId}");
                    continue;
                }
                var channel = channels[sobekBedFriction.BranchId];
                if (!channel.CrossSections.Any(c => c.CrossSectionType == CrossSectionType.ZW || c.CrossSectionType == CrossSectionType.Standard))
                {
                    LogWarning("Friction BDFR is linked to branch that does not exist; ignored.", $"{sobekBedFriction.Id}, branch {sobekBedFriction.BranchId}");
                    continue;
                }

                var sobekMainBedFrictionData = sobekBedFriction.MainFriction;

                if (sectionMain != null)
                {
                    SetPositiveAndNegativeRoughnessToSection(sectionMain, channel, sobekBedFriction.MainFriction, sobekMainBedFrictionData);
                }

                if (sectionFloodplain1 != null)
                {
                    SetPositiveAndNegativeRoughnessToSection(sectionFloodplain1, channel, sobekBedFriction.FloodPlain1Friction, sobekMainBedFrictionData);
                }

                if (sectionFloodplain2 != null)
                {
                    SetPositiveAndNegativeRoughnessToSection(sectionFloodplain2, channel, sobekBedFriction.FloodPlain2Friction, sobekMainBedFrictionData);
                }
            }
            coveragesWithInterpolationSet.Clear(); //clear our administration, we no longer care

            foreach (var kvp in warningList)
            {
                log.Warn(kvp.Key + Environment.NewLine + string.Join(Environment.NewLine, kvp.Value));
            }
        }

        private void SetSewerRoughness(CrossSectionSectionType sewerFriction, SobekFriction sobekFriction)
        {
            var waterFlowFMModel = GetModel<WaterFlowFMModel>();

            var sewerSection = GetRoughnessSection(sewerFriction.Name);

            var pipes = waterFlowFMModel.Network.Pipes.ToDictionary(p => p.Name, p => p);

            // process all BDFR records
            foreach (var sobekBedFriction in sobekFriction.SobekBedFrictionList)
            {
                if (ShouldCancel)
                {
                    return;
                }

                if (!pipes.ContainsKey(sobekBedFriction.BranchId))
                {
                    continue;
                }
                var pipe = pipes[sobekBedFriction.BranchId];

                if (sewerSection != null)
                {
                    SetRoughnessToSection(sewerSection, pipe, sobekBedFriction);
                }
            }
            coveragesWithInterpolationSet.Clear(); //clear our administration, we no longer care
        }

        private bool SobekBedFrictionContainsReverseRoughness(SobekFriction sobekFriction)
        {
            foreach (var sobekBedFriction in sobekFriction.SobekBedFrictionList)
            {
                var plains = new[]
                                 {
                                     sobekBedFriction.MainFriction, sobekBedFriction.FloodPlain1Friction,
                                     sobekBedFriction.FloodPlain2Friction
                                 };

                foreach (var sobekBedFrictionPlainData in plains)
                {
                    if (!sobekBedFrictionPlainData.Positive.Equals(sobekBedFrictionPlainData.Negative))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void SetPositiveAndNegativeRoughnessToSection(RoughnessSection section, IBranch branch, SobekBedFrictionData sobekBedFrictionData, SobekBedFrictionData sobekMainBedFrictionData)
        {
            section.RoughnessNetworkCoverage.SkipInterpolationForNewLocation = true;
            try
            {
                var waterFlowFMModel = GetModel<WaterFlowFMModel>();

                SetSobekBedFrictionToRoughnessCoverage(sobekBedFrictionData, sobekMainBedFrictionData,
                                                       true, section, branch);

                SetAndCheckInterpolationRoughnessCoverage(sobekBedFrictionData.Positive, section, branch);

                //is there a reverse roughness defined (not same as normal roughness)?
                if (waterFlowFMModel.UseReverseRoughness)
                {
                    var reverseRoughnessSection = waterFlowFMModel.RoughnessSections.GetApplicableReverseRoughnessSection(section);

                    bool shouldSetReverseRoughness = !sobekBedFrictionData.Positive.Equals(sobekBedFrictionData.Negative) ||
                                                     sobekBedFrictionData.FrictionType == SobekBedFrictionType.CopyOfMain;

                    if (reverseRoughnessSection.Reversed && shouldSetReverseRoughness)
                    {
                        ((ReverseRoughnessSection)reverseRoughnessSection).UseNormalRoughness = false;

                        SetSobekBedFrictionToRoughnessCoverage(sobekBedFrictionData, sobekMainBedFrictionData,
                                                               false, reverseRoughnessSection, branch);

                        SetAndCheckInterpolationRoughnessCoverage(sobekBedFrictionData.Negative, reverseRoughnessSection,
                                                                  branch);
                    }
                }
            }
            finally
            {
                section.RoughnessNetworkCoverage.SkipInterpolationForNewLocation = false;
            }
        }

        private void SetRoughnessToSection(RoughnessSection sewerSection, IPipe pipe, SobekBedFriction sobekBedFriction)
        {
            sewerSection.RoughnessNetworkCoverage.SkipInterpolationForNewLocation = true;
            try
            {
                sewerSection.RoughnessNetworkCoverage[new NetworkLocation(pipe, 0)] = new object[]
                {
                    sobekBedFriction.MainFriction.Positive.FrictionConst,
                    (int)sobekBedFriction.MainFriction.FrictionType
                };
            }
            finally
            {
                sewerSection.RoughnessNetworkCoverage.SkipInterpolationForNewLocation = false;
            }
        }

        private static void SetGlobalFrictionToRoughnessCoverages(SobekFriction sobekFriction, RoughnessSection sectionMain,
            RoughnessSection sectionFloodplain1, RoughnessSection sectionFloodplain2, bool usePositive)
        {
            double defaultRoughness = DefaultRoughnessValue;
            RoughnessType defaultRoughnessType = DefaultRoughnessType;

            if ((sectionMain == null) || (sectionFloodplain1 == null) || (sectionFloodplain2 == null))
            {
                return;
            }
            if (!sobekFriction.GlobalBedFrictionList.Any())
            {
                log.WarnFormat("No global friction available; set to to default {0}, {1}", DefaultRoughnessType, DefaultRoughnessValue);

                sectionMain.SetDefaults(DefaultRoughnessType, DefaultRoughnessValue);
                sectionFloodplain1.SetDefaults(DefaultRoughnessType, DefaultRoughnessValue);
                sectionFloodplain2.SetDefaults(DefaultRoughnessType, DefaultRoughnessValue);
                return;
            }

            // if the BDFR record is contained in the GLFR record the friction for the main branch is
            // also used for yz cross sections
            if (!Enum.IsDefined(typeof(RoughnessType), (int)sobekFriction.GlobalBedFrictionList.First().MainFriction.FrictionType))
            {

                log.WarnFormat("Default friction type {0} of frictionpart {1} is not supported. The default roughness has been set to {2} constant value {3}",
                    sobekFriction.GlobalBedFrictionList.First().MainFriction.FrictionType,
                    sectionMain.RoughnessNetworkCoverage.Name,
                    DefaultRoughnessType,
                    DefaultRoughnessValue
                    );

            }
            else
            {
                var directionData = usePositive
                                        ? sobekFriction.GlobalBedFrictionList.First().MainFriction.Positive
                                        : sobekFriction.GlobalBedFrictionList.First().MainFriction.Negative;

                defaultRoughness = directionData.FrictionConst;
                defaultRoughnessType = (RoughnessType)sobekFriction.GlobalBedFrictionList.First().MainFriction.FrictionType;
            }

            sectionMain.SetDefaults(defaultRoughnessType, defaultRoughness);
            SetSobekDefaultBedFrictionToCoverage(sectionFloodplain1,
                                       sobekFriction.GlobalBedFrictionList.First().FloodPlain1Friction.FrictionType ==
                                       SobekBedFrictionType.CopyOfMain
                                           ? sobekFriction.GlobalBedFrictionList.First().MainFriction
                                           : sobekFriction.GlobalBedFrictionList.First().FloodPlain1Friction, usePositive);

            SetSobekDefaultBedFrictionToCoverage(sectionFloodplain2,
                                       sobekFriction.GlobalBedFrictionList.First().FloodPlain2Friction.FrictionType ==
                                       SobekBedFrictionType.CopyOfMain
                                           ? sobekFriction.GlobalBedFrictionList.First().MainFriction
                                           : sobekFriction.GlobalBedFrictionList.First().FloodPlain2Friction, usePositive);
        }

        private static void SetSobekDefaultBedFrictionToCoverage(RoughnessSection roughnessSection, SobekBedFrictionData sobekBedFrictionData, bool usePositive)
        {
            //friction type not supported set default chezy and constant value 45
            if (!Enum.IsDefined(typeof(RoughnessType), (int)sobekBedFrictionData.FrictionType))
            {
                roughnessSection.SetDefaults(DefaultRoughnessType, DefaultRoughnessValue);

                log.WarnFormat("Default friction type {0} of frictionpart {1} is not supported. The default roughness has been set to {2} constant value {3}",
                    sobekBedFrictionData.FrictionType,
                    roughnessSection.Name,
                    DefaultRoughnessType,
                    DefaultRoughnessValue
                    );

                return;
            }

            var directionData = usePositive ? sobekBedFrictionData.Positive : sobekBedFrictionData.Negative;
            roughnessSection.SetDefaults((RoughnessType)sobekBedFrictionData.FrictionType, directionData.FrictionConst);
        }

        private static void SetSobekBedFrictionToRoughnessCoverage(SobekBedFrictionData sobekBedFrictionData, SobekBedFrictionData sobekMainBedFrictionData, bool usePositive,
            RoughnessSection roughnessSection, IBranch channel)
        {
            var bedFrictionData = sobekBedFrictionData;
            if (sobekBedFrictionData.FrictionType == SobekBedFrictionType.CopyOfMain)
            {
                bedFrictionData = sobekMainBedFrictionData;
            }

            var sobekBedFrictionDirectionData = usePositive ? bedFrictionData.Positive : bedFrictionData.Negative;

            //friction type not supported set default chezy and constant value 45
            if (!Enum.IsDefined(typeof(RoughnessType), (int)bedFrictionData.FrictionType))
            {

                roughnessSection.RoughnessNetworkCoverage[new NetworkLocation(channel, 0)] = new object[]
                                                                                        {
                                                                                            DefaultRoughnessValue,
                                                                                            DefaultRoughnessType
                                                                                        };

                log.WarnFormat("Friction type {0} of frictionpart {1} is not supported. The default roughness of branch {2} has been set to {3} constant value {4}",
                    sobekBedFrictionData.FrictionType,
                    roughnessSection.RoughnessNetworkCoverage.Name,
                    channel.Name,
                    DefaultRoughnessType,
                    DefaultRoughnessValue
                    );

                return;
            }

            switch (sobekBedFrictionDirectionData.FunctionType)
            {
                case SobekFrictionFunctionType.Constant:
                    roughnessSection.RoughnessNetworkCoverage[new NetworkLocation(channel, 0)] = new object[]
                                                                                        {
                                                                                            sobekBedFrictionDirectionData.FrictionConst,
                                                                                            (int)bedFrictionData.FrictionType
                                                                                        };
                    break;
                case SobekFrictionFunctionType.FunctionOfLocation:
                    foreach (DataRow row in sobekBedFrictionDirectionData.LocationTable.Rows)
                    {
                        roughnessSection.RoughnessNetworkCoverage[new NetworkLocation(channel, (double)row[0])] = new object[]
                                                                              {
                                                                                  (double) row[1],
                                                                                  (int)bedFrictionData.FrictionType
                                                                              };
                    }
                    break;
                case SobekFrictionFunctionType.FunctionOfH:
                    var functionOfH = roughnessSection.AddHRoughnessFunctionToBranch(channel);
                    // first column is H, next columns are chainage
                    ConvertSobekBedFrictionToFunction(sobekBedFrictionDirectionData.HTable, functionOfH);
                    AddFunctionNetworkLocationsToCoverage(channel, bedFrictionData, roughnessSection, functionOfH);
                    break;
                case SobekFrictionFunctionType.FunctionOfQ:
                    var functionOfQ = roughnessSection.AddQRoughnessFunctionToBranch(channel);
                    // first column is H, next columns are chainage
                    ConvertSobekBedFrictionToFunction(sobekBedFrictionDirectionData.QTable, functionOfQ);
                    AddFunctionNetworkLocationsToCoverage(channel, bedFrictionData, roughnessSection, functionOfQ);
                    break;
            }
        }

        private readonly IList<RoughnessNetworkCoverage> coveragesWithInterpolationSet = new List<RoughnessNetworkCoverage>();

        private void SetAndCheckInterpolationRoughnessCoverage(SobekBedFrictionDirectionData sobekBedFrictionDirectionData, RoughnessSection roughnessSection, IBranch channel)
        {
            var coverageName = roughnessSection.Name;

            var coverage = roughnessSection.RoughnessNetworkCoverage;

            if (!coveragesWithInterpolationSet.Contains(coverage) && sobekBedFrictionDirectionData.Interpolation != SobekBedFrictionDirectionData.InterpolationNotSetValue)
            {
                coverage.Arguments[0].InterpolationType = sobekBedFrictionDirectionData.Interpolation;
                coveragesWithInterpolationSet.Add(coverage);
                log.WarnFormat("Interpolation {0} (of branch {1} ({2})) has been set as network-wide interpolation for roughness section '{3}'. Only a single interpolation type for entire network is supported.",
                    sobekBedFrictionDirectionData.Interpolation, channel.Name, channel.Name, coverageName);
            }

            if (sobekBedFrictionDirectionData.Interpolation != coverage.Arguments[0].InterpolationType &&
                sobekBedFrictionDirectionData.Interpolation != SobekBedFrictionDirectionData.InterpolationNotSetValue)
            {
                log.WarnFormat("Interpolation {0} of branch {1} ({2}) cannot be set for roughness section '{3}'. Only one interpolation type supported for entire network. Interpolation is {4}.",
                    sobekBedFrictionDirectionData.Interpolation, channel.Name, channel.Name, coverageName,
                    coverage.Arguments[0].InterpolationType);
            }
        }

        /// <summary>
        /// Adds for all given chainages in the function of Q or H a network location to the roughness coverage
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="bedFrictionData"></param>
        /// <param name="roughnessSection"></param>
        /// <param name="functionOfH"></param>
        private static void AddFunctionNetworkLocationsToCoverage(IBranch channel, SobekBedFrictionData bedFrictionData, RoughnessSection roughnessSection, IFunction functionOfH)
        {
            foreach (double chainage in functionOfH.Arguments[0].Values)
            {
                var location = new NetworkLocation(channel, chainage);
                roughnessSection.RoughnessNetworkCoverage[location] = new[]
                                                                    {
                                                                        ((MultiDimensionalArray)(functionOfH[chainage])).MinValue,
                                                                        (int)bedFrictionData.FrictionType
                                                                    };
            }
        }

        private CrossSectionSectionType GetCrossSectionSectionType(string name)
        {
            var roughnessSection = HydroNetwork.CrossSectionSectionTypes.FirstOrDefault(csst => csst.Name == name);

            if (roughnessSection != null)
            {
                return roughnessSection;
            }

            roughnessSection = new CrossSectionSectionType { Name = name };
            HydroNetwork.CrossSectionSectionTypes.Add(roughnessSection);
            return roughnessSection;
        }

        private RoughnessSection GetRoughnessSection(string sectionTypeName)
        {
            var waterFlowFMModel = GetModel<WaterFlowFMModel>();

            var roughnessSection = waterFlowFMModel.RoughnessSections.FirstOrDefault(rs => string.Equals(rs.Name, sectionTypeName, StringComparison.InvariantCultureIgnoreCase));

            if (roughnessSection != null)
            {
                return roughnessSection;
            }

            var crossSectionSectionType = HydroNetwork.CrossSectionSectionTypes.FirstOrDefault(cst => cst.Name == sectionTypeName);
            if (crossSectionSectionType == null)
            {
                crossSectionSectionType = new CrossSectionSectionType() { Name = sectionTypeName };
                HydroNetwork.CrossSectionSectionTypes.Add(crossSectionSectionType);
            }

            roughnessSection = waterFlowFMModel.RoughnessSections.FirstOrDefault(rs => string.Equals(rs.Name, sectionTypeName, StringComparison.InvariantCultureIgnoreCase));
            if (roughnessSection == null)
            {
                roughnessSection = new RoughnessSection(crossSectionSectionType, HydroNetwork);
                waterFlowFMModel.RoughnessSections.Add(roughnessSection);
            }

            return roughnessSection;
        }

        private static void ConvertSobekBedFrictionToFunction(DataTable sobekBedFrictionData, IFunction function)
        {
            for (var c = 1; c < sobekBedFrictionData.Columns.Count; c++)
            {
                var chainage = double.Parse(sobekBedFrictionData.Columns[c].ColumnName, CultureInfo.InvariantCulture);
                for (var r = 0; r < sobekBedFrictionData.Rows.Count; r++)
                {
                    // sobekBedFriction.MainFriction.HTable.Columns[4].ColumnName
                    function[chainage, sobekBedFrictionData.Rows[r][0]] = sobekBedFrictionData.Rows[r][c];
                }
            }
        }

        /// <summary>
        /// Set Sobek Cross section friction (CRFR) to imported cross section.
        /// 1 - create segments (start, end, CrossSectionsectionType) in cross section
        /// 2 - set the appropriate roughness value and type to the corresponding roughness coverage
        /// </summary>
        /// <param name="networkLocation"></param>
        /// <param name="sobekCrossSectionFriction"></param>
        private void SetFrictionToCrossSectionLocation(IDictionary<string, RoughnessType> roughnessTypePerBranchSection,
            INetworkLocation networkLocation, SobekCrossSectionFriction sobekCrossSectionFriction)
        {
            IDictionary<DelftTools.Utils.Tuple<RoughnessType, double>, string> usedFriction = new Dictionary<DelftTools.Utils.Tuple<RoughnessType, double>, string>();

            var frictionSegmentCount = sobekCrossSectionFriction.Segments.Count;
            for (var i = 0; i < frictionSegmentCount; i++)
            {
                var sobekFrictionSegment = sobekCrossSectionFriction.Segments[i];

                string sectionName = "Main";

                if (!sobekCrossSectionFriction.IsSameAsMainFriction)
                {
                    sectionName = GetSectionTypeName(roughnessTypePerBranchSection, usedFriction,
                                                 new DelftTools.Utils.Tuple<RoughnessType, double>(sobekFrictionSegment.FrictionType,
                                                                                  sobekFrictionSegment.Friction));
                }

                SetRoughnessDataToRoughnessCoverage(networkLocation.Branch, networkLocation.Chainage, sobekFrictionSegment, sectionName);
            }
        }

        private void SetRoughnessDataToRoughnessCoverage(IBranch branch, double offset, SobekFrictionSegment sobekFrictionSegment, string sectionTypeName)
        {
            if (!sectionTypeLocations.ContainsKey(sectionTypeName))
            {
                sectionTypeLocations[sectionTypeName] = new Dictionary<NetworkLocation, DelftTools.Utils.Tuple<double, int>>();
            }

            var d = sectionTypeLocations[sectionTypeName];

            if (!Enum.IsDefined(typeof(RoughnessType), (int)sobekFrictionSegment.FrictionType))
            {
                d[new NetworkLocation(branch, offset)] = new DelftTools.Utils.Tuple<double, int>(DefaultRoughnessValue, (int)DefaultRoughnessType);

                log.WarnFormat(
                    "Friction type {0} of branch {1} is not supported. The roughness has been set to {2} constant value {3}",
                    sobekFrictionSegment.FrictionType,
                    branch.Name,
                    DefaultRoughnessType,
                    DefaultRoughnessValue
                    );
            }
            else
            {
                d[new NetworkLocation(branch, offset)] = new DelftTools.Utils.Tuple<double, int>(sobekFrictionSegment.Friction, (int)sobekFrictionSegment.FrictionType);
            }
        }

        private string GetSectionTypeName(IDictionary<string, RoughnessType> roughnessTypePerBranchSection,
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

        public static Dictionary<IBranch, IList<DelftTools.Utils.Tuple<INetworkLocation, SobekCrossSectionFriction>>> GetCrossSectionRoughnessPerBranch(IDictionary<string, SobekBranchLocation> sobekCrossSectionLocations, IDictionary<string, string> sobekCrossSectionMapping, SobekFriction sobekFriction, IDictionary<string, IBranch> branches, string crossSectionDefinitionReaderPath)
        {
            Dictionary<string, SobekCrossSectionDefinition> sobekCrossSectionDefinitions = null;
            var sobekCrossSectionFrictions = sobekFriction.CrossSectionFrictionList.ToDictionaryWithErrorDetails("friction file", csf => csf.CrossSectionID, csf => csf);
            var sobekBranchMainFrictions = sobekFriction.SobekBedFrictionList.ToDictionaryWithErrorDetails("friction file", bf => bf.BranchId, bf => bf.MainFriction);

            var crossSectionRoughnessPerBranch = new Dictionary<IBranch, IList<DelftTools.Utils.Tuple<INetworkLocation, SobekCrossSectionFriction>>>();

            var warningList = new Dictionary<string, IList<string>>();

            void LogWarning(string key, string value)
            {
                warningList.AddToList(key, value);
            }

            foreach (var crossSectionID in sobekCrossSectionLocations.Keys)
            {
                var crossSectionLocation = sobekCrossSectionLocations[crossSectionID];

                if (!branches.ContainsKey(crossSectionLocation.BranchID))
                {
                    LogWarning("Branch of the following locations doesn't exist. Feature has been skipped.",
                        $"branch \"{crossSectionLocation.BranchID}\", location \"{crossSectionLocation.ID} - {crossSectionLocation.Name}\"");
                    continue;
                }

                var branch = branches[crossSectionLocation.BranchID];
                if (crossSectionLocation.Offset > branch.Length)
                {
                    LogWarning("Offset of the following locations is out of branch length. Feature has been skipped.",
                        $"location \"{crossSectionLocation.ID} - {crossSectionLocation.Name} ({crossSectionLocation.Offset:N1})\", branch \"{crossSectionLocation.BranchID}, {branch.Length.ToString("N1")}\"");
                    continue;
                }

                if (!crossSectionRoughnessPerBranch.ContainsKey(branch))
                {
                    crossSectionRoughnessPerBranch[branch] = new List<DelftTools.Utils.Tuple<INetworkLocation, SobekCrossSectionFriction>>();
                }
                var crossSectionRoughnessPerBranchItem = crossSectionRoughnessPerBranch[branch];

                var definitionID = "";

                if (sobekCrossSectionMapping.ContainsKey(crossSectionID))
                {
                    definitionID = sobekCrossSectionMapping[crossSectionID];
                }
                else
                {
                    LogWarning("The following cross-sections have no definition. Look-up for roughness of these cross-sections have been skipped",  
                        $"Cross-section \"{crossSectionID}\"");
                    continue;
                }

                if (sobekCrossSectionFrictions.ContainsKey(definitionID))
                {
                    var sobekCrossSectionFriction = sobekCrossSectionFrictions[definitionID];
                    crossSectionRoughnessPerBranchItem.Add(new DelftTools.Utils.Tuple<INetworkLocation, SobekCrossSectionFriction>(new NetworkLocation(branch, crossSectionLocation.Offset), sobekCrossSectionFriction));
                }
                else
                {
                    if (sobekCrossSectionDefinitions == null)
                    {
                        var crossSectionDefinitionReader = new CrossSectionDefinitionReader();
                        sobekCrossSectionDefinitions = crossSectionDefinitionReader.Read(crossSectionDefinitionReaderPath).ToDictionaryWithDuplicateLogging(crossSectionDefinitionReaderPath, csDef => csDef.ID, csDef => csDef);
                    }
                    SobekCrossSectionDefinition sobekCrossSectionDefinition = null;

                    if (!sobekCrossSectionDefinitions.TryGetValue(definitionID, out sobekCrossSectionDefinition))
                    {
                        LogWarning("The following cross-section definitions were not found. Look-up for roughness of these cross-sections have been skipped", 
                            $"Cross-section definition \"{definitionID}\"");
                        continue;
                    }

                    if (sobekCrossSectionDefinition.YZ.Count == 0)
                    {
                        continue; //GetSobekCrossSectionFrictionBasedOnBranchFriction is for yz values
                    }

                    if (sobekBranchMainFrictions.ContainsKey(crossSectionLocation.BranchID))
                    {
                        LogWarning("Friction data for the following cross-section definitions have not been found. The main bed of branch is used as roughness data.", 
                            $"Definition \"{definitionID}\", cross-section \"{crossSectionID}\", branch \"{crossSectionLocation.BranchID}\"");
                        
                        var sobekCrossSectionFriction = GetSobekCrossSectionFrictionBasedOnBranchFriction(crossSectionLocation, sobekCrossSectionDefinitions[definitionID], sobekBranchMainFrictions[crossSectionLocation.BranchID]);

                        crossSectionRoughnessPerBranchItem.Add(new DelftTools.Utils.Tuple<INetworkLocation, SobekCrossSectionFriction>(new NetworkLocation(branch, crossSectionLocation.Offset), sobekCrossSectionFriction));
                    }
                    else if (sobekFriction.GlobalBedFrictionList.Any())
                    {
                        LogWarning("Friction data for the following cross-section definitions have not been found. The main bed of branch has not been found. The global friction will be used.",
                            $"Definition \"{definitionID}\", cross-section \"{crossSectionID}\", branch \"{crossSectionLocation.BranchID}\"");
                        
                        var sobekCrossSectionFriction = GetSobekCrossSectionFrictionBasedOnBranchFriction(crossSectionLocation, sobekCrossSectionDefinitions[definitionID], sobekFriction.GlobalBedFrictionList.First().MainFriction);

                        crossSectionRoughnessPerBranchItem.Add(new DelftTools.Utils.Tuple<INetworkLocation, SobekCrossSectionFriction>(new NetworkLocation(branch, crossSectionLocation.Offset), sobekCrossSectionFriction));
                    }
                    else
                    {
                       LogWarning("Friction data for the following cross-section definitions have not been found. The main bed of branch has not been found. Additionally no global friction has been found.",
                           $"Definition \"{definitionID}\", cross-section \"{crossSectionID}\", branch \"{crossSectionLocation.BranchID}\"");
                    }
                }
            }

            if (warningList.Any())
            {
                foreach (var kvp in warningList)
                {
                    log.Warn(kvp.Key + Environment.NewLine + string.Join(Environment.NewLine, kvp.Value));
                }
            }

            return crossSectionRoughnessPerBranch;
        }

        /// <summary>
        /// GetSobekCrossSectionFrictionBasedOnBranchFriction is only needed if the friction data is missing CrossSection Friction data for yz profiles
        /// </summary>
        /// <param name="crossSectionLocation"></param>
        /// <param name="sobekCrossSectionDefinition"></param>
        /// <param name="sobekBranchMainFriction"></param>
        /// <returns></returns>
        private static SobekCrossSectionFriction GetSobekCrossSectionFrictionBasedOnBranchFriction(SobekBranchLocation crossSectionLocation, SobekCrossSectionDefinition sobekCrossSectionDefinition, SobekBedFrictionData sobekBranchMainFriction)
        {
            var dataTableFormat = new DataTable("format");
            dataTableFormat.Columns.Add(new DataColumn("een", typeof(double)));
            dataTableFormat.Columns.Add(new DataColumn("twee", typeof(double)));

            var sobekCrossSectionFriction = new SobekCrossSectionFriction
            {
                ID = crossSectionLocation.ID,
                IsSameAsMainFriction = true
            };

            //section
            var row = dataTableFormat.NewRow();
            row[0] = sobekCrossSectionDefinition.YZ.First().X;
            row[1] = sobekCrossSectionDefinition.YZ.Last().X;
            sobekCrossSectionFriction.AddYSections(row);

            //value
            row = dataTableFormat.NewRow();
            row[0] = Convert.ToDouble(sobekBranchMainFriction.FrictionType, CultureInfo.InvariantCulture);
            row[1] = sobekBranchMainFriction.Positive.FrictionConst;
            sobekCrossSectionFriction.AddFrictionValues(row);
            return sobekCrossSectionFriction;
        }
    }
}
