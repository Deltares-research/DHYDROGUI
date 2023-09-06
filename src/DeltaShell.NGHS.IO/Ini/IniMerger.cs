using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.IO.Ini
{
    /// <summary>
    /// Merges the contents of two INI files.
    /// </summary>
    /// <remarks>
    /// The <see cref="Merge"/> method merges the <see cref="Original"/> and the <see cref="Modified"/> INI data into a
    /// new <see cref="IniData"/> instance. The <see cref="Original"/> and the <see cref="Modified"/> instances are not modified.
    /// <para/>
    /// By default any additions and/or removals are applied to the merge result. The merge behavior can be controlled through
    /// <see cref="AddAddedSections"/>, <see cref="AddAddedProperties"/>, <see cref="RemoveRemovedSections"/> and
    /// <see cref="RemoveRemovedProperties"/>.
    /// <para/>
    /// Merging INI files with duplicate section names and/or duplicate property keys within the same section is supported,
    /// with the assumption that these INI sections and/or properties have the same order.
    /// </remarks>
    public sealed class IniMerger
    {
        private IniData original;
        private IniData modified;

        private UniqueIniData source;
        private UniqueIniData target;

        /// <summary>
        /// Initializes a new instance of the <see cref="IniMerger"/> class.
        /// </summary>
        public IniMerger()
            : this(new IniData(), new IniData())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IniMerger"/> class.
        /// </summary>
        /// <param name="original">The original INI data to merge from.</param>
        /// <param name="modified">The modified INI data to merge with.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="original"/> or <paramref name="modified"/> is <c>null</c>.</exception>
        public IniMerger(IniData original, IniData modified)
        {
            Ensure.NotNull(original, nameof(original));
            Ensure.NotNull(modified, nameof(modified));

            this.original = original;
            this.modified = modified;
        }

        /// <summary>
        /// Gets or sets the original INI data to merge from.
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="value"/> is <c>null</c>.</exception>
        public IniData Original
        {
            get => original;
            set
            {
                Ensure.NotNull(value, nameof(value));
                original = value;
            }
        }

        /// <summary>
        /// Gets or sets the modified INI data to merge with.
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="value"/> is <c>null</c>.</exception>
        public IniData Modified
        {
            get => modified;
            set
            {
                Ensure.NotNull(value, nameof(value));
                modified = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether new sections should be added
        /// to the merge result. The default value is <c>true</c>.
        /// </summary>
        public bool AddAddedSections { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that indicates whether new properties should be added
        /// to the merge result. The default value is <c>true</c>.
        /// </summary>
        public bool AddAddedProperties { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that indicates whether removed sections should be removed
        /// from the merge result. The default value is <c>true</c>.
        /// </summary>
        public bool RemoveRemovedSections { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that indicates whether removed properties should be removed
        /// from the merge result. The default value is <c>true</c>.
        /// </summary>
        public bool RemoveRemovedProperties { get; set; } = true;

        /// <summary>
        /// Merges the contents of the <see cref="Original"/> and <see cref="Modified"/> INI data.
        /// </summary>
        /// <returns>A new <see cref="IniData"/> instance containing the merged INI contents.</returns>
        /// <remarks>
        /// The merging process takes the <see cref="Original"/> INI data as the base and applies modifications from
        /// the <see cref="Modified"/> INI data. The result is a new <see cref="IniData"/> instance representing
        /// the merged INI data contents.
        /// </remarks>
        public IniData Merge()
        {
            InitializeUniqueIniData();

            ProcessChanged();

            if (AddAddedSections || AddAddedProperties)
            {
                ProcessAdded();
            }

            if (RemoveRemovedSections || RemoveRemovedProperties)
            {
                ProcessRemoved();
            }

            return target.ToIniData();
        }

        /// <summary>
        /// Creates unique copies of the <see cref="Original"/> and <see cref="Modified"/> INI data for merging.
        /// </summary>
        private void InitializeUniqueIniData()
        {
            var originalCopy = new IniData(original);
            var modifiedCopy = new IniData(modified);

            source = new UniqueIniData(modifiedCopy);
            target = new UniqueIniData(originalCopy);
        }

        /// <summary>
        /// This method updates properties in the <see cref="target"/> INI data with values from the <see cref="source"/> INI data.
        /// Existing properties in the <see cref="target"/> INI data are updated to match the changes in the <see cref="source"/>
        /// INI data. If corresponding properties do not exist in the <see cref="target"/> INI data, no changes are made.
        /// </summary>
        private void ProcessChanged()
        {
            foreach (UniqueIniSection sourceSection in source.Sections)
            {
                UniqueIniSection targetSection = target.GetSection(sourceSection.Id);

                if (targetSection == null)
                {
                    continue;
                }

                foreach (UniqueIniProperty sourceProperty in sourceSection.Properties)
                {
                    UniqueIniProperty targetProperty = targetSection.GetProperty(sourceProperty.Id);

                    if (targetProperty != null)
                    {
                        targetProperty.Value = sourceProperty.Value;
                    }
                }
            }
        }

        /// <summary>
        /// This method adds new sections and properties from the <see cref="source"/> INI data into the <see cref="target"/>
        /// INI data. If a section or property already exists in the <see cref="target"/> INI data, it remains unaffected.
        /// The inclusion behavior is determined by <see cref="AddAddedSections"/> and <see cref="AddAddedProperties"/>.
        /// </summary>
        private void ProcessAdded()
        {
            foreach (UniqueIniSection sourceSection in source.Sections)
            {
                UniqueIniSection targetSection = target.GetSection(sourceSection.Id);

                if (targetSection == null && AddAddedSections)
                {
                    target.AddSection(sourceSection);
                }

                if (targetSection == null || !AddAddedProperties)
                {
                    continue;
                }

                foreach (UniqueIniProperty sourceProperty in sourceSection.Properties)
                {
                    if (!targetSection.ContainsProperty(sourceProperty.Id))
                    {
                        targetSection.AddProperty(sourceProperty);
                    }
                }
            }
        }

        /// <summary>
        /// This method removes sections and properties from the <see cref="target"/> INI data that are not present in the
        /// <see cref="source"/> INI data. The removal behavior is determined by <see cref="RemoveRemovedSections"/>
        /// and <see cref="RemoveRemovedProperties"/>.
        /// </summary>
        private void ProcessRemoved()
        {
            for (int i = target.Count - 1; i >= 0; i--)
            {
                UniqueIniSection targetSection = target.SectionAt(i);
                UniqueIniSection sourceSection = source.GetSection(targetSection.Id);

                if (sourceSection == null && RemoveRemovedSections)
                {
                    target.RemoveSection(targetSection.Id);
                }

                if (sourceSection != null && RemoveRemovedProperties)
                {
                    targetSection.RemoveAllProperties(
                        targetProperty => !sourceSection.ContainsProperty(targetProperty.Id));
                }
            }
        }

        /// <summary>
        /// Represents a duplicate-free container for INI data sections during merging.
        /// </summary>
        private sealed class UniqueIniData
        {
            private readonly IniData iniData;
            private readonly Dictionary<string, UniqueIniSection> sections;

            public UniqueIniData(IniData iniData)
            {
                this.iniData = iniData;

                sections = CreateUniqueSections();
            }

            public int Count
                => sections.Count;

            public IEnumerable<UniqueIniSection> Sections
                => sections.Values;

            private Dictionary<string, UniqueIniSection> CreateUniqueSections()
                => iniData.Sections
                          .Select(CreateUniqueSection)
                          .ToDictionary(x => x.Id);

            private UniqueIniSection CreateUniqueSection(IniSection section, int index)
                => new UniqueIniSection($"{section.Name.ToLower()}_{GetCounter(section, index)}", section);

            private int GetCounter(IniSection section, int index)
                => iniData.Sections.Take(index + 1).Count(e => e.IsNameEqualTo(section.Name));

            public UniqueIniSection GetSection(string id)
                => sections.TryGetValue(id, out UniqueIniSection section) ? section : null;

            public UniqueIniSection SectionAt(int index)
                => sections.Values.ElementAt(index);

            public void AddSection(UniqueIniSection section)
                => sections.Add(section.Id, section);

            public void RemoveSection(string id)
                => sections.Remove(id);

            public IniData ToIniData()
            {
                iniData.Clear();
                iniData.AddMultipleSections(
                    Sections.Select(x => x.ToIniSection()));
                return iniData;
            }
        }

        /// <summary>
        /// Represents a duplicate-free container for INI data properties during merging.
        /// </summary>
        private sealed class UniqueIniSection
        {
            private readonly IniSection section;
            private readonly Dictionary<string, UniqueIniProperty> properties;

            public UniqueIniSection(string id, IniSection section)
            {
                Id = id;

                this.section = section;
                properties = CreateUniqueProperties();
            }

            public string Id { get; }

            public IEnumerable<UniqueIniProperty> Properties
                => properties.Values;

            private Dictionary<string, UniqueIniProperty> CreateUniqueProperties() =>
                section.Properties
                       .Select(CreateUniqueProperty)
                       .ToDictionary(x => x.Id);

            private UniqueIniProperty CreateUniqueProperty(IniProperty property, int index)
                => new UniqueIniProperty($"{property.Key.ToLower()}_{GetCounter(property, index)}", property);

            private int GetCounter(IniProperty property, int index)
                => section.Properties.Take(index + 1).Count(e => e.IsKeyEqualTo(property.Key));

            public bool ContainsProperty(string id)
                => properties.ContainsKey(id);

            public UniqueIniProperty GetProperty(string id)
                => properties.TryGetValue(id, out UniqueIniProperty property) ? property : null;

            public void AddProperty(UniqueIniProperty property)
                => properties.Add(property.Id, property);

            public void RemoveAllProperties(Predicate<UniqueIniProperty> predicate)
                => properties.Where(kvp => predicate(kvp.Value)).ToArray()
                             .ForEach(kvp => properties.Remove(kvp.Key));

            public IniSection ToIniSection()
            {
                section.Clear();
                section.AddMultipleProperties(
                    Properties.Select(x => x.ToIniProperty()));
                return section;
            }
        }

        /// <summary>
        /// Represents an INI data property with ensured uniqueness within its parent section during merging.
        /// </summary>
        private sealed class UniqueIniProperty
        {
            private readonly IniProperty property;

            public UniqueIniProperty(string id, IniProperty property)
            {
                Id = id;

                this.property = property;
            }

            public string Id { get; }

            public string Value
            {
                get => property.Value;
                set => property.Value = value;
            }

            public IniProperty ToIniProperty()
                => property;
        }
    }
}