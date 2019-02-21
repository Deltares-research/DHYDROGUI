using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class Flow1DFunctionExtensionsTest
    {
        /// <summary>
        /// GIVEN a function with an interpolation set
        /// WHEN GetInterpolationType is retrieved
        /// THEN the interpolation corresponding with the argument interpolation type is retrieved
        /// </summary>
        [TestCase(InterpolationType.Constant, Flow1DInterpolationType.BlockFrom)]
        [TestCase(InterpolationType.Linear,   Flow1DInterpolationType.Linear)]
        [TestCase(InterpolationType.None,     Flow1DInterpolationType.Linear)]
        public void GivenAFunctionWithNoInterpolationSet_WhenGetInterpolationTypeIsRetrieved_ThenTheInterpolationTypeCorrespondingWithTheArgumentInterpolationTypeIsRetrieved(InterpolationType inInterpolationType,
                                                                                                                                                                              Flow1DInterpolationType expectedOutInterpolationType)
        {
            // Given
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time")
            {
                InterpolationType = inInterpolationType,
            });

            // When
            var result = function.GetInterpolationType();

            // Then
            Assert.That(result,
                Is.EqualTo(expectedOutInterpolationType),
                        $"Expected a different interpolation type when the function interpolation type is {inInterpolationType}.");

            Assert.That(function.Attributes.ContainsKey("Interpolation"),
                Is.True, "Expected function Attributes to have Interpolation defined.");
            Assert.That(function.Attributes["Interpolation"],
                Is.EqualTo(expectedOutInterpolationType.ToString()),
                        "Expected a different interpolation type stored in the attribute dictionary.");
        }

        /// <summary>
        /// GIVEN a function with attributes set
        /// WHEN GetInterpolationType is retrieved
        /// THEN the stored interpolation type is retrieved
        /// </summary>
        [TestCase(Flow1DInterpolationType.BlockFrom)]
        [TestCase(Flow1DInterpolationType.BlockTo)]
        [TestCase(Flow1DInterpolationType.Linear)]
        public void GivenAFunctionWithAttributesSet_WhenGetInterpolationTypeIsRetrieved_ThenTheStoredInterpolationTypeIsRetrieved(Flow1DInterpolationType expectedOutInterpolationType)
        {
            // Given
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time"));
            function.Attributes["Interpolation"] = expectedOutInterpolationType.ToString();

            // When
            var result = function.GetInterpolationType();

            // Then
            Assert.That(result,
                Is.EqualTo(expectedOutInterpolationType),
                        $"Expected a different interpolation type when the function attribute interpolation type is {expectedOutInterpolationType}.");

            Assert.That(function.Attributes.ContainsKey("Interpolation"),
                Is.True, "Expected function Attributes to have Interpolation defined.");
            Assert.That(function.Attributes["Interpolation"],
                Is.EqualTo(expectedOutInterpolationType.ToString()),
                        "Expected a different interpolation type stored in the attribute dictionary.");
        }

        /// <summary>
        /// GIVEN a function with an incorrect interpolation type
        /// WHEN GetInterpolationType is retrieved
        /// THEN an exception is thrown
        /// </summary>
        [Test]
        public void GivenAFunctionWithAnIncorrectInterpolationType_WhenGetInterpolationTypeIsRetrieved_ThenAnExceptionIsThrown()
        {
            // Given
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time"));
            function.Attributes["Interpolation"] = "This is most certainly not something correct";

            var expectedMsg =
                $"The 'Interpolation' in the Attributes dictionary of function {function.Name} has been corrupted.";

            // When | Then
            Assert.That(() => function.GetInterpolationType(), 
            Throws.Exception.TypeOf<InvalidOperationException>().And.Message.EqualTo(expectedMsg),
                "Expected an exception to be thrown when the attribute dict is set incorrectly.");
        }

        /// <summary>
        /// GIVEN a function with no arguments
        ///   AND any Flow1DInterpolationType argument
        /// WHEN SetInterpolationType is called with this argument
        /// THEN nothing is changed
        /// </summary>
        [TestCase(Flow1DInterpolationType.BlockFrom)]
        [TestCase(Flow1DInterpolationType.BlockTo)]
        [TestCase(Flow1DInterpolationType.Linear)]
        public void GivenAFunctionWithNoArgumentsAndAnyArgument_WhenSetInterpolationTypeIsCalledWithThisArgument_ThenNothingIsChanged(Flow1DInterpolationType arg)
        {
            // Given
            var mockedFunction = MockRepository.GenerateStrictMock<IFunction>();
            mockedFunction.Expect(f => f.Arguments).Return(null);

            // When 
            mockedFunction.SetInterpolationType(arg);

            // Then
            mockedFunction.VerifyAllExpectations();
        }

        /// <summary>
        /// GIVEN a function with arguments
        ///   AND an interpolation type
        /// WHEN SetInterpolationType is called with this argument
        /// THEN the correct interpolation type is set in attributes
        ///  AND the correct interpolation type is set on the argument
        /// </summary>
        [TestCase(Flow1DInterpolationType.BlockFrom, InterpolationType.Constant)]
        [TestCase(Flow1DInterpolationType.BlockTo,   InterpolationType.Constant)]
        [TestCase(Flow1DInterpolationType.Linear,    InterpolationType.Linear)]
        public void GivenAFunctionWithArgumentsAndInterpolationType_WhenSetInterpolationTypeIsCalledWithThisArgument_ThenTheCorrectInterpolationTypeIsSetInAttributesAndTheCorrectInterpolationTypeIsSetOnTheArgument(Flow1DInterpolationType setValue,
                                                                                                                                                                                                                      InterpolationType expectedType)
        {
            // Given
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time")
            {
                InterpolationType = InterpolationType.None,
            });

            // When
            function.SetInterpolationType(setValue);

            // Then
            Assert.That(function.Arguments[0].InterpolationType,
                        Is.EqualTo(expectedType),
                        $"Expected a different interpolation type when the function is set to type {setValue.ToString()}.");

            Assert.That(function.Attributes.ContainsKey("Interpolation"),
                Is.True, "Expected function Attributes to have Interpolation defined.");
            Assert.That(function.Attributes["Interpolation"],
                        Is.EqualTo(setValue.ToString()),
                        "Expected a different interpolation type stored in the attribute dictionary.");
        }

        /// <summary>
        /// GIVEN a function with no attributes set
        /// WHEN GetExtrapolationType is retrieved
        /// THEN the extrapolation type corresponding with the argument extrapolation type is retrieved
        /// </summary>
        [TestCase(ExtrapolationType.Linear,   Flow1DExtrapolationType.Linear)]
        [TestCase(ExtrapolationType.Constant, Flow1DExtrapolationType.Constant)]
        [TestCase(ExtrapolationType.Periodic, Flow1DExtrapolationType.Constant)]
        [TestCase(ExtrapolationType.None,     Flow1DExtrapolationType.Constant)]
        public void GivenAFunctionWithNoAttributesSet_WhenGetExtrapolationTypeIsRetrieved_ThenTheExtrapolationTypeCorrespondingWithTheArgumentExtrapolationTypeIsRetrieved(ExtrapolationType inExtrapolationType,
                                                                                                                                                                           Flow1DExtrapolationType expectedOutExtrapolationType)
        {
            // Given
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time")
            {
                ExtrapolationType = inExtrapolationType
            });

            // When
            var result = function.GetExtrapolationType();

            // Then
            Assert.That(result,
                Is.EqualTo(expectedOutExtrapolationType),
                        $"Expected a different extrapolation type when the function extrapolation type is {inExtrapolationType}.");

            Assert.That(function.Attributes.ContainsKey("Extrapolation"),
                Is.True, "Expected function Attributes to have Extrapolation defined.");
            Assert.That(function.Attributes["Extrapolation"],
                Is.EqualTo(expectedOutExtrapolationType.ToString()),
                        "Expected a different extrapolation type stored in the attribute dictionary.");
        }

        /// <summary>
        /// GIVEN a function with attributes set
        /// WHEN GetExtrapolationType is retrieved
        /// THEN the stored extrapolation type is retrieved
        /// </summary>
        [TestCase(Flow1DExtrapolationType.Linear)]
        [TestCase(Flow1DExtrapolationType.Constant)]
        public void GivenAFunctionWithAttributesSet_WhenGetExtrapolationTypeIsRetrieved_ThenTheStoredExtrapolationTypeIsRetrieved(Flow1DExtrapolationType extrapolationType)
        {
            // Given
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time"));
            function.Attributes["Extrapolation"] = extrapolationType.ToString();

            // When
            var result = function.GetExtrapolationType();

            // Then
            Assert.That(result,
                Is.EqualTo(extrapolationType),
                        $"Expected a different extrapolation type when the function attribute extrapolation type is {extrapolationType}.");

            Assert.That(function.Attributes.ContainsKey("Extrapolation"),
                Is.True, "Expected function Attributes to have Extrapolation defined.");
            Assert.That(function.Attributes["Extrapolation"],
                Is.EqualTo(extrapolationType.ToString()),
                        "Expected a different extrapolation type stored in the attribute dictionary.");
        }

        /// <summary>
        /// GIVEN a function with an incorrect extrapolation type
        /// WHEN GetExtrapolationType is retrieved
        /// THEN an exception is thrown
        /// </summary>
        [Test]
        public void GivenAFunctionWithAnIncorrectExtrapolationType_WhenGetExtrapolationTypeIsRetrieved_ThenAnExceptionIsThrown()
        {
            // Given
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time"));
            function.Attributes["Extrapolation"] = "This is most certainly not something correct";

            var expectedMsg =
                $"The 'Extrapolation' in the Attributes dictionary of function {function.Name} has been corrupted.";

            // When | Then
            Assert.That(() => function.GetExtrapolationType(),
            Throws.Exception.TypeOf<InvalidOperationException>().And.Message.EqualTo(expectedMsg),
                        "Expected an exception to be thrown when the attribute dict is set incorrectly.");
        }

        /// <summary>
        /// GIVEN a function with no arguments
        ///   AND any argument
        /// WHEN SetExtrapolationType is called with this argument
        /// THEN nothing is changed
        /// </summary>
        [TestCase(Flow1DExtrapolationType.Linear)]
        [TestCase(Flow1DExtrapolationType.Constant)]
        public void GivenAFunctionWithNoArgumentsAndAnyArgument_WhenSetExtrapolationTypeIsCalledWithThisArgument_ThenNothingIsChanged(Flow1DExtrapolationType arg)
        {
            // Given
            var mockedFunction = MockRepository.GenerateStrictMock<IFunction>();
            mockedFunction.Expect(f => f.Arguments).Return(null);

            // When 
            mockedFunction.SetExtrapolationType(arg);

            // Then
            mockedFunction.VerifyAllExpectations();
        }

        /// <summary>
        /// GIVEN a function with arguments
        ///   AND extrapolation type
        /// WHEN SetExtrapolationType is called with this argument
        /// THEN the correct extrapolation type is set in attributes
        ///  AND the correct extrapolation type is set on the argument
        /// </summary>
        [TestCase(Flow1DExtrapolationType.Linear,   ExtrapolationType.Linear)]
        [TestCase(Flow1DExtrapolationType.Constant, ExtrapolationType.Constant)]
        public void GivenAFunctionWithArgumentsAndExtrapolationType_WhenSetExtrapolationTypeIsCalledWithThisArgument_ThenTheCorrectExtrapolationTypeIsSetInAttributesAndTheCorrectExtrapolationTypeIsSetOnTheArgument(Flow1DExtrapolationType setValue,
                                                                                                                                                                                                                      ExtrapolationType expectedType)
        {
            // Given
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time")
            {
                ExtrapolationType = ExtrapolationType.Constant
            });

            // When
            function.SetExtrapolationType(setValue);

            // Then
            Assert.That(function.Arguments[0].ExtrapolationType,
                Is.EqualTo(expectedType),
                        $"Expected a different extrapolation type when the function is set to type {setValue.ToString()}.");

            Assert.That(function.Attributes.ContainsKey("Extrapolation"),
                Is.True, "Expected function Attributes to have Extrapolation defined.");
            Assert.That(function.Attributes["Extrapolation"],
                Is.EqualTo(setValue.ToString()),
                        "Expected a different extrapolation type stored in the attribute dictionary.");
        }

        /// <summary>
        /// GIVEN a function with no arguments
        /// WHEN HasPeriodicity is retrieved
        /// THEN false is returned
        /// </summary>
        [Test]
        public void GivenAFunctionWithNoArguments_WhenHasPeriodicityIsRetrieved_ThenFalseIsReturned()
        {
            // Given
            var mockedFunction = MockRepository.GenerateStrictMock<IFunction>();
            mockedFunction.Expect(f => f.Arguments).Return(null);

            // When 
            var result = mockedFunction.HasPeriodicity();

            // Then
            mockedFunction.VerifyAllExpectations();
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// GIVEN a function with periodicity
        /// WHEN HasPeriodicity is retrieved
        /// THEN true is returned
        /// </summary>
        [Test]
        public void GivenAFunctionWithPeriodicity_WhenHasPeriodicityIsRetrieved_ThenTrueIsReturned()
        {
            // Given
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time")
            {
                ExtrapolationType = ExtrapolationType.Periodic
            });

            // When
            var result = function.HasPeriodicity();

            // Then
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// GIVEN a function without periodicity
        /// WHEN HasPeriodicity is retrieved
        /// THEN false is returned
        /// </summary>
        [Test]
        public void GivenAFunctionWithoutPeriodicity_WhenHasPeriodicityIsRetrieved_ThenFalseIsReturned()
        {
            // Given
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time")
            {
                InterpolationType = InterpolationType.Linear,
                ExtrapolationType = ExtrapolationType.Constant
            });

            // When
            var result = function.HasPeriodicity();

            // Then
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// GIVEN a function with no arguments
        ///   AND any argument
        /// WHEN SetPeriodicity is called with this argument
        /// THEN nothing is changed
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        public void GivenAFunctionWithNoArgumentsAndAnyArgument_WhenSetPeriodicityIsCalledWithThisArgument_ThenNothingIsChanged(bool arg)
        {
            // Given
            var mockedFunction = MockRepository.GenerateStrictMock<IFunction>();
            mockedFunction.Expect(f => f.Arguments).Return(null);

            // When 
            mockedFunction.SetPeriodicity(arg);

            // Then
            mockedFunction.VerifyAllExpectations();
        }

        /// <summary>
        /// GIVEN a function
        /// WHEN SetPeriodicity is called with true
        /// THEN this function has periodicity
        /// </summary>
        [TestCase(Flow1DExtrapolationType.Constant, ExtrapolationType.Constant)]
        [TestCase(Flow1DExtrapolationType.Linear,   ExtrapolationType.Linear)]
        public void GivenAFunction_WhenSetPeriodicityIsCalledWithTrue_ThenThisFunctionHasPeriodicity(Flow1DExtrapolationType previousExtrapolationType,
                                                                                                     ExtrapolationType previousArgExtrapolationType)
        {
            // Given
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time")
            {
                ExtrapolationType = previousArgExtrapolationType
            });

            function.Attributes["Extrapolation"] = previousExtrapolationType.ToString();

            // When
            function.SetPeriodicity(true);

            // Then
            Assert.That(function.Arguments[0].ExtrapolationType, 
                Is.EqualTo(ExtrapolationType.Periodic),
                "Expected the Arguments[0].ExtrapolationType to be different.");
            Assert.That(function.Attributes["Extrapolation"], 
                Is.EqualTo(previousExtrapolationType.ToString()),
                "Expected the attributes 'Extrapolation' to be different.");
        }

        /// <summary>
        /// GIVEN a function with periodicity and a previous extrapolation
        /// WHEN SetPeriodicity is called with false
        /// THEN this function does not have periodicity
        ///  AND the previous extrapolation is restored
        /// </summary>
        [TestCase(Flow1DExtrapolationType.Constant, ExtrapolationType.Constant)]
        [TestCase(Flow1DExtrapolationType.Linear,   ExtrapolationType.Linear)]
        public void GivenAFunctionWithPeriodicityAndAPreviousExtrapolation_WhenSetPeriodicityIsCalledWithFalse_ThenThisFunctionDoesNotHavePeriodicity(Flow1DExtrapolationType previousExtrapolationType,
                                                                                                                                                         ExtrapolationType previousArgExtrapolationType)
        {
            // Given
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time")
            {
                ExtrapolationType = ExtrapolationType.Periodic
            });

            function.Attributes["Extrapolation"] = previousExtrapolationType.ToString();

            // When
            function.SetPeriodicity(false);

            // Then
            Assert.That(function.Arguments[0].ExtrapolationType,
                Is.EqualTo(previousArgExtrapolationType),
                        "Expected the Arguments[0].ExtrapolationType to be different.");
            Assert.That(function.Attributes["Extrapolation"],
                Is.EqualTo(previousExtrapolationType.ToString()),
                        "Expected the attributes 'Extrapolation' to be different.");
        }
    }
}
