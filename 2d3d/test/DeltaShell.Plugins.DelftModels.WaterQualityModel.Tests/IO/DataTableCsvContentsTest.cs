using System;
using System.Collections.Generic;
using System.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class DataTableCsvContentsTest
    {
        [Test]
        public void DefaultConstructorExpectedValues()
        {
            // call
            var dataTableFileContents = new DataTableCsvContents();

            // assert
            CollectionAssert.IsEmpty(dataTableFileContents.DataRows);
            Assert.AreEqual(DataTableInterpolationType.Linear, dataTableFileContents.Interpolation);
            Assert.AreEqual(string.Empty, dataTableFileContents.Name);
            Assert.IsNull(dataTableFileContents.UseforIncludeFolderPath);
        }

        [Test]
        public void GetSubstanceUseforFileNameTest()
        {
            // setup
            var dataTableFileContents = new DataTableCsvContents {Name = "test"};

            // call
            string fileName = dataTableFileContents.GetSubstanceUseforFileName();

            // assert
            Assert.AreEqual("test.usefors", fileName);
        }

        [Test]
        [TestCase(DataTableInterpolationType.Linear)]
        [TestCase(DataTableInterpolationType.Block)]
        public void CreateDataTableDelwaqFormat_LinearData_CreateText(DataTableInterpolationType type)
        {
            // setup
            string useforIncludeFolderPath = Path.Combine("test", "hihi");
            var dataTableFileContents = new DataTableCsvContents
            {
                Name = "Some file name",
                Interpolation = type,
                UseforIncludeFolderPath = useforIncludeFolderPath
            };

            var locationA = new LocationData {Name = "A"};
            locationA.TimeDependentSubstanceData[new DateTime(2015, 3, 25, 10, 10, 0)] = new Dictionary<string, string>
            {
                {"foo", "1.1"},
                {"test", "3.3"}
            };
            locationA.TimeDependentSubstanceData[new DateTime(2015, 3, 25, 10, 14, 0)] = new Dictionary<string, string>
            {
                {"bar", "4.4"},
                {"test", "5.5"}
            };
            dataTableFileContents.DataRows.Add(locationA);

            var locationB = new LocationData {Name = "B"};
            locationB.TimeDependentSubstanceData[new DateTime(2015, 3, 25, 10, 33, 0)] = new Dictionary<string, string> {{"Not_in_SubstancesInFile", "1.1"}};
            dataTableFileContents.DataRows.Add(locationB);

            // call
            string delwaqFormattedFileContents = dataTableFileContents.CreateDataTableDelwaqFormat();

            // assert
            string expectedUseforsFilePath = Path.Combine(useforIncludeFolderPath, "Some file name.usefors");
            string expectedText =
                "DATA_ITEM" + Environment.NewLine +
                "'A'" + Environment.NewLine +
                "CONCENTRATIONS" + Environment.NewLine +
                string.Format("INCLUDE '{0}'", expectedUseforsFilePath) + Environment.NewLine +
                string.Format("TIME {0} DATA", type.ToString().ToUpper()) + Environment.NewLine +
                "'foo' 'test' 'bar'" + Environment.NewLine +
                "2015/03/25-10:10:00 1.1 3.3 -999" + Environment.NewLine +
                "2015/03/25-10:14:00 -999 5.5 4.4" + Environment.NewLine +
                Environment.NewLine +
                "DATA_ITEM" + Environment.NewLine +
                "'B'" + Environment.NewLine +
                "CONCENTRATIONS" + Environment.NewLine +
                string.Format("INCLUDE '{0}'", expectedUseforsFilePath) + Environment.NewLine +
                string.Format("TIME {0} DATA", type.ToString().ToUpper()) + Environment.NewLine +
                "'Not_in_SubstancesInFile'" + Environment.NewLine +
                "2015/03/25-10:33:00 1.1" + Environment.NewLine +
                Environment.NewLine;
            Assert.AreEqual(expectedText, delwaqFormattedFileContents);
        }

        [Test]
        public void SubstanceDataShouldBeSortedByDatetime()
        {
            string useforIncludeFolderPath = Path.Combine("test", "hihi");
            var type = DataTableInterpolationType.Linear;
            var dataTableFileContents = new DataTableCsvContents
            {
                Name = "Some file name",
                Interpolation = type,
                UseforIncludeFolderPath = useforIncludeFolderPath
            };

            var locationA = new LocationData {Name = "A"};
            locationA.TimeDependentSubstanceData[new DateTime(2015, 3, 25, 10, 10, 0)] = new Dictionary<string, string>
            {
                {"foo", "1.1"},
                {"test", "3.3"}
            };
            locationA.TimeDependentSubstanceData[new DateTime(2015, 3, 24, 10, 10, 0)] = new Dictionary<string, string>
            {
                {"bar", "4.4"},
                {"test", "5.5"}
            };
            locationA.TimeDependentSubstanceData[new DateTime(2015, 3, 26, 10, 10, 0)] = new Dictionary<string, string>
            {
                {"foo", "7.7"},
                {"bar", "8.8"}
            };
            dataTableFileContents.DataRows.Add(locationA);

            var locationB = new LocationData {Name = "B"};
            locationB.TimeDependentSubstanceData[new DateTime(2015, 3, 25, 10, 33, 0)] = new Dictionary<string, string> {{"Not_in_SubstancesInFile", "1.1"}};
            locationB.TimeDependentSubstanceData[new DateTime(2015, 3, 24, 10, 33, 0)] = new Dictionary<string, string> {{"Not_in_SubstancesInFile", "2.2"}};
            locationB.TimeDependentSubstanceData[new DateTime(2015, 3, 26, 10, 33, 0)] = new Dictionary<string, string> {{"Not_in_SubstancesInFile", "3.3"}};
            dataTableFileContents.DataRows.Add(locationB);

            // call
            string delwaqFormattedFileContents = dataTableFileContents.CreateDataTableDelwaqFormat();

            // assert
            string expectedUseforsFilePath = Path.Combine(useforIncludeFolderPath, "Some file name.usefors");

            string expectedText = string.Format(
                "DATA_ITEM{0}'A'{0}CONCENTRATIONS{0}INCLUDE '{1}'{0}TIME {2} DATA{0}" +
                "'bar' 'test' 'foo'{0}" +
                "2015/03/24-10:10:00 4.4 5.5 -999{0}" +
                "2015/03/25-10:10:00 -999 3.3 1.1{0}" +
                "2015/03/26-10:10:00 8.8 -999 7.7{0}{0}" +
                "DATA_ITEM{0}'B'{0}CONCENTRATIONS{0}INCLUDE '{1}'{0}TIME {2} DATA{0}" +
                "'Not_in_SubstancesInFile'{0}" +
                "2015/03/24-10:33:00 2.2{0}" +
                "2015/03/25-10:33:00 1.1{0}" +
                "2015/03/26-10:33:00 3.3{0}{0}",
                Environment.NewLine, expectedUseforsFilePath, type.ToString().ToUpper());

            Assert.AreEqual(expectedText, delwaqFormattedFileContents);
        }

        [Test]
        public void CreateDefaultSubstanceUseforContentsTest()
        {
            // setup
            var dataTableFileContents = new DataTableCsvContents();
            var locationA = new LocationData {Name = "A"};
            locationA.TimeDependentSubstanceData[new DateTime(2015, 3, 25, 10, 10, 0)] = new Dictionary<string, string>
            {
                {"foo", "1.1"},
                {"test", "3.3"}
            };
            locationA.TimeDependentSubstanceData[new DateTime(2015, 3, 25, 10, 14, 0)] = new Dictionary<string, string>
            {
                {"bar", "4.4"},
                {"test", "5.5"}
            };
            dataTableFileContents.DataRows.Add(locationA);

            var locationB = new LocationData {Name = "B"};
            locationB.TimeDependentSubstanceData[new DateTime(2015, 3, 25, 10, 33, 0)] = new Dictionary<string, string> {{"haha", "1.1"}};
            dataTableFileContents.DataRows.Add(locationB);

            // call
            string fileContents = dataTableFileContents.CreateDefaultSubstanceUseforContents();

            // assert
            string expected =
                "USEFOR 'foo' 'foo'" + Environment.NewLine +
                "USEFOR 'test' 'test'" + Environment.NewLine +
                "USEFOR 'bar' 'bar'" + Environment.NewLine +
                "USEFOR 'haha' 'haha'";
            Assert.AreEqual(expected, fileContents);
        }

        [Test]
        public void Read00AM_Becomes12AM_TOOLS22259()
        {
            var dataTableFileContents = new DataTableCsvContents()
            {
                UseforIncludeFolderPath = "lala",
                Name = "first"
            };

            var locationData = new LocationData() {Name = "loc 1"};

            // add one timeslot
            locationData.TimeDependentSubstanceData.Add(new DateTime(1999, 12, 16, 0, 0, 0),
                                                        new Dictionary<string, string>() {{"NH4", "10"}});
            dataTableFileContents.DataRows.Add(locationData);

            string content = dataTableFileContents.CreateDataTableDelwaqFormat();

            string expected = "DATA_ITEM" + Environment.NewLine +
                              "'loc 1'" + Environment.NewLine +
                              "CONCENTRATIONS" + Environment.NewLine +
                              "INCLUDE 'lala\\first.usefors'" + Environment.NewLine +
                              "TIME LINEAR DATA" + Environment.NewLine +
                              "'NH4'" + Environment.NewLine +
                              "1999/12/16-00:00:00 10" + Environment.NewLine + Environment.NewLine;

            Assert.AreEqual(expected, content);
        }
    }
}