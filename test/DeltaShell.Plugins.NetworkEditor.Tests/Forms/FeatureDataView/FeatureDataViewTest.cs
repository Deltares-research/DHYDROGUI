using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.FeatureDataView
{
    
    [TestFixture]
    public class FeatureDataViewTest
    {
        private static readonly MockRepository mocks = new MockRepository();

/* being refactored by Martijn 
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Show()
        {
            //mock a gui returning something for WQLSD
            var gui = mocks.Stub<IGui>();

            var viewProvider = mocks.Stub<IViewProvider>();
            Expect.Call(viewProvider.GetCompatibleViewTypes(null)).IgnoreArguments().Return(new Type[0]).Repeat.Any();

            //Return the viewprovider
            Expect.Call(gui.viewProvider).Return(viewProvider).Repeat.Any();
            
            mocks.ReplayAll();
            //Gui.viewProvider = viewProvider;

            var flowModelCollection = new FeatureDataContainer();
            flowModelCollection.Name = "Flow Model 1D";
            IList<IFeatureData> sources = new List<IFeatureData>();
            var item = mocks.Stub<IFeatureData>();
            item.Name = "Hallo";
            sources.Add(item);
            flowModelCollection.FeatureData = sources;

            var waterQualityCollection = new FeatureDataContainer();
            waterQualityCollection.Name = "Water Quality 1D";
            IList<IFeatureData> wqSources = new List<IFeatureData>();
            wqSources.Add(mocks.Stub<IFeatureData>());
            waterQualityCollection.FeatureData = wqSources;

            var data = new FeatureDataViewData()
                           {
                               Containers = new[] {flowModelCollection, waterQualityCollection},
                               FeatureName = "Scheveningen"
                           };

            var localizedDataView = new NetworkEditor.Forms.FeatureDataView.FeatureDataView()
                                        {
                                            Gui = gui,
                                            Data = data
                                        };
            
            
            WindowsFormsTestHelper.ShowModal(localizedDataView);
        }
        */

    }
}