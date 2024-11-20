using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView
{
    [TestFixture]
    public class CrossSectionViewModelTest
    {
        [Test]
        public void AddingDefinitionToSharedDefinitionsFireDefinitionsChanged()
        {
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var definition = new CrossSectionDefinitionYZ();

            var crossSection = new CrossSection(definition) {Branch = hydroNetwork.Channels.First()};

            int callCount = 0;
            var viewModel = new CrossSectionViewModel(crossSection);
            viewModel.SharedDefinitionsChanged += (s, e) => { callCount++; };

            //action 
            hydroNetwork.SharedCrossSectionDefinitions.Add(new CrossSectionDefinitionYZ());

            Assert.AreEqual(1, callCount);
        }
        [Test]
        public void FirePropertyChangedOnCanSelectSharedWhenAddingOrRemovingDefinitions()
        {
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var definition = new CrossSectionDefinitionYZ();

            var crossSection = new CrossSection(definition) { Branch = hydroNetwork.Channels.First() };

            int callCount = 0;
            var viewModel = new CrossSectionViewModel(crossSection);
            viewModel.PropertyChanged += (s, e) =>
                                             {
                                                 callCount++;
                                                 Assert.AreEqual("CanSelectSharedDefinitions", e.PropertyName);
                                             };

            //both add and remove should cause a change
            hydroNetwork.SharedCrossSectionDefinitions.Add(new CrossSectionDefinitionYZ());
            hydroNetwork.SharedCrossSectionDefinitions.RemoveAt(0);
            Assert.AreEqual(2,callCount);
        }

        [Test]
        public void MakingACrossSectionLocalCreatesACopyOfTemplateDefinition()
        {
            var innerDefinition = CrossSectionDefinitionYZ.CreateDefault();
            
            var proxy = new CrossSectionDefinitionProxy(innerDefinition);

            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            var crossSection = new CrossSection(proxy) {Branch = hydroNetwork.Channels.First()};

            var viewModel = new CrossSectionViewModel(crossSection);

            Assert.IsFalse(viewModel.UseLocalDefinition);
            //action! change it to local
            viewModel.UseLocalDefinition= true;

            Assert.IsTrue(viewModel.UseLocalDefinition);


        }
        [Test]
        public void CannotShareXYZDefinition()
        {
            var xyzDefinition = CrossSectionDefinitionXYZ.CreateDefault();
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            var crossSection = new CrossSection(xyzDefinition) { Branch = hydroNetwork.Channels.First() };

            var model = new CrossSectionViewModel(crossSection);
            Assert.IsFalse(model.CanShareDefinition);
        }

    }
}