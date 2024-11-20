using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class SideViewWaterLevelFunctionUpdaterTest
    {
        [Test]
        [TestCaseSource(nameof(UpdateFunctionWithExtraDataPointsForStructures_ArgumentNullCases))]
        public void UpdateFunctionWithExtraDataPointsForStructures_ArgumentNull_ThrowsNullException(
            IFunction function, IReadOnlyList<double> structureChainages)
        {
            // Call
            TestDelegate call = () => SideViewWaterLevelFunctionUpdater.UpdateFunctionWithExtraDataPointsForStructures(
                function, structureChainages);
            
            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        private static IEnumerable<TestCaseData> UpdateFunctionWithExtraDataPointsForStructures_ArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IReadOnlyList<double>>());
            yield return new TestCaseData(Substitute.For<IFunction>(), null);
        }

        [Test]
        public void UpdateFunctionWithExtraDataPointsForStructures_AddingMultipleStructures_CorrectlyUpdatesExistingFunction()
        {
            // Setup
            double[] existingChainages = { 5.0, 7.0, 10.0 };
            double[] existingWaterLevels = { 1.5, 2.5, 3.5};
            IFunction function = CreateWaterLevelFunction(existingChainages, existingWaterLevels);

            var structureChainages = new []{ 1.0, 6.0, 100.0 };

            // Call
            SideViewWaterLevelFunctionUpdater.UpdateFunctionWithExtraDataPointsForStructures(function, structureChainages);

            // Assert
            Assert.That(function, Is.Not.Null);

            IMultiDimensionalArray<double> updatedChainages = function.Arguments[0].GetValues<double>();
            double[] expectedChainages = { 0.999, 1.001, 5.0, 5.999, 6.001, 7.0, 10.0, 99.999, 100.001 }; // two new data points per structure
            Assert.That(updatedChainages, Is.EqualTo(expectedChainages));

            IMultiDimensionalArray<double> updatedWaterLevels = function.Components[0].GetValues<double>();

            double[] expectedWaterLevels = { 1.5, 1.5, 1.5, 1.5, 2.5, 2.5, 3.5, 3.5, 3.5 };
            Assert.That(updatedWaterLevels, Is.EqualTo(expectedWaterLevels));
        }

        [Test]
        public void UpdateFunctionWithExtraDataPointsForStructures_AddingUnsortedStructures_ThrowsInvalidOperationException()
        {
            // Setup - non monotonous structure chainages
            double[] existingChainages =   { 5.0, 7.0, 10.0 };
            double[] existingWaterLevels = { 1.5, 2.5, 3.5 };
            double[] structureChainages =  { 7.1, 5.5, 10.2 };  
            IFunction function = CreateWaterLevelFunction(existingChainages, existingWaterLevels);

            TestDelegate call = () => SideViewWaterLevelFunctionUpdater.UpdateFunctionWithExtraDataPointsForStructures(function, structureChainages);
            
            // Assert
            Assert.That( call, Throws.InvalidOperationException);
        }

        [Test]
        public void UpdateFunctionWithExtraDataPointsForStructures_AddingUnsortedChainages_ThrowsInvalidOperationException()
        {
            // Setup - non-monotonous location chainage
            double[] existingChainages =   { 5.0, 6.0, 5.4 };
            double[] existingWaterLevels = { 1.5, 3.5, 2.5 };
            double[] structureChainages =  { 5.5 };  
            IFunction function = CreateWaterLevelFunction(existingChainages, existingWaterLevels);

            TestDelegate call = () => SideViewWaterLevelFunctionUpdater.UpdateFunctionWithExtraDataPointsForStructures(function, structureChainages);
            // Assert
            Assert.That( call, Throws.InvalidOperationException );
        }
        

        private static IFunction CreateWaterLevelFunction(IEnumerable<double> existingChainages, IEnumerable<double> existingWaterLevels)
        {
            IVariable waterLevelVariable = CreateVariableWithValues("Water Level", "m AD", existingWaterLevels);
            IVariable chainageVariable = CreateVariableWithValues("Chainage", "m", existingChainages);

            IFunction function = new Function();

            function.Arguments.Add(chainageVariable);
            function.Components.Add(waterLevelVariable);
            function.Name = "Water Level";

            return function;
        }

        private static IVariable CreateVariableWithValues(string name, string unit, IEnumerable<double> values)
        {
            IVariable variable = new Variable<double>(name)
            {
                Unit = new Unit(name, unit)
            };

            FunctionHelper.SetValuesRaw<double>(variable, values);

            return variable;
        }
    }
}