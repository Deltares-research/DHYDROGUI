using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class CmpFileExporterTest
    {
        private MockRepository mocks;
        private CmpFileExporter exporter;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            exporter = new CmpFileExporter();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void GivenANonIBoundaryConditionWhenExportingThenReturnFalse()
        {
            bool exportResult = exporter.Export("TheString", Arg<string>.Is.Anything);
            Assert.IsFalse(exportResult);
        }

        [Test]
        [TestCase(BoundaryConditionDataType.Empty)]
        [TestCase(BoundaryConditionDataType.Constant)]
        [TestCase(BoundaryConditionDataType.ParameterizedSpectrumConstant)]
        [TestCase(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)]
        [TestCase(BoundaryConditionDataType.Qh)]
        [TestCase(BoundaryConditionDataType.SpectrumFromFile)]
        [TestCase(BoundaryConditionDataType.TimeSeries)]
        public void GivenAnIBoundaryConditionWhenExportingWithNonValidDataTypeThenReturnFalse(BoundaryConditionDataType type)
        {
            var boundaryCondition = mocks.DynamicMock<IBoundaryCondition>();
            boundaryCondition.Expect(bc => bc.DataType).Return(type).Repeat.Any();
            mocks.ReplayAll();

            bool exportResult = exporter.Export(boundaryCondition, Arg<string>.Is.Anything);
            Assert.IsFalse(exportResult);
        }

        [Test]
        [TestCase(BoundaryConditionDataType.AstroComponents)]
        [TestCase(BoundaryConditionDataType.AstroCorrection)]
        [TestCase(BoundaryConditionDataType.Harmonics)]
        [TestCase(BoundaryConditionDataType.HarmonicCorrection)]
        public void GivenAnIBoundaryConditionWhenExportingWithFilePathEqualToNullThenReturnFalse(BoundaryConditionDataType type)
        {
            var boundaryCondition = mocks.DynamicMock<IBoundaryCondition>();
            boundaryCondition.Expect(bc => bc.DataType).Return(type).Repeat.Any();
            mocks.ReplayAll();

            bool exportResult = exporter.Export(boundaryCondition, null);
            Assert.IsFalse(exportResult);
        }

        [Test]
        public void GivenABoundaryConditionWhenExportingWithValidFilePathThenWritesCmpFileAndReturnFalse()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var boundaryCondition = mocks.DynamicMock<IBoundaryCondition>();
                boundaryCondition.Expect(bc => bc.DataType).Return(BoundaryConditionDataType.AstroComponents).Repeat.Any();
                mocks.ReplayAll();

                bool exportResult = exporter.Export(boundaryCondition, Path.Combine(temporaryDirectory.Path, "dummy.cmp"));
                Assert.IsTrue(exportResult);
            }
        }

        [Test]
        public void GivenCmpFileExporterWhenGettingSourceTypesThenReturnEmptyCollection()
        {
            IList<Type> sourceTypes = exporter.SourceTypes().AsList();
            Assert.IsEmpty(sourceTypes);
        }

        [Test]
        public void GivenCmpFileExporterWhenRequestingFileFilterThenReturnStringThatEndsWithCmp()
        {
            string filter = exporter.FileFilter;
            Assert.That(filter.EndsWith("*.cmp"), Is.True);
        }
    }
}