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
            TextBoxIsEnabled = true;
        }

        public ICommand CustomCommand
        {
            get { return new RelayCommand(ExecuteAction); }
        }

        public bool ButtonIsVisible { get { return ButtonFunction != null; } }

        /// <summary>
        /// Determines whether the user may also enter a value in
        /// the accompanied text box.
        /// </summary>
        public bool TextBoxIsEnabled { get; set; }

        public Bitmap ButtonImage { get; set; }

        private void ExecuteAction(object dummyObject)
        {
            var model = GetModel?.Invoke();
            ButtonFunction?.Invoke(model);
            UpdateAction?.Invoke();
        }

        public Func<object> GetModel { get; set; }

        public Action UpdateAction { get; set; }

        public Action<object> ButtonFunction { get; set; }
    }
}