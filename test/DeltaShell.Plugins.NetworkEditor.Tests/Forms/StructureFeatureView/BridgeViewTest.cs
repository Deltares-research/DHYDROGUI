using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DelftTools.Controls.Swf.Table;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{
    [TestFixture]
    public class BridgeViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmpty()
        {
            var bridgeView = new BridgeView {Data = null};
            WindowsFormsTestHelper.ShowModal(bridgeView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowBridgeView()
        {
            var bridge = new Bridge();
            bridge.BridgeType = BridgeType.Rectangle;
            bridge.FrictionType = BridgeFrictionType.WhiteColebrook;
            bridge.Friction = 99.0;
            bridge.OffsetY = 10.0;
            bridge.SetRectangleCrossSection(0.0,5.0,2.0);
            bridge.OutletLossCoefficient = 0.5;
            bridge.InletLossCoefficient = 0.35;
            //ground layer stuff
            bridge.GroundLayerEnabled = true;
            bridge.GroundLayerThickness = 0.5;
            bridge.GroundLayerRoughness = 0.05;
            bridge.AllowNegativeFlow = false;
            bridge.AllowPositiveFlow = true;

            var bridgeView = new BridgeView();
            bridgeView.Data = bridge;
            WindowsFormsTestHelper.ShowModal(bridgeView, f =>
            {
                var control = f.Controls.GetAllControlsRecursive().SingleOrDefault(c => c.Name == "groupBox2");
                Assert.IsNotNull(control);
                //Assert.True(control.Visible);// not yet implemented in the kernel
                Assert.False(control.Visible);
            });
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void BridgeViewWithPillarBridgeFunctionality()
        {
            var bridge = new Bridge();
            bridge.BridgeType = BridgeType.Pillar;
            bridge.AllowNegativeFlow = false;
            bridge.AllowPositiveFlow = true;
            bridge.PillarWidth = 84.2;
            bridge.ShapeFactor = 1.2;

            var bridgeView = new BridgeView();
            bridgeView.Data = bridge;
            bridgeView.Load += delegate
                                   {
                                       var bridgeTypeCombobox = bridgeView.Controls.Find("bridgeTypeCombobox", true).FirstOrDefault() as ComboBox;
                                       var txtPillarBridge = bridgeView.Controls.Find("textBoxPillarWidth", true).FirstOrDefault() as TextBox;
                                       var txtShapeFactor = bridgeView.Controls.Find("textBoxShapeFactor", true).FirstOrDefault() as TextBox;
                                       Assert.IsNotNull(bridgeTypeCombobox);
                                       Assert.IsNotNull(txtPillarBridge);
                                       Assert.IsNotNull(txtShapeFactor);
                                       //Assert.That((BridgeType)bridgeTypeCombobox.SelectedItem, Is.EqualTo(BridgeType.Pillar)); // not yet implemented in the kernel
                                       Assert.That((BridgeType[]) bridgeTypeCombobox.DataSource,Does.Not.Contains(BridgeType.Pillar));
                                       Assert.IsTrue(txtPillarBridge.Text.StartsWith("84"));
                                       Assert.IsTrue(txtShapeFactor.Text.StartsWith("1"));
                                       Assert.IsTrue(txtPillarBridge.Enabled);
                                       Assert.IsTrue(txtShapeFactor.Enabled);
                                       bridge.BridgeType = BridgeType.Rectangle;
                                       Assert.IsFalse(txtPillarBridge.Enabled, "test after type change");
                                       Assert.IsFalse(txtShapeFactor.Enabled);
                                   };
            WindowsFormsTestHelper.ShowModal(bridgeView);
        }

        [Test]
        public void InputValidatorTest()
        {
            var bridge = new Bridge
                {
                    BridgeType = BridgeType.Tabulated
                };
            bridge.TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(0, 2, 0);
            bridge.TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(2, 2, 0);

            var view = new BridgeView { Data = bridge };
            var tableView = TypeUtils.GetField<BridgeView, TableView>(view, "tableViewTabulatedData");
            tableView.ExceptionMode = TableView.ValidationExceptionMode.NoAction;

            Assert.AreEqual(2.0, tableView.GetCellValue(0, 0));
            Assert.AreEqual(0.0, tableView.GetCellValue(1, 0));

            var succes = true;
            const string errorMsg = "Can not set value into cell [1, 0] reason:Validation of cell failed: Z must be unique.";
            TestHelper.AssertLogMessageIsGenerated(() => succes = tableView.SetCellValue(1, 0, "2"), errorMsg, 1);
            Assert.IsFalse(succes, "Should not allow a duplicate to be entered.");

            // Verify that data is unchanged:
            Assert.AreEqual(2.0, tableView.GetCellValue(0, 0));
            Assert.AreEqual(0.0, tableView.GetCellValue(1, 0));
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowBridgeMDE()
        {
            var view = new VectorLayerAttributeTableView()
            {
                TableView = { AutoGenerateColumns = false }
            };
            view.Data = new VectorLayer
            {
                DataSource = new FeatureCollection(new[] { Bridge.CreateDefault() }.ToList(), typeof(Bridge))
            };
            WindowsFormsTestHelper.ShowModal(view.TableView);
        }
    }
}