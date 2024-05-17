using System.Collections;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Forms.ProjectExplorer
{
    [TestFixture]
    public class WaterQualityModelNodePresenterTest
    {
        [Test]
        public void GetInputChildNodeObjectsTest()
        {
            // setup
            var mocks = new MockRepository();
            var guiStub = mocks.Stub<GuiPlugin>();

            var model = new WaterQualityModel();

            var modelNodePresenter = new WaterQualityModelNodePresenter(guiStub);

            IEnumerator childnodes = modelNodePresenter.GetChildNodeObjects(model, null).GetEnumerator();
            Assert.IsTrue(childnodes.MoveNext(), "Expect at least 1 item to exist.");
            var inputFolder = (TreeFolder) childnodes.Current;
            Assert.AreEqual("Input", inputFolder.Text);

            // call
            IEnumerator inputFolderChildren = inputFolder.ChildItems.GetEnumerator();

            // assert
            AssertNextNodeIsModelDataItem(inputFolderChildren, model, WaterQualityModel.InputFileCommandLineDataItemMetaData.Tag);
            AssertNextNodeIsModelDataItem(inputFolderChildren, model, WaterQualityModel.SubstanceProcessLibraryDataItemMetaData.Tag);
            AssertNextNodeIsModelDataItem(inputFolderChildren, model, WaterQualityModel.GridDataItemMetaData.Tag);
            AssertNextNodeIsModelDataItem(inputFolderChildren, model, WaterQualityModel.BathymetryDataItemMetaData.Tag);
            AssertNextNodeIsModelDataItemInWrapper(inputFolderChildren, model, WaterQualityModel.InitialConditionsDataItemMetaData.Tag);
            AssertNextNodeIsModelDataItemInWrapper(inputFolderChildren, model, WaterQualityModel.ProcessCoefficientsDataItemMetaData.Tag);
            AssertNextNodeIsModelDataItemInWrapper(inputFolderChildren, model, WaterQualityModel.DispersionDataItemMetaData.Tag);
            AssertNextNodeIsDataItemWithModelData(inputFolderChildren, model.Boundaries);
            AssertNextNodeIsDataItemWithModelData(inputFolderChildren, model.Loads);
            AssertNextNodeIsDataItemWithModelData(inputFolderChildren, model.ObservationPoints);
            AssertNextNodeIsModelDataItem(inputFolderChildren, model, WaterQualityModel.ObservationAreasDataItemMetaData.Tag);
            AssertNextNodeIsModelDataItem(inputFolderChildren, model, WaterQualityModel.BoundaryDataDataItemMetaData.Tag);
            AssertNextNodeIsModelDataItem(inputFolderChildren, model, WaterQualityModel.LoadsDataDataItemMetaData.Tag);
        }

        private void AssertNextNodeIsModelDataItemInWrapper(IEnumerator inputFolderChildren, WaterQualityModel model, string dataItemTag)
        {
            Assert.IsTrue(inputFolderChildren.MoveNext(), "Expected a child node.");
            IDataItem expectedDataItemMatch = model.GetDataItemByTag(dataItemTag);
            var currentDataItem = (IDataItem) inputFolderChildren.Current;
            Assert.AreEqual(expectedDataItemMatch.Name, currentDataItem.Name);
            Assert.AreEqual(typeof(WaterQualityFunctionDataWrapper), currentDataItem.ValueType);
            Assert.AreEqual(expectedDataItemMatch.Role, currentDataItem.Role);
            Assert.AreEqual(expectedDataItemMatch.Tag, currentDataItem.Tag);
            Assert.AreSame(model, currentDataItem.Owner);
        }

        private static void AssertNextNodeIsDataItemWithModelData(IEnumerator inputFolderChildren, object modelData)
        {
            Assert.IsTrue(inputFolderChildren.MoveNext(), "Expected a child node.");
            Assert.IsInstanceOf<IDataItem>(inputFolderChildren.Current);
            Assert.AreSame(modelData, ((IDataItem) inputFolderChildren.Current).Value);
        }

        private static void AssertNextNodeIsModelDataItem(IEnumerator inputFolderChildren, WaterQualityModel model, string dataItemTag)
        {
            Assert.IsTrue(inputFolderChildren.MoveNext(), "Expected a child node.");
            Assert.AreSame(model.GetDataItemByTag(dataItemTag), inputFolderChildren.Current);
        }
    }
}