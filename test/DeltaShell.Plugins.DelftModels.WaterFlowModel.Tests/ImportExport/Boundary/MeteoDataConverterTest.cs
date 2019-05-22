using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary.TestHelpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    [TestFixture]
    public class MeteoDataConverterTest
    {
        // Happy flow
        /// <summary>
        /// GIVEN a set of DelftBcCategories containing the correct meteo data with a single entry
        /// WHEN MeteoDataConverter convert is called with these parameters
        /// THEN a new MeteoFunction will be returned
        ///  AND no error is logged
        /// </summary>
        [TestCase(BoundaryRegion.TimeInterpolationStrings.BlockTo,              Flow1DInterpolationType.BlockTo,   Flow1DExtrapolationType.Constant, false)]
        [TestCase(BoundaryRegion.TimeInterpolationStrings.BlockTo,              Flow1DInterpolationType.BlockTo,   Flow1DExtrapolationType.Constant, true)]
        [TestCase(BoundaryRegion.TimeInterpolationStrings.BlockFrom,            Flow1DInterpolationType.BlockFrom, Flow1DExtrapolationType.Constant, false)]
        [TestCase(BoundaryRegion.TimeInterpolationStrings.BlockFrom,            Flow1DInterpolationType.BlockFrom, Flow1DExtrapolationType.Constant, true)]
        [TestCase(BoundaryRegion.TimeInterpolationStrings.Linear,               Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Constant, false)]
        [TestCase(BoundaryRegion.TimeInterpolationStrings.Linear,               Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Constant, true)]
        [TestCase(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Linear,   false)]
        [TestCase(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Linear,   true)]

        public void GivenASetOfDelftBcCategoriesContainingTheCorrectMeteoDataWithASingleEntry_WhenMeteoDataConverterConvertIsCalledWithTheseParameters_ThenANewMeteoFunctionWillBeReturnedAndNoErrorIsLogged(string timeInterpolation, 
                                                                                                                                                                                                             Flow1DInterpolationType expectedInterpolationType,
                                                                                                                                                                                                             Flow1DExtrapolationType expectedExtrapolationType,
                                                                                                                                                                                                             bool hasPeriodicity)
        {
            // Given
            // Construct input set with a single value.
            var startTime = DateTime.Today;
            const double airTemperature = 10.0;
            const double relativeHumidity = 5.0;
            const double cloudiness = 20.0;
            var timeValue = startTime.AddHours(1);
            var isPeriodic = hasPeriodicity ? "1" : null;

            var timeValues = new List<DateTime>() {timeValue};
            var airTemperatureValues = new List<double>() {airTemperature};
            var relativeHumidityValues = new List<double>() {relativeHumidity};
            var cloudinessValues = new List<double>() {cloudiness};

            var inputSet = GetTestMeteoDefinition(timeInterpolation,
                                                  isPeriodic,
                                                  startTime,
                                                  timeValues,
                                                  airTemperatureValues,
                                                  relativeHumidityValues,
                                                  cloudinessValues);

            var errorMessages = new List<string>();

            // When
            var outputFunc = MeteoDataConverter.Convert(inputSet, errorMessages);

            // Then
            // Time
            Assert.That(outputFunc, Is.Not.Null, "Expected the read function not to be null.");

            Assert.That(outputFunc.GetInterpolationType(), Is.EqualTo(expectedInterpolationType), "Expected a different interpolation type.");
            Assert.That(outputFunc.GetExtrapolationType(), Is.EqualTo(expectedExtrapolationType), "Expected a different extrapolation type.");
            Assert.That(outputFunc.HasPeriodicity(), Is.EqualTo(hasPeriodicity), "Expected a different periodicity.");
            
            Assert.That(outputFunc.Arguments[0].Values.Count, Is.EqualTo(1), "Expected a different number of arguments.");
            Assert.That(outputFunc.Arguments[0].Values[0], Is.EqualTo(timeValue), "Expected a different first argument value.");

            // Air temperature
            Assert.That(outputFunc.AirTemperature.Values.Count, Is.EqualTo(1), "Expected a different number of values in the air temperature.");
            Assert.That(outputFunc.AirTemperature.Values[0], Is.EqualTo(airTemperature), "Expected a different value for the air temperature.");

            // Relative humidity
            Assert.That(outputFunc.RelativeHumidity.Values.Count, Is.EqualTo(1), "Expected a different number of values in the relative humidity.");
            Assert.That(outputFunc.RelativeHumidity.Values[0], Is.EqualTo(relativeHumidity), "Expected a different value for the relative humidity.");

            // Cloudiness
            Assert.That(outputFunc.Cloudiness.Values.Count, Is.EqualTo(1), "Expected a different number of values in the cloudiness.");
            Assert.That(outputFunc.Cloudiness.Values[0], Is.EqualTo(cloudiness), "Expected a different value for the cloudiness.");

            Assert.That(errorMessages.Count, Is.EqualTo(0), "Expected no error messages when reading the meteo function.");
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

            var inputSet = GetTestMeteoDefinition(interpolationType,
                                                  null,
                                                  startTime,
                                                  timeValues,
                                                  airTemperatureValues,
                                                  relativeHumidityValues,
                                                  cloudinessValues);

            var errorMessages = new List<string>();

            // When
            var outputFunc = MeteoDataConverter.Convert(inputSet, errorMessages);

            // Then
            // Time
            Assert.That(outputFunc, Is.Not.Null);
            Assert.That(outputFunc.Arguments[0].InterpolationType, Is.EqualTo(InterpolationType.Linear));
            Assert.That(outputFunc.Arguments[0].ExtrapolationType, Is.EqualTo(ExtrapolationType.Linear));

            Assert.That(outputFunc.GetInterpolationType(), Is.EqualTo(Flow1DInterpolationType.Linear), "Expected a different interpolation type.");
            Assert.That(outputFunc.GetExtrapolationType(), Is.EqualTo(Flow1DExtrapolationType.Linear), "Expected a different extrapolation type.");
            Assert.That(outputFunc.HasPeriodicity(),       Is.EqualTo(false), "Expected a different periodicity.");

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
            Assert.That(outputFunc.Arguments[0].ExtrapolationType, Is.EqualTo(ExtrapolationType.Linear));

            Assert.That(outputFunc.GetInterpolationType(), Is.EqualTo(Flow1DInterpolationType.Linear), "Expected a different interpolation type.");
            Assert.That(outputFunc.GetExtrapolationType(), Is.EqualTo(Flow1DExtrapolationType.Linear), "Expected a different extrapolation type.");
            Assert.That(outputFunc.HasPeriodicity(), Is.EqualTo(false), "Expected a different periodicity.");

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
            var meteoFunction = MeteoDataConverter.Convert(emptySet, errorMessages);

            // Then
            Assert.IsNull(meteoFunction);
        }


        /// <summary>
        /// GIVEN a MeteoFunction description with an invalid interpolation
        /// WHEN Convert is called
        /// THEN a MeteoFunction with linear extrapolate is generated
        ///  AND an error message is logged
        /// </summary>
        [Test]
        public void GivenAMeteoFunctionDescriptionWithAnInvalidInterpolation_WhenConvertIsCalled_ThenAMeteoFunctionWithLinearExtrapolateIsGeneratedAndAnErrorMessageIsLogged()
        {
            // Given
            // Construct input set with a single value.
            var startTime = DateTime.Today;
            const double airTemperature = 10.0;
            const double relativeHumidity = 5.0;
            const double cloudiness = 20.0;
            var timeValue = startTime.AddHours(1);

            var timeValues = new List<DateTime>() {timeValue};
            var airTemperatureValues = new List<double>() {airTemperature};
            var relativeHumidityValues = new List<double>() {relativeHumidity};
            var cloudinessValues = new List<double>() {cloudiness};

            var inputSet = GetTestMeteoDefinition("DefinitelyNotACorrectInterpolation",
                                                  null,
                                                  startTime,
                                                  timeValues,
                                                  airTemperatureValues,
                                                  relativeHumidityValues,
                                                  cloudinessValues);

            var errorMessages = new List<string>();

            // When
            var outputFunc = MeteoDataConverter.Convert(inputSet, errorMessages);

            // Then
            // Time
            Assert.That(outputFunc, Is.Not.Null, 
                        "Expected the read function not to be null.");

            Assert.That(outputFunc.GetInterpolationType(), Is.EqualTo(Flow1DInterpolationType.Linear),
                        "Expected a different interpolation type.");
            Assert.That(outputFunc.GetExtrapolationType(), Is.EqualTo(Flow1DExtrapolationType.Linear),
                        "Expected a different extrapolation type.");
            Assert.That(outputFunc.HasPeriodicity(), Is.EqualTo(false), 
                        "Expected a different periodicity.");

            Assert.That(outputFunc.Arguments[0].Values.Count, Is.EqualTo(1),
                        "Expected a different number of arguments.");
            Assert.That(outputFunc.Arguments[0].Values[0], Is.EqualTo(timeValue),
                        "Expected a different first argument value.");

            // Air temperature
            Assert.That(outputFunc.AirTemperature.Values.Count, Is.EqualTo(1),
                        "Expected a different number of values in the air temperature.");
            Assert.That(outputFunc.AirTemperature.Values[0], Is.EqualTo(airTemperature),
                        "Expected a different value for the air temperature.");

            // Relative humidity
            Assert.That(outputFunc.RelativeHumidity.Values.Count, Is.EqualTo(1),
                        "Expected a different number of values in the relative humidity.");
            Assert.That(outputFunc.RelativeHumidity.Values[0], Is.EqualTo(relativeHumidity),
                        "Expected a different value for the relative humidity.");

            // Cloudiness
            Assert.That(outputFunc.Cloudiness.Values.Count, Is.EqualTo(1),
                        "Expected a different number of values in the cloudiness.");
            Assert.That(outputFunc.Cloudiness.Values[0], Is.EqualTo(cloudiness),
                        "Expected a different value for the cloudiness.");

            const string expectedErrorMessage =
                "Unable to parse MeteoFunction interpolation, defaulting to linear-extrapolate.";
            Assert.That(errorMessages.Count, Is.EqualTo(1),
                        "Expected an error message when reading the meteo function with an incorrect interpolation.");
            Assert.That(errorMessages[0], Is.EqualTo(expectedErrorMessage),
                "Expected a different error message when reading an incorrect interpolation.");
        }

        /// <summary>
        /// GIVEN a MeteoFunction description with an invalid periodicity
        /// WHEN Convert is called
        /// THEN a MeteoFunction with no periodicity
        ///  AND an error message is logged
        /// </summary>
        [Test]
        public void GivenAMeteoFunctionDescriptionWithAnInvalidPeriodicity_WhenConvertIsCalled_ThenAMeteoFunctionWithNoPeriodicityAndAnErrorMessageIsLogged()
        {
            // Given
            // Construct input set with a single value.
            var startTime = DateTime.Today;
            const double airTemperature = 10.0;
            const double relativeHumidity = 5.0;
            const double cloudiness = 20.0;
            var timeValue = startTime.AddHours(1);

            var timeValues = new List<DateTime>() { timeValue };
            var airTemperatureValues = new List<double>() { airTemperature };
            var relativeHumidityValues = new List<double>() { relativeHumidity };
            var cloudinessValues = new List<double>() { cloudiness };

            var inputSet = GetTestMeteoDefinition(BoundaryRegion.TimeInterpolationStrings.BlockTo,
                                                  "DefinitelyNotACorrectPeriodicity",
                                                  startTime,
                                                  timeValues,
                                                  airTemperatureValues,
                                                  relativeHumidityValues,
                                                  cloudinessValues);

            var errorMessages = new List<string>();

            // When
            var outputFunc = MeteoDataConverter.Convert(inputSet, errorMessages);

            // Then
            // Time
            Assert.That(outputFunc, Is.Not.Null,
                        "Expected the read function not to be null:");

            Assert.That(outputFunc.GetInterpolationType(), Is.EqualTo(Flow1DInterpolationType.BlockTo),
                        "Expected a different interpolation type.");
            Assert.That(outputFunc.GetExtrapolationType(), Is.EqualTo(Flow1DExtrapolationType.Constant),
                        "Expected a different extrapolation type.");
            Assert.That(outputFunc.HasPeriodicity(), Is.EqualTo(false),
                        "Expected a different periodicity.");

            Assert.That(outputFunc.Arguments[0].Values.Count, Is.EqualTo(1),
                        "Expected a different number of arguments.");
            Assert.That(outputFunc.Arguments[0].Values[0], Is.EqualTo(timeValue),
                        "Expected a different first argument value.");

            // Air temperature
            Assert.That(outputFunc.AirTemperature.Values.Count, Is.EqualTo(1),
                        "Expected a different number of values in the air temperature.");
            Assert.That(outputFunc.AirTemperature.Values[0], Is.EqualTo(airTemperature),
                        "Expected a different value for the air temperature.");

            // Relative humidity
            Assert.That(outputFunc.RelativeHumidity.Values.Count, Is.EqualTo(1),
                        "Expected a different number of values in the relative humidity.");
            Assert.That(outputFunc.RelativeHumidity.Values[0], Is.EqualTo(relativeHumidity),
                        "Expected a different value for the relative humidity.");

            // Cloudiness
            Assert.That(outputFunc.Cloudiness.Values.Count, Is.EqualTo(1),
                        "Expected a different number of values in the cloudiness.");
            Assert.That(outputFunc.Cloudiness.Values[0], Is.EqualTo(cloudiness),
                        "Expected a different value for the cloudiness.");

            const string expectedErrorMessage =
                "Unable to parse MeteoFunction periodicity, defaulting to false.";
            Assert.That(errorMessages.Count, Is.EqualTo(1),
                        "Expected an error message when reading the meteo function with an incorrect interpolation.");
            Assert.That(errorMessages[0], Is.EqualTo(expectedErrorMessage),
                "Expected a different error message when reading an incorrect interpolation.");
        }

        /// <summary>
        /// Get a simple test meteo definition with the specified parameters.
        /// </summary>
        /// <param name="timeInterpolation">The time interpolation.</param>
        /// <param name="isPeriodic">The is periodic.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="timeValues">The time values.</param>
        /// <param name="airTemperatureValues">The air temperature values.</param>
        /// <param name="relativeHumidityValues">The relative humidity values.</param>
        /// <param name="cloudinessValues">The cloudiness values.</param>
        /// <returns>A meteo definition describing the specified parameters.</returns>
        private static IList<IDelftBcCategory> GetTestMeteoDefinition(string timeInterpolation,
                                                                             string isPeriodic,
                                                                             DateTime startTime,
                                                                             IList<DateTime> timeValues,
                                                                             IList<double> airTemperatureValues,
                                                                             IList<double> relativeHumidityValues,
                                                                             IList<double> cloudinessValues)
        {
            var inputSet = new List<IDelftBcCategory>();

            var airTempBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var airTempDefinition = airTempBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                              BoundaryRegion.FunctionStrings.TimeSeries,
                                                              timeInterpolation,
                                                              isPeriodic);
            airTempDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                  timeValues,
                                                                                  airTemperatureValues,
                                                                                  BoundaryRegion.QuantityStrings.MeteoDataAirTemperature,
                                                                                  BoundaryRegion.UnitStrings.MeteoDataAirTemperature);
            inputSet.Add(airTempDefinition);


            var relHumidBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var relHumidDefinition = relHumidBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                                BoundaryRegion.FunctionStrings.TimeSeries,
                                                                timeInterpolation,
                                                                isPeriodic);
            relHumidDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                 timeValues,
                                                                                 relativeHumidityValues,
                                                                                 BoundaryRegion.QuantityStrings.MeteoDataHumidity,
                                                                                 BoundaryRegion.UnitStrings.MeteoDataHumidity);
            inputSet.Add(relHumidDefinition);


            var cloudinessBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var cloudinessDefinition = cloudinessBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                                    BoundaryRegion.FunctionStrings.TimeSeries,
                                                                    timeInterpolation,
                                                                    isPeriodic);
            cloudinessDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                   timeValues,
                                                                                   cloudinessValues,
                                                                                   BoundaryRegion.QuantityStrings.MeteoDataCloudiness,
                                                                                   BoundaryRegion.UnitStrings.MeteoDataCloudiness);
            inputSet.Add(cloudinessDefinition);
            return inputSet;
        }
    }
}
