using System.Windows;
using System.Windows.Controls;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    /// <summary>
    /// Interaction logic for TracerDefinitionsEditorWpf.xaml
    /// </summary>
    public partial class TracerDefinitionsEditorWpf : UserControl
    {
        private IEventedList<string> tracers;

        public TracerDefinitionsEditorWpf()
        {
            InitializeComponent();

            ViewModel.MayRemove = (s) =>
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Removing \"{s}\" tracer definition will remove all boundary conditions and initial tracers. All data will be lost. Continue?",
                    "All data will be lost", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                return result != MessageBoxResult.Cancel;
            };
        }

        /// <summary>
        /// </summary>
        public IEventedList<string> Tracers
        {
            get
            {
                return tracers;
            }
            set
            {
                tracers = value;
                ViewModel.TracersList = tracers;
            }
        }
    }
}