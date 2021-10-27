using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO
{
    public class DelftBcReader : DelftIniReader
    {
        public IList<IDelftBcCategory> ReadDelftBcFile(string bcFile)
        {
            var content = new List<IDelftBcCategory>();
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

                    if (IsNewCategory(line, ref categoryName))
                    {
                        currentCategory = new DelftBcCategory(categoryName) { LineNumber = LineNumber };
                        content.Add(currentCategory);
                        continue;
                    }
                    if (currentCategory == null) continue;

                    if (line.Contains('='))
                    {
                        var fields = GetKeyValueComment(line);
                        var property = new DelftIniProperty
                        {
                            Name = fields[0],
                            Value = fields[1],
                            Comment = fields[2],
                            LineNumber = LineNumber
                        };

                        // TODO: use const strings (currently in WFM1D.IO BoundaryRegion.cs - these will need to be moved, we should look at this again when we begin working on the readers)
                        if (property.Name == "quantity") 
                        {
                            currentCategory.Table.Add(new DelftBcQuantityData(property));
                        }
                        else if (property.Name == "unit")
                        {
                            currentCategory.Table.Last().Unit = property;
                        }
                        else
                        {
                            currentCategory.Properties.Add(property);
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
