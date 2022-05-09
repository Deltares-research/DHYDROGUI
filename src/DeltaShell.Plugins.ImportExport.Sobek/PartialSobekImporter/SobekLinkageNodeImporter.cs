using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DeltaShell.Sobek.Readers.Readers;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekLinkageNodeImporter : PartialSobekImporterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekLinkageNodeImporter));

        public SobekLinkageNodeImporter()
        {
            IsVisible = false;
        }

        public override string DisplayName
        {
            get { return "Sobek Linkage Node Importer"; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

        protected override void PartialImport()
        {
            if (!HydroNetwork.Branches.Any())
            {
                Log.Error("Network has no branches; can not import linkage nodes.");
                return;
            }

            Log.DebugFormat("Importing linkage node ...");

            var lstCalcPointsToAddToDiscretization = new List<NetworkLocation>();

            ImportLinkageNode(ref lstCalcPointsToAddToDiscretization);
        }

        /// <summary>
        /// Splits branches on linkage node, add location of the default nodes. The resulting network is a graph
        /// </summary>
        private void ImportLinkageNode(ref List<NetworkLocation> lstCalcPointsToAddToDiscretization)
        {

            var nodes = HydroNetwork.Nodes.ToDictionary(n => n.Name, n => n);
            var channels = HydroNetwork.Channels.ToDictionary(c => c.Name, c => c);
            var highestOrderNumber = -1;
            if (HydroNetwork.Channels.Any())
            {
                highestOrderNumber = HydroNetwork.Channels.Select(c => c.OrderNumber).Max();
            }
            if (highestOrderNumber < 1)
            {
                highestOrderNumber = 1; //starts order number with 1
            }

            var linkageNodes = new SobekNetworkLinkageNodeFileReader().Read(GetFilePath(SobekFileNames.SobekNetworkFileName)).ToList();

            // The DeltaShell network does not support linkage nodes as Sobek does. Linkage nodes will split
            // the branch they connect to.
            for (var i = 0; i < linkageNodes.Count; i++)
            {
                // TODO: don't set ID here 
                if (!channels.ContainsKey(linkageNodes[i].BranchID))
                {
                    Log.ErrorFormat("Unable to process: linkage node {0} links to not existing channel {1}",
                                    linkageNodes[i].ID, linkageNodes[i].BranchID);
                    continue;
                }

                var branchToSplit = channels[linkageNodes[i].BranchID];

                if (branchToSplit.OrderNumber == -1)
                {
                    branchToSplit.OrderNumber = highestOrderNumber++;
                }

                var secondBranch = HydroNetworkHelper.SplitChannelAtNode(HydroNetwork,
                                                                         branchToSplit,
                                                                         nodes[linkageNodes[i].ID]);

                if (secondBranch == null)
                {
                    Log.WarnFormat("Unable to process: linkage node {0} doesn't match with the geometry of channel {1}",
                                   linkageNodes[i].ID, linkageNodes[i].BranchID);
                    continue;
                }

                HydroNetworkHelper.UpdateChannelNames(branchToSplit, secondBranch);

                //list of calculation points to add to the discretization
                lstCalcPointsToAddToDiscretization.Add(new NetworkLocation(branchToSplit, branchToSplit.Length));
                //last point first branch
                lstCalcPointsToAddToDiscretization.Add(new NetworkLocation(secondBranch, 0.0));
                //first point second branch
                lstCalcPointsToAddToDiscretization.Add(new NetworkLocation(secondBranch, secondBranch.Length));
                //last point second branch



                var originalBranchName = linkageNodes[i].BranchID;

                channels[secondBranch.Name] = secondBranch;
                if (channels.ContainsKey(originalBranchName)) channels.Remove(originalBranchName);
                channels[branchToSplit.Name] = branchToSplit;

                // update the relevant references to the original branch with the new branch and renamed branch
                for (int j = (i + 1); j < linkageNodes.Count; j++)
                {
                    if (linkageNodes[j].BranchID == originalBranchName)
                    {
                        if (linkageNodes[j].ReachLocation <= branchToSplit.Length)
                        {
                            linkageNodes[j].BranchID = branchToSplit.Name;
                        }
                        else
                        {
                            linkageNodes[j].ReachLocation -= branchToSplit.Length;
                            linkageNodes[j].BranchID = secondBranch.Name;
                        }
                    }
                }
            }
        }
    }
}
