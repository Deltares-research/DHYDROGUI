using System.Text.RegularExpressions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.Properties;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class NetworkEditorApplicationPluginTest
    {
        [Test]
        public void CheckNetworkEditorApplicationPluginProperties()
        {
            // Create an application plugin instance
            var applicationPlugin = new NetworkEditorApplicationPlugin();
            Assert.That(applicationPlugin.Name,
                        Is.EqualTo("Network"));
            Assert.That(applicationPlugin.DisplayName,
                        Is.EqualTo(Resources.NetworkEditorApplicationPlugin_DisplayName_Hydro_Region_Plugin));
            Assert.That(applicationPlugin.Description,
                        Is.EqualTo(Resources.NetworkEditorApplicationPlugin_Description));
            Assert.That(applicationPlugin.Version,
                        Is.EqualTo(AssemblyUtils.GetAssemblyInfo(applicationPlugin.GetType().Assembly).Version));
            Assert.IsTrue(new Regex(@"\d.\d.\d.\d").IsMatch(applicationPlugin.FileFormatVersion));
        }

        [Test]
        public void AddChildRegionDataItemsRegionIsNullSoReturnTest()
        {
            var mocks = new MockRepository();
            var dataItemWithoutRegion = mocks.DynamicMock<IDataItem>();
            dataItemWithoutRegion.Expect(d => d.Value).Return(null).Repeat.Once();
            mocks.ReplayAll();
            TypeUtils.CallPrivateStaticMethod(typeof(NetworkEditorApplicationPlugin), "AddChildRegionDataItems",
                                              dataItemWithoutRegion);
            mocks.VerifyAll();
        }
    }
}