using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using DelftTools.Utils.PropertyBag.Dynamic;
using DelftTools.Utils.Reflection;

namespace DeltaShell.NGHS.Common.Gui.PropertyGrid
{
    public class WtmsLayerSrsTypeEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        [RefreshProperties(RefreshProperties.All)]
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context == null || provider == null || context.Instance == null)
            {
                return EditValue(provider, value);
            }
            
            editorService = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
           
            // use a list box
            var lb = new ListBox
            {
                SelectionMode = SelectionMode.One
            };

            lb.SelectedValueChanged += OnListBoxSelectedValueChanged;


            var pb = context.Instance as DynamicPropertyBag;
            var wmtsLayerProperties = TypeUtils.GetField(pb, "propertyObject") as WmtsLayerProperties;
 
            foreach (string epsg in wmtsLayerProperties.SupportedEpsgs)
            {
                // we store benchmarks objects directly in the listbox
                int index = lb.Items.Add(epsg);
                if (epsg.Equals(value))
                {
                    lb.SelectedIndex = index;
                }
            }

            // show this model stuff
            editorService.DropDownControl(lb);

            if (lb.SelectedItem == null) // no selection, return the passed-in value as is
                return value;

            return lb.SelectedItem;
        }

        private void OnListBoxSelectedValueChanged(object sender, EventArgs e)
        {
            // close the drop down as soon as something is clicked
            editorService.CloseDropDown();
        }
    }
}