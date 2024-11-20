using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils.NHibernate;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.NHibernate
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class WaterQualityModel1DNHibernateTest : NHibernateIntegrationTestBase
    {
        private void SaveAndRetrieveFunctionWithObjectTArgument<T>(T object1, T object2)
        {
            var entity = new Function
            {
                Arguments = {new Variable<T>()},
                Components = {new Variable<double>()}
            };

            entity[object1] = 1.1;
            entity[object2] = 2.2;

            Function retrievedEntity = SaveAndRetrieveObject(entity);

            Assert.IsNotNull(retrievedEntity);
            Assert.IsNotNull(retrievedEntity.Arguments);
            Assert.AreEqual(1, retrievedEntity.Arguments.Count);
            Assert.IsNotNull(retrievedEntity.Components);
            Assert.AreEqual(1, retrievedEntity.Components.Count);
            Assert.AreEqual(entity.Arguments[0].Values, retrievedEntity.Arguments[0].Values);
            Assert.AreEqual(entity.Components[0].Values, retrievedEntity.Components[0].Values);
        }

        # region Model

        [Test]
        public void SaveAndRetrieveWaterQualityModel1DSettings()
        {
            var entity = new WaterQualityModelSettings
            {
                HisStartTime = new DateTime(2010, 1, 1),
                HisStopTime = new DateTime(2010, 1, 2),
                HisTimeStep = new TimeSpan(1, 1, 1),
                MapStartTime = new DateTime(2011, 1, 1),
                MapStopTime = new DateTime(2011, 1, 2),
                MapTimeStep = new TimeSpan(2, 2, 2),
                BalanceStartTime = new DateTime(2012, 1, 1),
                BalanceStopTime = new DateTime(2012, 1, 2),
                BalanceTimeStep = new TimeSpan(3, 3, 3),
                NumericalScheme = NumericalScheme.Scheme1,
                NoDispersionIfFlowIsZero = true,
                NoDispersionOverOpenBoundaries = true,
                UseFirstOrder = false,
                UseForesterFilter = true,
                UseAnticreepFilter = true,
                Balance = true,
                LumpProcesses = false,
                LumpTransport = false,
                LumpLoads = false,
                SuppressSpace = false,
                SuppressTime = false,
                BalanceUnit = BalanceUnit.Gram,
                NoBalanceMonitoringPoints = false,
                NoBalanceMonitoringAreas = false,
                NoBalanceMonitoringModelWide = true,
                ProcessesActive = true,
                MonitoringOutputLevel = MonitoringOutputLevel.PointsAndAreas,
                CorrectForEvaporation = false,
                ClosureErrorCorrection = true,
                DryCellThreshold = 0.3d,
                IterationMaximum = 5,
                OutputDirectory = @"D:\Temp\output",
                NrOfThreads = 3,
                Tolerance = 0.2d
            };

            WaterQualityModelSettings retrievedEntity = SaveAndRetrieveObject(entity);

            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual(new DateTime(2010, 1, 1), retrievedEntity.HisStartTime);
            Assert.AreEqual(new DateTime(2010, 1, 2), retrievedEntity.HisStopTime);
            Assert.AreEqual(new TimeSpan(1, 1, 1), retrievedEntity.HisTimeStep);
            Assert.AreEqual(new DateTime(2011, 1, 1), retrievedEntity.MapStartTime);
            Assert.AreEqual(new DateTime(2011, 1, 2), retrievedEntity.MapStopTime);
            Assert.AreEqual(new TimeSpan(2, 2, 2), retrievedEntity.MapTimeStep);
            Assert.AreEqual(new DateTime(2012, 1, 1), retrievedEntity.BalanceStartTime);
            Assert.AreEqual(new DateTime(2012, 1, 2), retrievedEntity.BalanceStopTime);
            Assert.AreEqual(new TimeSpan(3, 3, 3), retrievedEntity.BalanceTimeStep);
            Assert.AreEqual(NumericalScheme.Scheme1, retrievedEntity.NumericalScheme);
            Assert.IsTrue(retrievedEntity.NoDispersionIfFlowIsZero);
            Assert.IsTrue(retrievedEntity.NoDispersionOverOpenBoundaries);
            Assert.IsFalse(retrievedEntity.UseFirstOrder);
            Assert.IsTrue(retrievedEntity.UseForesterFilter);
            Assert.IsTrue(retrievedEntity.UseAnticreepFilter);
            Assert.IsTrue(retrievedEntity.Balance);
            Assert.IsFalse(retrievedEntity.LumpProcesses);
            Assert.IsFalse(retrievedEntity.LumpTransport);
            Assert.IsFalse(retrievedEntity.LumpLoads);
            Assert.IsFalse(retrievedEntity.SuppressSpace);
            Assert.IsFalse(retrievedEntity.SuppressTime);
            Assert.AreEqual(BalanceUnit.Gram, retrievedEntity.BalanceUnit);
            Assert.IsFalse(retrievedEntity.NoBalanceMonitoringPoints);
            Assert.IsFalse(retrievedEntity.NoBalanceMonitoringAreas);
            Assert.IsTrue(retrievedEntity.NoBalanceMonitoringModelWide);
            Assert.IsTrue(retrievedEntity.ProcessesActive);
            Assert.AreEqual(MonitoringOutputLevel.PointsAndAreas, retrievedEntity.MonitoringOutputLevel);
            Assert.IsFalse(retrievedEntity.CorrectForEvaporation);

            Assert.IsTrue(retrievedEntity.ClosureErrorCorrection);
            Assert.AreEqual(0.3d, retrievedEntity.DryCellThreshold);
            Assert.AreEqual(5, retrievedEntity.IterationMaximum);
            Assert.AreEqual(@"D:\Temp\output", retrievedEntity.OutputDirectory);
            Assert.AreEqual(3, retrievedEntity.NrOfThreads);
            Assert.AreEqual(0.2d, retrievedEntity.Tolerance);
        }

        [Test]
        public void SaveAndRetrieveWaterQualityObservationVariableOutputWithObservationVariable()
        {
            var outputVariableTuples = new List<DelftTools.Utils.Tuple<string, string>>
            {
                new DelftTools.Utils.Tuple<string, string>("Substance", "mg/l"),
                new DelftTools.Utils.Tuple<string, string>("Output parameter", "")
            };

            var waterQualityObservationVariableOutput = new WaterQualityObservationVariableOutput(outputVariableTuples) {ObservationVariable = new WaterQualityObservationPoint {Name = "Observation point"}};

            WaterQualityObservationVariableOutput retrievedObject = SaveAndRetrieveObject(waterQualityObservationVariableOutput);

            Assert.IsNotNull(retrievedObject);
            Assert.IsNotNull(retrievedObject.ObservationVariable);
            Assert.AreEqual("Observation point", retrievedObject.Name);
            Assert.IsNotNull(retrievedObject.TimeSeriesList);
            Assert.AreEqual(2, retrievedObject.TimeSeriesList.Count());
            Assert.AreEqual("Substance", retrievedObject.TimeSeriesList.ElementAt(0).Name);
            Assert.AreEqual("Output parameter", retrievedObject.TimeSeriesList.ElementAt(1).Name);
        }

        [Test]
        public void SaveAndRetrieveWaterQualityObservationVariableOutputWithoutObservationVariable()
        {
            var outputVariableTuples = new List<DelftTools.Utils.Tuple<string, string>>
            {
                new DelftTools.Utils.Tuple<string, string>("Substance", "mg/l"),
                new DelftTools.Utils.Tuple<string, string>("Output parameter", "")
            };

            var waterQualityObservationVariableOutput = new WaterQualityObservationVariableOutput(outputVariableTuples) {Name = "Observation point"};

            WaterQualityObservationVariableOutput retrievedObject = SaveAndRetrieveObject(waterQualityObservationVariableOutput);

            Assert.IsNotNull(retrievedObject);
            Assert.IsNull(retrievedObject.ObservationVariable);
            Assert.AreEqual("Observation point", retrievedObject.Name);
            Assert.IsNotNull(retrievedObject.TimeSeriesList);
            Assert.AreEqual(2, retrievedObject.TimeSeriesList.Count());
            Assert.AreEqual("Substance", retrievedObject.TimeSeriesList.ElementAt(0).Name);
            Assert.AreEqual("Output parameter", retrievedObject.TimeSeriesList.ElementAt(1).Name);
        }

        # endregion

        # region Substance process library

        [Test]
        public void SaveAndRetrieveWaterQualitySubstance()
        {
            var entity = new WaterQualitySubstance
            {
                Name = "Substance variable name",
                Description = "Substance variable description",
                Active = true,
                InitialValue = 0.03,
                ConcentrationUnit = "g/L",
                WasteLoadUnit = "g"
            };

            WaterQualitySubstance retrievedEntity = SaveAndRetrieveObject(entity);

            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual("Substance variable name", retrievedEntity.Name);
            Assert.AreEqual("Substance variable description", retrievedEntity.Description);
            Assert.AreEqual(true, retrievedEntity.Active);
            Assert.AreEqual("g/L", retrievedEntity.ConcentrationUnit);
            Assert.AreEqual("g", retrievedEntity.WasteLoadUnit);
            Assert.AreEqual(0.03, retrievedEntity.InitialValue);
        }

        [Test]
        public void SaveAndRetrieveFunctionWithWaterQualitySubstanceArgument()
        {
            SaveAndRetrieveFunctionWithObjectTArgument(new WaterQualitySubstance {Name = "Substance 1"}, new WaterQualitySubstance {Name = "Substance 2"});
        }

        [Test]
        public void SaveAndRetrieveWaterQualityParameter()
        {
            var entity = new WaterQualityParameter
            {
                Name = "Parameter name",
                Description = "Parameter description",
                Unit = "m",
                DefaultValue = 2.1
            };

            WaterQualityParameter retrievedEntity = SaveAndRetrieveObject(entity);

            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual("Parameter name", retrievedEntity.Name);
            Assert.AreEqual("Parameter description", retrievedEntity.Description);
            Assert.IsNotNull(retrievedEntity.Unit);
            Assert.AreEqual("m", retrievedEntity.Unit);
            Assert.AreEqual(2.1, retrievedEntity.DefaultValue);
        }

        [Test]
        public void SaveAndRetrieveWaterQualityProcess()
        {
            var entity = new WaterQualityProcess
            {
                Name = "Process name",
                Description = "Process description"
            };

            WaterQualityProcess retrievedEntity = SaveAndRetrieveObject(entity);

            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual("Process name", retrievedEntity.Name);
            Assert.AreEqual("Process description", retrievedEntity.Description);
        }

        [Test]
        public void SaveAndRetrieveWaterQualityOutputParameter()
        {
            var entity = new WaterQualityOutputParameter
            {
                Name = "Output parameter name",
                Description = "Output parameter description",
                ShowInHis = true,
                ShowInMap = true
            };

            WaterQualityOutputParameter retrievedEntity = SaveAndRetrieveObject(entity);

            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual("Output parameter name", retrievedEntity.Name);
            Assert.AreEqual("Output parameter description", retrievedEntity.Description);
            Assert.AreEqual(true, retrievedEntity.ShowInHis);
            Assert.AreEqual(true, retrievedEntity.ShowInMap);
        }

        [Test]
        public void SaveAndRetrieveSubstanceProcessLibrary()
        {
            var entity = new SubstanceProcessLibrary
            {
                Name = "Substance process library",
                Substances =
                {
                    new WaterQualitySubstance {Name = "Substance 1"},
                    new WaterQualitySubstance {Name = "Substance 2"}
                },
                Processes =
                {
                    new WaterQualityProcess {Name = "Process 1"},
                    new WaterQualityProcess {Name = "Process 2"}
                },
                Parameters =
                {
                    new WaterQualityParameter {Name = "Parameter 1"},
                    new WaterQualityParameter {Name = "Parameter 2"}
                },
                OutputParameters =
                {
                    new WaterQualityOutputParameter {Name = "Output parameter 1"},
                    new WaterQualityOutputParameter {Name = "Output parameter 2"}
                },
                ProcessDllFilePath = "Process dll file path",
                ProcessDefinitionFilesPath = "Process definition files path"
            };

            SubstanceProcessLibrary retrievedEntity = SaveAndRetrieveObject(entity);

            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual("Substance process library", retrievedEntity.Name);
            Assert.AreEqual(2, retrievedEntity.Substances.Count());
            Assert.AreEqual("Substance 1", retrievedEntity.Substances[0].Name);
            Assert.AreEqual("Substance 2", retrievedEntity.Substances[1].Name);
            Assert.AreEqual(2, retrievedEntity.Processes.Count());
            Assert.AreEqual("Process 1", retrievedEntity.Processes[0].Name);
            Assert.AreEqual("Process 2", retrievedEntity.Processes[1].Name);
            Assert.AreEqual(2, retrievedEntity.Parameters.Count());
            Assert.AreEqual("Parameter 1", retrievedEntity.Parameters[0].Name);
            Assert.AreEqual("Parameter 2", retrievedEntity.Parameters[1].Name);
            Assert.AreEqual(2, retrievedEntity.OutputParameters.Count());
            Assert.AreEqual("Output parameter 1", retrievedEntity.OutputParameters[0].Name);
            Assert.AreEqual("Output parameter 2", retrievedEntity.OutputParameters[1].Name);
            Assert.AreEqual("Process dll file path", retrievedEntity.ProcessDllFilePath);
            Assert.AreEqual("Process definition files path", retrievedEntity.ProcessDefinitionFilesPath);
        }

        # endregion

        # region Initial conditions, process coefficients, dispersion and meteo (each of these entities is either a const function, coverage function or time series function)

        [Test]
        public void SaveAndRetrieveWaterQualityFunctionFactoryConst()
        {
            IFunction waterQualityFunctionFactoryConst = WaterQualityFunctionFactory.CreateConst("Name", 10.2, "Component name", "Unit", "Description");

            IFunction retrievedObject = SaveAndRetrieveObject(waterQualityFunctionFactoryConst);

            Assert.IsNotNull(retrievedObject);
            Assert.IsTrue(retrievedObject.IsConst());
            Assert.AreEqual("Name", retrievedObject.Name);
            Assert.AreEqual(10.2, retrievedObject.Components[0].DefaultValue);
            Assert.AreEqual("Component name", retrievedObject.Components[0].Name);
            Assert.AreEqual("Unit", retrievedObject.Components[0].Unit.Name);
            Assert.AreEqual("Unit", retrievedObject.Components[0].Unit.Symbol);
        }

        [Test]
        public void SaveAndRetrieveWaterQualityFunctionFactoryTimeSeries()
        {
            IFunction waterQualityFunctionFactoryTimeSeries = WaterQualityFunctionFactory.CreateTimeSeries("Name", 10.2, "Component name", "Unit", "Description");

            waterQualityFunctionFactoryTimeSeries[new DateTime(2010, 1, 1, 0, 0, 1)] = 10.5;
            waterQualityFunctionFactoryTimeSeries.Arguments[0].InterpolationType = InterpolationType.Linear;

            IFunction retrievedObject = SaveAndRetrieveObject(waterQualityFunctionFactoryTimeSeries);

            Assert.AreNotEqual(null, retrievedObject);
            Assert.IsTrue(retrievedObject.IsTimeSeries());
            Assert.AreEqual("Name", retrievedObject.Name);
            Assert.AreEqual(10.2, retrievedObject.Components[0].DefaultValue);
            Assert.AreEqual("Component name", retrievedObject.Components[0].Name);
            Assert.AreEqual("Unit", retrievedObject.Components[0].Unit.Name);
            Assert.AreEqual("Unit", retrievedObject.Components[0].Unit.Symbol);
            Assert.AreEqual(10.5, retrievedObject[new DateTime(2010, 1, 1, 0, 0, 1)]);
            Assert.AreEqual(InterpolationType.Linear, retrievedObject.Arguments[0].InterpolationType);
            Assert.AreEqual("Description", retrievedObject.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE]);
        }

        # endregion

        protected override NHibernateProjectRepository CreateProjectRepository()
        {
            return new DHYDRONHibernateProjectRepositoryBuilder().WithWaterQuality().Build();
        }
    }
}