using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Fnm;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.IO.Files.Fnm
{
    [TestFixture]
    public class FnmFileTest
    {
        [Test]
        public void GetSubFiles_ReturnsCorrectResult()
        {
            // Setup
            var fnmFile = new FnmFile();

            // Call
            FnmSubFile[] result = fnmFile.SubFiles.ToArray();

            // Assert
            Assert.That(result, Has.Length.EqualTo(123));
            Assert.That(result.Select(r => r.FileName), Is.Unique);
            Assert.That(result.Select(r => r.Index), Is.Unique);
        }

        [Test]
        public void GetEvaporation_ReturnsCorrectResult()
        {
            // Setup
            var fnmFile = new FnmFile();

            // Call
            FnmSubFile result = fnmFile.Evaporation;

            // Assert
            Assert.That(result.FileName, Is.EqualTo("default.evp"));
            Assert.That(result.Index, Is.EqualTo(14));
            Assert.That(result.Description, Is.EqualTo("verdampingsfile"));
            Assert.That(result.FileType, Is.EqualTo(FnmSubFileType.Input));
        }
    }
}