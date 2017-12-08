using System;
using System.Data;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    public class GwswImporterTestHelper
    {
        protected static char csvCommaDelimeter = ',';
        protected static char csvSemiColonDelimeter = ';';

        public static DataTable GwswFileImportAsDataTableWorksCorrectly(string filePath, CsvMappingData mappingData, bool continousTesting = false)
        {
            var importer = new GwswBaseImporter();
            Assert.IsNotNull(importer);

            var importedTable = importer.ImportFileAsDataTable(filePath, mappingData);
            Assert.IsNotNull(importedTable, string.Format("The .csv file {0}, could not be imported.", filePath));

            var fileAsStringList = File.ReadAllLines(filePath);
            var numberOfLines = fileAsStringList.Length - 1; // we should not include the header
            var rowsCount = importedTable.Rows.Count;
            if (rowsCount != numberOfLines && !continousTesting)
            {
                //Check there are no repeated columns in the .CSV
                var repeatedElements = fileAsStringList[0].Split(csvCommaDelimeter).GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key)
                    .ToList();
                
                /* If there were repeated columns, that table will simply not be imported, and the user will receive a log message saying so. 
                  as for this test, we are interested onto continuing, so we can ignore when a column is repeated. Its corresponding test will fail. */
                if (repeatedElements.Count == 0)
                {
                    Assert.AreEqual(numberOfLines, rowsCount, string.Format("The imported .csv file {0}, contains {1} rows but {2} were imported.", filePath, numberOfLines, rowsCount));
                }
            }

            return importedTable;
        }

        public static void CheckCsvIsImportedCorrectly(string filePath, CsvMappingData mappingData)
        {
            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);

            try
            {
                var importedCsv = csvImporter.ImportCsv(filePath, mappingData);
                Assert.IsNotNull(importedCsv, string.Format("The .csv file {0}, could not be imported.", filePath));

                var numberOfLines = File.ReadAllLines(filePath).Length - 1; // we should not include the header
                var rowsCount = importedCsv.Rows.Count;
                Assert.AreEqual(numberOfLines, rowsCount, string.Format("The imported .csv file {0}, contains {1} rows but {2} were imported.", filePath, numberOfLines, rowsCount));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        public static string GetFileAndCreateLocalCopy(string path)
        {
            var filePath = TestHelper.GetTestFilePath(path);
            Assert.IsTrue(File.Exists(filePath), string.Format("File {0} could not be located", filePath));

            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(filePath), string.Format("File {0} could not be located", filePath));

            return filePath;
        }
    }
}