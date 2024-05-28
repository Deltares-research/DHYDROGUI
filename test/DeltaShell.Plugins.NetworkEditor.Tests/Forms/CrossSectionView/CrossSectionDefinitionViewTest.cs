using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Table;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpTestsEx;
using Clipboard = DelftTools.Controls.Clipboard;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView
{
    [TestFixture]
    public class CrossSectionDefinitionViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void PasteIntoTableOfZWCrossSection()
        {
            EventedList<CrossSectionSectionType> sectionTypes = GetSectionTypesList(new[] {"Main", "FloodPlain1", "FloodPlain2"});
            var hydroNetwork = new HydroNetwork {CrossSectionSectionTypes = sectionTypes};

            var crossSection = new CrossSectionDefinitionZW();
            var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection, hydroNetwork);
            
            var crossSectionView = new CrossSectionDefinitionView
                {
                    Data = crossSection,
                    ViewModel = viewModel
                };

            var tableView = crossSectionView.FieldValue<TableView>("tableView"); //hax
            var controller = tableView.PasteController;

            crossSection.BeginEdit("test");
            controller.PasteLines(new[] { "0\t1\t1", "1\t1\t1", "2\t1\t1" });
            crossSection.EndEdit();

            crossSection.RawData.Clear();

            //in order gaps
            controller.PasteLines(new[] { "0\t1\t1", "2\t1\t1", "4\t1\t1" });
            crossSection.RawData.Clear();

            WindowsFormsTestHelper.ShowModal(crossSectionView, f =>
                                                                   {
                                                                       //in order
                                                                       controller.PasteLines(
                                                                           new[] { "0\t1\t1", "1\t1\t1", "2\t1\t1" });
                                                                       crossSection.RawData.Clear();

                                                                       //in order gaps
                                                                       controller.PasteLines(
                                                                           new[] { "0\t1\t1", "2\t1\t1", "4\t1\t1" });
                                                                       crossSection.RawData.Clear();

                                                                       //out of order (not supported fully)
                                                                       controller.PasteLines(
                                                                           new[] { "0\t1\t1", "5\t1\t1", "1\t1\t1" });
                                                                   });
        }

        [Test]
        public void DeleteCellWithZvalueShouldNotThrowWhenCausingDuplicatesTools9909()
        {
            EventedList<CrossSectionSectionType> sectionTypes = GetSectionTypesList(new[] { "Main", "FloodPlain1", "FloodPlain2" });
            var hydroNetwork = new HydroNetwork { CrossSectionSectionTypes = sectionTypes };

            var crossSection = new CrossSectionDefinitionZW();
            crossSection.SetDefaultZWTable();

            var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection, hydroNetwork);

            var crossSectionView = new CrossSectionDefinitionView
                {
                    Data = crossSection,
                    ViewModel = viewModel
                };

            var table = TypeUtils.GetField<CrossSectionDefinitionView, TableView>(crossSectionView, "tableView");

            table.SelectCells(1,0,1,0);
            table.DeleteCurrentSelection(); // Caused exception

            Assert.AreEqual(0, crossSection.ZWDataTable[0].Z);
            Assert.AreEqual(-10, crossSection.ZWDataTable[1].Z);
        }

        [Test]
        public void PreventDeletionOfRowIfMinimumViolated()
        {
            EventedList<CrossSectionSectionType> sectionTypes = GetSectionTypesList(new[] { "Main", "FloodPlain1", "FloodPlain2" });
            var hydroNetwork = new HydroNetwork { CrossSectionSectionTypes = sectionTypes };

            var crossSection = new CrossSectionDefinitionZW();
            crossSection.SetDefaultZWTable();

            var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection, hydroNetwork);

            var crossSectionView = new CrossSectionDefinitionView
            {
                Data = crossSection,
                ViewModel = viewModel
            };

            var table = TypeUtils.GetField<CrossSectionDefinitionView, TableView>(crossSectionView, "tableView");

            table.SelectRows(new[]{0,1});
            table.DeleteCurrentSelection();

            Assert.AreEqual(2, crossSection.ZWDataTable.Count);
            Assert.AreEqual(0, crossSection.ZWDataTable[0].Z);
            Assert.AreEqual(100.0, crossSection.ZWDataTable[0].Width);

            Assert.AreEqual(-10, crossSection.ZWDataTable[1].Z);
            Assert.AreEqual(100.0/3.0, crossSection.ZWDataTable[1].Width);
        }

        [Test]
        public void PreventDeletionOfRowIfMinimumViolatedFiltered()
        {
            EventedList<CrossSectionSectionType> sectionTypes = GetSectionTypesList(new[] { "Main", "FloodPlain1", "FloodPlain2" });
            var hydroNetwork = new HydroNetwork { CrossSectionSectionTypes = sectionTypes };

            var crossSection = new CrossSectionDefinitionZW();
            crossSection.SetDefaultZWTable();
            crossSection.ZWDataTable.AddCrossSectionZWRow(2, 2, 0);

            var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection, hydroNetwork);

            var crossSectionView = new CrossSectionDefinitionView
                {
                    Data = crossSection,
                    ViewModel = viewModel
                };

            var table = TypeUtils.GetField<CrossSectionDefinitionView, TableView>(crossSectionView, "tableView");
            table.SetColumnFilter("Z", String.Format(@"[{0}] = '{1}'", "Z", 2));

            table.SelectRows(new[] { 0 });
            table.DeleteCurrentSelection();

            Assert.AreEqual(2, crossSection.ZWDataTable.Count);
            Assert.AreEqual(0, crossSection.ZWDataTable[0].Z);
            Assert.AreEqual(100.0, crossSection.ZWDataTable[0].Width);

            Assert.AreEqual(-10, crossSection.ZWDataTable[1].Z);
            Assert.AreEqual(100.0 / 3.0, crossSection.ZWDataTable[1].Width);
        }
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void PasteIntoTableOfYZCrossSectionTools8131()
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                EventedList<CrossSectionSectionType> sectionTypes =
                    GetSectionTypesList(new[] {"Main", "FloodPlain1", "FloodPlain2"});
                var hydroNetwork = new HydroNetwork {CrossSectionSectionTypes = sectionTypes};

                var crossSection = new CrossSectionDefinitionYZ();
                crossSection.YZDataTable.AddCrossSectionYZRow(0, 0);
                crossSection.YZDataTable.AddCrossSectionYZRow(50, 0);
                crossSection.YZDataTable.AddCrossSectionYZRow(100, 0);
                var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection, hydroNetwork);

                var crossSectionView = new CrossSectionDefinitionView
                {
                    Data = crossSection,
                    ViewModel = viewModel
                };

                var tableView = crossSectionView.FieldValue<TableView>("tableView"); //hax
                var controller = tableView.PasteController;

                WindowsFormsTestHelper.ShowModal(crossSectionView, f => controller.PasteLines(
                    new[]
                        {
                            "0\t1\t1",
                            "33.33\t1\t1",
                            "50.0\t1\t1",
                            "100.0\t1\t1",
                        }));

                Assert.AreEqual(0.0, (double) crossSection.YZDataTable.Rows[0][0], 0.001);
                Assert.AreEqual(33.33, (double)crossSection.YZDataTable.Rows[1][0], 0.001);
                Assert.AreEqual(50.0, (double)crossSection.YZDataTable.Rows[2][0], 0.001);
                Assert.AreEqual(100.0, (double)crossSection.YZDataTable.Rows[3][0], 0.001);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void PasteSingleColumnIntoTableOfYZCrossSectionTools8968()
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                EventedList<CrossSectionSectionType> sectionTypes =
                    GetSectionTypesList(new[] { "Main", "FloodPlain1", "FloodPlain2" });
                var hydroNetwork = new HydroNetwork { CrossSectionSectionTypes = sectionTypes };

                var crossSection = new CrossSectionDefinitionYZ();
                crossSection.YZDataTable.AddCrossSectionYZRow(0, 0);
                crossSection.YZDataTable.AddCrossSectionYZRow(50, 0);
                crossSection.YZDataTable.AddCrossSectionYZRow(100, 0);
                var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection, hydroNetwork);

                var crossSectionView = new CrossSectionDefinitionView
                {
                    Data = crossSection,
                    ViewModel = viewModel
                };

                var tableView = crossSectionView.FieldValue<TableView>("tableView"); //hax
                var controller = tableView.PasteController;

                WindowsFormsTestHelper.ShowModal(crossSectionView, f =>
                    {
                        tableView.SelectCells(2, 1, 2, 1);
                        controller.PasteLines(
                            new[]
                                {
                                    "0",
                                    "1",
                                    "2",
                                    "3",
                                });
                    }
                    );
                Assert.AreEqual(6, crossSection.YZDataTable.Rows.Count, "something should be pasted");
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void PasteSingleColumnIntoTableOfZWCrossSectionTools8968()
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                EventedList<CrossSectionSectionType> sectionTypes =
                    GetSectionTypesList(new[] { "Main", "FloodPlain1", "FloodPlain2" });
                var hydroNetwork = new HydroNetwork { CrossSectionSectionTypes = sectionTypes };

                var crossSection = new CrossSectionDefinitionZW();
                crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);
                crossSection.ZWDataTable.AddCrossSectionZWRow(50, 0, 0);
                crossSection.ZWDataTable.AddCrossSectionZWRow(100, 0, 0);
                var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection, hydroNetwork);

                var crossSectionView = new CrossSectionDefinitionView
                {
                    Data = crossSection,
                    ViewModel = viewModel
                };

                var tableView = crossSectionView.FieldValue<TableView>("tableView"); //hax
                var controller = tableView.PasteController;

                WindowsFormsTestHelper.ShowModal(crossSectionView, f =>
                    {
                        tableView.SelectCells(2, 1, 2, 1);
                        controller.PasteLines(
                            new[]
                                {
                                    "0",
                                    "1",
                                    "2",
                                    "3",
                                });
                    }
                    );
                Assert.AreEqual(6, crossSection.ZWDataTable.Rows.Count, "something should be pasted");
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void PasteIntoTableOfYZCrossSectionTools8968()
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                EventedList<CrossSectionSectionType> sectionTypes =
                    GetSectionTypesList(new[] { "Main", "FloodPlain1", "FloodPlain2" });
                var hydroNetwork = new HydroNetwork { CrossSectionSectionTypes = sectionTypes };

                var crossSection = new CrossSectionDefinitionYZ();
                crossSection.YZDataTable.AddCrossSectionYZRow(0, 0);
                crossSection.YZDataTable.AddCrossSectionYZRow(50, 0);
                crossSection.YZDataTable.AddCrossSectionYZRow(100, 0);
                var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection, hydroNetwork);

                var crossSectionView = new CrossSectionDefinitionView
                {
                    Data = crossSection,
                    ViewModel = viewModel
                };

                var tableView = crossSectionView.FieldValue<TableView>("tableView"); //hax
                var controller = tableView.PasteController;

                WindowsFormsTestHelper.ShowModal(crossSectionView, f =>
                    {
                        tableView.SelectCells(2, 0, 2, 0);
                        controller.PasteLines(
                            new[]
                                {
                                    "150\t0\t0",
                                    "20\t0\t0",
                                    "40\t0\t0",
                                    "60\t0\t0",
                                });
                    });
                Assert.AreEqual(7, crossSection.YZDataTable.Rows.Count, "something should be pasted");
                Assert.AreEqual(new[] {0, 20, 40, 50, 60, 100, 150},
                                crossSection.YZDataTable.Select(r => r[0]).OrderBy(y => y).ToArray());
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void PasteIntoTableOfYZCrossSectionCheckArgumentBasedOverwritingTools8968()
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                EventedList<CrossSectionSectionType> sectionTypes =
                    GetSectionTypesList(new[] { "Main", "FloodPlain1", "FloodPlain2" });
                var hydroNetwork = new HydroNetwork { CrossSectionSectionTypes = sectionTypes };

                var crossSection = new CrossSectionDefinitionYZ();
                crossSection.YZDataTable.AddCrossSectionYZRow(0, 0);
                crossSection.YZDataTable.AddCrossSectionYZRow(50, 15);
                crossSection.YZDataTable.AddCrossSectionYZRow(100, 5);
                var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection, hydroNetwork);

                var crossSectionView = new CrossSectionDefinitionView
                {
                    Data = crossSection,
                    ViewModel = viewModel
                };

                var tableView = crossSectionView.FieldValue<TableView>("tableView"); //hax
                var controller = tableView.PasteController;

                WindowsFormsTestHelper.ShowModal(crossSectionView, f =>
                {
                    tableView.SelectCells(2, 0, 2, 0);
                    controller.PasteLines(
                        new[]
                                {
                                    "150\t10\t0",
                                    "20\t0\t0",
                                    "50\t25\t0",
                                });
                });
                Assert.AreEqual(new[] { 0, 20, 50, 100, 150 }, crossSection.YZDataTable.OrderBy(r => r[0]).Select(r => r[0]).ToArray());
                Assert.AreEqual(new[] { 0,  0, 25,   5,  10}, crossSection.YZDataTable.OrderBy(r => r[0]).Select(r => r[1]).ToArray());
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void PasteInvalidDataIntoTableOfZWCrossSectionTools8131()
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                EventedList<CrossSectionSectionType> sectionTypes =
                    GetSectionTypesList(new[] {"Main", "FloodPlain1", "FloodPlain2"});
                var hydroNetwork = new HydroNetwork {CrossSectionSectionTypes = sectionTypes};

                var crossSection = CrossSectionDefinitionZW.CreateDefault();
                var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection, hydroNetwork);

                var crossSectionView = new CrossSectionDefinitionView
                {
                    Data = crossSection,
                    ViewModel = viewModel
                };

                var tableView = crossSectionView.FieldValue<TableView>("tableView"); //hax
                var controller = tableView.PasteController;

                WindowsFormsTestHelper.ShowModal(crossSectionView, f => controller.PasteLines(
                    new[]
                        {
                            "0\t1\t2",
                            "-10\t1\t2",
                            "50.0\t1\t1", //valid row
                            "100.0\t1\t2",
                        }));

                Assert.AreEqual(3, crossSection.ZWDataTable.Count);
                Assert.AreEqual(50.0, (double) crossSection.ZWDataTable.Rows[0][0], 0.001);
                Assert.AreEqual(0.0, (double)crossSection.ZWDataTable.Rows[1][0], 0.001);
                Assert.AreEqual(-10, (double)crossSection.ZWDataTable.Rows[2][0], 0.001);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void PasteIntoTableOfYZCrossSectionReverseTools8131()
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                EventedList<CrossSectionSectionType> sectionTypes =
                    GetSectionTypesList(new[] {"Main", "FloodPlain1", "FloodPlain2"});
                var hydroNetwork = new HydroNetwork {CrossSectionSectionTypes = sectionTypes};

                var crossSection = new CrossSectionDefinitionYZ();
                crossSection.YZDataTable.AddCrossSectionYZRow(0, 0);
                crossSection.YZDataTable.AddCrossSectionYZRow(50, 0);
                crossSection.YZDataTable.AddCrossSectionYZRow(100, 0);
                var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection, hydroNetwork);

                var crossSectionView = new CrossSectionDefinitionView
                {
                    Data = crossSection,
                    ViewModel = viewModel
                };

                var tableView = crossSectionView.FieldValue<TableView>("tableView"); //hax
                var controller = tableView.PasteController;

                WindowsFormsTestHelper.ShowModal(crossSectionView, f => controller.PasteLines(
                    new[]
                        {
                            "100.0\t1\t1",
                            "50.0\t1\t1",
                            "0\t1\t1",
                            "33.33\t1\t1",
                        }));

                Assert.AreEqual(0.0, (double)crossSection.YZDataTable.Rows[0][0], 0.001);
                Assert.AreEqual(33.33, (double)crossSection.YZDataTable.Rows[1][0], 0.001);
                Assert.AreEqual(50.0, (double)crossSection.YZDataTable.Rows[2][0], 0.001);
                Assert.AreEqual(100.0, (double)crossSection.YZDataTable.Rows[3][0], 0.001);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void PasteIntoTableAndVerify()
        {
            using (var clipboardMock = new ClipboardMock())
            using (CultureUtils.SwitchToCulture("nl-NL"))
            {
                clipboardMock.GetText_Returns_SetText();

                EventedList<CrossSectionSectionType> sectionTypes = GetSectionTypesList(new[]
                {
                    "Main",
                    "FloodPlain1",
                    "FloodPlain2"
                });
                var hydroNetwork = new HydroNetwork {CrossSectionSectionTypes = sectionTypes};

                var csDef = new CrossSectionDefinitionZW();
                var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(csDef, hydroNetwork);

                var crossSectionView = new CrossSectionDefinitionView
                {
                    Data = csDef,
                    ViewModel = viewModel
                };

                var tableView = crossSectionView.FieldValue<TableView>("tableView"); //hax
                var controller = tableView.PasteController;

                WindowsFormsTestHelper.ShowModal(crossSectionView, f =>
                {
                    var text = "49,74\t374\t12\r\n" + "49,43\t374\t13\r\n" + "48,74\t368\t19\r\n";
                    text = text.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                    Clipboard.SetText(text);
                    controller.PasteClipboardContents();
                    Assert.AreEqual(3, csDef.RawData.Rows.Count);
                    Assert.AreEqual(12.0, csDef.ZWDataTable[0].StorageWidth);
                    Assert.AreEqual(13.0, csDef.ZWDataTable[1].StorageWidth);
                    Assert.AreEqual(19.0, csDef.ZWDataTable[2].StorageWidth);
                });
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void PasteNewValueIntoTableOfYZCrossSection()
        {
            var sectionTypes = GetSectionTypesList(new[] { "Main", "FloodPlain1", "FloodPlain2" });
            var hydroNetwork = new HydroNetwork { CrossSectionSectionTypes = sectionTypes };

            var crossSection = new CrossSectionDefinitionYZ();
            var viewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection, hydroNetwork);

            var crossSectionView = new CrossSectionDefinitionView
            {
                Data = crossSection,
                ViewModel = viewModel
            };

            var tableView = crossSectionView.FieldValue<TableView>("tableView"); //hax
            var controller = tableView.PasteController;

            Action<Form> shown = delegate
                                     {
                                         controller.PasteLines(new[] { "0\t1\t1" }); //need a 0 value -> new value is default always with a 0 value -> thats the conflict
                                         controller.PasteLines(new[] { "1000\t1\t1" });
                                     };
            WindowsFormsTestHelper.Show(crossSectionView, shown);

            WindowsFormsTestHelper.CloseAll();
        }

        private static EventedList<CrossSectionSectionType> GetSectionTypesList(IEnumerable<string> names)
        {
            return new EventedList<CrossSectionSectionType>(
                names
                    .Select(name => new CrossSectionSectionType
                                        {
                                            Name = name
                                        }));
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithProxyIsReadOnly()
        {
            EventedList<CrossSectionSectionType> sectionTypes = GetSectionTypesList(new[] {"Main"});
            var hydroNetwork = new HydroNetwork { CrossSectionSectionTypes = sectionTypes };

            var innerDefinition = CrossSectionDefinitionZW.CreateDefault();
            var proxyDefinition = new CrossSectionDefinitionProxy(innerDefinition);
            proxyDefinition.LevelShift = 44.0;
            var crossSectionViewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(proxyDefinition, hydroNetwork);

            var crossSectionView = new CrossSectionDefinitionView
            {
                Data = proxyDefinition,
                ViewModel = crossSectionViewModel
            };


            WindowsFormsTestHelper.ShowModal(crossSectionView);
        }
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmptyCrossSection()
        {
            
            var crossSection = new CrossSectionDefinitionZW();
            var hydroNetwork = new HydroNetwork {CrossSectionSectionTypes = GetSectionTypesList(new[] {"Main"})};

            var crossSectionViewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection,hydroNetwork);

            var crossSectionView = new CrossSectionDefinitionView
            {
                Data = crossSection,
                ViewModel = crossSectionViewModel
            };
                                       

            WindowsFormsTestHelper.ShowModal(crossSectionView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithYZCrossSection()
        {
            var crossSectionDefinitionYZ = new CrossSectionDefinitionYZ();
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(0, 8);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(1, 6);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(2, 4);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(3, 3);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(4, 4);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(5, 6);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(6, 8);

            var crossSectionView = new CrossSectionDefinitionView
            {
                Data = crossSectionDefinitionYZ,
                ViewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSectionDefinitionYZ)
            };

            WindowsFormsTestHelper.ShowModal(crossSectionView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithXYZCrossSection()
        {
            var crossSection = new CrossSectionDefinitionXYZ();
            crossSection.Geometry = new LineString(new []
                                   {
                                       new Coordinate(0, 0, 0), new Coordinate(2, 2, -2), new Coordinate(4, 2, -2),
                                       new Coordinate(6, 0, 0)
                                   });
            var crossSectionView = new CrossSectionDefinitionView
            {
                Data = crossSection,
                ViewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection)
            };

            WindowsFormsTestHelper.ShowModal(crossSectionView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithCrossSectionZW()
        {
            var crossSectionDefinitionZW = GetCrossSectionZW();

            var crossSectionView = new CrossSectionDefinitionView
            {
                Data = crossSectionDefinitionZW,
                ViewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSectionDefinitionZW)
            };

            WindowsFormsTestHelper.ShowModal(crossSectionView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithSummerDike()
        {
            CrossSectionDefinitionZW crossSectionDefinitionZW = GetCrossSectionZW();

            crossSectionDefinitionZW.SummerDike.Active = true;
            crossSectionDefinitionZW.SummerDike.CrestLevel = 5;

            var crossSectionView = new CrossSectionDefinitionView
            {
                Data = crossSectionDefinitionZW,
                ViewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSectionDefinitionZW)
            };

            WindowsFormsTestHelper.ShowModal(crossSectionView);
        }

        private static CrossSectionDefinitionZW GetCrossSectionZW()
        {
            var crossSection = new CrossSectionDefinitionZW();
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);
            crossSection.ZWDataTable.AddCrossSectionZWRow(1, 1, 1);
            crossSection.ZWDataTable.AddCrossSectionZWRow(2, 3, 1);
            crossSection.ZWDataTable.AddCrossSectionZWRow(3, 5, 1);
            crossSection.ZWDataTable.AddCrossSectionZWRow(4, 8, 1);
            crossSection.ZWDataTable.AddCrossSectionZWRow(5, 13, 1);
            crossSection.ZWDataTable.AddCrossSectionZWRow(6, 17, 1);
            return crossSection;
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Integration)]
        public void ShowWithYZCrossSectionWithRoughnessSections()
        {
            var mainType = new CrossSectionSectionType {Name = "Main"};
            var fp1Type = new CrossSectionSectionType { Name = "FloodPlain1" };
            var fp2Type = new CrossSectionSectionType { Name = "FloodPlain2" };

            var network = NetworkEditorTestHelper.CreateDemoNetwork(mainType);
            network.CrossSectionSectionTypes.Add(mainType);
            network.CrossSectionSectionTypes.Add(fp1Type);
            network.CrossSectionSectionTypes.Add(fp2Type);

            var crossSectionDefinitionYZ = new CrossSectionDefinitionYZ();
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(0, 8);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(1, 6);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(2, 4);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(3, 3);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(4, 4);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(5, 6);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(6, 8);

            CrossSectionHelper.SetDefaultThalweg(crossSectionDefinitionYZ);
            
            crossSectionDefinitionYZ.AddSection(fp1Type, 2);
            crossSectionDefinitionYZ.AddSection(mainType, 8);
            crossSectionDefinitionYZ.AddSection(fp2Type, 2);

            var crossSectionView = new CrossSectionDefinitionView
            {
                Data = crossSectionDefinitionYZ,
                ViewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSectionDefinitionYZ,network)
            };
            
            WindowsFormsTestHelper.ShowModal(crossSectionView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Integration)]
        public void ShowWithCrossSectionZWWithRoughnessSections()
        {
            var mainType = new CrossSectionSectionType { Name = "Main" };
            var fp1Type = new CrossSectionSectionType { Name = "FloodPlain1" };
            var fp2Type = new CrossSectionSectionType { Name = "FloodPlain2" };

            var network = NetworkEditorTestHelper.CreateDemoNetwork(mainType);
            network.CrossSectionSectionTypes.Add(mainType);
            network.CrossSectionSectionTypes.Add(fp1Type);
            network.CrossSectionSectionTypes.Add(fp2Type);

            var crossSection = new CrossSectionDefinitionZW { Name = "crs1"};
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);
            crossSection.ZWDataTable.AddCrossSectionZWRow(1, 1, 1);
            crossSection.ZWDataTable.AddCrossSectionZWRow(2, 3, 1);
            crossSection.ZWDataTable.AddCrossSectionZWRow(3, 5, 1);
            crossSection.ZWDataTable.AddCrossSectionZWRow(4, 8, 1);
            crossSection.ZWDataTable.AddCrossSectionZWRow(5, 13, 1);
            crossSection.ZWDataTable.AddCrossSectionZWRow(7, 17, 1);

            crossSection.AddSection(mainType, 10.0);
            crossSection.AddSection(fp1Type, 2.0);
            crossSection.AddSection(fp2Type, 5.0);

            var crossSectionView = new CrossSectionDefinitionView
            {
                Data = crossSection,
                ViewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection,network)
            };

            WindowsFormsTestHelper.ShowModal(crossSectionView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Integration)]
        public void ShowHistory()
        {
            var mainType = new CrossSectionSectionType { Name = "Main" };

            var network = NetworkEditorTestHelper.CreateDemoNetwork(mainType);
            network.CrossSectionSectionTypes.Add(mainType);

            var crossSection1 = new CrossSectionDefinitionYZ {Name = "maas1", Thalweg = 5};
            crossSection1.YZDataTable.AddCrossSectionYZRow(0, 4);
            crossSection1.YZDataTable.AddCrossSectionYZRow(5, 0);
            crossSection1.YZDataTable.AddCrossSectionYZRow(10, 4);

            var crossSection2 = new CrossSectionDefinitionYZ { Name = "maas2", Thalweg = 0};
            crossSection2.YZDataTable.AddCrossSectionYZRow(0, 5);
            crossSection2.YZDataTable.AddCrossSectionYZRow(5, 0);
            crossSection2.YZDataTable.AddCrossSectionYZRow(10, 5);

            var crossSection3 = new CrossSectionDefinitionYZ { Name = "maas3", Thalweg = 5 };
            crossSection3.YZDataTable.AddCrossSectionYZRow(0, 6);
            crossSection3.YZDataTable.AddCrossSectionYZRow(5, 0);
            crossSection3.YZDataTable.AddCrossSectionYZRow(10, 6);

            var crossSectionSection = new CrossSectionSection { MinY = 0, MaxY = 17, SectionType = mainType };
            
            crossSection1.Sections.Add(crossSectionSection);
            crossSection2.Sections.Add(crossSectionSection);
            crossSection3.Sections.Add(crossSectionSection);

            var crossSectionView = new CrossSectionDefinitionView
            {
                HistoryToolEnabled = true,
                Data = crossSection1,
                ViewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection1, network)
            };

            crossSectionView.Data = crossSection2;
            crossSectionView.ViewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection2, network);

            crossSectionView.Data = crossSection3;
            crossSectionView.ViewModel = CrossSectionDefinitionViewModelProvider.GetViewModel(crossSection3, network);
            
            WindowsFormsTestHelper.ShowModal(crossSectionView);
        }
    }
}
