using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Plugins.ImportExport.Sobek.Tests.Builders;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class SobekRRBoundaryConditionsImporterTest
    {
        [Test]
        public void GivenTimeSeriesData_WhenImport_ThenBoundaryDataShouldBeTimeSeriesData()
        {
            //Arrange
            string testDirectory = TestHelper.GetTestDataDirectory() + @"\SobekRRBoundaryConditionsImporter\TimesSeriesBoundaryConditions\";
            using (var temp = new TemporaryDirectory())
            {
                string directory = temp.CopyDirectoryToTempDirectory(testDirectory);
                string filePath = Path.Combine(directory, "BoundaryConditions.bc");
                var importer = new SobekRRBoundaryConditionsImporter
                {
                    IsActive = true,
                    PathSobek = filePath
                };

                const string catchmentName = "Catchment_1D_1";
                using (RainfallRunoffModel rainfallRunoffModel = RainfallRunoffModelBuilder.Start().WithUnpavedCatchmentWithName(catchmentName)
                                                                                           .Build())
                {
                    var unpavedData = rainfallRunoffModel.ModelData.First() as UnpavedData;
                    importer.TargetObject = rainfallRunoffModel;

                    //Act
                    importer.Import();

                    //Assert
                    var expectedTimeSeriesSetValuesFromFile = new List<double>()
                    {
                        1.0,
                        2.0,
                        3.0
                    };

                    Assert.That(unpavedData, Is.Not.Null);
                    Assert.That(unpavedData.BoundaryData.Data.Components.First().Values[0], Is.EqualTo(expectedTimeSeriesSetValuesFromFile[0]));
                    Assert.That(unpavedData.BoundaryData.Data.Components.First().Values[1], Is.EqualTo(expectedTimeSeriesSetValuesFromFile[1]));
                    Assert.That(unpavedData.BoundaryData.Data.Components.First().Values[2], Is.EqualTo(expectedTimeSeriesSetValuesFromFile[2]));
                    Assert.That(unpavedData.BoundaryData.IsConstant, Is.False);
                    Assert.That(unpavedData.BoundaryData.IsTimeSeries, Is.True);
                }
            }
        }

        [Test]
        public void GivenConstantData_WhenImport_ThenBoundaryDataShouldBeConstantData()
        {
            //Arrange
            string testDirectory = TestHelper.GetTestDataDirectory() + @"\SobekRRBoundaryConditionsImporter\ConstantBoundaryConditions\";
            using (var temp = new TemporaryDirectory())
            {
                string directory = temp.CopyDirectoryToTempDirectory(testDirectory);
                string filePath = Path.Combine(directory, "BoundaryConditions.bc");

                var importer = new SobekRRBoundaryConditionsImporter
                {
                    IsActive = true,
                    PathSobek = filePath
                };

                const string catchmentName = "Catchment_1D_1";
                using (RainfallRunoffModel rainfallRunoffModel = RainfallRunoffModelBuilder.Start().WithUnpavedCatchmentWithName(catchmentName)
                                                                                           .Build())
                {
                    var unpavedData = rainfallRunoffModel.ModelData.First() as UnpavedData;
                    importer.TargetObject = rainfallRunoffModel;

                    //Act
                    importer.Import();

                    //Assert
                    const double expectedConstantValueFromFile = 10.0;
                    Assert.That(unpavedData, Is.Not.Null);
                    Assert.That(unpavedData.BoundaryData.Value, Is.EqualTo(expectedConstantValueFromFile));
                    Assert.That(unpavedData.BoundaryData.IsConstant, Is.True);
                    Assert.That(unpavedData.BoundaryData.IsTimeSeries, Is.False);
                }
            }
        }
    }
}