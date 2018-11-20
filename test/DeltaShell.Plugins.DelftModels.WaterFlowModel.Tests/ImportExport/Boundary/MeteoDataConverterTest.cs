using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    [TestFixture]
    class MeteoDataConverterTest
    {
        // Happy flow
        /// <summary>
        /// GIVEN a set of DelftBcCategories containing the correct wind data with a single entry
        /// WHEN MeteoDataConverter convert is called with these parameters
        /// THEN a new MeteoFunction will be returned
        ///  AND no error is logged
        /// </summary>
        [Test]
        public void GivenASetOfDelftBcCategoriesContainingTheCorrectWindDataWithASingleEntry_WhenMeteoDataConverterConvertIsCalledWithTheseParameters_ThenANewMeteoFunctionWillBeReturnedAndNoErrorIsLogged()
        {
            // Given
            // Construct input set with a single value.
            var inputSet = new List<IDelftBcCategory>();
            var startTime = DateTime.Today;
            const string interpolationType = BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate;
            const double airTemperature = 10.0;
            const double relativeHumidity = 5.0;
            const double cloudiness = 20.0;
            var timeValue = startTime.AddHours(1);

            var airTempBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var airTempDefinition = airTempBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                              BoundaryRegion.FunctionStrings.TimeSeries,
                                                              interpolationType);
            airTempDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                  new List<DateTime>() { timeValue },
                                                                                  new List<double>() { airTemperature },
                                                                                  BoundaryRegion.QuantityStrings.MeteoDataAirTemperature,
                                                                                  BoundaryRegion.UnitStrings.MeteoDataAirTemperature);
            inputSet.Add(airTempDefinition);


            var relHumidBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var relHumidDefinition = relHumidBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                                BoundaryRegion.FunctionStrings.TimeSeries,
                                                                interpolationType);
            relHumidDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                 new List<DateTime>() { timeValue },
                                                                                 new List<double>() { relativeHumidity },
                                                                                 BoundaryRegion.QuantityStrings.MeteoDataHumidity,
                                                                                 BoundaryRegion.UnitStrings.MeteoDataHumidity);
            inputSet.Add(relHumidDefinition);


            var cloudinessBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var cloudinessDefinition = cloudinessBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                                    BoundaryRegion.FunctionStrings.TimeSeries,
                                                                    interpolationType);
            cloudinessDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                   new List<DateTime>() { timeValue },
                                                                                   new List<double>() { cloudiness },
                                                                                   BoundaryRegion.QuantityStrings.MeteoDataCloudiness,
                                                                                   BoundaryRegion.UnitStrings.MeteoDataCloudiness);
            inputSet.Add(cloudinessDefinition);

            var errorMessages = new List<string>();

            // When
            var outputFunc = MeteoDataConverter.Convert(inputSet, errorMessages);

            // Then
            // Time
            Assert.That(outputFunc, Is.Not.Null);
            Assert.That(outputFunc.Arguments[0].InterpolationType, Is.EqualTo(InterpolationType.Linear));
            Assert.That(outputFunc.Arguments[0].ExtrapolationType, Is.EqualTo(ExtrapolationType.Constant));

            Assert.That(outputFunc.Arguments[0].Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.Arguments[0].Values[0], Is.EqualTo(timeValue));

            // Air temperature
            Assert.That(outputFunc.AirTemperature.Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.AirTemperature.Values[0], Is.EqualTo(airTemperature));

            // Relative humidity
            Assert.That(outputFunc.RelativeHumidity.Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.RelativeHumidity.Values[0], Is.EqualTo(relativeHumidity));

            // Cloudiness
            Assert.That(outputFunc.Cloudiness.Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.Cloudiness.Values[0], Is.EqualTo(cloudiness));

            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// GIVEN a set of DelftBcCategories containing the correct wind data with multiple entries
        /// WHEN MeteoDataConverter convert is called with these parameters
        /// THEN a new MeteoFunction will be returned
        ///  AND no error is logged
        /// </summary>
        [Test]
        public void GivenASetOfDelftBcCategoriesContainingTheCorrectWindDataWithMultipleEntries_WhenMeteoDataConverterConvertIsCalledWithTheseParameters_ThenANewMeteoFunctionWillBeReturnedAndNoErrorIsLogged()
        {
            // Given
            // Construct input set with a single value.
            var inputSet = new List<IDelftBcCategory>();
            var startTime = DateTime.Today;
            const string interpolationType = BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate;
            var airTemperatureValues = new List<double>() { 10.0, 20.0, 30.0, 40.0, 50.0 };
            var relativeHumidityValues = new List<double>() { 101.0, 21.0, 31.0, 41.0, 51.0 };
            var cloudinessValues = new List<double>() { 601.0, 521.0, 431.0, 413.0, 512.0 };
            var timeValues = new List<DateTime>()
            {
                startTime.AddHours(1),
                startTime.AddHours(2),
                startTime.AddHours(3),
                startTime.AddHours(4),
                startTime.AddHours(5),
            };


            var airTempBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var airTempDefinition = airTempBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                              BoundaryRegion.FunctionStrings.TimeSeries,
                                                              interpolationType);
            airTempDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                  timeValues,
                                                                                  airTemperatureValues,
                                                                                  BoundaryRegion.QuantityStrings.MeteoDataAirTemperature,
                                                                                  BoundaryRegion.UnitStrings.MeteoDataAirTemperature);
            inputSet.Add(airTempDefinition);


            var relHumidBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var relHumidDefinition = relHumidBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                                BoundaryRegion.FunctionStrings.TimeSeries,
                                                                interpolationType);
            relHumidDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                 timeValues,
                                                                                 relativeHumidityValues,
                                                                                 BoundaryRegion.QuantityStrings.MeteoDataHumidity,
                                                                                 BoundaryRegion.UnitStrings.MeteoDataHumidity);
            inputSet.Add(relHumidDefinition);


            var cloudinessBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var cloudinessDefinition = cloudinessBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                                    BoundaryRegion.FunctionStrings.TimeSeries,
                                                                    interpolationType);
            cloudinessDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                   timeValues,
                                                                                   cloudinessValues,
                                                                                   BoundaryRegion.QuantityStrings.MeteoDataCloudiness,
                                                                                   BoundaryRegion.UnitStrings.MeteoDataCloudiness);
            inputSet.Add(cloudinessDefinition);

            var errorMessages = new List<string>();

            // When
            var outputFunc = MeteoDataConverter.Convert(inputSet, errorMessages);

            // Then
            // Time
            Assert.That(outputFunc, Is.Not.Null);
            Assert.That(outputFunc.Arguments[0].InterpolationType, Is.EqualTo(InterpolationType.Linear));
            Assert.That(outputFunc.Arguments[0].ExtrapolationType, Is.EqualTo(ExtrapolationType.Constant));


            Assert.That(outputFunc.Arguments[0].Values.Count, Is.EqualTo(timeValues.Count));
            Assert.That(outputFunc.AirTemperature.Values.Count, Is.EqualTo(timeValues.Count));
            Assert.That(outputFunc.RelativeHumidity.Values.Count, Is.EqualTo(timeValues.Count));
            Assert.That(outputFunc.Cloudiness.Values.Count, Is.EqualTo(timeValues.Count));

            for (var i = 0; i < timeValues.Count; i++)
            {
                Assert.That(outputFunc.Arguments[0].Values[i], Is.EqualTo(timeValues[i]));
                Assert.That(outputFunc.AirTemperature.Values[i], Is.EqualTo(airTemperatureValues[i]));
                Assert.That(outputFunc.RelativeHumidity.Values[i], Is.EqualTo(relativeHumidityValues[i]));
                Assert.That(outputFunc.Cloudiness.Values[i], Is.EqualTo(cloudinessValues[i]));
            }

            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// GIVEN a set of DelftBcCategories containing the correct wind data with a single entry and other data
        /// WHEN MeteoDataConverter convert is called with these parameters
        /// THEN a new MeteoFunction will be returned
        ///  AND no error is logged
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        public void GivenASetOfDelftBcCategoriesContainingTheCorrectWindDataWithASingleEntryAndOtherData_WhenMeteoDataConverterConvertIsCalledWithTheseParameters_ThenANewMeteoFunctionWillBeReturnedAndNoErrorIsLogged(bool doReverseCategories)
        {
            // Given
            // Construct input set with a single value.
            var inputSet = new List<IDelftBcCategory>();
            var startTime = DateTime.Today;
            const string interpolationType = BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate;
            var airTemperatureValues = new List<double>() { 10.0, 20.0, 30.0, 40.0, 50.0 };
            var relativeHumidityValues = new List<double>() { 101.0, 21.0, 31.0, 41.0, 51.0 };
            var cloudinessValues = new List<double>() { 601.0, 521.0, 431.0, 413.0, 512.0 };
            var timeValues = new List<DateTime>()
            {
                startTime.AddHours(1),
                startTime.AddHours(2),
                startTime.AddHours(3),
                startTime.AddHours(4),
                startTime.AddHours(5),
            };

            inputSet.Add(new DelftBcCategory("Garbage"));

            var airTempBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var airTempDefinition = airTempBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                              BoundaryRegion.FunctionStrings.TimeSeries,
                                                              interpolationType);
            airTempDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                  timeValues,
                                                                                  airTemperatureValues,
                                                                                  BoundaryRegion.QuantityStrings.MeteoDataAirTemperature,
                                                                                  BoundaryRegion.UnitStrings.MeteoDataAirTemperature);
            inputSet.Add(airTempDefinition);
            inputSet.Add(new DelftBcCategory("Yurp"));


            var relHumidBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var relHumidDefinition = relHumidBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                                BoundaryRegion.FunctionStrings.TimeSeries,
                                                                interpolationType);
            relHumidDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                 timeValues,
                                                                                 relativeHumidityValues,
                                                                                 BoundaryRegion.QuantityStrings.MeteoDataHumidity,
                                                                                 BoundaryRegion.UnitStrings.MeteoDataHumidity);
            inputSet.Add(relHumidDefinition);
            inputSet.Add(new DelftBcCategory("GarbageTruck"));


            var cloudinessBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var cloudinessDefinition = cloudinessBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                                    BoundaryRegion.FunctionStrings.TimeSeries,
                                                                    interpolationType);
            cloudinessDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                   timeValues,
                                                                                   cloudinessValues,
                                                                                   BoundaryRegion.QuantityStrings.MeteoDataCloudiness,
                                                                                   BoundaryRegion.UnitStrings.MeteoDataCloudiness);
            inputSet.Add(cloudinessDefinition);
            inputSet.Add(new DelftBcCategory("Potato"));

            if (doReverseCategories)
                inputSet.Reverse();

            var errorMessages = new List<string>();

            // When
            var outputFunc = MeteoDataConverter.Convert(inputSet, errorMessages);

            // Then
            // Time
            Assert.That(outputFunc, Is.Not.Null);
            Assert.That(outputFunc.Arguments[0].InterpolationType, Is.EqualTo(InterpolationType.Linear));
            Assert.That(outputFunc.Arguments[0].ExtrapolationType, Is.EqualTo(ExtrapolationType.Constant));


            Assert.That(outputFunc.Arguments[0].Values.Count, Is.EqualTo(timeValues.Count));
            Assert.That(outputFunc.AirTemperature.Values.Count, Is.EqualTo(timeValues.Count));
            Assert.That(outputFunc.RelativeHumidity.Values.Count, Is.EqualTo(timeValues.Count));
            Assert.That(outputFunc.Cloudiness.Values.Count, Is.EqualTo(timeValues.Count));

            for (var i = 0; i < timeValues.Count; i++)
            {
                Assert.That(outputFunc.Arguments[0].Values[i], Is.EqualTo(timeValues[i]));
                Assert.That(outputFunc.AirTemperature.Values[i], Is.EqualTo(airTemperatureValues[i]));
                Assert.That(outputFunc.RelativeHumidity.Values[i], Is.EqualTo(relativeHumidityValues[i]));
                Assert.That(outputFunc.Cloudiness.Values[i], Is.EqualTo(cloudinessValues[i]));
            }

            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }

        // Slightly less happy flow
        /// <summary>
        /// GIVEN a null set of DelftBcCategories
        ///   AND an empty list of error messages
        /// WHEN MeteoDataConverter convert is called with these parameters
        /// THEN a null function will be returned
        ///  AND a single error is logged
        /// </summary>
        [Test]
        public void GivenANullSetOfDelftBcCategoriesAndAnEmptyListOfErrorMessages_WhenMeteoDataConverterConvertIsCalledWithTheseParameters_ThenANullFunctionWillBeReturnedAndASingleErrorIsLogged()
        {
            // Given
            const IList<IDelftBcCategory> nullSet = null;
            var errorMessages = new List<string>();

            // When
            var outputFunc = MeteoDataConverter.Convert(nullSet, errorMessages);

            // Then
            Assert.That(outputFunc, Is.Null);

            Assert.That(errorMessages.Count, Is.EqualTo(1));

            const string expectedErrorMessage = "Unable to parse null meteo data function.";
            Assert.That(errorMessages[0], Is.EqualTo(expectedErrorMessage));
        }

        /// <summary>
        /// GIVEN an empty set of DelftBcCategories
        ///   AND an empty list of error messages
        /// WHEN MeteoDataConverter convert is called with these parameters
        /// THEN a null function will be returned
        ///  AND a single error is logged
        /// </summary>
        [Test]
        public void GivenAnEmptySetOfDelftBcCategoriesAndAnEmptyListOfErrorMessages_WhenMeteoDataConverterConvertIsCalledWithTheseParameters_ThenANullFunctionWillBeReturnedAndASingleErrorIsLogged()
        {
            var emptySet = new List<IDelftBcCategory>();
            var errorMessages = new List<string>();

            // When
            var outputFunc = MeteoDataConverter.Convert(emptySet, errorMessages);

            // Then
            Assert.That(outputFunc, Is.Null);

            Assert.That(errorMessages.Count, Is.EqualTo(1));

            const string expectedErrorMessage = "Unable to parse empty set of meteo data.";
            Assert.That(errorMessages[0], Is.EqualTo(expectedErrorMessage));
        }
    }
}
