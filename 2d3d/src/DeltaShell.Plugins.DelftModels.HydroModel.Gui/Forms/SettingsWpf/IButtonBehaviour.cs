namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Interface that defines the behaviour of a button when it is pressed.
    /// </summary>
    public interface IButtonBehaviour
    {
        /// <summary>
        /// Execute the button behaviour.
        /// </summary>
        /// <param name="inputObject"> The input object. </param>
        void Execute(object inputObject);
    }
}