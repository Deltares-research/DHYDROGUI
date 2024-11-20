using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class QhFileExporterTest
    {
        private QhFileExporter exporter;

        [SetUp]
        public void Setup()
        {
            exporter = new QhFileExporter();
        }

        [Test]
        public void GivenAnQhFileExporter_WhenTheNamePropertyIsCalled_ThenNameIsReturned()
        {
            const string expectedValue = "Boundary data to .qh file";
            Assert.That(exporter.Name, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenAnQhFileExporter_WhenTheCategoryPropertyIsCalled_ThenTheCategoryIsReturned()
        {
            const string expectedValue = "General";
            Assert.That(exporter.Category, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenAnQhFileExporter_WhenExportIsCalledWithANullItem_ThenFalseIsReturned()
        {
            Assert.That(exporter.Export(null, Arg<string>.Is.Anything), Is.False);
        }

        [Test]
        public void GivenAnQhFileExporterAndAValidItem_WhenExportIsCalledWithAnNullPath_ThenAnErrorIsLoggedAndFalseIsReturned()
        {
            var mocks = new MockRepository();
            var itemMock = mocks.DynamicMock<IBoundaryCondition>();
            var dataFunctionMock = mocks.DynamicMock<IFunction>();

            dataFunctionMock.Expect(n => n.Arguments).Return(null).Repeat.Any();

            itemMock.Expect(n => n.DataType).Return(BoundaryConditionDataType.Qh).Repeat.Any();
            itemMock.Expect(n => n.GetDataAtPoint(Arg<int>.Is.Anything)).IgnoreArguments().Return(dataFunctionMock).Repeat.Any();

            mocks.ReplayAll();

            Assert.That(exporter.Export(itemMock, null), Is.False);

            const string expectedLogMessage = "Failed to export data to";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => exporter.Export(itemMock, null), expectedLogMessage);

            mocks.VerifyAll();
        }

        [Test]
        public void GivenAnQhFileExporterAndAnItemWithoutData_WhenExportIsCalled_ThenFalseIsReturned()
        {
            var mocks = new MockRepository();
            var itemMock = mocks.DynamicMock<IBoundaryCondition>();

            itemMock.Expect(n => n.DataType).Return(BoundaryConditionDataType.Qh).Repeat.Any();
            itemMock.Expect(n => n.GetDataAtPoint(Arg<int>.Is.Anything)).IgnoreArguments().Return(null).Repeat.Any();

            mocks.ReplayAll();

            Assert.That(exporter.Export(itemMock, Arg<string>.Is.Anything), Is.False);
            mocks.VerifyAll();
        }

        [Test]
        public void GivenAnQhFileExporterAndAValidItemAndAValidPath_WhenExportIsCalled_ThenTrueIsReturned()
        {
            var mocks = new MockRepository();
            var itemMock = mocks.DynamicMock<IBoundaryCondition>();
            var dataFunctionMock = mocks.DynamicMock<IFunction>();
            var arrayMock = mocks.DynamicMock<IMultiDimensionalArray>();

            List<object>.Enumerator emptyList = new List<object>().GetEnumerator(); // We need an empty list for QhFile to succeed at writing.
            arrayMock.Expect(n => n.GetEnumerator()).Return(null).WhenCalled(x => x.ReturnValue = emptyList);
            dataFunctionMock.Expect(n => n.Arguments[0].Values).Return(arrayMock).Repeat.Any();

            itemMock.Expect(n => n.DataType).Return(BoundaryConditionDataType.Qh).Repeat.Any();
            itemMock.Expect(n => n.GetDataAtPoint(Arg<int>.Is.Anything)).IgnoreArguments().Return(dataFunctionMock).Repeat.Any();

            mocks.ReplayAll();

            string exportDir = FileUtils.CreateTempDirectory();
            FileUtils.CreateDirectoryIfNotExists(exportDir);
            try
            {
                Assert.That(exporter.Export(itemMock, Path.Combine(exportDir, "myFile.qh")), Is.True);
            }
            finally
            {
                FileUtils.DeleteIfExists(exportDir);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void GivenAnQHFileExporter_WhenSourceTypesIsCalled_ThenAnEmptyEnumerableIsReturned()
        {
            Assert.That(exporter.SourceTypes().Any(), Is.False);
        }

        [Test]
        public void GivenAnQhFileExporter_WhenTheFileFilterPropertyIsCalled_ThenTheFileFilterIsReturned()
        {
            const string expectedValue = "Q-h series series file|*.qh";
            Assert.That(exporter.FileFilter, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenAnQhFileExporter_WhenTheCanExportForIsCalledWithAnyObject_ThenTrueIsReturned()
        {
            Assert.That(exporter.CanExportFor(Arg<object>.Is.Anything), Is.True);
        }

        [Test]
        public void GivenAnQhFileExporter_WhenForcingTypesIsCalled_ThenAnEnumerableWithOnlyQhIsReturned()
        {
            Assert.That(exporter.ForcingTypes.Count(), Is.EqualTo(1));
            Assert.That(exporter.ForcingTypes.Single(), Is.EqualTo(BoundaryConditionDataType.Qh));
        }
    }
}