using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Gui;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class FlowFMApplicationPluginTest
    {
        private FlowFMApplicationPlugin plugin;

        [OneTimeSetUp]
        public void SetUp()
        {
            plugin = new FlowFMApplicationPlugin();
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
        public void GetFileImporters_ContainsExpectedImporters()
        {
            // Call
            IFileImporter[] importers = plugin.GetFileImporters().ToArray();

            // Assert
            Assert.That(importers, Has.Length.EqualTo(36));
            Contains<WaterFlowFMFileImporter>(importers);
            Contains<Area2DStructuresImporter>(importers);
            Contains<StructuresListImporter>(importers, 2);
            Contains<FMMapFileImporter>(importers);
            Contains<FMHisFileImporter>(importers);
            Contains<FMRestartFileImporter>(importers);
            Contains<BcFileImporter>(importers);
            Contains<BcmFileImporter>(importers);
            Contains<GroupablePointCloudImporter>(importers);
            Contains<PlizFileImporterExporter<FixedWeir, FixedWeir>>(importers);
            Contains<PlizFileImporterExporter<BridgePillar, BridgePillar>>(importers);
            Contains<PliFileImporterExporter<ThinDam2D, ThinDam2D>>(importers);
            Contains<PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>>(importers);
            Contains<PliFileImporterExporter<Structure, Structure>>(importers);
            Contains<PliFileImporterExporter<Pump, Pump>>(importers);
            Contains<PliFileImporterExporter<SourceAndSink, Feature2D>>(importers);
            Contains<PliFileImporterExporter<BoundaryConditionSet, Feature2D>>(importers);
            Contains<PointFileImporterExporter>(importers);
            Contains<ObsFileImporterExporter<GroupableFeature2DPoint>>(importers);
            Contains<PolFileImporterExporter>(importers);
            Contains<LdbFileImporterExporter>(importers);
            Contains<FlowFMNetFileImporter>(importers);
            Contains<TimFileImporter>(importers, 2);
            Contains<ShapeFileImporter<ILineString, LandBoundary2D>>(importers);
            Contains<ShapeFileImporter<IPoint, GroupablePointFeature>>(importers);
            Contains<ShapeFileImporter<IPolygon, GroupableFeature2DPolygon>>(importers);
            Contains<ShapeFileImporter<ILineString, ThinDam2D>>(importers);
            Contains<ShapeFileImporter<ILineString, FixedWeir>>(importers);
            Contains<ShapeFileImporter<IPoint, GroupableFeature2DPoint>>(importers);
            Contains<ShapeFileImporter<ILineString, ObservationCrossSection2D>>(importers);
            Contains<ShapeFileImporter<ILineString, BridgePillar>>(importers);
            Contains<ShapeFileImporter<ILineString, Pump>>(importers);
            Contains<ShapeFileImporter<ILineString, Structure>>(importers);
            Contains<SamplesImporter>(importers);
        }

        private static void Contains<T>(IFileImporter[] source, int n = 1)
        {
            Assert.That(source.OfType<T>().ToList(), Has.Count.EqualTo(n),
                        $"Collection should contain {n} of {typeof(T).Name}");
        }

        [Test]
        public void GetFileExporters_ContainsExpectedExporters()
        {
            // Call
            IFileExporter[] exporters = plugin.GetFileExporters().ToArray();

            // Assert
            Contains<SamplesExporter>(exporters);
        }
        
        private static void Contains<T>(IFileExporter[] source, int n = 1)
        {
            Assert.That(source.OfType<T>().ToList(), Has.Count.EqualTo(n),
                        $"Collection should contain {n} of {typeof(T).Name}");
        }

        /// <summary>
        /// GIVEN a FlowFMApplicationPlugin
        /// AND an EventedList of Weir2Ds
        /// WHEN GetExporters is called
        /// AND GetSupportedExporterForItemIsCalled for these exporter and the EventedList
        /// THEN there exists a PliImporterExporter within the result
        /// </summary>
        [TestCase(typeof(Structure))]
        [TestCase(typeof(Pump))]
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