using System;
using System.Collections.Generic;

namespace DeltaShell.NGHS.IO.Helpers
{
    public interface IDelftIniCategory
    {
        string Name { get; }
        IList<DelftIniProperty> Properties { get; set; }

        /// <summary>
        /// The line number where this category was read in the file.
        /// </summary>
        int LineNumber { get; set; }

        /// <summary>
        /// for unique property names, otherwise first!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        string GetPropertyValue(string name, string defaultValue = null);
        
        /// <summary>
        /// returns all values, ordered, for a property with multiplicity > 1
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEnumerable<string> GetPropertyValues(string name);

        void AddProperty(string name, string value, string comment = null);
        void AddProperty(string name, DateTime time, string comment = null, string format = "yyyy-MM-dd HH:mm:ss");
        void AddProperty(string name, IEnumerable<double> values, string comment = null, string format = "e7");
        void AddProperty(string name, double value,  string comment = null, string format = "e7");
        void AddProperty(string name, IEnumerable<int> values, string comment = null);
        void AddProperty(string name, int value, string comment = null);

        void SetProperty(string name, string value, string comment = null);
        void SetProperty(string name, double value, string comment = null, string format = null);
    }
}