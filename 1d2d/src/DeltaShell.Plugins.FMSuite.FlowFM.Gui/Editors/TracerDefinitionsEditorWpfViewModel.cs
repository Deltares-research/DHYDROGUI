using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    /// <summary>
    /// View model for <see cref="TracerDefinitionsEditorWpf"/>
    /// </summary>
    public class TracerDefinitionsEditorWpfViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> tracers;
        private ICommand removeTracerCommand;
        private ICommand addTracerCommand;
        private IEventedList<string> tracersList;
        private string canAddMessage;

        private static readonly string[] DefaultNames = WaterFlowFMModelDefinition.SpatialDataItemNames;

        public TracerDefinitionsEditorWpfViewModel()
        {
            tracers = new ObservableCollection<string>();

            AddTracerCommand = new RelayCommand(
                p => Tracers.Add((string) p), 
                p => IsNameValid((string) p, DefaultNames, tracers));

            RemoveTracerCommand = new RelayCommand(p =>
            {
                var item = (string)p;

                var mayRemove = MayRemove?.Invoke(item) ?? true;
                if (!mayRemove) return;
                
                Tracers.Remove(item);
            });
        }

        /// <summary>
        /// Injected function to cancel a delete
        /// </summary>
        public Func<string, bool> MayRemove { get; set; }

        /// <summary>
        /// Command for adding a tracer
        /// </summary>
        public ICommand AddTracerCommand
        {
            get { return addTracerCommand; }
            set
            {
                addTracerCommand = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Command for removing a tracer
        /// </summary>
        public ICommand RemoveTracerCommand
        {
            get { return removeTracerCommand; }
            set
            {
                removeTracerCommand = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Set of tracers to bind to
        /// </summary>
        public ObservableCollection<string> Tracers
        {
            get { return tracers; }
            set
            {
                if (tracers != null)
                {
                    tracers.CollectionChanged -= TracersCollectionChanged;
                }

                tracers = value;

                if (tracers != null)
                {
                    tracers.CollectionChanged += TracersCollectionChanged;
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Real tracer list to sync with
        /// </summary>
        public IEventedList<string> TracersList
        {
            get { return tracersList; }
            set
            {
                tracersList = value;
                Tracers = new ObservableCollection<string>(tracersList);
            }
        }

        /// <summary>
        /// String containing the message for the add button tooltip
        /// </summary>
        public string CanAddMessage
        {
            get { return canAddMessage; }
            set
            {
                canAddMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property changed event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void TracersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    tracersList.AddRange(e.NewItems.OfType<string>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    e.OldItems.OfType<string>().ForEach(s => tracersList.Remove(s));
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }
        }

        private bool IsNameValid(string name, string[] defaultNames = null, ICollection<string> definedNames = null)
        {
            // check for empty name string
            if (string.IsNullOrEmpty(name))
            {
                CanAddMessage = "No name entered";
                return false;
            }

            var errorMessages = new List<string>();

            // check for default names first
            if (defaultNames != null && defaultNames.Any(n => n.StartsWith(name)))
            {
                errorMessages.Add(string.Format("The name '{0}' cannot be a known default name", name));
            }

            // check if name starts with number
            if (name.Length > 0 && Regex.IsMatch(name, @"^\d"))
            {
                errorMessages.Add(string.Format("The name '{0}' starts with a number", name));
            }

            // check if name is already defined
            if (definedNames != null && definedNames.Contains(name))
            {
                errorMessages.Add(string.Format("The name '{0}' is already defined", name));
            }

            // don't allow white spaces, slashes and back slashes in names
            var regex = new Regex(@"^[^\s\/\\]+$", RegexOptions.Multiline);
            if (!regex.IsMatch(name))
            {
                errorMessages.Add(string.Format("The name '{0}', cannot contain spaces or (back-)slashes", name));
            }

            CanAddMessage = errorMessages.Count > 0 ? string.Join("\n\r", errorMessages) : null;

            return errorMessages.Count == 0;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}