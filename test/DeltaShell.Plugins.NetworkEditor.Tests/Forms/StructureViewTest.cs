using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Controls.Swf.Table;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms
{
    [TestFixture]
    public class StructureViewTest
    {
        private static IStructureViewData GetStructureViewData(ICompositeBranchStructure structure)
        {
            return CompositeStructureViewDataBuilder.GetCompositeStructureViewDataForStructure(structure);
        }

        private static HydroNetwork HydroNetwork;
        private static IChannel Branch1;
        private static ICompositeBranchStructure CompositeBranchStructure;
        private static IWeir Weir;

        [SetUp]
        public void NetworkSetup()
        {
            HydroNetwork = new HydroNetwork();

            Branch1 = new Channel { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(20, 0) }) };
            var node1 = new HydroNode { Geometry = new Point(0, 0) };
            var node2 = new HydroNode { Geometry = new Point(20, 0) };
            HydroNetwork.Nodes.Add(node1);
            HydroNetwork.Nodes.Add(node2);

            Weir = new Weir
                       {
                           Geometry = new Point(5, 0),
                           OffsetY = 5, //150,
                           CrestWidth = 5,
                           CrestLevel = -3,
                           WeirFormula = new SimpleWeirFormula()
                       };

            CompositeBranchStructure = new CompositeBranchStructure { Geometry = new Point(5, 0), Chainage = 5 };

            NetworkHelper.AddBranchFeatureToBranch(CompositeBranchStructure, Branch1, CompositeBranchStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(CompositeBranchStructure, Weir);

            Branch1.Source = node1;
            Branch1.Target = node2;

            HydroNetwork.Branches.Add(Branch1);
        }

        private static CrossSectionDefinition AddCrossSection(double offset)
        {
            var crossSection = new CrossSectionDefinitionYZ();
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(Branch1, crossSection, offset);
            CrossSectionHelper.SetDefaultYZTableAndUpdateThalWeg(crossSection, 50);
            
            return crossSection;
        }

        ///<summary>
        /// Show structure view with weir and a cross section
        ///</summary>
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DisplayStructureViewSingleWeirWithCrossSection()
        {
            AddCrossSection(1);
            CrossSectionDefinition crossSectionDefinition = AddCrossSection(Branch1.Length - 1);
            crossSectionDefinition.ShiftLevel(5);
            var structureView = new StructureView
                                    {
                                        Dock = DockStyle.Fill,
                                        Data = GetStructureViewData(Weir.ParentStructure)
                                    };
            ((StructurePresenter) structureView.CommandReceiver).IsAddPointActive = true;
            WindowsFormsTestHelper.ShowModal(structureView, Weir);
        }

        ///<summary>
        /// Show structure view with weir and an empty cross section
        ///</summary>
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DisplayStructureViewSingleWeirWithEmptyCrossSection()
        {
            var crossSectionDef = new CrossSectionDefinitionYZ();
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(Branch1, crossSectionDef, 0);
            
            var structureView = new StructureView
            {
                Dock = DockStyle.Fill,
                Data = GetStructureViewData(Weir.ParentStructure)
            };
            ((StructurePresenter)structureView.CommandReceiver).IsAddPointActive = true;
            WindowsFormsTestHelper.ShowModal(structureView, Weir);
        }

        ///<summary>
        /// Show structure view with weir and proxied cross sections
        ///</summary>
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DisplayStructureViewSingleWeirWithProxiedCrossSection()
        {
            var crossSection = new CrossSectionDefinitionProxy(CrossSectionDefinitionYZ.CreateDefault());
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(Branch1, crossSection, 0);

            var crossSection2 = new CrossSectionDefinitionProxy(CrossSectionDefinitionYZ.CreateDefault());
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(Branch1, crossSection2, Branch1.Length);

            var structureView = new StructureView
            {
                Dock = DockStyle.Fill,
                Data = GetStructureViewData(Weir.ParentStructure)
            };
            ((StructurePresenter)structureView.CommandReceiver).IsAddPointActive = true;
            WindowsFormsTestHelper.ShowModal(structureView, Weir);
        }

        ///<summary>
        /// Show structure view with weir
        ///</summary>
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DisplayStructureViewSingleWeirWithoutCrossSection()
        {
            var structureView = new StructureView
                                    {
                                        Dock = DockStyle.Fill,
                                        Data = GetStructureViewData(Weir.ParentStructure)
                                    };
            WindowsFormsTestHelper.ShowModal(structureView, Weir);
        }

        ///<summary>
        ///Show cross section view with some default data
        ///</summary>
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DisplayStructureViewSingleWeirBridge()
        {
            AddCrossSection(10);
            Bridge bridge = new Bridge { Geometry = new Point(5, 0) };

            NetworkHelper.AddBranchFeatureToBranch(CompositeBranchStructure, Branch1, CompositeBranchStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(CompositeBranchStructure, bridge);

            var structureView = new StructureView
            {
                Dock = DockStyle.Fill,
                Data = GetStructureViewData(Weir.ParentStructure)
            };
            ((StructurePresenter)structureView.CommandReceiver).IsAddPointActive = true;
            WindowsFormsTestHelper.ShowModal(structureView, Weir);
        }

        ///<summary>
        /// Show structureview with free form weir
        ///</summary>
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DisplayStructureViewSingleFreeFormWeir()
        {
            // create a free form weir
            IWeir freeFormulaWeir = new Weir();
            FreeFormWeirFormula freeFormWeirFormula = new FreeFormWeirFormula();
            freeFormWeirFormula.SetShape(new[] {15.0, 25.0, 30.0, 35.0, 38.0, 45.0},
                                         new[] {0.0, -8.0, -5.0, 0.0, -5.0, 0.0});

            freeFormulaWeir.WeirFormula = freeFormWeirFormula;
            // add weir to existing compopsite structure
            HydroNetworkHelper.AddStructureToComposite(CompositeBranchStructure, freeFormulaWeir);

            var splitContainer = new SplitContainer {Orientation = Orientation.Vertical};

            // Create structure view
            var structureView = new StructureView
            {
                Dock = DockStyle.Fill,
                Data = GetStructureViewData(Weir.ParentStructure)
            };

            // Create a table view to bind the shape of the freeformweir
            TableView tableView = new TableView
                                      {
                                          Data = freeFormWeirFormula.Shape.Coordinates.ToList()
                                      };
            //IBindingList bindingList = new FunctionBindingList(freeFormWeirFormula.Shape);

            // Create a simple ui : toolbar with 4 command, structureview (chart) and tableview
            ToolStripButton buttonSelect = new ToolStripButton {Text = "select"};
            buttonSelect.Click += (s, a) => { ((ICanvasEditor) structureView.CommandReceiver).IsSelectItemActive = true; };
            ToolStripButton buttonMove = new ToolStripButton {Text = "move"};
            buttonMove.Click += (s, a) => { ((ICanvasEditor) structureView.CommandReceiver).IsMoveItemActive = true; };
            ToolStripButton buttonInsert = new ToolStripButton {Text = "insert"};
            buttonInsert.Click += (s, a) => { ((ICanvasEditor) structureView.CommandReceiver).IsAddPointActive = true; };
            ToolStripButton buttonDelete = new ToolStripButton {Text = "delete"};
            buttonDelete.Click += (s, a) => { ((ICanvasEditor) structureView.CommandReceiver).IsDeleteItemActive = true; };
            ToolStrip toolStrip = new ToolStrip();
            toolStrip.Items.AddRange(new ToolStripItem[]
                                         {
                                             buttonSelect,
                                             buttonMove,
                                             buttonInsert,
                                             buttonDelete
                                         });

            Panel panel = new Panel {Dock = DockStyle.Fill};
            toolStrip.Anchor = AnchorStyles.Top;
            structureView.Dock = DockStyle.Fill;
            panel.Controls.Add(toolStrip);
            panel.Controls.Add(structureView);
            splitContainer.Panel1.Controls.Add(panel);
            splitContainer.Panel2.Controls.Add(tableView);
            WindowsFormsTestHelper.ShowModal(splitContainer);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DisplayGatedWeir()
        {
            IWeir weir1 = new Weir {OffsetY = 30, CrestWidth = 10};
            GatedWeirFormula gatedWeirFormula = new GatedWeirFormula {GateOpening = 2.0};
            weir1.WeirFormula = gatedWeirFormula;
            // add weir to existing compopsite structure; this composite structure has 2 weirs
            HydroNetworkHelper.AddStructureToComposite(CompositeBranchStructure, weir1);

            var structureView = new StructureView
            {
                Dock = DockStyle.Fill,
                Data = GetStructureViewData(weir1.ParentStructure)
            };
            ((StructurePresenter)structureView.CommandReceiver).IsAddPointActive = true;
            WindowsFormsTestHelper.ShowModal(structureView, Weir);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DisplayStructureView7Weirs()
        {
            Weir lastWeir = null;
            for (int i=0 ; i< 6; i++)
            {
                lastWeir = new Weir
                                  {
                                      Network = HydroNetwork,
                                      Geometry = new Point(5, 0),
                                      OffsetY = 10 + (i * 5),
                                      CrestWidth = 5,
                                      CrestLevel = -4
                                  };
                Branch1.BranchFeatures.Add(lastWeir);
                HydroNetworkHelper.AddStructureToComposite(CompositeBranchStructure, lastWeir);
            }
            
            AddCrossSection(10);

            var structureView = new StructureView
            {
                Dock = DockStyle.Fill,
                Data = GetStructureViewData(lastWeir.ParentStructure)
            };
            WindowsFormsTestHelper.ShowModal(structureView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DisplayStructureView1PumpWithCrossSection()
        {
            var pump = new Pump
            {
                Network = HydroNetwork,
                Geometry = new Point(5, 0),
                OffsetY = 225,
                StartDelivery = -8,
                StopDelivery= 20,
                StopSuction = -7,
                StartSuction = -1
            };
            Branch1.BranchFeatures.Add(pump);
            HydroNetworkHelper.AddStructureToComposite(CompositeBranchStructure, pump);
            AddCrossSection(10);

            var structureView = new StructureView
            {
                Dock = DockStyle.Fill,
                Data = GetStructureViewData(pump.ParentStructure)
            };
            WindowsFormsTestHelper.ShowModal(structureView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DisplayStructureViewPumpExtraPumpCrossSection()
        {
            AddExtraStructure(new Pump
                                  {
                                      Network = HydroNetwork,
                                      Geometry = new Point(5, 0),
                                      StartDelivery = -8,
                                      StopDelivery = 20,
                                      StopSuction = -7,
                                      StartSuction = -1
                                  });

            AddExtraStructure(new Pump
                                  {
                                      Network = HydroNetwork,
                                      Geometry = new Point(5, 0),
                                      StartDelivery = -8,
                                      StopDelivery = 20,
                                      StopSuction = -7,
                                      StartSuction = -1
                                  });


            AddCrossSection(10);

            var structureView = new StructureView
            {
                Dock = DockStyle.Fill,
                Data = GetStructureViewData(CompositeBranchStructure)
            };
            WindowsFormsTestHelper.ShowModal(structureView);
        }

        private void AddExtraStructure(IStructure1D structure)
        {
            Branch1.BranchFeatures.Add(structure);
            HydroNetworkHelper.AddStructureToComposite(CompositeBranchStructure, structure);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DisplayStructureView1PumpWithoutCrossSection()
        {
            var compositeBranchStructure2 = new CompositeBranchStructure("dd", 10);
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure2, Branch1, compositeBranchStructure2.Chainage);

            for (int i = 0; i < 2; i++)
            {
                var pump = new Pump(string.Format("Pump{0}", i));
                Branch1.BranchFeatures.Add(pump);

                HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure2, pump);
            }
            
            var structureView = new StructureView
            {
                Dock = DockStyle.Fill,
                Data = GetStructureViewData(compositeBranchStructure2)
            };

            WindowsFormsTestHelper.ShowModal(structureView);
        }
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void StructureViewUpdatesWhenPumpChanges()
        {
            var pump = new Pump("test") {OffsetY = 50, StopDelivery = -5.0};
            Branch1.BranchFeatures.Add(pump);
            var compositeBranchStructure2 = new CompositeBranchStructure("dd", 50);

            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure2, Branch1, compositeBranchStructure2.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure2, pump);
            AddCrossSection(10);
            var structureView = new StructureView
            {
                Dock = DockStyle.Fill,
                Data = GetStructureViewData(Weir.ParentStructure)
            };

            WindowsFormsTestHelper.Show(structureView);
            for (int i = 0; i < 2; i++)
            {
                Thread.Sleep(50);
                pump.StopDelivery+= 0.1;
                if (i % 3 == 0)
                {
                    pump.DirectionIsPositive = !pump.DirectionIsPositive;
                }
                Application.DoEvents();
            }
            for (int i = 0; i < 2; i++)
            {
                Thread.Sleep(50);
                pump.StopDelivery -= 0.2;
                if (i % 3 == 0)
                {
                    pump.DirectionIsPositive = !pump.DirectionIsPositive;
                }
                Application.DoEvents();
            }

            WindowsFormsTestHelper.CloseAll();
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void AddBridgeToOpenView()
        {
            AddCrossSection(10);
            NetworkHelper.AddBranchFeatureToBranch(CompositeBranchStructure, Branch1, CompositeBranchStructure.Chainage);

            var structureView = new StructureView
            {
                Dock = DockStyle.Fill,
                Data = GetStructureViewData(Weir.ParentStructure)
            };
            
            WindowsFormsTestHelper.Show(structureView);

            Bridge bridge = new Bridge { Geometry = new Point(5, 0) };

            //add the bridge to the network first and then to the composite structure
            //this mimics behaviour in the loader.
            Branch1.BranchFeatures.Add(bridge);
            bridge.Branch = Branch1;
            bridge.ParentStructure = CompositeBranchStructure;
            bridge.Chainage = CompositeBranchStructure.Chainage;
            CompositeBranchStructure.Structures.Add(bridge);


            //HydroNetworkHelper.AddStructureToComposite(CompositeBranchStructure, bridge);

            WindowsFormsTestHelper.CloseAll();
        }

        [Test]
        public void ShiftCrossSectionDefinitionShouldUpdateStructureView()
        {
            var crossSectionDefinition=AddCrossSection(10);
            var structureView = new StructureView()
                                    {
                                        Dock = DockStyle.Fill,
                                        Data = GetStructureViewData(Weir.ParentStructure)
                                    };
            var crossSectionDefinitionSeries = TypeUtils.GetField<StructureView,ILineChartSeries>(structureView,"crossSectionDefinitionSeries");
            
            var minYBefore = crossSectionDefinitionSeries.MinYValue();
            const double shift = 10;
            crossSectionDefinition.ShiftLevel(shift);
            var minYAfter = crossSectionDefinitionSeries.MinYValue();
            
            Assert.AreEqual(minYBefore + shift, minYAfter);
        }

        // The next test is dubious. It depends too much on the internal implementation of StructureView
 /*         [Test]
        public void MoveWeirInCrossSection()
        {
          var secondWeir = new Weir
                                  {
                                      HydroNetwork = HydroNetwork,
                                      Geometry = new Point(5, 0),
                                      OffsetY = 225,
                                      CrestWidth = 75,
                                      CrestLevel = -4
                                  };
            branch1.BranchFeatures.Add(secondWeir);
            CompositeBranchStructure.Structures.Add(secondWeir);
            AddCrossSection(10);

            var structureView = new StructureView()
            {
                Dock = DockStyle.Fill,
                Data = weir.ParentStructure
            };

            IChartView chartView = structureView.ChartView;
            IEnumerable<IChartViewTool> shapeModifyTools = chartView.Tools.Where(t => t is ShapeModifyTool);
            
            var shapeModifyTool = (ShapeModifyTool) shapeModifyTools.Where(smt => ((ShapeModifyTool) smt).ShapeFeatures.Count > 1).First();
            var rectangleShapeFeature = (RectangleShapeFeature)shapeModifyTool.ShapeFeatures[0];
            
            Assert.AreEqual(rectangleShapeFeature.Width, weir.CrestWidth);
            weir.CrestWidth += 10;
            Assert.AreEqual(rectangleShapeFeature.Width, weir.CrestWidth);
            // to test the reverse this should be done via the form view; the following doesn't work (as expected)
            // rectangleShapeFeature.Right += 10;
            // Assert.AreEqual(rectangleShapeFeature.Width, weir.CrestWidth);
        }*/
    }
}
