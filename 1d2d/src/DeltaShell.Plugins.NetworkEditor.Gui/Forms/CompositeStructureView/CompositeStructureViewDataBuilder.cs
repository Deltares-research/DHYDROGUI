using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView
{
    /// <summary>
    /// Class responsible for create view data for sideView and structure view from a branch structure
    /// </summary>
    public class CompositeStructureViewDataBuilder
    {
        private static int SideViewDataCounter = 0;

        private static double GetMaxStructureLength(ICompositeBranchStructure compositeBranchStructure)
        {
            var maxStructureLength = 0.0;

            foreach (var structure in compositeBranchStructure.Structures)
            {
                if (maxStructureLength < structure.Length)
                {
                    maxStructureLength = structure.Length;
                }
            }

            if (maxStructureLength == 0)
            {
                maxStructureLength = 1;
            }
            return maxStructureLength;
        }

        public static CompositeStructureViewDataController GetCompositeStructureViewDataForStructure(ICompositeBranchStructure compositeBranchStructure)
        {
            ICrossSection crossSectionBefore;
            ICrossSection crossSectionAfter;

            NetworkHelper.GetNeighboursOnBranch(compositeBranchStructure.Branch, compositeBranchStructure.Chainage,
                                                               out crossSectionBefore, out crossSectionAfter);

            // set NetworkSideViewData
            double maxStructureLength = GetMaxStructureLength(compositeBranchStructure);

            var startPosition = (compositeBranchStructure.Chainage - maxStructureLength < 0.0)
                                ? 0.0
                                : compositeBranchStructure.Chainage - maxStructureLength;

            var endPosition = (compositeBranchStructure.Chainage + maxStructureLength > compositeBranchStructure.Branch.Length)
                                ? compositeBranchStructure.Branch.Length
                                : compositeBranchStructure.Chainage + maxStructureLength;

            if (crossSectionBefore != null)
            {
                startPosition = Math.Max(0.0, crossSectionBefore.Chainage - 3);
            }
            if (crossSectionAfter != null)
            {
                endPosition = Math.Min(compositeBranchStructure.Branch.Length, crossSectionAfter.Chainage + 3);
            }

            var route = RouteHelper.CreateRoute(new NetworkLocation(compositeBranchStructure.Branch, startPosition),
                                                new NetworkLocation(compositeBranchStructure.Branch, endPosition));
            if (!route.Segments.Values.Any() && compositeBranchStructure.Branch is ISewerConnection)
                route.Segments.AddValues(new[]
                {
                    new NetworkSegment()
                    {
                        Branch = compositeBranchStructure.Branch, Length = endPosition - startPosition,
                        Chainage = startPosition
                    }
                });
            SideViewDataCounter++;
            var networkSideViewData = new CompositeStructureViewDataController(compositeBranchStructure, route, null)
                                          {
                                              Name = SideViewDataCounter.ToString(),
                                              ActiveCompositeStructure = compositeBranchStructure,
                                              CrossSectionBefore = crossSectionBefore,
                                              CrossSectionAfter = crossSectionAfter
                                          };
            return networkSideViewData;
        }
    }
}