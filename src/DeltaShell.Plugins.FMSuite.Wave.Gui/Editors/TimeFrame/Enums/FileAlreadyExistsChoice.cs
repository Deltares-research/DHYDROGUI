using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Enums
{
    /// <summary>
    /// <see cref="FileAlreadyExistsChoice"/> defines the choices presented to a
    /// user when a file already exists in the input folder.
    /// </summary>
    public enum FileAlreadyExistsChoice
    {
        [Description("Overwrite")]
        Overwrite,
        [Description("Add New")]
        Add,
        [Description("Use Existing")]
        UseExisting,
        [Description("Cancel")]
        Cancel,
    }
}