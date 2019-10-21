using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DeltaShell.NGHS.IO.Helpers
{
    /// <summary>
    /// Representation of a category in a .ini file.
    /// </summary>
    public class DelftIniCategory : IDelftIniCategory
    {
        /// <summary>
        /// Creates an instance of <see cref="DelftIniCategory"/>.
        /// </summary>
        /// <param name="categoryName"> The category name. </param>
        public DelftIniCategory(string categoryName)
        {
            Name = categoryName;
            Properties = new List<IDelftIniProperty>();
        }

        public DelftIniCategory(string categoryName, int lineNumber)
            : this(categoryName)
        {
            LineNumber = lineNumber;
        }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public IList<IDelftIniProperty> Properties { get; set; }

        /// <inheritdoc />
        public int LineNumber { get; }

        /// <inheritdoc />
        public string GetPropertyValue(string name, string defaultValue = null)
        {
            IDelftIniProperty prop = Properties.FirstOrDefault(p => p.Name == name);
            return prop != null
                       ? prop.Value
                       : defaultValue;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetPropertyValues(string name)
        {
            return Properties.Where(p => p.Name == name).Select(p => p.Value);
        }

        /// <inheritdoc />
        public void AddProperty(string name, string value, string comment = null)
        {
            Properties.Add(new DelftIniProperty(name, value, comment ?? ""));
        }

        /// <inheritdoc />
        public void AddProperty(string name, DateTime time, string comment = null,
                                string format = "yyyy-MM-dd HH:mm:ss")
        {
            AddProperty(name, time.ToString(format, CultureInfo.InvariantCulture), comment);
        }

        /// <inheritdoc />
        public void AddProperty(string name, double value, string comment = null, string format = "e7")
        {
            AddProperty(name, value.ToString(format, CultureInfo.InvariantCulture), comment);
        }

        /// <inheritdoc />
        public void AddProperty(string name, int value, string comment = null)
        {
            AddProperty(name, value.ToString(CultureInfo.InvariantCulture), comment);
        }

        /// <inheritdoc />
        public void SetProperty(string name, string value, string comment = null)
        {
            IDelftIniProperty prop = Properties.FirstOrDefault(p => p.Name == name);
            if (prop != null)
            {
                prop.Value = value;
                prop.Comment = comment;
            }
            else
            {
                AddProperty(name, value, comment);
            }
        }

        /// <inheritdoc />
        public void SetProperty(string name, double value, string comment = null, string format = "e7")
        {
            SetProperty(name, value.ToString(format, CultureInfo.InvariantCulture), comment);
        }
    }
}