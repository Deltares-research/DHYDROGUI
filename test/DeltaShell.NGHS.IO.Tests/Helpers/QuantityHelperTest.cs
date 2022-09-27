using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using NSubstitute;
using NUnit.Framework;
using Is = NUnit.Framework.Is;

namespace DeltaShell.NGHS.IO.Tests.Helpers
{
    [TestFixture]
    public class QuantityHelperTest
    {
        private const string orificeQuantity = "orifice_gateLowerEdgeLevel";
        private const string orificeCrestLevelQuantity = "orifice_crestLevel";
        private const string pumpQuantity = "pump_capacity";
        private const string weirQuantity = "weir_crestLevel";
        private const string culvertQuantity = "culvert_valveOpeningHeight";
        private const string nameOfUnit = "m";
        private const string nameQuantity = "Quantity";
        
        private const string timeSeriesNameCrestLevel = "Crest level";
        private const string timeSeriesNameSeriesName = "GateLowerEdgeLevel";
        private const string timeSeriesNameGateOpening = "Gate opening";

        private static IEnumerable<TestCaseData> StructureTypes()
        {
            yield return new TestCaseData(new Orifice(), timeSeriesNameCrestLevel, orificeCrestLevelQuantity);
            yield return new TestCaseData(new Orifice(), timeSeriesNameSeriesName, orificeQuantity);
            yield return new TestCaseData(new Orifice(), timeSeriesNameGateOpening, orificeQuantity);
            yield return new TestCaseData(new Pump(), pumpQuantity, pumpQuantity);
            yield return new TestCaseData(new Weir(), weirQuantity, weirQuantity);
            yield return new TestCaseData(new Culvert(), culvertQuantity, culvertQuantity);
        }
        
        [Test]
        [TestCaseSource(nameof(StructureTypes))]
        public void WhenGetQuantityData_ThenReturnExpectedString(IStructure1D structure, string timeSeriesName, string expectedString)
        {
            //Arrange & Act & Assert
            Assert.That(QuantityHelper.GetQuantity(structure, timeSeriesName), Is.EqualTo(expectedString));
        }
        
        private static IEnumerable<TestCaseData> OrificeStructures()
        {
            yield return new TestCaseData(timeSeriesNameCrestLevel, orificeCrestLevelQuantity);
            yield return new TestCaseData(timeSeriesNameSeriesName, orificeQuantity);
            yield return new TestCaseData(timeSeriesNameGateOpening, orificeQuantity);
        }
        
        [Test]
        [TestCaseSource(nameof(OrificeStructures))]
        public void WhenGetQuantityDataWithTimeSeriesNameOnly_ThenReturnExpectedString_ReturnEmptyStringIfNotSupported(string timeSeriesName, string expectedString)
        {
            //Arrange & Act & Assert
            Assert.That(QuantityHelper.GetQuantity(new Orifice(), timeSeriesName), Is.EqualTo(expectedString));
        }
        
        [Test]
        public void WhenGetQuantityDataWithTimeSeriesNameOnly_InvalidName_ThrowsNotSupportedException()
        {
            // Setup
            const string invalidName = "invalidName";

            // Call
            void Action() => QuantityHelper.GetQuantity(new Orifice(),invalidName);

            // Assert
            Assert.That(Action, Throws.TypeOf<NotSupportedException>());
        }
        
        [Test]
        public void WhenGetQuantityDataWithTimeSeriesNameOnly_Null_ThrowsArgumentNullException()
        {
            // Call
            void Action() => QuantityHelper.GetQuantity(new Orifice(),null);

            // Assert
            Assert.That(Action, Throws.TypeOf<ArgumentNullException>());
        }
        
        [Test]
        public void WhenGetQuantityDataWithStructureOnly_Null_ThrowsArgumentNullException()
        {
            // Call
            void Action() => QuantityHelper.GetQuantity(null,"");

            // Assert
            Assert.That(Action, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetQuantity_InvalidStructureType_ThrowsNotSupportedException()
        {
            // Setup
            var invalidStructureType = new Gate();

            // Call
            void Action() => QuantityHelper.GetQuantity(invalidStructureType, "");

            // Assert
            Assert.That(Action, Throws.TypeOf<NotSupportedException>());
        }
        
        [Test]
        public void WhenGetQuantityData_GivenEmptyString_ReturnKeyStandardNameFromFunctionAndUnitNameAsExpectedStrings()
        {
            //Arrange
            IFunction function = InitializeSubstituteFunction();;

            //Act
            var data = QuantityHelper.GetQuantityAndUnit(function, string.Empty);
            
            //Assert
            Assert.That(data.ContainsKey(nameQuantity), Is.True);
            Assert.That(data[nameQuantity], Is.EqualTo(nameOfUnit));
        }
        
        [Test]
        public void WhenGetQuantityData_GivenQuantityStringName_ThenReturnKeyAndUnitNameAsExpectedStrings()
        {
            //Arrange 
            IFunction function = InitializeSubstituteFunction();;
            const string expectedName = "QuantityChangeName";

            //Act
            var data = QuantityHelper.GetQuantityAndUnit(function, expectedName);
            
            //Assert
            Assert.That(data.ContainsKey(expectedName), Is.True);
            Assert.That(data[expectedName], Is.EqualTo(nameOfUnit));
        }
        
        private static IFunction InitializeSubstituteFunction()
        {
            IFunction function = Substitute.For<IFunction>();
            IUnit unit = Substitute.For<IUnit>();
            IVariable variable = Substitute.For<IVariable>();
            unit.Name.Returns(nameOfUnit);
            variable.Unit.Returns(unit);
            function.Name.Returns(nameQuantity);
            EventedList<IVariable> listWithUnit = new EventedList<IVariable> {variable};
            function.Components.Returns(listWithUnit);
            return function;
        }
    }
}