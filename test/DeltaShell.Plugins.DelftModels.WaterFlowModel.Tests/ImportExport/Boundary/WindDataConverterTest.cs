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
    public class WindDataConverterTest
    {
        // Happy flow
        /// <summary>
        /// GIVEN a set of DelftBcCategories containing the correct wind data with a single entry
        /// WHEN WindDataConverter convert is called with these parameters
        /// THEN a new windfunction will be returned
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
        public void GivenASetOfDelftBcCategoriesContainingTheCorrectWindDataWithASingleEntry_WhenWindDataConverterConvertIsCalledWithTheseParameters_ThenANewWindfunctionWillBeReturnedAndNoErrorIsLogged(string timeInterpolation,
                                                                                                                                                                                                          Flow1DInterpolationType expectedInterpolationType,
                                                                                                                                                                                                          Flow1DExtrapolationType expectedExtrapolationType,
                                                                                                                                                                                                          bool hasPeriodicity)
        {
            // Given
            // Construct input set with a single value.
            
            var startTime = DateTime.Today;
            const double windSpeed = 30.0;
            const double windDirection = 5.0;
            var timeValue = startTime.AddHours(1);
            var isPeriodic = hasPeriodicity ? "1" : null;

            var timeValues = new List<DateTime>() {timeValue};
            var valuesWindSpeed = new List<double>() { windSpeed };
            var valuesWindDir = new List<double>() {windDirection};

            var inputSet = GetTestWindDefinition(timeInterpolation, 
                                             isPeriodic, 
                                             startTime, 
                                             timeValues, 
                                             valuesWindSpeed,
                                             valuesWindDir);

            var errorMessages = new List<string>();

            // When
            var outputFunc = WindDataConverter.Convert(inputSet, errorMessages);

            // Then
            Assert.That(outputFunc, Is.Not.Null);

            Assert.That(outputFunc.GetInterpolationType(), Is.EqualTo(expectedInterpolationType), "Expected a different interpolation type.");
            Assert.That(outputFunc.GetExtrapolationType(), Is.EqualTo(expectedExtrapolationType), "Expected a different extrapolation type.");
            Assert.That(outputFunc.HasPeriodicity(), Is.EqualTo(hasPeriodicity), "Expected a different periodicity.");

            Assert.That(outputFunc.Arguments[0].Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.Arguments[0].Values[0], Is.EqualTo(timeValue));

            Assert.That(outputFunc.Velocity.Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.Velocity.Values[0], Is.EqualTo(windSpeed));

            Assert.That(outputFunc.Direction.Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.Direction.Values[0], Is.EqualTo(windDirection));

            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// GIVEN a set of DelftBcCategories containing the correct wind data with multiple entries
        /// WHEN WindDataConverter convert is called with these parameters
        /// THEN a new windfunction will be returned
        ///  AND no error is logged
        /// </summary>
        [Test]
        public void GivenASetOfDelftBcCategoriesContainingTheCorrectWindDataWithMultipleEntries_WhenWindDataConverterConvertIsCalledWithTheseParameters_ThenANewWindfunctionWillBeReturnedAndNoErrorIsLogged()
        {
            // Given
            // Construct input set with a single value.
            var startTime = DateTime.Today;
            const string interpolationType = BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate;
            var windSpeedValues = new List<double>() {30.0, 40.0, 50.0, 60.0 };
            var windDirectionValues = new List<double>() { 5.0, 20.0, 10.0, 30.0 };
            var timeValues = new List<DateTime>()
            {
                startTime.AddHours(1),
                startTime.AddHours(2),
                startTime.AddHours(3),
                startTime.AddHours(4),
            };

            var inputSet = GetTestWindDefinition(interpolationType,
                                                 "1",
                                                 startTime,
                                                 timeValues,
                                                 windSpeedValues,
                                                 windDirectionValues);

            var errorMessages = new List<string>();

            // When
            var outputFunc = WindDataConverter.Convert(inputSet, errorMessages);

            // Then
            Assert.That(outputFunc, Is.Not.Null);
            Assert.That(outputFunc.Arguments[0].InterpolationType, Is.EqualTo(InterpolationType.Linear));
            Assert.That(outputFunc.Arguments[0].ExtrapolationType, Is.EqualTo(ExtrapolationType.Periodic));

            Assert.That(outputFunc.GetInterpolationType(), Is.EqualTo(Flow1DInterpolationType.Linear), "Expected a different interpolation type.");
            Assert.That(outputFunc.GetExtrapolationType(), Is.EqualTo(Flow1DExtrapolationType.Linear), "Expected a different extrapolation type.");
            Assert.That(outputFunc.HasPeriodicity(),       Is.EqualTo(true), "Expected a different periodicity.");

            Assert.That(outputFunc.Arguments[0].Values.Count, Is.EqualTo(timeValues.Count));
            Assert.That(outputFunc.Velocity.Values.Count, Is.EqualTo(windSpeedValues.Count));
            Assert.That(outputFunc.Direction.Values.Count, Is.EqualTo(windDirectionValues.Count));

            for (var i = 0; i < timeValues.Count; i++)
            {
                Assert.That(outputFunc.Arguments[0].Values[i], Is.EqualTo(timeValues[i]));
                Assert.That(outputFunc.Velocity.Values[i], Is.EqualTo(windSpeedValues[i]));
                Assert.That(outputFunc.Direction.Values[i], Is.EqualTo(windDirectionValues[i]));
            }
            
            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// GIVEN a set of DelftBcCategories containing the correct wind data with a single entry and other data
        /// WHEN WindDataConverter convert is called with these parameters
        /// THEN a new windfunction will be returned
        ///  AND no error is logged
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        public void GivenASetOfDelftBcCategoriesContainingTheCorrectWindDataWithASingleEntryAndOtherData_WhenWindDataConverterConvertIsCalledWithTheseParameters_ThenANewWindfunctionWillBeReturnedAndNoErrorIsLogged(bool doReverseCategories)
        {
            // Given
            // Construct input set with a single value.
            var inputSet = new List<IDelftBcCategory>();
            var startTime = DateTime.Today;
            const string interpolationType = BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate;
            var windSpeedValues = new List<double>() { 30.0, 40.0, 50.0, 60.0 };
            var windDirectionValues = new List<double>() { 5.0, 20.0, 10.0, 30.0 };
            var timeValues = new List<DateTime>()
            {
                startTime.AddHours(1),
                startTime.AddHours(2),
                startTime.AddHours(3),
                startTime.AddHours(4),
            };

            inputSet.Add(new DelftBcCategory("Garbage"));           

            var windSpeedBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var windSpeedDefinition = windSpeedBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                                  BoundaryRegion.FunctionStrings.TimeSeries,
                                                                  interpolationType);

            windSpeedDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                  timeValues,
                                                                                  windSpeedValues,
                                                                                  BoundaryRegion.QuantityStrings.WindSpeed,
                                                                                  BoundaryRegion.UnitStrings.WindSpeed);
            inputSet.Add(windSpeedDefinition);
            inputSet.Add(new DelftBcCategory("GarbageTruck"));

            var windDirBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var windDirDefinition = windDirBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                BoundaryRegion.FunctionStrings.TimeSeries, interpolationType);

            windDirDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                timeValues,
                                                                                windDirectionValues,
                                                                                BoundaryRegion.QuantityStrings.WindDirection,
                                                                                BoundaryRegion.UnitStrings.WindDirection);
            inputSet.Add(windDirDefinition);
            inputSet.Add(new DelftBcCategory("Potato"));

            if (doReverseCategories)
                inputSet.Reverse();

            var errorMessages = new List<string>();

            // When
            var outputFunc = WindDataConverter.Convert(inputSet, errorMessages);

            // Then
            Assert.That(outputFunc, Is.Not.Null);
            Assert.That(outputFunc.Arguments[0].InterpolationType, Is.EqualTo(InterpolationType.Linear));
            Assert.That(outputFunc.Arguments[0].ExtrapolationType, Is.EqualTo(ExtrapolationType.Linear));

            Assert.That(outputFunc.GetInterpolationType(), Is.EqualTo(Flow1DInterpolationType.Linear), "Expected a different interpolation type.");
            Assert.That(outputFunc.GetExtrapolationType(), Is.EqualTo(Flow1DExtrapolationType.Linear), "Expected a different extrapolation type.");
            Assert.That(outputFunc.HasPeriodicity(),       Is.EqualTo(false), "Expected a different periodicity.");

            Assert.That(outputFunc.Arguments[0].Values.Count, Is.EqualTo(timeValues.Count));
            Assert.That(outputFunc.Velocity.Values.Count, Is.EqualTo(windSpeedValues.Count));
            Assert.That(outputFunc.Direction.Values.Count, Is.EqualTo(windDirectionValues.Count));

            for (var i = 0; i < timeValues.Count; i++)
            {
                Assert.That(outputFunc.Arguments[0].Values[i], Is.EqualTo(timeValues[i]));
                Assert.That(outputFunc.Velocity.Values[i], Is.EqualTo(windSpeedValues[i]));
                Assert.That(outputFunc.Direction.Values[i], Is.EqualTo(windDirectionValues[i]));
            }

            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }

        // Slightly less happy flow
        /// <summary>
        /// GIVEN a null set of DelftBcCategories
        ///   AND an empty list of error messages
        /// WHEN WindDataConverter convert is called with these parameters
        /// THEN a null function will be returned
        ///  AND a single error is logged
        /// </summary>
        [Test]
        public void GivenANullSetOfDelftBcCategoriesAndAnEmptyListOfErrorMessages_WhenWindDataConverterConvertIsCalledWithTheseParameters_ThenANullFunctionWillBeReturnedAndASingleErrorIsLogged()
        {
            // Given
            const IList<IDelftBcCategory> nullSet = null;
            var errorMessages = new List<string>();

            // When
            var outputFunc = WindDataConverter.Convert(nullSet, errorMessages);

            // Then
            Assert.That(outputFunc, Is.Null);

            Assert.That(errorMessages.Count, Is.EqualTo(1));

            const string expectedErrorMessage = "Unable to parse null wind data function.";
            Assert.That(errorMessages[0], Is.EqualTo(expectedErrorMessage));
        }

        /// <summary>
        /// GIVEN an empty set of DelftBcCategories
        ///   AND an empty list of error messages
        /// WHEN WindDataConverter convert is called with these parameters
        /// THEN a null function will be returned
        ///  AND a single error is logged
        /// </summary>
        [Test]
        public void GivenAnEmptySetOfDelftBcCategoriesAndAnEmptyListOfErrorMessages_WhenWindDataConverterConvertIsCalledWithTheseParameters_ThenANullFunctionWillBeReturnedAndASingleErrorIsLogged()
        {
            // Given
            var emptySet = new List<IDelftBcCategory>();
            var errorMessages = new List<string>();

            // When
            var outputFunc = WindDataConverter.Convert(emptySet, errorMessages);

            // Then
            Assert.That(outputFunc, Is.Null);

            Assert.That(errorMessages.Count, Is.EqualTo(1));

            const string expectedErrorMessage = "Unable to parse empty set of wind data.";
            Assert.That(errorMessages[0], Is.EqualTo(expectedErrorMessage));
        }

        /// <summary>
        /// GIVEN a WindFunction description with an invalid interpolation
        /// WHEN Convert is called
        /// THEN a WindFunction with linear extrapolate is generated
        ///  AND an error message is logged
        /// </summary>
        [Test]
        public void GivenAWindFunctionDescriptionWithAnInvalidInterpolation_WhenConvertIsCalled_ThenAWindFunctionWithLinearExtrapolateIsGeneratedAndAnErrorMessageIsLogged()
        {
            // Given
            // Construct input set with a single value.
            var startTime = DateTime.Today;
            const double windSpeed = 30.0;
            const double windDirection = 5.0;
            var timeValue = startTime.AddHours(1);

            var timeValues = new List<DateTime>() { timeValue };
            var valuesWindSpeed = new List<double>() { windSpeed };
            var valuesWindDir = new List<double>() { windDirection };

            var inputSet = GetTestWindDefinition("SomeInvalidInterpolationKey",
                                                 null,
                                                 startTime,
                                                 timeValues,
                                                 valuesWindSpeed,
                                                 valuesWindDir);

            var errorMessages = new List<string>();

            // When
            var outputFunc = WindDataConverter.Convert(inputSet, errorMessages);

            // Then
            Assert.That(outputFunc, Is.Not.Null);

            Assert.That(outputFunc.GetInterpolationType(), Is.EqualTo(Flow1DInterpolationType.Linear), "Expected a different interpolation type.");
            Assert.That(outputFunc.GetExtrapolationType(), Is.EqualTo(Flow1DExtrapolationType.Linear), "Expected a different extrapolation type.");
            Assert.That(outputFunc.HasPeriodicity(), Is.EqualTo(false), "Expected a different periodicity.");

            Assert.That(outputFunc.Arguments[0].Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.Arguments[0].Values[0], Is.EqualTo(timeValue));

            Assert.That(outputFunc.Velocity.Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.Velocity.Values[0], Is.EqualTo(windSpeed));

            Assert.That(outputFunc.Direction.Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.Direction.Values[0], Is.EqualTo(windDirection));

            const string expectedErrorMessage =
                "Unable to parse WindFunction interpolation, defaulting to linear-extrapolate.";
            Assert.That(errorMessages.Count, Is.EqualTo(1));
            Assert.That(errorMessages[0], Is.EqualTo(expectedErrorMessage));
        }

        /// <summary>
        /// GIVEN a WindFunction description with an invalid periodicity
        /// WHEN Convert is called
        /// THEN a WindFunction with no periodicity is generated
        ///  AND an error message is logged
        /// </summary>
        [Test]
        public void GivenAWindFunctionDescriptionWithAnInvalidPeriodicity_WhenConvertIsCalled_ThenAWindFunctionWithNoPeriodicityIsGeneratedAndAnErrorMessageIsLogged()
        {
            // Given
            // Construct input set with a single value.
            var startTime = DateTime.Today;
            const double windSpeed = 30.0;
            const double windDirection = 5.0;
            var timeValue = startTime.AddHours(1);

            var timeValues = new List<DateTime>() { timeValue };
            var valuesWindSpeed = new List<double>() { windSpeed };
            var valuesWindDir = new List<double>() { windDirection };

            var inputSet = GetTestWindDefinition(BoundaryRegion.TimeInterpolationStrings.BlockTo,
                                             "SomeInvalidHasPeriodicityKey",
                                             startTime,
                                             timeValues,
                                             valuesWindSpeed,
                                             valuesWindDir);

            var errorMessages = new List<string>();

            // When
            var outputFunc = WindDataConverter.Convert(inputSet, errorMessages);

            // Then
            Assert.That(outputFunc, Is.Not.Null);

            Assert.That(outputFunc.GetInterpolationType(), Is.EqualTo(Flow1DInterpolationType.BlockTo), "Expected a different interpolation type.");
            Assert.That(outputFunc.GetExtrapolationType(), Is.EqualTo(Flow1DExtrapolationType.Constant), "Expected a different extrapolation type.");
            Assert.That(outputFunc.HasPeriodicity(), Is.EqualTo(false), "Expected a different periodicity.");

            Assert.That(outputFunc.Arguments[0].Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.Arguments[0].Values[0], Is.EqualTo(timeValue));

            Assert.That(outputFunc.Velocity.Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.Velocity.Values[0], Is.EqualTo(windSpeed));

            Assert.That(outputFunc.Direction.Values.Count, Is.EqualTo(1));
            Assert.That(outputFunc.Direction.Values[0], Is.EqualTo(windDirection));

            const string expectedErrorMessage =
                "Unable to parse WindFunction periodicity, defaulting to false.";
            Assert.That(errorMessages.Count, Is.EqualTo(1));
            Assert.That(errorMessages[0], Is.EqualTo(expectedErrorMessage));
        }

        /// <summary>
        /// Get a simple test wind definition with the specified parameters.
        /// </summary>
        /// <param name="timeInterpolation">The time interpolation.</param>
        /// <param name="isPeriodic">The is periodic.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="timeValues">The time values.</param>
        /// <param name="valuesWindSpeed">The values wind speed.</param>
        /// <param name="valuesWindDir">The values wind dir.</param>
        /// <returns>A wind definition describing the specified parameters.</returns>
        private static IList<IDelftBcCategory> GetTestWindDefinition(string timeInterpolation,
                                                                     string isPeriodic,
                                                                     DateTime startTime,
                                                                     IList<DateTime> timeValues,
                                                                     IList<double> valuesWindSpeed,
                                                                     IList<double> valuesWindDir)
        {
            var inputSet = new List<IDelftBcCategory>();
            var windSpeedBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var windSpeedDefinition = windSpeedBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                                  BoundaryRegion.FunctionStrings.TimeSeries,
                                                                  timeInterpolation,
                                                                  isPeriodic);

            windSpeedDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                  timeValues,
                                                                                  valuesWindSpeed,
                                                                                  BoundaryRegion.QuantityStrings.WindSpeed,
                                                                                  BoundaryRegion.UnitStrings.WindSpeed);
            inputSet.Add(windSpeedDefinition);

            var windDirBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var windDirDefinition = windDirBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                              BoundaryRegion.FunctionStrings.TimeSeries,
                                                              timeInterpolation,
                                                              isPeriodic);


            windDirDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                timeValues,
                                                                                valuesWindDir,
                                                                                BoundaryRegion.QuantityStrings.WindDirection,
                                                                                BoundaryRegion.UnitStrings.WindDirection);
            inputSet.Add(windDirDefinition);

            return inputSet;
        }
    }
}
