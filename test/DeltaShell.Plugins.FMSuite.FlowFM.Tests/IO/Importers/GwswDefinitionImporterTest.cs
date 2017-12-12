using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class GwswDefinitionImporterTest: GwswImporterTestHelper
    {
        #region Gwsw Import Elements

        [Test]
        public void TestImportFromDefinitionFileCreatesAllSortOfElementsInNetwork()
        {
            ImportFromDefinitionFileAndCheckFeatures();
        }

        [Test]
        public void TestImportFromDefinitionFileAndCheckGwswUseCaseImportsAllSewerConnectionsCorrectly()
        {
            var network = ImportFromDefinitionFileAndCheckFeatures();
            var numberOfSewerConnectionsInGwsw = 97;
            Assert.IsNotNull(network);
            Assert.IsNotNull(network.SewerConnections);

            var repeadedSewerConnections = network.SewerConnections.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            Assert.IsEmpty(repeadedSewerConnections, string.Format("Repeated compartments entries. {0}", String.Concat(repeadedSewerConnections.Select(cmp => cmp.Name + " "))));

            var sewerConnectionsWithoutPlaceholders = network.SewerConnections.Where(sc => sc.Source != null && sc.Target != null).ToList();
            Assert.AreEqual(numberOfSewerConnectionsInGwsw, sewerConnectionsWithoutPlaceholders.Count);

            //CheckPipes
            var numberOfPipes = 81;
            Assert.AreEqual(numberOfPipes, network.Pipes.Count(), "Not all pipes were found.");

            //CheckPumps
            var numberOfPumps = 8;
            Assert.AreEqual(numberOfPumps, network.Pumps.Count(), "Not all pumps were found.");

            //CheckOrifices
            var numberOfOrifices = 2;
            Assert.AreEqual(numberOfOrifices, sewerConnectionsWithoutPlaceholders.Count(sc => (sc as SewerConnection).IsOrifice()), "Not all orifices were found.");

            //Check sewer profiles
            var expectedNumberOfSewerProfiles = 41;
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(expectedNumberOfSewerProfiles));
        }

        [Test]
        public void TestImportFromDefinitionFileAndCheckCheckGwswUseCaseImportsAllCompartmentsCorrectly()
        {
            var network = ImportFromDefinitionFileAndCheckFeatures();
            var numberOfManholesInGwsw = 76;
            Assert.IsNotNull(network);

            //CheckManholes
            Assert.IsNotNull(network.Manholes);
            var repeatedManholes = network.Manholes.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            Assert.IsEmpty(repeatedManholes, string.Format("Repeated manhole entries. {0}", String.Concat(repeatedManholes.Select(cmp => cmp.Name + " "))));

            var manholesWithoutPlaceholders = network.Manholes.Where(mh => mh.Compartments.Any()).ToList();
            Assert.AreEqual(numberOfManholesInGwsw, manholesWithoutPlaceholders.Count);

            //Check compartments
            var compartments = manholesWithoutPlaceholders.SelectMany(mh => mh.Compartments).ToList();
            var repeatedCompartments = compartments.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            Assert.IsEmpty(repeatedCompartments, string.Format("Repeated compartments entries. {0}", String.Concat(repeatedCompartments.Select(cmp => cmp.Name + " "))));

            var numberOfCompartmentsInGwsw = 90;
            Assert.AreEqual(numberOfCompartmentsInGwsw, compartments.Count, "Not all compartments were found.");

            //CheckOutlets
            var numberOfOutlets = 4;
            Assert.AreEqual(numberOfOutlets, compartments.Count(cmp => cmp.IsOutletCompartment()), "Not all outlets were found.");
        }
        
        [Test]
        public void ImportGwswDefinitionFileLoadsAsManyAttributesAsLinesInTheCsv()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            var gwswImporter = new GwswBaseImporter();
            var attributeTable = gwswImporter.ImportGwswDefinitionFile(filePath);
            Assert.IsNotNull(attributeTable);

            var numberOfLines = File.ReadAllLines(filePath).Length - 1; // we should not include the header
            var rowsCount = attributeTable.Rows.Count;
            Assert.AreEqual(numberOfLines, rowsCount, string.Format("The imported .csv file {0}, contains {1} rows but {2} were imported.", filePath, numberOfLines, rowsCount));

            Assert.AreEqual(numberOfLines, gwswImporter.GwswAttributesDefinition.Count, string.Format("The imported .csv file {0}, contains {1} rows but {2} were imported.", filePath, numberOfLines, rowsCount));
        }

        [Test]
        public void CreateGwswDataTableFromDefinitionFileThenImportFilesAsDataTables()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            // Import Csv Definition.
            var gwswImporter = new GwswBaseImporter();
            var definitionTable = gwswImporter.ImportGwswDefinitionFile(filePath);
            Assert.IsNotNull(definitionTable);

            var attributeList = new List<GwswAttributeType>();

            foreach (DataRow row in definitionTable.Rows)
            {
                var attributeFile = row.ItemArray[0].ToString();
                var attributeElement = row.ItemArray[1].ToString();
                var attributeName = row.ItemArray[2].ToString();
                var attributeCode = row.ItemArray[3].ToString();
                var attributeCodeInt = row.ItemArray[4].ToString();
                var attributeDefinition = row.ItemArray[5].ToString();
                var attributeType = row.ItemArray[6].ToString();

                var attribute = new GwswAttributeType()
                {
                    Name = attributeName,
                    ElementName = attributeElement,
                    Definition = attributeDefinition,
                    FileName = attributeFile,
                    Key = attributeCodeInt,
                    LocalKey = attributeCode,
                    AttributeType = FMParser.GetClrType(attributeName, attributeType, ref attributeDefinition,
                        attributeFile, definitionTable.Rows.IndexOf(row)),
                };

                attributeList.Add(attribute);
            }

            Assert.IsTrue(attributeList.Count > 0, string.Format("Attributes found {0}", attributeList.Count));

            var uniqueFileList = attributeList.GroupBy(i => i.FileName).Select(grp => grp.Key).ToList();
            Assert.AreEqual(uniqueFileList.Count, 13, "Mismatch on found filenames.");

            var csvSettings = new CsvSettings
            {
                Delimiter = csvCommaDelimeter,
                FirstRowIsHeader = true,
                SkipEmptyLines = true
            };

            var importedTables = new List<DataTable>();

            //Read each one of the files.
            foreach (var fileName in uniqueFileList)
            {
                var directoryName = Path.GetDirectoryName(filePath);
                var elementFilePath = Path.Combine(directoryName, fileName);
                Assert.IsTrue(File.Exists(elementFilePath), string.Format("Could not find file {0}", elementFilePath));

                //Import file elements based on their attributes
                var fileAttributes = attributeList.Where(at => at.FileName.Equals(fileName)).ToList();
                var fileColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>();
                //Create column mapping
                fileAttributes.ForEach(
                    attr =>
                        fileColumnMapping.Add(
                            new CsvRequiredField(attr.Key, attr.AttributeType),
                            new CsvColumnInfo(fileAttributes.IndexOf(attr), CultureInfo.InvariantCulture)));

                var mapping = new CsvMappingData()
                {
                    Settings = csvSettings,
                    FieldToColumnMapping = fileColumnMapping
                };

                var importedElementTable = GwswFileImportAsDataTableWorksCorrectly(elementFilePath, mapping, true);
                importedTables.Add(importedElementTable);
            }

            Assert.AreEqual(uniqueFileList.Count, importedTables.Count, string.Format("Not all files were imported correctly."));
        }

        #endregion

        #region Helpers

        private static IHydroNetwork ImportFromDefinitionFileAndCheckFeatures()
        {
            var model = new WaterFlowFMModel();
            var network = model.Network;
            Assert.IsFalse(network.Pipes.Any());
            Assert.IsFalse(network.Manholes.Any());
            Assert.IsFalse(network.SharedCrossSectionDefinitions.Any());
            Assert.IsFalse(network.Pumps.Any());

            var gwswImporter = new GwswDefinitionImporter();
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            try
            {
                gwswImporter.ImportItem(filePath, model);
            }
            catch (Exception e)
            {
                Assert.Fail("While importing an exception was thrown {0}", e.Message);
            }

            Assert.IsTrue(network.Manholes.Any());
            Assert.IsTrue(network.SharedCrossSectionDefinitions.Any());
            Assert.IsTrue(network.Pipes.Any()); //There are some pipes defined within the verbinding.csv
            Assert.IsTrue(network.Pumps.Any()); //There are some pumps defined within the verbinding.csv

            return network;
        }

        #endregion
    }
}