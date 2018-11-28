using System;
using System.Collections.Generic;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary.TestHelpers;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{

    [TestFixture]
    public class BoundaryConditionFileReaderTest
    {
        #region SetUp
        private const string bcNodeName = "Salami";
        private const string ldNodeName = "Steak";


        private MeteoFunction meteoFunction;
        private WindFunction windFunction;

        private BoundaryCondition boundaryCondition;

        private LateralDischarge lateralDischarge;

        // BoundaryConditions
        private BoundaryConditionWater constantWaterLevelBcComponent;
        private BoundaryConditionWater constantWaterDischargeBcComponent;

        private BoundaryConditionWater levelDischargeTableBcComponent;

        private BoundaryConditionWater timeDependentWaterLevelBcComponent;
        private BoundaryConditionWater timeDependentWaterDischargeBcComponent;

        private BoundaryConditionSalt constantSaltBcComponent;
        private BoundaryConditionSalt timeDependentSaltBcComponent;

        private BoundaryConditionTemperature constantTemperatureBcComponent;
        private BoundaryConditionTemperature timeDependentTemperatureBcComponent;

        // LateralDischarge
        private LateralDischargeWater constantWaterLdComponent;
        private LateralDischargeWater tableWaterLdComponent;
        private LateralDischargeWater timeDependentWaterLdComponent;

        private LateralDischargeSalt constantSaltMassLdComponent;
        private LateralDischargeSalt timeDependentSaltMassLdComponent;

        private LateralDischargeSalt constantSaltConcentrationLdComponent;
        private LateralDischargeSalt timeDependentSaltConcentrationLdComponent;

        private LateralDischargeTemperature constantTemperatureLdComponent;
        private LateralDischargeTemperature timeDependentTemperatureLdComponent;


        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // BoundaryConditions
            // Water
            constantWaterLevelBcComponent =
                BoundaryObjectConstructionTestHelper.GetConstantWaterLevelBcComponent();
            constantWaterDischargeBcComponent =
                BoundaryObjectConstructionTestHelper.GetConstantWaterDischargeComponent();

            levelDischargeTableBcComponent =
                BoundaryObjectConstructionTestHelper.GetLevelDischargeTableBcComponent();
            timeDependentWaterLevelBcComponent =
                BoundaryObjectConstructionTestHelper.GetTimeDependentWaterLevelBcComponent();
            timeDependentWaterDischargeBcComponent =
                BoundaryObjectConstructionTestHelper.GetTimeDependentWaterDischargeBcComponent();

            // Salt
            constantSaltBcComponent =
                BoundaryObjectConstructionTestHelper.GetConstantSaltBcComponent();
            timeDependentSaltBcComponent =
                BoundaryObjectConstructionTestHelper.GetTimeDependentSaltBcComponent();

            // Temperature
            constantTemperatureBcComponent =
                BoundaryObjectConstructionTestHelper.GetConstantTemperatureBcComponent();
            timeDependentTemperatureBcComponent =
                BoundaryObjectConstructionTestHelper.GetTimeDependentTemperatureBcComponent();

            // LateralDischarge
            // Water
            constantWaterLdComponent = BoundaryObjectConstructionTestHelper.GetConstantWaterLdComponent();
            tableWaterLdComponent = BoundaryObjectConstructionTestHelper.GetTableWaterLdComponent();
            timeDependentWaterLdComponent = BoundaryObjectConstructionTestHelper.GetTimeDependentWaterLdComponent();

            // Salt
            constantSaltMassLdComponent = BoundaryObjectConstructionTestHelper.GetConstantSaltMassLdComponent();
            constantSaltConcentrationLdComponent = BoundaryObjectConstructionTestHelper.GetConstantSaltConcentrationLdComponent();
            timeDependentSaltMassLdComponent = BoundaryObjectConstructionTestHelper.GetTimeDependentSaltMassLdComponent();
            timeDependentSaltConcentrationLdComponent = BoundaryObjectConstructionTestHelper.GetTimeDependentSaltConcentrationLdComponent();

            // Temperature
            constantTemperatureLdComponent = BoundaryObjectConstructionTestHelper.GetConstantTemperatureLdComponent();
            timeDependentTemperatureLdComponent = BoundaryObjectConstructionTestHelper.timeDependentTemperatureLdComponent();
        }


        [SetUp]
        public void SetUp()
        {
            // Meteo Function
            meteoFunction = new MeteoFunction();
            meteoFunction.Arguments[0].SetValues(new List<DateTime>() { DateTime.Today });
            meteoFunction.AirTemperature.SetValues(new List<double>() { 12.0 });
            meteoFunction.Cloudiness.SetValues(new List<double>() { 13.0 });
            meteoFunction.RelativeHumidity.SetValues(new List<double>() { 13.0 });

            // WindFunction
            windFunction = new WindFunction();
            windFunction.Arguments[0].SetValues(new List<DateTime>() { DateTime.Today });
            windFunction.Direction.SetValues(new List<double>() { 10.0 });
            windFunction.Velocity.SetValues(new List<double>() { 11.0 });

            // Boundary Condition

            boundaryCondition = new BoundaryCondition(bcNodeName)
            {
                WaterComponent = new BoundaryConditionWater(WaterFlowModel1DBoundaryNodeDataType.FlowConstant, InterpolationType.Constant, false, 20.0),
                SaltComponent = new BoundaryConditionSalt(SaltBoundaryConditionType.Constant, InterpolationType.Constant, false, 21.0),
                TemperatureComponent = new BoundaryConditionTemperature(TemperatureBoundaryConditionType.Constant, InterpolationType.Constant, false, 22.0)
            };

            // Lateral Discharge
            lateralDischarge = new LateralDischarge(ldNodeName)
            {
                WaterComponent = new LateralDischargeWater(WaterFlowModel1DLateralDataType.FlowConstant, InterpolationType.Constant, false, 30.0),
                SaltComponent = new LateralDischargeSalt(SaltLateralDischargeType.ConcentrationConstant, InterpolationType.Constant, false, 31.0),
                TemperatureComponent = new LateralDischargeTemperature(TemperatureLateralDischargeType.Constant, InterpolationType.Constant, false, 32.0)
            };
        }

        #endregion

        /// <summary>
        /// WHEN a BoundaryConditionFileReader is constructed with no arguments
        /// THEN no exception is thrown
        /// </summary>
        [Test]
        public void WhenABoundaryConditionFileReaderIsConstructedWithNoArguments_ThenNoExceptionIsThrown()
        {
            Assert.That(new BoundaryConditionFileReader(), Is.Not.Null);
        }

        /// <summary>
        /// GIVEN some ErrorReportFunction
        /// WHEN a BoundaryConditionFileReader is constructed with this ErrorReportFunction
        /// THEN no exception is thrown
        /// </summary>
        [Test]
        public void GivenSomeErrorReportFunction_WhenABoundaryConditionFileReaderIsConstructedWithThisErrorReportFunction_ThenNoExceptionIsThrown()
        {
            Action<string, IList<string>> someErrorReportFunction = (a, b) => { };

            Assert.That(new BoundaryConditionFileReader(someErrorReportFunction), Is.Not.Null);
        }

        /// <summary>
        /// GIVEN some ErrorReportFunction
        ///   AND some parseFunction
        ///   AND some meteoConverterFunction
        ///   AND some windConverterFunction
        ///   AND some boundaryConditionConverterFunction
        ///   AND some lateralDischargeConverterFunction
        /// WHEN a BoundaryConditionFileReader is constructed with these parameters
        /// THEN no exception is thrown
        /// </summary>
        [Test]
        public void GivenSomeErrorReportFunctionAndSomeParseFunctionAndSomeMeteoConverterFunctionAndSomeWindConverterFunctionAndSomeBoundaryConditionConverterFunctionAndSomeLateralDischargeConverterFunction_WhenABoundaryConditionFileReaderIsConstructedWithTheseParameters_ThenNoExceptionIsThrown()
        {
            Func<string, IList<IDelftBcCategory>> someParser = a => null;

            Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction> someMeteoConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, WindFunction> someWindConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>> someBoundaryConditionConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>> someLateralDischargeConverter = (a, b) => null;

            Action<string, IList<string>> someErrorReportFunction = (a, b) => { };
            Assert.That(new BoundaryConditionFileReader(someParser,
                                                        someMeteoConverter,
                                                        someWindConverter,
                                                        someBoundaryConditionConverter,
                                                        someLateralDischargeConverter,
                                                        someErrorReportFunction),
                Is.Not.Null);
        }

        /// <summary>
        /// GIVEN some ErrorReportFunction
        ///   AND a null parseFunction
        ///   AND some meteoConverterFunction
        ///   AND some windConverterFunction
        ///   AND some boundaryConditionConverterFunction
        ///   AND some lateralDischargeConverterFunction
        /// WHEN a BoundaryConditionFileReader is constructed with these parameters
        /// THEN an exception is thrown
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Parser cannot be null.")]
        public void GivenSomeErrorReportFunctionAndANullParseFunctionAndSomeMeteoConverterFunctionAndSomeWindConverterFunctionAndSomeBoundaryConditionConverterFunctionAndSomeLateralDischargeConverterFunction_WhenABoundaryConditionFileReaderIsConstructedWithTheseParameters_ThenAnExceptionIsThrown()
        {
            Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction> someMeteoConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, WindFunction> someWindConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>> someBoundaryConditionConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>> someLateralDischargeConverter = (a, b) => null;

            Action<string, IList<string>> someErrorReportFunction = (a, b) => { };
            var thisWillGenerateAnException = new BoundaryConditionFileReader(null,
                someMeteoConverter,
                someWindConverter,
                someBoundaryConditionConverter,
                someLateralDischargeConverter,
                someErrorReportFunction);
        }

        /// <summary>
        /// GIVEN some ErrorReportFunction
        ///   AND some parseFunction
        ///   AND a null meteoConverterFunction
        ///   AND some windConverterFunction
        ///   AND some boundaryConditionConverterFunction
        ///   AND some lateralDischargeConverterFunction
        /// WHEN a BoundaryConditionFileReader is constructed with these parameters
        /// THEN an exception is thrown
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Converter cannot be null.")]
        public void GivenSomeErrorReportFunctionAndSomeParseFunctionAndANullMeteoConverterFunctionAndSomeWindConverterFunctionAndSomeBoundaryConditionConverterFunctionAndSomeLateralDischargeConverterFunction_WhenABoundaryConditionFileReaderIsConstructedWithTheseParameters_ThenAnExceptionIsThrown()
        {
            Func<string, IList<IDelftBcCategory>> someParser = a => null;
            Func<IList<IDelftBcCategory>, IList<string>, WindFunction> someWindConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>> someBoundaryConditionConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>> someLateralDischargeConverter = (a, b) => null;

            Action<string, IList<string>> someErrorReportFunction = (a, b) => { };
            var thisWillGenerateAnException = new BoundaryConditionFileReader(someParser,
                null,
                someWindConverter,
                someBoundaryConditionConverter,
                someLateralDischargeConverter,
                someErrorReportFunction);
        }

        /// <summary>
        /// GIVEN some ErrorReportFunction
        ///   AND some parseFunction
        ///   AND some meteoConverterFunction
        ///   AND a null windConverterFunction
        ///   AND some boundaryConditionConverterFunction
        ///   AND some lateralDischargeConverterFunction
        /// WHEN a BoundaryConditionFileReader is constructed with these parameters
        /// THEN an exception is thrown
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Converter cannot be null.")]
        public void GivenSomeErrorReportFunctionAndSomeParseFunctionAndSomeMeteoConverterFunctionAndANullWindConverterFunctionAndSomeBoundaryConditionConverterFunctionAndSomeLateralDischargeConverterFunction_WhenABoundaryConditionFileReaderIsConstructedWithTheseParameters_ThenAnExceptionIsThrown()
        {
            Func<string, IList<IDelftBcCategory>> someParser = a => null;
            Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction> someMeteoConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>> someBoundaryConditionConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>> someLateralDischargeConverter = (a, b) => null;

            Action<string, IList<string>> someErrorReportFunction = (a, b) => { };
            var thisWillGenerateAnException = new BoundaryConditionFileReader(someParser,
                someMeteoConverter,
                null,
                someBoundaryConditionConverter,
                someLateralDischargeConverter,
                someErrorReportFunction);
        }

        /// <summary>
        /// GIVEN some ErrorReportFunction
        ///   AND some parseFunction
        ///   AND some meteoConverterFunction
        ///   AND some windConverterFunction
        ///   AND a null boundaryConditionConverterFunction
        ///   AND some lateralDischargeConverterFunction
        /// WHEN a BoundaryConditionFileReader is constructed with these parameters
        /// THEN an exception is thrown
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Converter cannot be null.")]
        public void GivenSomeErrorReportFunctionAndSomeParseFunctionAndSomeMeteoConverterFunctionAndSomeWindConverterFunctionAndANullBoundaryConditionConverterFunctionAndSomeLateralDischargeConverterFunction_WhenABoundaryConditionFileReaderIsConstructedWithTheseParameters_ThenAnExceptionIsThrown()
        {
            Func<string, IList<IDelftBcCategory>> someParser = a => null;
            Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction> someMeteoConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, WindFunction> someWindConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>> someLateralDischargeConverter = (a, b) => null;

            Action<string, IList<string>> someErrorReportFunction = (a, b) => { };
            var thisWillGenerateAnException = new BoundaryConditionFileReader(someParser,
                someMeteoConverter,
                someWindConverter,
                null,
                someLateralDischargeConverter,
                someErrorReportFunction);
        }

        /// <summary>
        /// GIVEN some ErrorReportFunction
        ///   AND some parseFunction
        ///   AND some meteoConverterFunction
        ///   AND some windConverterFunction
        ///   AND some boundaryConditionConverterFunction
        ///   AND a null lateralDischargeConverterFunction
        /// WHEN a BoundaryConditionFileReader is constructed with these parameters
        /// THEN an exception is thrown
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Converter cannot be null.")]
        public void GivenSomeErrorReportFunctionAndSomeParseFunctionAndSomeMeteoConverterFunctionAndSomeWindConverterFunctionAndSomeBoundaryConditionConverterFunctionAndANullLateralDischargeConverterFunction_WhenABoundaryConditionFileReaderIsConstructedWithTheseParameters_ThenAnExceptionIsThrown()
        {
            Func<string, IList<IDelftBcCategory>> someParser = a => null;
            Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction> someMeteoConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, WindFunction> someWindConverter = (a, b) => null;
            Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>> someBoundaryConditionConverter = (a, b) => null;

            Action<string, IList<string>> someErrorReportFunction = (a, b) => { };
            var thisWillGenerateAnException = new BoundaryConditionFileReader(someParser,
                someMeteoConverter,
                someWindConverter,
                someBoundaryConditionConverter,
                null,
                someErrorReportFunction);
        }

        /// <summary>
        /// GIVEN a BoundaryConditionFileReader
        ///   AND a MeteoFunction
        /// WHEN a file containing meteo data is read with this BoundaryConditionFileReader and this MeteoFunction
        /// THEN the MeteoFunction is updated with this meteo data
        /// </summary>
        [Test]
        public void GivenABoundaryConditionFileReaderAndAMeteoFunction_WhenAFileContainingMeteoDataIsReadWithThisBoundaryConditionFileReaderAndThisMeteoFunction_ThenTheMeteoFunctionIsUpdatedWithThisMeteoData()
        {
            // Given
            const string filePath = "Landjaeger.Meatloaf";
            var errorHandlingHasBeenCalled = false;
            var loggedErrors = new List<string>();
            var errorHeader = "";

            var mocks = new MockRepository();

            // Parser
            var bcCategories = new List<IDelftBcCategory>();

            var someParser = mocks.DynamicMock<Func<string, IList<IDelftBcCategory>>>();
            someParser.Expect(e => e.Invoke(filePath))
                .Return(bcCategories)
                .Repeat.AtLeastOnce();


            // Converters
            var meteoFunctionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction>>();
            meteoFunctionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(meteoFunction)
                .Repeat.AtLeastOnce();

            var windFunction = new WindFunction();
            var windFunctionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, WindFunction>>();
            windFunctionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(windFunction)
                .Repeat.Any();

            var bcDict = new Dictionary<string, BoundaryCondition>();
            var boundaryConditionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>>>();
            boundaryConditionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(bcDict)
                .Repeat.Any();

            var ldDict = new Dictionary<string, LateralDischarge>();
            var lateralDischargeConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>>>();
            lateralDischargeConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(ldDict)
                .Repeat.Any();

            // Error Handling
            Action<string, IList<string>> someErrorReportFunction =
                (header, msgs) =>
                {
                    errorHandlingHasBeenCalled = true;
                    errorHeader = header;
                    loggedErrors.AddRange(msgs);
                };

            // Parameters
            var meteoFuncParameter = new MeteoFunction();
            var windFuncParameter = new WindFunction();

            var boundaryNodes = new EventedList<WaterFlowModel1DBoundaryNodeData>();
            var lateralDischargeNodes = new EventedList<WaterFlowModel1DLateralSourceData>();

            mocks.ReplayAll();
            var reader = new BoundaryConditionFileReader(someParser, meteoFunctionConverter, windFunctionConverter, boundaryConditionConverter, lateralDischargeConverter, someErrorReportFunction);

            // When
            reader.Read(filePath,
                        meteoFuncParameter,
                        windFuncParameter,
                        boundaryNodes,
                        lateralDischargeNodes);

            mocks.VerifyAll();
            // Then
            BoundaryAssertionTestHelper.AssertThatTimeDependentFunctionIsEqualTo(meteoFuncParameter, meteoFunction);

            Assert.That(boundaryNodes, Is.Empty);
            Assert.That(lateralDischargeNodes, Is.Empty);

            Assert.That(errorHandlingHasBeenCalled, Is.False);
        }

        /// <summary>
        /// GIVEN a BoundaryConditionFileReader
        ///   AND a WindFunction
        /// WHEN a file containing wind data is read with this BoundaryConditionFileReader and this WindFunction
        /// THEN the WindFunction is updated with this wind data
        /// </summary>
        [Test]
        public void GivenABoundaryConditionFileReaderAndAWindFunction_WhenAFileContainingWindDataIsReadWithThisBoundaryConditionFileReaderAndThisWindFunction_ThenTheWindFunctionIsUpdatedWithThisWindData()
        {
            // Given
            const string filePath = "Venison.Brisket";
            var errorHandlingHasBeenCalled = false;
            var loggedErrors = new List<string>();
            var errorHeader = "";

            var mocks = new MockRepository();

            // Parser
            var bcCategories = new List<IDelftBcCategory>();

            var someParser = mocks.DynamicMock<Func<string, IList<IDelftBcCategory>>>();
            someParser.Expect(e => e.Invoke(filePath))
                .Return(bcCategories)
                .Repeat.AtLeastOnce();


            // Converters
            var meteoFunctionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction>>();
            meteoFunctionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(new MeteoFunction())
                .Repeat.Any();

            var windFunctionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, WindFunction>>();
            windFunctionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(windFunction)
                .Repeat.AtLeastOnce();

            var bcDict = new Dictionary<string, BoundaryCondition>();
            var boundaryConditionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>>>();
            boundaryConditionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(bcDict)
                .Repeat.Any();

            var ldDict = new Dictionary<string, LateralDischarge>();
            var lateralDischargeConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>>>();
            lateralDischargeConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(ldDict)
                .Repeat.Any();

            // Error Handling
            Action<string, IList<string>> someErrorReportFunction =
                (header, msgs) =>
                {
                    errorHandlingHasBeenCalled = true;
                    errorHeader = header;
                    loggedErrors.AddRange(msgs);
                };

            // Parameters
            var meteoFuncParameter = new MeteoFunction();
            var windFuncParameter = new WindFunction();

            var boundaryNodes = new EventedList<WaterFlowModel1DBoundaryNodeData>();
            var lateralDischargeNodes = new EventedList<WaterFlowModel1DLateralSourceData>();

            mocks.ReplayAll();
            var reader = new BoundaryConditionFileReader(someParser, meteoFunctionConverter, windFunctionConverter, boundaryConditionConverter, lateralDischargeConverter, someErrorReportFunction);

            // When
            reader.Read(filePath,
                        meteoFuncParameter,
                        windFuncParameter,
                        boundaryNodes,
                        lateralDischargeNodes);

            mocks.VerifyAll();
            // Then
            BoundaryAssertionTestHelper.AssertThatTimeDependentFunctionIsEqualTo(windFuncParameter, windFunction);

            Assert.That(boundaryNodes, Is.Empty);
            Assert.That(lateralDischargeNodes, Is.Empty);

            Assert.That(errorHandlingHasBeenCalled, Is.False);
        }

        /// <summary>
        /// GIVEN a BoundaryConditionFileReader
        ///   AND a BoundaryNodeData
        /// WHEN a file containing boundary condition data is read with this BoundaryConditionFileReader and this BoundaryNodeData
        /// THEN the BoundaryNodeData is updated with this boundary condition data
        /// </summary>
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.None, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.None, HasComponent.None)]
        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.TimeDependent, HasComponent.None)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.TimeDependent, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.TimeDependent, HasComponent.None)]

        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.TimeDependent, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.TimeDependent, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.TimeDependent, HasComponent.Constant)]

        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.Constant, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.Constant, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.TimeDependent, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.TimeDependent, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.TimeDependent, HasComponent.TimeDependent)]

        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Level, HasComponent.None, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Level, HasComponent.None, HasComponent.None)]
        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Level, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Level, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.TimeDependent, HasComponent.None)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Level, HasComponent.TimeDependent, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Level, HasComponent.TimeDependent, HasComponent.None)]

        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Level, HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Level, HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Level, HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Level, HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.TimeDependent, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Level, HasComponent.TimeDependent, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Level, HasComponent.TimeDependent, HasComponent.Constant)]

        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Level, HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Level, HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.Constant, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Level, HasComponent.Constant, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Level, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.None, BoundaryConditionConverterTest.WaterType.None, HasComponent.TimeDependent, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Level, HasComponent.TimeDependent, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, BoundaryConditionConverterTest.WaterType.Level, HasComponent.TimeDependent, HasComponent.TimeDependent)]

        [TestCase(HasComponent.Table, BoundaryConditionConverterTest.WaterType.None, HasComponent.None, HasComponent.None)]
        [TestCase(HasComponent.Table, BoundaryConditionConverterTest.WaterType.None, HasComponent.Constant, HasComponent.None)]
        [TestCase(HasComponent.Table, BoundaryConditionConverterTest.WaterType.None, HasComponent.TimeDependent, HasComponent.None)]

        [TestCase(HasComponent.Table, BoundaryConditionConverterTest.WaterType.None, HasComponent.None, HasComponent.Constant)]
        [TestCase(HasComponent.Table, BoundaryConditionConverterTest.WaterType.None, HasComponent.Constant, HasComponent.Constant)]
        [TestCase(HasComponent.Table, BoundaryConditionConverterTest.WaterType.None, HasComponent.TimeDependent, HasComponent.Constant)]

        [TestCase(HasComponent.Table, BoundaryConditionConverterTest.WaterType.None, HasComponent.None, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Table, BoundaryConditionConverterTest.WaterType.None, HasComponent.Constant, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Table, BoundaryConditionConverterTest.WaterType.None, HasComponent.TimeDependent, HasComponent.TimeDependent)]

        public void GivenABoundaryConditionFileReaderAndABoundaryNodeData_WhenAFileContainingBoundaryConditionDataIsReadWithThisBoundaryConditionFileReaderAndThisBoundaryNodeData_ThenTheBoundaryNodeDataIsUpdatedWithThisBoundaryConditionData(HasComponent hasWater, BoundaryConditionConverterTest.WaterType waterType, HasComponent hasSalt, HasComponent hasTemperature)
        {
            // Given
            const string filePath = "Venison.Brisket";
            var errorHandlingHasBeenCalled = false;
            var loggedErrors = new List<string>();
            var errorHeader = "";

            var mocks = new MockRepository();

            // Parser
            var bcCategories = new List<IDelftBcCategory>();

            var someParser = mocks.DynamicMock<Func<string, IList<IDelftBcCategory>>>();
            someParser.Expect(e => e.Invoke(filePath))
                .Return(bcCategories)
                .Repeat.AtLeastOnce();

            // Converters
            var someMeteoFunction = new MeteoFunction();
            var meteoFunctionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction>>();
            meteoFunctionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(someMeteoFunction)
                .Repeat.Any();

            var someWindFunction = new WindFunction();
            var windFunctionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, WindFunction>>();
            windFunctionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(someWindFunction)
                .Repeat.Any();

            var boundaryCondition = GetBoundaryCondition(bcNodeName, hasWater, waterType, hasSalt, hasTemperature);
            var bcDict = new Dictionary<string, BoundaryCondition>() { [bcNodeName] = boundaryCondition };
            var boundaryConditionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>>>();
            boundaryConditionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(bcDict)
                .Repeat.AtLeastOnce();

            var ldDict = new Dictionary<string, LateralDischarge>();
            var lateralDischargeConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>>>();
            lateralDischargeConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(ldDict)
                .Repeat.Any();

            // Error Handling
            Action<string, IList<string>> someErrorReportFunction =
                (header, msgs) =>
                {
                    errorHandlingHasBeenCalled = true;
                    errorHeader = header;
                    loggedErrors.AddRange(msgs);
                };

            // Parameters
            var meteoFuncParameter = new MeteoFunction();
            var windFuncParameter = new WindFunction();

            var boundaryNode = new WaterFlowModel1DBoundaryNodeData { Feature = new HydroNode(bcNodeName) };
            boundaryNode.UseSalt = true;
            boundaryNode.UseTemperature = true;

            var boundaryNodes = new EventedList<WaterFlowModel1DBoundaryNodeData>() { boundaryNode };

            var lateralDischargeNodes = new EventedList<WaterFlowModel1DLateralSourceData>();

            mocks.ReplayAll();
            var reader = new BoundaryConditionFileReader(someParser, meteoFunctionConverter, windFunctionConverter, boundaryConditionConverter, lateralDischargeConverter, someErrorReportFunction);

            // When
            reader.Read(filePath,
                        meteoFuncParameter,
                        windFuncParameter,
                        boundaryNodes,
                        lateralDischargeNodes);

            mocks.VerifyAll();
            // Then
            BoundaryAssertionTestHelper.AssertThatBoundaryConditionIsEqualTo(boundaryNode, boundaryCondition);
            Assert.That(lateralDischargeNodes, Is.Empty);

            Assert.That(errorHandlingHasBeenCalled, Is.False);
        }

        /// <summary>
        /// GIVEN a BoundaryConditionFileReader
        ///   AND a LateralSourceData
        /// WHEN a file containing lateral discharge data is read with this BoundaryConditionFileReader and this LateralSourceData
        /// THEN the LateralSourceData is updated with this lateral discharge data
        /// </summary>
        [TestCase(HasComponent.None, HasComponent.None, LateralDischargeConverterTest.SaltType.None, HasComponent.Constant)]
        [TestCase(HasComponent.None, HasComponent.None, LateralDischargeConverterTest.SaltType.None, HasComponent.TimeDependent)]

        [TestCase(HasComponent.None, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.None, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.None, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.None, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.None, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.None, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.TimeDependent)]

        [TestCase(HasComponent.None, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.None, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.None, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.None, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.None, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.None, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.TimeDependent)]


        [TestCase(HasComponent.Constant, HasComponent.None, LateralDischargeConverterTest.SaltType.None, HasComponent.None)]
        [TestCase(HasComponent.Constant, HasComponent.None, LateralDischargeConverterTest.SaltType.None, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, HasComponent.None, LateralDischargeConverterTest.SaltType.None, HasComponent.TimeDependent)]

        [TestCase(HasComponent.Constant, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.Constant, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.Constant, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.TimeDependent)]

        [TestCase(HasComponent.Constant, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.Constant, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Constant, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.Constant, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.Constant, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.TimeDependent)]


        [TestCase(HasComponent.Table, HasComponent.None, LateralDischargeConverterTest.SaltType.None, HasComponent.None)]
        [TestCase(HasComponent.Table, HasComponent.None, LateralDischargeConverterTest.SaltType.None, HasComponent.Constant)]
        [TestCase(HasComponent.Table, HasComponent.None, LateralDischargeConverterTest.SaltType.None, HasComponent.TimeDependent)]

        [TestCase(HasComponent.Table, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.Table, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.Table, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Table, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.Table, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.Table, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.TimeDependent)]

        [TestCase(HasComponent.Table, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.Table, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.Table, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.Table, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.Table, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.Table, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.TimeDependent)]


        [TestCase(HasComponent.TimeDependent, HasComponent.None, LateralDischargeConverterTest.SaltType.None, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, HasComponent.None, LateralDischargeConverterTest.SaltType.None, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, HasComponent.None, LateralDischargeConverterTest.SaltType.None, HasComponent.TimeDependent)]

        [TestCase(HasComponent.TimeDependent, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.TimeDependent)]

        [TestCase(HasComponent.TimeDependent, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Mass, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Mass, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Mass, HasComponent.TimeDependent)]
        [TestCase(HasComponent.TimeDependent, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.None)]
        [TestCase(HasComponent.TimeDependent, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.Constant)]
        [TestCase(HasComponent.TimeDependent, HasComponent.TimeDependent, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.TimeDependent)]
        public void GivenABoundaryConditionFileReaderAndALateralSourceData_WhenAFileContainingLateralDischargeDataIsReadWithThisBoundaryConditionFileReaderAndThisLateralSourceData_ThenTheLateralSourceDataIsUpdatedWithThisLateralDischargeData(HasComponent hasWater, HasComponent hasSalt, LateralDischargeConverterTest.SaltType saltType, HasComponent hasTemperature)
        {
            // Given
            const string filePath = "Venison.Brisket";
            var errorHandlingHasBeenCalled = false;
            var loggedErrors = new List<string>();
            var errorHeader = "";

            var mocks = new MockRepository();

            // Parser
            var bcCategories = new List<IDelftBcCategory>();

            var someParser = mocks.DynamicMock<Func<string, IList<IDelftBcCategory>>>();
            someParser.Expect(e => e.Invoke(filePath))
                .Return(bcCategories)
                .Repeat.AtLeastOnce();


            // Converters
            var someMeteoFunction = new MeteoFunction();
            var meteoFunctionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction>>();
            meteoFunctionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(someMeteoFunction)
                .Repeat.Any();

            var someWindFunction = new WindFunction();
            var windFunctionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, WindFunction>>();
            windFunctionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(someWindFunction)
                .Repeat.Any();

            var bcDict = new Dictionary<string, BoundaryCondition>();
            var boundaryConditionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>>>();
            boundaryConditionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(bcDict)
                .Repeat.Any();

            var lateralDischarge = GetLateralDischarge(ldNodeName, hasWater, hasSalt, saltType, hasTemperature);
            var ldDict = new Dictionary<string, LateralDischarge>() { [ldNodeName] = lateralDischarge };

            var lateralDischargeConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>>>();
            lateralDischargeConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(ldDict)
                .Repeat.AtLeastOnce();

            // Error Handling
            Action<string, IList<string>> someErrorReportFunction =
                (header, msgs) =>
                {
                    errorHandlingHasBeenCalled = true;
                    errorHeader = header;
                    loggedErrors.AddRange(msgs);
                };

            // Parameters
            var meteoFuncParameter = new MeteoFunction();
            var windFuncParameter = new WindFunction();

            var boundaryNodes = new EventedList<WaterFlowModel1DBoundaryNodeData>();

            var lateralNode = new WaterFlowModel1DLateralSourceData();
            lateralNode.Feature = new LateralSource();
            lateralNode.Feature.Name = ldNodeName;
            lateralNode.UseSalt = true;
            lateralNode.UseTemperature = true;

            var lateralDischargeNodes = new EventedList<WaterFlowModel1DLateralSourceData>() { lateralNode };

            mocks.ReplayAll();
            var reader = new BoundaryConditionFileReader(someParser, meteoFunctionConverter, windFunctionConverter, boundaryConditionConverter, lateralDischargeConverter, someErrorReportFunction);

            // When
            reader.Read(filePath,
                        meteoFuncParameter,
                        windFuncParameter,
                        boundaryNodes,
                        lateralDischargeNodes);

            mocks.VerifyAll();
            // Then
            Assert.That(boundaryNodes, Is.Empty);
            BoundaryAssertionTestHelper.AssertThatLateralDischargeIsEqualTo(lateralNode, lateralDischarge);

            Assert.That(errorHandlingHasBeenCalled, Is.False);
        }


        /// <summary>
        /// GIVEN a BoundaryConditionFileReader with some parser which fails on parsing
        ///   AND some meteo function
        ///   AND some wind function
        ///   AND some set of BoundaryConditions
        ///   AND some set of LateralDischarges
        /// WHEN a file is read with this BoundaryConditionFileReader
        /// THEN an error is logged
        ///  AND the input parameters are unchanged
        /// </summary>
        [Test]
        public void GivenABoundaryConditionFileReaderWithSomeParserWhichFailsOnParsingAndSomeMeteoFunctionAndSomeWindFunctionAndSomeSetOfBoundaryConditionsAndSomeSetOfLateralDischarges_WhenAFileIsReadWithThisBoundaryConditionFileReader_ThenAnErrorIsLoggedAndTheInputParametersAreUnchanged()
        {
            // Given
            const string filePath = "somePath.File";
            var errorHandlingHasBeenCalled = false;
            var loggedErrors = new List<string>();
            var errorHeader = "";

            var mocks = new MockRepository();

            // Parser
            const string errorMsg = "Some exception occurred during reading.";
            var someParser = mocks.DynamicMock<Func<string, IList<IDelftBcCategory>>>();
            someParser.Expect(e => e.Invoke(filePath))
                .Return(null)
                .Throw(new Exception(errorMsg))
                .Repeat.AtLeastOnce();


            // Converters
            Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction> someMeteoConverter = (a, b) => meteoFunction;
            Func<IList<IDelftBcCategory>, IList<string>, WindFunction> someWindConverter = (a, b) => windFunction;
            Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>> someBoundaryConditionConverter = (a, b) => new Dictionary<string, BoundaryCondition>() { [bcNodeName] = boundaryCondition };
            Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>> someLateralDischargeConverter = (a, b) => new Dictionary<string, LateralDischarge>() { [ldNodeName] = lateralDischarge };

            Action<string, IList<string>> someErrorReportFunction =
                (header, msgs) =>
                {
                    errorHandlingHasBeenCalled = true;
                    errorHeader = header;
                    loggedErrors.AddRange(msgs);
                };

            // Parameters
            var meteoFuncParameter = new MeteoFunction();
            var meteoFuncCopy = (MeteoFunction)meteoFuncParameter.Clone(true, false, false);

            var windFuncParameter = new WindFunction();
            var windFuncCopy = (WindFunction)windFuncParameter.Clone(true, false, false);

            var feature = new HydroNode(bcNodeName);
            var boundaryNode = new WaterFlowModel1DBoundaryNodeData();
            boundaryNode.Feature = feature;

            var boundaryNodeCopy = (WaterFlowModel1DBoundaryNodeData)boundaryNode.Clone();
            boundaryNodeCopy.Feature = feature;

            var lateralSourceNode = new LateralSource();
            lateralSourceNode.Name = ldNodeName;
            var lateralDischargeNode = new WaterFlowModel1DLateralSourceData();
            lateralDischargeNode.Feature = lateralSourceNode;

            var lateralDischargeNodeCopy = (WaterFlowModel1DLateralSourceData)lateralDischargeNode.Clone();
            lateralDischargeNodeCopy.Feature = lateralSourceNode;

            var boundaryNodes = new EventedList<WaterFlowModel1DBoundaryNodeData>() { boundaryNode };
            var lateralDischargeNodes = new EventedList<WaterFlowModel1DLateralSourceData>() { lateralDischargeNode };

            mocks.ReplayAll();
            var reader = new BoundaryConditionFileReader(someParser, someMeteoConverter, someWindConverter, someBoundaryConditionConverter, someLateralDischargeConverter, someErrorReportFunction);

            // When
            reader.Read(filePath,
                        meteoFuncParameter,
                        windFuncParameter,
                        boundaryNodes,
                        lateralDischargeNodes);

            mocks.VerifyAll();
            // Then
            BoundaryAssertionTestHelper.AssertThatTimeDependentFunctionIsEqualTo(windFuncParameter, windFuncCopy);
            BoundaryAssertionTestHelper.AssertThatTimeDependentFunctionIsEqualTo(meteoFuncParameter, meteoFuncCopy);

            Assert.That(boundaryNodes.Count, Is.EqualTo(1));
            Assert.That(BoundaryFileReaderTestHelper.CompareBoundaryNodeData(boundaryNode, boundaryNodeCopy, false));

            Assert.That(lateralDischargeNodes.Count, Is.EqualTo(1));
            Assert.That(BoundaryFileReaderTestHelper.CompareLateralSourceData(lateralDischargeNode, lateralDischargeNodeCopy, false));

            Assert.That(errorHandlingHasBeenCalled, Is.True);
            Assert.That(errorHeader, Is.EqualTo("While reading the boundary locations from file, the following errors occured:"));
            Assert.That(loggedErrors.Count, Is.EqualTo(1));
            Assert.That(loggedErrors.Contains(errorMsg));
        }

        /// <summary>
        /// GIVEN a BoundaryConditionFileReader with converters which will report an error
        ///   AND some meteo function
        ///   AND some wind function
        ///   AND some set of BoundaryConditions
        ///   AND some set of LateralDischarges
        /// WHEN a BoundaryConditionFileReader is constructed with these parameters
        /// THEN these errors are logged
        ///  AND the parameters are adjusted appropriately
        /// </summary>
        [Test]
        public void GivenABoundaryConditionFileReaderWithConvertersWhichWillReportAnErrorAndSomeMeteoFunctionAndSomeWindFunctionAndSomeSetOfBoundaryConditionsAndSomeSetOfLateralDischarges_WhenABoundaryConditionFileReaderIsConstructedWithTheseParameters_ThenTheseErrorsAreLoggedAndTheParametersAreAdjustedAppropriately()
        {
            // Given
            const string filePath = "Venison.Brisket";
            var errorHandlingHasBeenCalled = false;
            var loggedErrors = new List<string>();
            var errorHeader = "";

            var mocks = new MockRepository();

            // Parser
            var bcCategories = new List<IDelftBcCategory>();

            var someParser = mocks.DynamicMock<Func<string, IList<IDelftBcCategory>>>();
            someParser.Expect(e => e.Invoke(filePath))
                .Return(bcCategories)
                .Repeat.AtLeastOnce();


            // Converters
            const string errorMsgMeteo = "Shankle drumstick sausage hamburger tenderloin strip steak pork chop swine turducken turkey shoulder meatloaf.";
            var meteoFunctionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction>>();
            meteoFunctionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(meteoFunction)
                .WhenCalled((methodInvocation) => { ((IList<string>)methodInvocation.Arguments[1]).Add(errorMsgMeteo); })
                .Repeat.AtLeastOnce();

            const string errorMsgWind = "Pork chop tenderloin bacon, tri-tip strip steak salami venison sausage chicken pancetta ribeye.";
            var windFunctionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, WindFunction>>();
            windFunctionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(windFunction)
                .WhenCalled((methodInvocation) => { ((IList<string>)methodInvocation.Arguments[1]).Add(errorMsgWind); })
                .Repeat.AtLeastOnce();

            const string errorMsgBoundary = "Corned beef sirloin biltong frankfurter, jowl ground round boudin pig beef pork shoulder turkey meatloaf swine alcatra.";
            var boundaryCondition = GetBoundaryCondition(bcNodeName, HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.Constant, HasComponent.Constant);
            var bcDict = new Dictionary<string, BoundaryCondition>() { [bcNodeName] = boundaryCondition };
            var boundaryConditionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>>>();
            boundaryConditionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(bcDict)
                .WhenCalled((methodInvocation) => { ((IList<string>)methodInvocation.Arguments[1]).Add(errorMsgBoundary); })
                .Repeat.AtLeastOnce();

            const string errorMsgLateral = "Short ribs pig beef pork beef ribs turkey leberkas landjaeger filet mignon.";
            var lateralDischarge = GetLateralDischarge(ldNodeName, HasComponent.Constant, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.Constant);
            var ldDict = new Dictionary<string, LateralDischarge>() { [ldNodeName] = lateralDischarge };
            var lateralDischargeConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>>>();
            lateralDischargeConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(ldDict)
                .WhenCalled((methodInvocation) => { ((IList<string>)methodInvocation.Arguments[1]).Add(errorMsgLateral); })
                .Repeat.AtLeastOnce();

            // Error Handling
            Action<string, IList<string>> someErrorReportFunction =
                (_, msgs) =>
                {
                    errorHandlingHasBeenCalled = true;
                    loggedErrors.AddRange(msgs);
                };

            // Parameters
            var meteoFuncParameter = new MeteoFunction();
            var windFuncParameter = new WindFunction();

            var boundaryNode = new WaterFlowModel1DBoundaryNodeData { Feature = new HydroNode(bcNodeName) };
            boundaryNode.UseSalt = true;
            boundaryNode.UseTemperature = true;

            var boundaryNodes = new EventedList<WaterFlowModel1DBoundaryNodeData>() { boundaryNode };

            var lateralNode = new WaterFlowModel1DLateralSourceData();
            lateralNode.Feature = new LateralSource();
            lateralNode.Feature.Name = ldNodeName;
            lateralNode.UseSalt = true;
            lateralNode.UseTemperature = true;

            var lateralDischargeNodes = new EventedList<WaterFlowModel1DLateralSourceData>() { lateralNode };

            mocks.ReplayAll();
            var reader = new BoundaryConditionFileReader(someParser, meteoFunctionConverter, windFunctionConverter, boundaryConditionConverter, lateralDischargeConverter, someErrorReportFunction);

            // When
            reader.Read(filePath,
                        meteoFuncParameter,
                        windFuncParameter,
                        boundaryNodes,
                        lateralDischargeNodes);

            mocks.VerifyAll();
            // Then
            BoundaryAssertionTestHelper.AssertThatTimeDependentFunctionIsEqualTo(meteoFuncParameter, meteoFunction);
            BoundaryAssertionTestHelper.AssertThatTimeDependentFunctionIsEqualTo(meteoFuncParameter, meteoFunction);
            Assert.That(boundaryNodes.Count, Is.EqualTo(1));
            BoundaryAssertionTestHelper.AssertThatBoundaryConditionIsEqualTo(boundaryNode, boundaryCondition);
            Assert.That(lateralDischargeNodes.Count, Is.EqualTo(1));
            BoundaryAssertionTestHelper.AssertThatLateralDischargeIsEqualTo(lateralNode, lateralDischarge);

            Assert.That(errorHandlingHasBeenCalled, Is.True);
            Assert.That(loggedErrors.Count, Is.EqualTo(4));

            Assert.That(loggedErrors.Contains(errorMsgMeteo));
            Assert.That(loggedErrors.Contains(errorMsgWind));
            Assert.That(loggedErrors.Contains(errorMsgBoundary));
            Assert.That(loggedErrors.Contains(errorMsgLateral));
        }

        /// <summary>
        /// GIVEN a BoundaryConditionFileReader with valid functions
        ///   AND some meteo function
        ///   AND some wind function
        ///   AND some set of BoundaryConditions
        ///   AND some set of LateralDischarges
        /// WHEN a file is read with this BoundaryConditionFileReader
        /// THEN the parameters are adjusted appropriately
        ///  AND no errors are logged
        /// </summary>
        [Test]
        public void GivenABoundaryConditionFileReaderWithValidFunctionsAndSomeMeteoFunctionAndSomeWindFunctionAndSomeSetOfBoundaryConditionsAndSomeSetOfLateralDischarges_WhenAFileIsReadWithThisBoundaryConditionFileReader_ThenTheParametersAreAdjustedAppropriatelyAndNoErrorsAreLogged()
        {
            // Given
            const string filePath = "Buffalo.Prosciutto";
            var errorHandlingHasBeenCalled = false;
            var loggedErrors = new List<string>();
            var errorHeader = "";

            var mocks = new MockRepository();

            // Parser
            var bcCategories = new List<IDelftBcCategory>();

            var someParser = mocks.DynamicMock<Func<string, IList<IDelftBcCategory>>>();
            someParser.Expect(e => e.Invoke(filePath))
                .Return(bcCategories)
                .Repeat.AtLeastOnce();

            // Converters
            var meteoFunctionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, MeteoFunction>>();
            meteoFunctionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(meteoFunction)
                .Repeat.AtLeastOnce();

            var windFunctionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, WindFunction>>();
            windFunctionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(windFunction)
                .Repeat.AtLeastOnce();

            // Boundary Condition converter mock
            const string bcNodeName1 = "meatloaf";
            const string bcNodeName2 = "ham";
            var boundaryCondition1 = GetBoundaryCondition(bcNodeName1, HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.Constant, HasComponent.Constant);
            var boundaryCondition2 = GetBoundaryCondition(bcNodeName2, HasComponent.Constant, BoundaryConditionConverterTest.WaterType.Discharge, HasComponent.TimeDependent, HasComponent.Constant);
            var bcDict = new Dictionary<string, BoundaryCondition>()
            {
                [bcNodeName1] = boundaryCondition1,
                [bcNodeName2] = boundaryCondition2
            };
            var boundaryConditionConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, BoundaryCondition>>>();
            boundaryConditionConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(bcDict)
                .Repeat.AtLeastOnce();

            // Lateral Discharge converter mock
            const string ldNodeName1 = "burgdoggen";
            const string ldNodeName2 = "frankfurter";
            var lateralDischarge1 = GetLateralDischarge(ldNodeName1, HasComponent.TimeDependent, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.Constant);
            var lateralDischarge2 = GetLateralDischarge(ldNodeName2, HasComponent.Constant, HasComponent.Constant, LateralDischargeConverterTest.SaltType.Concentration, HasComponent.TimeDependent);
            var ldDict = new Dictionary<string, LateralDischarge>()
            {
                [ldNodeName1] = lateralDischarge1,
                [ldNodeName2] = lateralDischarge2
            };
            var lateralDischargeConverter =
                mocks.StrictMock<Func<IList<IDelftBcCategory>, IList<string>, IDictionary<string, LateralDischarge>>>();
            lateralDischargeConverter.Expect(e => e.Invoke(Arg<IList<IDelftBcCategory>>.Matches(arg => bcCategories.Equals(arg)), Arg<IList<string>>.Is.NotNull))
                .Return(ldDict)
                .Repeat.AtLeastOnce();

            // Error Handling
            Action<string, IList<string>> someErrorReportFunction =
                (_, msgs) =>
                {
                    errorHandlingHasBeenCalled = true;
                    loggedErrors.AddRange(msgs);
                };

            // Parameters
            var meteoFuncParameter = new MeteoFunction();
            var windFuncParameter = new WindFunction();

            //  - Boundary nodes + copies for verification
            var boundaryNode1 = new WaterFlowModel1DBoundaryNodeData { Feature = new HydroNode(bcNodeName1) };
            boundaryNode1.UseSalt = true;
            boundaryNode1.UseTemperature = true;

            var boundaryNode2 = new WaterFlowModel1DBoundaryNodeData { Feature = new HydroNode(bcNodeName2) };
            boundaryNode2.UseSalt = true;
            boundaryNode2.UseTemperature = true;

            const string bcNodeName3 = "salami";
            var boundaryNode3 = new WaterFlowModel1DBoundaryNodeData { Feature = new HydroNode(bcNodeName3) };
            boundaryNode3.UseSalt = true;
            boundaryNode3.UseTemperature = true;

            var boundaryNode3Copy = new WaterFlowModel1DBoundaryNodeData { Feature = new HydroNode(bcNodeName3) };
            boundaryNode3Copy.UseSalt = true;
            boundaryNode3Copy.UseTemperature = true;

            const string bcNodeName4 = "hamburger";
            var boundaryNode4 = new WaterFlowModel1DBoundaryNodeData { Feature = new HydroNode(bcNodeName4) };
            boundaryNode4.UseSalt = true;
            boundaryNode4.UseTemperature = true;

            var boundaryNode4Copy = new WaterFlowModel1DBoundaryNodeData { Feature = new HydroNode(bcNodeName4) };
            boundaryNode4Copy.UseSalt = true;
            boundaryNode4Copy.UseTemperature = true;

            var boundaryNodes = new EventedList<WaterFlowModel1DBoundaryNodeData>() { boundaryNode3, boundaryNode1, boundaryNode4, boundaryNode2 };

            //  - Lateral nodes + copies for verification
            var lateralNode1 = new WaterFlowModel1DLateralSourceData();
            lateralNode1.Feature = new LateralSource();
            lateralNode1.Feature.Name = ldNodeName1;
            lateralNode1.UseSalt = true;
            lateralNode1.UseTemperature = true;

            var lateralNode2 = new WaterFlowModel1DLateralSourceData();
            lateralNode2.Feature = new LateralSource();
            lateralNode2.Feature.Name = ldNodeName2;
            lateralNode2.UseSalt = true;
            lateralNode2.UseTemperature = true;

            const string ldNodeName3 = "pastrami";
            var lateralNode3 = new WaterFlowModel1DLateralSourceData();
            lateralNode3.Feature = new LateralSource();
            lateralNode3.Feature.Name = ldNodeName3;
            lateralNode3.UseSalt = true;
            lateralNode3.UseTemperature = true;

            var lateralNode3Copy = new WaterFlowModel1DLateralSourceData();
            lateralNode3Copy.Feature = new LateralSource();
            lateralNode3Copy.Feature.Name = ldNodeName3;
            lateralNode3Copy.UseSalt = true;
            lateralNode3Copy.UseTemperature = true;

            const string ldNodeName4 = "shankle";
            var lateralNode4 = new WaterFlowModel1DLateralSourceData();
            lateralNode4.Feature = new LateralSource();
            lateralNode4.Feature.Name = ldNodeName4;
            lateralNode4.UseSalt = true;
            lateralNode4.UseTemperature = true;

            var lateralNode4Copy = new WaterFlowModel1DLateralSourceData();
            lateralNode4Copy.Feature = new LateralSource();
            lateralNode4Copy.Feature.Name = ldNodeName4;
            lateralNode4Copy.UseSalt = true;
            lateralNode4Copy.UseTemperature = true;

            var lateralDischargeNodes = new EventedList<WaterFlowModel1DLateralSourceData>() { lateralNode3, lateralNode2, lateralNode1, lateralNode4 };

            mocks.ReplayAll();
            var reader = new BoundaryConditionFileReader(someParser, meteoFunctionConverter, windFunctionConverter, boundaryConditionConverter, lateralDischargeConverter, someErrorReportFunction);

            // When
            reader.Read(filePath,
                        meteoFuncParameter,
                        windFuncParameter,
                        boundaryNodes,
                        lateralDischargeNodes);

            mocks.VerifyAll();
            // Then
            BoundaryAssertionTestHelper.AssertThatTimeDependentFunctionIsEqualTo(meteoFuncParameter, meteoFunction);
            BoundaryAssertionTestHelper.AssertThatTimeDependentFunctionIsEqualTo(meteoFuncParameter, meteoFunction);

            Assert.That(boundaryNodes.Count, Is.EqualTo(4));
            BoundaryAssertionTestHelper.AssertThatBoundaryConditionIsEqualTo(boundaryNode1, boundaryCondition1);
            BoundaryAssertionTestHelper.AssertThatBoundaryConditionIsEqualTo(boundaryNode2, boundaryCondition2);
            Assert.That(BoundaryFileReaderTestHelper.CompareBoundaryNodeData(boundaryNode3, boundaryNode3Copy, false));
            Assert.That(BoundaryFileReaderTestHelper.CompareBoundaryNodeData(boundaryNode4, boundaryNode4Copy, false));

            Assert.That(lateralDischargeNodes.Count, Is.EqualTo(4));
            BoundaryAssertionTestHelper.AssertThatLateralDischargeIsEqualTo(lateralNode1, lateralDischarge1);
            BoundaryAssertionTestHelper.AssertThatLateralDischargeIsEqualTo(lateralNode2, lateralDischarge2);
            Assert.That(BoundaryFileReaderTestHelper.CompareLateralSourceData(lateralNode3, lateralNode3Copy, false));
            Assert.That(BoundaryFileReaderTestHelper.CompareLateralSourceData(lateralNode4, lateralNode4Copy, false));

            Assert.That(errorHandlingHasBeenCalled, Is.False);
        }


        #region TestHelpers

        private BoundaryCondition GetBoundaryCondition(string name, HasComponent water, BoundaryConditionConverterTest.WaterType waterType, HasComponent salt, HasComponent temperature)
        {
            var boundaryCondition = new BoundaryCondition(name);

            switch (water)
            {
                case HasComponent.Constant:
                    boundaryCondition.WaterComponent = waterType == BoundaryConditionConverterTest.WaterType.Discharge ? constantWaterDischargeBcComponent : constantWaterLevelBcComponent;
                    break;
                case HasComponent.Table:
                    boundaryCondition.WaterComponent = levelDischargeTableBcComponent;
                    break;
                case HasComponent.TimeDependent:
                    boundaryCondition.WaterComponent = waterType == BoundaryConditionConverterTest.WaterType.Discharge ? timeDependentWaterDischargeBcComponent : timeDependentWaterLevelBcComponent;
                    break;
            }

            switch (salt)
            {
                case HasComponent.Constant:
                    boundaryCondition.SaltComponent = constantSaltBcComponent;
                    break;
                case HasComponent.TimeDependent:
                    boundaryCondition.SaltComponent = timeDependentSaltBcComponent;
                    break;
            }

            switch (temperature)
            {
                case HasComponent.Constant:
                    boundaryCondition.TemperatureComponent = constantTemperatureBcComponent;
                    break;
                case HasComponent.TimeDependent:
                    boundaryCondition.TemperatureComponent = timeDependentTemperatureBcComponent;
                    break;
            }

            return boundaryCondition;
        }

        private LateralDischarge GetLateralDischarge(string name, HasComponent water, HasComponent salt, LateralDischargeConverterTest.SaltType type, HasComponent temperature)
        {
            var lateralDischarge = new LateralDischarge(name);

            switch (water)
            {
                case HasComponent.Constant:
                    lateralDischarge.WaterComponent = constantWaterLdComponent;
                    break;
                case HasComponent.Table:
                    lateralDischarge.WaterComponent = tableWaterLdComponent;
                    break;
                case HasComponent.TimeDependent:
                    lateralDischarge.WaterComponent = timeDependentWaterLdComponent;
                    break;
            }

            switch (salt)
            {
                case HasComponent.Constant:
                    lateralDischarge.SaltComponent = type == LateralDischargeConverterTest.SaltType.Concentration
                        ? constantSaltConcentrationLdComponent
                        : constantSaltMassLdComponent;
                    break;
                case HasComponent.TimeDependent:
                    lateralDischarge.SaltComponent = type == LateralDischargeConverterTest.SaltType.Concentration
                        ? timeDependentSaltConcentrationLdComponent
                        : timeDependentSaltMassLdComponent;
                    break;
            }

            switch (temperature)
            {
                case HasComponent.Constant:
                    lateralDischarge.TemperatureComponent = constantTemperatureLdComponent;
                    break;
                case HasComponent.TimeDependent:
                    lateralDischarge.TemperatureComponent = timeDependentTemperatureLdComponent;
                    break;
            }

            return lateralDischarge;
        }


        #endregion
    }
}