using System.Windows.Forms;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FixedFiles;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses
{
    public class FixedFileTextDocumentPropertyGridEditor : TextDocumentPropertyGridEditor
    {
        protected override void OnBeforeShowForm(Form form, TextDocument textDocument)
        {
            base.OnBeforeShowForm(form, textDocument);

            var button = new Button {Text = "Restore to default", Width = 200, Dock = DockStyle.Left};
            var panel = new Panel {Height = 30, Dock = DockStyle.Top};
            button.Click +=
                (s, e) =>
                textDocument.Content = RainfallRunoffModelFixedFiles.ReadFixedFileFromResource(textDocument.Name);
            panel.Controls.Add(button);
            form.Controls.Add(panel);
        }
    }
}