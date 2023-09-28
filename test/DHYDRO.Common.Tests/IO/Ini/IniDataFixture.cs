using System.Linq;
using DHYDRO.Common.IO.Ini;

namespace DHYDRO.Common.Tests.IO.Ini
{
    public static class IniDataFixture
    {
        public static IniData CreateEmptyIniData()
        {
            return new IniData();
        }

        public static IniData CreateIniData()
        {
            var iniData = new IniData();
            iniData.AddMultipleSections(CreateSections());
            return iniData;
        }
        
        public static IniData CreateIniDataWithSingleSection()
        {
            var iniData = new IniData();
            iniData.AddSection(CreateSection());
            return iniData;
        }

        public static IniData CreateIniData(params IniSection[] sections)
        {
            var iniData = new IniData();
            iniData.AddMultipleSections(sections);
            return iniData;
        }

        public static IniSection[] CreateSections(string namePrefix = "section")
        {
            return Enumerable.Range(1, 3).Select(i => CreateSection($"{namePrefix}{i}", i)).ToArray();
        }

        public static IniSection CreateSection(string name = "section", int lineNumber = 0)
        {
            var section = new IniSection(name) { LineNumber = lineNumber };
            section.AddMultipleProperties(CreateProperties());
            return section;
        }

        public static IniSection CreateSection(params IniProperty[] properties)
        {
            var section = new IniSection("section");
            section.AddMultipleProperties(properties);
            return section;
        }

        public static IniSection CreateEmptySection()
        {
            return new IniSection("section");
        }

        public static IniProperty[] CreateProperties(string keyPrefix = "property")
        {
            return Enumerable.Range(1, 3).Select(i => CreateProperty($"{keyPrefix}{i}", $"value{i}")).ToArray();
        }

        public static IniProperty CreateProperty(string key = "property", string value = "value", string comment = "comment", int lineNumber = 0)
        {
            return new IniProperty(key, value, comment) { LineNumber = lineNumber };
        }
    }
}