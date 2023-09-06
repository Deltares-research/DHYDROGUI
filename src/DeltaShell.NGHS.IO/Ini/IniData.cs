using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.IO.Ini
{
    /// <summary>
    /// Represents a collection of sections in an INI file.
    /// </summary>
    /// <remarks>
    /// This class encapsulates a collection of sections within an INI data structure.
    /// <para/>
    /// It's allowed to add multiple sections with the same name within the INI data.
    /// When using methods like <see cref="GetSection"/> the first section found with the specified name is operated upon.
    /// <para/>
    /// Section names are compared in a case-insensitive manner.
    /// </remarks>
    public sealed class IniData : IEquatable<IniData>
    {
        private readonly List<IniSection> sections;

        /// <summary>
        /// Initializes a new instance of the <see cref="IniData"/> class.
        /// </summary>
        public IniData()
        {
            sections = new List<IniSection>();

            Comment = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IniData"/> class with the same values as the specified data.
        /// </summary>
        /// <param name="other">The data to copy values from.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="other"/> is <c>null</c>.</exception>
        public IniData(IniData other)
        {
            Ensure.NotNull(other, nameof(other));

            sections = other.Sections
                            .Select(s => new IniSection(s.Name, s))
                            .ToList();

            Comment = other.Comment;
        }

        /// <summary>
        /// Gets or sets the comment associated with the INI data. The default value is an empty string.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets the sections within the INI data.
        /// </summary>
        public IEnumerable<IniSection> Sections => sections;

        /// <summary>
        /// Gets the number of sections within the INI data.
        /// </summary>
        public int SectionCount => sections.Count;

        /// <summary>
        /// Adds a new section with the specified name to the INI data.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <returns>The added <see cref="IniSection"/> instance.</returns>
        /// <exception cref="ArgumentException">When <paramref name="name"/> is <c>null</c> or empty.</exception>
        public IniSection AddSection(string name)
        {
            Ensure.NotNullOrEmpty(name, nameof(name));

            var section = new IniSection(name);
            sections.Add(section);

            return section;
        }

        /// <summary>
        /// Adds a section to the INI data.
        /// </summary>
        /// <param name="section">The section to add.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="section"/> is <c>null</c>.</exception>
        public void AddSection(IniSection section)
        {
            Ensure.NotNull(section, nameof(section));

            sections.Add(section);
        }

        /// <summary>
        /// Adds a collection of sections to the INI data.
        /// </summary>
        /// <param name="sectionsToAdd">The sections to add.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="sectionsToAdd"/> is <c>null</c>.</exception>
        public void AddMultipleSections(IEnumerable<IniSection> sectionsToAdd)
        {
            Ensure.NotNull(sectionsToAdd, nameof(sectionsToAdd));

            sections.AddRange(sectionsToAdd);
        }

        /// <summary>
        /// Determines whether the INI data contains a section with the specified name.
        /// </summary>
        /// <param name="name">The name of the section to locate in the data.</param>
        /// <returns><c>true</c> if a section with the specified name is found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is <c>null</c> or empty.</exception>
        public bool ContainsSection(string name)
        {
            Ensure.NotNullOrEmpty(name, nameof(name));

            return GetSection(name) != null;
        }

        /// <summary>
        /// Gets the first section found in the INI data with the specified name.
        /// </summary>
        /// <param name="name">The name of the section to retrieve.</param>
        /// <returns>The first <see cref="IniSection"/> with the specified name, or <c>null</c> if not found.</returns>
        /// <exception cref="ArgumentException">When <paramref name="name"/> is <c>null</c> or empty.</exception>
        public IniSection GetSection(string name)
        {
            Ensure.NotNullOrEmpty(name, nameof(name));

            return sections.Find(
                p => p.IsNameEqualTo(name));
        }

        /// <summary>
        /// Gets all sections in the INI data with the specified name.
        /// </summary>
        /// <param name="name">The name of the sections to retrieve.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all sections with the specified name.</returns>
        /// <exception cref="ArgumentException">When <paramref name="name"/> is <c>null</c> or empty.</exception>
        public IEnumerable<IniSection> GetAllSections(string name)
        {
            Ensure.NotNullOrEmpty(name, nameof(name));

            return sections.Where(
                p => p.IsNameEqualTo(name));
        }

        /// <summary>
        /// Removes the specified section from the INI data.
        /// </summary>
        /// <param name="section">The section to remove.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="section"/> is <c>null</c>.</exception>
        /// <remarks>Returns silently if the section was not found in the data.</remarks>
        public void RemoveSection(IniSection section)
        {
            Ensure.NotNull(section, nameof(section));

            sections.Remove(section);
        }

        /// <summary>
        /// Removes all sections with the specified name from the INI data.
        /// </summary>
        /// <param name="name">The name of sections to remove.</param>
        /// <exception cref="ArgumentException">When <paramref name="name"/> is <c>null</c> or empty.</exception>
        /// <remarks>Returns silently if no section with specified name was found in the data.</remarks>
        public void RemoveAllSections(string name)
        {
            Ensure.NotNullOrEmpty(name, nameof(name));

            RemoveAllSections(p => p.IsNameEqualTo(name));
        }

        /// <summary>
        /// Removes all sections that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="predicate">A delegate that defines the conditions of the sections to remove.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="predicate"/> is <c>null</c>.</exception>
        /// <remarks>Returns silently if no section matched the conditions defined by the specified predicate.</remarks>
        public void RemoveAllSections(Predicate<IniSection> predicate)
        {
            Ensure.NotNull(predicate, nameof(predicate));

            sections.RemoveAll(predicate);
        }

        /// <summary>
        /// Clears all sections from the INI data.
        /// </summary>
        public void Clear()
        {
            sections.Clear();
        }

        /// <summary>
        /// Renames all sections with the specified name.
        /// </summary>
        /// <param name="oldName">The name of the sections to be renamed.</param>
        /// <param name="newName">The name to assign to the sections.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="oldName"/> or <paramref name="newName"/> is <c>null</c> or empty.
        /// </exception>
        /// <remarks>Returns silently if no section with specified key was found in the data.</remarks>
        public void RenameSections(string oldName, string newName)
        {
            Ensure.NotNullOrEmpty(oldName, nameof(oldName));
            Ensure.NotNullOrEmpty(newName, nameof(newName));

            for (var i = 0; i < sections.Count; i++)
            {
                if (sections[i].IsNameEqualTo(oldName))
                {
                    sections[i] = new IniSection(newName, sections[i]);
                }
            }
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) ||
                   (obj is IniData other && Equals(other));
        }

        /// <inheritdoc/>
        public bool Equals(IniData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;

            return string.Equals(Comment, other.Comment, comparison) &&
                   Sections.SequenceEqual(other.Sections);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return 0; // no immutable fields for hashing
        }
    }
}