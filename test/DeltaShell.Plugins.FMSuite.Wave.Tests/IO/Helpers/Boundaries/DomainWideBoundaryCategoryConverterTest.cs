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
    public class DomainWideBoundaryCategoryConverterTest
    {
        [Test]
        public void IsDomainWideBoundaryCategory_BoundaryCategoriesNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => DomainWideBoundaryCategoryConverter.IsDomainWideBoundaryCategory(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryCategories"));
        }

        [Test]
        public void IsDomainWideBoundaryCategory_WithoutBoundaryCategories_ReturnsFalse()
        {
            // Call
            bool isDomainWide = DomainWideBoundaryCategoryConverter.IsDomainWideBoundaryCategory(Enumerable.Empty<DelftIniCategory>());

            // Assert
            Assert.That(isDomainWide, Is.False);
        }

        [Test]
        public void IsDomainWideBoundaryCategory_WithMultipleBoundaryCategories_ReturnsFalse()
        {
            // Call
            bool isDomainWide = DomainWideBoundaryCategoryConverter.IsDomainWideBoundaryCategory(new[]
            {
                new DelftIniCategory(KnownWaveCategories.BoundaryCategory),
                new DelftIniCategory(KnownWaveCategories.BoundaryCategory)
            });

            // Assert
            Assert.That(isDomainWide, Is.False);
        }

        [Test]
        public void IsDomainWideBoundaryCategory_WithNonDomainWideBoundaryCategory_ReturnsFalse()
        {
            // Setup
            var boundaryCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
            boundaryCategory.SetProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.OrientationDefinitionType);

            // Call
            bool isDomainWide = DomainWideBoundaryCategoryConverter.IsDomainWideBoundaryCategory(new[]
            {
                boundaryCategory
            });

            // Assert
            Assert.That(isDomainWide, Is.False);
        }

        [Test]
        public void IsDomainWideBoundaryCategory_WithDomainWideBoundaryCategory_ReturnsTrue()
        {
            // Setup
            DelftIniCategory boundaryCategory = CreateDomainWideBoundaryCategory();

            // Call
            bool isDomainWide = DomainWideBoundaryCategoryConverter.IsDomainWideBoundaryCategory(new[]
            {
                boundaryCategory
            });

            // Assert
            Assert.That(isDomainWide, Is.True);
        }

        [Test]
        public void Convert_BoundariesPerFileNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => DomainWideBoundaryCategoryConverter.Convert(null, Enumerable.Empty<DelftIniCategory>(), "path");

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundariesPerFile"));
        }

        [Test]
        public void Convert_BoundaryCategoriesNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => DomainWideBoundaryCategoryConverter.Convert(Substitute.For<IBoundariesPerFile>(), null, "path");

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryCategories"));
        }

        [Test]
        public void Convert_MdwDirPathNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => DomainWideBoundaryCategoryConverter.Convert(Substitute.For<IBoundariesPerFile>(), Enumerable.Empty<DelftIniCategory>(), null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("mdwDirPath"));
        }

        [Test]
        [TestCase(KnownWaveBoundariesFileConstants.OrientationDefinitionType)]
        [TestCase(KnownWaveBoundariesFileConstants.CoordinatesDefinitionType)]
        public void Convert_WithNonDomainWideBoundaryCategory_DoesNotUpdateBoundariesPerFile(string definitionType)
        {
            // Setup
            var boundariesPerFile = Substitute.For<IBoundariesPerFile>();
            var boundaryCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
            boundaryCategory.SetProperty(KnownWaveProperties.Definition, definitionType);

            // Call
            DomainWideBoundaryCategoryConverter.Convert(boundariesPerFile, new[]
            {
                boundaryCategory
            }, "path");

            // Assert
            Assert.That(boundariesPerFile.FilePathForBoundariesPerFile, Is.Empty);
            Assert.That(boundariesPerFile.DefinitionPerFileUsed, Is.False);
        }

        [Test]
        public void Convert_WithDomainWideBoundaryCategoryAndEmptyPath_BoundariesPerFileSetToEmptyPath()
        {
            // Setup
            var boundariesPerFile = Substitute.For<IBoundariesPerFile>();
            var boundaryCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
            boundaryCategory.SetProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.SpectrumFileDefinitionType);
            boundaryCategory.SetProperty(KnownWaveProperties.OverallSpecFile, string.Empty);

            // Call
            DomainWideBoundaryCategoryConverter.Convert(boundariesPerFile, new[]
            {
                boundaryCategory
            }, @"C:\folder");

            // Assert
            Assert.That(boundariesPerFile.FilePathForBoundariesPerFile, Is.Empty);
            Assert.That(boundariesPerFile.DefinitionPerFileUsed, Is.True);
        }

        [Test]
        public void Convert_WithDomainWideBoundaryCategory_SetsExpectedPropertiesOnBoundariesPerFile()
        {
            // Setup
            var boundariesPerFile = Substitute.For<IBoundariesPerFile>();
            DelftIniCategory boundaryCategory = CreateDomainWideBoundaryCategory();

            // Call
            DomainWideBoundaryCategoryConverter.Convert(boundariesPerFile, new[]
            {
                boundaryCategory
            }, @"C:\folder");

            // Assert
            Assert.That(boundariesPerFile.FilePathForBoundariesPerFile, Is.EqualTo(@"C:\folder\spectrumFile.sp2"));
            Assert.That(boundariesPerFile.DefinitionPerFileUsed, Is.True);
        }

        private static DelftIniCategory CreateDomainWideBoundaryCategory()
        {
            var boundaryCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
            boundaryCategory.SetProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.SpectrumFileDefinitionType);
            boundaryCategory.SetProperty(KnownWaveProperties.OverallSpecFile, "spectrumFile.sp2");
            return boundaryCategory;
        }
    }
}