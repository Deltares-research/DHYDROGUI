using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public class DataImportPointsListBox : CheckedListBox
    {
        public DataImportPointsListBox()
        {
            DataPointIndices = new List<int>();
            CheckOnClick = true;
        }

        public IList<int> DataPointIndices { get; set; }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            var containsData = DesignMode || DataPointIndices.Contains(e.Index); // breaks in designer?!

            var args = new DrawItemEventArgs(e.Graphics, e.Font, new Rectangle(e.Bounds.Location,e.Bounds.Size), 
                                             e.Index, e.State, containsData ? e.ForeColor : Color.LightGray,
                                             e.BackColor);
            base.OnDrawItem(args);
        }
    }
}