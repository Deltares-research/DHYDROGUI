using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using NUnit.Framework;

using HasComponent = DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary.BoundaryTestHelper.HasComponent;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    [TestFixture]
    public class LateralDischargeConverterTest
    {
        #region setup
        private LateralDischargeWater constantWaterComponent;
        private LateralDischargeWater tableWaterComponent;
        private LateralDischargeWater timeDependentWaterComponent;

        private LateralDischargeSalt constantSaltMassComponent;
        private LateralDischargeSalt timeDependentSaltMassComponent;

        private LateralDischargeSalt constantSaltConcentrationComponent;
        private LateralDischargeSalt timeDependentSaltConcentrationComponent;

        private LateralDischargeTemperature constantTemperatureComponent;
        private LateralDischargeTemperature timeDependentTemperatureComponent;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // Water
            constantWaterComponent = new LateralDischargeWater(WaterFlowModel1DLateralDataType.FlowConstant, InterpolationType.Constant, false, 21.0);

            var valuesWaterLevelTable = new List<double>() { 55.0, 520.0, 1150.0, 1530.0 };
            var valuesWaterDischargeTable = new List<double>() { 95.0, 120.0, 210.0, 430.0 };
            var tableFunction = new Function();

            tableFunction.Arguments.Add(new Variable<double>(BoundaryRegion.QuantityStrings.WaterLevel)
            {
                InterpolationType = InterpolationType.Linear,
                ExtrapolationType = ExtrapolationType.Periodic
            });

            tableFunction.Components.Add(new Variable<double>(BoundaryRegion.QuantityStrings.WaterDischarge, new Unit("", "")));
            tableFunction.Arguments[0].SetValues(valuesWaterLevelTable);
            tableFunction.Components[0].SetValues(valuesWaterDischargeTable);
            tableWaterComponent = new LateralDischargeWater(WaterFlowModel1DLateralDataType.FlowWaterLevelTable, InterpolationType.Linear, true, tableFunction);

            var startTime = DateTime.Today;
            var valuesWaterLevel = new List<double>() { 5.0, 20.0, 10.0, 30.0 };
            var timeValuesWaterLevel = new List<DateTime>()
            {
                startTime.AddHours(2),
                startTime.AddHours(4),
                startTime.AddHours(6),
                startTime.AddHours(8),
            };

            var functionWaterLevel = BoundaryTestHelper.GetNewTimeFunction("Water Level", "", "");
            functionWaterLevel.Arguments[0].SetValues(timeValuesWaterLevel);
            functionWaterLevel.Components[0].SetValues(valuesWaterLevel);
            timeDependentWaterComponent = new LateralDischargeWater(WaterFlowModel1DLateralDataType.FlowTimeSeries,
                                                                    InterpolationType.Linear,
                                                                    true,
                                                                    functionWaterLevel);

            // Salt
            constantSaltMassComponent = 
                new LateralDischargeSalt(SaltLateralDischargeType.MassConstant, InterpolationType.Constant, false, 23.0);
            constantSaltConcentrationComponent = 
                new LateralDischargeSalt(SaltLateralDischargeType.ConcentrationConstant, InterpolationType.Constant, false, 25.0);


            var valuesSaltMass = new List<double>() { 7.0, 22.0, 12.0, 32.0 };
            var timeValuesSaltMass = new List<DateTime>()
            {
                startTime.AddHours(4),
                startTime.AddHours(6),
                startTime.AddHours(8),
                startTime.AddHours(10),
            };

            var functionSaltMass = BoundaryTestHelper.GetNewTimeFunction(BoundaryRegion.QuantityStrings.WaterSalinity, "", "");
            functionSaltMass.Arguments[0].SetValues(timeValuesSaltMass);
            functionSaltMass.Components[0].SetValues(valuesSaltMass);
            timeDependentSaltMassComponent = new LateralDischargeSalt(SaltLateralDischargeType.MassTimeSeries,
                                                                      InterpolationType.Linear,
                                                                      true,
                                                                      functionSaltMass);

            var valuesSaltConcentration = new List<double>() { 7.0, 22.0, 12.0, 32.0 };
            var timeValuesSaltConcentration = new List<DateTime>()
            {
                startTime.AddHours(8),
                startTime.AddHours(10),
                startTime.AddHours(12),
                startTime.AddHours(14),
            };

            var functionSaltConcentration = BoundaryTestHelper.GetNewTimeFunction(BoundaryRegion.QuantityStrings.WaterSalinity, "", "");
            functionSaltConcentration.Arguments[0].SetValues(timeValuesSaltConcentration);
            functionSaltConcentration.Components[0].SetValues(valuesSaltConcentration);
            timeDependentSaltConcentrationComponent = new LateralDischargeSalt(SaltLateralDischargeType.ConcentrationTimeSeries,
                                                                               InterpolationType.Linear,
                                                                               true,
                                                                               functionSaltMass);

            // Temperature
            constantTemperatureComponent = new LateralDischargeTemperature(TemperatureLateralDischargeType.Constant, InterpolationType.Constant, false, 24.0);

            var valuesTemperature = new List<double>() { 5.0, 20.0, 10.0, 30.0 };
            var timeValuesTemperature = new List<DateTime>()
            {
                startTime.AddHours(1),
                startTime.AddHours(2),
                startTime.AddHours(3),
                startTime.AddHours(4),
            };
            var functionTemperature = BoundaryTestHelper.GetNewTimeFunction(BoundaryRegion.QuantityStrings.WaterTemperature, "", "");
            functionTemperature.Arguments[0].SetValues(timeValuesTemperature);
            functionTemperature.Components[0].SetValues(valuesTemperature);
            timeDependentTemperatureComponent = new LateralDischargeTemperature(TemperatureLateralDischargeType.TimeDependent,
                                                                                 InterpolationType.Linear,
                                                                                 true,
                                                                                 functionTemperature);

        }

        #endregion

        #region UnitTests
        /// <summary>
        /// GIVEN a null set of DelftBcCategories
        ///   AND an empty list of error messages
        /// WHEN LateralDischargeConverter convert is called with these parameters
        /// THEN an empty set will be returned
        ///  AND a single error is logged
        /// </summary>
        [Test]
        public void GivenANullSetOfDelftBcCategoriesAndAnEmptyListOfErrorMessages_WhenLateralDischargeConverterConvertIsCalledWithTheseParameters_ThenAnEmptySetWillBeReturnedAndASingleErrorIsLogged()
        {
            // Given
            const IList<IDelftBcCategory> nullSet = null;
            var errorMessages = new List<string>();

            // When
            var output = LateralDischargeConverter.Convert(nullSet, errorMessages);

            // Then
            AssertEmptyOutput(output);
            AssertSingleErrorMessage(errorMessages, "Unable to parse null set of LateralDischarges.");
        }

        /// <summary>
        /// GIVEN an empty set of DelftBcCategories
        ///   AND an empty list of error messages
        /// WHEN LateralDischargeConverter convert is called with these parameters
        /// THEN an empty set will be returned
        ///  AND a single error is logged
        /// </summary>
        [Test]
        public void GivenAnEmptySetOfDelftBcCategoriesAndAnEmptyListOfErrorMessages_WhenLateralDischargeConverterConvertIsCalledWithTheseParameters_ThenAnEmptySetWillBeReturnedAndASingleErrorIsLogged()
        {
            var emptySet = new List<IDelftBcCategory>();
            var errorMessages = new List<string>();

            // When
            var output = LateralDischargeConverter.Convert(emptySet, errorMessages);

            // Then
            AssertEmptyOutput(output);
            AssertSingleErrorMessage(errorMessages, "Unable to parse empty set of LateralDischarges.");
        }

        /// <summary> an empty set will be returned </summary>
        private void AssertEmptyOutput(Dictionary<string, LateralDischarge> output)
        {
            Assert.That(output, Is.Not.Null);
            Assert.That(output, Is.Empty);
        }

        /// <summary> a single error is logged </summary>
        private void AssertSingleErrorMessage(IList<string> errorMessages, string expectedErrorMessage)
        {
            Assert.That(errorMessages.Count, Is.EqualTo(1));
            Assert.That(errorMessages[0], Is.EqualTo(expectedErrorMessage));
        }


        /// <summary>
        /// GIVEN a set of DelftBcCategories describing LateralDischarges containing all components on the same node
        ///   AND an empty list of error messages
        /// WHEN LateralDischargeConverter convert is called with these parameters
        /// THEN A set containing a single LateralDischarge corresponding with the input is returned
        ///  AND three errors are logged
        /// </summary>
        [Test]
        public void GivenASetOfDelftBcCategoriesDescribingLateralDischargesContainingAllComponentsOnTheSameNodeAndAnEmptyListOfErrorMessages_WhenLateralDischargeConverterConvertIsCalledWithTheseParameters_ThenASetContainingASingleLateralDischargeCorrespondingWithTheInputIsReturnedAndThreeErrorsAreLogged()
        {
            var inputSet = new List<IDelftBcCategory>();
            var errorMessages = new List<string>();

            const string nodeName = "Tenderloin";
            var boundaryCondition1 = GetLateralDischarge(nodeName, HasComponent.Constant, HasComponent.Constant, SaltType.Concentration, HasComponent.Constant);
            inputSet.AddRange(ToBcCategories(boundaryCondition1));

            var boundaryCondition2 = GetLateralDischarge(nodeName, HasComponent.Constant, HasComponent.Constant, SaltType.Mass, HasComponent.Constant);
            inputSet.AddRange(ToBcCategories(boundaryCondition2));

            for (var i = 0; i < inputSet.Count; i++)
                inputSet[i].LineNumber = 10 * i;

            // When
            var output = LateralDischargeConverter.Convert(inputSet, errorMessages);

            // Then
            AssertOutputEqualsExpectedOutput(output, 
                                             new Dictionary<string, LateralDischarge>() { {nodeName, boundaryCondition1} });
            AssertErrorMessagesAreLogged(errorMessages, 
                                         new List<string>()
                                         {
                                             $"Could not parse lateral discharge category: {inputSet[3].Name} at line {inputSet[3].LineNumber}: Component has already been defined",
                                             $"Could not parse lateral discharge category: {inputSet[4].Name} at line {inputSet[4].LineNumber}: Component has already been defined",
                                             $"Could not parse lateral discharge category: {inputSet[5].Name} at line {inputSet[5].LineNumber}: Component has already been defined"
                                         });
        }

        private void AssertOutputEqualsExpectedOutput(IDictionary<string, LateralDischarge> output, IDictionary<string, LateralDischarge> expectedOutput)
        {
            Assert.That(expectedOutput, Is.Not.Null);
            Assert.That(output, Is.Not.Null);

            Assert.That(output.Count, Is.EqualTo(expectedOutput.Count));

            foreach (var expectedKey in expectedOutput.Keys)
            {
                Assert.That(output.ContainsKey(expectedKey));
                AssertThatLateralDischargeIsEqualTo(output[expectedKey], expectedOutput[expectedKey]);
            }
        }

        private void AssertErrorMessagesAreLogged(IList<string> loggedErrorMessages,
                                                  IList<string> expectedErrorMessages)
        {
            Assert.That(loggedErrorMessages.Count, Is.EqualTo(expectedErrorMessages.Count));

            foreach(var expectedErrorMessage in expectedErrorMessages)
                Assert.That(loggedErrorMessages.Contains(expectedErrorMessage));
        }

        /// <summary>
        /// GIVEN a set of IDelftBcCategories describing a single LateralDischarge
        ///   AND an empty list of error messages
        /// WHEN LateralDischargeConverter convert is called with these parameters
        /// THEN A set containing a single LateralDischarge corresponding with the input is returned
        ///  AND no errors are logged
        /// </summary>
        [TestCase(HasComponent.None, HasComponent.None, SaltType.None, HasComponent.Constant)]
        [TestCase(HasComponent.None, HasComponent.None, SaltType.None, HasComponent.TimeDependent)]

        [TestCase(HasComponent.None, HasComponent.Constant, SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.None, HasComponent.Constant, SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.None, HasComponent.Constant, SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.None, HasComponent.Constant, SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.None, HasComponent.Constant, SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.None, HasComponent.Constant, SaltType.Concentration, HasComponent.TimeDependent)]

        [TestCase(HasComponent.None, HasComponent.TimeDependent, SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.None, HasComponent.TimeDependent, SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.None, HasComponent.TimeDependent, SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.None, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.None, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.None, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.TimeDependent)]


        [TestCase(HasComponent.Constant, HasComponent.None, SaltType.None, HasComponent.None)]
        [TestCase(HasComponent.Constant, HasComponent.None, SaltType.None, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, HasComponent.None, SaltType.None, HasComponent.TimeDependent)]

        [TestCase(HasComponent.Constant, HasComponent.Constant, SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.Constant, HasComponent.Constant, SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, HasComponent.Constant, SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant, HasComponent.Constant, SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.Constant, HasComponent.Constant, SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, HasComponent.Constant, SaltType.Concentration, HasComponent.TimeDependent)]

        [TestCase(HasComponent.Constant, HasComponent.TimeDependent, SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.Constant, HasComponent.TimeDependent, SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, HasComponent.TimeDependent, SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.Constant, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.TimeDependent)]


        [TestCase(HasComponent.Table, HasComponent.None, SaltType.None, HasComponent.None)]
        [TestCase(HasComponent.Table, HasComponent.None, SaltType.None, HasComponent.Constant)]
        [TestCase(HasComponent.Table, HasComponent.None, SaltType.None, HasComponent.TimeDependent)]

        [TestCase(HasComponent.Table, HasComponent.Constant, SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.Table, HasComponent.Constant, SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.Table, HasComponent.Constant, SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Table, HasComponent.Constant, SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.Table, HasComponent.Constant, SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.Table, HasComponent.Constant, SaltType.Concentration, HasComponent.TimeDependent)]

        [TestCase(HasComponent.Table, HasComponent.TimeDependent, SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.Table, HasComponent.TimeDependent, SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.Table, HasComponent.TimeDependent, SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Table, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.Table, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.Table, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.TimeDependent)]


        [TestCase(HasComponent.TimeDependent, HasComponent.None, SaltType.None, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, HasComponent.None, SaltType.None, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, HasComponent.None, SaltType.None, HasComponent.TimeDependent)]

        [TestCase(HasComponent.TimeDependent, HasComponent.Constant, SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, HasComponent.Constant, SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, HasComponent.Constant, SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, HasComponent.Constant, SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, HasComponent.Constant, SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, HasComponent.Constant, SaltType.Concentration, HasComponent.TimeDependent)]

        [TestCase(HasComponent.TimeDependent, HasComponent.TimeDependent, SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, HasComponent.TimeDependent, SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, HasComponent.TimeDependent, SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.TimeDependent)]
        public void GivenASetOfIDelftBcCategoriesDescribingASingleLateralDischargeAndAnEmptyListOfErrorMessages_WhenLateralDischargeConverterConvertIsCalledWithTheseParameters_ThenASetContainingASingleLateralDischargeCorrespondingWithTheInputIsReturnedAndNoErrorsAreLogged(HasComponent hasWater, HasComponent hasSalt, SaltType saltType, HasComponent hasTemperature)
        {
            const string nodeName = "Bacon";
            var boundaryCondition = GetLateralDischarge(nodeName, hasWater, hasSalt, saltType, hasTemperature);
            var inputSet = ToBcCategories(boundaryCondition);

            var errorMessages = new List<string>();

            // When
            var output = LateralDischargeConverter.Convert(inputSet, errorMessages);

            // Then
            AssertOutputEqualsExpectedOutput(output,
                                             new Dictionary<string, LateralDischarge>() { { nodeName, boundaryCondition } });
            Assert.That(errorMessages, Is.Empty);

        }

        /// <summary>
        /// GIVEN a set of IDelftBcCategories describing a multiple LateralDischarges
        ///   AND an empty list of error messages
        /// WHEN LateralDischargeConverter convert is called with these parameters
        /// THEN A set containing the corresponding LateralDischarges is returned
        ///  AND no errors are logged
        /// </summary>
        [Test]
        public void GivenASetOfIDelftBcCategoriesDescribingAMultipleLateralDischargesAndAnEmptyListOfErrorMessages_WhenLateralDischargeConverterConvertIsCalledWithTheseParameters_ThenASetContainingTheCorrespondingLateralDischargesIsReturnedAndNoErrorsAreLogged()
        {
            var inputNodes = new List<LateralDischarge>()
            {
                GetLateralDischarge("Prosciutto", HasComponent.Constant,      HasComponent.None,          SaltType.Mass,          HasComponent.None),
                GetLateralDischarge("Drumstick",  HasComponent.TimeDependent, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.TimeDependent),
                GetLateralDischarge("Venison",    HasComponent.TimeDependent, HasComponent.None,          SaltType.Concentration, HasComponent.None),
                GetLateralDischarge("T-bone",     HasComponent.Table,         HasComponent.Constant,      SaltType.Mass,          HasComponent.None),
                GetLateralDischarge("Ribs",       HasComponent.Constant,      HasComponent.None,          SaltType.Mass,          HasComponent.Constant),
            };

            var errorMessages = new List<string>();

            var inputNodesCategory = new List<IDelftBcCategory>();
            foreach (var n in inputNodes) inputNodesCategory.AddRange(ToBcCategories(n));
            BoundaryTestHelper.Shuffle(inputNodesCategory);

            // When
            var output = LateralDischargeConverter.Convert(inputNodesCategory, errorMessages);

            // Then
            AssertOutputEqualsExpectedOutput(output, 
                new Dictionary<string, LateralDischarge>()
                {
                    { inputNodes[0].Name, inputNodes[0] },
                    { inputNodes[1].Name, inputNodes[1] },
                    { inputNodes[2].Name, inputNodes[2] },
                    { inputNodes[3].Name, inputNodes[3] },
                    { inputNodes[4].Name, inputNodes[4] },
                });

            Assert.That(errorMessages, Is.Empty);
        }

        /// <summary>
        /// GIVEN a set of IDelftBcCategories describing a multiple LateralDischarges and other data
        ///   AND an empty list of error messages
        /// WHEN LateralDischargeConverter convert is called with these parameters
        /// THEN A set containing the corresponding LateralDischarges is returned
        ///  AND no errors are logged
        /// </summary>
        [Test]
        public void GivenASetOfIDelftBcCategoriesDescribingAMultipleLateralDischargesAndOtherDataAndAnEmptyListOfErrorMessages_WhenLateralDischargeConverterConvertIsCalledWithTheseParameters_ThenASetContainingTheCorrespondingLateralDischargesIsReturnedAndNoErrorsAreLogged()
        {
            var inputNodes = new List<LateralDischarge>()
            {
                GetLateralDischarge("Prosciutto", HasComponent.Constant,      HasComponent.None,          SaltType.Mass,          HasComponent.None),
                GetLateralDischarge("Drumstick",  HasComponent.TimeDependent, HasComponent.TimeDependent, SaltType.Concentration, HasComponent.TimeDependent),
                GetLateralDischarge("Venison",    HasComponent.TimeDependent, HasComponent.None,          SaltType.Concentration, HasComponent.None),
                GetLateralDischarge("T-bone",     HasComponent.Table,         HasComponent.Constant,      SaltType.Mass,          HasComponent.None),
                GetLateralDischarge("Ribs",       HasComponent.Constant,      HasComponent.None,          SaltType.Mass,          HasComponent.Constant),
            };

            var errorMessages = new List<string>();

            var inputNodesCategory = new List<IDelftBcCategory>();
            foreach (var n in inputNodes) inputNodesCategory.AddRange(ToBcCategories(n));
            inputNodesCategory.Add(new DelftBcCategory("Sausage"));
            inputNodesCategory.Add(new DelftBcCategory("Meatloaf"));
            inputNodesCategory.Add(new DelftBcCategory("Pancetta"));
            inputNodesCategory.Add(new DelftBcCategory("Pork"));
            inputNodesCategory.Add(new DelftBcCategory("Pastrami"));

            BoundaryTestHelper.Shuffle(inputNodesCategory);

            // When
            var output = LateralDischargeConverter.Convert(inputNodesCategory, errorMessages);

            // Then
            AssertOutputEqualsExpectedOutput(output,
                new Dictionary<string, LateralDischarge>()
                {
                    { inputNodes[0].Name, inputNodes[0] },
                    { inputNodes[1].Name, inputNodes[1] },
                    { inputNodes[2].Name, inputNodes[2] },
                    { inputNodes[3].Name, inputNodes[3] },
                    { inputNodes[4].Name, inputNodes[4] },
                });

            Assert.That(errorMessages, Is.Empty);
        }

        /// <summary>
        /// GIVEN a LateralDischarge with a default salt value
        ///   AND a set containing this LateralDischarge
        ///   AND an empty list of error messages
        /// WHEN LateralDischargeConverter convert is called with these parameters
        /// THEN A set containing a single LateralDischarge with a Default salt value is returned
        ///  AND no errors are logged
        /// </summary>
        [Test]
        public void GivenALateralDischargeWithADefaultSaltValueAndASetContainingThisLateralDischargeAndAnEmptyListOfErrorMessages_WhenLateralDischargeConverterConvertIsCalledWithTheseParameters_ThenASetContainingASingleLateralDischargeWithADefaultSaltValueIsReturnedAndNoErrorsAreLogged()
        {
            // Given
            const string nodeName = "Tenderloin";
            var lateralDischargeSalt =
                new LateralDischargeSalt(SaltLateralDischargeType.Default, InterpolationType.Constant, false, 0.0);
            var lateralDischarge = GetLateralDischarge(nodeName, HasComponent.Constant, HasComponent.None, SaltType.None, HasComponent.None);
            lateralDischarge.SaltComponent = lateralDischargeSalt;

            var inputNodesCategory = new List<IDelftBcCategory>();
            inputNodesCategory.AddRange(ToBcCategories(lateralDischarge));

            var errorMessages = new List<string>();

            // When
            var output = LateralDischargeConverter.Convert(inputNodesCategory, errorMessages);

            // Then
            AssertOutputEqualsExpectedOutput(output, new Dictionary<string, LateralDischarge>() { { nodeName, lateralDischarge } });
            Assert.That(errorMessages, Is.Empty);
        }
        #endregion

        #region testhelpers

        public enum SaltType
        {
            None,
            Mass,
            Concentration,
        }

        private LateralDischarge GetLateralDischarge(string name, HasComponent water, HasComponent salt, SaltType type,  HasComponent temperature)
        {
            var lateralDischarge = new LateralDischarge(name);

            switch (water)
            {
                case HasComponent.Constant:
                    lateralDischarge.WaterComponent = constantWaterComponent;
                    break;
                case HasComponent.Table:
                    lateralDischarge.WaterComponent = tableWaterComponent;
                    break;
                case HasComponent.TimeDependent:
                    lateralDischarge.WaterComponent = timeDependentWaterComponent;
                    break;
            }

            switch (salt)
            {
                case HasComponent.Constant:
                    lateralDischarge.SaltComponent = type == SaltType.Concentration
                        ? constantSaltConcentrationComponent
                        : constantSaltMassComponent;
                    break;
                case HasComponent.TimeDependent:
                    lateralDischarge.SaltComponent = type == SaltType.Concentration
                        ? timeDependentSaltConcentrationComponent
                        : timeDependentSaltMassComponent;
                    break;
            }

            switch (temperature)
            {
                case HasComponent.Constant:
                    lateralDischarge.TemperatureComponent = constantTemperatureComponent;
                    break;
                case HasComponent.TimeDependent:
                    lateralDischarge.TemperatureComponent = timeDependentTemperatureComponent;
                    break;
            }

            return lateralDischarge;
        }

        public static IList<IDelftBcCategory> ToBcCategories(LateralDischarge lateralDischarge)
        {
            var result = new List<IDelftBcCategory>();


            if (lateralDischarge.WaterComponent != null)
                result.Add(GetWaterComponentCategory(lateralDischarge.Name, lateralDischarge.WaterComponent));

            if (lateralDischarge.SaltComponent != null)
                result.Add(GetSaltComponentCategory(lateralDischarge.Name, lateralDischarge.SaltComponent));

            if (lateralDischarge.TemperatureComponent != null)
                result.Add(GetTemperatureComponentCategory(lateralDischarge.Name, lateralDischarge.TemperatureComponent));

            return result;
        }

        private static IDelftBcCategory GetWaterComponentCategory(string name, LateralDischargeWater lateralDischarge)
        {
            // Set common elements
            var boundaryDefinition = BoundaryTestHelper.GetCommonCategory(BoundaryRegion.BcLateralHeader,
                                                                          name,
                                                                          BoundaryFileWriterHelper.GetFunctionString(lateralDischarge.BoundaryType),
                                                                          lateralDischarge.InterpolationType,
                                                                          BoundaryTestHelper.GetTimeSeriesIsPeriodicProperty(lateralDischarge.TimeDependentBoundaryValue));

            // Create actual table
            switch (lateralDischarge.BoundaryType)
            {
                case WaterFlowModel1DLateralDataType.FlowConstant:
                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityConstantValue(lateralDischarge.ConstantBoundaryValue,
                                                                                             BoundaryRegion.QuantityStrings.WaterDischarge,
                                                                                             BoundaryRegion.UnitStrings.WaterDischarge);
                    break;
                case WaterFlowModel1DLateralDataType.FlowTimeSeries:
                    var dateTimesF = ((MultiDimensionalArray<DateTime>)lateralDischarge.TimeDependentBoundaryValue.Arguments[0].Values).ToList();
                    var valuesF = ((MultiDimensionalArray<double>)lateralDischarge.TimeDependentBoundaryValue.Components[0].Values).ToList();

                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(dateTimesF[0],
                                                                                         dateTimesF,
                                                                                         valuesF,
                                                                                         BoundaryRegion.QuantityStrings.WaterDischarge,
                                                                                         BoundaryRegion.UnitStrings.WaterDischarge);
                    break;
                case WaterFlowModel1DLateralDataType.FlowWaterLevelTable:
                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(lateralDischarge.TimeDependentBoundaryValue);
                    break;
            }
            return boundaryDefinition;
        }

        private static IDelftBcCategory GetSaltComponentCategory(string name, LateralDischargeSalt LateralDischarge)
        {
            // Set common elements
            var boundaryDefinition = BoundaryTestHelper.GetCommonCategory(BoundaryRegion.BcLateralHeader,
                                                                          name,
                                                                          BoundaryFileWriterHelper.GetFunctionString(LateralDischarge.BoundaryType),
                                                                          LateralDischarge.InterpolationType,
                                                                          BoundaryTestHelper.GetTimeSeriesIsPeriodicProperty(LateralDischarge.TimeDependentBoundaryValue));

            switch (LateralDischarge.BoundaryType)
            {
                case SaltLateralDischargeType.ConcentrationConstant:
                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityConstantValue(
                                                                      LateralDischarge.ConstantBoundaryValue,
                                                                      BoundaryRegion.QuantityStrings.WaterSalinity,
                                                                      BoundaryRegion.UnitStrings.SaltPpt);
                    break;
                case SaltLateralDischargeType.ConcentrationTimeSeries:
                {
                    var dateTimes = ((MultiDimensionalArray<DateTime>) LateralDischarge.TimeDependentBoundaryValue.Arguments[0].Values).ToList();
                    var values = ((MultiDimensionalArray<double>) LateralDischarge.TimeDependentBoundaryValue.Components[0].Values).ToList();

                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(dateTimes[0],
                                                                                         dateTimes,
                                                                                         values,
                                                                                         BoundaryRegion.QuantityStrings.WaterSalinity,
                                                                                         BoundaryRegion.UnitStrings.SaltPpt);
                } break;
                case SaltLateralDischargeType.MassConstant:
                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityConstantValue(LateralDischarge.ConstantBoundaryValue,
                                                                                             BoundaryRegion.QuantityStrings.WaterSalinity,
                                                                                             BoundaryRegion.UnitStrings.SaltMass);
                    break;
                case SaltLateralDischargeType.MassTimeSeries:
                {
                    var dateTimes = ((MultiDimensionalArray<DateTime>) LateralDischarge.TimeDependentBoundaryValue.Arguments[0].Values).ToList();
                    var values = ((MultiDimensionalArray<double>) LateralDischarge.TimeDependentBoundaryValue.Components[0].Values).ToList();

                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(dateTimes[0],
                                                                                         dateTimes,
                                                                                         values,
                                                                                         BoundaryRegion.QuantityStrings.WaterSalinity,
                                                                                         BoundaryRegion.UnitStrings.SaltMass);
                } break;
                case SaltLateralDischargeType.Default:
                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityConstantValue(WaterFlowModel1DLateralSourceData.DefaultSalinity, BoundaryRegion.QuantityStrings.WaterSalinity, BoundaryRegion.UnitStrings.SaltPpt);
                    break;
            }

            return boundaryDefinition;
        }

        private static IDelftBcCategory GetTemperatureComponentCategory(string name,
                                                                        LateralDischargeTemperature lateralDischarge)
        {
            var boundaryDefinition = BoundaryTestHelper.GetCommonCategory(BoundaryRegion.BcLateralHeader,
                                                                          name,
                                                                          BoundaryFileWriterHelper.GetFunctionString(lateralDischarge.BoundaryType),
                                                                          lateralDischarge.InterpolationType,
                                                                          BoundaryTestHelper.GetTimeSeriesIsPeriodicProperty(lateralDischarge.TimeDependentBoundaryValue));
            switch (lateralDischarge.BoundaryType)
            {
                case TemperatureLateralDischargeType.Constant:
                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityConstantValue(lateralDischarge.ConstantBoundaryValue,
                                                                                             BoundaryRegion.QuantityStrings.WaterTemperature,
                                                                                             BoundaryRegion.UnitStrings.WaterTemperature);
                    break;
                case TemperatureLateralDischargeType.TimeDependent:
                    var dateTimes = ((MultiDimensionalArray<DateTime>)lateralDischarge.TimeDependentBoundaryValue.Arguments[0].Values).ToList();
                    var values = ((MultiDimensionalArray<double>)lateralDischarge.TimeDependentBoundaryValue.Components[0].Values).ToList();

                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(dateTimes[0],
                                                                                         dateTimes,
                                                                                         values,
                                                                                         BoundaryRegion.QuantityStrings.WaterTemperature,
                                                                                         BoundaryRegion.UnitStrings.WaterTemperature);
                    break;
            }
            return boundaryDefinition;
        }

        private static void AssertThatLateralDischargeIsEqualTo(LateralDischarge actual, LateralDischarge expected)
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(expected, Is.Not.Null);

            Assert.That(actual.Name, Is.EqualTo(expected.Name));

            // Water component
            if (expected.WaterComponent != null)
            {
                Assert.That(actual.WaterComponent, 
                    Is.Not.Null);
                Assert.That(actual.WaterComponent.BoundaryType, 
                    Is.EqualTo(expected.WaterComponent.BoundaryType));
                Assert.That(actual.WaterComponent.InterpolationType, 
                    Is.EqualTo(expected.WaterComponent.InterpolationType));
                Assert.That(actual.WaterComponent.IsPeriodic,
                    Is.EqualTo(expected.WaterComponent.IsPeriodic));

                if (expected.WaterComponent.BoundaryType == WaterFlowModel1DLateralDataType.FlowConstant)
                    Assert.That(actual.WaterComponent.ConstantBoundaryValue, 
                        Is.EqualTo(expected.WaterComponent.ConstantBoundaryValue));
                else
                    BoundaryTestHelper.AssertThatTimeDependentFunctionIsEqualTo(actual.WaterComponent.TimeDependentBoundaryValue,
                                                                                expected.WaterComponent.TimeDependentBoundaryValue);
            }
            else
            {
                Assert.That(actual.WaterComponent, Is.Null);
            }

            // Salt component
            if (expected.SaltComponent != null)
            {
                Assert.That(actual.SaltComponent,
                    Is.Not.Null);
                Assert.That(actual.SaltComponent.BoundaryType, 
                    Is.EqualTo(expected.SaltComponent.BoundaryType));
                Assert.That(actual.SaltComponent.InterpolationType, 
                    Is.EqualTo(expected.SaltComponent.InterpolationType));
                Assert.That(actual.SaltComponent.IsPeriodic, 
                    Is.EqualTo(expected.SaltComponent.IsPeriodic));
                if (expected.SaltComponent.BoundaryType == SaltLateralDischargeType.Default)
                {
                    Assert.That(actual.SaltComponent.ConstantBoundaryValue, 
                        Is.EqualTo(WaterFlowModel1DLateralSourceData.DefaultSalinity));
                } else if (expected.SaltComponent.BoundaryType == SaltLateralDischargeType.ConcentrationConstant ||
                    expected.SaltComponent.BoundaryType == SaltLateralDischargeType.MassConstant)
                {
                    Assert.That(actual.SaltComponent.ConstantBoundaryValue, 
                        Is.EqualTo(expected.SaltComponent.ConstantBoundaryValue));
                }
                else
                {
                    BoundaryTestHelper.AssertThatTimeDependentFunctionIsEqualTo(actual.SaltComponent.TimeDependentBoundaryValue,
                                                                                expected.SaltComponent.TimeDependentBoundaryValue);
                }
            }
            else
            {
                Assert.That(actual.SaltComponent, Is.Null);
            }

            // Temperature component
            if (expected.TemperatureComponent != null)
            {
                Assert.That(actual.TemperatureComponent,
                    Is.Not.Null);
                Assert.That(actual.TemperatureComponent.BoundaryType, 
                    Is.EqualTo(expected.TemperatureComponent.BoundaryType));
                Assert.That(actual.TemperatureComponent.InterpolationType,
                    Is.EqualTo(expected.TemperatureComponent.InterpolationType));
                Assert.That(actual.TemperatureComponent.IsPeriodic,
                    Is.EqualTo(expected.TemperatureComponent.IsPeriodic));
                if (expected.TemperatureComponent.BoundaryType == TemperatureLateralDischargeType.Constant)
                {
                    Assert.That(actual.TemperatureComponent.ConstantBoundaryValue, 
                        Is.EqualTo(expected.TemperatureComponent.ConstantBoundaryValue));
                }
                else
                {
                    BoundaryTestHelper.AssertThatTimeDependentFunctionIsEqualTo(actual.TemperatureComponent.TimeDependentBoundaryValue,
                                                                                expected.TemperatureComponent.TimeDependentBoundaryValue);
                }
            }
            else
            {
                Assert.That(actual.TemperatureComponent, Is.Null);
            }

        }
        #endregion
    }
}
