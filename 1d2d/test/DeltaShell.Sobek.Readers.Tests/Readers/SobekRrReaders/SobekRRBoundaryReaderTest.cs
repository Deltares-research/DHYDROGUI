using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRBoundaryReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadBoundaryLineFromManual()
        {
            string line = @"BOUN id '1'  bl 0  -0.5   is 100.  boun";

            var boundaryData = new SobekRRBoundaryReader().Parse(line).First();
            Assert.AreEqual("1", boundaryData.Id);
            Assert.AreEqual(-0.5, boundaryData.FixedLevel);
            Assert.AreEqual(100.0, boundaryData.InitialSaltConcentration);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadBoundaryLineFromDRRSA()
        {
            string line = @"BOUN id 'cnGFE1022' bl 1 '1' is  0 boun";

            var boundaryData = new SobekRRBoundaryReader().Parse(line).First();
            Assert.AreEqual("cnGFE1022", boundaryData.Id);
            Assert.AreEqual("1", boundaryData.TableId);
            Assert.AreEqual(0, boundaryData.InitialSaltConcentration);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadBoundaryLineFromLSMWithEndTagInId()
        {
            string line = @"BOUN id 'cnboundaryGFE1022' bl 1 '1' is 0 boun "; //boun is also in id

            var boundaryData = new SobekRRBoundaryReader().Parse(line).First();
            Assert.AreEqual("cnboundaryGFE1022", boundaryData.Id);
            Assert.AreEqual("1", boundaryData.TableId);
            Assert.AreEqual(0, boundaryData.InitialSaltConcentration);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadBoundaryLineFromTholen()
        {
            string line = @"BOUN id 'cGFE820' bl 2 'cGFE820' is 200 boun";

            var boundaryData = new SobekRRBoundaryReader().Parse(line).First();
            Assert.AreEqual("cGFE820", boundaryData.Id);
            Assert.AreEqual("cGFE820", boundaryData.VariableLevel);
            Assert.AreEqual(200, boundaryData.InitialSaltConcentration);
        }
    }
}