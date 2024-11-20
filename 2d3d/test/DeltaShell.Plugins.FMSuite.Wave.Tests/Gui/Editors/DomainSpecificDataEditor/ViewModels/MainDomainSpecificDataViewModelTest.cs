using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    [TestFixture]
    public class MainDomainSpecificDataViewModelTest
    {
        [Test]
        public void ConstructorShouldCreateCorrectSubViewModels()
        {
            // Arrange and Act
            CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);

            // Assert
            ObservableCollection<DomainSpecificSettingsViewModel> viewModelsList = mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList;
            Assert.AreEqual(2, viewModelsList.Count);
            Assert.AreEqual("test", viewModelsList[0].DomainName);
            Assert.AreEqual("subdomain", viewModelsList[1].DomainName);
        }

        [Test]
        public void ConstructorShouldSelectTheFirstSubViewModel()
        {
            // Arrange and Act
            CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);

            // Assert
            Assert.AreEqual(mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList[0], mainDomainSpecificDataViewModel.SelectedViewModel);
        }

        [Test]
        public void ConstructorCanHandleNullArgument()
        {
            // Arrange and Act
            var overviewDomainSpecificDataViewModel = new MainDomainSpecificDataViewModel(null);

            //Assert
            Assert.AreEqual(0, overviewDomainSpecificDataViewModel.DomainSpecificDataViewModelsList.Count);
            Assert.IsNull(overviewDomainSpecificDataViewModel.SelectedViewModel);
        }

        [Test]
        public void GivenAMainViewModel_WhenAddingASubDomainToTheRootDomain_ThenTheViewModelListUpdates()
        {
            // Arrange
            WaveDomainData rootWaveDomainData = CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);

            // Act
            AddSubDomainTo(rootWaveDomainData);

            // Assert
            ObservableCollection<DomainSpecificSettingsViewModel> viewModelsList = mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList;
            Assert.AreEqual(3, viewModelsList.Count);
            Assert.AreEqual("test", viewModelsList[0].DomainName);
            Assert.AreEqual("subdomain", viewModelsList[1].DomainName);
            Assert.AreEqual("subdomain", viewModelsList[2].DomainName);
        }

        [Test]
        public void GivenAMainViewModel_WhenAddingASubDomainToASubDomainOfTheRootDomain_ThenTheViewModelListUpdates()
        {
            // Arrange
            var waveDomainData = new WaveDomainData("test");
            WaveDomainData subDomain = AddSubDomainTo(waveDomainData);

            var mainDomainSpecificDataViewModel = new MainDomainSpecificDataViewModel(waveDomainData);

            // Act
            AddSubDomainTo(subDomain);

            // Assert
            ObservableCollection<DomainSpecificSettingsViewModel> viewModelsList = mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList;
            Assert.AreEqual(3, viewModelsList.Count);
            Assert.AreEqual("test", viewModelsList[0].DomainName);
            Assert.AreEqual("subdomain", viewModelsList[1].DomainName);
            Assert.AreEqual("subdomain", viewModelsList[2].DomainName);
        }

        [Test]
        public void GivenAMainViewModel_WhenAddingANewExteriorDomain_ThenTheViewModelListUpdates()
        {
            // Arrange
            WaveDomainData rootWaveDomainData = CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);

            // Act
            AddExteriorWaveDomainTo(rootWaveDomainData);

            // Assert
            ObservableCollection<DomainSpecificSettingsViewModel> viewModelsList = mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList;
            Assert.AreEqual(3, viewModelsList.Count);
            Assert.AreEqual("exterior", viewModelsList[0].DomainName);
            Assert.AreEqual("test", viewModelsList[1].DomainName);
            Assert.AreEqual("subdomain", viewModelsList[2].DomainName);
        }

        [Test]
        public void GivenAMainViewModel_WhenDeletingASubDomain_ThenTheViewModelListUpdates()
        {
            // Arrange
            WaveDomainData rootWaveDomainData = CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);

            // Act
            rootWaveDomainData.SubDomains.Clear();

            // Assert
            ObservableCollection<DomainSpecificSettingsViewModel> viewModelsList = mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList;
            Assert.AreEqual(1, viewModelsList.Count);
            Assert.AreEqual("test", viewModelsList[0].DomainName);
        }

        [Test]
        public void GivenAMainViewModel_WhenDeletingTheExteriorDomain_ThenTheViewModelListUpdates()
        {
            // Arrange
            WaveDomainData rootWaveDomainData = CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);

            // Act
            rootWaveDomainData.SubDomains[0].SuperDomain = null;

            // Assert
            ObservableCollection<DomainSpecificSettingsViewModel> viewModelsList = mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList;
            Assert.AreEqual(1, viewModelsList.Count);
            Assert.AreEqual("subdomain", viewModelsList[0].DomainName);
        }

        [Test]
        public void GivenAMainViewModel_WhenAddingASubDomain_ThenTheSameViewModelIsStillSelected()
        {
            // Arrange
            WaveDomainData rootWaveDomainData = CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);
            mainDomainSpecificDataViewModel.SelectedViewModel =
                mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList[1];

            // Act
            AddSubDomainTo(rootWaveDomainData);

            // Assert
            Assert.AreEqual(mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList[1], mainDomainSpecificDataViewModel.SelectedViewModel);
        }

        [Test]
        public void GivenAMainViewModel_WhenAddingAnExteriorDomain_ThenTheSameViewModelIsStillSelected()
        {
            // Arrange
            WaveDomainData rootWaveDomainData =
                CreateMainDomainSpecificDataViewModelWithOneSubDomain(
                    out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);
            mainDomainSpecificDataViewModel.SelectedViewModel =
                mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList[1];

            // Act
            AddExteriorWaveDomainTo(rootWaveDomainData);

            // Assert
            Assert.AreEqual(mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList[2], mainDomainSpecificDataViewModel.SelectedViewModel);
        }

        [Test]
        public void GivenAMainViewModel_WhenDeletingASelectedSubDomain_ThenTheRootViewModelShouldBeSelected()
        {
            // Arrange
            WaveDomainData rootWaveDomainData =
                CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);
            mainDomainSpecificDataViewModel.SelectedViewModel =
                mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList[1];

            // Act
            rootWaveDomainData.SubDomains.Clear();

            // Assert
            Assert.AreEqual(mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList[0], mainDomainSpecificDataViewModel.SelectedViewModel);
        }

        [Test]
        public void GivenAMainViewModel_WhenDeletingTheSelectedExteriorDomain_ThenTheRootViewModelShouldBeSelected()
        {
            // Arrange
            WaveDomainData rootWaveDomainData = CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);

            // Act
            rootWaveDomainData.SubDomains[0].SuperDomain = null;

            // Assert
            Assert.AreEqual(mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList[0], mainDomainSpecificDataViewModel.SelectedViewModel);
            Assert.AreEqual("subdomain", mainDomainSpecificDataViewModel.SelectedViewModel.DomainName);
        }

        [Test]
        public void GivenAMainViewModelAfterAddingAnNewExteriorDomain_WhenAddingASubDomain_ThenTheViewModelStillUpdates()
        {
            // Arrange
            WaveDomainData rootWaveDomainData = CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);
            AddExteriorWaveDomainTo(rootWaveDomainData);

            // Act
            AddSubDomainTo(rootWaveDomainData);

            ObservableCollection<DomainSpecificSettingsViewModel> viewModelsListAfter = mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList;
            Assert.AreEqual(4, viewModelsListAfter.Count);
            Assert.AreEqual("exterior", viewModelsListAfter[0].DomainName);
            Assert.AreEqual("test", viewModelsListAfter[1].DomainName);
            Assert.AreEqual("subdomain", viewModelsListAfter[2].DomainName);
            Assert.AreEqual("subdomain", viewModelsListAfter[3].DomainName);
        }

        [Test]
        public void GivenAMainViewModelAfterAddingANewExteriorDomain_WhenAddingAnNewExteriorDomainAgain_ThenTheViewModelStillUpdates()
        {
            // Arrange
            WaveDomainData rootWaveDomainData = CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);
            WaveDomainData newExteriorDomain1 = AddExteriorWaveDomainTo(rootWaveDomainData);

            // Act
            var newExteriorDomain2 = new WaveDomainData("exterior2") {SubDomains = {newExteriorDomain1}};
            newExteriorDomain1.SuperDomain = newExteriorDomain2;

            ObservableCollection<DomainSpecificSettingsViewModel> viewModelsListAfter = mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList;
            Assert.AreEqual(4, viewModelsListAfter.Count);
            Assert.AreEqual("exterior2", viewModelsListAfter[0].DomainName);
            Assert.AreEqual("exterior", viewModelsListAfter[1].DomainName);
            Assert.AreEqual("test", viewModelsListAfter[2].DomainName);
            Assert.AreEqual("subdomain", viewModelsListAfter[3].DomainName);
        }

        [Test]
        public void GivenAMainViewModelAfterDeletingTheExteriorDomain_WhenAddingASubDomain_ThenTheViewModelStillUpdates()
        {
            // Arrange
            WaveDomainData rootWaveDomainData = CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);
            IWaveDomainData newExteriorDomain1 = rootWaveDomainData.SubDomains[0];
            newExteriorDomain1.SuperDomain = null;

            // Act
            AddSubDomainTo(newExteriorDomain1);

            ObservableCollection<DomainSpecificSettingsViewModel> viewModelsListAfter = mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList;
            Assert.AreEqual(2, viewModelsListAfter.Count);
            Assert.AreEqual("subdomain", viewModelsListAfter[0].DomainName);
            Assert.AreEqual("subdomain", viewModelsListAfter[1].DomainName);
        }

        [Test]
        public void GivenAMainViewModelAfterDeletingTheExteriorDomain_WhenAddingAnNewExteriorDomain_ThenTheViewModelStillUpdates()
        {
            // Arrange
            WaveDomainData rootWaveDomainData = CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel);
            IWaveDomainData newExteriorDomain1 = rootWaveDomainData.SubDomains[0];
            newExteriorDomain1.SuperDomain = null;

            // Act
            var newExteriorDomain2 = new WaveDomainData("exterior2") {SubDomains = {newExteriorDomain1}};
            newExteriorDomain1.SuperDomain = newExteriorDomain2;

            ObservableCollection<DomainSpecificSettingsViewModel> viewModelsListAfter = mainDomainSpecificDataViewModel.DomainSpecificDataViewModelsList;
            Assert.AreEqual(2, viewModelsListAfter.Count);
            Assert.AreEqual("exterior2", viewModelsListAfter[0].DomainName);
            Assert.AreEqual("subdomain", viewModelsListAfter[1].DomainName);
        }

        [Test]
        public void Dispose_UnsubscribesDomains()
        {
            // Setup
            INotifyPropertyChange domain = Substitute.For<INotifyPropertyChange, IWaveDomainData>();
            var rootDomain = (IWaveDomainData) domain;
            var subDomains = Substitute.For<IEventedList<IWaveDomainData>>();
            rootDomain.SubDomains.Returns(subDomains);

            var viewModel = new MainDomainSpecificDataViewModel(rootDomain);

            // Call
            viewModel.Dispose();

            // Assert
            domain.ReceivedWithAnyArgs().PropertyChanged -= Arg.Any<PropertyChangedEventHandler>();
            subDomains.ReceivedWithAnyArgs().CollectionChanged -= Arg.Any<NotifyCollectionChangedEventHandler>();
        }

        private static WaveDomainData CreateMainDomainSpecificDataViewModelWithOneSubDomain(out MainDomainSpecificDataViewModel mainDomainSpecificDataViewModel)
        {
            var rootWaveDomainData = new WaveDomainData("test");
            AddSubDomainTo(rootWaveDomainData);

            mainDomainSpecificDataViewModel = new MainDomainSpecificDataViewModel(rootWaveDomainData);
            return rootWaveDomainData;
        }

        private static WaveDomainData AddSubDomainTo(IWaveDomainData waveDomainData)
        {
            var subWaveDomainData = new WaveDomainData("subdomain") {SuperDomain = waveDomainData};
            waveDomainData.SubDomains.Add(subWaveDomainData);
            return subWaveDomainData;
        }

        private static WaveDomainData AddExteriorWaveDomainTo(WaveDomainData waveDomainData)
        {
            var exteriorWaveDomainData = new WaveDomainData("exterior") {SubDomains = {waveDomainData}};
            waveDomainData.SuperDomain = exteriorWaveDomainData;
            return exteriorWaveDomainData;
        }
    }
}