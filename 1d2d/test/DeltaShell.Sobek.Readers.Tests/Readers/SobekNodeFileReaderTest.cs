using System.Linq;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
        [TestFixture]
    public class SobekodeFileReaderTest
    {

        [Test]
        public void ReadNode()
        {
            const string source = @"NODE id 'B2&B3_h_x=0m' ty 0 ni 1 r1 '1' r2 '2' node"; //NODES.DAT
            var sobekNode = new SobekNodeFileReader().Parse(source).FirstOrDefault();

            Assert.IsNotNull(sobekNode);
            Assert.AreEqual("B2&B3_h_x=0m", sobekNode.ID);
            Assert.AreEqual(true, sobekNode.InterpolationOverNode);
            Assert.AreEqual("1", sobekNode.InterpolationFrom);
            Assert.AreEqual("2", sobekNode.InterpolationTo);
        }

        [Test]
        public void ReadNodeWithInterpolationFalse()
        {
            const string source = @"NODE id '3' ty 0 ni 0 r1 '11' r2 '22' node"; //NODES.DAT
            var sobekNode = new SobekNodeFileReader().Parse(source).FirstOrDefault();

            Assert.IsNotNull(sobekNode);
            Assert.AreEqual("3", sobekNode.ID);
            Assert.AreEqual(false, sobekNode.InterpolationOverNode);
            Assert.AreEqual("11", sobekNode.InterpolationFrom);
            Assert.AreEqual("22", sobekNode.InterpolationTo);
        }

 
    }
}
