using DHYDRO.Common.IO.BndExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceFileOptionsTest
    {
        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Constructor_ExtForceFilePathIsNullOrEmpty_ThrowsArgumentException(string filePath)
        {
            Assert.That(() => _ = new BndExtForceFileOptions(filePath), Throws.ArgumentException);
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Constructor_ExtReferenceFilePathIsNullOrEmpty_ThrowsArgumentException(string referenceFilePath)
        {
            Assert.That(() => _ = new BndExtForceFileOptions("FlowFM_bnd.ext", referenceFilePath), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_ExpectedResults()
        {
            var options = new BndExtForceFileOptions("FlowFM_bnd.ext", "FlowFM.mdu");

            Assert.That(options.ExtForceFilePath, Is.EqualTo("FlowFM_bnd.ext"));
            Assert.That(options.ReferenceFilePath, Is.EqualTo("FlowFM.mdu"));
            Assert.That(options.SwitchTo, Is.False);
        }
    }
}