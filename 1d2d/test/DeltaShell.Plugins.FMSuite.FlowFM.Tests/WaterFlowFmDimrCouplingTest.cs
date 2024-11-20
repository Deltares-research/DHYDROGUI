using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFmDimrCouplingTest
    {
        [Test]
        public void GivenNull_WhenConstructing_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new WaterFlowFmDimrCoupling(null);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("network"));
        }
        
        [Test]
        public void GivenCouplingWithTargetAndSourceWhichCanLink_WhenCreateLink_ThenLinkAndReturnHydroLink()
        {
            //Arrange
            var network = Substitute.For<IHydroNetwork>();
            
            var target = Substitute.For<IHydroObject>();
            target.Name.Returns("target");
            
            var source = Substitute.For<IHydroObject>();
            source.Name.Returns("source");
            source.CanLinkTo(target).Returns(true);

            var coupling = new WaterFlowFmDimrCoupling(network);

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
            var network = Substitute.For<IHydroNetwork>();
            
            var target = Substitute.For<IHydroObject>();
            target.Name.Returns("target");
            
            var source = Substitute.For<IHydroObject>();
            source.Name.Returns("source");
            source.CanLinkTo(target).Returns(false);

            var coupling = new WaterFlowFmDimrCoupling(network);

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
            var coupling = new WaterFlowFmDimrCoupling(Substitute.For<IHydroNetwork>());
            
            // Call
            
            void Call() => coupling.CreateLink(source, target);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<ArgumentNullException>());
        }
        
        private static IEnumerable<TestCaseData> ArgNullCasesCreateLink()
        {
            yield return new TestCaseData(Substitute.For<IHydroObject>(),null);
            yield return new TestCaseData(null, Substitute.For<IHydroObject>());
        }
    }
}