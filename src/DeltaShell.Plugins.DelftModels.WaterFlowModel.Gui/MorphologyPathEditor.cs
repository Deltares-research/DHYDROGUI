using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui
{
    public class MorphologyPathEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        [RefreshProperties(RefreshProperties.All)]
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context == null || provider == null || context.Instance == null)
            {
                return EditValue(provider, value);
            }

            var fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                return fileDialog.FileName;
            }

            return null;
        }
    }
}
