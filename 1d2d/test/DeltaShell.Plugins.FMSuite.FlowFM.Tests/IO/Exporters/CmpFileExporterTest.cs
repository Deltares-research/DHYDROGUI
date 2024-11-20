using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
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
            var exportResult = exporter.Export("TheString", Arg<string>.Is.Anything);
            Assert.IsFalse(exportResult);
        }

        [Test]
        [TestCase(BoundaryConditionDataType.Empty)]
        [TestCase(BoundaryConditionDataType.Constant)]
        [TestCase(BoundaryConditionDataType.ParametrizedSpectrumConstant)]
        [TestCase(BoundaryConditionDataType.ParametrizedSpectrumTimeseries)]
        [TestCase(BoundaryConditionDataType.Qh)]
        [TestCase(BoundaryConditionDataType.SpectrumFromFile)]
        [TestCase(BoundaryConditionDataType.TimeSeries)]
        public void GivenAnIBoundaryConditionWhenExportingWithNonValidDataTypeThenReturnFalse(BoundaryConditionDataType type)
        {
            var boundaryCondition = mocks.DynamicMock<IBoundaryCondition>();
            boundaryCondition.Expect(bc => bc.DataType).Return(type).Repeat.Any();
            mocks.ReplayAll();

            var exportResult = exporter.Export(boundaryCondition, Arg<string>.Is.Anything);
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

            var exportResult = exporter.Export(boundaryCondition, null);
            Assert.IsFalse(exportResult);
        }

        [Test]
        public void GivenABoundaryConditionWhenExportingWithValidFilePathThenWritesCmpFileAndReturnFalse()
        {
            var dummyFilePath = TestHelper.GetTestFilePath(Path.Combine("cmpFiles", "dummy.cmp"));

            var boundaryCondition = mocks.DynamicMock<IBoundaryCondition>();
            boundaryCondition.Expect(bc => bc.DataType).Return(BoundaryConditionDataType.AstroComponents).Repeat.Any();
            mocks.ReplayAll();

            var exportResult = exporter.Export(boundaryCondition, dummyFilePath);
            Assert.IsTrue(exportResult);
        }

        [Test]
        public void GivenCmpFileExporterWhenGettingSourceTypesThenReturnEmptyCollection()
        {
            var sourceTypes = exporter.SourceTypes().AsList();
            Assert.IsEmpty(sourceTypes);
        }

        [Test]
        public void GivenCmpFileExporterWhenRequestingFileFilterThenReturnStringThatEndsWithCmp()
        {
            var filter = exporter.FileFilter;
            Assert.That(filter.EndsWith("*.cmp"), Is.True);
        }
    }
}