using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffDimrCouplingTest
    {
        [Test]
        [TestCaseSource(nameof(ArgNullCases))]
        public void GivenNull_WhenConstructing_ThrowsArgumentNullException(IDrainageBasin basin, Dictionary<string, SobekRRLink[]> lateralToLinkableObjects)
        {
            // Call
            void Call() => _ = new RainfallRunoffDimrCoupling(basin, lateralToLinkableObjects);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenCouplingWithTargetAndSourceWhichCanLink_WhenCreateLink_ThenLinkAndReturnHydroLink()
        {
            //Arrange
            var basin = Substitute.For<IDrainageBasin>();
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();

            var target = Substitute.For<IHydroObject>();
            target.Name.Returns("target");

            var source = Substitute.For<IHydroObject>();
            source.Name.Returns("source");
            source.CanLinkTo(target).Returns(true);

            var coupling = new RainfallRunoffDimrCoupling(basin, lateralToLinkableObjects);

            //Act
            coupling.CreateLink(source, target);

            //Assert
            source.Received(1).CanLinkTo(target);
            source.Received(1).LinkTo(target);
        }

        [Test]
        public void GivenCouplingWithTargetAndSourceWhichCanNotLink_WhenCreateLink_ThenDoNotLinkAndReturnNull()
        {
            //Arrange
            var basin = Substitute.For<IDrainageBasin>();
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();

            var target = Substitute.For<IHydroObject>();
            target.Name.Returns("target");

            var source = Substitute.For<IHydroObject>();
            source.Name.Returns("source");
            source.CanLinkTo(target).Returns(false);

            var coupling = new RainfallRunoffDimrCoupling(basin, lateralToLinkableObjects);

            //Act
            coupling.CreateLink(source, target);

            //Assert
            source.Received(1).CanLinkTo(target);
            source.Received(0).LinkTo(target);
        }

        [Test]
        [TestCaseSource(nameof(ArgNullCasesCreateLink))]
        public void GivenTargetOrSourceNull_WhenCreateLink_ThrowsArgumentNullException(IHydroObject source, IHydroObject target)
        {
            //Arrange
            var basin = Substitute.For<IDrainageBasin>();
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();
            var coupling = new RainfallRunoffDimrCoupling(basin, lateralToLinkableObjects);

            // Call
            void Call() => coupling.CreateLink(source, target);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenCatchment_WhenCanLink_ReturnsTrue()
        {
            //Arrange
            var basin = Substitute.For<IDrainageBasin>();
            var lateralToCatchmentLookup = new Dictionary<string, SobekRRLink[]>();
            var coupling = new RainfallRunoffDimrCoupling(basin, lateralToCatchmentLookup);
            var catchment = new Catchment();

            // Call
            bool canLink = coupling.CanLink(catchment);

            // Assert
            Assert.That(canLink, Is.True);
        }

        [Test]
        public void GivenWasteWaterTreatmentPlant_WhenCanLink_ReturnsTrue()
        {
            //Arrange
            var basin = Substitute.For<IDrainageBasin>();
            var lateralToCatchmentLookup = new Dictionary<string, SobekRRLink[]>();
            var coupling = new RainfallRunoffDimrCoupling(basin, lateralToCatchmentLookup);
            var wwtp = new WasteWaterTreatmentPlant();

            // Call
            bool canLink = coupling.CanLink(wwtp);

            // Assert
            Assert.That(canLink, Is.True);
        }

        [Test]
        public void GivenNoCatchment_WhenCanLink_ReturnsFalse()
        {
            //Arrange
            var basin = Substitute.For<IDrainageBasin>();
            var catchment = Substitute.For<IHydroObject>();
            var lateralToCatchmentLookup = new Dictionary<string, SobekRRLink[]>();

            var coupling = new RainfallRunoffDimrCoupling(basin, lateralToCatchmentLookup);

            // Call
            bool canLink = coupling.CanLink(catchment);

            // Assert
            Assert.That(canLink, Is.False);
        }

        [Test]
        [TestCase("name")]
        [TestCase("name/as")]
        [TestCase("name/as/something/else")]
        public void GivenStringWithNotThreeSlashes_WhenGetLinkHydroObjectsByItemString_ThenThrowArgumentException(string stringWithNotThreeSlashes)
        {
            //Arrange
            var basin = Substitute.For<IDrainageBasin>();
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();
            
            var coupling = new RainfallRunoffDimrCoupling(basin, lateralToLinkableObjects);

            // Call
            void Call() => coupling.GetLinkHydroObjectsByItemString(stringWithNotThreeSlashes);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenStringWithThreeSlashesButNotNameCatchments_WhenGetLinkHydroObjectsByItemString_ThenReturnEmptyList()
        {
            //Arrange
            const string itemString = "name/as/something";
            
            var basin = Substitute.For<IDrainageBasin>();
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();
            
            var coupling = new RainfallRunoffDimrCoupling(basin, lateralToLinkableObjects);

            // Call
            IList<IHydroObject> items = coupling.GetLinkHydroObjectsByItemString(itemString).ToList();

            // Assert
            Assert.That(items, Is.Empty);
        }

        [Test]
        public void GivenStringWithThreeSlashesAndCategoryAsCatchmentsAndCatchment_WhenGetLinkHydroObjectsByItemString_ThenReturnListContainingGivenCatchment()
        {
            //Arrange
            const string itemString = "catchments/as/name";
            
            var basin = Substitute.For<IDrainageBasin>();
            var catchment = new Catchment { Name = "as" };
            basin.AllCatchments.Returns(new[] { catchment });

            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();

            var coupling = new RainfallRunoffDimrCoupling(basin, lateralToLinkableObjects);
            coupling.Prepare();

            // Call
            IList<IHydroObject> items = coupling.GetLinkHydroObjectsByItemString(itemString).ToList();

            // Assert
            Assert.That(items.Contains(catchment), Is.True);
        }

        [Test]
        public void GivenCatchmentWhichIsLinkedToRunOffBoundaries_WhenGetLinkHydroObjectsByItemString_ThenReturnListContainingGivenCatchment()
        {
            //Arrange
            const string itemString = "catchments/as/name";

            var basin = Substitute.For<IDrainageBasin>();
            var catchment = new Catchment { Name = "Marlon" };
            basin.AllCatchments.Returns(new[] { catchment });

            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>
            {
                ["as"] = new SobekRRLink[]
                {
                    new SobekRRLink { NodeFromId = "Marlon" }
                }
            };
            
            var coupling = new RainfallRunoffDimrCoupling(basin, lateralToLinkableObjects);
            coupling.Prepare();

            // Call
            IList<IHydroObject> items = coupling.GetLinkHydroObjectsByItemString(itemString).ToList();

            // Assert
            Assert.That(items.Contains(catchment), Is.True);
        }

        [Test]
        public void GivenWasteWaterTreatmentPlantWhichIsLinkedToRunOffBoundary_WhenGetLinkHydroObjectsByItemString_ReturnsGivenWasteWaterTreatmentPlant()
        {
            //Arrange
            const string itemString = "catchments/as/name";
            
            var basin = Substitute.For<IDrainageBasin>();
            var wwtp = new WasteWaterTreatmentPlant { Name = "Marlon" };
            basin.WasteWaterTreatmentPlants.Returns(new EventedList<WasteWaterTreatmentPlant>(new[] { wwtp }));
            
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>
            {
                ["as"] = new SobekRRLink[]
                {
                    new SobekRRLink { NodeFromId = "Marlon" }
                }
            };
            
            var coupling = new RainfallRunoffDimrCoupling(basin, lateralToLinkableObjects);
            coupling.Prepare();

            // Call
            IList<IHydroObject> items = coupling.GetLinkHydroObjectsByItemString(itemString).ToList();

            // Assert
            Assert.That(items.Contains(wwtp), Is.True);
        }

        private static IEnumerable<TestCaseData> ArgNullCases()
        {
            yield return new TestCaseData(Substitute.For<IDrainageBasin>(), null);
            yield return new TestCaseData(null, new Dictionary<string, SobekRRLink[]>());
        }

        private static IEnumerable<TestCaseData> ArgNullCasesCreateLink()
        {
            yield return new TestCaseData(Substitute.For<IHydroObject>(), null);
            yield return new TestCaseData(null, Substitute.For<IHydroObject>());
        }
    }
}