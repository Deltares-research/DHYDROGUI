using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Importers
{
    public class MeteoDataImporterTest
    {
        [Test]
        [TestCaseSource(nameof(ConstructorArgNullCases))]
        public void Constructor_ArgNull_ThrowsArgumentNullException(PrecipitationDataImporter precipitationImporter,
                                                                    EvaporationDataImporter evaporationImporter,
                                                                    TemperatureDataImporter temperatureImporter,
                                                                    string expParamName)
        {
            // Call
            void Call() => new MeteoDataImporter(precipitationImporter, evaporationImporter, temperatureImporter);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException
                                    .With.Property(nameof(ArgumentException.ParamName)).EqualTo(expParamName));
        }

        private static IEnumerable<TestCaseData> ConstructorArgNullCases()
        {
            yield return new TestCaseData(null, new EvaporationDataImporter(), new TemperatureDataImporter(), "precipitationImporter");
            yield return new TestCaseData(new PrecipitationDataImporter(), null, new TemperatureDataImporter(), "evaporationImporter");
            yield return new TestCaseData(new PrecipitationDataImporter(), new EvaporationDataImporter(), null, "temperatureImporter");
        }
    }
}