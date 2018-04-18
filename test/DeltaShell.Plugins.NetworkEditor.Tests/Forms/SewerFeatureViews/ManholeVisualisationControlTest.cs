using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NUnit.Framework;
using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
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

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void GivenManhole_SimpleNetworkTest()
        {
            var view = new ManholeVisualisationControl();

            var manhole = SetUpSimpleSewerNetwork();
            view.Manhole = manhole;

            WpfTestHelper.ShowModal(view);
        }

        private static Manhole SetUpSimpleSewerNetwork()
        {
            var manhole = new Manhole("manhole 1");
            var network = new HydroNetwork();

            manhole.Network = network;

            var compartment1 = new Compartment("Compartment 1") { SurfaceLevel = 1, BottomLevel = -2, ManholeWidth = 2.5 };
            var compartment2 = new Compartment("Compartment 2") { SurfaceLevel = 1.5, BottomLevel = -1, ManholeWidth = 2.5 };
            
            manhole.Compartments.AddRange(new List<Compartment>
            {
                compartment1,
                compartment2,
            });
            return manhole;
        }

        private static Manhole SetUpSewerNetwork()
        {
            var manhole = new Manhole("manhole 1");
            var network = new HydroNetwork();

            manhole.Network = network;

            var compartment1 = new Compartment("Compartment 1") { SurfaceLevel = 1, BottomLevel = -2, ManholeWidth = 2.500 };
            var compartment2 = new Compartment("Compartment 2") { SurfaceLevel = 1.5, BottomLevel = -1.5, ManholeWidth = 2.500 };
            var compartment3 = new Compartment("Compartment 3") { SurfaceLevel = 2, BottomLevel = -1.5, ManholeWidth = 2.500 };
            var compartment4 = new Compartment("Compartment 4") { SurfaceLevel = 2.5, BottomLevel = -1.5, ManholeWidth = 2.500 };
            var compartment5 = new Compartment("Compartment 5") { SurfaceLevel = 2.5, BottomLevel = -1.5, ManholeWidth = 2.500 };

            var pumpConnection = new SewerConnection
            {
                Name = "Pump connection",
                SourceCompartment = compartment2,
                TargetCompartment = compartment3,
                Source = manhole,
                Target = manhole,
            };
            pumpConnection.BranchFeatures.Add(new Pump { StartSuction = 1.3, StopSuction = -0.8, StartDelivery = 1, StopDelivery = -1, Name = "Pump 01" });

            var pumpConnection2 = new SewerConnection
            {
                Name = "Pump connection2",
                SourceCompartment = compartment3,
                TargetCompartment = compartment4,
                Source = manhole,
                Target = manhole,
            };
            pumpConnection2.BranchFeatures.Add(new Pump { StartSuction = 1.8, StopSuction = .5, StartDelivery = 1.3, StopDelivery = -1, Name = "Pump 02"});

            var weirConnection = new SewerConnection
            {
                Name = "Weir connection",
                SourceCompartment = compartment4,
                TargetCompartment = compartment5,
                Source = manhole,
                Target = manhole,
            };
            weirConnection.BranchFeatures.Add(new Weir { CrestLevel = 1.8, Name = "Weir 01"});

            var connections = new List<ISewerConnection>
            {
                new Pipe {Name = "leiding 1", SourceCompartment = compartment1, LevelSource = 0, SewerProfileDefinition = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeRectangle {Width = 1, Height = 1}) },
                new Pipe {Name = "leiding 2", TargetCompartment = compartment1, LevelTarget = 0, SewerProfileDefinition = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeRound { Diameter = 0.6 }) },
                new Pipe {Name = "leiding 3", SourceCompartment = compartment2, LevelSource = -1.5, SewerProfileDefinition = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeRectangle {Width = 1, Height = 1}) },
                new SewerConnectionOrifice
                {
                    SourceCompartment = compartment1,
                    Source = manhole,
                    TargetCompartment = compartment2,
                    Target = manhole,
                    Bottom_Level = -1
                },
                pumpConnection,
                pumpConnection2,
                weirConnection
            };

            network.Branches.AddRange(connections);

            manhole.Compartments.AddRange(new List<Compartment>
            {
                compartment1,
                compartment2,
                compartment3,
                compartment4,
                compartment5,
            });
            return manhole;
        }
    }
}