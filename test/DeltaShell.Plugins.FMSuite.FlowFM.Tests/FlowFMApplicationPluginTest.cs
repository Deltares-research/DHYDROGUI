using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Gui;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class FlowFMApplicationPluginTest
    {
        private FlowFMApplicationPlugin plugin;

        [TestFixtureSetUp]
        public void SetUp()
        {
            plugin = new FlowFMApplicationPlugin();
        }

        /// <summary>
        /// GIVEN a FlowFMApplicationPlugin
        ///   AND an EventedList of Weir2Ds
        /// WHEN GetExporters is called
        ///  AND GetSupportedExporterForItemIsCalled for these exporter and the EventedList
        /// THEN there exists a PliImporterExporter within the result
        /// </summary>
        [TestCase(typeof(Weir2D))]
        [TestCase(typeof(Pump2D))]
        public void GivenAFlowFMApplicationPlugin_WhenGetExportersIsCalledAndGetSupportedExporterForItemIsCalled_ThenThereExistsAPliImporterExporterWithinTheResult(Type t)
        {
            // Given
            var fileExportersGetterMock =
                MockRepository.GenerateStrictMock<Func<object, IEnumerable<IFileExporter>>>();

            fileExportersGetterMock.Expect(o => o.Invoke(Arg<object>.Is.Anything))
                                   .Return(plugin.GetFileExporters())
                                   .IgnoreArguments()
                                   .Repeat.Once();

            fileExportersGetterMock.Replay();

            Type eventedListType = typeof(EventedList<>).MakeGenericType(t);
            object eventedList = Activator.CreateInstance(eventedListType);

            // When
            var exportHandler = new GuiExportHandler(fileExportersGetterMock, null);
            IList<IFileExporter> filteredExporters = exportHandler.GetSupportedExportersForItem(eventedList).ToList();

            // Then
            fileExportersGetterMock.VerifyAllExpectations();

            // We only check for the GenericType to ensure a PliFileImporterExporter
            // exists for t. We assume that GuiExportHandler.GetSupportedExportersForItem
            // is correct, and thus will produce the right PliFileImporterExporter when
            // it produces a PliFileImporterExporter.
            IFileExporter relevantExporter =
                filteredExporters.FirstOrDefault(e => IsOfGenericType(e, typeof(PliFileImporterExporter<,>)));

            Assert.That(relevantExporter, Is.Not.Null, 
                        "Expected a PliFileImporterExporter within the list of exporters, but found none.");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsCompositeActivity_ThenHelperMethodReturnsCompositeActivityAndThisWillBeUsed()
        {
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNotNull(plugin);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsNull_ThenHelperMethodReturnsNullAndRootFolderWillBeUsed()
        {
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNull(plugin);
        }

        [Test]
        public void GetFileExporters_ContainsExpectedExporterForFixedWeirs()
        {
            // Call
            IEnumerable<IFileExporter> exporters = plugin.GetFileExporters();

            // Assert
            Type expectedType = typeof(PlizFileImporterExporter<FixedWeir, FixedWeir>);
            Assert.NotNull(exporters.SingleOrDefault(e => e.GetType() == expectedType),
                           $"An exporter of type {expectedType} was expected to be returned.");
        }

        [Test]
        public void GetFileImporters_ContainsExpectedImporterForFixedWeirs()
        {
            // Call
            IEnumerable<IFileImporter> importer = plugin.GetFileImporters();

            // Assert
            Type expectedType = typeof(PlizFileImporterExporter<FixedWeir, FixedWeir>);
            Assert.NotNull(importer.SingleOrDefault(e => e.GetType() == expectedType),
                           $"An importer of type {expectedType} was expected to be returned.");
        }

        [Test]
        public void GetFileExporters_ContainsExpectedExporterForEmbankments()
        {
            // Call
            IEnumerable<IFileExporter> exporters = plugin.GetFileExporters();

            // Assert
            Type expectedType = typeof(PlizFileImporterExporter<Embankment, Embankment>);
            var embankmentExporter = (IFeature2DImporterExporter) exporters.SingleOrDefault(e => e.GetType() == expectedType);
            Assert.That(embankmentExporter, Is.Not.Null,
                        $"No file exporter with the expected type was found: {nameof(expectedType)}.");
            Assert.That(embankmentExporter.Mode, Is.EqualTo(Feature2DImportExportMode.Export),
                        $"The property {embankmentExporter.Mode} of the file exporter was incorrect.");
        }

        /// <summary>
        /// Determines whether [Obj] is of the specified [GenericType].
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="GenericType">Type of the generic.</param>
        /// <returns><c>true</c> if [the specified object] [is of generic type]; otherwise, <c>false</c>.</returns>
        private static bool IsOfGenericType(object obj, Type GenericType)
        {
            return obj?.GetType().GetGenericTypeDefinition() == GenericType;
        }
    }
}
