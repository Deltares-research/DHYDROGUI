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

            var manhole = new Manhole("manhole 1");

            var compartment1 = new Compartment("Compartment 1") { SurfaceLevel = 1, BottomLevel = -2, ManholeWidth = 5000 };
            var compartment2 = new Compartment("Compartment 2") { SurfaceLevel = 3, BottomLevel = -1, ManholeWidth = 3000 };
            
            var network = new HydroNetwork();
            var orifice = new SewerConnectionOrifice
            {
                SourceCompartment = compartment1,
                Source = manhole,
                TargetCompartment = compartment2,
                Target = manhole,
            };

            var connections = new List<ISewerConnection>
            {
                new Pipe {Name = "leiding 1", SourceCompartment = compartment1, LevelSource = 0.8},
                new Pipe {Name = "leiding 2", TargetCompartment = compartment1, LevelTarget = 0.25},
                new Pipe {Name = "leiding 3", SourceCompartment = compartment2, LevelSource = -1.2},
                orifice,
            };
            
            network.SewerConnections = connections;
            manhole.Network = network;


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