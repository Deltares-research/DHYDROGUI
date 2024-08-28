using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Graph;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using GeoAPI.Extensions.Feature;
using Netron.GraphLib;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;
using Clipboard = DelftTools.Controls.Clipboard;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ControlGroupEditorTest
    {
        private MockRepository mocks;
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";

        private RuleBase rule;
        private ConditionBase condition;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository(); // create on every test to prevent test interference

            rule = new PIDRule
            {
                Name = "test",
                Kp = 0,
                Ki = 0,
                Kd = 0,
                Setting = new Setting
                {
                    Min = 0,
                    Max = 0,
                    MaxSpeed = 0
                }
            };
            var input = new Input
            {
                ParameterName = "inputTestX",
                Feature = new RtcTestFeature {Name = "Element"}
            };
            rule.Inputs.Add(input);

            condition = new StandardCondition();
            var output = new Output
            {
                ParameterName = "outputTestY",
                Feature = new RtcTestFeature {Name = "Element"}
            };
            rule.Outputs.Add(output);

            condition = new StandardCondition
            {
                Name = "test",
                Reference = "testImplicit",
                Input = new Input
                {
                    ParameterName = "testInput",
                    Feature = new RtcTestFeature {Name = "Element"}
                }
            };
        }

        [Test]
        [TestCase(true, -1)] // RTC Inputs less than limit
        [TestCase(true, 0)]  // RTC Inputs equal to limit
        [TestCase(false, 1)] // RTC Inputs greater than limit
        public void TestRtcInputsGenerateExpectedContextMenu(bool expectedResult, int additionalValues = 0)
        {
            using (var clipboardStub = new ClipboardStub())
            {
                var rtcModel = mocks.DynamicMock<IRealTimeControlModel>();

                var controlGroupEditor = new ControlGroupEditor
                {
                    Data = new ControlGroup(),
                    Model = rtcModel
                };

                var maxValues = (int) TypeUtils.GetField(controlGroupEditor, "MaxLocationsToDisplayIndividually");

                rtcModel.Expect(m => m.GetChildDataItemLocationsFromControlledModels(DataItemRole.Output))
                        .Return(Enumerable.Range(0, maxValues + additionalValues).Select(i => new Feature()))
                        .Repeat.Once();

                rtcModel.Expect(m => m.GetChildDataItemsFromControlledModelsForLocation(null)).IgnoreArguments()
                        .Return(new List<DataItem> {new DataItem() {Role = DataItemRole.Output}})
                        .Repeat.Any();

                mocks.ReplayAll();

                var shape = new InputItemShape()
                {
                    Tag = new Input(),
                    IsSelected = true
                };
                controlGroupEditor.GraphControl.NetronGraph.Shapes.Add(shape);

                TypeUtils.CallPrivateMethod(controlGroupEditor, "OnGraphControlContextMenu", new object[]
                {
                    null,
                    null
                });

                var menuItems = new MenuItem[controlGroupEditor.GraphControl.ContextMenuItems.Count];
                controlGroupEditor.GraphControl.ContextMenuItems.CopyTo(menuItems, 0);

                List<string> menuItemNames = menuItems.Select(b => b.Text).ToList();

                Assert.AreEqual(expectedResult, menuItemNames.Contains("Input locations"),
                                "Context menu differs from what was expected");

                Assert.IsTrue(menuItemNames.Contains("Choose input locations..."),
                              "Users should always have the option to 'choose input location...' for RTC Outputs");

                mocks.VerifyAll();
            }
        }

        [Test]
        [TestCase(true, -1)] // RTC Outputs less than limit
        [TestCase(true, 0)]  // RTC Outputs equal to limit
        [TestCase(false, 1)] // RTC Outputs greater than limit
        public void TestRtcOutputsGenerateExpectedContextMenu(bool expectedResult, int additionalValues = 0)
        {
            using (new ClipboardStub())
            {
                var rtcModel = mocks.DynamicMock<IRealTimeControlModel>();

                var controlGroupEditor = new ControlGroupEditor
                {
                    Data = new ControlGroup(),
                    Model = rtcModel
                };

                var maxValues = (int) TypeUtils.GetField(controlGroupEditor, "MaxLocationsToDisplayIndividually");

                rtcModel.Expect(m => m.GetChildDataItemLocationsFromControlledModels(DataItemRole.Input))
                        .Return(Enumerable.Range(0, maxValues + additionalValues).Select(i => new Feature()))
                        .Repeat.Once();

                rtcModel.Expect(m => m.GetChildDataItemsFromControlledModelsForLocation(null)).IgnoreArguments()
                        .Return(new List<DataItem> {new DataItem() {Role = DataItemRole.Input}})
                        .Repeat.Any();

                mocks.ReplayAll();

                var shape = new OutputItemShape()
                {
                    Tag = new Output(),
                    IsSelected = true
                };
                controlGroupEditor.GraphControl.NetronGraph.Shapes.Add(shape);

                TypeUtils.CallPrivateMethod(controlGroupEditor, "OnGraphControlContextMenu", new object[]
                {
                    null,
                    null
                });

                var menuItems = new MenuItem[controlGroupEditor.GraphControl.ContextMenuItems.Count];
                controlGroupEditor.GraphControl.ContextMenuItems.CopyTo(menuItems, 0);

                List<string> menuItemNames = menuItems.Select(b => b.Text).ToList();

                Assert.AreEqual(expectedResult, menuItemNames.Contains("Output locations"),
                                "Context menu differs from what was expected");

                Assert.IsTrue(menuItemNames.Contains("Choose output locations..."),
                              "Users should always have the option to 'choose output location...' for RTC Outputs");

                mocks.VerifyAll();
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void ShowControlGroupGraphView()
        {
            var controlGroupEditor = new ControlGroupEditor {Data = new ControlGroup()};
            WindowsFormsTestHelper.ShowModal(controlGroupEditor);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void EditorForGroup2Rules()
        {
            ControlGroup controlGroup = RealTimeControlTestHelper.CreateGroup2Rules();
            controlGroup.Conditions[0].LongName = "LongName";
            controlGroup.Conditions[0].Value = 7.53;

            var controlGroupEditor = new ControlGroupEditor {Data = controlGroup};

            WindowsFormsTestHelper.ShowModal(controlGroupEditor);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void DomainObject2ShapeTest()
        {
            using (var controlGroupEditor = new ControlGroupEditor {Data = RealTimeControlTestHelper.CreateGroup2Rules()})
            {
                //controlGroupEditor.BindObjectDataToGraph();
                GraphControl graphControl = controlGroupEditor.GraphControl;

                List<ShapeBase> shapes = graphControl.GetShapes<ShapeBase>().ToList();

                // 3 input shape, 1 condition shape, 2 rule shapes, 2 output shapes
                Assert.AreEqual(8, shapes.Count);
                Assert.AreEqual(7, graphControl.NetronGraph.Connections.Count);

                Assert.AreEqual(3, shapes.Count(s => s.Tag is Input));
                Assert.AreEqual(1, shapes.Count(s => s.Tag is ConditionBase));
                Assert.AreEqual(2, shapes.Count(s => s.Tag is RuleBase));
                Assert.AreEqual(2, shapes.Count(s => s.Tag is Output));
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void DomainObject2ShapeConnectionTest()
        {
            using (var controlGroupEditor = new ControlGroupEditor {Data = RealTimeControlTestHelper.CreateGroup2Rules()})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                // 3 input shape, 1 condition shape, 2 rule shapes, 2 output shapes

                IList<ShapeBase> shapes = new List<ShapeBase>(graphControl.GetShapes<ShapeBase>());
                // inputs connect a Bottom to a Top connector
                IEnumerable<ShapeBase> inputs = shapes.Where(s => s.Tag is Input);
                Assert.AreEqual(3, inputs.Count());
                inputs.ForEach(
                    i =>
                        Assert.IsTrue(
                            i.Connectors.Count == 1 && i.Connectors[0].Name == "Bottom"
                                                    && i.Connectors[0].Connections[0].To.Name == "Top"));

                // rules connect right to output
                IEnumerable<ShapeBase> rules = shapes.Where(s => s.Tag is RuleBase);
                Assert.AreEqual(2, rules.Count());
                ShapeBase trueRuleShape = rules.FirstOrDefault();
                Assert.AreEqual(4, trueRuleShape.Connectors.Count);
                // left : condition
                Assert.AreEqual(1, trueRuleShape.Connectors[0].Connections.Count);
                Assert.AreEqual("Left", trueRuleShape.Connectors[0].Name);
                Assert.AreEqual(trueRuleShape.Connectors[0], trueRuleShape.Connectors[0].Connections[0].To);
                Assert.AreEqual("Left", trueRuleShape.Connectors[0].Connections[0].To.Name);
                Assert.AreEqual("Right", trueRuleShape.Connectors[0].Connections[0].From.Name);
                // top : input
                Assert.AreEqual(1, trueRuleShape.Connectors[1].Connections.Count);
                Assert.AreEqual("Top", trueRuleShape.Connectors[1].Name);
                Assert.AreEqual(trueRuleShape.Connectors[1], trueRuleShape.Connectors[1].Connections[0].To);
                Assert.AreEqual("Top", trueRuleShape.Connectors[1].Connections[0].To.Name);
                Assert.AreEqual("Bottom", trueRuleShape.Connectors[1].Connections[0].From.Name);
                // right : output
                Assert.AreEqual("Right", trueRuleShape.Connectors[2].Name);
                Assert.AreEqual(trueRuleShape.Connectors[2], trueRuleShape.Connectors[2].Connections[0].From);
                Assert.AreEqual("Left", trueRuleShape.Connectors[2].Connections[0].To.Name);
                Assert.AreEqual("Right", trueRuleShape.Connectors[2].Connections[0].From.Name);

                ShapeBase falseRuleShape = rules.LastOrDefault();
                // left : condition
                Assert.AreEqual(1, falseRuleShape.Connectors[0].Connections.Count);
                Assert.AreEqual("Left", falseRuleShape.Connectors[0].Name);
                Assert.AreEqual(falseRuleShape.Connectors[0], falseRuleShape.Connectors[0].Connections[0].To);
                Assert.AreEqual("Left", falseRuleShape.Connectors[0].Connections[0].To.Name);
                Assert.AreEqual("Bottom", falseRuleShape.Connectors[0].Connections[0].From.Name);

                IEnumerable<ShapeBase> conditions = shapes.Where(s => s.Tag is ConditionBase);
                ShapeBase conditionShape = conditions.FirstOrDefault();
                Assert.AreEqual(4, conditionShape.Connectors.Count);
                // left : empty
                Assert.AreEqual(0, conditionShape.Connectors[0].Connections.Count);
                Assert.AreEqual("Left", conditionShape.Connectors[0].Name);
                // top : input
                Assert.AreEqual(1, conditionShape.Connectors[1].Connections.Count);
                Assert.AreEqual("Top", conditionShape.Connectors[1].Name);
                Assert.AreEqual(conditionShape.Connectors[1], conditionShape.Connectors[1].Connections[0].To);
                Assert.AreEqual("Top", conditionShape.Connectors[1].Connections[0].To.Name);
                Assert.AreEqual("Bottom", conditionShape.Connectors[1].Connections[0].From.Name);
                // right : true rule
                Assert.AreEqual("Right", conditionShape.Connectors[2].Name);
                Assert.AreEqual(conditionShape.Connectors[2], conditionShape.Connectors[2].Connections[0].From);
                Assert.AreEqual("Left", conditionShape.Connectors[2].Connections[0].To.Name);
                Assert.AreEqual("Right", conditionShape.Connectors[2].Connections[0].From.Name);
                // bottom : false rule
                Assert.AreEqual("Bottom", conditionShape.Connectors[3].Name);
                Assert.AreEqual(conditionShape.Connectors[3], conditionShape.Connectors[3].Connections[0].From);
                Assert.AreEqual("Left", conditionShape.Connectors[3].Connections[0].To.Name);
                Assert.AreEqual("Bottom", conditionShape.Connectors[3].Connections[0].From.Name);
            }
        }

        [Test]
        public void ClearFeatureResetsShapeText()
        {
            var controlGroup = new ControlGroup {Inputs = {new Input {Feature = new RtcTestFeature()}}};

            // init controls
            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;

                controlGroup.Inputs[0].Feature = null;

                // check shape text
                ShapeBase inputShape = graphControl.GetShapes<ShapeBase>().First(s => s.Tag is Input);
                Assert.AreEqual("[Not Set]", inputShape.Title);
            }
        }

        [Test]
        public void SetFeatureUpdatesShapeText()
        {
            var controlGroup = new ControlGroup {Inputs = {new Input()}};

            // init controls
            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;

                controlGroup.Inputs[0].ParameterName = "p";
                controlGroup.Inputs[0].Feature = new RtcTestFeature {Name = "f"};

                // check shape text
                ShapeBase inputShape = graphControl.GetShapes<ShapeBase>().First(s => s.Tag is Input);
                Assert.AreEqual("f_p", inputShape.Title);
            }
        }

        [Test]
        public void DomainObject2ShapePropertyChangedTest()
        {
            ControlGroup controlGroup = RealTimeControlTestHelper.CreateGroup2Rules();
            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;
                IList<ShapeBase> shapes = new List<ShapeBase>(graphControl.GetShapes<ShapeBase>());

                foreach (ShapeBase shape in shapes)
                {
                    if (shape.Tag is INameable)
                    {
                        Assert.IsTrue(shape.Title == ((INameable) shape.Tag).Name);
                    }
                }

                ConditionBase firstCondition = controlGroup.Conditions.FirstOrDefault();
                firstCondition.Name = "firstCondition";
                RuleBase firstRule = controlGroup.Rules.FirstOrDefault();
                firstRule.Name = "firstRule";
                foreach (ShapeBase shape in shapes)
                {
                    if (shape.Tag is INameable)
                    {
                        Assert.IsTrue(shape.Title == ((INameable) shape.Tag).Name);
                    }
                }
            }
        }

        [Test]
        public void CopyToClipboard()
        {
            var controlGroup = new ControlGroup();
            using (var clipboardMock = new ClipboardMock())
            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                clipboardMock.GetText_Returns_SetText();

                var menuItem = new MenuItem {Tag = rule};
                controlGroupEditor.CopyXmlToClipboard(menuItem, null);
                AssertCopyXmlToClipboard(rule, controlGroup.Name);

                menuItem = new MenuItem {Tag = condition};
                controlGroupEditor.CopyXmlToClipboard(menuItem, null);
                AssertCopyXmlToClipboard(condition, controlGroup.Name);
            }
        }

        [Test]
        public void CopyRuleToClipBoard()
        {
            var controlGroup = new ControlGroup();
            using (var clipboardMock = new ClipboardMock())
            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                clipboardMock.GetText_Returns_SetText();

                controlGroupEditor.CopyXmlToClipboard(rule);

                AssertCopyXmlToClipboard(rule, controlGroup.Name);
            }
        }

        [Test]
        public void CopyConditionToClipBoard()
        {
            var controlGroup = new ControlGroup();
            using (var clipboardMock = new ClipboardMock())
            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                clipboardMock.GetText_Returns_SetText();

                controlGroupEditor.CopyXmlToClipboard(condition);

                AssertCopyXmlToClipboard(condition, controlGroup.Name);
            }
        }

        [Test]
        public void CopyExpressionToClipBoard()
        {
            var controlGroup = new ControlGroup();
            using (var clipboardMock = new ClipboardMock())
            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                clipboardMock.GetText_Returns_SetText();

                var input = new Input();
                var expression = new MathematicalExpression();
                expression.Inputs.Add(input);

                expression.Expression = "A+6+8";

                controlGroupEditor.CopyXmlToClipboard(expression);

                AssertCopyXmlToClipboard(expression, controlGroup.Name);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void LayoutOfShapesIsRestoredOnViewOpen()
        {
            using (IGui gui = new DHYDROGuiBuilder().Build())
            {
                gui.Run();

                gui.Application.ProjectService.CreateProject();

                // setup mock model / control group
                var controlGroup = new ControlGroup();
                controlGroup.Rules.Add(new PIDRule {Name = "testRule"});

                var model = mocks.StrictMultiMock<IRealTimeControlModel>(typeof(INotifyCollectionChanged), typeof(INotifyPropertyChanged));

                ((INotifyCollectionChanged) model).CollectionChanged -= null;
                LastCall.Repeat.Any().IgnoreArguments();
                ((INotifyCollectionChanged) model).CollectionChanged += null;
                LastCall.Repeat.Any().IgnoreArguments();
                ((INotifyPropertyChanged) model).PropertyChanged -= null;
                LastCall.Repeat.Any().IgnoreArguments();
                ((INotifyPropertyChanged) model).PropertyChanged += null;
                LastCall.Repeat.Any().IgnoreArguments();

                Expect.Call(model.GetChildDataItemLocationsFromControlledModels(DataItemRole.None)).IgnoreArguments().Return(Enumerable.Empty<IFeature>()).Repeat.AtLeastOnce();
                model.ControlGroups = new EventedList<ControlGroup> {controlGroup};
                model.Stub(m => m.ControlledModels).Return(new EventedList<IModel>());
                mocks.ReplayAll();

                // create view
                var controlGroupGraphView = new ControlGroupGraphView
                {
                    Data = controlGroup,
                    Model = model
                };
                gui.DocumentViews.Add(controlGroupGraphView);

                // change X of the first shape
                var controller = (ControlGroupEditorController) TypeUtils.GetField(controlGroupGraphView.ControlGroupEditor, "controller");
                controller.GraphControl.Shapes[0].X = 100;

                // remove view (will keep view context containing layout of the shapes)
                gui.DocumentViews.Remove(controlGroupGraphView);

                ViewInfo viewInfo = new RealTimeControlGuiPlugin().GetViewInfoObjects().FirstOrDefault(vi => vi.ViewType == typeof(ControlGroupGraphView));

                // re-create view (reloads view context)
                var newControlGroupGraphView = new ControlGroupGraphView
                {
                    Data = controlGroup,
                    Model = model,
                    ViewInfo = viewInfo
                };
                gui.DocumentViews.Add(newControlGroupGraphView);

                // check if coordinate is restored
                controller = (ControlGroupEditorController) TypeUtils.GetField(newControlGroupGraphView.ControlGroupEditor, "controller");
                Assert.AreEqual(100, controller.GraphControl.Shapes[0].X);
            }
        }

        [Test]
        public void ControlGroupEditorSupportsCopyPaste()
        {
            // Setup
            var helper = RealTimeControlModelCopyPasteHelper.Instance;
            helper.ClearData();

            // Precondition
            // Note: the helper is a singleton, so for every test make sure 
            // that the helper is in a clear state
            Assert.That(helper.IsDataSet, Is.False);
            Assert.That(helper.CopiedShapes, Is.Empty);

            var controlGroup = new ControlGroup();
            using (var controlGroupEditor = new ControlGroupEditor {Data = controlGroup})
            {
                GraphControl graphControl = controlGroupEditor.GraphControl;

                controlGroup.Inputs.Add(new Input());
                controlGroup.Outputs.Add(new Output());
                var pidRule = new PIDRule();
                controlGroup.Rules.Add(pidRule);
                controlGroup.Conditions.Add(new StandardCondition());

                ControlGroupEditorController controller = controlGroupEditor.Controller;
                //new ControlGroupEditorController { ControlGroup = controlGroup, GraphControl = graphControl };
                IEnumerable<ShapeBase> shapeCollection = graphControl.GetShapes<ShapeBase>();

                helper.SetCopiedData(shapeCollection);

                // Precondition
                Assert.That(helper.IsDataSet, Is.True);
                Assert.That(helper.CopiedShapes.Count(), Is.EqualTo(shapeCollection.Count()));

                // Call
                helper.CopyShapesToController(controller, Point.Empty);

                // Assert
                Assert.AreEqual(8, graphControl.GetShapes<ShapeBase>().Count());
            }
        }

        [Test]
        public void LinkOutputToInputDataItem()
        {
            var mockModel = mocks.Stub<IRealTimeControlModel>();
            var mockOutputShape = mocks.Stub<Shape>();
            var mockInputDataItem = mocks.Stub<IDataItem>();

            mockOutputShape.Tag = new Output();
            mockInputDataItem.Role = DataItemRole.Input;

            mockModel.Expect(m => m.GetDataItemByValue(null)).IgnoreArguments().Return(mockInputDataItem);
            mockModel.Expect(m => m.BeginEdit("")).IgnoreArguments();
            mockModel.Expect(m => m.EndEdit()).IgnoreArguments();
            mockInputDataItem.Expect(i => i.LinkedBy).IgnoreArguments().Return(new EventedList<IDataItem>());
            mockInputDataItem.Expect(i => i.LinkTo(null)).IgnoreArguments().Return(true);
            mockInputDataItem.Stub(i => i.LinkedTo).Return(null);

            mocks.ReplayAll();

            using (var control = new ControlGroupEditor {Model = mockModel})
            {
                control.Link(mockOutputShape, mockInputDataItem, null);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void AttemptToLinkTwiceToSameInputIsFine()
        {
            var mockModel = mocks.Stub<IRealTimeControlModel>();
            var shape = mocks.Stub<Shape>();
            var dataItem = mocks.Stub<IDataItem>();
            var diLink = mocks.Stub<IDataItem>();

            shape.Tag = new Input();
            dataItem.Role = DataItemRole.Output;

            mockModel.Expect(m => m.GetDataItemByValue(null)).IgnoreArguments().Return(dataItem);
            mockModel.Expect(m => m.BeginEdit("")).IgnoreArguments();
            mockModel.Expect(m => m.EndEdit()).IgnoreArguments();
            dataItem.Expect(i => i.LinkTo(null)).IgnoreArguments().Return(true);

            mocks.ReplayAll();

            using (var control = new ControlGroupEditor {Model = mockModel})
            {
                control.Link(shape, dataItem, null);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void AttemptToLinkTwiceToSameOutputRemovesOldLink()
        {
            var rtcModel = mocks.Stub<IRealTimeControlModel>();
            var shape = mocks.Stub<Shape>();
            var flowDataItem = mocks.Stub<IDataItem>();
            var existingLinkedDataItem = mocks.Stub<IDataItem>();

            shape.Tag = new Output();
            flowDataItem.Role = DataItemRole.Input;

            rtcModel.Expect(m => m.GetDataItemByValue(null)).IgnoreArguments().Return(flowDataItem);
            rtcModel.Expect(m => m.BeginEdit("")).IgnoreArguments();
            rtcModel.Expect(m => m.EndEdit()).IgnoreArguments();
            flowDataItem.Expect(i => i.LinkedBy).IgnoreArguments().Return(new EventedList<IDataItem> {existingLinkedDataItem});
            existingLinkedDataItem.Expect(l => l.Unlink()).IgnoreArguments();
            flowDataItem.Expect(i => i.LinkTo(null)).IgnoreArguments().Return(true);
            flowDataItem.Stub(i => i.LinkedTo).Return(null);

            mocks.ReplayAll();

            using (var control = new ControlGroupEditor {Model = rtcModel})
            {
                control.Link(shape, flowDataItem, null);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void Link_ShapeTagIsOutputAndInquireContinuationTrue_LinksDataItem()
        {
            // Arrange
            var groupEditor = new ControlGroupEditor();

            var controlGroup = new ControlGroup();
            groupEditor.Data = controlGroup;
            var model = Substitute.For<IRealTimeControlModel>();
            groupEditor.Model = model;

            var target = Substitute.For<IDataItem>();
            model.GetDataItemByValue(default(object)).ReturnsForAnyArgs(target);
            model.WhenForAnyArgs(x => x.BeginEdit("")).Do(x => { return; });

            var shape = Substitute.For<Shape>();
            shape.Tag = new Output();

            var dataItem = Substitute.For<IDataItem>();
            dataItem.Role = DataItemRole.Input;

            var inquiryHelper = Substitute.For<IInquiryHelper>();
            inquiryHelper.InquireContinuation(Resources.RealTimeControlModelNodePresenter_OutputLocationWarningMessage).Returns(true);

            // Act
            groupEditor.Link(shape, dataItem, inquiryHelper);

            // Assert

            dataItem.Received(1).LinkTo(target);
        }

        [Test]
        public void Link_ShapeTagIsOutputAndInquireContinuationTrue_DoesNotLinksDataItem()
        {
            // Arrange
            var groupEditor = new ControlGroupEditor();

            var controlGroup = new ControlGroup();
            groupEditor.Data = controlGroup;
            var model = Substitute.For<IRealTimeControlModel>();
            groupEditor.Model = model;

            var target = Substitute.For<IDataItem>();

            var shape = Substitute.For<Shape>();
            shape.Tag = new Output();

            var dataItem = Substitute.For<IDataItem>();
            dataItem.Role = DataItemRole.Input;
            var inquiryHelper = Substitute.For<IInquiryHelper>();
            inquiryHelper.InquireContinuation(Resources.RealTimeControlModelNodePresenter_OutputLocationWarningMessage).Returns(false);

            // Act
            groupEditor.Link(shape, dataItem, inquiryHelper);

            // Assert
            dataItem.DidNotReceive().LinkTo(target);
        }

        private static void AssertCopyXmlToClipboard(RtcBaseObject rtcBaseObject, string controlGroupName)
        {
            RtcSerializerBase serializer = SerializerCreator.CreateSerializerType(rtcBaseObject);
            IEnumerable<XElement> listXElements = serializer.ToXml(Fns, controlGroupName);
            var stringBuilder = new StringBuilder();
            foreach (XElement xElement in listXElements)
            {
                stringBuilder.Append(xElement + Environment.NewLine);
            }

            Assert.AreEqual(stringBuilder.ToString(), Clipboard.GetText());
        }
    }
}