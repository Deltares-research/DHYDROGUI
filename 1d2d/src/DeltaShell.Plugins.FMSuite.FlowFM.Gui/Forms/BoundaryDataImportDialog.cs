using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public partial class BoundaryDataImportDialog : Form
    {
        public BoundaryDataImportDialog(IEnumerable<string> points, IEnumerable<int> indices)
        {
            InitializeComponent();
            dataImportPointsListBox1.Items.AddRange(points.OfType<object>().ToArray());
            dataImportPointsListBox1.DataPointIndices = indices.ToList();
        }

        public void Select(int index)
        {
            dataImportPointsListBox1.SetItemChecked(index, true);
            dataImportPointsListBox1.SelectedIndex = index;
        }

        public void Configure(BoundaryDataImporterBase importer)
        {
            importer.DataPointIndices = dataImportPointsListBox1.CheckedIndices.Cast<int>().ToList();
        }


    }
}
