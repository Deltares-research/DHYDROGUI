using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Editors.MeteoDataEditor.ViewModels
{
    [TestFixture]
    public class MeteoStationsListViewModelTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var stations = new EventedList<string>(new[] { "a", "b", "c" });

            // Call
            using (var viewModel = new MeteoStationsListViewModel(stations))
            {
                // Assert
                Assert.Multiple(() =>
                {
                    Assert.That(viewModel, Is.InstanceOf<IMeteoStationsListViewModel>());
                    Assert.That(viewModel, Is.InstanceOf<INotifyPropertyChanged>());

                    Assert.That(viewModel.Stations, Has.Count.EqualTo(3));
                    Assert.That(viewModel.Stations, Has.Exactly(1).Items
                                                       .With.Property(nameof(MeteoStationViewModel.Name))
                                                       .EqualTo("a"));
                    Assert.That(viewModel.Stations, Has.Exactly(1).Items
                                                       .With.Property(nameof(MeteoStationViewModel.Name))
                                                       .EqualTo("b"));
                    Assert.That(viewModel.Stations, Has.Exactly(1).Items
                                                       .With.Property(nameof(MeteoStationViewModel.Name))
                                                       .EqualTo("c"));

                    Assert.That(viewModel.NewStationName, Is.Empty);
                    Assert.That(viewModel.SelectedStations, Is.Empty);

                    Assert.That(viewModel.AddStationCommand, Is.Not.Null);
                    Assert.That(viewModel.RemoveStationsCommand, Is.Not.Null);
                });
            }
        }

        [Test]
        public void Constructor_StationsNull_ThrowsArgumentNullException()
        {
            void Call() => new MeteoStationsListViewModel(null);
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void StationsAdded_UpdatesStationsCollection()
        {
            // Setup
            var stations = new EventedList<string>(new[] { "a", "b", "c" });

            using (var viewModel = new MeteoStationsListViewModel(stations))
            {
                // Call
                stations.Add("d");

                // Assert
                Assert.Multiple(() =>
                {
                    Assert.That(viewModel.Stations, Has.Count.EqualTo(4));
                    Assert.That(viewModel.Stations, Has.Exactly(1).Items
                                                       .With.Property(nameof(MeteoStationViewModel.Name))
                                                       .EqualTo("a"));
                    Assert.That(viewModel.Stations, Has.Exactly(1).Items
                                                       .With.Property(nameof(MeteoStationViewModel.Name))
                                                       .EqualTo("b"));
                    Assert.That(viewModel.Stations, Has.Exactly(1).Items
                                                       .With.Property(nameof(MeteoStationViewModel.Name))
                                                       .EqualTo("c"));
                    Assert.That(viewModel.Stations, Has.Exactly(1).Items
                                                       .With.Property(nameof(MeteoStationViewModel.Name))
                                                       .EqualTo("d"));
                });
            }
        }

        [Test]
        public void StationsRemoved_UpdatesStationsCollection()
        {
            // Setup
            var stations = new EventedList<string>(new[] { "a", "b", "c" });

            using (var viewModel = new MeteoStationsListViewModel(stations))
            {
                // Call
                stations.Remove("b");

                // Assert
                Assert.Multiple(() =>
                {
                    Assert.That(viewModel.Stations, Has.Count.EqualTo(2));
                    Assert.That(viewModel.Stations, Has.Exactly(1).Items
                                                       .With.Property(nameof(MeteoStationViewModel.Name))
                                                       .EqualTo("a"));
                    Assert.That(viewModel.Stations, Has.Exactly(1).Items
                                                       .With.Property(nameof(MeteoStationViewModel.Name))
                                                       .EqualTo("c"));
                });
            }
        }

        [Test]
        public void StationSelected_UpdatesSelectedStations()
        {
            // Setup
            var stations = new EventedList<string>(new[] { "a", "b", "c" });

            using (var viewModel = new MeteoStationsListViewModel(stations))
            {
                Assert.That(viewModel.SelectedStations, Is.Empty);
                MeteoStationViewModel station = viewModel.Stations[1];

                // Call
                station.IsSelected = true;

                // Assert
                Assert.That(viewModel.SelectedStations, Has.Exactly(1).Items);
                Assert.That(viewModel.SelectedStations.First(), Is.SameAs(station));
            }
        }

        [Test]
        public void StationsDeselected_UpdatesSelectedStations()
        {
            // Setup
            var stations = new EventedList<string>(new[] { "a", "b", "c" });

            using (var viewModel = new MeteoStationsListViewModel(stations))
            {
                MeteoStationViewModel station = viewModel.Stations[1];
                station.IsSelected = true;
                Assert.That(viewModel.SelectedStations, Has.Exactly(1).Items);

                // Call
                station.IsSelected = false;

                // Assert
                Assert.That(viewModel.SelectedStations, Is.Empty);
            }
        }

        [Test]
        public void SelectedStationRemoved_IsAlsoRemovedFromSelectedStations()
        {
            // Setup
            var stations = new EventedList<string>(new[] { "a", "b", "c" });

            using (var viewModel = new MeteoStationsListViewModel(stations))
            {
                MeteoStationViewModel station = viewModel.Stations[1];
                station.IsSelected = true;
                Assert.That(viewModel.SelectedStations, Has.Exactly(1).Items);

                // Call
                stations.Remove(station.Name);

                // Assert
                Assert.That(viewModel.SelectedStations, Is.Empty);
            }
        }

        [Test]
        public void RemovedMeteoStationDoesNotAffectSelectedStations()
        {
            // Setup
            var stations = new EventedList<string>(new[] { "a", "b", "c" });

            using (var viewModel = new MeteoStationsListViewModel(stations))
            {
                MeteoStationViewModel station = viewModel.Stations[1];
                stations.Remove(station.Name);
                Assert.That(viewModel.SelectedStations, Is.Empty);

                // Call
                station.IsSelected = true;

                // Assert
                Assert.That(viewModel.SelectedStations, Is.Empty);
            }
        }

        private static IEnumerable<TestCaseData> SetSelectionData()
        {
            TestCaseData ToData(MeteoStationsListViewModel model, 
                                ISet<string> newSelection, 
                                ISet<string> expectedSelection,
                                string name) =>
                new TestCaseData(model, newSelection, expectedSelection).SetName(name);

            var emptyList = new EventedList<string>();

            yield return ToData(new MeteoStationsListViewModel(emptyList), 
                                new SortedSet<string>(), 
                                new SortedSet<string>(), 
                                "Empty ViewModel | Empty Set");
            yield return ToData(new MeteoStationsListViewModel(emptyList),
                                new SortedSet<string>(new[] {"test", "test2"}),
                                new SortedSet<string>(),
                                "Empty ViewModel | Non-empty Set");

            var nonEmptyList = new EventedList<string>(new [] {"a", "b", "c", "d", "e"});
            yield return ToData(new MeteoStationsListViewModel(nonEmptyList),
                                new SortedSet<string>(),
                                new SortedSet<string>(),
                                "Nothing Selected | Empty Set");
            yield return ToData(new MeteoStationsListViewModel(nonEmptyList),
                                new SortedSet<string>(new[] { "1", "2"}),
                                new SortedSet<string>(),
                                "Nothing Selected | Non-existent elements");
            yield return ToData(new MeteoStationsListViewModel(nonEmptyList),
                                new SortedSet<string>(new[] { "b", "d"}),
                                new SortedSet<string>(new[] {"b", "d"}),
                                "Nothing Selected | New Selection");

            MeteoStationsListViewModel WithSelection()
            {
                var viewModelWithSelection = new MeteoStationsListViewModel(nonEmptyList); 
                foreach (MeteoStationViewModel station in viewModelWithSelection.Stations) 
                { 
                    station.IsSelected = true;
                }

                return viewModelWithSelection;
            }

            yield return ToData(WithSelection(),
                                new SortedSet<string>(),
                                new SortedSet<string>(),
                                "Everything Selected | Empty Set");
            yield return ToData(WithSelection(),
                                new SortedSet<string>(new[] {"a", "c", "e", "1", "2"}),
                                new SortedSet<string>(new[] {"a", "c", "e"}),
                                "Everything Selected | New Selection with non-existent");
            yield return ToData(WithSelection(),
                                new SortedSet<string>(new[] {"a", "c", "e"}),
                                new SortedSet<string>(new[] {"a", "c", "e"}),
                                "Everything Selected | New Selection");

        }

        [Test]
        [TestCaseSource(nameof(SetSelectionData))]
        public void SetSelection_ExpectedResults(MeteoStationsListViewModel viewModel,
                                                 ISet<string> newSelection,
                                                 ISet<string> expectedSelection)
        {
            // Call
            viewModel.SetSelection(newSelection);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(viewModel.SelectedStations, Has.Count.EqualTo(expectedSelection.Count));

                foreach (string selectedStation in expectedSelection)
                    Assert.That(viewModel.SelectedStations, Has.Exactly(1).Items
                                                               .With.Property(nameof(MeteoStationViewModel.Name))
                                                               .EqualTo(selectedStation));
            });

            // Clean up
            viewModel.Dispose();
        }

        [Test]
        public void NewStationNames_NotifiesPropertyChanged()
        {
            // Setup
            var nonEmptyList = new EventedList<string>(new [] {"a", "b", "c", "d", "e"});
            var callBacks = new List<(object Sender, PropertyChangedEventArgs e)>();

            void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
                callBacks.Add((sender, e));

            using (var viewModel = new MeteoStationsListViewModel(nonEmptyList))
            {
                viewModel.PropertyChanged += OnPropertyChanged;

                // Call
                viewModel.NewStationName = "station";

                // Assert
                Assert.That(callBacks, Has.Count.EqualTo(1));

                Assert.Multiple(() =>
                {
                    (object s, PropertyChangedEventArgs args) = callBacks.First();
                    Assert.That(s, Is.SameAs(viewModel));
                    Assert.That(args.PropertyName, Is.EqualTo(nameof(viewModel.NewStationName)));
                });

                // Clean up
                viewModel.PropertyChanged -= OnPropertyChanged;
            }
        }

        [Test]
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("      ", false)]
        [TestCase("a", false)]
        [TestCase("f", true)]
        public void AddStationCommand_CanExecute_ExpectedResults(string inputName, bool expectedResult)
        {
            var nonEmptyList = new EventedList<string>(new [] {"a", "b", "c", "d", "e"});

            using (var viewModel = new MeteoStationsListViewModel(nonEmptyList))
            {
                viewModel.NewStationName = inputName;
                Assert.That(viewModel.AddStationCommand.CanExecute(null), Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void AddStationCommand_Execute_ExpectedResults()
        {
            const string inputName = "f";
            var nonEmptyList = new EventedList<string>(new [] {"a", "b", "c", "d", "e"});

            using (var viewModel = new MeteoStationsListViewModel(nonEmptyList))
            {
                viewModel.NewStationName = inputName;
                viewModel.AddStationCommand.Execute(null);

                Assert.Multiple(() =>
                {
                    Assert.That(viewModel.Stations, Has.Exactly(6).Items);
                    Assert.That(viewModel.Stations, Has.Exactly(1).Items
                                                       .With.Property(nameof(MeteoStationViewModel.Name))
                                                       .EqualTo(inputName));
                    Assert.That(nonEmptyList, Has.Exactly(6).Items);
                    Assert.That(nonEmptyList, Has.Member(inputName));
                });
            }
        }

        private static IEnumerable<TestCaseData> RemoveStationCommandCanExecuteData()
        {
            TestCaseData ToData(MeteoStationsListViewModel viewModel, 
                                bool expectedResult,
                                string name) =>
                new TestCaseData(viewModel, expectedResult).SetName(name);

            var emptyList = new EventedList<string>();
            yield return ToData(new MeteoStationsListViewModel(emptyList), false, "Empty view model");

            var nonEmptyList = new EventedList<string>(new [] {"a", "b", "c", "d", "e"});
            yield return ToData(new MeteoStationsListViewModel(nonEmptyList), false, "Non-empty view model with no selection");

            var viewModelWithSelection = new MeteoStationsListViewModel(nonEmptyList); 
            foreach (MeteoStationViewModel station in viewModelWithSelection.Stations) 
            { 
                station.IsSelected = true;
            }

            yield return ToData(viewModelWithSelection, true, "Non-empty view model with selection");
        }

        [Test]
        [TestCaseSource(nameof(RemoveStationCommandCanExecuteData))]
        public void RemoveStationCommand_CanExecute_ExpectedResults(MeteoStationsListViewModel viewModel,
                                                                    bool expectedResult)
        {
            Assert.That(viewModel.RemoveStationsCommand.CanExecute(null), 
                        Is.EqualTo(expectedResult));
        }

        [Test]
        public void RemoveStationCommand_Execute_ExpectedResults()
        {
            var nonEmptyList = new EventedList<string>(new [] {"a", "b", "c", "d", "e"});

            using (var viewModel = new MeteoStationsListViewModel(nonEmptyList))
            {
                var station1 = viewModel.Stations[1];
                var station3 = viewModel.Stations[3];

                station1.IsSelected = true;
                station3.IsSelected = true;

                viewModel.RemoveStationsCommand.Execute(null);

                Assert.Multiple(() =>
                {
                    Assert.That(viewModel.Stations, Has.Exactly(3).Items);
                    Assert.That(viewModel.Stations, Has.Exactly(0).Items
                                                       .With.Property(nameof(MeteoStationViewModel.Name))
                                                       .EqualTo(station1));
                    Assert.That(viewModel.Stations, Has.Exactly(0).Items
                                                       .With.Property(nameof(MeteoStationViewModel.Name))
                                                       .EqualTo(station3));
                    Assert.That(nonEmptyList, Has.Exactly(3).Items);
                    Assert.That(nonEmptyList, Has.No.Member(station1.Name));
                    Assert.That(nonEmptyList, Has.No.Member(station3.Name));
                });
            }
        }
    }
}