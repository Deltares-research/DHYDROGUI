using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    [TestFixture]
    public class Sp2FileTest
    {
        [Test]
        public void ReadCoordinatesFromSp2File()
        {
            var sp2Path = TestHelper.GetTestFilePath(@"boundaryFromSp2\Nest002.sp2");

            var sp2File = new Sp2File();
            var data = sp2File.Read(sp2Path);

            var coordinates = data.Keys.ToList();

            Assert.AreEqual(212, coordinates.Count, "number of coordinates in sp2 file");
            Assert.AreEqual(150260.2969, coordinates[3].X);   
            Assert.AreEqual(614724.2500, coordinates[3].Y);
        }
    }
}