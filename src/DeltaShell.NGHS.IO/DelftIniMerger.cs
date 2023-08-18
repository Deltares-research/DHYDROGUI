using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;

namespace DeltaShell.NGHS.IO
{
    /// <summary>
    /// Merges the contents of two INI files.
    /// </summary>
    /// <remarks>
    /// By default any additions and/or removals are applied to the merge target. The merge behavior can be controlled through
    /// <see cref="AddAddedCategories"/>, <see cref="AddAddedProperties"/>, <see cref="RemoveRemovedCategories"/> and
    /// <see cref="RemoveRemovedProperties"/>.
    /// <para/>
    /// Merging INI files with duplicate category identifiers and/or duplicate property identifiers within the same category
    /// will result in a merge exception. Identifiers are compared by case-insensitive invariant culture.
    /// </remarks>
    public sealed class DelftIniMerger
    {
        private IEnumerable<DelftIniCategory> source;
        private IEnumerable<DelftIniCategory> target;
        private List<DelftIniCategory> merged;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelftIniMerger"/> class.
        /// </summary>
        public DelftIniMerger()
            : this(Enumerable.Empty<DelftIniCategory>(), Enumerable.Empty<DelftIniCategory>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelftIniMerger"/> class.
        /// </summary>
        /// <param name="source">The source to merge from.</param>
        /// <param name="target">The target to merge to.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="source"/> or <paramref name="target"/> is <c>null</c>.</exception>
        public DelftIniMerger(IEnumerable<DelftIniCategory> source, IEnumerable<DelftIniCategory> target)
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(target, nameof(target));

            this.source = source;
            this.target = target;
        }

        /// <summary>
        /// Gets or sets the source to merge from.
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="value"/> is <c>null</c>.</exception>
        public IEnumerable<DelftIniCategory> Source
        {
            get => source;
            set
            {
                Ensure.NotNull(value, nameof(value));
                source = value;
            }
        }

        /// <summary>
        /// Gets or sets the target to merge to.
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="value"/> is <c>null</c>.</exception>
        public IEnumerable<DelftIniCategory> Target
        {
            get => target;
            set
            {
                Ensure.NotNull(value, nameof(value));
                target = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether new categories should be added
        /// to the merge result. The default value is <c>true</c>.
        /// </summary>
        public bool AddAddedCategories { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that indicates whether new properties should be added
        /// to the merge result. The default value is <c>true</c>.
        /// </summary>
        public bool AddAddedProperties { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that indicates whether removed categories should be removed
        /// from the merge result. The default value is <c>true</c>.
        /// </summary>
        public bool RemoveRemovedCategories { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that indicates whether removed properties should be removed
        /// from the merge result. The default value is <c>true</c>.
        /// </summary>
        public bool RemoveRemovedProperties { get; set; } = true;

        /// <summary>
        /// Tries to merge the specified source and target.
        /// </summary>
        /// <param name="result">An enumerable collection containing the merged INI contents.</param>
        /// <returns><c>true</c> if merging was successful; otherwise <c>false</c>.</returns>
        public bool TryMerge(out IEnumerable<DelftIniCategory> result)
        {
            try
            {
                result = Merge();
                return true;
            }
            catch (InvalidOperationException)
            {
                result = Enumerable.Empty<DelftIniCategory>();
                return false;
            }
        }

        /// <summary>
        /// Merges the specified source and target.
        /// </summary>
        /// <returns>An enumerable collection containing the merged INI contents.</returns>
        /// <exception cref="InvalidOperationException">When duplicate categories or properties are found.</exception>
        public IEnumerable<DelftIniCategory> Merge()
        {
            InitializeMergedFromTarget();

            ProcessChanged();

            if (AddAddedCategories || AddAddedProperties)
            {
                ProcessAdded();
            }

            if (RemoveRemovedCategories || RemoveRemovedProperties)
            {
                ProcessRemoved();
            }

            return merged;
        }

        private void InitializeMergedFromTarget()
        {
            merged = new List<DelftIniCategory>(
                target.Select(c => new DelftIniCategory(c)));
        }

        private void ProcessChanged()
        {
            foreach (DelftIniCategory sourceCategory in source)
            {
                DelftIniCategory targetCategory = FindTargetCategory(sourceCategory);

                if (targetCategory != null)
                {
                    UpdateTargetProperties(sourceCategory, targetCategory);
                }
            }
        }

        private void UpdateTargetProperties(DelftIniCategory sourceCategory, DelftIniCategory targetCategory)
        {
            foreach (DelftIniProperty sourceProperty in sourceCategory.Properties)
            {
                DelftIniProperty targetProperty = FindTargetProperty(targetCategory, sourceProperty);

                if (targetProperty != null)
                {
                    UpdateTargetProperty(sourceProperty, targetProperty);
                }
            }
        }

        private void UpdateTargetProperty(DelftIniProperty sourceProperty, DelftIniProperty targetProperty)
        {
            targetProperty.Value = sourceProperty.Value;
        }

        private void ProcessAdded()
        {
            foreach (DelftIniCategory sourceCategory in source)
            {
                DelftIniCategory targetCategory = FindTargetCategory(sourceCategory);

                if (targetCategory == null && AddAddedCategories)
                {
                    AddTargetCategory(sourceCategory);
                }
                else if (targetCategory != null && AddAddedProperties)
                {
                    AddTargetProperties(sourceCategory, targetCategory);
                }
            }
        }

        private void AddTargetCategory(DelftIniCategory sourceCategory)
        {
            merged.Add(new DelftIniCategory(sourceCategory));
        }

        private void AddTargetProperties(DelftIniCategory sourceCategory, DelftIniCategory targetCategory)
        {
            foreach (DelftIniProperty sourceProperty in sourceCategory.Properties)
            {
                if (!targetCategory.ContainsPropertyWithId(sourceProperty.Id))
                {
                    targetCategory.AddProperty(new DelftIniProperty(sourceProperty));
                }
            }
        }

        private void ProcessRemoved()
        {
            for (int i = merged.Count - 1; i >= 0; i--)
            {
                DelftIniCategory targetCategory = merged[i];
                DelftIniCategory sourceCategory = FindSourceCategory(targetCategory);

                if (sourceCategory == null && RemoveRemovedCategories)
                {
                    RemoveTargetCategory(targetCategory);
                }
                else if (sourceCategory != null && RemoveRemovedProperties)
                {
                    RemoveTargetProperties(sourceCategory, targetCategory);
                }
            }
        }

        private void RemoveTargetCategory(DelftIniCategory targetCategory)
        {
            merged.Remove(targetCategory);
        }

        private void RemoveTargetProperties(DelftIniCategory sourceCategory, DelftIniCategory targetCategory)
        {
            targetCategory.RemoveAllPropertiesWhere(
                targetProperty => !sourceCategory.ContainsPropertyWithId(targetProperty.Id));
        }

        private DelftIniCategory FindSourceCategory(DelftIniCategory targetCategory)
        {
            return source.SingleOrDefault(
                sourceCategory => sourceCategory.IdEqualsTo(targetCategory.Id));
        }

        private DelftIniCategory FindTargetCategory(DelftIniCategory sourceCategory)
        {
            return merged.SingleOrDefault(
                targetCategory => targetCategory.IdEqualsTo(sourceCategory.Id));
        }

        private DelftIniProperty FindTargetProperty(DelftIniCategory targetCategory, DelftIniProperty sourceProperty)
        {
            return targetCategory.Properties.SingleOrDefault(
                targetProperty => targetProperty.IdEqualsTo(sourceProperty.Id));
        }
    }
}