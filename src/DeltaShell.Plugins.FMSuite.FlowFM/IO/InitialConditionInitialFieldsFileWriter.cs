using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.Helpers;


namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Filewriter for the initialFields.ini file.
    /// </summary>
    public static class InitialConditionInitialFieldsFileWriter
    {
        public static void WriteFile(string filename, 
            InitialConditionQuantity globalInitialConditionQuantity)
        {
            var categories = new List<DelftIniCategory>();

            categories.Add(CreateInitialConditionQuantityCategory(globalInitialConditionQuantity));

            if (File.Exists(filename)) File.Delete(filename);
            new IniFileWriter().WriteIniFile(categories, filename);
        }

        private static DelftIniCategory CreateInitialConditionQuantityCategory(
            InitialConditionQuantity globalInitialConditionQuantity)
        {
            var category = new DelftIniCategory(InitialConditionRegion.InitialConditionIniHeader);

            category.AddProperty(InitialConditionRegion.Quantity.Key, globalInitialConditionQuantity.ToString().ToLower());
            category.AddProperty(InitialConditionRegion.DataFile.Key, $"Initial{globalInitialConditionQuantity}.ini");
            category.AddProperty(InitialConditionRegion.DataFileType, "1dField");

            return category;
        }
        
        
    }
}