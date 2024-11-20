using System;
using System.Drawing;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    public class CommandHelper
    {
        public CommandHelper(Action updateAction)
        {
            UpdateAction = updateAction;
            TextBoxEnabled = true;
        }

        public ICommand CustomCommand
        {
            get
            {
                return new RelayCommand(ExecuteAction);
            }
        }

        public bool ButtonIsVisible
        {
            get
            {
                return ButtonBehaviour != null;
            }
        }

        /// <summary>
        /// Determines whether the user may also enter a value in
        /// the accompanied text box.
        /// </summary>
        public bool TextBoxEnabled { get; set; }

        /// <summary>
        /// Determines whether the button has no image to show.
        /// </summary>
        public bool HasNoImage => ButtonImage == null;

        public Bitmap ButtonImage { get; set; }

        public Func<object> GetModel { get; set; }

        public Action UpdateAction { get; set; }

        public IButtonBehaviour ButtonBehaviour { get; set; }

        private void ExecuteAction(object dummyObject)
        {
            object model = GetModel?.Invoke();
            ButtonBehaviour?.Execute(model);
            UpdateAction?.Invoke();
        }
    }
}