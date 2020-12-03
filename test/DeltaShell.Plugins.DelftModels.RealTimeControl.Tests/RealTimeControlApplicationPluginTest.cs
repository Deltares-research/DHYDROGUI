using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlApplicationPluginTest
    {
        [Test]
        public void FileFormatVersion_ShouldReturnCurrentVersionNumber()
        {
            var applicationPlugin = new RealTimeControlApplicationPlugin();
            Assert.AreEqual("3.8.0.0", applicationPlugin.FileFormatVersion);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsCompositeActivity_ThenHelperMethodReturnsCompositeActivityAndThisWillBeUsed()
        {
            var realTimeControlApplicationPlugin = new RealTimeControlApplicationPlugin();
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNotNull(realTimeControlApplicationPlugin);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsNull_ThenHelperMethodReturnsNullAndRootFolderWillBeUsed()
        {
            var realTimeControlApplicationPlugin = new RealTimeControlApplicationPlugin();
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNull(realTimeControlApplicationPlugin);
        }

        [Test]
        [TestCase(typeof(RealTimeControlModelExporter))]
        [TestCase(typeof(RealTimeControlRestartFileExporter))]
        public void GetFileExporters_ContainsExpectedExporter(Type exporterType)
        {
            // Setup
            var plugin = new RealTimeControlApplicationPlugin();

            // Call
            IEnumerable<IFileExporter> exporters = plugin.GetFileExporters();

            // Assert
            Assert.NotNull(exporters.SingleOrDefault(e => e.GetType() == exporterType));
        }
    }
}