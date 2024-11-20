using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.Utils;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRetentionImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRetentionImporter));

        private string displayName = "Retentions";
        public override string DisplayName
        {
            get { return displayName; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

        protected override void PartialImport()
        {
            string retentionPath = GetFilePath(SobekFileNames.SobekNodeFileName);
            string netterFilePath = GetFilePath(SobekFileNames.SobekNetworkNetterFileName);

            var netterFileExists = File.Exists(netterFilePath);

            if (!File.Exists(retentionPath))
            {
                log.WarnFormat("Retention file [{0}] not found; skipping...", retentionPath);
                return;
            }

            var sobekRetentionsReader = new SobekRetentionsReader();
            sobekRetentionsReader.Sobek2Import = SobekType == DeltaShell.Sobek.Readers.SobekType.Sobek212;
            var sobekRetsobekRetentions = sobekRetentionsReader.Read(retentionPath).ToList();

            if (!sobekRetsobekRetentions.Any()) return;

            var nodes = HydroNetwork.Nodes.ToDictionary(n => n.Name, n => n);
            var lateralSources = HydroNetwork.LateralSources.ToDictionary(ls => ls.Name, ls => ls);
            var retentions = HydroNetwork.Retentions.ToDictionary(r => r.Name, r => r);

            if (SobekType == DeltaShell.Sobek.Readers.SobekType.Sobek212)
            {
                if (netterFileExists)
                {
                    ImportSobek212Retentions(sobekRetsobekRetentions, nodes, retentions, SobekNetworkNetterReader.ReadNodeTypes(netterFilePath));
                }
                else
                {
                    ImportSobek212Retentions(sobekRetsobekRetentions, nodes, retentions);
                }

            }
            else
            {
                ImportSobekRERetentions(sobekRetentionsReader, sobekRetsobekRetentions, retentions, lateralSources);
            }
        }
        
        private void ImportSobekRERetentions(SobekRetentionsReader sobekRetentionsReader, IEnumerable<Retention> sobekRetsobekRetentions, Dictionary<string, IRetention> retentions, Dictionary<string, ILateralSource> lateralSources)
        {

            string structureDefPath = GetFilePath(SobekFileNames.SobekStructureDefinitionFileName);
            string profileDefPath = GetFilePath(SobekFileNames.SobekProfileDefinitionsFileName);
            if (!File.Exists(structureDefPath))
            {
                log.WarnFormat("Structure definition file [{0}] not found; skipping...", structureDefPath);
                return;
            }

            if (!File.Exists(profileDefPath))
            {
                log.WarnFormat("Profile definition file [{0}] not found; skipping...", profileDefPath);
                return;
            }

            var definitions = new SobekStructureDefFileReader(SobekType).Read(structureDefPath).ToDictionaryWithErrorDetails(structureDefPath, d => d.Id, d => d);

            var crossSectionDefinitionReader = new CrossSectionDefinitionReader();
            var sobekCrossSectionDefinitions = new Dictionary<string, SobekCrossSectionDefinition>();
            foreach (var sobekCrossSectionDefinition in crossSectionDefinitionReader.Read(profileDefPath))
            {
                sobekCrossSectionDefinitions[sobekCrossSectionDefinition.ID] = sobekCrossSectionDefinition;
            }

            var sobekValveData = GetSobekValveData();


            var structurebuilders = new List<Builders.IBranchStructureBuilder>
                                        {
                                            new Builders.WeirBuilder(sobekCrossSectionDefinitions),
                                            new Builders.PumpBuilder(),
                                            new Builders.BridgeBuilder(sobekCrossSectionDefinitions),
                                            new Builders.CulvertBuilder(sobekCrossSectionDefinitions,sobekValveData)
                                        };

            var splitBranches = new List<ChannelToSplit>();

            foreach (var retention in sobekRetsobekRetentions)
            {
                if (retentions.ContainsKey(retention.Name))
                {
                    //To complex for updating (specially from RE)
                    log.ErrorFormat("Retention '{0}-{1}' already exists. Import skipped this retention", retention.Name, retention.LongName);
                    continue;
                }

                if (!lateralSources.ContainsKey(retention.Name))
                {
                    log.ErrorFormat("Retention '{0}-{1}' has no corresponding LateralSource. Import skipped this retention. Please import first the lateral  sources.", retention.Name, retention.LongName);
                    continue;
                }

                var lateralSource = lateralSources[retention.Name];

                retention.LongName = lateralSource.LongName;

                var line = new LengthIndexedLine(lateralSource.Branch.Geometry);

                var mapOffset = NetworkHelper.MapChainage(lateralSource.Branch, lateralSource.Chainage);

                var touchLine = line.ExtractLine(mapOffset - 1, mapOffset + 1);
                if (touchLine.IsEmpty)
                {
                    log.ErrorFormat("Unable to connect retention '{0}' at chainage {1} to channel {2}.",
                                    retention.Name, mapOffset, lateralSource.Branch.Name);
                    continue;
                }
                var startCoordinate = touchLine.Coordinates[0];
                var endCoordinate = touchLine.Coordinates[touchLine.Coordinates.Length - 1];

                var angle = Math.Atan2(endCoordinate.Y - startCoordinate.Y, endCoordinate.X - startCoordinate.X);
                var perpendicularAngle = angle - Math.PI / 2;

                var splitLocation = line.ExtractPoint(mapOffset);
                var splitNode = (INode)Activator.CreateInstance(lateralSource.Branch.Source.GetType());
                splitNode.Name = NetworkHelper.GetUniqueName(null, lateralSource.Branch.Network.Nodes, "Node");
                splitNode.Geometry = new Point((Coordinate)splitLocation.Clone());
                splitNode.Network = lateralSource.Network;
                splitNode.Network.Nodes.Add(splitNode);

                splitBranches.Add(new ChannelToSplit((IChannel)lateralSource.Branch, splitNode, mapOffset));

                INode newNode = (INode)Activator.CreateInstance(splitNode.GetType());
                newNode.Name = splitNode.Name + "-retn";
                newNode.Geometry = (IGeometry)splitNode.Geometry.Clone();
                newNode.Geometry.Coordinate.X += 100 * Math.Cos(perpendicularAngle);
                newNode.Geometry.Coordinate.Y += 100 * Math.Sin(perpendicularAngle);
                newNode.Network = lateralSource.Network;
                newNode.Network.Nodes.Add(newNode);

                Channel channel = new Channel(newNode, splitNode, 100);
                channel.Name = splitNode.Name + "-retb";
                channel.IsLengthCustom = true;

                channel.Geometry = new GeometryFactory().CreateLineString(new[]
                                                                              {
                                                                                  newNode.Geometry.Coordinate,
                                                                                  splitNode.Geometry.Coordinate
                                                                              });
                channel.Network = lateralSource.Network;
                channel.Network.Branches.Add(channel);

                lateralSource.Branch.BranchFeatures.Remove(lateralSource);

                retention.Geometry = (IGeometry)newNode.Geometry.Clone();
                NetworkHelper.AddBranchFeatureToBranch(retention, channel, 0.0);

                string structureId = sobekRetentionsReader.RetentionStructures.ContainsKey(retention)
                                         ? sobekRetentionsReader.RetentionStructures[retention]
                                         : "";

                string secondStructureId = sobekRetentionsReader.SecondRetentionStructures.ContainsKey(retention)
                                               ? sobekRetentionsReader.SecondRetentionStructures[retention]
                                               : "";

                CreateStructure(channel, 50.0, definitions, structurebuilders, structureId, secondStructureId);
            }

            SplitBranches(splitBranches);
        }

        private void ImportSobek212Retentions(IEnumerable<Retention> sobekRetsobekRetentions, Dictionary<string, INode> nodes, Dictionary<string, IRetention> retentions)
        {
            ImportSobek212Retentions(sobekRetsobekRetentions, nodes, retentions, null);
        }

        private void ImportSobek212Retentions(IEnumerable<Retention> sobekRetsobekRetentions, Dictionary<string, INode> nodes, Dictionary<string, IRetention> retentions, IDictionary<string, int> readNodeTypes)
        {
            foreach (var retention in sobekRetsobekRetentions)
            {
                if (!nodes.TryGetValue(retention.Name, out var node)||
                    readNodeTypes != null && 
                    readNodeTypes.ContainsKey(retention.Name) && 
                    (SobekNetworkNetterReader.IsConnectionNode(readNodeTypes[retention.Name]) || 
                     SobekNetworkNetterReader.IsLateralSourceNode(readNodeTypes[retention.Name]) || 
                     SobekNetworkNetterReader.IsFlowConnectionNode(readNodeTypes[retention.Name])) ||
                    node is IManhole)
                {
                    //verification if node is declared in netter file as lateral source or connection node
                    continue;   
                }
                
                retention.Geometry = (IGeometry)node.Geometry.Clone();

                if (node.OutgoingBranches.Count > 0)
                {
                    retention.Chainage = 0;
                    AddOrReplaceRetention(node.OutgoingBranches[0], retention, retentions);
                }
                else if (node.IncomingBranches.Count > 0)
                {
                    retention.Chainage = node.IncomingBranches[0].Length;
                    AddOrReplaceRetention(node.IncomingBranches[0], retention, retentions);
                }
            }
        }


        private IList<SobekValveData> GetSobekValveData()
        {
            if (SobekType == DeltaShell.Sobek.Readers.SobekType.SobekRE)
            {
                // SobekRe has no culverts and thus no valves
                return new List<SobekValveData>();
            }

            if (SobekFileNames.SobekValveDataFileName == null)
            {
                return new List<SobekValveData>();
            }

            string valveDataPath = GetFilePath(SobekFileNames.SobekValveDataFileName);
            if (!File.Exists(valveDataPath))
            {
                log.WarnFormat("Valve data file [{0}] not found; skipping...", valveDataPath);
                return new List<SobekValveData>();
            }
            return new DeltaShell.Sobek.Readers.Readers.SobekValveDataReader().Read(valveDataPath).ToList();
        }

        private void CreateStructure(Channel channel, double offset, Dictionary<string, SobekStructureDefinition> definitions, IEnumerable<Builders.IBranchStructureBuilder> structureBuilders, params string[] structureIds)
        {
            ICompositeBranchStructure compositeStructure = null;

            foreach (string structureId in structureIds)
            {
                if (!structureId.Equals("") && definitions.ContainsKey(structureId))
                {
                    SobekStructureDefinition definition = definitions[structureId];
                    IGeometry geometry = GeometryHelper.GetPointGeometry(channel, offset);

                    if (compositeStructure == null)
                    {
                        compositeStructure = CreateCompositeStructureAndAddItToTheBranch(channel, offset, geometry);
                    }

                    var structures = structureBuilders.SelectMany(sb => sb.GetBranchStructures(definition));

                    //How do we know that the structures of this definition belongs to the composite structure (offset / geometry check)?

                    foreach (var structure in structures)
                    {
                        structure.LongName = definition.Name;
                        HydroNetworkHelper.AddStructureToComposite(compositeStructure, structure);
                    }
                }
            }
        }

        private static ICompositeBranchStructure CreateCompositeStructureAndAddItToTheBranch(IChannel channel, double offset, IGeometry geometry)
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


        private void SplitBranches(List<ChannelToSplit> splitBranches)
        {
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            splitBranches.Sort();

            //we need the total split for the naming..
            var totalChannelSplits = splitBranches.Select(sp => sp.Channel).Distinct().ToDictionary(c => c, c =>
                                                                                                    splitBranches.Count(ch => ch.Channel == c));


            //keep a list of how many time a branch was split. Defaults to 0
            var currentChannelSplitCount = splitBranches.Select(cts => cts.Channel).Distinct().ToDictionary(c => c, cts => 0);

            foreach (ChannelToSplit branchToSplit in splitBranches)
            {
                IChannel splittedChannel = branchToSplit.Channel;

                var newChannel = HydroNetworkHelper.SplitChannelAtNode(splittedChannel.Network,
                                                      splittedChannel, branchToSplit.Node);

                currentChannelSplitCount[splittedChannel]++;
                //start at 1 because A is reserved for the original branch
                newChannel.LongName = string.Format("{0}_{1}", splittedChannel.LongName, alpha[1 + totalChannelSplits[splittedChannel] - currentChannelSplitCount[splittedChannel]]);
                newChannel.Name = string.Format("{0}_{1}", splittedChannel.Name, alpha[1 + totalChannelSplits[splittedChannel] - currentChannelSplitCount[splittedChannel]]);

                AddCrossSectionToNewChannelIfOldChannelHasCsAtTheEnd(newChannel, splittedChannel);
            }

            foreach (var channel in splitBranches.Select(bts => bts.Channel).Distinct())
            {
                channel.LongName = channel.LongName + "_A";
                channel.Name = channel.Name + "_A";
            }

        }

        private static void AddCrossSectionToNewChannelIfOldChannelHasCsAtTheEnd(IChannel newChannel, IChannel oldChannel)
        {
            //select a cs at the end
            var sourceCs = oldChannel.CrossSections.FirstOrDefault(c => oldChannel.Length - c.Chainage < 1);
            if (sourceCs == null)
            {
                return;
            }
            var cloneCs = (ICrossSection)sourceCs.Clone();//
            cloneCs.Name = cloneCs.Name + "-retcs";

            NetworkHelper.AddBranchFeatureToBranch(cloneCs, newChannel, 0);
        }

        private void AddOrReplaceRetention(IBranch branch, Retention retention, Dictionary<string, IRetention> retentions)
        {
            if (retentions.ContainsKey(retention.Name))
            {
                var targetRetention = retentions[retention.Name];
                targetRetention.CopyFrom(retention);
                if (targetRetention.Branch != branch)
                {
                    targetRetention.Branch.BranchFeatures.Remove(targetRetention);
                    NetworkHelper.AddBranchFeatureToBranch(targetRetention, branch, targetRetention.Chainage);
                }
                return;
            }
            NetworkHelper.AddBranchFeatureToBranch(retention, branch, retention.Chainage);
        }
    }

    class ChannelToSplit : IComparable
    {
        private IChannel channel = null;
        private INode node = null;
        private double offset = 0;

        public ChannelToSplit(IChannel channel, INode node, double offset)
        {
            this.channel = channel;
            this.node = node;
            this.offset = offset;
        }

        public IChannel Channel
        {
            get { return channel; }
        }

        public INode Node
        {
            get { return node; }
        }

        public double Offset
        {
            get { return offset; }
        }

        public int CompareTo(object obj)
        {
            ChannelToSplit other = (ChannelToSplit)obj;
            if (this.Channel != other.Channel)
            {
                return this.Channel.CompareTo(other.Channel);
            }
            else
            {
                return -this.Offset.CompareTo(other.Offset);
            }
        }

    }

}
