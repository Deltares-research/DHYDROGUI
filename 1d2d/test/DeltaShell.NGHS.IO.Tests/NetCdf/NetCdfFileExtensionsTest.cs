using System;
using System.Collections.Generic;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.NetCdf;
using GeoAPI.Extensions.CoordinateSystems;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.NetCdf
{
    public class NetCdfFileExtensionsTest
    {
        [Test]
        public void GetConvention_NetCdfFileNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((NetCdfFile)null).GetConvention();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("netCdfFile"));
        }

        [Test]
        public void GetConvention_ConventionAttributeMissing_ReturnsNull()
        {
            // Setup
            var netCdfFile = Substitute.For<INetCdfFile>();

            // Call
            NetCdfConvention convention = netCdfFile.GetConvention();

            // Assert
            Assert.That(convention, Is.Null);
        }

        [Test]
        [TestCaseSource(nameof(GetConventionCases))]
        public void GetConvention_ReturnsCorrectResult(string conventionsAttributeValue, NetCdfConvention expConvention)
        {
            // Setup
            var netCdfFile = Substitute.For<INetCdfFile>();
            var conventionsAttribute = new NetCdfAttribute("Conventions", conventionsAttributeValue);
            netCdfFile.GetGlobalAttribute("Conventions").Returns(conventionsAttribute);

            // Call
            NetCdfConvention convention = netCdfFile.GetConvention();

            // Assert
            Assert.That(convention.Cf, Is.EqualTo(expConvention.Cf));
            Assert.That(convention.UGrid, Is.EqualTo(expConvention.UGrid));
            Assert.That(convention.Deltares, Is.EqualTo(expConvention.Deltares));
        }

        private static IEnumerable<TestCaseData> GetConventionCases()
        {
            var expConvention1 = new NetCdfConvention(new Version(1, 23),
                                                      new Version(4, 56),
                                                      new Version(7, 89));
            yield return new TestCaseData("CF-1.23 UGRID-4.56 Deltares-7.89", expConvention1);
            yield return new TestCaseData("CF-1.23 UGRID-4.56/Deltares-7.89", expConvention1);

            var expConvention2 = new NetCdfConvention(null,
                                                      new Version(4, 56),
                                                      new Version(7, 89));
            yield return new TestCaseData("UGRID-4.56 Deltares-7.89", expConvention2);

            var expConvention3 = new NetCdfConvention(new Version(1, 23),
                                                      null,
                                                      new Version(7, 89));
            yield return new TestCaseData("CF-1.23 Deltares-7.89", expConvention3);

            var expConvention4 = new NetCdfConvention(new Version(1, 23),
                                                      new Version(4, 56),
                                                      null);
            yield return new TestCaseData("CF-1.23 UGRID-4.56", expConvention4);

            var expConvention5 = new NetCdfConvention(null, null, null);
            yield return new TestCaseData("", expConvention5);
        }

        [Test]
        public void GetCoordinateSystem_NetCdfFileNull_ThrowsArgumentNullException()
        {
            // Setup
            var coordinateSystemFactory = Substitute.For<ICoordinateSystemFactory>();

            // Call
            void Call() => ((NetCdfFile)null).GetCoordinateSystem(coordinateSystemFactory);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("netCdfFile"));
        }

        [Test]
        public void GetCoordinateSystem_CoordinateSystemFactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var netCdfFile = Substitute.For<INetCdfFile>();

            // Call
            void Call() => netCdfFile.GetCoordinateSystem(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("coordinateSystemFactory"));
        }

        [Test]
        public void GetCoordinateSystem_CoordinateSystemVariableMissing_ReturnsNull()
        {
            // Setup
            var netCdfFile = Substitute.For<INetCdfFile>();
            var coordinateSystemFactory = Substitute.For<ICoordinateSystemFactory>();

            // Call
            ICoordinateSystem result = netCdfFile.GetCoordinateSystem(coordinateSystemFactory);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetCoordinateSystem_EpsgAttributeMissing_ReturnsNull()
        {
            // Setup
            var netCdfFile = Substitute.For<INetCdfFile>();
            var coordinateSystemFactory = Substitute.For<ICoordinateSystemFactory>();

            netCdfFile.GetVariableByName("projected_coordinate_system").Returns(new NetCdfVariable(1));

            // Call
            ICoordinateSystem result = netCdfFile.GetCoordinateSystem(coordinateSystemFactory);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetCoordinateSystem_CannotParseEpsg_ReturnsNull()
        {
            // Setup
            var netCdfFile = Substitute.For<INetCdfFile>();
            var coordinateSystemFactory = Substitute.For<ICoordinateSystemFactory>();

            var coordinateSystemVariable = new NetCdfVariable(1);
            netCdfFile.GetVariableByName("projected_coordinate_system").Returns(coordinateSystemVariable);
            netCdfFile.GetAttributeValue(coordinateSystemVariable, "epsg").Returns("abcde");

            // Call
            ICoordinateSystem result = netCdfFile.GetCoordinateSystem(coordinateSystemFactory);

            // Assert
            Assert.That(result, Is.Null);
        }

        [TestCase("projected_coordinate_system")]
        [TestCase("wgs84")]
        public void GetCoordinateSystem_ReturnsCorrectResult(string coordinateSystemVariableName)
        {
            // Setup
            var netCdfFile = Substitute.For<INetCdfFile>();
            var coordinateSystemFactory = Substitute.For<ICoordinateSystemFactory>();

            var coordinateSystemVariable = new NetCdfVariable(1);
            netCdfFile.GetVariableByName(coordinateSystemVariableName).Returns(coordinateSystemVariable);
            netCdfFile.GetAttributeValue(coordinateSystemVariable, "epsg").Returns("12345");

            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            coordinateSystemFactory.CreateFromEPSG(12345).Returns(coordinateSystem);

            // Call
            ICoordinateSystem result = netCdfFile.GetCoordinateSystem(coordinateSystemFactory);

            // Assert
            Assert.That(result, Is.SameAs(coordinateSystem));
        }
    }
}