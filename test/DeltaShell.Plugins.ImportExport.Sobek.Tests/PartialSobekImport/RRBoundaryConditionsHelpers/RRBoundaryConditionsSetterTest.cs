using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers;
using DeltaShell.Plugins.ImportExport.Sobek.Properties;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport.RRBoundaryConditionsHelpers
{
    [TestFixture]
    public class RRBoundaryConditionsSetterTest
    {
        [Test]
        public void WhenConditionConstructor_ArgumentNull_ThrowsArgumentNullException()
        {
            // Arrange
            void Call() => new RRBoundaryConditionsSetter(null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void WhenConditionSetterSet_ArgumentNull_ThrowsArgumentNullException()
        {
            //Arrange
            var conditionsSetter = new RRBoundaryConditionsSetter(Substitute.For<ILogHandler>());

            // Act
            void Call() => conditionsSetter.Set(null);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void WhenValueGiven_WhenConditionSetterSet_ValueSetOnLinkedData()
        {
            // Arrange
            const double expectedValue = 10;
            const string name = "name";

            var conditionsSetter = new RRBoundaryConditionsSetter(Substitute.For<ILogHandler>());

            var boundaryData = new Dictionary<string, RainfallRunoffBoundaryData>();
            var unpavedDataByName = new Dictionary<string, UnpavedData>();
            var lateralToCatchmentLookup = new Dictionary<string, SobekRRLink[]>();

            boundaryData[name] = new RainfallRunoffBoundaryData() { Value = expectedValue };

            var unpaved = new UnpavedData(new Catchment()) { Name = name };
            unpavedDataByName[name] = unpaved;
            lateralToCatchmentLookup[name] = new[]
            {
                new SobekRRLink() { NodeFromId = name }
            };

            var rrModel = new RainfallRunoffModel();
            rrModel.LateralToCatchmentLookup = lateralToCatchmentLookup;
            rrModel.ModelData.Add(unpaved);
            var data = new RRModelBoundarySetterData(rrModel, boundaryData);

            //Act
            conditionsSetter.Set(data);

            //Assert
            Assert.That(unpaved.BoundaryData.Value, Is.EqualTo(expectedValue));
            Assert.That(unpaved.BoundaryData, Is.SameAs(boundaryData[name]));
        }

        [Test]
        public void WhenValueGivenNotInLookup_WhenConditionSetterSet_ValueSetOnLinkedData()
        {
            // Arrange
            const double expectedValue = 10;
            const string name = "name";
            const string name2 = "name2";

            var logHandler = Substitute.For<ILogHandler>();

            var conditionsSetter = new RRBoundaryConditionsSetter(logHandler);

            var boundaryData = new Dictionary<string, RainfallRunoffBoundaryData>();
            var unpavedDataByName = new Dictionary<string, UnpavedData>();
            var lateralToCatchmentLookup = new Dictionary<string, SobekRRLink[]>();

            boundaryData[name] = new RainfallRunoffBoundaryData() { Value = expectedValue };

            var unpaved = new UnpavedData(new Catchment()) { Name = name };
            unpavedDataByName[name] = unpaved;
            lateralToCatchmentLookup[name2] = new[]
            {
                new SobekRRLink() { NodeFromId = name2 }
            };

            var rrModel = new RainfallRunoffModel();
            rrModel.LateralToCatchmentLookup = lateralToCatchmentLookup;
            rrModel.ModelData.Add(unpaved);
            var data = new RRModelBoundarySetterData(rrModel, boundaryData);

            //Act
            conditionsSetter.Set(data);

            //Assert
            logHandler.Received(1).ReportWarning(string.Format(Resources.RRBoundaryConditionsSetter_Set_Could_not_find__0__linked_to_boundary_, name));
        }

        private static IEnumerable<TestCaseData> ConstructorArgumentNullCases()
        {
            yield return new TestCaseData(null, new Dictionary<string, UnpavedData>(), new Dictionary<string, SobekRRLink[]>());
            yield return new TestCaseData(new Dictionary<string, RainfallRunoffBoundaryData>(), null, new Dictionary<string, SobekRRLink[]>());
            yield return new TestCaseData(new Dictionary<string, RainfallRunoffBoundaryData>(), new Dictionary<string, UnpavedData>(), null);
        }
    }
}