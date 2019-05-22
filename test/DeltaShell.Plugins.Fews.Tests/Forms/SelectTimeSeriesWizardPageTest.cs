using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.Fews.Forms;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.Fews.Tests.Forms
{
    [TestFixture]
    public class SelectTimeSeriesWizardPageTest
    {
        private static readonly MockRepository mocks = new MockRepository();
        private PiTimeSeriesImporter importerMock;

        [SetUp]
        public void SetUp()
        {
            importerMock = mocks.DynamicMock<PiTimeSeriesImporter>();
            var series = new[] { new TimeSeries { Name = "TimeSeries 1" }, new TimeSeries { Name = "TimeSeries 2" } };
            importerMock.Expect(import => import.TimeSeries).Return(new List<TimeSeries>(series)).Repeat.Any();

            mocks.ReplayAll();
        }

        [Test]
        public void PageMultiSelectFalse()
        {
            var page = new SelectTimeSeriesWizardPage();
            page.MultiSelect = false;

            page.PiTimeSeriesImporter = importerMock;
            page.InitPage();

            var listBox = page.Controls.Find("lbTimeSeries",true).FirstOrDefault() as ListBox;
            Assert.IsNotNull(listBox);
            Assert.AreEqual(2,listBox.Items.Count);
            Assert.AreEqual(SelectionMode.One,listBox.SelectionMode);
        }

        [Test]
        public void PageMultiSelectTrue()
        {
            var page = new SelectTimeSeriesWizardPage();
            page.MultiSelect = true;
            page.PiTimeSeriesImporter = importerMock;
            page.InitPage();

            var listBox = page.Controls.Find("lbTimeSeries", true).FirstOrDefault() as ListBox;
            Assert.IsNotNull(listBox);
            Assert.AreEqual(2, listBox.Items.Count);
            Assert.AreEqual(SelectionMode.MultiSimple, listBox.SelectionMode);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void PageGetSelectedTimeSeries()
        {
            var page = new SelectTimeSeriesWizardPage();
            page.MultiSelect = false;
            page.PiTimeSeriesImporter = importerMock;
            page.InitPage();

            var listBox = page.Controls.Find("lbTimeSeries", true).FirstOrDefault() as ListBox;
            Assert.IsNotNull(listBox);
            listBox.SelectedIndex = 1;
            Assert.AreEqual(1,page.GetSelectedTimeSeries.Count());
            Assert.AreEqual("TimeSeries 2", page.GetSelectedTimeSeries.FirstOrDefault().Name);

            page.MultiSelect = true;
            Assert.AreEqual(2, page.GetSelectedTimeSeries.Count());
        }

/*
 *      Disabled this test because it does not close by itself
 *      
        [Test]
        [Category(TestCategory.WindowsForms)]
        [STAThreadAttribute]
        public void WizardTest()
        {
            var wizard = new ImportPiTimeSeriesDialog();
            wizard.Data = new PiTimeSeriesImporter();
            WindowsFormsTestHelper.ShowModal(wizard);
        }
*/
    }
}
