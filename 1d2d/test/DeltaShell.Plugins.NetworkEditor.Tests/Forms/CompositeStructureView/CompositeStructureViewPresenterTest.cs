using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMap.Converters.WellKnownText;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CompositeStructureView
{
    [TestFixture]
    public class CompositeStructureViewPresenterTest
    {
        private ICompositeStructureView view; 
        private CompositeStructureViewPresenter presenter;
        // private readonly IGui gui = new MockGui();

        readonly List<IStructure1D> structures = new List<IStructure1D>
                                              {
                                                  new Pump("pump1") { OffsetY = 150 },
                                                  new Weir("Weir1") { CrestLevel = 15 }
                 
                                              };
        // In progress

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            presenter = new CompositeStructureViewPresenter();            
            view = new MockCompositeStructureView(presenter);
            view.Data = CreateBrancheStructureWithPumpAndWeir(structures);
            presenter.View = view;
        }

        [Test]
        public void PresenterShouldRespondToGuiSelectedObjectChanges()
        {
//            gui.Selection = structures[0];
//            Assert.AreSame(presenter.View.SelectedStructure, gui.Selection);
        }

        [Test]
        public void PresenterShouldChangeGuiSelection()
        {            
//            view.ActivateFormView(structures[0]);
//            Assert.AreSame(gui.Selection, presenter.View.SelectedStructure);
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {

        }

        private static ICompositeBranchStructure CreateBrancheStructureWithPumpAndWeir(IEnumerable<IStructure1D> structures)
        {
            // create network
            var network = new HydroNetwork();

            var node1 = new HydroNode("node1");
            var node2 = new HydroNode("node2");

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Channel("branch1", node1, node2)
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")
            };

            network.Branches.Add(branch1);

            var compositeBranchStructure = new CompositeBranchStructure();

            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);

            foreach (var structure in structures)
            {
                HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, structure);
            }           

            return compositeBranchStructure;
        }
    }
}