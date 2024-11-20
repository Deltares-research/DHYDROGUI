using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Adapters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Editors.MeteoDataEditor.ViewModels
{
    [TestFixture]
    public class MeteoEditorEvaporationViewModelTest
    {
        private IEvaporationMeteoData MeteoData { get; set; }
        private IMeteoStationsListViewModel StationsViewModel { get; set; }
        private ITableViewMeteoStationSelectionAdapter Adapter { get; set; }
        private ITimeDependentFunctionSplitter FunctionSplitter { get; set; }
        private MeteoEditorEvaporationViewModel ViewModel { get; set; }

        [SetUp]
        public void SetUp()
        {
            MeteoData = Substitute.For<IEvaporationMeteoData>();
            StationsViewModel = Substitute.For<IMeteoStationsListViewModel>();

            var stations = new ObservableCollection<MeteoStationViewModel>();
            StationsViewModel.Stations.Returns(stations);

            var selectedStations = new ObservableCollection<MeteoStationViewModel>();
            StationsViewModel.SelectedStations.Returns(selectedStations);
            
            Adapter = Substitute.For<ITableViewMeteoStationSelectionAdapter>();

            FunctionSplitter = Substitute.For<ITimeDependentFunctionSplitter>();

            ViewModel = new MeteoEditorEvaporationViewModel(MeteoData,
                                                            StationsViewModel,
                                                            Adapter,
                                                            FunctionSplitter,
                                                            DateTime.Now,
                                                            DateTime.Now);
        }
        
        [Test]
        [TestCase(MeteoDataSource.UserDefined, true)]
        [TestCase(MeteoDataSource.GuidelineSewerSystems, false)]
        [TestCase(MeteoDataSource.LongTermAverage, false)]
        public void ShowYears_WhenExpected(MeteoDataSource givenMeteoDataSource, bool expectedShowingOfYears)
        {
            using (ViewModel)
            {
                ViewModel.MeteoDataDistributionType = MeteoDataDistributionType.Global;
                ViewModel.ActiveMeteoDataSource = givenMeteoDataSource;
                Assert.That(ViewModel.ShowYears, Is.EqualTo(expectedShowingOfYears));
            }
        }
        
        [Test]
        [TestCase(MeteoDataDistributionType.Global, MeteoDataDistributionType.PerStation, false)]
        [TestCase(MeteoDataDistributionType.Global, MeteoDataDistributionType.PerFeature, false)]
        [TestCase(MeteoDataDistributionType.PerStation, MeteoDataDistributionType.Global, true)]
        [TestCase(MeteoDataDistributionType.PerStation, MeteoDataDistributionType.PerFeature, false)]
        [TestCase(MeteoDataDistributionType.PerFeature, MeteoDataDistributionType.Global, true)]
        [TestCase(MeteoDataDistributionType.PerFeature, MeteoDataDistributionType.PerStation, false)]
        public void WhenMeteoDataDistributionTypeSwitched_ThenCanEditActiveMeteoDataSourceChangedAsExpected_ActiveMeteoDataSourceSetToUserDefined(MeteoDataDistributionType startType, MeteoDataDistributionType changeToType, bool expectedEditMode)
        {
            //Arrange
            using (ViewModel)
            {

                ViewModel.ActiveMeteoDataSource = MeteoDataSource.GuidelineSewerSystems;
                ViewModel.MeteoDataDistributionType = startType;
                
                //Act
                ViewModel.MeteoDataDistributionType = changeToType;

                //Assert
                Assert.That(ViewModel.CanEditActiveMeteoDataSource, Is.EqualTo(expectedEditMode));
                Assert.That(ViewModel.ActiveMeteoDataSource, Is.EqualTo(MeteoDataSource.UserDefined));
            }
        }
        
        [Test]
        public void ActiveMeteoDataSource_NotifiesPropertyChanged()
        {
            // Setup
            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            ViewModel.PropertyChanged += OnPropertyChanged;
            
            // Call
            ViewModel.ActiveMeteoDataSource = MeteoDataSource.GuidelineSewerSystems;

            // Assert
            Assert.That(callBacks, Has.Count.EqualTo(3));

            Assert.Multiple(() =>
            {
                AssertHasCallback(callBacks, nameof(ViewModel.ActiveMeteoDataSource), ViewModel);
                AssertHasCallback(callBacks, nameof(ViewModel.TimeSeries), ViewModel);
                AssertHasCallback(callBacks, nameof(ViewModel.ShowYears), ViewModel);
            });

            // Clean up
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }
        
        [Test]
        public void MeteoDataDistributionType_UpdatesMeteoDataAndNotifiesPropertyChanged()
        {
            // Setup
            MeteoData.DataDistributionType = MeteoDataDistributionType.Global;

            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            ViewModel.PropertyChanged += OnPropertyChanged;
            
            MeteoData.ClearReceivedCalls();

            // Call
            ViewModel.MeteoDataDistributionType = MeteoDataDistributionType.PerFeature;

            // Assert
            Assert.That(callBacks, Has.Count.EqualTo(2));

            Assert.Multiple(() =>
            {
                AssertHasCallback(callBacks, nameof(ViewModel.MeteoDataDistributionType), ViewModel);
            });

            MeteoData.Received(1).DataDistributionType = MeteoDataDistributionType.PerFeature;

            // Clean up
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }
        
        private static void AssertHasCallback(IEnumerable<(object Sender, PropertyChangedEventArgs e)> callBacks, 
                                              string name, 
                                              object expectedSender = null)
        {
            (object Sender, PropertyChangedEventArgs e) callBack = callBacks.FirstOrDefault(x => x.e.PropertyName == name);
            Assert.That(callBack, Is.Not.Null);

            if (expectedSender != null) Assert.That(callBack.Sender, Is.SameAs(expectedSender));
        }
    }
}