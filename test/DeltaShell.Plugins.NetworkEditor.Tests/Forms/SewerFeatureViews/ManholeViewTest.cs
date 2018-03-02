using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture]
    public class ManholeViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Test()
        {
            var view = new ManholeView();

            var compartment1 = new Compartment("Compartment 1");
            var compartment2 = new Compartment("Compartment 2");

            var network = new HydroNetwork();
            var pipes = new List<IPipe>
            {
                new Pipe { Name = "leiding 1", SourceCompartment = compartment1},
                new Pipe { Name = "leiding 2", TargetCompartment = compartment1},
                new Pipe { Name = "leiding 3", SourceCompartment = compartment2},
            };

            network.Pipes = pipes;

            var manhole = new Manhole("manhole 1")
            {
                Network = network
            };


            manhole.Compartments.AddRange(new List<Compartment>
            {
                compartment1,
                compartment2,
            });

            view.Data = manhole;
            WpfTestHelper.ShowModal(view);
        }
    }
}