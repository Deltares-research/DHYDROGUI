using System;
using System.Windows.Input;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class SedimentFractionsEditorRelayCommand : ICommand
    {
        private Action actionToPerform;

        public event EventHandler CanExecuteChanged;

        public SedimentFractionsEditorRelayCommand(Action relayedAction)
        {
            actionToPerform = relayedAction;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            actionToPerform.Invoke();
        }
    }
}