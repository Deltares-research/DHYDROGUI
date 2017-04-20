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

        public ControlGroupEditor ControlGroupEditor
        {
            get { return controlGroupEditor; }
        }

        public IGui Gui
        {
            get
            {
                return gui;
            }
            set
            {
                gui = value;
                controlGroupEditor.Gui = value;
            }
        } // selection and opening views

        public object Data
        {
            get { return controlGroup; }
            set
            {
                UnsubscribeEventListeners();
                UnbindViews();

                controlGroup = value as ControlGroup;

                if (controlGroup == null)
                    return;

                BindViews();
                SubscribeEventListeners();
            }
        }

        public IRealTimeControlModel Model
        {
            get { return model; }
            set
            {
                model = value;
                controlGroupEditor.Model = Model;
            }
        }

        public Image Image
        {
            get { return null; }
            set { }
        }

        public IViewContext ViewContext
        {
            get { return controlGroupEditor.ViewContext; }
            set { controlGroupEditor.ViewContext = value; }
        }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        private void SubscribeEventListeners()
        {
            if (null != controlGroup)
            {
                ((INotifyPropertyChanged)controlGroup).PropertyChanged += ControlGroupPropertyChanged;
            }
        }

        private void UnsubscribeEventListeners()
        {
            if (null != controlGroup)
            {
                ((INotifyPropertyChanged)controlGroup).PropertyChanged -= ControlGroupPropertyChanged;
            }
        }

        void ControlGroupPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name" && sender is ControlGroup)
            {
                Text = ((ControlGroup) sender).Name;
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
