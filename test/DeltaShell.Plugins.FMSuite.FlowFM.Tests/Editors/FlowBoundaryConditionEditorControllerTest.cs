using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors
{
    [TestFixture]
    public class FlowBoundaryConditionEditorControllerTest
    {
        [Test]
        public void ChangeInSalinityUpdatesCategories()
        {
            var mocks = new MockRepository();
            var editor = mocks.Stub<BoundaryConditionEditor>();

            editor.DepthLayerControlVisible = false;
            editor.Expect(e => e.RefreshAvailableCategories()).Repeat.Once();
            editor.Expect(e => e.ModelDepthLayerDefinition = null).IgnoreArguments().Repeat.Once();
            editor.Expect(e => e.SupportedVerticalProfileTypes = null).IgnoreArguments().Repeat.Once();

            mocks.ReplayAll();

            var model = new WaterFlowFMModel();
            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = false;

            // Controller should call BoundaryConditionEditor.RefreshAvailableCategories on 
            var controller = new FlowBoundaryConditionEditorController
            {
                Model = model,
                Editor = editor
            };

            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = true;

            mocks.VerifyAll();
        }

        [Test]
        public void GetVariablesForProcessForSalinityTest()
        {
            var model = new WaterFlowFMModel();
            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = true;

            var controller = new FlowBoundaryConditionEditorController
            {
                Model = model
            };

            Assert.AreEqual(1, controller.GetVariablesForProcess("Salinity").Count());

            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = false;

            Assert.AreEqual(0, controller.GetVariablesForProcess("Salinity").Count());
        }

        [Test]
        public void SupportedProcessNamesForSalinity()
        {
            var model = new WaterFlowFMModel();
            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = true;

            var controller = new FlowBoundaryConditionEditorController
            {
                Model = model
            };

            Assert.AreEqual(3, controller.SupportedProcessNames.Count());

            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = false;

            Assert.AreEqual(2, controller.SupportedProcessNames.Count());
        }
    }
}