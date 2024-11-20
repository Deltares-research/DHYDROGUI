using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport.RRBoundaryConditionsHelpers
{
    [TestFixture]
    public class RRModelBoundarySetterDataTest
    {
        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(RainfallRunoffModel rainfallRunoffModel, IReadOnlyDictionary<string, RainfallRunoffBoundaryData> boundaryDataByBoundaryName)
        {
            // Arrange & Act
            void Call() => new RRModelBoundarySetterData(rainfallRunoffModel, boundaryDataByBoundaryName);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void GivenRainFallRunoffData_WhenConstructed_ThenPropertiesContainData()
        {
            //Arrange
            var rrModel = new RainfallRunoffModel();
            var boundaryDataByBoundaryName = Substitute.For<IReadOnlyDictionary<string, RainfallRunoffBoundaryData>>();
            var unpavedCatchment = new UnpavedData(new Catchment());
            unpavedCatchment.Name = "catchmentName";
            rrModel.ModelData.Add(unpavedCatchment);

            //Act
            var dataSet = new RRModelBoundarySetterData(rrModel, boundaryDataByBoundaryName);

            //Assert
            Assert.That(dataSet.LateralToCatchmentLookup, Is.EqualTo(rrModel.LateralToCatchmentLookup));
            Assert.That(dataSet.BoundaryDataByBoundaryName, Is.EqualTo(boundaryDataByBoundaryName));
            Assert.That(dataSet.UnpavedDataByName["catchmentName"], Is.EqualTo(unpavedCatchment));
        }

        private static IEnumerable<TestCaseData> ConstructorArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IReadOnlyDictionary<string, RainfallRunoffBoundaryData>>());
            yield return new TestCaseData(new RainfallRunoffModel(), null);
        }
    }
}