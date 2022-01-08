using System;
using System.Collections.Generic;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.Utils.NetCdf;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Utils.Test.NetCdf
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
    }
}