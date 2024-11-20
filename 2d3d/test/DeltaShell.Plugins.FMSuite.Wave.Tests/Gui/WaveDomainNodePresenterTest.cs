using System.Linq;
using DelftTools.Controls.Swf.TreeViewControls;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveDomainNodePresenterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var nodePresenter = new WaveDomainNodePresenter(null);

            // Assert
            Assert.That(nodePresenter, Is.InstanceOf<TreeViewNodePresenterBase<WaveDomainData>>());
        }

        [Test]
        public void GetChildNodeObjects_WithArguments_ReturnsExpectedItems()
        {
            // Setup
            var subDomains = new[]
            {
                new WaveDomainData("Data1"),
                new WaveDomainData("Data2")
            };

            var waveDomainData = new WaveDomainData(string.Empty);
            waveDomainData.SubDomains.AddRange(subDomains);

            using (var model = new WaveModel())
            {
                var nodePresenter = new WaveDomainNodePresenter(wdd => model);

                // Precondition
                CurvilinearGrid waveDomainGrid = waveDomainData.Grid;
                Assert.That(waveDomainGrid, Is.Not.Null);

                CurvilinearCoverage waveDomainBathymetry = waveDomainData.Bathymetry;
                Assert.That(waveDomainBathymetry, Is.Not.Null);

                // Call
                object[] childNodes = nodePresenter.GetChildNodeObjects(waveDomainData, null)
                                                   .Cast<object>()
                                                   .ToArray();

                // Assert
                Assert.That(childNodes.Length, Is.EqualTo(4), "Amount of child nodes should be equal to the grid node + bathymetry node + the total of subdomains present in the parent node.");

                var gridNode = childNodes[0] as WaveModelTreeShortcut;
                Assert.That(gridNode, Is.Not.Null);
                Assert.That(gridNode.Text, Is.EqualTo(waveDomainGrid.Name));
                Assert.That(gridNode.WaveModel, Is.SameAs(model));
                Assert.That(gridNode.Value, Is.SameAs(waveDomainGrid));
                Assert.That(gridNode.ShortCutType, Is.EqualTo(ShortCutType.Grid));

                var bathymetryNode = childNodes[1] as WaveModelTreeShortcut;
                Assert.That(bathymetryNode, Is.Not.Null);
                Assert.That(bathymetryNode.Text, Is.EqualTo(waveDomainBathymetry.Name));
                Assert.That(bathymetryNode.WaveModel, Is.SameAs(model));
                Assert.That(bathymetryNode.Value, Is.SameAs(waveDomainBathymetry));
                Assert.That(bathymetryNode.ShortCutType, Is.EqualTo(ShortCutType.SpatialCoverageWithView));

                Assert.That(childNodes[2], Is.SameAs(subDomains[0]));
                Assert.That(childNodes[3], Is.SameAs(subDomains[1]));
            }
        }
    }
}