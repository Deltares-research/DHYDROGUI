using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Csv.Importer;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests.IO.Importers
{
    public class GwswFileImporterTestHelper
    {
        protected static char csvCommaDelimeter = ',';
        protected static char csvSemiColonDelimeter = ';';

        protected static DataTable GwswFileImportAsDataTableWorksCorrectly(string filePath, CsvMappingData mappingData, bool continousTesting = false)
        {
            var importer = new GwswFileImporter(new DefinitionsProvider());
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

        protected static void CheckCsvIsImportedCorrectly(string filePath, CsvMappingData mappingData)
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

        protected static string GetFileAndCreateLocalCopy(string path)
        {
            var filePath = TestHelper.GetTestFilePath(path);
            Assert.IsTrue(File.Exists(filePath), string.Format("File {0} could not be located", filePath));

            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(filePath), string.Format("File {0} could not be located", filePath));

            return filePath;
        }

        protected static IList<GwswElement> GwswFileImportAsGwswElementsWorksCorrectly(GwswFileImporter importer, string filePath, bool continousTesting = false)
        {
            var importedElementList = importer.ImportGwswElementsFromGwswFiles(filePath);
            Assert.IsNotNull(importedElementList, string.Format("The .csv file {0}, could not be imported.", filePath));

            var fileAsStringList = File.ReadAllLines(filePath);
            var numberOfLines = fileAsStringList.Length - 1; // we should not include the header
            var rowsCount = importedElementList.SelectMany(e => e).Count();
            if (rowsCount != numberOfLines && !continousTesting)
            {
                //Check there are no repeated columns in the .CSV
                var repeatedElements = fileAsStringList[0].Split(GwswFileImporterTest.csvCommaDelimeter).GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key)
                    .ToList();

                /* If there were repeated columns, that table will simply not be imported, and the user will receive a log message saying so. 
                  as for this test, we are interested onto continuing, so we can ignore when a column is repeated. Its corresponding test will fail. */
                if (repeatedElements.Count == 0)
                {
                    Assert.AreEqual(numberOfLines, rowsCount, string.Format("The imported .csv file {0}, contains {1} rows but {2} were imported.", filePath, numberOfLines, rowsCount));
                }
            }

            return importedElementList.SelectMany(e => e).ToList();
        }

        protected static void CheckThatGwswAttributeValidationLogMessageIsReturned(string fileName, int lineNumber,
            string localKey, string key, GwswAttribute invalidAttribute)
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            invalidAttribute.IsValidAttribute(logHandler);
            logHandler.Received().ReportErrorFormat(Resources.GwswElementExtensions_LogInvalidAttribute_File__0___line__1___Column__2____3___contains_invalid_value___4___and_will_not_be_imported_
                                                   , fileName, lineNumber, localKey, key, invalidAttribute.ValueAsString);
        }
    }
}