using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.ProjectExplorer;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class HydroAreaProjectTreeViewNodePresenterTest
    {
        [Test]
        public void Test_HydroAreaProjectTreeViewNodePresenter_GetChildNodeObjects_Returns_BridgePillars()
        {
            var hydroArea = new HydroAreaProjectTreeViewNodePresenter();

            var resultingHydroArea= new HydroArea();
            var parentNodeData = hydroArea.GetChildNodeObjects(resultingHydroArea, null);

            Assert.IsNotNull(resultingHydroArea);
            CollectionAssert.Contains(parentNodeData, resultingHydroArea.BridgePillars);
        }
    }
}
