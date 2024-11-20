using System;
using System.Drawing;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;

namespace DeltaShell.NGHS.Common.Gui.WPF.SettingsView
{
    public class CommandHelper
    {
        public CommandHelper(Action updateAction)
        {
            UpdateAction = updateAction;
        }

        public ICommand CustomCommand => new RelayCommand(ExecuteAction);

        public bool ButtonIsVisible => ButtonFunction != null;

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

        public string Tooltip { get; set; }
    }
}