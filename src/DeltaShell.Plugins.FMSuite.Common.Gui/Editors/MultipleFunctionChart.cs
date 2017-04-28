using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Functions;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public partial class MultipleFunctionChart : Form
    {
        private IDictionary<string, Func<IEnumerable<IFunction>>> availableFunctions;
        private readonly IDictionary<string, IFunction> cachedFunctions;
        private readonly MultipleFunctionView multipleFunctionView;

        public MultipleFunctionChart()
        {
            multipleFunctionView = new MultipleFunctionView {ShowTableView = false};
            cachedFunctions = new Dictionary<string, IFunction>();
            InitializeComponent();
            FillAvailableFunctionsListBox();
        }

        public IDictionary<string, Func<IEnumerable<IFunction>>> AvailableFunctions
        {
            private get { return availableFunctions; }
            set
            {
                availableFunctions = value;
                FillAvailableFunctionsListBox();
                if (availableFunctions.Any())
                {
                    functionsListBox.SetItemChecked(0, true);
                }
            }
        }

        private void FillAvailableFunctionsListBox()
        {
            functionsListBox.Items.Clear();
            if (availableFunctions != null)
            {
                functionsListBox.Items.AddRange(availableFunctions.Keys.ToArray());
            }
        }

        private void FunctionsListBoxItemCheck(object sender, ItemCheckEventArgs e)
        {
            multipleFunctionView.Data = null;
            var selectedItems = functionsListBox.CheckedItems.OfType<string>().ToList();
            if (e.NewValue == CheckState.Checked)
            {
                selectedItems.Add((string) functionsListBox.Items[e.Index]);
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                selectedItems.Remove((string)functionsListBox.Items[e.Index]);
            }
            var cachedFunctionsClone = new Dictionary<string, IFunction>(cachedFunctions);

            foreach (var cachedFunction in cachedFunctionsClone)
            {
                if (!selectedItems.Contains(cachedFunction.Key))
                {
                    cachedFunctions.Remove(cachedFunction);
                }
                else
                {
                    selectedItems.Remove(cachedFunction.Key);
                }
            }
            foreach (var selectedItem in selectedItems)
            {
                cachedFunctions.Add(selectedItem, availableFunctions[selectedItem]().FirstOrDefault());
            }
            if (cachedFunctions.Any())
            {
                multipleFunctionView.Data = cachedFunctions.Values;
            }
        }
    }
}
