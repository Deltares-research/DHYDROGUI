using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroCouplingTest
    {
        [Test]
        public void GivenConstructor_WhenConstructor_HasEndedIsFalse()
        {
            //Arrange & Act
            var coupling = new HydroCoupling();
            
            //Assert
            Assert.That(coupling.HasEnded, Is.False);
        }
        
        [Test]
        public void GivenConstructor_WhenCouplingEnded_HasEndedIsTrue()
        {
            //Arrange
            var coupling = new HydroCoupling();
            
            //Act
            coupling.End();
            
            //Assert
            Assert.That(coupling.HasEnded, Is.True);
        }
        
        [Test]
        public void GivenSource_WhenCanLink_ReturnFalse()
        {
            //Arrange
            var coupling = new HydroCoupling();
            var source = Substitute.For<IHydroObject>();
            
            //Act
            bool canLink = coupling.CanLink(source);
            
            //Assert
            Assert.That(canLink, Is.False);
        }
        
        [Test]
        public void GivenCouplingWithTargetAndSourceWhichCanLink_WhenCreateLink_ThenLinkAndReturnHydroLink()
        {
            //Arrange
            var target = Substitute.For<IHydroObject>();
            target.Name.Returns("target");
            
            var source = Substitute.For<IHydroObject>();
            source.Name.Returns("source");
            source.CanLinkTo(target).Returns(true);

            var hydroLink = new HydroLink(source, target);
            source.LinkTo(target).Returns(hydroLink);

            var coupling = new HydroCoupling();

            //Act
            IHydroLink receivedHydroLink = coupling.CreateLink(source, target);
            
            //Assert
            source.Received(1).CanLinkTo(target);
            source.Received(1).LinkTo(target);
            Assert.That(receivedHydroLink, Is.EqualTo(hydroLink));
        }

        [Test]
        public void GivenCouplingWithTargetAndSourceWhichCanNotLink_WhenCreateLink_ThenDoNotLinkAndReturnNull()
        {
            //Arrange
            var target = Substitute.For<IHydroObject>();
            target.Name.Returns("target");
            
            var source = Substitute.For<IHydroObject>();
            source.Name.Returns("source");
            source.CanLinkTo(target).Returns(false);

            var hydroLink = new HydroLink(source, target);
            source.LinkTo(target).Returns(hydroLink);

            var coupling = new HydroCoupling();

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
            var coupling = new HydroCoupling();
            
            // Call
            
            void Call() => coupling.CreateLink(source, target);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<ArgumentNullException>());
        }
        
        [Test]
        public void GivenAnyString_WhenGetLinkHydroObjectsByItemString_ReturnEmptyList()
        {
            //Arrange
            var coupling = new HydroCoupling();
            
            // Call
            IList<IHydroObject> list = coupling.GetLinkHydroObjectsByItemString("AnyString").ToList();

            // Assert
            Assert.That(list, Is.Empty);
        }

        private static IEnumerable<TestCaseData> ArgNullCasesCreateLink()
        {
            yield return new TestCaseData(Substitute.For<IHydroObject>(),null);
            yield return new TestCaseData(null, Substitute.For<IHydroObject>());
        }
    }
}