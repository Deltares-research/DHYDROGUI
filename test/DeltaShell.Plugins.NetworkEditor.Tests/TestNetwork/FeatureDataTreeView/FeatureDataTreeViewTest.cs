using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.NetworkEditor.Tests.TestNetwork.FeatureDataTreeView
{
    [TestFixture]
    public class FeatureDataTreeViewTest
    {
        private static readonly MockRepository mocks = new MockRepository();

/* refactored by Martijn
 * 
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Show()
        {
            var flowModelCollection = new FeatureDataContainer();
            flowModelCollection.Name = "Flow Model 1D";
            IList<IFeatureData> sources = new List<IFeatureData>();

            var qtItem = mocks.Stub<IFeatureData>();
            qtItem.Name = "Q(t)";
            sources.Add(qtItem);
            flowModelCollection.FeatureData = sources;

            var waterQualityCollection = new FeatureDataContainer();
            waterQualityCollection.Name = "Water Quality 1D";
            IList<IFeatureData> wqSources = new List<IFeatureData>();
            var clItem = mocks.Stub<IFeatureData>();
            clItem.Name = "Cl";
            wqSources.Add(clItem);
            waterQualityCollection.FeatureData = wqSources;

            FeatureDataTreeViewData data = new FeatureDataTreeViewData()
                                               {
                                                   Containers = new[] {flowModelCollection,waterQualityCollection}
                                               };

            NetworkEditor.Forms.FeatureDataTreeView.FeatureDataTreeView treeView = new NetworkEditor.Forms.FeatureDataTreeView.FeatureDataTreeView()
                                               {
                                                   Data = data
                                               };
            int callCount = 0;
            treeView.SelectionChanged += delegate
                                             {
                                                 callCount++;
                                             };

            WindowsFormsTestHelper.ShowModal(treeView);
        }
*/
    }
}