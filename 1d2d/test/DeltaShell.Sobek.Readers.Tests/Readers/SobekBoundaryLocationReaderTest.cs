using System;
using System.Linq;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekBoundaryLocationReaderTest
    {
        [Test]
        public void BranchBoundary()
        {
            const string source = @"FLBR id 'Zwolle' nm 'ZwolleName' ci 'stadsgrachten Zwolle Midden' lc 437.188205277233 flbr";

            SobekBoundaryLocationReader sobekLateralFlowReader = new SobekBoundaryLocationReader { SobekType = SobekType.Sobek212 };
            var boundaryLocation = sobekLateralFlowReader.GetBoundaryLocation(source);
            Assert.AreEqual("Zwolle", boundaryLocation.Id);
            Assert.AreEqual("ZwolleName", boundaryLocation.Name);
            Assert.AreEqual("stadsgrachten Zwolle Midden", boundaryLocation.ConnectionId);
            Assert.AreEqual(437.188205277233, boundaryLocation.Offset, 1.0e-6);
        }

        [Test]
        public void SaltNode() // only valid for Sobek RE model
        {
            const string source = @"STBO id '14' nm '(null)' ci '0' stbo";

            var sobekLateralFlowReader = new SobekBoundaryLocationReader { SobekType = SobekType.SobekRE };
            var boundaryLocation = sobekLateralFlowReader.GetBoundaryLocation(source);
            Assert.AreEqual(SobekBoundaryLocationType.SaltNode, boundaryLocation.SobekBoundaryLocationType);
            Assert.AreEqual("14", boundaryLocation.Id);
            Assert.AreEqual("0", boundaryLocation.ConnectionId);
        }

        [Test]
        public void SaltLateral() // only valid for Sobek RE model
        {
            const string source = @"STBR id '21' nm 's2' ci '10' lc 0.67 stbr";

            var sobekLateralFlowReader = new SobekBoundaryLocationReader { SobekType = SobekType.SobekRE };
            var boundaryLocation = sobekLateralFlowReader.GetBoundaryLocation(source);
            Assert.AreEqual(SobekBoundaryLocationType.SaltLateral, boundaryLocation.SobekBoundaryLocationType);
            Assert.AreEqual("21", boundaryLocation.Id);
            Assert.AreEqual("10", boundaryLocation.ConnectionId);
            Assert.AreEqual(0.67, boundaryLocation.Offset, 1.0e-6);
        }

        [Test]
        public void ParseMultiptipleLocations()
        {
            string source = 
                @"FLBO id 'Hancate' ci 'Hancate' flbo" + Environment.NewLine + 
                @"FLBO id 'Boxbergen' ci 'Boxbergen' flbo" + Environment.NewLine + 
                @"FLBR id 'Zwolle' nm 'Zwolle' ci 'stadsgrachten Zwolle Midden' lc 437.188205277233 flbr " + Environment.NewLine + 
                @"FLBR id 'Herfte' nm 'Herfte' ci 'Almelose kanaal' lc 1026.41368952124 flbr ";
            SobekBoundaryLocationReader sobekLateralFlowReader = new SobekBoundaryLocationReader { SobekType = SobekType.Sobek212 };
            var sobekBoundaryLocations = sobekLateralFlowReader.Parse(source);
            Assert.AreEqual(4, sobekBoundaryLocations.Count());
            Assert.AreEqual(2,
                sobekBoundaryLocations.Where(bl => bl.SobekBoundaryLocationType == SobekBoundaryLocationType.Node).Count());
            Assert.AreEqual(2,
                sobekBoundaryLocations.Where(bl => bl.SobekBoundaryLocationType == SobekBoundaryLocationType.Branch).Count());
        }

        [Test]
        public void ParseRRLaterals()
        {
            var source = @"CN_1.1\n" +
                         @"FLBO id '2' ci '2' flbo\n" +
                         @"FLBX id 'cnGFE1022' nm '' ci '1' lc 670.711334421796 flbx\n" +
                         @"FLBX id 'cnGFE1021' nm '' ci '1' lc 1711.10082063482 flbx\n";
            var sobekLateralFlowReader = new SobekBoundaryLocationReader { SobekType = SobekType.Sobek212 };
            var sobekBoundaryLocations = sobekLateralFlowReader.Parse(source);
            Assert.AreEqual(3, sobekBoundaryLocations.Count());
        }
    }
}
