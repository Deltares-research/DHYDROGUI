using System;
using System.Linq;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.IO.Ini.Configuration;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.Ini
{
    [TestFixture]
    public class IniMergerTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            IniMerger iniMerger = CreateIniMerger();

            Assert.That(iniMerger.Configuration, Is.Not.Null);
            Assert.That(iniMerger.Configuration.AddAddedSections, Is.True);
            Assert.That(iniMerger.Configuration.AddAddedProperties, Is.True);
            Assert.That(iniMerger.Configuration.RemoveRemovedSections, Is.True);
            Assert.That(iniMerger.Configuration.RemoveRemovedProperties, Is.True);
        }
        
        [Test]
        public void Configuration_SetToNull_ThrowsArgumentNullException()
        {
            IniMerger iniMerger = CreateIniMerger();

            Assert.Throws<ArgumentNullException>(() => iniMerger.Configuration = null);
        }
        
        [Test]
        public void Configuration_SetToValidConfiguration_ReturnsSameInstance()
        {
            IniMergeConfiguration configuration = CreateConfiguration();
            IniMerger iniMerger = CreateIniMerger();
            
            iniMerger.Configuration = configuration;
         
            Assert.That(configuration, Is.SameAs(iniMerger.Configuration));
        }

        [Test]
        public void Merge_OriginalIsNull_ThrowsArgumentNullException()
        {
            IniData modified = IniDataFixture.CreateIniData();

            IniMerger iniMerger = CreateIniMerger();

            Assert.Throws<ArgumentNullException>(() => iniMerger.Merge(null, modified));
        }

        [Test]
        public void Merge_ModifiedIsNull_ThrowsArgumentNullException()
        {
            IniData original = IniDataFixture.CreateIniData();

            IniMerger iniMerger = CreateIniMerger();

            Assert.Throws<ArgumentNullException>(() => iniMerger.Merge(original, null));
        }

        [Test]
        public void Merge_OriginalAndModifiedIniDataAreEmpty_ReturnsEmpty()
        {
            IniData original = IniDataFixture.CreateEmptyIniData();
            IniData modified = IniDataFixture.CreateEmptyIniData();

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged.Sections, Is.Empty);
        }

        [Test]
        public void Merge_OriginalAndModifiedIniDataAreEqual_ReturnsEqual()
        {
            IniData original = IniDataFixture.CreateIniData();
            IniData modified = IniDataFixture.CreateIniData();

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged, Is.EqualTo(original));
            Assert.That(merged, Is.EqualTo(modified));
        }

        [Test]
        public void Merge_OriginalAndModifiedIniDataAreEqual_ReturnsNewInstances()
        {
            IniData original = IniDataFixture.CreateIniData();
            IniData modified = IniDataFixture.CreateIniData();

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged.Sections.All(m => !original.Sections.Any(s => ReferenceEquals(m, s))));
            Assert.That(merged.Sections.All(m => !modified.Sections.Any(t => ReferenceEquals(m, t))));
        }

        [Test]
        public void Merge_OriginalAndModifiedHaveMixedCasedSectionNames_ReturnsOriginal()
        {
            IniSection[] originalSections = IniDataFixture.CreateSections("Section");
            IniSection[] modifiedSections = IniDataFixture.CreateSections("SECTION");

            IniData original = IniDataFixture.CreateIniData(originalSections);
            IniData modified = IniDataFixture.CreateIniData(modifiedSections);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged, Is.EqualTo(original));
            Assert.That(merged.Sections.Select(x => x.Name), Is.EqualTo(original.Sections.Select(x => x.Name)));
        }

        [Test]
        public void Merge_OriginalAndModifiedHaveDuplicateSectionNames_ReturnsEqual()
        {
            IniSection[] originalSections =
            {
                IniDataFixture.CreateSection("s"), 
                IniDataFixture.CreateSection("x"), 
                IniDataFixture.CreateSection("s")
            };
            
            IniSection[] modifiedSections =
            {
                IniDataFixture.CreateSection("s"), 
                IniDataFixture.CreateSection("x"), 
                IniDataFixture.CreateSection("s")
            };

            IniData original = IniDataFixture.CreateIniData(originalSections);
            IniData modified = IniDataFixture.CreateIniData(modifiedSections);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged, Is.EqualTo(original));
            Assert.That(merged, Is.EqualTo(modified));
        }

        [Test]
        public void Merge_OriginalIsEmptyAndModifiedHasSectionsAndAddAddedSectionsIsTrue_ReturnsModified()
        {
            IniData original = IniDataFixture.CreateEmptyIniData();
            IniData modified = IniDataFixture.CreateIniData();

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.AddAddedSections = true;

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged, Is.EqualTo(modified));
        }

        [Test]
        public void Merge_OriginalIsEmptyAndModifiedHasSectionsAndAddAddedSectionsIsFalse_ReturnsEmpty()
        {
            IniData original = IniDataFixture.CreateEmptyIniData();
            IniData modified = IniDataFixture.CreateIniData();

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.AddAddedSections = false;

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged.Sections, Is.Empty);
        }

        [Test]
        public void Merge_OriginalHasSectionsAndModifiedIsEmptyAndRemoveRemovedSectionsIsTrue_ReturnsEmpty()
        {
            IniData original = IniDataFixture.CreateIniData();
            IniData modified = IniDataFixture.CreateEmptyIniData();

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.RemoveRemovedSections = true;

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged.Sections, Is.Empty);
        }

        [Test]
        public void Merge_OriginalHasSectionsAndModifiedIsEmptyAndRemoveRemovedSectionsIsFalse_ReturnsOriginal()
        {
            IniData original = IniDataFixture.CreateIniData();
            IniData modified = IniDataFixture.CreateEmptyIniData();

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.RemoveRemovedSections = false;

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged, Is.EqualTo(original));
        }

        [Test]
        public void Merge_OriginalAndModifiedPropertiesAreEmpty_ReturnsEmpty()
        {
            IniSection originalSection = IniDataFixture.CreateEmptySection();
            IniSection modifiedSection = IniDataFixture.CreateEmptySection();

            IniData original = IniDataFixture.CreateIniData(originalSection);
            IniData modified = IniDataFixture.CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.Empty);
        }

        [Test]
        public void Merge_OriginalAndModifiedPropertiesAreEqual_ReturnsEqual()
        {
            IniSection originalSection = IniDataFixture.CreateSection();
            IniSection modifiedSection = IniDataFixture.CreateSection();

            IniData original = IniDataFixture.CreateIniData(originalSection);
            IniData modified = IniDataFixture.CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.EqualTo(originalSection.Properties));
            Assert.That(mergedSection.Properties, Is.EqualTo(modifiedSection.Properties));
        }

        [Test]
        public void Merge_OriginalAndModifiedPropertiesAreEqual_ReturnsNewInstances()
        {
            IniSection originalSection = IniDataFixture.CreateSection();
            IniSection modifiedSection = IniDataFixture.CreateSection();

            IniData original = IniDataFixture.CreateIniData(originalSection);
            IniData modified = IniDataFixture.CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties.All(m => !originalSection.Properties.Any(s => ReferenceEquals(m, s))));
            Assert.That(mergedSection.Properties.All(m => !modifiedSection.Properties.Any(t => ReferenceEquals(m, t))));
        }

        [Test]
        public void Merge_OriginalAndModifiedHaveMixedCasedPropertyKeys_ReturnsOriginal()
        {
            IniProperty[] originalProperties = IniDataFixture.CreateProperties("Property");
            IniProperty[] modifiedProperties = IniDataFixture.CreateProperties("PROPERTY");

            IniSection originalSection = IniDataFixture.CreateSection(originalProperties);
            IniSection modifiedSection = IniDataFixture.CreateSection(modifiedProperties);

            IniData original = IniDataFixture.CreateIniData(originalSection);
            IniData modified = IniDataFixture.CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.EqualTo(originalSection.Properties));
            Assert.That(mergedSection.Properties.Select(x => x.Key), Is.EqualTo(originalSection.Properties.Select(x => x.Key)));
        }

        [Test]
        public void Merge_OriginalAndModifiedHaveDuplicatePropertyKeys_ReturnsEqual()
        {
            IniProperty[] originalProperties =
            {
                IniDataFixture.CreateProperty("p"), 
                IniDataFixture.CreateProperty("x"), 
                IniDataFixture.CreateProperty("p")
            };
            
            IniProperty[] modifiedProperties =
            {
                IniDataFixture.CreateProperty("p"), 
                IniDataFixture.CreateProperty("x"), 
                IniDataFixture.CreateProperty("p")
            };

            IniSection originalSection = IniDataFixture.CreateSection(originalProperties);
            IniSection modifiedSection = IniDataFixture.CreateSection(modifiedProperties);

            IniData original = IniDataFixture.CreateIniData(originalSection);
            IniData modified = IniDataFixture.CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.EqualTo(originalSection.Properties));
            Assert.That(mergedSection.Properties, Is.EqualTo(modifiedSection.Properties));
        }

        [Test]
        public void Merge_OriginalAndModifiedPropertyValuesAreNotEqual_ReturnsModified()
        {
            IniProperty[] originalProperties = IniDataFixture.CreateProperties();
            IniProperty[] modifiedProperties = originalProperties.Select(
                (p, i) => IniDataFixture.CreateProperty(p.Key.ToUpper(), $"new value {i}", $"new comment {i}", i)).ToArray();

            IniSection originalSection = IniDataFixture.CreateSection(originalProperties);
            IniSection modifiedSection = IniDataFixture.CreateSection(modifiedProperties);

            IniData original = IniDataFixture.CreateIniData(originalSection);
            IniData modified = IniDataFixture.CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            // Only the property value should be updated; preserve the name, comment & line number
            Assert.That(mergedSection.Properties.Select(x => x.Value), Is.EqualTo(modifiedProperties.Select(x => x.Value)));
            Assert.That(mergedSection.Properties.Select(x => x.Key), Is.EqualTo(originalProperties.Select(x => x.Key)));
            Assert.That(mergedSection.Properties.Select(x => x.Comment), Is.EqualTo(originalProperties.Select(x => x.Comment)));
            Assert.That(mergedSection.Properties.Select(x => x.LineNumber), Is.EqualTo(originalProperties.Select(x => x.LineNumber)));
        }

        [Test]
        public void Merge_OriginalIsEmptyAndModifiedHasPropertiesAndAddAddedPropertiesIsTrue_ReturnsModified()
        {
            IniSection originalSection = IniDataFixture.CreateEmptySection();
            IniSection modifiedSection = IniDataFixture.CreateSection();

            IniData original = IniDataFixture.CreateIniData(originalSection);
            IniData modified = IniDataFixture.CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.AddAddedProperties = true;

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.EqualTo(modifiedSection.Properties));
        }

        [Test]
        public void Merge_OriginalIsEmptyAndModifiedHasPropertiesAndAndAddAddedPropertiesIsFalse_ReturnsEmpty()
        {
            IniSection originalSection = IniDataFixture.CreateEmptySection();
            IniSection modifiedSection = IniDataFixture.CreateSection();

            IniData original = IniDataFixture.CreateIniData(originalSection);
            IniData modified = IniDataFixture.CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.AddAddedProperties = false;

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.Empty);
        }

        [Test]
        public void Merge_OriginalHasPropertiesAndModifiedIsEmptyAndRemoveRemoveRemovedPropertiesIsTrue_ReturnsEmpty()
        {
            IniSection originalSection = IniDataFixture.CreateSection();
            IniSection modifiedSection = IniDataFixture.CreateEmptySection();

            IniData original = IniDataFixture.CreateIniData(originalSection);
            IniData modified = IniDataFixture.CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.RemoveRemovedProperties = true;

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.Empty);
        }

        [Test]
        public void Merge_OriginalHasPropertiesAndModifiedIsEmptyAndRemoveRemovedPropertiesIsFalse_ReturnsOriginal()
        {
            IniSection originalSection = IniDataFixture.CreateSection();
            IniSection modifiedSection = IniDataFixture.CreateEmptySection();

            IniData original = IniDataFixture.CreateIniData(originalSection);
            IniData modified = IniDataFixture.CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.RemoveRemovedProperties = false;

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.EqualTo(originalSection.Properties));
        }

        private static IniMerger CreateIniMerger()
        {
            return new IniMerger();
        }

        private static IniMergeConfiguration CreateConfiguration()
        {
            return new IniMergeConfiguration();
        }
    }
}