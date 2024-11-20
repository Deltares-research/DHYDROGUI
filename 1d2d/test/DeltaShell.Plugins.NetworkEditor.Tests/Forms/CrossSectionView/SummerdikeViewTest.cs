using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView
{
    [TestFixture]
    public class SummerDikeViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Databinding()
        {
            var hasSummerdike = true;
            var summerdikeCrestLevel = 1.0;
            var summerdikeFloodPlainLevel = 2.0;
            var summerdikeTotalSurface = 4.0;
            var summerdikeFloodSurface = 3.0;


            var summerDike = new SummerDike
                                 {
                                     Active = hasSummerdike,
                                     CrestLevel = summerdikeCrestLevel,
                                     FloodPlainLevel = summerdikeFloodPlainLevel,
                                     TotalSurface = summerdikeTotalSurface,
                                     FloodSurface = summerdikeFloodSurface
                                 };
            

            var crossSectionViewSummerdike = new SummerDikeView {Data = summerDike};

            int called = 0;
            
            WindowsFormsTestHelper.ShowModal(crossSectionViewSummerdike,
                delegate
                    {
                        if (called > 0)
                            return;

                        Assert.AreEqual(hasSummerdike, crossSectionViewSummerdike.cbxHasSummerdike.Checked);
                        Assert.AreEqual(summerdikeCrestLevel,Convert.ToDouble(crossSectionViewSummerdike.txtCrestLevel.Text));
                        Assert.AreEqual(summerdikeFloodPlainLevel,Convert.ToDouble(crossSectionViewSummerdike.txtFloodplainLevel.Text));
                        Assert.AreEqual(summerdikeTotalSurface,Convert.ToDouble(crossSectionViewSummerdike.txtTotalSurface.Text));
                        Assert.AreEqual(summerdikeFloodSurface,Convert.ToDouble(crossSectionViewSummerdike.txtFloodSurface.Text));

                        called++;
                    });

            Assert.GreaterOrEqual(called, 1);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DatabindingNoDataNoError()
        {
            var crossSectionViewSummerdike = new SummerDikeView();

            WindowsFormsTestHelper.Show(crossSectionViewSummerdike);
            WindowsFormsTestHelper.CloseAll();
        }
    }
}
