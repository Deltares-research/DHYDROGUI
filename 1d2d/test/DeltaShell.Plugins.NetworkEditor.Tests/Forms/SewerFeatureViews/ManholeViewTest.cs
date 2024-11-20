using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ManholeViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DisplayViewWithoutErrors()
        {
            var view = new ManholeView();

            var manhole = CreateManhole();
            view.Data = manhole;
            WpfTestHelper.ShowModal(view);
        }

        [Test]
        public void TestReorderingOfShapes()
        {
            var manhole = CreateManhole();
            var shapes = manhole.CreateShapes().ToList();

            shapes.OrderShapes();
        }

        private static Manhole CreateManhole()
        {
            var manhole = new Manhole("manhole 1");

            var compartment1 = new Compartment("Compartment 1") {SurfaceLevel = 1, BottomLevel = -3, ManholeWidth = 2.5};
            var compartment2 = new Compartment("Compartment 2") {SurfaceLevel = 1, BottomLevel = -2, ManholeWidth = 2.5};
            var compartment3 = new Compartment("Compartment 3") {SurfaceLevel = 1, BottomLevel = -1, ManholeWidth = 2.5};
            var compartment4 = new Compartment("Compartment 4") {SurfaceLevel = 1, BottomLevel = -2, ManholeWidth = 2.5};

            manhole.Compartments.AddRange(new List<Compartment>
            {
                compartment1,
                compartment2,
                compartment3,
                compartment4
            });

            var network = new HydroNetwork();
            network.Nodes.AddRange(new List<IManhole> { manhole });
            manhole.Network = network;

            var orificeConnection = new SewerConnection
            {
                SourceCompartment = compartment1,
                Source = manhole,
                TargetCompartment = compartment2,
                Target = manhole,
            };
            var orifice = new Orifice();
            orificeConnection.AddStructureToBranch(orifice);
            
            var pumpConnection = new SewerConnection
            {
                Name = "Interne verbinding",
                SourceCompartment = compartment2,
                TargetCompartment = compartment3,
                Source = manhole,
                Target = manhole,
            };
            pumpConnection.AddStructureToBranch(new Pump {StartSuction = 1.3, StopSuction = -0.8, StartDelivery = 1, StopDelivery = -1, Name = "Pump 01"});

            var weirConnection = new SewerConnection
            {
                SourceCompartment = compartment3,
                TargetCompartment = compartment4,
                Source = manhole,
                Target = manhole,
            };
            weirConnection.AddStructureToBranch(new Weir {CrestLevel = 0.23});

            var connections = new List<ISewerConnection>
            {
                new Pipe {Name = "leiding 1", SourceCompartment = compartment3, Source = manhole, LevelSource = 0.5, CrossSection = new CrossSection(new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle {Diameter = 1}))},
                new Pipe {Name = "leiding 2", TargetCompartment = compartment2, Target = manhole, LevelTarget = 0.25, CrossSection = new CrossSection(new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle {Diameter = 1}))},
                new Pipe {Name = "leiding 3", SourceCompartment = compartment4, Source = manhole, LevelSource = -1.2, CrossSection = new CrossSection(new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle {Diameter = 1}))},
                pumpConnection,
                orificeConnection,
                weirConnection
            };

            network.Branches.AddRange(connections);
            
            return manhole;
        }
    }
}