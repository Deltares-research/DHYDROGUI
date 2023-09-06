using System;
using System.Linq;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class DomainWideBoundaryCategoryConverterTest
    {
        [Test]
        public void IsDomainWideBoundarySection_BoundarySectionsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => DomainWideBoundaryCategoryConverter.IsDomainWideBoundarySection(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundarySections"));
        }

        [Test]
        public void IsDomainWideBoundarySection_WithoutBoundarySections_ReturnsFalse()
        {
            // Call
            bool isDomainWide = DomainWideBoundaryCategoryConverter.IsDomainWideBoundarySection(Enumerable.Empty<IniSection>());

            // Assert
            Assert.That(isDomainWide, Is.False);
        }

        [Test]
        public void IsDomainWideBoundarySection_WithMultipleBoundarySections_ReturnsFalse()
        {
            // Call
            bool isDomainWide = DomainWideBoundaryCategoryConverter.IsDomainWideBoundarySection(new[]
            {
                new IniSection(KnownWaveSections.BoundarySection),
                new IniSection(KnownWaveSections.BoundarySection)
            });

            // Assert
            Assert.That(isDomainWide, Is.False);
        }

        [Test]
        public void IsDomainWideBoundarySection_WithNonDomainWideBoundarySection_ReturnsFalse()
        {
            // Setup
            var boundarySection = new IniSection(KnownWaveSections.BoundarySection);
            boundarySection.AddProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.OrientationDefinitionType);

            // Call
            bool isDomainWide = DomainWideBoundaryCategoryConverter.IsDomainWideBoundarySection(new[]
            {
                boundarySection
            });

            // Assert
            Assert.That(isDomainWide, Is.False);
        }

        [Test]
        public void IsDomainWideBoundarySection_WithDomainWideBoundarySection_ReturnsTrue()
        {
            // Setup
            IniSection boundarySection = CreateDomainWideBoundarySection();

            // Call
            bool isDomainWide = DomainWideBoundaryCategoryConverter.IsDomainWideBoundarySection(new[]
            {
                boundarySection
            });

            // Assert
            Assert.That(isDomainWide, Is.True);
        }

        [Test]
        [TestCase(KnownWaveBoundariesFileConstants.OrientationDefinitionType)]
        [TestCase(KnownWaveBoundariesFileConstants.CoordinatesDefinitionType)]
        public void IsDomainWideBoundaryCategory_WithNonDomainWideBoundarySection_DoesNotUpdateBoundariesPerFile(string definitionType)
        {
            // Setup
            var boundariesPerFile = Substitute.For<IBoundariesPerFile>();
            var boundarySection = new IniSection(KnownWaveSections.BoundarySection);
            boundarySection.AddProperty(KnownWaveProperties.Definition, definitionType);

            // Call
            DomainWideBoundaryCategoryConverter.IsDomainWideBoundarySection(new[]
            {
                boundarySection
            });

            // Assert
            Assert.That(boundariesPerFile.FilePathForBoundariesPerFile, Is.Empty);
            Assert.That(boundariesPerFile.DefinitionPerFileUsed, Is.False);
        }

        [Test]
        public void Convert_BoundariesPerFileNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => DomainWideBoundaryCategoryConverter.Convert(null, Enumerable.Empty<IniSection>(), "path");

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundariesPerFile"));
        }

        [Test]
        public void Convert_BoundarySectionsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => DomainWideBoundaryCategoryConverter.Convert(Substitute.For<IBoundariesPerFile>(), null, "path");

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundarySections"));
        }

        [Test]
        public void Convert_MdwDirPathNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => DomainWideBoundaryCategoryConverter.Convert(Substitute.For<IBoundariesPerFile>(), Enumerable.Empty<IniSection>(), null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("mdwDirPath"));
        }

        [Test]
        [TestCase(KnownWaveBoundariesFileConstants.OrientationDefinitionType)]
        [TestCase(KnownWaveBoundariesFileConstants.CoordinatesDefinitionType)]
        public void Convert_WithNonDomainWideBoundarySection_DoesNotUpdateBoundariesPerFile(string definitionType)
        {
            // Setup
            var boundariesPerFile = Substitute.For<IBoundariesPerFile>();
            var boundarySection = new IniSection(KnownWaveSections.BoundarySection);
            boundarySection.AddProperty(KnownWaveProperties.Definition, definitionType);

            // Call
            DomainWideBoundaryCategoryConverter.Convert(boundariesPerFile, new[]
            {
                boundarySection
            }, "path");

            // Assert
            Assert.That(boundariesPerFile.FilePathForBoundariesPerFile, Is.Empty);
            Assert.That(boundariesPerFile.DefinitionPerFileUsed, Is.False);
        }

        [Test]
        public void Convert_WithDomainWideBoundarySectionAndEmptyPath_BoundariesPerFileSetToEmptyPath()
        {
            // Setup
            var boundariesPerFile = Substitute.For<IBoundariesPerFile>();
            var boundarySection = new IniSection(KnownWaveSections.BoundarySection);
            boundarySection.AddProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.SpectrumFileDefinitionType);
            boundarySection.AddProperty(KnownWaveProperties.OverallSpecFile, string.Empty);

            // Call
            DomainWideBoundaryCategoryConverter.Convert(boundariesPerFile, new[]
            {
                boundarySection
            }, @"C:\folder");

            // Assert
            Assert.That(boundariesPerFile.FilePathForBoundariesPerFile, Is.Empty);
            Assert.That(boundariesPerFile.DefinitionPerFileUsed, Is.True);
        }

        [Test]
        public void Convert_WithDomainWideBoundarySection_SetsExpectedPropertiesOnBoundariesPerFile()
        {
            // Setup
            var boundariesPerFile = Substitute.For<IBoundariesPerFile>();
            IniSection boundarySection = CreateDomainWideBoundarySection();

            // Call
            DomainWideBoundaryCategoryConverter.Convert(boundariesPerFile, new[]
            {
                boundarySection
            }, @"C:\folder");

            // Assert
            Assert.That(boundariesPerFile.FilePathForBoundariesPerFile, Is.EqualTo(@"C:\folder\spectrumFile.sp2"));
            Assert.That(boundariesPerFile.DefinitionPerFileUsed, Is.True);
        }

        private static IniSection CreateDomainWideBoundarySection()
        {
            var boundarySection = new IniSection(KnownWaveSections.BoundarySection);
            boundarySection.AddProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.SpectrumFileDefinitionType);
            boundarySection.AddProperty(KnownWaveProperties.OverallSpecFile, "spectrumFile.sp2");
            return boundarySection;
        }
    }
}