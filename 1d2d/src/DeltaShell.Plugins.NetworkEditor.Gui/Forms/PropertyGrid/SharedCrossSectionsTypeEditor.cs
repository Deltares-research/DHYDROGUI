using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public class SharedCrossSectionsTypeEditor : UITypeEditor
    {
        private IWindowsFormsEditorService _editorService;
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            _editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            // use a list box
            ListBox lb = new ListBox();
            lb.SelectionMode = SelectionMode.One;
            lb.SelectedValueChanged += OnListBoxSelectedValueChanged;

            // use the Name property for list box display
            lb.DisplayMember = "Name"; // hmm maybe not needed

            // get the pipe object from context
            // this is how we get the list of possible shared cross section definitions
            var propertyBag = context?.Instance as DelftTools.Utils.PropertyBag.Dynamic.DynamicPropertyBag;
            if (propertyBag == null)
                propertyBag = (context?.Instance as object[])?[0] as DelftTools.Utils.PropertyBag.Dynamic.DynamicPropertyBag;
            if (propertyBag == null) return value;
            // have no clue how to do this better...
            var sewerConnectionProperties = TypeUtils.GetField(propertyBag, "propertyObject") as SewerConnectionProperties;
            var sewerConnection = sewerConnectionProperties?.Data as ISewerConnection;
            var sharedCrossSectionNames = sewerConnection?.HydroNetwork?.SharedCrossSectionDefinitions?.Select(scsd => scsd.Name);
            if (sharedCrossSectionNames == null) return value;

            foreach (var scsdName in sharedCrossSectionNames)
            {
                // we store shared cross section names directly in the listbox
                int index = lb.Items.Add(scsdName);
                if (scsdName.Equals(value))
                {
                    lb.SelectedIndex = index;
                }
            }

            // show this model stuff
            _editorService.DropDownControl(lb);
            if (lb.SelectedItem == null) // no selection, return the passed-in value as is
                return value;

            return lb.SelectedItem;
        }

        private void OnListBoxSelectedValueChanged(object sender, EventArgs e)
        {
            // close the drop down as soon as something is clicked
            _editorService.CloseDropDown();
        }
    }
}