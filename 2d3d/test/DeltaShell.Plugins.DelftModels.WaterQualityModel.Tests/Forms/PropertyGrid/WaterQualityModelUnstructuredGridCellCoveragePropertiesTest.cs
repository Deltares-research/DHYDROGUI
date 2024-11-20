using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.PropertyBag.Dynamic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Forms.PropertyGrid
{
    [TestFixture]
    public class WaterQualityModelUnstructuredGridCellCoveragePropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowPropertyGridWithBoundaryDataPropertiesForNodeWithFractionCalculation()
        {
            var mockrepos = new MockRepository();
            var guiMock = mockrepos.Stub<IGui>();

            mockrepos.ReplayAll();

            var unstructuredGridCellCoverage = new UnstructuredGridCellCoverage(new UnstructuredGrid(), false);

            WindowsFormsTestHelper.ShowModal(new DeltaShell.Gui.Forms.PropertyGrid.PropertyGrid(guiMock) {Data = new DynamicPropertyBag(new WaterQualityModelUnstructuredGridCellCoverageProperties {Data = unstructuredGridCellCoverage})});
        }
    }
}