using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders.Retention
{
    /// <summary>
    /// Class containing parsing logic for retention INI sections
    /// </summary>
    public static class RetentionSectionParser
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RetentionSectionParser));
        
        /// <summary>
        /// Parses the provided <paramref name="iniSections"/> to <see cref="IRetention"/>s
        /// and adds them to the provided <paramref name="network"/>
        /// </summary>
        /// <param name="iniSections">INI sections containing the retention information</param>
        /// <param name="network">Network to add parsed retentions to</param>
        /// <returns>Parsed retentions</returns>
        public static IEnumerable<IRetention> ParseIniSections(IEnumerable<IniSection> iniSections, IHydroNetwork network)
        {
            var retentionIniSections = iniSections
                          .Where(IsRetention)
                          .ToArray();

            if (!retentionIniSections.Any())
            {
                return Enumerable.Empty<IRetention>();
            }

            // make branch and node lookups to improve performance
            var branchLookUp = network.Branches.ToDictionaryWithDuplicateLogging("Branches", b => b.Name, b => b, comparer: StringComparer.InvariantCultureIgnoreCase);
            var nodeLookUp = network.Nodes.ToDictionaryWithDuplicateLogging("Nodes", b => b.Name, b => b, comparer: StringComparer.InvariantCultureIgnoreCase);

            return ParseIniSections(retentionIniSections, branchLookUp, nodeLookUp);
        }

        private static IEnumerable<IRetention> ParseIniSections(IEnumerable<IniSection> retentionIniSections, Dictionary<string, IBranch> branchLookUp, Dictionary<string, INode> nodeLookUp)
        {
            foreach (IniSection retentionIniSection in retentionIniSections)
            {
                if (ReadRetention(retentionIniSection, branchLookUp, nodeLookUp, out var retention))
                {
                    yield return retention;
                }
            }
        }

        private static bool ReadRetention(IniSection iniSection, IReadOnlyDictionary<string, IBranch> branchLookUp, IReadOnlyDictionary<string, INode> nodeLookUp, out IRetention retention)
        {
            retention = new DelftTools.Hydro.Retention
            {
                Name = iniSection.ReadProperty<string>(RetentionRegion.Id.Key),
                LongName = iniSection.ReadProperty<string>(RetentionRegion.Name.Key)
            };

            if (!TrySetBranchChainage(retention, iniSection, nodeLookUp, branchLookUp))
            {
                return false;
            }

            retention.Geometry = HydroNetworkHelper.GetStructureGeometry(retention.Branch, retention.Chainage);
            
            retention.UseTable = iniSection.ReadProperty<int>(RetentionRegion.NumLevels.Key) > 1;
            if (!retention.UseTable)
            {
                retention.BedLevel = iniSection.ReadProperty<double>(RetentionRegion.Levels.Key);
                retention.StorageArea = iniSection.ReadProperty<double>(RetentionRegion.StorageArea.Key);
                return true;
            }

            var interpolationTypeString = iniSection.ReadProperty<string>(RetentionRegion.Interpolate.Key);
            retention.Data.Arguments[0].InterpolationType = GetInterpolationType(interpolationTypeString);

            var levels = iniSection.ReadPropertiesToListOfType<double>(RetentionRegion.Levels.Key, true);
            var storageAreas = iniSection.ReadPropertiesToListOfType<double>(RetentionRegion.StorageArea.Key, true);

            if (levels == null || storageAreas == null)
                return true;

            retention.Data.Arguments[0].SetValues(levels);
            retention.Data.Components[0].SetValues(storageAreas);

            return true;
        }

        /// <summary>
        /// Tries to set branch and chainage on the <paramref name="retention"/>
        /// based on nodeId or branch chainage properties in the provided <paramref name="iniSection"/>
        /// </summary>
        /// <param name="retention">Retention to set</param>
        /// <param name="iniSection">Section containing read values</param>
        /// <param name="nodeLookUp">A lookup for quickly finding nodes by name</param>
        /// <param name="branchLookUp">A lookup for quickly finding branches by name</param>
        /// <returns>If location of the retention could be determined from the provided <paramref name="iniSection"/></returns>
        private static bool TrySetBranchChainage(IBranchFeature retention, IniSection iniSection, IReadOnlyDictionary<string, INode> nodeLookUp, IReadOnlyDictionary<string, IBranch> branchLookUp)
        {
            var nodeId = iniSection.ReadProperty<string>(RetentionRegion.NodeId.Key, isOptional: true);
            if (nodeId != null)
            {
                if (!nodeLookUp.TryGetValue(nodeId, out var node))
                {
                    log.Error(string.Format(Resources.RetentionSectionParser_TrySetBranchChainage_Could_not_find_node_with_nodeId__0__for_retention__1_, nodeId, retention.Name, iniSection.LineNumber));
                    return false;
                }

                var firstBranch = node.OutgoingBranches.Concat(node.IncomingBranches).FirstOrDefault();
                if (firstBranch == null)
                {
                    log.Error(string.Format(Resources.RetentionSectionParser_TrySetBranchChainage_Could_not_find_a_branch_for_node_with_nodeId__0___retention__1__, nodeId, retention.Name, iniSection.LineNumber));
                    return false;
                }

                retention.Branch = firstBranch;
                retention.Chainage = firstBranch.Target == node 
                                         ? firstBranch.Length 
                                         : 0;
                return true;
            }

            var branchId = iniSection.ReadProperty<string>(RetentionRegion.BranchId.Key);
            if (!branchLookUp.TryGetValue(branchId, out var branch))
            {
                log.Error(string.Format(Resources.RetentionSectionParser_TrySetBranchChainage_Could_not_find_branch_for_branch_id___0____retention__1__, branchId, retention.Name, iniSection.LineNumber));
                return false;
            }
            
            retention.Branch = branch;
            retention.Chainage = branch.GetBranchSnappedChainage(iniSection.ReadProperty<double>(RetentionRegion.Chainage.Key));

            return true;
        }

        private static InterpolationType GetInterpolationType(string interpolationTypeString)
        {
            switch (interpolationTypeString?.ToLower())
            {
                case "block": return InterpolationType.Constant;
                case "linear": return InterpolationType.Linear;
                default: return InterpolationType.None;
            }
        }
        
        private static bool IsRetention(IniSection iniSection)
        {
            if (iniSection.Name != RetentionRegion.StorageNodeHeader)
            {
                return false;
            }

            IniProperty useTableProperty = iniSection.GetProperty(RetentionRegion.UseTable.Key);
            if (useTableProperty != null)
            {
                return useTableProperty.ReadBooleanValue();
            }

            log.ErrorFormat(Resources.NodeFile_The_section_does_not_contain_property, 
                            iniSection.Name, iniSection.LineNumber, RetentionRegion.UseTable.Key);
            return false;
        }
    }
}