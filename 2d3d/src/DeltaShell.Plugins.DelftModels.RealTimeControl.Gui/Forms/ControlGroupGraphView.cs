using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    public partial class ControlGroupGraphView : UserControl, IContextAwareView
    {
        private IGui gui;
        private ControlGroup controlGroup;
        private IRealTimeControlModel model;

        public ControlGroupGraphView()
        {
            InitializeComponent();
            Text = @"Control Group Editor";
        }

        public ControlGroupEditor ControlGroupEditor => controlGroupEditor;

        public IGui Gui
        {
            get => gui;
            set
            {
                gui = value;
                controlGroupEditor.Gui = value;
            }
        } // selection and opening views

        public IRealTimeControlModel Model
        {
            get => model;
            set
            {
                model = value;
                controlGroupEditor.Model = Model;
            }
        }

        public object Data
        {
            get => controlGroup;
            set
            {
                UnsubscribeEventListeners();
                UnbindViews();

                controlGroup = value as ControlGroup;

                if (controlGroup == null)
                {
                    return;
                }

                BindViews();
                SubscribeEventListeners();
            }
        }

        public Image Image
        {
            get => null;
            set {}
        }

        public IViewContext ViewContext
        {
            get => controlGroupEditor.ViewContext;
            set => controlGroupEditor.ViewContext = value;
        }

        public ViewInfo ViewInfo { get; set; }

        public void EnsureVisible(object item)
        {
            // Nothing to be done.
        }

        private void SubscribeEventListeners()
        {
            if (null != controlGroup)
            {
                ((INotifyPropertyChanged) controlGroup).PropertyChanged += ControlGroupPropertyChanged;
            }
        }

        private void UnsubscribeEventListeners()
        {
            if (null != controlGroup)
            {
                ((INotifyPropertyChanged) controlGroup).PropertyChanged -= ControlGroupPropertyChanged;
            }
        }

        private void ControlGroupPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name" && sender is ControlGroup sendingControlGroup)
            {
                Text = sendingControlGroup.Name;
            }
        }

        private void BindViews()
        {
            controlGroupEditor.Model = Model;
            controlGroupEditor.Data = controlGroup;
        }

        private void UnbindViews()
        {
            controlGroupEditor.Data = null;
            controlGroupEditor.Model = null;
        }
    }
}