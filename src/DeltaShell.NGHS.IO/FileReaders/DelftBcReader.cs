using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils.Extensions;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public class DelftBcReader : DelftIniReader, IDelftBcReader
    {
        public IList<DelftBcCategory> ReadDelftBcFile(string bcFile)
        {
            var content = new List<DelftBcCategory>();
            OpenInputFile(bcFile);
            try
            {
                string line;
                DelftBcCategory currentCategory = null;
                string categoryName = null;
                while ((line = GetNextLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line)) continue; // Skip white-space characters.

                    if (IsNewIniSection(line, ref categoryName))
                    {
                        currentCategory = new DelftBcCategory(categoryName);
                        currentCategory.Section.LineNumber = LineNumber;
                        content.Add(currentCategory);
                        continue;
                    }
                    if (currentCategory == null) continue;

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
                            currentCategory.Table.Add(new DelftBcQuantityData(property));
                        }
                        else if (property.Key == "unit")
                        {
                            currentCategory.Table.Last().Unit = property;
                        }
                        else
                        {
                            currentCategory.Section.AddProperty(property);
                        }
                    }
                    else
                    {
                        var tableRow = line.SplitOnEmptySpace();
                        
                        for(int i = 0; i < currentCategory.Table.Count; i++)
                        {
                            currentCategory.Table[i].Values.Add(tableRow[i]);
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
