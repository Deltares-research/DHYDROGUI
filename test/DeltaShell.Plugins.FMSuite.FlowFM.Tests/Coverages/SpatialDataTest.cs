using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Coverages
{
    [TestFixture]
    public class SpatialDataTest
    {
        [Test]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SpatialData(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();

            // Call
            var data = new SpatialData(model);

            // Assert
            Assert.That(data.Bathymetry, Is.Null);
            Assert.That(data.InitialWaterLevel, Is.Null);
            Assert.That(data.InitialSalinity, Is.Null);
            Assert.That(data.InitialTemperature, Is.Null);
            Assert.That(data.Roughness, Is.Null);
            Assert.That(data.Viscosity, Is.Null);
            Assert.That(data.Diffusivity, Is.Null);
            Assert.That(data.InitialTracers, Is.Empty);
            Assert.That(data.InitialFractions, Is.Empty);

            IEventedList<IDataItem> dataItems = data.DataItems;
            Assert.That(dataItems, Has.Count.EqualTo(7));
            Assert.That(dataItems.Select(d => d.Value), Is.All.Null);
        }

        [Test]
        public void SetBathymetry_SetsDataItemValueCorrectly()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            model.ModelDefinition.Returns(new WaterFlowFMModelDefinition());
            var data = new SpatialData(model);
            var coverage = new UnstructuredGridCellCoverage(new UnstructuredGrid(), false);

            // Call
            data.Bathymetry = coverage;

            // Assert
            Assert.That(data.Bathymetry, Is.SameAs(coverage));
            Assert.That(model.ModelDefinition.Bathymetry, Is.SameAs(coverage));

            IDataItem[] dataItems = data.DataItems.Where(d => d.Value != null).ToArray();
            Assert.That(dataItems, Has.Length.EqualTo(1));
            Assert.That(dataItems[0].Value, Is.EqualTo(coverage));
        }

        [Test]
        public void SetInitialWaterLevel_SetsDataItemValueCorrectly()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);
            var coverage = new UnstructuredGridCellCoverage(new UnstructuredGrid(), false);

            // Call
            data.InitialWaterLevel = coverage;

            // Assert
            Assert.That(data.InitialWaterLevel, Is.SameAs(coverage));
            IDataItem[] dataItems = data.DataItems.Where(d => d.Value != null).ToArray();
            Assert.That(dataItems, Has.Length.EqualTo(1));
            Assert.That(dataItems[0].Value, Is.EqualTo(coverage));
        }

        [Test]
        public void SetInitialSalinity_SetsDataItemValueCorrectly()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);
            var coverage = new UnstructuredGridCellCoverage(new UnstructuredGrid(), false);

            // Call
            data.InitialSalinity = coverage;

            // Assert
            Assert.That(data.InitialSalinity, Is.SameAs(coverage));
            IDataItem[] dataItems = data.DataItems.Where(d => d.Value != null).ToArray();
            Assert.That(dataItems, Has.Length.EqualTo(1));
            Assert.That(dataItems[0].Value, Is.EqualTo(coverage));
        }

        [Test]
        public void SetInitialTemperature_SetsDataItemValueCorrectly()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);
            var coverage = new UnstructuredGridCellCoverage(new UnstructuredGrid(), false);

            // Call
            data.InitialTemperature = coverage;

            // Assert
            Assert.That(data.InitialTemperature, Is.SameAs(coverage));
            IDataItem[] dataItems = data.DataItems.Where(d => d.Value != null).ToArray();
            Assert.That(dataItems, Has.Length.EqualTo(1));
            Assert.That(dataItems[0].Value, Is.EqualTo(coverage));
        }

        [Test]
        public void SetRoughness_SetsDataItemValueCorrectly()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);
            var coverage = new UnstructuredGridFlowLinkCoverage(new UnstructuredGrid(), false);

            // Call
            data.Roughness = coverage;

            // Assert
            Assert.That(data.Roughness, Is.SameAs(coverage));
            IDataItem[] dataItems = data.DataItems.Where(d => d.Value != null).ToArray();
            Assert.That(dataItems, Has.Length.EqualTo(1));
            Assert.That(dataItems[0].Value, Is.EqualTo(coverage));
        }

        [Test]
        public void SetViscosity_SetsDataItemValueCorrectly()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);
            var coverage = new UnstructuredGridFlowLinkCoverage(new UnstructuredGrid(), false);

            // Call
            data.Viscosity = coverage;

            // Assert
            Assert.That(data.Viscosity, Is.SameAs(coverage));
            IDataItem[] dataItems = data.DataItems.Where(d => d.Value != null).ToArray();
            Assert.That(dataItems, Has.Length.EqualTo(1));
            Assert.That(dataItems[0].Value, Is.EqualTo(coverage));
        }

        [Test]
        public void SetDiffusivity_SetsDataItemValueCorrectly()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);
            var coverage = new UnstructuredGridFlowLinkCoverage(new UnstructuredGrid(), false);

            // Call
            data.Diffusivity = coverage;

            // Assert
            Assert.That(data.Diffusivity, Is.SameAs(coverage));
            IDataItem[] dataItems = data.DataItems.Where(d => d.Value != null).ToArray();
            Assert.That(dataItems, Has.Length.EqualTo(1));
            Assert.That(dataItems[0].Value, Is.EqualTo(coverage));
        }

        [Test]
        public void GetDataItems_GetsTheCorrectDataItems()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            model.ModelDefinition.Returns(new WaterFlowFMModelDefinition());
            var data = new SpatialData(model);
            var grid = new UnstructuredGrid();

            data.Bathymetry = new UnstructuredGridCellCoverage(grid, false);
            data.InitialWaterLevel = new UnstructuredGridCellCoverage(grid, false);
            data.InitialSalinity = new UnstructuredGridCellCoverage(grid, false);
            data.InitialTemperature = new UnstructuredGridCellCoverage(grid, false);
            data.Roughness = new UnstructuredGridFlowLinkCoverage(grid, false);
            data.Viscosity = new UnstructuredGridFlowLinkCoverage(grid, false);
            data.Diffusivity = new UnstructuredGridFlowLinkCoverage(grid, false);
            data.AddTracer(new UnstructuredGridCellCoverage(grid, false) {Name = "a"});
            data.AddTracer(new UnstructuredGridCellCoverage(grid, false) {Name = "b"});
            data.AddFraction(new UnstructuredGridCellCoverage(grid, false) {Name = "a"});
            data.AddFraction(new UnstructuredGridCellCoverage(grid, false) {Name = "b"});

            // Call
            IEventedList<IDataItem> dataItems = data.DataItems;

            // Assert
            object[] values = dataItems.Select(d => d.Value).ToArray();
            Assert.That(values, Has.Length.EqualTo(11));
            Assert.That(values, Contains.Item(data.Bathymetry));
            Assert.That(values, Contains.Item(data.InitialWaterLevel));
            Assert.That(values, Contains.Item(data.InitialSalinity));
            Assert.That(values, Contains.Item(data.InitialTemperature));
            Assert.That(values, Contains.Item(data.Roughness));
            Assert.That(values, Contains.Item(data.Viscosity));
            Assert.That(values, Contains.Item(data.Diffusivity));
            Assert.That(data.InitialTracers, Is.SubsetOf(values));
            Assert.That(data.InitialFractions, Is.SubsetOf(values));
        }

        [Test]
        public void AddTracer_ArgumentNull_ThrowsArgumentNullException()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);

            // Call
            void Call() => data.AddTracer(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("coverage"));
        }

        [Test]
        public void AddTracer_AddsTracer()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);

            // Call
            data.AddTracer(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "a"});

            // Assert
            UnstructuredGridCellCoverage[] coverages = data.InitialTracers.ToArray();
            Assert.That(coverages, Has.Length.EqualTo(1));
            Assert.That(coverages[0].Name, Is.EqualTo("a"));
        }

        [Test]
        public void AddFraction_ArgumentNull_ThrowsArgumentNullException()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);

            // Call
            void Call() => data.AddFraction(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("coverage"));
        }

        [Test]
        public void AddFraction_DataItemWithSameNameAlreadyExists_Returns()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);

            data.AddFraction(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "a"});

            // Call
            data.AddFraction(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "a"});

            // Assert
            UnstructuredGridCellCoverage[] coverages = data.InitialFractions.ToArray();
            Assert.That(coverages, Has.Length.EqualTo(1));
            Assert.That(coverages[0].Name, Is.EqualTo("a"));
        }

        [Test]
        public void AddTracer_DataItemWithSameNameAlreadyExists_Returns()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);

            data.AddTracer(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "a"});

            // Call
            data.AddTracer(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "a"});

            // Assert
            UnstructuredGridCellCoverage[] coverages = data.InitialTracers.ToArray();
            Assert.That(coverages, Has.Length.EqualTo(1));
            Assert.That(coverages[0].Name, Is.EqualTo("a"));
        }

        [Test]
        public void AddFraction_AddsFraction()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);

            // Call
            data.AddFraction(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "a"});

            // Assert
            UnstructuredGridCellCoverage[] coverages = data.InitialFractions.ToArray();
            Assert.That(coverages, Has.Length.EqualTo(1));
            Assert.That(coverages[0].Name, Is.EqualTo("a"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("b")]
        public void RemoveTracer_DataItemNotFound_Returns(string name)
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);

            data.AddTracer(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "a"});

            // Call
            data.RemoveTracer(name);

            // Assert
            UnstructuredGridCellCoverage[] coverages = data.InitialTracers.ToArray();
            Assert.That(coverages, Has.Length.EqualTo(1));
            Assert.That(coverages[0].Name, Is.EqualTo("a"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("b")]
        public void RemoveFraction_DataItemNotFound_Returns(string name)
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);

            data.AddFraction(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "a"});

            // Call
            data.RemoveFraction(name);

            // Assert
            UnstructuredGridCellCoverage[] coverages = data.InitialFractions.ToArray();
            Assert.That(coverages, Has.Length.EqualTo(1));
            Assert.That(coverages[0].Name, Is.EqualTo("a"));
        }

        [Test]
        public void RemoveTracer_RemovesCorrectTracer()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);

            data.AddTracer(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "a"});
            data.AddTracer(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "b"});
            data.AddTracer(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "c"});

            // Call
            data.RemoveTracer("b");

            // Assert
            UnstructuredGridCellCoverage[] coverages = data.InitialTracers.ToArray();
            Assert.That(coverages, Has.Length.EqualTo(2));
            Assert.That(coverages[0].Name, Is.EqualTo("a"));
            Assert.That(coverages[1].Name, Is.EqualTo("c"));
        }

        [Test]
        public void RemoveFraction_RemovesCorrectFraction()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);

            data.AddFraction(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "a"});
            data.AddFraction(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "b"});
            data.AddFraction(new UnstructuredGridCellCoverage(new UnstructuredGrid(), false) {Name = "c"});

            // Call
            data.RemoveFraction("b");

            // Assert
            UnstructuredGridCellCoverage[] coverages = data.InitialFractions.ToArray();
            Assert.That(coverages, Has.Length.EqualTo(2));
            Assert.That(coverages[0].Name, Is.EqualTo("a"));
            Assert.That(coverages[1].Name, Is.EqualTo("c"));
        }

        [Test]
        public void SwitchTo_SetsCorrectFilePathOnAllImportSamplesOperations()
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);

            ImportSamplesOperation[] operations = SetImportSamplesOperationsOnDataItems(data.DataItems).ToArray();

            // Call
            data.SwitchTo(@"the\new\directory");

            // Assert
            Assert.That(operations[0].FilePath, Is.EqualTo("the\\original\\directory\\file.xyz"));
            foreach (ImportSamplesOperation operation in operations.Skip(1))
            {
                Assert.That(operation.FilePath, Is.EqualTo(@"the\new\directory\file.xyz"));
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void AddTracer_CoverageNameNullOrEmpty_ThrowsArgumentException(string name)
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);
            var grid = new UnstructuredGrid();
            var coverage = new UnstructuredGridCellCoverage(grid, false) {Name = name};

            // Call
            void Call() => data.AddTracer(coverage);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("Name"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void AddFraction_CoverageNameNullOrEmpty_ThrowsArgumentException(string name)
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);
            var grid = new UnstructuredGrid();
            var coverage = new UnstructuredGridCellCoverage(grid, false) {Name = name};

            // Call
            void Call() => data.AddFraction(coverage);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("Name"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void SwitchTo_ArgumentNullOrEmpty_ThrowsArgumentException(string targetDir)
        {
            // Setup
            var model = Substitute.For<IWaterFlowFMModel>();
            var data = new SpatialData(model);

            // Call
            void Call() => data.SwitchTo(targetDir);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("targetDir"));
        }

        private static IEnumerable<ImportSamplesOperation> SetImportSamplesOperationsOnDataItems(IEnumerable<IDataItem> dataItems)
        {
            foreach (IDataItem dataItem in dataItems)
            {
                var operation = new ImportSamplesOperation(false) {FilePath = @"the\original\directory\file.xyz"};
                dataItem.ValueConverter = GetConverterWith(operation);

                yield return operation;
            }
        }

        private static SpatialOperationSetValueConverter GetConverterWith(ISpatialOperation operation)
        {
            var spatialOperationSet = Substitute.For<ISpatialOperationSet>();
            spatialOperationSet.GetOperationsRecursive().Returns(new[]
            {
                operation
            });

            var valueConverter = Substitute.For<SpatialOperationSetValueConverter>();
            valueConverter.SpatialOperationSet.Returns(spatialOperationSet);

            return valueConverter;
        }
    }
}