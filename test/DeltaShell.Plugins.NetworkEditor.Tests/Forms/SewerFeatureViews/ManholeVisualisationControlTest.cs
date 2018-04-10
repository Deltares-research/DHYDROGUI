using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NUnit.Framework;
using System.Collections.Generic;
using DelftTools.TestUtils;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture]
    public class ManholeVisualisationControlTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void GivenManhole_WhenOpeningView_ThenOpenWithoutError()
        {
            var view = new ManholeVisualisationControl();

            var manhole = SetUpSewerNetwork();

            view.Manhole = manhole;

            WpfTestHelper.ShowModal(view);
        }

        public static Manhole SetUpSewerNetwork()
        {
            var manhole = new Manhole("manhole 1");
            var network = new HydroNetwork();

            manhole.Network = network;

            var compartment1 = new Compartment("Compartment 1") {SurfaceLevel = 1, BottomLevel = -2, ManholeWidth = 2500};
            var compartment2 = new Compartment("Compartment 2") {SurfaceLevel = 3, BottomLevel = -1.5, ManholeWidth = 2500};
            var compartment3 = new Compartment("Compartment 3") {SurfaceLevel = 3, BottomLevel = -1.5, ManholeWidth = 2500};
            var compartment4 = new Compartment("Compartment 4") {SurfaceLevel = 3, BottomLevel = -1.5, ManholeWidth = 2500};

            var pumpConnection = new SewerConnection
            {
                Name = "Pump connection",
                SourceCompartment = compartment2,
                TargetCompartment = compartment3,
                Source = manhole,
                Target = manhole,
            };

            var pumpConnection2 = new SewerConnection
            {
                Name = "Pump connection2",
                SourceCompartment = compartment3,
                TargetCompartment = compartment4,
                Source = manhole,
                Target = manhole,
            };
            pumpConnection.BranchFeatures.Add(
                new Pump {StartSuction = 2, StopSuction = 0, StartDelivery = 1, StopDelivery = -1});
            pumpConnection2.BranchFeatures.Add(new Pump
            {
                StartSuction = 2.5,
                StopSuction = .5,
                StartDelivery = 1.3,
                StopDelivery = -1.4
            });

            var connections = new List<ISewerConnection>
            {
                new Pipe {Name = "leiding 1", SourceCompartment = compartment1, LevelSource = 0.8},
                new Pipe {Name = "leiding 2", TargetCompartment = compartment1, LevelTarget = 0.25},
                new Pipe {Name = "leiding 3", SourceCompartment = compartment2, LevelSource = -1.5},
                new SewerConnectionOrifice
                {
                    SourceCompartment = compartment1,
                    Source = manhole,
                    TargetCompartment = compartment2,
                    Target = manhole,
                    Bottom_Level = 0
                },
                pumpConnection,
                pumpConnection2,
            };

            network.Branches.AddRange(connections);

            manhole.Compartments.AddRange(new List<Compartment>
            {
                compartment1,
                compartment2,
                compartment3,
                compartment4,
            });
            return manhole;
        }
    }
}