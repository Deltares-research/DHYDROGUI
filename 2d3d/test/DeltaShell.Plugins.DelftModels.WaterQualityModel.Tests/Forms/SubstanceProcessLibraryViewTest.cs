using DelftTools.Controls;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Forms
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class SubstanceProcessLibraryViewTest
    {
        private SubstanceProcessLibrary substanceProcessLibrary;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            substanceProcessLibrary = SubstanceProcessLibraryTestHelper.CreateDemoSubstanceProcessLibrary();
        }

        [Test]
        public void ShowSubstanceProcessLibraryViewEmpty()
        {
            var view = new SubstanceProcessLibraryView {Data = null};

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        public void ShowSubstanceProcessLibraryViewWithData()
        {
            var view = new SubstanceProcessLibraryView {Data = substanceProcessLibrary};

            WindowsFormsTestHelper.ShowModal(view, delegate
            {
                Assert.AreEqual(2, ((ITableView) TypeUtils.GetField(view, "tableViewProcesses")).Columns.Count);
                Assert.AreEqual(4, ((ITableView) TypeUtils.GetField(view, "tableViewParameters")).Columns.Count);
                Assert.AreEqual(4, ((ITableView) TypeUtils.GetField(view, "tableViewActiveSubstances")).Columns.Count);
                Assert.AreEqual(4, ((ITableView) TypeUtils.GetField(view, "tableViewInactiveSubstances")).Columns.Count);
                Assert.AreEqual(4, ((ITableView) TypeUtils.GetField(view, "tableViewOutputParameters")).Columns.Count);
            });
        }

        [Test]
        public void ShowSubstanceProcessLibraryViewWithDataAndNameAndDescriptionColumnsOnly()
        {
            var view = new SubstanceProcessLibraryView
            {
                Data = substanceProcessLibrary,
                ShowNameAndDescriptionColumnsOnly = true
            };

            WindowsFormsTestHelper.ShowModal(view, delegate
            {
                Assert.AreEqual(2, ((ITableView) TypeUtils.GetField(view, "tableViewProcesses")).Columns.Count);
                Assert.AreEqual(2, ((ITableView) TypeUtils.GetField(view, "tableViewParameters")).Columns.Count);
                Assert.AreEqual(2, ((ITableView) TypeUtils.GetField(view, "tableViewActiveSubstances")).Columns.Count);
                Assert.AreEqual(2, ((ITableView) TypeUtils.GetField(view, "tableViewInactiveSubstances")).Columns.Count);
                Assert.AreEqual(2, ((ITableView) TypeUtils.GetField(view, "tableViewOutputParameters")).Columns.Count);
            });
        }
    }
}