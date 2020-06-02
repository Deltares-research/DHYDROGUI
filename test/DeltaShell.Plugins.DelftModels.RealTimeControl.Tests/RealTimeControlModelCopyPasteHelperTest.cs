using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using DelftTools.Controls.Swf.Graph;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlModelCopyPasteHelperTest
    {
        [Test]
        public void IsClipBoardRtcObjectSetTest()
        {
            using (var clipboardStub = new ClipboardStub())
            {
                var shapeCollection = new ShapeBase[]
                {
                    new RuleShape(),
                    new ConditionShape()
                };
                RealTimeControlModelCopyPasteHelper.SetRtcObjectsToClipBoard(shapeCollection);
                Assert.IsTrue(RealTimeControlModelCopyPasteHelper.IsClipBoardRtcObjectSet());
            }
        }

        [Test]
        public void SetAndGetClipBoardRtcObjectsTest()
        {
            using (var clipboardStub = new ClipboardStub())
            {
                var ruleText = "ruleTest";
                var ruleShape = new RuleShape() {Text = ruleText};
                var shapeCollection = new ShapeBase[]
                {
                    ruleShape,
                    new ConditionShape()
                };
                RealTimeControlModelCopyPasteHelper.SetRtcObjectsToClipBoard(shapeCollection);

                IEnumerable<ShapeBase> retrievedObjects = RealTimeControlModelCopyPasteHelper.GetClipBoardRtcObjects();
                Assert.AreEqual(2, retrievedObjects.Count());
                foreach (RuleShape retrievedObject in retrievedObjects.OfType<RuleShape>())
                {
                    Assert.AreEqual(ruleText, retrievedObject.Text);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CopyPasteAllShapesUsingClipboard()
        {
            using (var clipboardMock = new ClipboardMock())
            {
                clipboardMock.GetData_Returns_SetData();

                var input = new Input
                {
                    ParameterName = "parameter",
                    Feature = new RtcTestFeature {Name = "input_feature"}
                };
                var output = new Output
                {
                    ParameterName = "parameter",
                    Feature = new RtcTestFeature {Name = "output_feature"}
                };

                const string ruleName = "ruleTest";
                RuleBase hydRule = new HydraulicRule {Name = ruleName};

                const string conditionName = "ruleTest";
                var condition = new StandardCondition {Name = conditionName};

                const string signalName = "ruleTest";
                var signal = new LookupSignal {Name = signalName};

                var controlGroup = new ControlGroup();
                var controlGroupEditor = new ControlGroupEditor {Data = controlGroup};
                GraphControl graphControl = controlGroupEditor.GraphControl;

                hydRule.Inputs.Add(input);
                hydRule.Outputs.Add(output);
                condition.Input = input;
                condition.TrueOutputs.Add(hydRule);
                signal.Inputs.Add(input);
                signal.RuleBases.Add(hydRule);

                controlGroup.Rules.Add(hydRule);
                controlGroup.Inputs.Add(input);
                controlGroup.Outputs.Add(output);
                controlGroup.Conditions.Add(condition);
                controlGroup.Signals.Add(signal);
                ControlGroupEditorController controller = controlGroupEditor.Controller;
                IEnumerable<ShapeBase> shapecollection = graphControl.GetShapes<ShapeBase>();

                RealTimeControlModelCopyPasteHelper.SetRtcObjectsToClipBoard(shapecollection);
                IEnumerable<ShapeBase> retievedCollection = RealTimeControlModelCopyPasteHelper.GetClipBoardRtcObjects();
                RealTimeControlModelCopyPasteHelper.CloneRtcObjectsFromClipBoardAndPlaceOnGraph(retievedCollection, controller, new Point(12, 15));

                Assert.AreEqual(10, graphControl.GetShapes<ShapeBase>().Count());

                List<RuleBase> retrievedRules = graphControl.GetShapes<RuleShape>().Select(rs => rs.Tag).Cast<RuleBase>().ToList();
                Assert.AreEqual(2, retrievedRules.Count);

                // Original rule inputs and outputs should still be set
                Assert.AreEqual(input.Name, retrievedRules[0].Inputs.First().Name);
                Assert.AreEqual(output.Name, retrievedRules[0].Outputs.First().Name);

                // Inputs and outputs should have been reset
                Assert.AreEqual("[Not Set]", retrievedRules[1].Inputs.First().Name);
                Assert.AreEqual("[Not Set]", retrievedRules[1].Outputs.First().Name);

                List<ConditionBase> copiedConditions = graphControl.GetShapes<ConditionShape>().Select(cs => cs.Tag).Cast<ConditionBase>().ToList();
                Assert.AreEqual(2, copiedConditions.Count);
                Assert.IsNotNull(copiedConditions.Last().Input);
                Assert.AreEqual(1, copiedConditions.Last().TrueOutputs.Count);

                List<SignalBase> copiedSignals = graphControl.GetShapes<SignalShape>().Select(ss => ss.Tag).Cast<SignalBase>().ToList();
                Assert.AreEqual(2, copiedSignals.Count);
                Assert.AreEqual(1, copiedSignals.Last().Inputs.Count);
                Assert.AreEqual(1, copiedSignals.Last().RuleBases.Count);
            }
        }

        [Test]
        public void PastedObjectsShouldHaveUniqueNames()
        {
            var controlGroup = new ControlGroup();
            var controlGroupEditor = new ControlGroupEditor {Data = controlGroup};
            GraphControl graphControl = controlGroupEditor.GraphControl;

            var pidRule1 = new PIDRule() {Name = "regelNaam1"};
            var pidRule2 = new PIDRule() {Name = "regelNaam2"};
            var pidRule3 = new PIDRule() {Name = "regelNaam3"};
            controlGroup.Rules.Add(pidRule1);
            controlGroup.Rules.Add(pidRule2);
            controlGroup.Rules.Add(pidRule3);
            ControlGroupEditorController controller = controlGroupEditor.Controller;
            IEnumerable<ShapeBase> shapeCollection = graphControl.GetShapes<ShapeBase>();
            RealTimeControlModelCopyPasteHelper.CloneRtcObjectsFromClipBoardAndPlaceOnGraph(shapeCollection, controller, new Point(12, 13));
            Assert.AreEqual(6, controller.ControlGroup.Rules.Count);

            var names = new List<string>();
            foreach (RuleBase rule in controller.ControlGroup.Rules)
            {
                if (!names.Contains(rule.Name))
                {
                    names.Add(rule.Name);
                }
                else
                {
                    throw new AmbiguousMatchException("name is not identical!");
                }
            }
        }
    }
}