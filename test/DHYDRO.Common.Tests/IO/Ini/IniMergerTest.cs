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
            IniData modified = CreateIniData();

            IniMerger iniMerger = CreateIniMerger();

            Assert.Throws<ArgumentNullException>(() => iniMerger.Merge(null, modified));
        }

        [Test]
        public void Merge_ModifiedIsNull_ThrowsArgumentNullException()
        {
            IniData original = CreateIniData();

            IniMerger iniMerger = CreateIniMerger();

            Assert.Throws<ArgumentNullException>(() => iniMerger.Merge(original, null));
        }

        [Test]
        public void Merge_OriginalAndModifiedIniDataAreEmpty_ReturnsEmpty()
        {
            IniData original = CreateEmptyIniData();
            IniData modified = CreateEmptyIniData();

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged.Sections, Is.Empty);
        }

        [Test]
        public void Merge_OriginalAndModifiedIniDataAreEqual_ReturnsEqual()
        {
            IniData original = CreateIniData();
            IniData modified = CreateIniData();

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged, Is.EqualTo(original));
            Assert.That(merged, Is.EqualTo(modified));
        }

        [Test]
        public void Merge_OriginalAndModifiedIniDataAreEqual_ReturnsNewInstances()
        {
            IniData original = CreateIniData();
            IniData modified = CreateIniData();

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged.Sections.All(m => !original.Sections.Any(s => ReferenceEquals(m, s))));
            Assert.That(merged.Sections.All(m => !modified.Sections.Any(t => ReferenceEquals(m, t))));
        }

        [Test]
        public void Merge_OriginalAndModifiedHaveMixedCasedSectionNames_ReturnsOriginal()
        {
            IniSection[] originalSections = CreateSections("Section");
            IniSection[] modifiedSections = CreateSections("SECTION");

            IniData original = CreateIniData(originalSections);
            IniData modified = CreateIniData(modifiedSections);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged, Is.EqualTo(original));
            Assert.That(merged.Sections.Select(x => x.Name), Is.EqualTo(original.Sections.Select(x => x.Name)));
        }

        [Test]
        public void Merge_OriginalAndModifiedHaveDuplicateSectionNames_ReturnsEqual()
        {
            IniSection[] originalSections = new[] { CreateSection("s"), CreateSection("x"), CreateSection("s") };
            IniSection[] modifiedSections = new[] { CreateSection("s"), CreateSection("x"), CreateSection("s") };

            IniData original = CreateIniData(originalSections);
            IniData modified = CreateIniData(modifiedSections);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged, Is.EqualTo(original));
            Assert.That(merged, Is.EqualTo(modified));
        }

        [Test]
        public void Merge_OriginalIsEmptyAndModifiedHasSectionsAndAddAddedSectionsIsTrue_ReturnsModified()
        {
            IniData original = CreateEmptyIniData();
            IniData modified = CreateIniData();

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.AddAddedSections = true;

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged, Is.EqualTo(modified));
        }

        [Test]
        public void Merge_OriginalIsEmptyAndModifiedHasSectionsAndAddAddedSectionsIsFalse_ReturnsEmpty()
        {
            IniData original = CreateEmptyIniData();
            IniData modified = CreateIniData();

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.AddAddedSections = false;

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged.Sections, Is.Empty);
        }

        [Test]
        public void Merge_OriginalHasSectionsAndModifiedIsEmptyAndRemoveRemovedSectionsIsTrue_ReturnsEmpty()
        {
            IniData original = CreateIniData();
            IniData modified = CreateEmptyIniData();

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.RemoveRemovedSections = true;

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged.Sections, Is.Empty);
        }

        [Test]
        public void Merge_OriginalHasSectionsAndModifiedIsEmptyAndRemoveRemovedSectionsIsFalse_ReturnsOriginal()
        {
            IniData original = CreateIniData();
            IniData modified = CreateEmptyIniData();

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.RemoveRemovedSections = false;

            IniData merged = iniMerger.Merge(original, modified);

            Assert.That(merged, Is.EqualTo(original));
        }

        [Test]
        public void Merge_OriginalAndModifiedPropertiesAreEmpty_ReturnsEmpty()
        {
            IniSection originalSection = CreateEmptySection();
            IniSection modifiedSection = CreateEmptySection();

            IniData original = CreateIniData(originalSection);
            IniData modified = CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.Empty);
        }

        [Test]
        public void Merge_OriginalAndModifiedPropertiesAreEqual_ReturnsEqual()
        {
            IniSection originalSection = CreateSection();
            IniSection modifiedSection = CreateSection();

            IniData original = CreateIniData(originalSection);
            IniData modified = CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.EqualTo(originalSection.Properties));
            Assert.That(mergedSection.Properties, Is.EqualTo(modifiedSection.Properties));
        }

        [Test]
        public void Merge_OriginalAndModifiedPropertiesAreEqual_ReturnsNewInstances()
        {
            IniSection originalSection = CreateSection();
            IniSection modifiedSection = CreateSection();

            IniData original = CreateIniData(originalSection);
            IniData modified = CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties.All(m => !originalSection.Properties.Any(s => ReferenceEquals(m, s))));
            Assert.That(mergedSection.Properties.All(m => !modifiedSection.Properties.Any(t => ReferenceEquals(m, t))));
        }

        [Test]
        public void Merge_OriginalAndModifiedHaveMixedCasedPropertyKeys_ReturnsOriginal()
        {
            IniProperty[] originalProperties = CreateProperties("Property");
            IniProperty[] modifiedProperties = CreateProperties("PROPERTY");

            IniSection originalSection = CreateSection(originalProperties);
            IniSection modifiedSection = CreateSection(modifiedProperties);

            IniData original = CreateIniData(originalSection);
            IniData modified = CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.EqualTo(originalSection.Properties));
            Assert.That(mergedSection.Properties.Select(x => x.Key), Is.EqualTo(originalSection.Properties.Select(x => x.Key)));
        }

        [Test]
        public void Merge_OriginalAndModifiedHaveDuplicatePropertyKeys_ReturnsEqual()
        {
            IniProperty[] originalProperties = new[] { CreateProperty("p"), CreateProperty("x"), CreateProperty("p") };
            IniProperty[] modifiedProperties = new[] { CreateProperty("p"), CreateProperty("x"), CreateProperty("p") };

            IniSection originalSection = CreateSection(originalProperties);
            IniSection modifiedSection = CreateSection(modifiedProperties);

            IniData original = CreateIniData(originalSection);
            IniData modified = CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.EqualTo(originalSection.Properties));
            Assert.That(mergedSection.Properties, Is.EqualTo(modifiedSection.Properties));
        }

        [Test]
        public void Merge_OriginalAndModifiedPropertyValuesAreNotEqual_ReturnsModified()
        {
            IniProperty[] originalProperties = CreateProperties();
            IniProperty[] modifiedProperties = originalProperties.Select(
                (p, i) => CreateProperty(p.Key.ToUpper(), $"new value {i}", $"new comment {i}", i)).ToArray();

            IniSection originalSection = CreateSection(originalProperties);
            IniSection modifiedSection = CreateSection(modifiedProperties);

            IniData original = CreateIniData(originalSection);
            IniData modified = CreateIniData(modifiedSection);

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
            IniSection originalSection = CreateEmptySection();
            IniSection modifiedSection = CreateSection();

            IniData original = CreateIniData(originalSection);
            IniData modified = CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.AddAddedProperties = true;

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.EqualTo(modifiedSection.Properties));
        }

        [Test]
        public void Merge_OriginalIsEmptyAndModifiedHasPropertiesAndAndAddAddedPropertiesIsFalse_ReturnsEmpty()
        {
            IniSection originalSection = CreateEmptySection();
            IniSection modifiedSection = CreateSection();

            IniData original = CreateIniData(originalSection);
            IniData modified = CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.AddAddedProperties = false;

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.Empty);
        }

        [Test]
        public void Merge_OriginalHasPropertiesAndModifiedIsEmptyAndRemoveRemoveRemovedPropertiesIsTrue_ReturnsEmpty()
        {
            IniSection originalSection = CreateSection();
            IniSection modifiedSection = CreateEmptySection();

            IniData original = CreateIniData(originalSection);
            IniData modified = CreateIniData(modifiedSection);

            IniMerger iniMerger = CreateIniMerger();
            iniMerger.Configuration.RemoveRemovedProperties = true;

            IniData merged = iniMerger.Merge(original, modified);
            IniSection mergedSection = merged.Sections.First();

            Assert.That(mergedSection.Properties, Is.Empty);
        }

        [Test]
        public void Merge_OriginalHasPropertiesAndModifiedIsEmptyAndRemoveRemovedPropertiesIsFalse_ReturnsOriginal()
        {
            IniSection originalSection = CreateSection();
            IniSection modifiedSection = CreateEmptySection();

            IniData original = CreateIniData(originalSection);
            IniData modified = CreateIniData(modifiedSection);

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

        private static IniData CreateIniData()
        {
            var iniData = new IniData();
            iniData.AddMultipleSections(CreateSections());
            return iniData;
        }

        private static IniData CreateIniData(params IniSection[] sections)
        {
            var iniData = new IniData();
            iniData.AddMultipleSections(sections);
            return iniData;
        }

        private static IniData CreateEmptyIniData()
        {
            return new IniData();
        }

        private static IniSection[] CreateSections(string namePrefix = "section")
        {
            return Enumerable.Range(1, 3).Select(i => CreateSection($"{namePrefix} {i}", i)).ToArray();
        }

        private static IniSection CreateSection(string name = "section", int lineNumber = 0)
        {
            var section = new IniSection(name) { LineNumber = lineNumber };
            section.AddMultipleProperties(CreateProperties());
            return section;
        }

        private static IniSection CreateSection(params IniProperty[] properties)
        {
            var section = new IniSection("section");
            section.AddMultipleProperties(properties);
            return section;
        }

        private static IniSection CreateEmptySection()
        {
            return new IniSection("section");
        }

        private static IniProperty[] CreateProperties(string keyPrefix = "property")
        {
            return Enumerable.Range(1, 3).Select(i => CreateProperty($"{keyPrefix} {i}", $"value {i}")).ToArray();
        }

        private static IniProperty CreateProperty(string key = "property", string value = "", string comment = "comment", int lineNumber = 0)
        {
            return new IniProperty(key, value, comment) { LineNumber = lineNumber };
        }
    }
}