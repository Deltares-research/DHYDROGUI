using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using DeltaShell.Plugins.NetworkEditor.Tests.Forms.CompositeStructureView;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{

    [TestFixture]
    public class WeirViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmpty()
        {
            var view = new WeirView
                           {
                               Data = null
                           };
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        public void CheckIfWeirViewDataDoesNotContainAdvancedWeir()
        {
            var view = new WeirView
            {
                Data = null
            };

            var weirViewData = TypeUtils.GetField<WeirView, WeirViewData>(view, "weirViewData");
            Assert.That(() => weirViewData.GetWeirCurrentFormula(typeof(PierWeirFormula)),
                Throws.Exception.TypeOf<KeyNotFoundException>());
            var pierWeirFormula = new PierWeirFormula();
            Assert.That(() => weirViewData.GetWeirFormulaType(pierWeirFormula.Name),
                Throws.Exception.TypeOf<KeyNotFoundException>());
            Assert.That(() => weirViewData.GetWeirFormulaTypeName(pierWeirFormula), Is.Null);

            var weir = new Weir() {WeirFormula = pierWeirFormula};
            Assert.That(() => { weirViewData.UpdateDataWithWeir(weir); }, Throws.Nothing);
            Assert.That(() => { view.Data = weir; }, Throws.Nothing);
        }
        [Test]
        public void CheckIfWeirViewDataDoesNotContainRiverWeir()
        {
            var view = new WeirView
            {
                Data = null
            };

            var weirViewData = TypeUtils.GetField<WeirView, WeirViewData>(view, "weirViewData");
            Assert.That(() => weirViewData.GetWeirCurrentFormula(typeof(RiverWeirFormula)),
                Throws.Exception.TypeOf<KeyNotFoundException>());
            var riverWeirFormula = new RiverWeirFormula();
            Assert.That(() => weirViewData.GetWeirFormulaType(riverWeirFormula.Name),
                Throws.Exception.TypeOf<KeyNotFoundException>());
            Assert.That(() => weirViewData.GetWeirFormulaTypeName(riverWeirFormula), Is.Null);

            var weir = new Weir() {WeirFormula = riverWeirFormula};
            Assert.That(() => { weirViewData.UpdateDataWithWeir(weir); }, Throws.Nothing);
            Assert.That(() => { view.Data = weir; }, Throws.Nothing);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirView()
        {
            var weir = new Weir("TestWeir");

            var weirView = new WeirView
                               {
                                   Data = weir
                               };

            WindowsFormsTestHelper.ShowModal(weirView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirViewWithGatedWeir()
        {
            var weir = new Weir("TestWeir");
            weir.WeirFormula = new GatedWeirFormula {GateOpening = 5};
            var weirView = new WeirView
            {
                Data = weir
            };

            WindowsFormsTestHelper.ShowModal(weirView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirViewWithGatedWeirAndActionToValidateIfGatedWeirFormulaIsUpdatedViaAnEvent()
        {
            var weir = new Weir("TestWeir");
            weir.WeirFormula = new GatedWeirFormula { LowerEdgeLevel = 18, GateOpening = 5 };
            var weirView = new WeirView
            {
                Data = weir
            };

            Action<Form> action = form =>
            {
                Control[] controls = form.Controls.Find("textBoxLowerEdgeLevel", true);
                Assert.That(int.Parse(controls[0].Text, NumberStyles.Any, CultureInfo.CurrentCulture), Is.EqualTo(18));
                ((IGatedWeirFormula)weir.WeirFormula).LowerEdgeLevel = 25;
                Assert.That(int.Parse(controls[0].Text, NumberStyles.Any, CultureInfo.CurrentCulture), Is.EqualTo(25));
            };
            WindowsFormsTestHelper.ShowModal(weirView, action);
        }
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirViewWithDetailedCrestDefinition()
        {
            var weir = new Weir("TestWeir") {WeirFormula = new RiverWeirFormula()};
            var weirView = new WeirView
            {
                Data = weir
            };

            WindowsFormsTestHelper.ShowModal(weirView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowRiverWeirViewShouldNotChangeValues()
        {
            var weir = new Weir("TestWeir")
                           {
                               WeirFormula =
                                   new RiverWeirFormula
                                       {
                                           CorrectionCoefficientNeg = 0.15,
                                           CorrectionCoefficientPos = 0.29,
                                           SubmergeLimitNeg = 0.77,
                                           SubmergeLimitPos = 0.44
                                       }
                           };
            var weirView = new WeirView
            {
                Data = weir
            };

            WindowsFormsTestHelper.ShowModal(weirView);
            Assert.AreEqual(0.15, ((RiverWeirFormula)weir.WeirFormula).CorrectionCoefficientNeg);
            Assert.AreEqual(0.29, ((RiverWeirFormula)weir.WeirFormula).CorrectionCoefficientPos);
            Assert.AreEqual(0.77, ((RiverWeirFormula)weir.WeirFormula).SubmergeLimitNeg);
            Assert.AreEqual(0.44, ((RiverWeirFormula)weir.WeirFormula).SubmergeLimitPos);
        }
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirMDEAndMakeSureWeirFormulaIsAlwaysReadyOnly()
        {
            var view = new VectorLayerAttributeTableView()
            {
                TableView = { AutoGenerateColumns = false }
            };
            view.Data = new VectorLayer
            {
                DataSource = new FeatureCollection(new[] { new Weir() }.ToList(), typeof(Weir))
            };
            view.SetCreateFeatureRowFunction(feature => new WeirRow((IWeir)feature, new NameValidator()));
            WindowsFormsTestHelper.ShowModal(view.TableView, (f) =>
            {
                var tableView = f.Controls.GetAllControlsRecursive().OfType<DelftTools.Controls.Swf.Table.TableView>().SingleOrDefault();
                Assert.That(tableView, Is.Not.Null);
                Assert.That(tableView.Columns.Select(c => c.Name), Contains.Item(nameof(WeirRow.Formula)));
            });
        }
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowOrificeMDEAndMakeSureWeirFormulaIsAlwaysReadyOnly()
        {
            var view = new VectorLayerAttributeTableView()
            {
                TableView = { AutoGenerateColumns = false }
            };
            view.Data = new VectorLayer
            {
                DataSource = new FeatureCollection(new[] { new Orifice() }.ToList(), typeof(Orifice))
            };
            view.SetCreateFeatureRowFunction(feature => new WeirRow((IWeir)feature, new NameValidator()));
            WindowsFormsTestHelper.ShowModal(view.TableView, (f) =>
            {
                var tableView = f.Controls.GetAllControlsRecursive().OfType<DelftTools.Controls.Swf.Table.TableView>().SingleOrDefault();
                Assert.That(tableView, Is.Not.Null);
                Assert.That(tableView.Columns.Select(c => c.Name), Contains.Item(nameof(WeirRow.Formula)));
            });
        }
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirMDEAndMakeSureWeirCrestWidthIsReadOnlyOnUniversalWeirFormula()
        {
            var view = new VectorLayerAttributeTableView()
            {
                TableView = { AutoGenerateColumns = false }
            };
            var weir1 = new Weir();
            var weir2 = new Weir() { WeirFormula = new FreeFormWeirFormula() };
            var weir3 = new Weir() { WeirFormula = new GeneralStructureWeirFormula() };
            view.Data = new VectorLayer
            {
                DataSource = new FeatureCollection(new[] { weir1, weir2, weir3 }.ToList(), typeof(Weir))
            };
            view.SetCreateFeatureRowFunction(feature => new WeirRow((IWeir)feature, new NameValidator()));
            WindowsFormsTestHelper.ShowModal(view.TableView, (f) =>
            {
                var tableView = f.Controls.GetAllControlsRecursive().OfType<DelftTools.Controls.Swf.Table.TableView>().SingleOrDefault();

                Assert.That(tableView, Is.Not.Null);
                Assert.That(tableView.Columns.Select(c => c.Name), Contains.Item(nameof(WeirRow.CrestWidth)));
                Assert.That(tableView.CellIsReadOnly(0, tableView.Columns.ToDictionary(c => c.Name, c => c)[nameof(WeirRow.CrestWidth)]), Is.False);
                Assert.That(tableView.CellIsReadOnly(1, tableView.Columns.ToDictionary(c => c.Name, c => c)[nameof(WeirRow.CrestWidth)]), Is.True);
                Assert.That(tableView.CellIsReadOnly(2, tableView.Columns.ToDictionary(c => c.Name, c => c)[nameof(WeirRow.CrestWidth)]), Is.False);
            });
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirMDEAndMakeSureUseVelocityHeightIsReadOnlyOnUniversalWeirFormula()
        {
            var view = new VectorLayerAttributeTableView()
            {
                TableView = {AutoGenerateColumns = false}
            };
            var weir1 = new Weir();
            var weir2 = new Weir() {WeirFormula = new FreeFormWeirFormula()};
            var weir3 = new Weir() {WeirFormula = new GeneralStructureWeirFormula()};

            view.Data = new VectorLayer
            {
                DataSource = new FeatureCollection(new[] {weir1, weir2, weir3}.ToList(), typeof(Weir))
            };
            view.SetCreateFeatureRowFunction(feature => new WeirRow((IWeir) feature, new NameValidator()));
            WindowsFormsTestHelper.ShowModal(view.TableView, (f) =>
            {
                var tableView = f.Controls.GetAllControlsRecursive().OfType<DelftTools.Controls.Swf.Table.TableView>()
                    .SingleOrDefault();

                Assert.That(tableView, Is.Not.Null);
                Assert.That(tableView.Columns.Select(c => c.Name),
                    Contains.Item(nameof(WeirRow.UseVelocityHeight)));
                Assert.That(
                    tableView.CellIsReadOnly(0,
                        tableView.Columns.ToDictionary(c => c.Name, c => c)[
                            nameof(WeirRow.UseVelocityHeight)]), Is.False);
                Assert.That(
                    tableView.CellIsReadOnly(1,
                        tableView.Columns.ToDictionary(c => c.Name, c => c)[
                            nameof(WeirRow.UseVelocityHeight)]), Is.True);
                Assert.That(
                    tableView.CellIsReadOnly(2,
                        tableView.Columns.ToDictionary(c => c.Name, c => c)[
                            nameof(WeirRow.UseVelocityHeight)]), Is.False);
            });
        }
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirMDEAndMakeSureUseVelocityHeightIsNotReadOnlyOnOrifices()
        {
            var view = new VectorLayerAttributeTableView()
            {
                TableView = { AutoGenerateColumns = false }
            };

            var orifice = new Orifice();
            view.Data = new VectorLayer
            {
                DataSource = new FeatureCollection(new[] { orifice }.ToList(), typeof(Orifice))
            };
            view.SetCreateFeatureRowFunction(feature => new WeirRow((IWeir)feature, new NameValidator()));
            WindowsFormsTestHelper.ShowModal(view.TableView, (f) =>
            {
                var tableView = f.Controls.GetAllControlsRecursive().OfType<DelftTools.Controls.Swf.Table.TableView>().SingleOrDefault();

                Assert.That(tableView, Is.Not.Null);
                Assert.That(tableView.Columns.Select(c => c.Name), Contains.Item(nameof(WeirRow.UseVelocityHeight)));
                Assert.That(tableView.CellIsReadOnly(0, tableView.Columns.ToDictionary(c => c.Name, c => c)[nameof(WeirRow.UseVelocityHeight)]), Is.False);
            });
        }
        [Test]
        public void CheckIfWeirViewDataDoesNotContainGatedWeir()
        {
            var view = new WeirView
            {
                Data = null
            };

            var weirViewData = TypeUtils.GetField<WeirView, WeirViewData>(view, "weirViewData");
            Assert.That(() => weirViewData.GetWeirCurrentFormula(typeof(GatedWeirFormula)),
                Throws.Exception.TypeOf<KeyNotFoundException>());
            var gatedWeirFormula = new GatedWeirFormula();
            Assert.That(() => weirViewData.GetWeirFormulaType(gatedWeirFormula.Name),
                Throws.Exception.TypeOf<KeyNotFoundException>());
            Assert.That(() => weirViewData.GetWeirFormulaTypeName(gatedWeirFormula), Is.Null);

            var weir = new Weir() { WeirFormula = gatedWeirFormula };
            Assert.That(() => { weirViewData.UpdateDataWithWeir(weir); }, Throws.Nothing);
            Assert.That(() => { view.Data = weir; }, Throws.Nothing);
        }
        [Test]
        public void CheckIfWeirViewDataWithOrificeDoesContainGatedWeir()
        {
            var view = new WeirView
            {
                Data = new Orifice()

            };

            var weirViewData = TypeUtils.GetField<WeirView, WeirViewData>(view, "weirViewData");
            Assert.That(() => weirViewData.GetWeirCurrentFormula(typeof(GatedWeirFormula)), Throws.Nothing);
            var gatedWeirFormula = new GatedWeirFormula();
            Assert.That(() => weirViewData.GetWeirFormulaType(gatedWeirFormula.Name), Throws.Nothing);
            Assert.That(() => weirViewData.GetWeirFormulaTypeName(gatedWeirFormula), Is.EqualTo(gatedWeirFormula.Name));

            var weir = new Weir() { WeirFormula = gatedWeirFormula };
            Assert.That(() => { weirViewData.UpdateDataWithWeir(weir); }, Throws.Nothing);
            Assert.That(() => { view.Data = weir; }, Throws.Nothing);
        }
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowBridge()
        {
            var network = CompositeStructureViewTestHelper.CreateDummyNetwork();
            var bridge = CompositeStructureViewTestHelper.GetBridge();
            bridge.BridgeType = BridgeType.Rectangle;

            CompositeStructureViewTestHelper.ShowStructureAtFirstBranch(bridge, network);
        }
    }
}