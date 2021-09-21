using System.Threading;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Forms
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class RainfallRunoffBoundaryDataViewTest
    {
        [Test]
        public void ShowEmpty()
        {
            var view = new RRBoundarySeriesView {Data = null};
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        public void ShowWithData()
        {
            var view = new RRBoundarySeriesView
                           {
                               Data = new RainfallRunoffBoundaryData()
                           };
            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}