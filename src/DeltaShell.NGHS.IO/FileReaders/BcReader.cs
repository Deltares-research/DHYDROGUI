using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using Deltares.Infrastructure.IO.Ini.Configuration;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders
{
    /// <summary>
    /// Class for reading .bc files.
    /// </summary>
    public class BcReader : IBcReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BcReader));
        
        private const string generalHeader = "General";
        private const string quantityKey = "quantity";
        private const string unitKey = "unit";

        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="BcReader"/> class.
        /// </summary>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileSystem"/> is <c>null</c>.</exception>
        public BcReader(IFileSystem fileSystem)
        {
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.fileSystem = fileSystem;
        }

        /// <inheritdoc/>
        public IEnumerable<BcIniSection> ReadBcFile(string bcFile)
        {
            Ensure.NotNullOrWhiteSpace(bcFile, nameof(bcFile));
            
            if (!fileSystem.File.Exists(bcFile))
            {
                throw new IOException($"File {bcFile} could not be found.");
            }

            IniData iniData = ReadIniFile(bcFile);

            return ConvertToBcIniSections(iniData.Sections);
        }

        private IniData ReadIniFile(string filepath)
        {
            IniParseConfiguration config = CreateBcParserConfig();
            var iniParser = new IniParser() { Configuration = config };

            log.InfoFormat(Resources.BcReader_ReadIniFile_Reading_boundary_conditions_from__0__, filepath);
            using (Stream stream = fileSystem.FileStream.New(filepath, FileMode.Open))
            {
                return iniParser.Parse(stream);
            }
        }

        private static IniParseConfiguration CreateBcParserConfig()
        {
            return new IniParseConfiguration() { AllowMultiLineValues = true };
        }

        private static IEnumerable<BcIniSection> ConvertToBcIniSections(IEnumerable<IniSection> iniSections)
        {
            var bcIniSections = new List<BcIniSection>();

            foreach (IniSection iniSection in iniSections)
            {
                BcIniSection bcIniSection = CreateBcIniSection(iniSection);
                bcIniSections.Add(bcIniSection);
            }

            return bcIniSections;
        }

        private static BcIniSection CreateBcIniSection(IniSection iniSection)
        {
            if (iniSection.IsNameEqualTo(generalHeader) || iniSection.PropertyCount == 0)
            {
                return new BcIniSection(iniSection);
            }

            var bcIniSection = new BcIniSection(iniSection.Name);
            foreach (IniProperty property in iniSection.Properties)
            {
                AddPropertyToBcIniSection(property, bcIniSection);
            }

            CreateBcIniSectionTable(iniSection, bcIniSection);

            return bcIniSection;
        }

        private static IEnumerable<string> ExtractDataBlockFromLastProperty(IniProperty lastProperty)
        {
            string[] values = lastProperty.Value.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            lastProperty.Value = values[0];

            return values;
        }

        private static void AddPropertyToBcIniSection(IniProperty property, BcIniSection bcIniSection)
        {
            if (property.IsKeyEqualTo(quantityKey))
            {
                bcIniSection.Table.Add(new BcQuantityData(property));
            }
            else if (property.IsKeyEqualTo(unitKey))
            {
                int lastIndex = bcIniSection.Table.Count - 1;
                bcIniSection.Table[lastIndex].Unit = property;
            }
            else
            {
                bcIniSection.Section.AddProperty(property);
            }
        }

        private static void CreateBcIniSectionTable(IniSection iniSection, BcIniSection bcIniSection)
        {
            IniProperty lastProperty = iniSection.Properties.Last();
            IEnumerable<string> values = ExtractDataBlockFromLastProperty(lastProperty);

            FillBcIniSectionTable(values, bcIniSection);
        }
        
        private static void FillBcIniSectionTable(IEnumerable<string> values, BcIniSection bcIniSection)
        {
            foreach (string row in values.Skip(1))
            {
                string[] rowValues = row.Split(new []{ ' '}, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < bcIniSection.Table.Count; i++)
                {
                    bcIniSection.Table[i].Values.Add(rowValues[i]);
                }
            }
        }
    }
}