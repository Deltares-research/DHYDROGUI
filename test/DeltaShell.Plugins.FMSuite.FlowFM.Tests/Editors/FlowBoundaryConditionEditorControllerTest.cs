using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
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
            var _ = new FlowBoundaryConditionEditorController
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

            var controller = new FlowBoundaryConditionEditorController {Model = model};

            Assert.AreEqual(1, controller.GetVariablesForProcess("Salinity").Count());

            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = false;

            Assert.AreEqual(0, controller.GetVariablesForProcess("Salinity").Count());
        }

        [Test]
        public void SupportedProcessNamesForSalinity()
        {
            var model = new WaterFlowFMModel();
            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = true;

            var controller = new FlowBoundaryConditionEditorController {Model = model};

            Assert.AreEqual(3, controller.SupportedProcessNames.Count());

            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = false;

            Assert.AreEqual(2, controller.SupportedProcessNames.Count());
        }

        [Test]
        public void ChangeInSedimentUpdatesCategories()
        {
            var mocks = new MockRepository();
            var editor = mocks.Stub<BoundaryConditionEditor>();

            editor.DepthLayerControlVisible = false;
            editor.Expect(e => e.RefreshAvailableCategories()).Repeat.Once();
            editor.Expect(e => e.ModelDepthLayerDefinition = null).IgnoreArguments().Repeat.Once();
            editor.Expect(e => e.SupportedVerticalProfileTypes = null).IgnoreArguments().Repeat.Once();

            mocks.ReplayAll();

            var model = new WaterFlowFMModel();
            model.ModelDefinition.UseMorphologySediment = false;

            // Controller should call BoundaryConditionEditor.RefreshAvailableCategories on 
            var _ = new FlowBoundaryConditionEditorController
            {
                Model = model,
                Editor = editor
            };

            model.ModelDefinition.UseMorphologySediment = true;

            mocks.VerifyAll();
        }

        [Test]
        public void SupportedProcessNamesForSediment()
        {
            var model = new WaterFlowFMModel();
            model.ModelDefinition.UseMorphologySediment = true;

            var controller = new FlowBoundaryConditionEditorController {Model = model};

            /**
             * Default: Flow, Tracer
             * MorSed: Sediment, SedimentConcentration
             * */

            Assert.AreEqual(4, controller.SupportedProcessNames.Count());

            model.ModelDefinition.UseMorphologySediment = false;

            Assert.AreEqual(2, controller.SupportedProcessNames.Count());
        }

        [Test]
        public void TestGetAllowedVariablesFor_SedimentFractions()
        {
            // setup
            var model = new WaterFlowFMModel();
            model.ModelDefinition.UseMorphologySediment = true;

            var controller = new FlowBoundaryConditionEditorController {Model = model};

            var sandFraction = new SedimentFraction() {Name = "sandFraction"};
            sandFraction.CurrentSedimentType = sandFraction.AvailableSedimentTypes.FirstOrDefault(st => st.Name == "Sand");
            Assert.NotNull(sandFraction.CurrentSedimentType);

            var mudFraction = new SedimentFraction() {Name = "mudFraction"};
            mudFraction.CurrentSedimentType = mudFraction.AvailableSedimentTypes.FirstOrDefault(st => st.Name == "Mud");
            Assert.NotNull(mudFraction.CurrentSedimentType);

            var bedloadFraction = new SedimentFraction() {Name = "bedloadFraction"};
            bedloadFraction.CurrentSedimentType = bedloadFraction.AvailableSedimentTypes.FirstOrDefault(st => st.Name == "Bed-load");
            Assert.NotNull(bedloadFraction.CurrentSedimentType);

            model.SedimentFractions.AddRange(new List<ISedimentFraction>
            {
                sandFraction,
                mudFraction,
                bedloadFraction
            });

            // call GetAllowedVariables
            List<string> allowedVariables = controller.GetAllowedVariablesFor("Sediment concentration", new BoundaryConditionSet()).ToList();

            // Assert bed-load fractions are not allowed
            Assert.IsTrue(allowedVariables.Contains(sandFraction.Name));
            Assert.IsTrue(allowedVariables.Contains(mudFraction.Name));
            Assert.IsFalse(allowedVariables.Contains(bedloadFraction.Name));
        }

        [TestCase("WaterLevel", FlowBoundaryQuantityType.WaterLevel, "Water level")]
        [TestCase("Riemann", FlowBoundaryQuantityType.Riemann, "Riemann invariant")]
        [TestCase("Tracer1", FlowBoundaryQuantityType.Tracer, "Tracer1")]
        [TestCase("WaterLevel", FlowBoundaryQuantityType.Tracer, "WaterLevel")]
        [TestCase("Riemann", FlowBoundaryQuantityType.Tracer, "Riemann")]
        [TestCase("Fraction1", FlowBoundaryQuantityType.SedimentConcentration, "Fraction1")]
        [TestCase("WaterLevel", FlowBoundaryQuantityType.SedimentConcentration, "WaterLevel")]
        [TestCase("3", FlowBoundaryQuantityType.SedimentConcentration, "3")]
        [TestCase("999", FlowBoundaryQuantityType.SedimentConcentration, "999")]
        public void TestGetVariableDescription_CorrectlyHandlesTracersAndFractionNames(string variable, FlowBoundaryQuantityType quantityType, string expectedDescription)
        {
            string category = quantityType.GetDescription();
            string returnedDescription = new FlowBoundaryConditionEditorController().GetVariableDescription(variable, category);

            Assert.AreEqual(expectedDescription, returnedDescription);
        }
    }
}