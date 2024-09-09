using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffHydroModelCouplingTest
    {
        [Test]
        [TestCaseSource(nameof(ArgNullCases))]
        public void GivenNull_WhenConstructing_ThrowsArgumentNullException(IDrainageBasin basin, Dictionary<string, SobekRRLink[]> lateralToLinkableObjects)
        {
            // Call
            void Call() => _ = new RainfallRunoffHydroCoupling(basin, lateralToLinkableObjects);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenParameters_WhenConstructing_HasEndedIsFalse()
        {
            //Arrange
            var basin = Substitute.For<IDrainageBasin>();
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();
            
            //Act
            var coupling = new RainfallRunoffHydroCoupling(basin, lateralToLinkableObjects);
            
            //Assert
            Assert.That(coupling.HasEnded, Is.False);
        }
        
        [Test]
        public void GivenParameters_WhenCouplingEnded_HasEndedIsTrue()
        {
            //Arrange
            var basin = Substitute.For<IDrainageBasin>();
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();
            var coupling = new RainfallRunoffHydroCoupling(basin, lateralToLinkableObjects);
            
            //Act
            coupling.End();
            
            //Assert
            Assert.That(coupling.HasEnded, Is.True);
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

            var hydroLink = new HydroLink(source, target);
            source.LinkTo(target).Returns(hydroLink);

            var coupling = new RainfallRunoffHydroCoupling(basin, lateralToLinkableObjects);

            //Act
            IHydroLink receivedHydroLink = coupling.CreateLink(source, target);
            
            //Assert
            source.Received(1).CanLinkTo(target);
            source.Received(1).LinkTo(target);
            Assert.That(receivedHydroLink, Is.EqualTo(hydroLink));
        }
        
        [Test]
        public void GivenCouplingWithTargetAndSourceWhichCanLinkButSourceNameNotKnown_WhenPrepareAndCreateLink_ThenLinkAndReturnHydroLink()
        {
            //Arrange
            var basin = Substitute.For<IDrainageBasin>();

            var target = Substitute.For<IHydroObject>();
            target.Name.Returns("target");
            
            var source = Substitute.For<IHydroObject>();
            source.Name.Returns("source");
            source.CanLinkTo(target).Returns(true);

            var hydroLink = new HydroLink(source, target);
            source.LinkTo(target).Returns(hydroLink);
            
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();
            const string lateralName = "Lateral";
            SobekRRLink[] sobekRRLink =
            {
                new SobekRRLink()
            };
            lateralToLinkableObjects.Add(lateralName, sobekRRLink);

            var coupling = new RainfallRunoffHydroCoupling(basin, lateralToLinkableObjects);

            //Act
            coupling.Prepare();
            IHydroLink receivedHydroLink = coupling.CreateLink(source, target);
            
            //Assert
            source.Received(1).CanLinkTo(target);
            source.Received(1).LinkTo(target);
            Assert.That(receivedHydroLink, Is.EqualTo(hydroLink));
        }
        
        [Test]
        public void GivenCouplingWithTargetAndSourceWhichCanLinkAndSourceNameIsKnown_WhenPrepareAndCreateLink_ThenLinkAndReturnHydroLink()
        {
            //Arrange
            const string sourceName = "catchment";
            const string targetName = "Lateral";

            var target = Substitute.For<IHydroObject>();
            target.Name.Returns(targetName);
            
            var source = Substitute.For<IHydroObject>();
            source.Name.Returns(sourceName);
            source.CanLinkTo(target).Returns(true);

            var hydroLink = new HydroLink(source, target);
            source.LinkTo(target).Returns(hydroLink);
            
            var basin = Substitute.For<IDrainageBasin>();
            
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();
            
            SobekRRLink[] sobekRRLink =
            {
                new SobekRRLink()
            };
            lateralToLinkableObjects.Add(targetName, sobekRRLink);

            var runoffBoundary = new RunoffBoundary { Name = targetName };
            var catchment1 = new Catchment
            {
                Name = sourceName,
                Basin = basin
            };

            basin.AllCatchments.Returns(new List<Catchment> { catchment1 });
            basin.Boundaries.Returns(new EventedList<RunoffBoundary> { runoffBoundary });

            var coupling = new RainfallRunoffHydroCoupling(basin, lateralToLinkableObjects);

            //Act
            coupling.Prepare();
            IHydroLink receivedHydroLink = coupling.CreateLink(source, target);
            
            //Assert
            source.Received(1).CanLinkTo(target);
            source.Received(1).LinkTo(target);
            Assert.That(catchment1.Basin.Boundaries, Is.Empty);
            Assert.That(receivedHydroLink, Is.EqualTo(hydroLink));
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

            var hydroLink = new HydroLink(source, target);
            source.LinkTo(target).Returns(hydroLink);

            var coupling = new RainfallRunoffHydroCoupling(basin, lateralToLinkableObjects);

            //Act
            IHydroLink receivedHydroLink = coupling.CreateLink(source, target);
            
            //Assert
            source.Received(1).CanLinkTo(target);
            source.Received(0).LinkTo(target);
            Assert.That(receivedHydroLink, Is.Null);
        }
        
        [Test]
        [TestCaseSource(nameof(ArgNullCasesCreateLink))]
        public void GivenTargetOrSourceNull_WhenCreateLink_ThrowsArgumentNullException(IHydroObject source, IHydroObject target)
        {
            //Arrange
            var basin = Substitute.For<IDrainageBasin>();
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();
            var coupling = new RainfallRunoffHydroCoupling(basin, lateralToLinkableObjects);
            
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
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();
            var coupling = new RainfallRunoffHydroCoupling(basin, lateralToLinkableObjects);
            IHydroObject catchment = new Catchment();
            
            // Call
            bool canLink = coupling.CanLink(catchment);

            // Assert
            Assert.That(canLink, Is.True);
        }
        
        [Test]
        public void GivenNoCatchment_WhenCanLink_ReturnsFalse()
        {
            //Arrange
            var basin = Substitute.For<IDrainageBasin>();
            var lateralToLinkableObjects = new Dictionary<string, SobekRRLink[]>();
            var coupling = new RainfallRunoffHydroCoupling(basin, lateralToLinkableObjects);
            var catchment = Substitute.For<IHydroObject>();
            
            // Call
            bool canLink = coupling.CanLink(catchment);

            // Assert
            Assert.That(canLink, Is.False);
        }
        
        private static IEnumerable<TestCaseData> ArgNullCases()
        {
            yield return new TestCaseData(Substitute.For<IDrainageBasin>(),null);
            yield return new TestCaseData(null, new Dictionary<string, SobekRRLink[]>());
        }
        
        private static IEnumerable<TestCaseData> ArgNullCasesCreateLink()
        {
            yield return new TestCaseData(Substitute.For<IHydroObject>(),null);
            yield return new TestCaseData(null, Substitute.For<IHydroObject>());
        }
    }
}