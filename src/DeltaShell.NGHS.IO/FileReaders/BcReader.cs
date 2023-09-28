using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils.Extensions;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public class BcReader : IniReader, IBcReader
    {
        public IList<BcIniSection> ReadBcFile(string bcFile)
        {
            var content = new List<BcIniSection>();
            OpenInputFile(bcFile);
            try
            {
                string line;
                BcIniSection currentIniSection = null;
                string sectionName = null;
                while ((line = GetNextLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line)) continue; // Skip white-space characters.

                    if (IsNewIniSection(line, ref sectionName))
                    {
                        currentIniSection = new BcIniSection(sectionName);
                        currentIniSection.Section.LineNumber = LineNumber;
                        content.Add(currentIniSection);
                        continue;
                    }
                    if (currentIniSection == null) continue;

                    if (line.Contains('='))
                    {
                        var fields = GetKeyValueComment(line);
                        var property = new IniProperty
                        (
                            fields[0],
                            fields[1],
                            fields[2]) {
                            LineNumber = LineNumber
                        };
                        
                        if (property.IsKeyEqualTo("quantity")) 
                        {
                            currentIniSection.Table.Add(new BcQuantityData(property));
                        }
                        else if (property.Key == "unit")
                        {
                            currentIniSection.Table.Last().Unit = property;
                        }
                        else
                        {
                            currentIniSection.Section.AddProperty(property);
                        }
                    }
                    else
                    {
                        var tableRow = line.SplitOnEmptySpace();
                        
                        for(int i = 0; i < currentIniSection.Table.Count; i++)
                        {
                            currentIniSection.Table[i].Values.Add(tableRow[i]);
                        }
                    }
                }
            }
            finally
            {
                CloseInputFile();
            }

            return content;
        }
    }
}
