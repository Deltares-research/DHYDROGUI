using System;
using System.IO;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class WindExtForceTest
    {
        private void CheckFilesExist(IWindField windField)
        {
            var griddedWindField = windField as GriddedWindField;
            if (griddedWindField != null)
            {
                Assert.IsTrue(File.Exists(griddedWindField.WindFilePath));
                if (griddedWindField.SeparateGridFile)
                {
                    Assert.IsTrue(File.Exists(griddedWindField.GridFilePath));
                }
            }
            var spiderWebWindField = windField as SpiderWebWindField;
            if (spiderWebWindField != null)
            {
                Assert.IsTrue(File.Exists(spiderWebWindField.WindFilePath));
            }
        }

        private void CompareVariables(IVariable firstVariable, IVariable secondVariable)
        {
            Assert.AreEqual(firstVariable.Name, secondVariable.Name);
            Assert.AreEqual(firstVariable.ValueType, secondVariable.ValueType);

            if (firstVariable.ValueType == typeof (DateTime))
            {
                Assert.AreEqual(firstVariable.GetValues<DateTime>(), secondVariable.GetValues<DateTime>());
                return;
            }
            if (firstVariable.ValueType == typeof (double))
            {
                Assert.AreEqual(firstVariable.GetValues<double>(), secondVariable.GetValues<double>());
                return;
            }

            throw new NotImplementedException(string.Format("Comparing variables with value type {0} not supported",
                firstVariable.ValueType));
        }

        private void CompareWindFields(IWindField firstWindField, IWindField secondWindField)
        {
            Assert.AreEqual(firstWindField.Quantity, secondWindField.Quantity);
            Assert.AreEqual(firstWindField.GetType(), secondWindField.GetType());
            if (firstWindField.Data == null)
            {
                Assert.IsNull(secondWindField.Data);
            }
            else
            {
                Assert.AreEqual(firstWindField.Data.Arguments.Count, secondWindField.Data.Arguments.Count);
                for (int i = 0; i < firstWindField.Data.Arguments.Count; ++i)
                {
                    CompareVariables(firstWindField.Data.Arguments[i], secondWindField.Data.Arguments[i]);
                }
                for (int i = 0; i < secondWindField.Data.Components.Count; ++i)
                {
                    CompareVariables(firstWindField.Data.Components[i], secondWindField.Data.Components[i]);
                }
            }

            var firstGridWindField = firstWindField as GriddedWindField;
            var secondGridWindField = secondWindField as GriddedWindField;
            if (firstGridWindField != null && secondGridWindField != null)
            {
                Assert.AreEqual(Path.GetFileName(firstGridWindField.WindFilePath), Path.GetFileName(secondGridWindField.WindFilePath));
                Assert.AreEqual(Path.GetFileName(firstGridWindField.GridFilePath), Path.GetFileName(secondGridWindField.GridFilePath));
            }

            var firstSpiderWebWindField = firstWindField as SpiderWebWindField;
            var secondSpiderWebWindField = secondWindField as SpiderWebWindField;
            if (firstSpiderWebWindField != null && secondSpiderWebWindField != null)
            {
                Assert.AreEqual(Path.GetFileName(firstSpiderWebWindField.WindFilePath), Path.GetFileName(secondSpiderWebWindField.WindFilePath));
            }
        }

        private string windXFile;
        private string windYFile;
        private string windPFile;
        private string windCurviFile;
        private string windCurviGrid;
        private string windSpwFile;

        [SetUp]
        public void CreateDummyWindFiles()
        {
            windXFile = TestHelper.GetTestFilePath(@"windtest\windX.amu");
            windYFile = TestHelper.GetTestFilePath(@"windtest\windY.amv");
            windPFile = TestHelper.GetTestFilePath(@"windtest\windP.amp");
            windCurviFile = TestHelper.GetTestFilePath(@"windtest\windXYP.apwxwy");
            windCurviGrid = TestHelper.GetTestFilePath(@"windtest\windXYP.grd");
            windSpwFile = TestHelper.GetTestFilePath(@"windtest\wind.spw");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadUniformForcings()
        {
            var modelDefinition = new WaterFlowFMModelDefinition("testmodel");
            var refDate = new DateTime(1981, 8, 29);
            modelDefinition.GetModelProperty(KnownProperties.RefDate).Value = DateOnly.FromDateTime(refDate);
            
            var windXField = UniformWindField.CreateWindXSeries();
            windXField.Data.Arguments[0].SetValues(new[] {refDate, refDate.AddHours(6), refDate.AddHours(12)});
            windXField.Data.Components[0].SetValues(new[] {-2.5, 0.0, 1.5});
            modelDefinition.WindFields.Add(windXField);

            var windYField = UniformWindField.CreateWindYSeries();
            windYField.Data.Arguments[0].SetValues(new[] { refDate, refDate.AddHours(6), refDate.AddHours(12) });
            windYField.Data.Components[0].SetValues(new[] { -1.5, 0.0, 2.5 });
            modelDefinition.WindFields.Add(windYField);

            var windXField2 = UniformWindField.CreateWindXSeries();
            windXField2.Data.Arguments[0].SetValues(new[] { refDate, refDate.AddHours(6), refDate.AddHours(12) });
            windXField2.Data.Components[0].SetValues(new[] { 0.05, 0.05, 0.0 });
            modelDefinition.WindFields.Add(windXField2);

            var windPolarField = UniformWindField.CreateWindPolarSeries();
            windPolarField.Data.Arguments[0].SetValues(new[] { refDate, refDate.AddHours(6), refDate.AddHours(12) });
            windPolarField.Data.Components[0].SetValues(new[] { 1.5, 1.0, 0.88 });
            windPolarField.Data.Components[0].SetValues(new[] { 173.0, 88.0, 34.0 });
            modelDefinition.WindFields.Add(windPolarField);

            var writer = new ExtForceFile();
            writer.Write("testmodel", modelDefinition);

            var loadedModelDefinition = new WaterFlowFMModelDefinition();
            loadedModelDefinition.GetModelProperty(KnownProperties.RefDate).Value = DateOnly.FromDateTime(refDate);
            writer.Read("testmodel", loadedModelDefinition);

            Assert.AreEqual(modelDefinition.WindFields.Count, loadedModelDefinition.WindFields.Count);
            for (var i = 0; i < loadedModelDefinition.WindFields.Count; ++i)
            {
                CompareWindFields(modelDefinition.WindFields[i], loadedModelDefinition.WindFields[i]);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadArcInfoWindForcing()
        {
            var modelDefinition = new WaterFlowFMModelDefinition("testmodel");
            var refDate = new DateTime(1981, 8, 29);
            modelDefinition.GetModelProperty(KnownProperties.RefDate).Value = DateOnly.FromDateTime(refDate);

            modelDefinition.WindFields.Add(GriddedWindField.CreateXField(windXFile));
            modelDefinition.WindFields.Add(GriddedWindField.CreateYField(windYFile));
            modelDefinition.WindFields.Add(GriddedWindField.CreatePressureField(windPFile));
            modelDefinition.WindFields.Add(GriddedWindField.CreateCurviField(windCurviFile, windCurviGrid));
            
            var writer = new ExtForceFile();
            writer.Write("testmodel", modelDefinition);

            var loadedModelDefinition = new WaterFlowFMModelDefinition();
            loadedModelDefinition.GetModelProperty(KnownProperties.RefDate).Value = DateOnly.FromDateTime(refDate);
            writer.Read("testmodel", loadedModelDefinition);

            Assert.AreEqual(modelDefinition.WindFields.Count, loadedModelDefinition.WindFields.Count);
            for (var i = 0; i < loadedModelDefinition.WindFields.Count; ++i)
            {
                CheckFilesExist(loadedModelDefinition.WindFields[i]);
                CompareWindFields(modelDefinition.WindFields[i], loadedModelDefinition.WindFields[i]);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadUniformForcingWithSpiderWeb()
        {
            var modelDefinition = new WaterFlowFMModelDefinition("testmodel");
            var refDate = new DateTime(1981, 8, 29);
            modelDefinition.GetModelProperty(KnownProperties.RefDate).Value = DateOnly.FromDateTime(refDate);

            var windXYField = UniformWindField.CreateWindXYSeries();
            windXYField.Data.Arguments[0].SetValues(new[] { refDate, refDate.AddHours(6), refDate.AddHours(12) });
            windXYField.Data.Components[0].SetValues(new[] { -2.5, 0.0, 1.5 });
            windXYField.Data.Components[0].SetValues(new[] { -1.5, 1.0, 1.5 });
            modelDefinition.WindFields.Add(windXYField);

            modelDefinition.WindFields.Add(SpiderWebWindField.Create(windSpwFile));

            var writer = new ExtForceFile();
            writer.Write("testmodel", modelDefinition);

            var loadedModelDefinition = new WaterFlowFMModelDefinition();
            loadedModelDefinition.GetModelProperty(KnownProperties.RefDate).Value = DateOnly.FromDateTime(refDate);
            writer.Read("testmodel", loadedModelDefinition);

            Assert.AreEqual(modelDefinition.WindFields.Count, loadedModelDefinition.WindFields.Count);
            for (var i = 0; i < loadedModelDefinition.WindFields.Count; ++i)
            {
                CheckFilesExist(loadedModelDefinition.WindFields[i]);
                CompareWindFields(modelDefinition.WindFields[i], loadedModelDefinition.WindFields[i]);
            }
        }
    }
}
