using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using DelftTools.Utils;
using DeltaShell.Plugins.CommonTools.Gui.Forms;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses
{
    public class TextDocumentPropertyGridEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((context.PropertyDescriptor).IsReadOnly)
            {
                return value;
            }
            var svc = (IWindowsFormsEditorService) provider.GetService(typeof (IWindowsFormsEditorService));
            if (svc != null)
            {
                var textDocument = value as TextDocument;
                if (textDocument == null)
                    return value;

                var form = new Form
                    {
                        FormBorderStyle = FormBorderStyle.SizableToolWindow,
                        Text = "Edit file: " + textDocument.Name,
                        Width = 1024,
                        Height = 768,
                        ShowInTaskbar = false,
                        ShowIcon = false,
                    };

                var textDocumentView = new TextDocumentView {Data = textDocument, Dock = DockStyle.Fill};
                form.Controls.Add(textDocumentView);
                form.Closing += (s, e) => textDocumentView.Data = null;

                OnBeforeShowForm(form, textDocument);

                svc.ShowDialog(form);
                form.Dispose();
            }
            return value;
        }

        protected virtual void OnBeforeShowForm(Form form, TextDocument textDocument)
        {
        }
    }
}