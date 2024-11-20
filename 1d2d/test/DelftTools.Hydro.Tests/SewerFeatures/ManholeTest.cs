using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.SewerFeatures
{
    [TestFixture]
    public class ManholeTest
    {
        private static string compartmentName1 = "myName1";
        private static string CompartmentName2 = "myName2";

        private EventedList<ICompartment> GetCompartmentList()
        {
            var compartment1 = new Compartment(compartmentName1);
            var compartment2 = new Compartment(CompartmentName2);
            return new EventedList<ICompartment> {compartment1, compartment2};
        }

        #region Manhole geometry

        [Test]
        public void GivenManholeWithTwoCompartmentsWithoutGeometry_WhenGettingTheGeometry_ThenDefaultCoordinateIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.IsTrue(manhole.Geometry.IsValid);
        }

        [Test]
        public void GivenManholeWithoutCompartments_WhenGettingGeometry_ThenDefaultGeometryIsReturned()
        {
            var manhole = new Manhole("myManhole");
            Assert.IsTrue(manhole.Geometry.IsValid);
            Assert.That(manhole.Geometry, Is.EqualTo(new Point(0, 0)));
        }

        [Test]
        public void GivenManhole_WhenSettingCompartmentsWithGeometries_ThenGeometryIsEqualToAverageGeometryOfCompartments()
        {
            var compartmentGeometry1 = new Point(2.0, 3.0);
            var compartmentGeometry2 = new Point(-1.0, -3.0);
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<ICompartment>
                {
                    new Compartment { Geometry = compartmentGeometry1 },
                    new Compartment { Geometry = compartmentGeometry2 },
                }
            };

            var averageX = (compartmentGeometry1.X + compartmentGeometry2.X) / 2;
            var averageY = (compartmentGeometry1.Y + compartmentGeometry2.Y) / 2;
            var expectedGeometry = new Point(averageX, averageY);

            Assert.That(manhole.Geometry, Is.EqualTo(expectedGeometry));
            manhole.Compartments.ForEach(c => Assert.That(c.Geometry, Is.EqualTo(expectedGeometry)));
        }

        [Test]
        public void GivenManhole_WhenAddingCompartmentWithGeometry_ThenManholeGeometryIsEqualToCompartmentGeometry()
        {
            var compartmentGeometry = new Point(2.0, 3.0);
            var manhole = new Manhole("myManhole");
            manhole.Compartments.Add(new Compartment { Geometry = compartmentGeometry });

            Assert.That(manhole.Geometry, Is.EqualTo(compartmentGeometry));
            manhole.Compartments.ForEach(c => Assert.That(c.Geometry, Is.EqualTo(compartmentGeometry)));
        }

        [Test]
        public void GivenManholeWithOneCompartment_WhenAddingCompartmentWithGeometry_ThenGeometryIsEqualToAverageGeometryOfCompartments()
        {
            var compartmentGeometry1 = new Point(2.0, 3.0);
            var compartmentGeometry2 = new Point(-1.0, -3.0);
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<ICompartment> { new Compartment { Geometry = compartmentGeometry1 } }
            };

            manhole.Compartments.Add(new Compartment { Geometry = compartmentGeometry2 });
            var averageX = (compartmentGeometry1.X + compartmentGeometry2.X) / 2;
            var averageY = (compartmentGeometry1.Y + compartmentGeometry2.Y) / 2;
            var expectedGeometry = new Point(averageX, averageY);

            Assert.That(manhole.Geometry, Is.EqualTo(expectedGeometry));
            manhole.Compartments.ForEach(c => Assert.That(c.Geometry, Is.EqualTo(expectedGeometry)));
        }

        [Test]
        public void GivenManholeWithOneCompartment_WhenAddingCompartmentWithoutGeometry_ThenGeometryIsEqualToOriginalGeometry()
        {
            var compartmentGeometry = new Point(2.0, 3.0);
            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<ICompartment> { new Compartment { Geometry = compartmentGeometry } }
            };

            manhole.Compartments.Add(new Compartment { Geometry = null });
            Assert.That(manhole.Geometry, Is.EqualTo(compartmentGeometry));
            manhole.Compartments.ForEach(c => Assert.That(c.Geometry, Is.EqualTo(compartmentGeometry)));
        }

        [Test]
        public void GivenNewManhole_WhenGettingYCoordinate_ThenXCoordinateIsReturned()
        {
            var manhole = new Manhole("myManhole");
            Assert.IsNotNull(manhole.XCoordinate);
        }

        [Test]
        public void GivenNewManhole_WhenGettingYCoordinate_ThenYCoordinateIsReturned()
        {
            var manhole = new Manhole("myManhole");
            Assert.IsNotNull(manhole.YCoordinate);
        }

        #endregion

        [Test]
        public void GivenManhole_WhenInstatiatingWithCompartment_ThenCompartmentHasManholeAsParentManhole()
        {
            const string manholeName = "myManhole";
            var compartment1 = new Compartment(compartmentName1);
            var manhole = new Manhole(manholeName)
            {
                Compartments = new EventedList<ICompartment> { compartment1 }
            };

            Assert.NotNull(manhole.Compartments.FirstOrDefault()?.ParentManhole);
            Assert.That(manhole.Compartments.FirstOrDefault()?.ParentManhole.Name, Is.EqualTo(manholeName));
        }

        [Test]
        public void GivenManhole_WhenAddingCompartment_ThenCompartmentHasManholeAsParentManhole()
        {
            const string manholeName = "myManhole";
            var compartment1 = new Compartment(compartmentName1);
            var manhole = new Manhole(manholeName);
            manhole.Compartments.Add(compartment1);

            Assert.NotNull(manhole.Compartments.FirstOrDefault()?.ParentManhole);
            Assert.That(manhole.Compartments.FirstOrDefault()?.ParentManhole.Name, Is.EqualTo(manholeName));
        }

        [Test]
        public void GivenManholeWithCompartment_WhenGettingCompartmentByName_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.IsNotNull(manhole.GetCompartmentByName(compartmentName1));
            Assert.IsNotNull(manhole.GetCompartmentByName(CompartmentName2));
        }

        [Test]
        public void GivenManholeWithCompartment_WhenGettingCompartmentByNameWithWrongName_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.IsNull(manhole.GetCompartmentByName("NonExistentCompartmentName"));
        }

        [Test]
        public void GivenManholeWithCompartment_WhenInvokingContainsCompartment_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.IsTrue(manhole.ContainsCompartmentWithName(compartmentName1));
            Assert.IsTrue(manhole.ContainsCompartmentWithName(CompartmentName2));
        }

        [Test]
        public void GivenManholeWithCompartment_WhenInvokingContainsCompartmentWithWrongName_ThenCompartmentIsReturned()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };
            Assert.IsFalse(manhole.ContainsCompartmentWithName("NonExistentCompartmentName"));
        }

        [Test]
        public void GivenManholeWithCompartment_RemovingCompartment_UpdatesListOfCompartments()
        {
            var manhole = new Manhole("myManhole")
            {
                Compartments = GetCompartmentList()
            };

            Assert.IsTrue(manhole.ContainsCompartmentWithName(compartmentName1));

            var compartmentToRemove = manhole.GetCompartmentByName(compartmentName1);
            manhole.Compartments.Remove(compartmentToRemove);

            Assert.IsFalse(manhole.ContainsCompartmentWithName(compartmentName1));
            Assert.AreNotEqual(compartmentToRemove.ParentManhole, manhole);
        }

        [Test] 
        public void GivenManholeWithCompartment_MovingCompartmentToNewManhole_UpdatesListOfCompartments()
        {
            var oldManholeName = "oldManhole";
            var compartmentName = "compartmentName";
            var compartmentToMove = new Compartment(compartmentName);
            var oldManhole = new Manhole(oldManholeName)
            {
                Compartments = new EventedList<ICompartment>() { compartmentToMove }
            };

            var newManholeName = "newManhole";
            var newManhole = new Manhole(newManholeName);

            Assert.IsTrue(oldManhole.ContainsCompartmentWithName(compartmentName));
            Assert.IsFalse(newManhole.ContainsCompartmentWithName(compartmentName));
            Assert.AreEqual(compartmentToMove.ParentManhole, oldManhole);

            newManhole.Compartments.Add(compartmentToMove);

            Assert.IsFalse(oldManhole.ContainsCompartmentWithName(compartmentName));
            Assert.IsTrue(newManhole.ContainsCompartmentWithName(compartmentName));
            Assert.AreEqual(compartmentToMove.ParentManhole, newManhole);
        }

        [Test]
        public void UpdateCompartmentToOutletCompartment()
        {
            var hydroNetwork = new HydroNetwork();
            var targetManhole = new Manhole("tm");
            var targetCompartment = new Compartment("tc") { SurfaceLevel = 0.0, Geometry = new Point(0, 0) };
            targetManhole.Compartments.Add(targetCompartment);

            var sourceManhole = new Manhole("sm");
            var sourceCompartment = new Compartment("sc") { SurfaceLevel = 0.0, Geometry = new Point(100, 0) };
            sourceManhole.Compartments.Add(sourceCompartment);

            var sewerConnection = new SewerConnection("pipe or buis")
            {
                SourceCompartment = sourceCompartment,
                TargetCompartment = targetCompartment,
                LevelSource = 0.0,
                LevelTarget = 0.0,
                WaterType = SewerConnectionWaterType.None,
                SourceCompartmentName = "sc",
                TargetCompartmentName = "tc"
            };
            sourceManhole.OutgoingBranches.Add(sewerConnection);
            targetManhole.IncomingBranches.Add(sewerConnection);

            hydroNetwork.Nodes.Add(sourceManhole);
            hydroNetwork.Nodes.Add(targetManhole);
            hydroNetwork.Branches.Add(sewerConnection);

            //call

            var outlet = targetManhole.UpdateCompartmentToOutletCompartment(targetCompartment);

            //check result

            Assert.AreEqual(1, targetManhole.Compartments.Count);
            Assert.AreSame(outlet, targetManhole.Compartments.FirstOrDefault());
            Assert.AreEqual(1, hydroNetwork.OutletCompartments.Count());
            Assert.AreSame(outlet, hydroNetwork.OutletCompartments.FirstOrDefault());

            Assert.AreSame(outlet, sewerConnection.TargetCompartment);

        }

        [Test]
        public void GetOutletCandidate()
        {
            var manhole = new Manhole("tm");
            var compartment = new Compartment("tc") { SurfaceLevel = 0.0, Geometry = new Point(0, 0) };
            manhole.Compartments.Add(compartment);

            var incomingSewerConnection = new SewerConnection("incoming")
            {
                TargetCompartment = compartment,//set incoming going sewerconnection
                LevelSource = 0.0,
                LevelTarget = 0.0,
                WaterType = SewerConnectionWaterType.None,
                TargetCompartmentName = "tc"
            };

            var outgoingSewerConnection = new SewerConnection("outgoing")
            {
                SourceCompartment = compartment, //set outgoing sewerconnection
                LevelSource = 0.0,
                LevelTarget = 0.0,
                WaterType = SewerConnectionWaterType.None,
                SourceCompartmentName = "sc"
            };

            Assert.IsNull(manhole.GetOutletCandidate());

            manhole.OutgoingBranches.Remove(outgoingSewerConnection); //condition outlet: at least one incoming connections, no outgoing connections

            Assert.AreSame(compartment,manhole.GetOutletCandidate());

            manhole.OutgoingBranches.Add(outgoingSewerConnection); //condition outlet: no outgoing connections

            Assert.IsNull(manhole.GetOutletCandidate());

            manhole.IncomingBranches.Remove(incomingSewerConnection); //condition outlet: at least one incoming connections, now none

            Assert.IsNull(manhole.GetOutletCandidate());
            manhole.IncomingBranches.Add(incomingSewerConnection); //condition outlet: at least one incoming connections, now none
            manhole.IncomingBranches.Add(incomingSewerConnection); //condition outlet: at least one incoming connections, now two
            manhole.OutgoingBranches.Remove(outgoingSewerConnection); //condition outlet: at least one incoming connections, no outgoing connections

            Assert.AreSame(compartment,manhole.GetOutletCandidate());

        }
    }
}