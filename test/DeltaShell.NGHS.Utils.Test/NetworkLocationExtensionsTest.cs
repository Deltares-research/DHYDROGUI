using System;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Utils.Test
{
    [TestFixture]
    public class NetworkLocationExtensionsTest
    {
        [Test]
        public void GivenNetworkLocation_IsOnEndOfBranch_ShouldReturnCorrectResult()
        {
            //Arrange
            var location = Substitute.For<INetworkLocation>();
            var branch = Substitute.For<IBranch>();

            location.Branch.Returns(branch);
            branch.Length.Returns(100);

            // Act & Assert
            location.Chainage = 100;
            Assert.IsTrue(location.IsOnEndOfBranch());

            location.Chainage = 100 - 1e-12;
            Assert.IsTrue(location.IsOnEndOfBranch());

            location.Chainage = 99;
            Assert.IsFalse(location.IsOnEndOfBranch());

            location.Chainage = double.NaN;
            Assert.IsFalse(location.IsOnEndOfBranch());

            location.Chainage = double.PositiveInfinity;
            Assert.IsFalse(location.IsOnEndOfBranch());
        }

        [Test]
        public void GivenNetworkLocation_IsOnEndOfBranch_ShouldThrowWhenNullOrBranchNull()
        {
            //Arrange
            var location = Substitute.For<INetworkLocation>();
            
            location.Chainage = 1;
            location.Branch = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => location.IsOnEndOfBranch());
            Assert.Throws<ArgumentNullException>(() => ((INetworkLocation)null).IsOnEndOfBranch());
        }
    }
}