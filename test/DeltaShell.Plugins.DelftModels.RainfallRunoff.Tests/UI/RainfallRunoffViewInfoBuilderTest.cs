using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FeatureCoverageProviders;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class RainfallRunoffViewInfoBuilderTest
    {
        [Test]
        public void BuildViewInfoObjects_ContainsCorrectViewInfoForCatchmentAttributeViewer()
        {
            // Setup
            var plugin = new RainfallRunoffGuiPlugin();

            // Call
            IEnumerable<ViewInfo> viewInfos = RainfallRunoffViewInfoBuilder.BuildViewInfoObjects(plugin);

            // Assert
            ViewInfo viewInfo = viewInfos.FirstOrDefault(vi => vi.Description == "Catchment attribute viewer");
            Assert.That(viewInfo, Is.Not.Null);

            var model = new RainfallRunoffModel();
            TreeFolder treeFolder = GetTreeFolder(model);
            var viewData = viewInfo.GetViewData(treeFolder) as CompositeFeatureCoverageProvider;
            Assert.That(viewData, Is.Not.Null);
            Assert.That(viewData.Model, Is.SameAs(model));

            var view = new CatchmentAttributeCoverageView {Data = viewData};
            bool viewDataContainsData = viewInfo.ViewDataContainsData(view, model);
            Assert.That(viewDataContainsData, Is.True);
        }

        private static TreeFolder GetTreeFolder(object parent) => new TreeFolder(parent, Enumerable.Empty<object>(), "", FolderImageType.Output);
    }
}