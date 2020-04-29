using System;
using System.Linq;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class OverallBoundaryCategoryConverterTest
    {
        [Test]
        public void Convert_BoundariesPerFileNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => OverallBoundaryCategoryConverter.Convert(null, Enumerable.Empty<DelftIniCategory>());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundariesPerFile"));
        }

        [Test]
        public void Convert_BoundaryCategoriesNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => OverallBoundaryCategoryConverter.Convert(Substitute.For<IBoundariesPerFile>(), null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryCategories"));
        }

        [Test]
        public void Convert_WithoutBoundaryCategories_ThrowsArgumentNullException()
        {
            // Setup
            var boundariesPerFile = new TestBoundariesPerFile();

            // Call
            OverallBoundaryCategoryConverter.Convert(boundariesPerFile, Enumerable.Empty<DelftIniCategory>());

            // Assert
            Assert.That(boundariesPerFile.FileNameForBoundariesPerFile, Is.Null);
            Assert.That(boundariesPerFile.DefinitionPerFileUsed, Is.False);
        }

        [Test]
        public void Convert_WithoutMultipleBoundaryCategories_ThrowsArgumentNullException()
        {
            // Setup
            var boundariesPerFile = new TestBoundariesPerFile();

            // Call
            OverallBoundaryCategoryConverter.Convert(boundariesPerFile, new []
            {
                new DelftIniCategory(KnownWaveCategories.BoundaryCategory), 
                new DelftIniCategory(KnownWaveCategories.BoundaryCategory)
            });

            // Assert
            Assert.That(boundariesPerFile.FileNameForBoundariesPerFile, Is.Null);
            Assert.That(boundariesPerFile.DefinitionPerFileUsed, Is.False);
        }

        [Test]
        public void Convert_WithNotOverallBoundaryCategory_ThrowsArgumentNullException()
        {
            // Setup
            var boundariesPerFile = new TestBoundariesPerFile();
            var boundaryCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
            boundaryCategory.SetProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.OrientationDefinitionType);

            // Call
            OverallBoundaryCategoryConverter.Convert(boundariesPerFile, new []
            {
                boundaryCategory
            });

            // Assert
            Assert.That(boundariesPerFile.FileNameForBoundariesPerFile, Is.Null);
            Assert.That(boundariesPerFile.DefinitionPerFileUsed, Is.False);
        }

        [Test]
        public void Convert_WithOverallBoundaryCategory_ThrowsArgumentNullException()
        {
            // Setup
            var boundariesPerFile = new TestBoundariesPerFile();
            var boundaryCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
            boundaryCategory.SetProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.SpectrumFileDefinitionType);
            boundaryCategory.SetProperty(KnownWaveProperties.OverallSpecFile, "spectrumFile.sp2");

            // Call
            OverallBoundaryCategoryConverter.Convert(boundariesPerFile, new []
            {
                boundaryCategory
            });

            // Assert
            Assert.That(boundariesPerFile.FileNameForBoundariesPerFile, Is.EqualTo("spectrumFile.sp2"));
            Assert.That(boundariesPerFile.DefinitionPerFileUsed, Is.True);
        }

        private class TestBoundariesPerFile : IBoundariesPerFile
        {
            public bool DefinitionPerFileUsed { get; set; }
            public string FileNameForBoundariesPerFile { get; set; }
        }
    }
}