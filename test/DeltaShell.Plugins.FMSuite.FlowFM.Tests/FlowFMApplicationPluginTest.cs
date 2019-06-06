using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters;
using NUnit.Framework;
using Rhino.Mocks;


namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class FlowFMApplicationPluginTest
    {
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
            var plugin = new FlowFMApplicationPlugin();
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
