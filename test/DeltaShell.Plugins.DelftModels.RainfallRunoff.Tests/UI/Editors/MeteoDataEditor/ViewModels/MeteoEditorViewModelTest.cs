using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Adapters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Editors.MeteoDataEditor.ViewModels
{
    [TestFixture]
    public class MeteoEditorViewModelTest
    {
        private IMeteoData MeteoData { get; set; }
        private IMeteoStationsListViewModel StationsViewModel { get; set; }
        private ITableViewMeteoStationSelectionAdapter Adapter { get; set; }
        private ITimeDependentFunctionSplitter FunctionSplitter { get; set; }
        private MeteoEditorViewModel ViewModel { get; set; }

        private const string catchmentName = "Catchment";

        [SetUp]
        public void SetUp()
        {
            MeteoData = Substitute.For<IMeteoData>();
            StationsViewModel = Substitute.For<IMeteoStationsListViewModel>();

            var stations = new ObservableCollection<MeteoStationViewModel>();
            StationsViewModel.Stations.Returns(stations);

            var selectedStations = new ObservableCollection<MeteoStationViewModel>();
            StationsViewModel.SelectedStations.Returns(selectedStations);
            
            Adapter = Substitute.For<ITableViewMeteoStationSelectionAdapter>();

            FunctionSplitter = Substitute.For<ITimeDependentFunctionSplitter>();

            ViewModel = new MeteoEditorViewModel(MeteoData,
                                                 StationsViewModel,
                                                 Adapter,
                                                 FunctionSplitter,
                                                 DateTime.Now,
                                                 DateTime.Now);
        }

        [TearDown]
        public void TearDown()
        {
            ViewModel.Dispose();
        }


        [Test]
        public void Constructor_ExpectedResults()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ViewModel, Is.InstanceOf<IMeteoEditorViewModel>());
                Assert.That(ViewModel, Is.InstanceOf<INotifyPropertyChanged>());

                Assert.That(ViewModel.GenerateTimeSeriesCommand, Is.Not.Null);
                Assert.That(ViewModel.StationsViewModel, Is.SameAs(StationsViewModel));
                Assert.That(ViewModel.CreateBindingList, Is.Not.Null);
            });
        }

        private static IEnumerable<TestCaseData> ConstructorArgumentNullData()
        {
            var meteoData = Substitute.For<IMeteoData>();
            var stationsViewModel = Substitute.For<IMeteoStationsListViewModel>();
            MeteoDataSource[] sources = { MeteoDataSource.UserDefined };
            var adapter = Substitute.For<ITableViewMeteoStationSelectionAdapter>();
            var functionSplitter = Substitute.For<ITimeDependentFunctionSplitter>();

            yield return new TestCaseData(null, stationsViewModel, adapter, functionSplitter).SetName("meteoData");
            yield return new TestCaseData(meteoData, null, adapter, functionSplitter).SetName("meteoStationsListViewModel");
            yield return new TestCaseData(meteoData, stationsViewModel, null, functionSplitter).SetName("tableSelectionAdapter");
            yield return new TestCaseData(meteoData, stationsViewModel, adapter, null).SetName("functionSplitter");
        }

        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullData))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(IMeteoData meteoData,
                                                                         IMeteoStationsListViewModel stationsViewModel,
                                                                         ITableViewMeteoStationSelectionAdapter adapter,
                                                                         ITimeDependentFunctionSplitter functionSplitter)
        {
            void Call() => new MeteoEditorViewModel(meteoData,
                                                    stationsViewModel,
                                                    adapter, 
                                                    functionSplitter,
                                                    DateTime.Now,
                                                    DateTime.Now);
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void StationsChanged_PropagatesCorrectNotifyChanges()
        {
            // Setup
            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            ViewModel.PropertyChanged += OnPropertyChanged;

            // Call
            StationsViewModel.Stations.Add(new MeteoStationViewModel());

            // Assert
            Assert.That(callBacks, Has.Count.EqualTo(2));

            Assert.Multiple(() =>
            {
                AssertHasCallback(callBacks, nameof(MeteoEditorViewModel.TimeSeries), ViewModel);
                AssertHasCallback(callBacks, nameof(MeteoEditorViewModel.ShowNoStationsWarning), ViewModel);
            });

            // Clean up
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        [Test]
        [TestCase(MeteoDataDistributionType.PerFeature)]
        [TestCase(MeteoDataDistributionType.Global)]
        public void SelectedStationsChanged_MeteoDataDistributionTypeNotPerStation_DoesNotAffectTable(MeteoDataDistributionType distributionType)
        {
            // Setup
            MeteoData.DataDistributionType.Returns(distributionType);

            // Call
            StationsViewModel.SelectedStations.Add(new MeteoStationViewModel());

            // Assert
            Assert.That(Adapter.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void SelectedStationsChanged_TableViewAdapterSetsSelection()
        {
            // Setup
            string[] selectedStations = { "a", "b", "c", "d" };

            foreach (string station in selectedStations)
                StationsViewModel.SelectedStations.Add(new MeteoStationViewModel() {
                    Name = station
                });
            MeteoData.DataDistributionType.Returns(MeteoDataDistributionType.PerStation);

            Adapter.ClearReceivedCalls();

            // Call
            StationsViewModel.SelectedStations.Add(new MeteoStationViewModel { Name = "e" });

            // Assert
            string[] expectedStations = { "a", "b", "c", "d", "e" };
            Adapter.Received(1).SetSelection(Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(expectedStations)));
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

        [Test]
        public void MeteoDataDistributionType_CurrentValue_DoesNotUpdateAndNotifyPropertyChanged()
        {
            // Setup
            MeteoData.DataDistributionType = MeteoDataDistributionType.Global;

            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            ViewModel.PropertyChanged += OnPropertyChanged;
            
            MeteoData.ClearReceivedCalls();

            // Call
            ViewModel.MeteoDataDistributionType = MeteoDataDistributionType.Global;

            // Assert
            Assert.That(callBacks, Is.Empty);
            MeteoData.DidNotReceiveWithAnyArgs().DataDistributionType = MeteoDataDistributionType.Global;

            // Clean up
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        [Test]
        public void TimeSeries_Global_ReturnsExpectedResult()
        {
            // Setup
            var function = Substitute.For<IFunction>();
            MeteoData.Data.Returns(function);
            MeteoData.DataDistributionType = MeteoDataDistributionType.Global;
            
            using (var viewModel = new MeteoEditorViewModel(MeteoData,
                                                            StationsViewModel,
                                                            Adapter,
                                                            FunctionSplitter,
                                                            DateTime.Now,
                                                            DateTime.Now))
            {

                // Call
                IFunction[] result = viewModel.TimeSeries;

                // Assert
                Assert.That(result, Is.EqualTo(new[]
                {
                    function
                }));
            }
        }

        [Test]
        [TestCase(MeteoDataDistributionType.Global, MeteoDataDistributionType.PerStation)]
        [TestCase(MeteoDataDistributionType.Global, MeteoDataDistributionType.PerFeature)]
        [TestCase(MeteoDataDistributionType.PerStation, MeteoDataDistributionType.Global)]
        [TestCase(MeteoDataDistributionType.PerStation, MeteoDataDistributionType.PerFeature)]
        [TestCase(MeteoDataDistributionType.PerFeature, MeteoDataDistributionType.Global)]
        [TestCase(MeteoDataDistributionType.PerFeature, MeteoDataDistributionType.PerStation)]
        public void WhenMeteoDataDistributionTypeSwitched_CanEditActiveMeteoDataSourceAlwaysFalse_ActiveMeteoDataSourceSetToUserDefined(MeteoDataDistributionType startType, MeteoDataDistributionType changeToType)
        {
            //Arrange
            using (ViewModel)
            {
                ViewModel.MeteoDataDistributionType = startType;
                
                //Act
                ViewModel.MeteoDataDistributionType = changeToType;

                //Assert
                Assert.That(ViewModel.CanEditActiveMeteoDataSource, Is.False);
                Assert.That(ViewModel.ActiveMeteoDataSource, Is.EqualTo(MeteoDataSource.UserDefined));
            }
        }

        [Test]
        [TestCase(MeteoDataDistributionType.PerFeature)]
        [TestCase(MeteoDataDistributionType.PerStation)]
        public void TimeSeries_NotGlobal_ReturnsExpectedResult(MeteoDataDistributionType distributionType)
        {
            // Setup
            var function = Substitute.For<IFunction>();
            MeteoData.Data.Returns(function);
            MeteoData.DataDistributionType = distributionType;

            var expectedResults = new[]
            {
                Substitute.For<IFunction>(),
                Substitute.For<IFunction>(),
                Substitute.For<IFunction>(),
            };
            FunctionSplitter.SplitIntoFunctionsPerArgumentValue(function)
                            .Returns(expectedResults);

            using (var viewModel = new MeteoEditorViewModel(MeteoData,
                                                            StationsViewModel,
                                                            Adapter,
                                                            FunctionSplitter,
                                                            DateTime.Now,
                                                            DateTime.Now))
            {

                // Call
                IFunction[] result = viewModel.TimeSeries;

                // Assert
                Assert.That(result, Is.EqualTo(expectedResults));
            }
        }

        [Test]
        public void TableSelectionChanged_DistributionTypeNotStations_DoesNothing()
        {
            // Setup
            MeteoData.DataDistributionType = MeteoDataDistributionType.Global;
            
            // Call
            ViewModel.TableSelectionChangedEventHandler.Invoke(null, null);

            // Assert
            StationsViewModel.DidNotReceiveWithAnyArgs().SetSelection(null);
        }

        [Test]
        public void TableSelectionChanged_SynchronizesStationsViewModel()
        {
            // Setup
            MeteoData.DataDistributionType = MeteoDataDistributionType.PerStation;

            ITableViewColumn CreateColumn(int i)
            {
                var column = Substitute.For<ITableViewColumn>();
                column.Name.Returns(i.ToString());
                return column;
            }

            ITableViewColumn[] columns = 
                Enumerable.Range(0, 5)
                          .Select(CreateColumn)
                          .ToArray();

            TableViewCell[] cells = columns.Select(c => new TableViewCell(5, c))
                                           .ToArray();

            var args = new TableSelectionChangedEventArgs(cells);

            // Call
            ViewModel.TableSelectionChangedEventHandler.Invoke(null, args);

            // Assert
            StationsViewModel.Received(1).SetSelection(Arg.Is<ISet<string>>(x => x.SetEquals(columns.Select(c => c.Name))));
        }

        [Test]
        public void CatchmentsChanged_PropagatesCorrectNotifyChanges()
        {
            // Setup
            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            var args = EventArgs.Empty;

            ViewModel.PropertyChanged += OnPropertyChanged;
            MeteoData.DataDistributionType = MeteoDataDistributionType.PerFeature;

            // Call
            MeteoData.CatchmentsChanged += Raise.EventWith(MeteoData, args);

            // Assert
            Assert.That(callBacks, Has.Count.EqualTo(3));

            Assert.Multiple(() =>
            {
                AssertHasCallback(callBacks, nameof(ViewModel.TimeSeries));
                AssertHasCallback(callBacks, nameof(ViewModel.ShowNoFeaturesWarning));
                AssertHasCallback(callBacks, nameof(ViewModel.ShowYears));
            });

            // Cleanup
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        [Test]
        public void CatchmentsChanged_MeteoDataDistributionTypeNotFeature_DoesNothing()
        {
            // Setup
            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            var args = EventArgs.Empty;

            ViewModel.PropertyChanged += OnPropertyChanged;
            MeteoData.DataDistributionType = MeteoDataDistributionType.Global;

            // Call
            MeteoData.CatchmentsChanged += Raise.EventWith(MeteoData, args);

            // Assert
            Assert.That(callBacks, Is.Empty);

            // Cleanup
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        [Test]
        public void CatchmentsChanged_SenderNotMeteoData_DoesNothing()
        {
            // Setup
            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            var args = EventArgs.Empty;

            ViewModel.PropertyChanged += OnPropertyChanged;
            MeteoData.DataDistributionType = MeteoDataDistributionType.PerFeature;

            // Call
            MeteoData.CatchmentsChanged += Raise.EventWith(new object(), args);

            // Assert
            Assert.That(callBacks, Is.Empty);

            // Cleanup
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        [Test]
        public void MeteoDataChanged_DataDistributionType_PropagatesCorrectNotifyChanges()
        {
            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            var eventArgs = new PropertyChangedEventArgs(nameof(IMeteoData.DataDistributionType));
            ViewModel.PropertyChanged += OnPropertyChanged;

            // Call
            MeteoData.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(MeteoData, eventArgs);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(callBacks, Has.Exactly(5).Items);
                AssertHasCallback(callBacks, nameof(ViewModel.MeteoDataDistributionType), ViewModel);
                AssertHasCallback(callBacks, nameof(ViewModel.TimeSeries), ViewModel);
                AssertHasCallback(callBacks, nameof(ViewModel.CreateBindingList), ViewModel);
                AssertHasCallback(callBacks, nameof(ViewModel.ShowNoFeaturesWarning), ViewModel);
                AssertHasCallback(callBacks, nameof(ViewModel.ShowNoStationsWarning), ViewModel);
            });

            // Cleanup
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        [Test]
        public void MeteoDataChanged_Data_PropagatesCorrectNotifyChanges()
        {
            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            var eventArgs = new PropertyChangedEventArgs(nameof(IMeteoData.Data));
            ViewModel.PropertyChanged += OnPropertyChanged;

            // Call
            MeteoData.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(MeteoData, eventArgs);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(callBacks, Has.Exactly(2).Items);
                AssertHasCallback(callBacks, nameof(ViewModel.TimeSeries), ViewModel);
                AssertHasCallback(callBacks, nameof(ViewModel.CreateBindingList), ViewModel);
            });

            // Cleanup
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        [Test]
        public void MeteoDataChanged_SenderNotMeteoData_DoesNothing()
        {
            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            var eventArgs = new PropertyChangedEventArgs(nameof(IMeteoData.Data));
            ViewModel.PropertyChanged += OnPropertyChanged;

            // Call
            MeteoData.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(new object(), eventArgs);

            // Assert
            Assert.That(callBacks, Is.Empty);

            // Cleanup
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        [Test]
        public void MeteoDataChanged_OtherProperty_DoesNothing()
        {
            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            var eventArgs = new PropertyChangedEventArgs(nameof(IMeteoData.Name));
            ViewModel.PropertyChanged += OnPropertyChanged;

            // Call
            MeteoData.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(MeteoData, eventArgs);

            // Assert
            Assert.That(callBacks, Is.Empty);

            // Cleanup
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        [Test]
        public void Dispose_DisposeStationsViewModel()
        {
            // Call
            ViewModel.Dispose();

            // Assert
            StationsViewModel.Received(1).Dispose();
        }

        private static IEnumerable<TestCaseData> ShowNoStationsWarningData()
        {
            TestCaseData ToData(MeteoDataDistributionType distributionType,
                                ICollection<string> stations,
                                bool expectedResults,
                                string name) => new TestCaseData(distributionType, stations, expectedResults).SetName(name);

            string[] emptyStations = Array.Empty<string>();
            string[] nonEmptyStations = {"a", "b", "c"};

            yield return ToData(MeteoDataDistributionType.Global, nonEmptyStations, false, "Global");
            yield return ToData(MeteoDataDistributionType.PerFeature, nonEmptyStations, false, "PerFeature");
            yield return ToData(MeteoDataDistributionType.PerStation, emptyStations, true, "Empty stations");
            yield return ToData(MeteoDataDistributionType.PerStation, nonEmptyStations, false, "Valid stations");
        }

        [Test]
        [TestCaseSource(nameof(ShowNoStationsWarningData))]
        public void ShowNoStationsWarning_ExpectedResult(MeteoDataDistributionType distributionType,
                                                         ICollection<string> stations,
                                                         bool expectedResult)
        {
            // Setup
            MeteoData.DataDistributionType.Returns(distributionType);

            foreach (string station in stations)
                StationsViewModel.Stations.Add(new MeteoStationViewModel { Name = station });

            // Call
            var result = ViewModel.ShowNoStationsWarning;

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private static IEnumerable<TestCaseData> ShowNoFeaturesWarningData()
        {
            TestCaseData ToData(MeteoDataDistributionType distributionType,
                                IFunction[] functions,
                                bool expectedResults,
                                string name) => new TestCaseData(distributionType, functions, expectedResults).SetName(name);

            IFunction[] emptyFunctions = Array.Empty<IFunction>();
            IFunction[] nonEmptyFunctions =
            {
                Substitute.For<IFunction>(),
                Substitute.For<IFunction>(),
                Substitute.For<IFunction>(),
            };

            yield return ToData(MeteoDataDistributionType.Global, emptyFunctions, false, "Global Distribution Type");
            yield return ToData(MeteoDataDistributionType.PerFeature, nonEmptyFunctions, false, "With Features");
            yield return ToData(MeteoDataDistributionType.PerFeature, emptyFunctions, true, "No Features");

        }

        [Test]
        [TestCaseSource(nameof(ShowNoFeaturesWarningData))]
        public void ShowNoFeaturesWarning_ExpectedResult(MeteoDataDistributionType distributionType,
                                                         IFunction[] functions,
                                                         bool expectedResult)
        {
            // Setup
            var function = Substitute.For<IFunction>();
            SetupMeteoDataSubstitute(function, functions.Length);
            MeteoData.Data.Returns(function);
            MeteoData.DataDistributionType = distributionType;

            FunctionSplitter.SplitIntoFunctionsPerArgumentValue(function)
                            .Returns(functions);

            using (var viewModel = new MeteoEditorViewModel(MeteoData,
                                                            StationsViewModel,
                                                            Adapter,
                                                            FunctionSplitter,
                                                            DateTime.Now,
                                                            DateTime.Now))
            {

                // Call
                bool result = viewModel.ShowNoFeaturesWarning;

                // Assert
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void ShowYearsTrue_ForNonEvaporation()
        {
            using (ViewModel)
            {
                Assert.That(ViewModel.ShowYears, Is.True);
            }
        }
        
        [Test]
        public void PossibleMeteoDataSource_AlwaysUserDefined_ForNonEvaporation()
        {
            using (ViewModel)
            {
                MeteoDataSource[] actualMeteoDataSources = ViewModel.PossibleMeteoDataSources;
                Assert.That(actualMeteoDataSources.Length, Is.EqualTo(1));
                Assert.That(actualMeteoDataSources.First(), Is.EqualTo(MeteoDataSource.UserDefined));
            }
        }
        
        [Test]
        public void WritingActiveMeteoDataSource_ThrowsNotSupportedException()
        {
            using (ViewModel)
            {
                void Call() => ViewModel.ActiveMeteoDataSource = MeteoDataSource.UserDefined;
                Assert.Throws<NotSupportedException>(Call);
            }
        }

        private static void SetupMeteoDataSubstitute(IFunction function, int amountOfCatchments)
        {
            var catchment = Substitute.For<IVariable>();
            catchment.Name.Returns(catchmentName);
            catchment.Values.Count.Returns(amountOfCatchments);
            var eventedList = new EventedList<IVariable> {catchment};
            function.Arguments.Returns(eventedList);
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