using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.DelftIniObjects;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests
{
    [TestFixture]
    public class DelftIniMergerTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            DelftIniMerger iniMerger = CreateIniMerger();

            Assert.That(iniMerger.Source, Is.Not.Null);
            Assert.That(iniMerger.Target, Is.Not.Null);
            Assert.That(iniMerger.AddAddedCategories, Is.True);
            Assert.That(iniMerger.AddAddedProperties, Is.True);
            Assert.That(iniMerger.RemoveRemovedCategories, Is.True);
            Assert.That(iniMerger.RemoveRemovedProperties, Is.True);
        }

        [Test]
        public void Constructor_SourceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => CreateIniMerger(null, Array.Empty<DelftIniCategory>()));
        }

        [Test]
        public void Constructor_TargetIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => CreateIniMerger(Array.Empty<DelftIniCategory>(), null));
        }

        [Test]
        public void Source_SetToNull_ThrowsArgumentNullException()
        {
            DelftIniMerger iniMerger = CreateIniMerger();

            Assert.Throws<ArgumentNullException>(() => iniMerger.Source = null);
        }

        [Test]
        public void Target_SetToNull_ThrowsArgumentNullException()
        {
            DelftIniMerger iniMerger = CreateIniMerger();

            Assert.Throws<ArgumentNullException>(() => iniMerger.Target = null);
        }

        [Test]
        public void Merge_SourceAndTargetCategoriesAreEmpty_ReturnsEmptyCategories()
        {
            DelftIniMerger iniMerger = CreateIniMerger();

            IEnumerable<DelftIniCategory> merged = iniMerger.Merge();

            Assert.That(merged, Is.Empty);
        }

        [Test]
        public void Merge_SourceAndTargetCategoriesAreEqual_ReturnsEqualCategories()
        {
            IEnumerable<DelftIniCategory> source = CreateCategories();
            IEnumerable<DelftIniCategory> target = CreateCategories();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            IReadOnlyCollection<DelftIniCategory> merged = iniMerger.Merge().ToArray();

            Assert.That(merged, Is.EqualTo(source).Using<DelftIniCategory>(IsCategoryEqual));
            Assert.That(merged, Is.EqualTo(target).Using<DelftIniCategory>(IsCategoryEqual));
        }

        [Test]
        public void Merge_SourceAndTargetCategoriesAreEqual_ReturnsDifferentCategoryObjects()
        {
            IEnumerable<DelftIniCategory> source = CreateCategories();
            IEnumerable<DelftIniCategory> target = CreateCategories();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            IReadOnlyCollection<DelftIniCategory> merged = iniMerger.Merge().ToArray();

            Assert.That(merged, Is.Not.SameAs(source));
            Assert.That(merged, Is.Not.SameAs(target));
        }

        [Test]
        public void Merge_SourceAndTargetHaveMixedCasedCategoryIdentifiers_ReturnsTargetCategories()
        {
            IReadOnlyCollection<DelftIniCategory> source = CreateCategories().ToArray();
            IReadOnlyCollection<DelftIniCategory> target = CreateCategories().ToArray();

            source.ForEach(c => c.Id = c.Id.ToUpper());
            target.ForEach(c => c.Id = c.Id.ToLower());

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            IEnumerable<DelftIniCategory> merged = iniMerger.Merge();

            Assert.That(merged, Is.EqualTo(target).Using<DelftIniCategory>(IsCategoryEqual));
        }

        [Test]
        public void Merge_SourceAndTargetHaveDuplicateCategoryIdentifiers_ThrowsInvalidOperationException()
        {
            IReadOnlyCollection<DelftIniCategory> source = CreateCategories().ToArray();
            IReadOnlyCollection<DelftIniCategory> target = CreateCategories().ToArray();

            source.ForEach(c => c.Id = "category");
            target.ForEach(c => c.Id = "category");

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            Assert.Throws<InvalidOperationException>(() => iniMerger.Merge());
        }

        [Test]
        public void Merge_SourceAndTargetHaveDuplicateCategoryNamesAndUniqueIdentifiers_ReturnsEqualCategories()
        {
            IReadOnlyCollection<DelftIniCategory> source = CreateCategories().ToArray();
            IReadOnlyCollection<DelftIniCategory> target = CreateCategories().ToArray();

            source.ForEach(c => c.Name = "category");
            target.ForEach(c => c.Name = "category");

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            IReadOnlyCollection<DelftIniCategory> merged = iniMerger.Merge().ToArray();

            Assert.That(merged, Is.EqualTo(source).Using<DelftIniCategory>(IsCategoryEqual));
            Assert.That(merged, Is.EqualTo(target).Using<DelftIniCategory>(IsCategoryEqual));
        }

        [Test]
        public void Merge_SourceHasCategoriesAndTargetIsEmptyAndAddAddedCategoriesIsTrue_ReturnsSourceCategories()
        {
            IEnumerable<DelftIniCategory> source = CreateCategories();
            IEnumerable<DelftIniCategory> target = Enumerable.Empty<DelftIniCategory>();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);
            iniMerger.AddAddedCategories = true;

            IEnumerable<DelftIniCategory> merged = iniMerger.Merge();

            Assert.That(merged, Is.EqualTo(source).Using<DelftIniCategory>(IsCategoryEqual));
        }

        [Test]
        public void Merge_SourceHasCategoriesAndTargetIsEmptyAndAddAddedCategoriesIsFalse_ReturnsEmptyCategories()
        {
            IEnumerable<DelftIniCategory> source = CreateCategories();
            IEnumerable<DelftIniCategory> target = Enumerable.Empty<DelftIniCategory>();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);
            iniMerger.AddAddedCategories = false;

            IEnumerable<DelftIniCategory> merged = iniMerger.Merge();

            Assert.That(merged, Is.Empty);
        }

        [Test]
        public void Merge_SourceIsEmptyAndTargetHasCategoriesAndRemoveRemovedCategoriesIsTrue_ReturnsEmptyCategories()
        {
            IEnumerable<DelftIniCategory> source = Array.Empty<DelftIniCategory>();
            IEnumerable<DelftIniCategory> target = CreateCategories();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);
            iniMerger.RemoveRemovedCategories = true;

            IEnumerable<DelftIniCategory> merged = iniMerger.Merge();

            Assert.That(merged, Is.Empty);
        }

        [Test]
        public void Merge_SourceIsEmptyAndTargetHasCategoriesAndRemoveRemovedCategoriesIsFalse_ReturnsTargetCategories()
        {
            IEnumerable<DelftIniCategory> source = Array.Empty<DelftIniCategory>();
            IEnumerable<DelftIniCategory> target = CreateCategories();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);
            iniMerger.RemoveRemovedCategories = false;

            IEnumerable<DelftIniCategory> merged = iniMerger.Merge();

            Assert.That(merged, Is.EqualTo(target).Using<DelftIniCategory>(IsCategoryEqual));
        }

        [Test]
        public void Merge_SourceAndTargetPropertiesAreEmpty_ReturnsEmptyProperties()
        {
            DelftIniCategory source = CreateEmptyCategory();
            DelftIniCategory target = CreateEmptyCategory();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            DelftIniCategory merged = iniMerger.Merge().First();

            Assert.That(merged.Properties, Is.Empty);
        }

        [Test]
        public void Merge_SourceAndTargetPropertiesAreEqual_ReturnsEqualProperties()
        {
            DelftIniCategory source = CreateCategory();
            DelftIniCategory target = CreateCategory();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            DelftIniCategory merged = iniMerger.Merge().First();

            Assert.That(merged.Properties, Is.EqualTo(source.Properties).Using<DelftIniProperty>(IsPropertyEqual));
            Assert.That(merged.Properties, Is.EqualTo(target.Properties).Using<DelftIniProperty>(IsPropertyEqual));
        }

        [Test]
        public void Merge_SourceAndTargetPropertiesAreEqual_ReturnsDifferentPropertyObjects()
        {
            DelftIniCategory source = CreateCategory();
            DelftIniCategory target = CreateCategory();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            DelftIniCategory merged = iniMerger.Merge().First();

            Assert.That(merged.Properties, Is.Not.SameAs(source.Properties));
            Assert.That(merged.Properties, Is.Not.SameAs(target.Properties));
        }

        [Test]
        public void Merge_SourceAndTargetHaveMixedCasedPropertyIdentifiers_ReturnsTargetProperties()
        {
            DelftIniCategory source = CreateCategory();
            DelftIniCategory target = CreateCategory();

            source.Properties.ForEach(c => c.Id = c.Id.ToUpper());
            target.Properties.ForEach(c => c.Id = c.Id.ToLower());

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            DelftIniCategory merged = iniMerger.Merge().First();

            Assert.That(merged.Properties, Is.EqualTo(target.Properties).Using<DelftIniProperty>(IsPropertyEqual));
        }

        [Test]
        public void Merge_SourceAndTargetHaveDuplicatePropertyIdentifiers_ThrowsInvalidOperationException()
        {
            DelftIniCategory source = CreateCategory();
            DelftIniCategory target = CreateCategory();

            source.Properties.ForEach(c => c.Id = "property");
            target.Properties.ForEach(c => c.Id = "property");

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            Assert.Throws<InvalidOperationException>(() => iniMerger.Merge());
        }

        [Test]
        public void Merge_SourceAndTargetHaveDuplicatePropertyNamesAndUniqueIdentifiers_ReturnsEqualProperties()
        {
            DelftIniCategory source = CreateCategory();
            DelftIniCategory target = CreateCategory();

            source.Properties.ForEach(c => c.Name = "property");
            target.Properties.ForEach(c => c.Name = "property");

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            DelftIniCategory merged = iniMerger.Merge().First();

            Assert.That(merged.Properties, Is.EqualTo(source.Properties).Using<DelftIniProperty>(IsPropertyEqual));
            Assert.That(merged.Properties, Is.EqualTo(target.Properties).Using<DelftIniProperty>(IsPropertyEqual));
        }

        [Test]
        public void Merge_SourceAndTargetPropertyValuesAreNotEqual_ReturnsSourceValues()
        {
            IReadOnlyCollection<DelftIniProperty> targetProperties = CreateProperties().ToArray();
            IReadOnlyCollection<DelftIniProperty> sourceProperties = targetProperties.Select(
                (p, i) => CreateProperty(p.Name.ToUpper(), $"new value {i}", $"new comment {i}", i)).ToArray();

            DelftIniCategory source = CreateCategory(sourceProperties);
            DelftIniCategory target = CreateCategory(targetProperties);

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            DelftIniCategory merged = iniMerger.Merge().First();

            // only the property value should be updated; preserve the name, comment & line number
            Assert.That(merged.Properties.Select(x => x.Value), Is.EqualTo(sourceProperties.Select(x => x.Value)));
            Assert.That(merged.Properties.Select(x => x.Name), Is.EqualTo(targetProperties.Select(x => x.Name)));
            Assert.That(merged.Properties.Select(x => x.Comment), Is.EqualTo(targetProperties.Select(x => x.Comment)));
            Assert.That(merged.Properties.Select(x => x.LineNumber), Is.EqualTo(targetProperties.Select(x => x.LineNumber)));
        }

        [Test]
        public void Merge_SourceHasPropertiesAndTargetPropertiesIsEmptyAndAddAddedPropertiesIsTrue_ReturnsSourceProperties()
        {
            DelftIniCategory source = CreateCategory();
            DelftIniCategory target = CreateEmptyCategory();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);
            iniMerger.AddAddedProperties = true;

            DelftIniCategory merged = iniMerger.Merge().First();

            Assert.That(merged.Properties, Is.EqualTo(source.Properties).Using<DelftIniProperty>(IsPropertyEqual));
        }

        [Test]
        public void Merge_SourceHasPropertiesAndTargetPropertiesIsEmptyAndAddAddedPropertiesIsFalse_ReturnsEmptyProperties()
        {
            DelftIniCategory source = CreateCategory();
            DelftIniCategory target = CreateEmptyCategory();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);
            iniMerger.AddAddedProperties = false;

            DelftIniCategory merged = iniMerger.Merge().First();

            Assert.That(merged.Properties, Is.Empty);
        }

        [Test]
        public void Merge_SourcePropertiesIsEmptyAndTargetHasPropertiesAndRemoveRemoveRemovedPropertiesIsTrue_ReturnsEmptyProperties()
        {
            DelftIniCategory source = CreateEmptyCategory();
            DelftIniCategory target = CreateCategory();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);
            iniMerger.RemoveRemovedProperties = true;

            DelftIniCategory merged = iniMerger.Merge().First();

            Assert.That(merged.Properties, Is.Empty);
        }

        [Test]
        public void Merge_SourcePropertiesIsEmptyAndTargetHasPropertiesAndRemoveRemovedPropertiesIsFalse_ReturnsTargetProperties()
        {
            DelftIniCategory source = CreateEmptyCategory();
            DelftIniCategory target = CreateCategory();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);
            iniMerger.RemoveRemovedProperties = false;

            DelftIniCategory merged = iniMerger.Merge().First();

            Assert.That(merged.Properties, Is.EqualTo(target.Properties).Using<DelftIniProperty>(IsPropertyEqual));
        }

        [Test]
        public void TryMerge_SourceAndTargetHaveDuplicateCategoryIdentifiers_ReturnsFalseAndEmptyCategories()
        {
            IReadOnlyCollection<DelftIniCategory> source = CreateCategories().ToArray();
            IReadOnlyCollection<DelftIniCategory> target = CreateCategories().ToArray();

            source.ForEach(c => c.Id = "category");
            target.ForEach(c => c.Id = "category");

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            bool isMerged = iniMerger.TryMerge(out IEnumerable<DelftIniCategory> merged);

            Assert.That(isMerged, Is.False);
            Assert.That(merged, Is.Empty);
        }

        [Test]
        public void TryMerge_SourceAndTargetCategoriesAreEqual_ReturnsTrueAndEqualCategories()
        {
            IEnumerable<DelftIniCategory> source = CreateCategories();
            IEnumerable<DelftIniCategory> target = CreateCategories();

            DelftIniMerger iniMerger = CreateIniMerger(source, target);

            bool isMerged = iniMerger.TryMerge(out IEnumerable<DelftIniCategory> merged);

            IReadOnlyCollection<DelftIniCategory> mergeResult = merged.ToArray();

            Assert.That(isMerged, Is.True);
            Assert.That(mergeResult, Is.EqualTo(source).Using<DelftIniCategory>(IsCategoryEqual));
            Assert.That(mergeResult, Is.EqualTo(target).Using<DelftIniCategory>(IsCategoryEqual));
        }

        private static DelftIniMerger CreateIniMerger()
        {
            return new DelftIniMerger();
        }

        private static DelftIniMerger CreateIniMerger(DelftIniCategory source, DelftIniCategory target)
        {
            return CreateIniMerger(new[] { source }, new[] { target });
        }

        private static DelftIniMerger CreateIniMerger(IEnumerable<DelftIniCategory> source, IEnumerable<DelftIniCategory> target)
        {
            return new DelftIniMerger(source, target);
        }

        private static IEnumerable<DelftIniCategory> CreateCategories()
        {
            return Enumerable.Range(1, 3).Select(i => CreateCategory($"category {i}", i));
        }

        private static IEnumerable<DelftIniProperty> CreateProperties()
        {
            return Enumerable.Range(1, 3).Select(i => CreateProperty($"property {i}", $"value {i}"));
        }

        private static DelftIniCategory CreateCategory(string name = "category", int lineNumber = 0)
        {
            var category = new DelftIniCategory(name, lineNumber);
            category.AddProperties(CreateProperties());
            return category;
        }

        private static DelftIniCategory CreateCategory(IEnumerable<DelftIniProperty> properties)
        {
            var category = new DelftIniCategory("category");
            category.AddProperties(properties);
            return category;
        }

        private static DelftIniCategory CreateEmptyCategory()
        {
            return CreateCategory(Array.Empty<DelftIniProperty>());
        }

        private static DelftIniProperty CreateProperty(string name, string value, string comment = "comment", int lineNumber = 0)
        {
            return new DelftIniProperty(name, value, comment, lineNumber);
        }

        private static bool IsCategoryEqual(DelftIniCategory category1, DelftIniCategory category2)
        {
            return Equals(category1.Id, category2.Id) &&
                   Equals(category1.Name, category2.Name) &&
                   Equals(category1.LineNumber, category2.LineNumber) &&
                   Equals(category1.Properties.Count(), category2.Properties.Count()) &&
                   Enumerable.Zip(category1.Properties, category2.Properties, IsPropertyEqual).All(_ => _);
        }

        private static bool IsPropertyEqual(DelftIniProperty property1, DelftIniProperty property2)
        {
            return Equals(property1.Id, property2.Id) &&
                   Equals(property1.Name, property2.Name) &&
                   Equals(property1.Value, property2.Value) &&
                   Equals(property1.Comment, property2.Comment) &&
                   Equals(property1.LineNumber, property2.LineNumber);
        }
    }
}