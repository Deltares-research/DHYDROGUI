using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.NetCdf;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.NetCdf
{
    [TestFixture]
    public class NetCdfConventionTest
    {
        [Test]
        public void Constructor_Default_InitializesInstanceCorrectly()
        {
            // Call
            var convention = new NetCdfConvention();

            // Assert
            Assert.That(convention.Cf, Is.Null);
            Assert.That(convention.UGrid, Is.Null);
            Assert.That(convention.Deltares, Is.Null);
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            var cf = new Version(1, 2, 3);
            var uGrid = new Version(4, 5, 6);
            var deltares = new Version(7, 8, 9);

            // Call
            var convention = new NetCdfConvention(cf, uGrid, deltares);

            // Assert
            Assert.That(convention.Cf, Is.SameAs(cf));
            Assert.That(convention.UGrid, Is.SameAs(uGrid));
            Assert.That(convention.Deltares, Is.SameAs(deltares));
        }

        [Test]
        public void Satisfies_RequiredConventionNull_ThrowsArgumentNullException()
        {
            // Setup
            var convention = new NetCdfConvention();

            // Call
            void Call() => convention.Satisfies(null);

            // 
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("requiredConvention"));
        }

        [Test]
        [TestCaseSource(nameof(SatisfiesCases))]
        public void Satisfies_ReturnsCorrectResult(NetCdfConvention convention, NetCdfConvention requiredConvention, bool expResult)
        {
            // Call
            bool result = convention.Satisfies(requiredConvention);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        [Test]
        [TestCaseSource(nameof(ToStringCases))]
        public void ToString_ReturnsCorrectResult(NetCdfConvention convention, string expResult)
        {
            // Call
            var result = convention.ToString();

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        private static IEnumerable<TestCaseData> SatisfiesCases()
        {
            var cf = new Version(1, 2, 3);
            var uGrid = new Version(4, 5, 6);
            var deltares = new Version(7, 8, 9);

            var cfHigher = new Version(1, 2, 4);
            var uGridHigher = new Version(4, 5, 7);
            var deltaresHigher = new Version(7, 8, 10);

            var cfLower = new Version(1, 2, 2);
            var uGridLower = new Version(4, 5, 5);
            var deltaresLower = new Version(7, 8, 8);

            // All equal versions
            yield return new TestCaseData(new NetCdfConvention(cf, uGrid, deltares), new NetCdfConvention(cf, uGrid, deltares), true).SetName("All equal");

            // All lower requirements
            yield return new TestCaseData(new NetCdfConvention(cf, uGrid, deltares), new NetCdfConvention(cfLower, uGridLower, deltaresLower), true).SetName("All lower requirements");

            // Higher requirements
            yield return new TestCaseData(new NetCdfConvention(cf, uGrid, deltares), new NetCdfConvention(cfHigher, uGrid, deltares), false).SetName("Higher required CF");
            yield return new TestCaseData(new NetCdfConvention(cf, uGrid, deltares), new NetCdfConvention(cf, uGridHigher, deltares), false).SetName("Higher required  UGRID");
            yield return new TestCaseData(new NetCdfConvention(cf, uGrid, deltares), new NetCdfConvention(cf, uGrid, deltaresHigher), false).SetName("Higher required  Deltares");

            // Missing versions
            yield return new TestCaseData(new NetCdfConvention(null, uGrid, deltares), new NetCdfConvention(cf, uGrid, deltares), false).SetName("Missing CF");
            yield return new TestCaseData(new NetCdfConvention(cf, null, deltares), new NetCdfConvention(cf, uGrid, deltares), false).SetName("Missing UGRID");
            yield return new TestCaseData(new NetCdfConvention(cf, uGrid, null), new NetCdfConvention(cf, uGrid, deltares), false).SetName("Missing Deltares");

            // Unspecified required versions
            yield return new TestCaseData(new NetCdfConvention(cf, uGrid, deltares), new NetCdfConvention(null, uGrid, deltares), true).SetName("Unspecified CF");
            yield return new TestCaseData(new NetCdfConvention(cf, uGrid, deltares), new NetCdfConvention(cf, null, deltares), true).SetName("Unspecified UGRID");
            yield return new TestCaseData(new NetCdfConvention(cf, uGrid, deltares), new NetCdfConvention(cf, uGrid, null), true).SetName("Unspecified Deltares");
        }

        private static IEnumerable<TestCaseData> ToStringCases()
        {
            var cf = new Version(1, 2, 3);
            var uGrid = new Version(4, 5, 6);
            var deltares = new Version(7, 8, 9);

            yield return new TestCaseData(new NetCdfConvention(null, null, null), string.Empty);

            yield return new TestCaseData(new NetCdfConvention(cf, null, null), "CF-1.2.3");
            yield return new TestCaseData(new NetCdfConvention(null, uGrid, null), "UGRID-4.5.6");
            yield return new TestCaseData(new NetCdfConvention(null, null, deltares), "Deltares-7.8.9");

            yield return new TestCaseData(new NetCdfConvention(cf, uGrid, null), "CF-1.2.3 UGRID-4.5.6");
            yield return new TestCaseData(new NetCdfConvention(cf, null, deltares), "CF-1.2.3 Deltares-7.8.9");
            yield return new TestCaseData(new NetCdfConvention(null, uGrid, deltares), "UGRID-4.5.6 Deltares-7.8.9");

            yield return new TestCaseData(new NetCdfConvention(cf, uGrid, deltares), "CF-1.2.3 UGRID-4.5.6 Deltares-7.8.9");
        }
    }
}