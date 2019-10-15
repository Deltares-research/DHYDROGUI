using System.Linq;
using System.Windows.Forms.VisualStyles;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DelftTools.Utils.UndoRedo;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Roughness
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.UndoRedo)]
    public class RoughnessNetworkCoverageUndoRedoIntegrationTest
    {
        [Test]
        public void UndoRedoAddLocationShouldUpdateSegments()
        {
            // Define network:
            var network = new HydroNetwork();

            var fromNode = new Node();
            var toNode = new Node();
            network.Nodes.Add(fromNode);
            network.Nodes.Add(toNode);

            var branch = new Branch {Source = fromNode, Target = toNode, Length = 1.0};
            network.Branches.Add(branch);
            network.CrossSectionSectionTypes.Add(new CrossSectionSectionType { Name = "main" });

            var roughnessCoverage = new RoughnessNetworkCoverage("", false, null) { Network = network };
            using (var undoRedoManager = new UndoRedoManager(roughnessCoverage))
            {
                undoRedoManager.TrackChanges = true;

                // Add new network location:
                roughnessCoverage[new NetworkLocation(branch, 10)] = new object[] { 22.0, RoughnessType.StricklerKn };

                Assert.AreEqual(1, roughnessCoverage.Segments.Values.Count, "There should be 1 segment after adding a network location.");
                Assert.AreEqual(1, undoRedoManager.UndoStack.Count());

                undoRedoManager.Undo();

                Assert.AreEqual(0, roughnessCoverage.Locations.Values.Count, "No more network locations if the single add is undone.");
                Assert.AreEqual(0, roughnessCoverage.Segments.Values.Count, "No network locations -> No segments!");
                Assert.AreEqual(1, undoRedoManager.RedoStack.Count());

                undoRedoManager.Redo();

                Assert.AreEqual(1, roughnessCoverage.Locations.Values.Count, "Should now be back at original situation before undo.");
                Assert.AreEqual(1, roughnessCoverage.Segments.Values.Count, "Should now be back at original situation before undo.");
            }
        }
    }
}