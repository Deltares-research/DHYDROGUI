using System;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class ExtForceFileHelperTest
    {
        [Test]
        public void GetPliFileName_FeatureWithoutName_ReturnsNull()
        {
            // Setup
            var featureData = new SourceAndSink
            {
                Feature = new Feature2D {Geometry = new Point(0.0, 0.0)},
                Data = null
            };

            // Call
            string fileName = ExtForceFileHelper.GetPliFileName(featureData);

            // Assert
            Assert.That(fileName, Is.Null);
        }

        [Test]
        public void GetPliFileName_FeatureWithName_ReturnsExpectedName()
        {
            // Setup
            const string name = "Test";
            var featureData = new SourceAndSink
            {
                Feature = new Feature2D
                {
                    Name = name,
                    Geometry = new Point(0.0, 0.0)
                },
                Data = null
            };

            // Call
            string fileName = ExtForceFileHelper.GetPliFileName(featureData);

            // Assert
            Assert.That(fileName, Is.EqualTo($"{name}.pli"));
        }

        [Test]
        public void GetNumberedFilePath_WithZero_ReturnsExpectedFilePath()
        {
            // Call
            string filePath = ExtForceFileHelper.GetNumberedFilePath("pliFilePath.pli", "pli", 0);

            // Assert
            Assert.That(filePath, Is.EqualTo("pliFilePath.pli"));
        }

        [Test]
        public void GetNumberedFilePath_WithOne_ReturnsExpectedFilePath()
        {
            // Call
            string filePath = ExtForceFileHelper.GetNumberedFilePath("pliFilePath.pli", "pli", 1);

            // Assert
            Assert.That(filePath, Is.EqualTo("pliFilePath_0001.pli"));
        }

        [Test]
        public void GetNumberedFilePath_FilePathNull_ThrowsFormatException()
        {
            // Setup
            string pliFilePath = null;

            // Call
            void Call() => ExtForceFileHelper.GetNumberedFilePath(pliFilePath, "pli", 1);

            // Assert
            var exception = Assert.Throws<FormatException>(Call);
            Assert.That(exception.Message, Is.EqualTo($"Invalid file path {pliFilePath}"));
        }
    }
}