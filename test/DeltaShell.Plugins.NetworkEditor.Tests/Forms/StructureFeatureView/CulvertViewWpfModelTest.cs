using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{
    [TestFixture]
    public class CulvertViewWpfModelTest
    {
        [TestCase(CulvertGeometryType.SteelCunette, true)]
        [TestCase(CulvertGeometryType.Cunette, false)]
        [TestCase(CulvertGeometryType.Arch, true)]
        [TestCase(CulvertGeometryType.Ellipse, true)]
        [TestCase(CulvertGeometryType.Rectangle, true)]
        [TestCase(CulvertGeometryType.Egg, false)]
        [TestCase(CulvertGeometryType.Round, false)]
        [TestCase(CulvertGeometryType.Tabulated, false)]
        public void GivenCulvertViewModel_WhenChangingGeometryType_ThenGeometryHeightIsAsExpected
            (CulvertGeometryType geometryType, bool isEnabled)
        {
            // Given - When
            var viewModel = new CulvertViewWpfViewModel
            {
                Culvert = new Culvert(),
                SelectedCulvertGeometryType = geometryType
            };

            // Then
            Assert.That(viewModel.GeometryHeightEnabled, Is.EqualTo(isEnabled));
        }

        [TestCase(CulvertGeometryType.SteelCunette, true)]
        [TestCase(CulvertGeometryType.Cunette, true)]
        [TestCase(CulvertGeometryType.Arch, true)]
        [TestCase(CulvertGeometryType.Ellipse, true)]
        [TestCase(CulvertGeometryType.Rectangle, true)]
        [TestCase(CulvertGeometryType.Egg, true)]
        [TestCase(CulvertGeometryType.Round, false)]
        [TestCase(CulvertGeometryType.Tabulated, false)]
        public void GivenCulvertViewModel_WhenChangingGeometryType_ThenGeometryHeightVisibilityIsAsExpected
            (CulvertGeometryType geometryType, bool isEnabled)
        {
            // Given - When
            var viewModel = new CulvertViewWpfViewModel
            {
                Culvert = new Culvert(),
                SelectedCulvertGeometryType = geometryType
            };

            // Then
            Assert.That(viewModel.GeometryHeightVisibility, Is.EqualTo(isEnabled));
        }

        [TestCase(CulvertGeometryType.SteelCunette, false)]
        [TestCase(CulvertGeometryType.Cunette, true)]
        [TestCase(CulvertGeometryType.Arch, true)]
        [TestCase(CulvertGeometryType.Ellipse, true)]
        [TestCase(CulvertGeometryType.Rectangle, true)]
        [TestCase(CulvertGeometryType.Egg, true)]
        [TestCase(CulvertGeometryType.Round, false)]
        [TestCase(CulvertGeometryType.Tabulated, false)]
        public void GivenCulvertViewModel_WhenChangingGeometryType_ThenGeometryWidthVisibilityIsAsExpected
            (CulvertGeometryType geometryType, bool isEnabled)
        {
            // Given - When
            var viewModel = new CulvertViewWpfViewModel
            {
                Culvert = new Culvert(),
                SelectedCulvertGeometryType = geometryType
            };

            // Then
            Assert.That(viewModel.GeometryWidthVisibility, Is.EqualTo(isEnabled));
        }
    }
}