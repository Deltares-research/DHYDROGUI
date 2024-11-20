using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using log4net;

namespace DeltaShell.NGHS.IO.FileWriters
{
    /// <summary>
    /// Class for writing .bc files.
    /// </summary>
    public sealed class BcWriter : IBcWriter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BcWriter));
        
        private readonly IFileSystem fileSystem;
        private const int indentationLevel = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="BcWriter"/> class.
        /// </summary>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileSystem"/> is <c>null</c>.</exception>
        public BcWriter(IFileSystem fileSystem)
        {
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.fileSystem = fileSystem;
        }
        
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="iniSections"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="iniFile"/> is <c>null</c> or white space.</exception>
        public void WriteBcFile(IEnumerable<BcIniSection> iniSections, string iniFile, bool appendToFile = false)
        {
            Ensure.NotNull(iniSections, nameof(iniSections));
            Ensure.NotNullOrWhiteSpace(iniFile, nameof(iniFile));

            IEnumerable<IniSection> iniSectionsToWrite = CreateIniSections(iniSections);
            WriteIniFile(iniFile, iniSectionsToWrite, appendToFile);
        }

        private static IEnumerable<IniSection> CreateIniSections(IEnumerable<BcIniSection> bcIniSections)
        {
            var iniSections = new List<IniSection>();

            foreach (BcIniSection bcIniSection in bcIniSections)
            {
                IniSection iniSection = ConvertToIniSection(bcIniSection);
                iniSections.Add(iniSection);
            }

            return iniSections;
        }

        private static IniSection ConvertToIniSection(BcIniSection bcIniSection)
        {
            IniSection iniSection = bcIniSection.Section;

            if (!bcIniSection.Table.Any())
            {
                return iniSection;
            }

            foreach (IBcQuantityData bcQuantityData in bcIniSection.Table)
            {
                iniSection.AddProperty(bcQuantityData.Quantity);
                iniSection.AddProperty(bcQuantityData.Unit);
            }

            IniProperty lastProperty = iniSection.Properties
                                                 .OrderBy(p => p.LineNumber)
                                                 .Last();

            string tableAsString = GetTableDataAsString(bcIniSection.Table);
            string newLastValue = string.Join(Environment.NewLine, lastProperty.Value, tableAsString);
            lastProperty.Value = newLastValue;

            return iniSection;
        }

        private static string GetTableDataAsString(IList<IBcQuantityData> table)
        {
            var tableRows = new StringBuilder[table[0].Values.Count]; // there will be as many rows as there are quantity values
            InitializeStringBuilderArray(tableRows);

            foreach (IBcQuantityData bcQuantityData in table)
            {
                InsertValuesInTableRows(bcQuantityData, tableRows); // each row will have as many elements as there are quantities
            }

            return string.Join(Environment.NewLine, tableRows.Select(sb => sb.ToString()));
        }

        private static void InitializeStringBuilderArray(StringBuilder[] tableRows)
        {
            for (var i = 0; i < tableRows.Length; i++)
            {
                tableRows[i] = new StringBuilder(new string(' ', indentationLevel));
            }
        }

        private static void InsertValuesInTableRows(IBcQuantityData bcQuantityData, StringBuilder[] tableRows)
        {
            for (var i = 0; i < bcQuantityData.Values.Count; i++)
            {
                tableRows[i].Append(bcQuantityData.Values[i]);
                tableRows[i].Append(" ");
            }
        }

        private void WriteIniFile(string targetFile, IEnumerable<IniSection> iniSections, bool appendToFile)
        {
            var iniFormatter = new IniFormatter()
            {
                Configuration =
                {
                    WriteComments = false,
                    PropertyIndentationLevel = indentationLevel,
                }
            };

            var iniData = new IniData();
            iniData.AddMultipleSections(iniSections);

            CreateDirectoryIfNotExists(targetFile);

            log.InfoFormat(Resources.BcWriter_WriteIniFile_Writing_boundary_conditions_to__0__, targetFile);
            FileMode fileMode = appendToFile ? FileMode.Append : FileMode.Create;
            using (Stream iniStream = fileSystem.File.Open(targetFile,fileMode))
            {
                iniFormatter.Format(iniData, iniStream);
            }
        }

        private void CreateDirectoryIfNotExists(string targetFile)
        {
            string directory = fileSystem.Path.GetDirectoryName(targetFile);

            if (!string.IsNullOrEmpty(directory))
            {
                fileSystem.CreateDirectoryIfNotExists(directory);
            }
        }
    }
}