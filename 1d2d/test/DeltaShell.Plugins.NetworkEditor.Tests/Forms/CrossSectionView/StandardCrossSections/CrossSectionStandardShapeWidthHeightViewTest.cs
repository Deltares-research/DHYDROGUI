using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView.StandardCrossSections
{
    [TestFixture]
    public class CrossSectionStandardShapeWidthHeightViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithEgg()
        {
            var data = new CrossSectionStandardShapeEgg {Width = 2};

            var view = new CrossSectionStandardShapeWidthHeightView()
                           {
                               Data = data
                           };

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithCunette()
        {
            var data = CrossSectionStandardShapeCunette.CreateDefault();

            var view = new CrossSectionStandardShapeWidthHeightView
            {
                Data = data
            };

            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}