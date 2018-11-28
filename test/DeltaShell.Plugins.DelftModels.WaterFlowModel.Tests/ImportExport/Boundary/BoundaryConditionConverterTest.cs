using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary.TestHelpers;
using NUnit.Framework;

using HasComponent = DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary.TestHelpers.HasComponent;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    [TestFixture]
    public class BoundaryConditionConverterTest
    {
        #region SetUp
        private BoundaryConditionWater constantWaterLevelComponent;
        private BoundaryConditionWater constantWaterDischargeComponent;

        private BoundaryConditionWater levelDischargeTableComponent;

        private BoundaryConditionWater timeDependentWaterLevelComponent;
        private BoundaryConditionWater timeDependentWaterDischargeComponent;

        private BoundaryConditionSalt constantSaltComponent;
        private BoundaryConditionSalt timeDependentSaltComponent;

        private BoundaryConditionTemperature constantTemperatureComponent;
        private BoundaryConditionTemperature timeDependentTemperatureComponent;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // Water
            constantWaterLevelComponent =
                BoundaryObjectConstructionTestHelper.GetConstantWaterLevelBcComponent();
            constantWaterDischargeComponent =
                BoundaryObjectConstructionTestHelper.GetConstantWaterDischargeComponent();

            levelDischargeTableComponent = 
                BoundaryObjectConstructionTestHelper.GetLevelDischargeTableBcComponent();
            timeDependentWaterLevelComponent =
                BoundaryObjectConstructionTestHelper.GetTimeDependentWaterLevelBcComponent();
            timeDependentWaterDischargeComponent =
                BoundaryObjectConstructionTestHelper.GetTimeDependentWaterDischargeBcComponent();

            // Salt
            constantSaltComponent =
                BoundaryObjectConstructionTestHelper.GetConstantSaltBcComponent();
            timeDependentSaltComponent = 
                BoundaryObjectConstructionTestHelper.GetTimeDependentSaltBcComponent();

            // Temperature
            constantTemperatureComponent =
                BoundaryObjectConstructionTestHelper.GetConstantTemperatureBcComponent();
            timeDependentTemperatureComponent =
                BoundaryObjectConstructionTestHelper.GetTimeDependentTemperatureBcComponent();
        }

        #endregion

        /// <summary>
        /// GIVEN a null set of DelftBcCategories
        ///   AND an empty list of error messages
        /// WHEN BoundaryConditionConverter convert is called with these parameters
        /// THEN an empty set will be returned
        ///  AND a single error is logged
        /// </summary>
        [Test]
        public void GivenANullSetOfDelftBcCategoriesAndAnEmptyListOfErrorMessages_WhenBoundaryConditionConverterConvertIsCalledWithTheseParameters_ThenAnEmptySetWillBeReturnedAndASingleErrorIsLogged()
        {
            // Given
            const IList<IDelftBcCategory> nullSet = null;
            var errorMessages = new List<string>();

            // When
            var output = BoundaryConditionConverter.Convert(nullSet, errorMessages);

            // Then
            Assert.That(output, Is.Not.Null);
            Assert.That(output, Is.Empty);

            Assert.That(errorMessages.Count, Is.EqualTo(1));

            const string expectedErrorMessage = "Unable to parse null set of BoundaryConditions.";
            Assert.That(errorMessages[0], Is.EqualTo(expectedErrorMessage));
        }

        /// <summary>
        /// GIVEN an empty set of DelftBcCategories
        ///   AND an empty list of error messages
        /// WHEN BoundaryConditionConverter convert is called with these parameters
        /// THEN an empty set will be returned
        ///  AND a single error is logged
        /// </summary>
        [Test]
        public void GivenAnEmptySetOfDelftBcCategoriesAndAnEmptyListOfErrorMessages_WhenBoundaryConditionConverterConvertIsCalledWithTheseParameters_ThenAnEmptySetWillBeReturnedAndASingleErrorIsLogged()
        {
            var emptySet = new List<IDelftBcCategory>();
            var errorMessages = new List<string>();

            // When
            var output = BoundaryConditionConverter.Convert(emptySet, errorMessages);

            // Then
            Assert.That(output, Is.Not.Null);
            Assert.That(output, Is.Empty);

            Assert.That(errorMessages.Count, Is.EqualTo(1));

            const string expectedErrorMessage = "Unable to parse empty set of BoundaryConditions.";
            Assert.That(errorMessages[0], Is.EqualTo(expectedErrorMessage));
        }

        /// <summary>
        /// GIVEN a set of DelftBcCategories containing two BoundaryConditions with all components on the same node
        ///   AND an empty list of error messages
        /// WHEN BoundaryConditionConverter convert is called with these parameters
        /// THEN A set containing a single BoundaryCondition corresponding with the first input is returned
        ///  AND a three errors are logged
        /// </summary>
        [Test]
        public void GivenASetOfDelftBcCategoriesContainingTwoBoundaryConditionsWithAllComponentsOnTheSameNodeAndAnEmptyListOfErrorMessages_WhenBoundaryConditionConverterConvertIsCalledWithTheseParameters_ThenASetContainingASingleBoundaryConditionCorrespondingWithTheFirstInputIsReturnedAndAThreeErrorsAreLogged()
        {
            // Given
            var inputSet = new List<IDelftBcCategory>();
            var errorMessages = new List<string>();

            const string nodeName = "Tenderloin";
            var boundaryCondition1 = GetBoundaryCondition(nodeName, HasComponent.Constant, WaterType.Discharge, HasComponent.Constant, HasComponent.Constant);
            inputSet.AddRange(ToBcCategories(boundaryCondition1));

            var boundaryCondition2 = GetBoundaryCondition(nodeName, HasComponent.Constant, WaterType.Level, HasComponent.Constant, HasComponent.Constant);
            inputSet.AddRange(ToBcCategories(boundaryCondition2));

            for (var i = 0; i < inputSet.Count; i++)
                inputSet[i].LineNumber = 10 * i;

            // When
            var output = BoundaryConditionConverter.Convert(inputSet, errorMessages);

            // Then
            // Proper output
            Assert.That(output, Is.Not.Null);
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output.ContainsKey(nodeName));

            var outputBoundaryCondition = output[nodeName];
            AssertThatBoundaryConditionIsEqualTo(outputBoundaryCondition, boundaryCondition1);

            // error messages
            Assert.That(errorMessages.Count, Is.EqualTo(3));

            for (var i = 0; i < 3; i++)
            {
                var expectedErrorMessage = $"Could not parse boundary condition category: {inputSet[i + 3].Name} at line {inputSet[i + 3].LineNumber}: Component has already been defined";
                Assert.That(errorMessages[i], Is.EqualTo(expectedErrorMessage));
            }
        }

        // Test all possible combinations of BoundaryConditions.
        /// <summary>
        /// GIVEN a set of IDelftBcCategories describing a single BoundaryCondition
        ///   AND an empty list of error messages
        /// WHEN BoundaryConditionConverter convert is called with these parameters
        /// THEN A set containing a single BoundaryCondition corresponding with the input is returned
        ///  AND no errors are logged
        /// </summary>
        [TestCase(HasComponent.Constant,      WaterType.Discharge, HasComponent.None, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, WaterType.Discharge, HasComponent.None, HasComponent.None)]
        [TestCase(HasComponent.None,          WaterType.None,      HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.Constant,      WaterType.Discharge, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, WaterType.Discharge, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.None,          WaterType.None,      HasComponent.TimeDependent, HasComponent.None)]
        [TestCase(HasComponent.Constant,      WaterType.Discharge, HasComponent.TimeDependent, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, WaterType.Discharge, HasComponent.TimeDependent, HasComponent.None)]

        [TestCase(HasComponent.None,          WaterType.None,      HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.Constant,      WaterType.Discharge, HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, WaterType.Discharge, HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.None,          WaterType.None,      HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.Constant,      WaterType.Discharge, HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, WaterType.Discharge, HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.None,          WaterType.None,      HasComponent.TimeDependent, HasComponent.Constant)]
        [TestCase(HasComponent.Constant,      WaterType.Discharge, HasComponent.TimeDependent, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, WaterType.Discharge, HasComponent.TimeDependent, HasComponent.Constant)]

        [TestCase(HasComponent.None,          WaterType.None,      HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant,      WaterType.Discharge, HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, WaterType.Discharge, HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.None,          WaterType.None,      HasComponent.Constant, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant,      WaterType.Discharge, HasComponent.Constant, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, WaterType.Discharge, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.None,          WaterType.None,      HasComponent.TimeDependent, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant,      WaterType.Discharge, HasComponent.TimeDependent, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, WaterType.Discharge, HasComponent.TimeDependent, HasComponent.TimeDependent)]

        [TestCase(HasComponent.Constant,      WaterType.Level, HasComponent.None, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, WaterType.Level, HasComponent.None, HasComponent.None)]
        [TestCase(HasComponent.None,          WaterType.None,  HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.Constant,      WaterType.Level, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, WaterType.Level, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.None,          WaterType.None,  HasComponent.TimeDependent, HasComponent.None)]
        [TestCase(HasComponent.Constant,      WaterType.Level, HasComponent.TimeDependent, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, WaterType.Level, HasComponent.TimeDependent, HasComponent.None)]

        [TestCase(HasComponent.None,          WaterType.None,  HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.Constant,      WaterType.Level, HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, WaterType.Level, HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.None,          WaterType.None,  HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.Constant,      WaterType.Level, HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, WaterType.Level, HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.None,          WaterType.None,  HasComponent.TimeDependent, HasComponent.Constant)]
        [TestCase(HasComponent.Constant,      WaterType.Level, HasComponent.TimeDependent, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, WaterType.Level, HasComponent.TimeDependent, HasComponent.Constant)]

        [TestCase(HasComponent.None,          WaterType.None,  HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant,      WaterType.Level, HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, WaterType.Level, HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.None,          WaterType.None,  HasComponent.Constant, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant,      WaterType.Level, HasComponent.Constant, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, WaterType.Level, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.None,          WaterType.None,  HasComponent.TimeDependent, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant,      WaterType.Level, HasComponent.TimeDependent, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, WaterType.Level, HasComponent.TimeDependent, HasComponent.TimeDependent)]

        [TestCase(HasComponent.Table, WaterType.None, HasComponent.None,          HasComponent.None)]
        [TestCase(HasComponent.Table, WaterType.None, HasComponent.Constant,      HasComponent.None)]
        [TestCase(HasComponent.Table, WaterType.None, HasComponent.TimeDependent, HasComponent.None)]

        [TestCase(HasComponent.Table, WaterType.None, HasComponent.None,          HasComponent.Constant)]
        [TestCase(HasComponent.Table, WaterType.None, HasComponent.Constant,      HasComponent.Constant)]
        [TestCase(HasComponent.Table, WaterType.None, HasComponent.TimeDependent, HasComponent.Constant)]

        [TestCase(HasComponent.Table, WaterType.None, HasComponent.None,          HasComponent.TimeDependent)]
        [TestCase(HasComponent.Table, WaterType.None, HasComponent.Constant,      HasComponent.TimeDependent)]
        [TestCase(HasComponent.Table, WaterType.None, HasComponent.TimeDependent, HasComponent.TimeDependent)]
        public void GivenASetOfIDelftBcCategoriesDescribingASingleBoundaryConditionAndAnEmptyListOfErrorMessages_WhenBoundaryConditionConverterConvertIsCalledWithTheseParameters_ThenASetContainingASingleBoundaryConditionCorrespondingWithTheInputIsReturnedAndNoErrorsAreLogged(HasComponent hasWater, WaterType waterType, HasComponent hasSalt, HasComponent hasTemperature)
        {
            const string nodeName = "Bacon";
            // Given
            var boundaryCondition = GetBoundaryCondition(nodeName, hasWater, waterType, hasSalt, hasTemperature);
            var inputSet = ToBcCategories(boundaryCondition);
            var errorMessages = new List<string>();

            // When
            var output = BoundaryConditionConverter.Convert(inputSet, errorMessages);

            // Then
            Assert.That(output, Is.Not.Null);
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output.ContainsKey(nodeName));

            var outputBoundaryCondition = output[nodeName];
            AssertThatBoundaryConditionIsEqualTo(outputBoundaryCondition, boundaryCondition);

            Assert.That(errorMessages, Is.Empty);
        }

        /// <summary>
        /// GIVEN a set of IDelftBcCategories describing a multiple BoundaryConditions
        ///   AND an empty list of error messages
        /// WHEN BoundaryConditionConverter convert is called with these parameters
        /// THEN A set containing the corresponding BoundaryConditions is returned
        ///  AND no errors are logged
        /// </summary>
        [Test]
        public void GivenASetOfIDelftBcCategoriesDescribingAMultipleBoundaryConditionsAndAnEmptyListOfErrorMessages_WhenBoundaryConditionConverterConvertIsCalledWithTheseParameters_ThenASetContainingTheCorrespondingBoundaryConditionsIsReturnedAndNoErrorsAreLogged()
        {
            var inputNodes = new List<BoundaryCondition>()
            {
                GetBoundaryCondition("Prosciutto", HasComponent.Constant,      WaterType.Discharge, HasComponent.None,          HasComponent.None),
                GetBoundaryCondition("Drumstick",  HasComponent.TimeDependent, WaterType.Level,     HasComponent.TimeDependent, HasComponent.TimeDependent),
                GetBoundaryCondition("Venison",    HasComponent.TimeDependent, WaterType.Discharge, HasComponent.None,          HasComponent.None),
                GetBoundaryCondition("T-bone",     HasComponent.Table,         WaterType.None,      HasComponent.Constant,      HasComponent.None),
                GetBoundaryCondition("Ribs",       HasComponent.Constant,      WaterType.Level,     HasComponent.None,          HasComponent.Constant),
            };

            var errorMessages = new List<string>();

            var inputNodesCategory = new List<IDelftBcCategory>();
            foreach (var n in inputNodes) inputNodesCategory.AddRange(ToBcCategories(n));
            BoundaryTestHelper.Shuffle(inputNodesCategory);

            // When
            var output = BoundaryConditionConverter.Convert(inputNodesCategory, errorMessages);

            // Then
            Assert.That(output, Is.Not.Null);
            Assert.That(output.Count, Is.EqualTo(inputNodes.Count));

            foreach (var node in inputNodes)
            {
                Assert.That(output.ContainsKey(node.Name));
                var boundaryCondition = output[node.Name];
                AssertThatBoundaryConditionIsEqualTo(boundaryCondition, node);
            }

            Assert.That(errorMessages, Is.Empty);
        }

        /// <summary>
        /// GIVEN a set of IDelftBcCategories describing a multiple BoundaryConditions and other data
        ///   AND an empty list of error messages
        /// WHEN BoundaryConditionConverter convert is called with these parameters
        /// THEN A set containing the corresponding BoundaryConditions is returned
        ///  AND no errors are logged
        /// </summary>
        [Test]
        public void GivenASetOfIDelftBcCategoriesDescribingAMultipleBoundaryConditionsAndOtherDataAndAnEmptyListOfErrorMessages_WhenBoundaryConditionConverterConvertIsCalledWithTheseParameters_ThenASetContainingTheCorrespondingBoundaryConditionsIsReturnedAndNoErrorsAreLogged()
        {
            var inputNodes = new List<BoundaryCondition>()
            {
                GetBoundaryCondition("Prosciutto", HasComponent.Constant,      WaterType.Discharge, HasComponent.None,          HasComponent.None),
                GetBoundaryCondition("Drumstick",  HasComponent.TimeDependent, WaterType.Level,     HasComponent.TimeDependent, HasComponent.TimeDependent),
                GetBoundaryCondition("Venison",    HasComponent.TimeDependent, WaterType.Discharge, HasComponent.None,          HasComponent.None),
                GetBoundaryCondition("T-bone",     HasComponent.Table,         WaterType.None,      HasComponent.Constant,      HasComponent.None),
                GetBoundaryCondition("Ribs",       HasComponent.Constant,      WaterType.Level,     HasComponent.None,          HasComponent.Constant),
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
            var output = BoundaryConditionConverter.Convert(inputNodesCategory, errorMessages);

            // Then
            Assert.That(output, Is.Not.Null);
            Assert.That(output.Count, Is.EqualTo(inputNodes.Count));

            foreach (var node in inputNodes)
            {
                Assert.That(output.ContainsKey(node.Name));
                var boundaryCondition = output[node.Name];
                AssertThatBoundaryConditionIsEqualTo(boundaryCondition, node);
            }

            Assert.That(errorMessages, Is.Empty);
        }

        /// <summary>
        /// GIVEN a set of IDelftBcCategories describing a multiple BoundaryConditions and ModelWide data
        ///   AND an empty list of error messages
        /// WHEN BoundaryConditionConverter convert is called with these parameters
        /// THEN A set containing the corresponding BoundaryConditions is returned
        ///  AND no errors are logged
        /// </summary>
        [Test]
        public void GivenASetOfIDelftBcCategoriesDescribingAMultipleBoundaryConditionsAndModelWideDataAndAnEmptyListOfErrorMessages_WhenBoundaryConditionConverterConvertIsCalledWithTheseParameters_ThenASetContainingTheCorrespondingBoundaryConditionsIsReturnedAndNoErrorsAreLogged()
        {
            var inputNodes = new List<BoundaryCondition>()
            {
                GetBoundaryCondition("Prosciutto", HasComponent.Constant,      WaterType.Discharge, HasComponent.None,          HasComponent.None),
                GetBoundaryCondition("Drumstick",  HasComponent.TimeDependent, WaterType.Level,     HasComponent.TimeDependent, HasComponent.TimeDependent),
                GetBoundaryCondition("Venison",    HasComponent.TimeDependent, WaterType.Discharge, HasComponent.None,          HasComponent.None),
                GetBoundaryCondition("T-bone",     HasComponent.Table,         WaterType.None,      HasComponent.Constant,      HasComponent.None),
                GetBoundaryCondition("Ribs",       HasComponent.Constant,      WaterType.Level,     HasComponent.None,          HasComponent.Constant),
            };

            var errorMessages = new List<string>();

            var inputNodesCategory = new List<IDelftBcCategory>();
            foreach (var n in inputNodes) inputNodesCategory.AddRange(ToBcCategories(n));
            inputNodesCategory.Add(new DelftBcCategory("Sausage"));
            inputNodesCategory.Add(new DelftBcCategory("Meatloaf"));
            inputNodesCategory.Add(new DelftBcCategory("Pancetta"));
            inputNodesCategory.Add(new DelftBcCategory("Pork"));
            inputNodesCategory.Add(new DelftBcCategory("Pastrami"));

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
            inputNodesCategory.Add(airTempDefinition);


            var relHumidBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var relHumidDefinition = relHumidBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                                BoundaryRegion.FunctionStrings.TimeSeries,
                                                                interpolationType);
            relHumidDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                 new List<DateTime>() { timeValue },
                                                                                 new List<double>() { relativeHumidity },
                                                                                 BoundaryRegion.QuantityStrings.MeteoDataHumidity,
                                                                                 BoundaryRegion.UnitStrings.MeteoDataHumidity);
            inputNodesCategory.Add(relHumidDefinition);


            var cloudinessBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var cloudinessDefinition = cloudinessBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                                                                    BoundaryRegion.FunctionStrings.TimeSeries,
                                                                    interpolationType);
            cloudinessDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(startTime,
                                                                                   new List<DateTime>() { timeValue },
                                                                                   new List<double>() { cloudiness },
                                                                                   BoundaryRegion.QuantityStrings.MeteoDataCloudiness,
                                                                                   BoundaryRegion.UnitStrings.MeteoDataCloudiness);
            inputNodesCategory.Add(cloudinessDefinition);


            BoundaryTestHelper.Shuffle(inputNodesCategory);

            // When
            var output = BoundaryConditionConverter.Convert(inputNodesCategory, errorMessages);

            // Then
            Assert.That(output, Is.Not.Null);
            Assert.That(output.Count, Is.EqualTo(inputNodes.Count));

            foreach (var node in inputNodes)
            {
                Assert.That(output.ContainsKey(node.Name));
                var boundaryCondition = output[node.Name];
                AssertThatBoundaryConditionIsEqualTo(boundaryCondition, node);
            }

            Assert.That(errorMessages, Is.Empty);
        }

        #region Helper functions

        // Enums to define
        public enum WaterType
        {
            None,
            Discharge,
            Level
        }

        private BoundaryCondition GetBoundaryCondition(string name, HasComponent water, WaterType waterType, HasComponent salt, HasComponent temperature)
        {
            var boundaryCondition = new BoundaryCondition(name);

            switch (water)
            {
                case HasComponent.Constant:
                    boundaryCondition.WaterComponent = waterType == WaterType.Discharge ? constantWaterDischargeComponent : constantWaterLevelComponent;
                    break;
                case HasComponent.Table:
                    boundaryCondition.WaterComponent = levelDischargeTableComponent;
                    break;
                case HasComponent.TimeDependent:
                    boundaryCondition.WaterComponent = waterType == WaterType.Discharge ? timeDependentWaterDischargeComponent : timeDependentWaterLevelComponent;
                    break;
            }

            switch (salt)
            {
                case HasComponent.Constant:
                    boundaryCondition.SaltComponent = constantSaltComponent;
                    break;
                case HasComponent.TimeDependent:
                    boundaryCondition.SaltComponent = timeDependentSaltComponent;
                    break;
            }

            switch (temperature)
            {
                case HasComponent.Constant:
                    boundaryCondition.TemperatureComponent = constantTemperatureComponent;
                    break;
                case HasComponent.TimeDependent:
                    boundaryCondition.TemperatureComponent = timeDependentTemperatureComponent;
                    break;
            }

            return boundaryCondition;
        }

        private static void AssertThatBoundaryConditionIsEqualTo(BoundaryCondition actual, BoundaryCondition expected)
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(expected, Is.Not.Null);

            Assert.That(actual.Name, Is.EqualTo(expected.Name));

            // Water component
            if (expected.WaterComponent != null)
            {
                Assert.That(actual.WaterComponent, Is.Not.Null);
                Assert.That(actual.WaterComponent.BoundaryType, Is.EqualTo(expected.WaterComponent.BoundaryType));
                Assert.That(actual.WaterComponent.InterpolationType, Is.EqualTo(expected.WaterComponent.InterpolationType));
                Assert.That(actual.WaterComponent.IsPeriodic, Is.EqualTo(expected.WaterComponent.IsPeriodic));

                if (expected.WaterComponent.BoundaryType == WaterFlowModel1DBoundaryNodeDataType.FlowConstant ||
                    expected.WaterComponent.BoundaryType == WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant)
                {
                    Assert.That(actual.WaterComponent.ConstantBoundaryValue, Is.EqualTo(expected.WaterComponent.ConstantBoundaryValue));
                }
                else
                {
                    BoundaryAssertionTestHelper.AssertThatTimeDependentFunctionIsEqualTo(actual.WaterComponent.TimeDependentBoundaryValue, 
                                                                                expected.WaterComponent.TimeDependentBoundaryValue);
                }
            }
            else
            {
                Assert.That(actual.WaterComponent, Is.Null);
            }

            // Salt component
            if (expected.SaltComponent != null)
            {
                Assert.That(actual.SaltComponent, Is.Not.Null);
                Assert.That(actual.SaltComponent.BoundaryType,          Is.EqualTo(expected.SaltComponent.BoundaryType));
                Assert.That(actual.SaltComponent.InterpolationType,     Is.EqualTo(expected.SaltComponent.InterpolationType));
                Assert.That(actual.SaltComponent.IsPeriodic,            Is.EqualTo(expected.SaltComponent.IsPeriodic));
                if (expected.SaltComponent.BoundaryType == SaltBoundaryConditionType.Constant)
                {
                    Assert.That(actual.SaltComponent.ConstantBoundaryValue, Is.EqualTo(expected.SaltComponent.ConstantBoundaryValue));
                }
                else
                {
                    BoundaryAssertionTestHelper.AssertThatTimeDependentFunctionIsEqualTo(actual.SaltComponent.TimeDependentBoundaryValue,
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
                Assert.That(actual.TemperatureComponent, Is.Not.Null);
                Assert.That(actual.TemperatureComponent.BoundaryType, Is.EqualTo(expected.TemperatureComponent.BoundaryType));
                Assert.That(actual.TemperatureComponent.InterpolationType, Is.EqualTo(expected.TemperatureComponent.InterpolationType));
                Assert.That(actual.TemperatureComponent.IsPeriodic, Is.EqualTo(expected.TemperatureComponent.IsPeriodic));
                if (expected.TemperatureComponent.BoundaryType == TemperatureBoundaryConditionType.Constant)
                {
                    Assert.That(actual.TemperatureComponent.ConstantBoundaryValue, Is.EqualTo(expected.TemperatureComponent.ConstantBoundaryValue));
                }
                else
                {
                    BoundaryAssertionTestHelper.AssertThatTimeDependentFunctionIsEqualTo(actual.TemperatureComponent.TimeDependentBoundaryValue,
                                                                                expected.TemperatureComponent.TimeDependentBoundaryValue);
                }
            }
            else
            {
                Assert.That(actual.TemperatureComponent, Is.Null);
            }
        }


        public static IList<IDelftBcCategory> ToBcCategories(BoundaryCondition boundaryCondition)
        {
            var result = new List<IDelftBcCategory>();


            if (boundaryCondition.WaterComponent != null)
                result.Add(GetWaterComponentCategory(boundaryCondition.Name, boundaryCondition.WaterComponent));

            if (boundaryCondition.SaltComponent != null)
                result.Add(GetSaltComponentCategory(boundaryCondition.Name, boundaryCondition.SaltComponent));

            if (boundaryCondition.TemperatureComponent != null)
                result.Add(GetTemperatureComponentCategory(boundaryCondition.Name, boundaryCondition.TemperatureComponent));

            return result;
        }


        private static IDelftBcCategory GetWaterComponentCategory(string name, BoundaryConditionWater boundaryCondition)
        {
            // Set common elements
            var boundaryDefinition = BoundaryTestHelper.GetCommonCategory(
                BoundaryRegion.BcBoundaryHeader, 
                name,
                BoundaryFileWriterHelper.GetFunctionString(boundaryCondition.BoundaryType),
                boundaryCondition.InterpolationType, 
                BoundaryTestHelper.GetTimeSeriesIsPeriodicProperty(boundaryCondition.TimeDependentBoundaryValue));

            // Create actual table
            switch (boundaryCondition.BoundaryType)
            {
                case WaterFlowModel1DBoundaryNodeDataType.FlowConstant:
                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityConstantValue(
                        boundaryCondition.ConstantBoundaryValue,
                        BoundaryRegion.QuantityStrings.WaterDischarge,
                        BoundaryRegion.UnitStrings.WaterDischarge);
                    break;
                case WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries:
                    var dateTimesF = ((MultiDimensionalArray<DateTime>)boundaryCondition.TimeDependentBoundaryValue.Arguments[0].Values).ToList();
                    var valuesF = ((MultiDimensionalArray<double>)boundaryCondition.TimeDependentBoundaryValue.Components[0].Values).ToList();

                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(dateTimesF[0],
                                                                                         dateTimesF,
                                                                                         valuesF,
                                                                                         BoundaryRegion.QuantityStrings.WaterDischarge,
                                                                                         BoundaryRegion.UnitStrings.WaterDischarge);
                    break;
                case WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable:
                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(boundaryCondition.TimeDependentBoundaryValue);
                    break;
                case WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant:
                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityConstantValue(
                        boundaryCondition.ConstantBoundaryValue,
                        BoundaryRegion.QuantityStrings.WaterLevel,
                        BoundaryRegion.UnitStrings.WaterLevel);
                    break;
                case WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    var dateTimesWL = ((MultiDimensionalArray<DateTime>)boundaryCondition.TimeDependentBoundaryValue.Arguments[0].Values).ToList();
                    var valuesWL = ((MultiDimensionalArray<double>)boundaryCondition.TimeDependentBoundaryValue.Components[0].Values).ToList();

                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(dateTimesWL[0],
                        dateTimesWL,
                        valuesWL,
                        BoundaryRegion.QuantityStrings.WaterLevel,
                        BoundaryRegion.UnitStrings.WaterLevel);
                    break;
            }
            return boundaryDefinition;
        }

        private static IDelftBcCategory GetSaltComponentCategory(string name, BoundaryConditionSalt boundaryCondition)
        {
            // Set common elements
            var boundaryDefinition = BoundaryTestHelper.GetCommonCategory(
                BoundaryRegion.BcBoundaryHeader, 
                name,
                BoundaryFileWriterHelper.GetFunctionString(boundaryCondition.BoundaryType),
                boundaryCondition.InterpolationType,
                BoundaryTestHelper.GetTimeSeriesIsPeriodicProperty(boundaryCondition.TimeDependentBoundaryValue));

            switch (boundaryCondition.BoundaryType)
            {
                case SaltBoundaryConditionType.Constant:
                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityConstantValue(
                        boundaryCondition.ConstantBoundaryValue,
                        BoundaryRegion.QuantityStrings.WaterSalinity,
                        BoundaryRegion.UnitStrings.SaltPpt);
                    break;
                case SaltBoundaryConditionType.TimeDependent:
                    var dateTimes = ((MultiDimensionalArray<DateTime>)boundaryCondition.TimeDependentBoundaryValue.Arguments[0].Values).ToList();
                    var values = ((MultiDimensionalArray<double>)boundaryCondition.TimeDependentBoundaryValue.Components[0].Values).ToList();

                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(dateTimes[0],
                        dateTimes,
                        values,
                        BoundaryRegion.QuantityStrings.WaterSalinity,
                        BoundaryRegion.UnitStrings.SaltPpt);
                    break;
            }

            return boundaryDefinition;
        }

        private static IDelftBcCategory GetTemperatureComponentCategory(string name,
            BoundaryConditionTemperature boundaryCondition)
        {
            var boundaryDefinition = BoundaryTestHelper.GetCommonCategory(
                BoundaryRegion.BcBoundaryHeader,
                name,
                BoundaryFileWriterHelper.GetFunctionString(boundaryCondition.BoundaryType),
                boundaryCondition.InterpolationType, 
                BoundaryTestHelper.GetTimeSeriesIsPeriodicProperty(boundaryCondition.TimeDependentBoundaryValue));
            switch (boundaryCondition.BoundaryType)
            {
                case TemperatureBoundaryConditionType.Constant:
                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityConstantValue(
                        boundaryCondition.ConstantBoundaryValue,
                        BoundaryRegion.QuantityStrings.WaterTemperature,
                        BoundaryRegion.UnitStrings.WaterTemperature);
                    break;
                case TemperatureBoundaryConditionType.TimeDependent:
                    var dateTimes = ((MultiDimensionalArray<DateTime>)boundaryCondition.TimeDependentBoundaryValue.Arguments[0].Values).ToList();
                    var values = ((MultiDimensionalArray<double>)boundaryCondition.TimeDependentBoundaryValue.Components[0].Values).ToList();

                    boundaryDefinition.Table = BoundaryTestHelper.GetBcQuantityDataTable(dateTimes[0],
                                                                                         dateTimes,
                                                                                         values,
                                                                                         BoundaryRegion.QuantityStrings.WaterTemperature,
                                                                                         BoundaryRegion.UnitStrings.WaterTemperature);
                    break;
            }
            return boundaryDefinition;
        }
    }
    #endregion
}
