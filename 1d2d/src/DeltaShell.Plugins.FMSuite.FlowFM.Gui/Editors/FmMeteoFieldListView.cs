using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    /// <summary>
    /// Class responsible for providing the list of items of which you can create a Meteo view.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.UserControl" />
    /// <seealso cref="DelftTools.Controls.ICompositeView" />
    public partial class FmMeteoFieldListView : UserControl, ICompositeView
    {
        public override string Text
        {
            get { return "Meteo Editor"; }
        }
        private IEventedList<IFmMeteoField> fmMeteoItems;

        private IEventedList<IFmMeteoField> FmMeteoItems
        {
            get { return fmMeteoItems; }
            set
            {
                if (fmMeteoItems != null)
                {
                    fmMeteoItems.CollectionChanged -= FmMeteoItemsCollectionChanged;
                }
                fmMeteoItems = value;
                if (fmMeteoItems != null)
                {
                    fmMeteoItems.CollectionChanged += FmMeteoItemsCollectionChanged;
                }
                PopulateMeteoItemsListBox();
                fmMeteoItemsListBox.SelectedIndex = (fmMeteoItems == null || !fmMeteoItems.Any()) ? -1 : 0;
            }
        }

        private void PopulateMeteoItemsListBox()
        {
            fmMeteoItemsListBox.Items.Clear();
            var zeroData = fmMeteoItems == null;
            buttonAdd.Enabled = !zeroData;
            buttonRemove.Enabled = !zeroData && fmMeteoItems.Any();
            if (!zeroData)
            {
                fmMeteoItemsListBox.Items.AddRange(fmMeteoItems.OfType<object>().ToArray());
            }
        }

        private void FmMeteoItemsListBoxOnFormat(object sender, ListControlConvertEventArgs e)
        {
            var fmMeteoField = e.ListItem as IFmMeteoField;
            if (fmMeteoField != null)
            {
                e.Value = fmMeteoField.Name;
            }
        }

        private void FmMeteoItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    fmMeteoItemsListBox.Items.Insert(e.GetRemovedOrAddedIndex(), e.GetRemovedOrAddedItem());
                    fmMeteoItemsListBox.SelectedIndex = e.GetRemovedOrAddedIndex();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldIndex = fmMeteoItemsListBox.SelectedIndex;
                    fmMeteoItemsListBox.Items.Remove(e.GetRemovedOrAddedItem());
                    fmMeteoItemsListBox.SelectedIndex = fmMeteoItems.Any()
                        ? -1
                        : Math.Min(oldIndex, fmMeteoItemsListBox.Items.Count - 1);
                    break;
                default:
                    throw new ArgumentException("Collection change event on wind items is not supported.");
            }
        }

        private void FmMeteoItemsListBoxOnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            buttonRemove.Enabled = fmMeteoItemsListBox.SelectedIndex != -1;
            if (ChildViews.Any())
            {
                foreach (var childView in ChildViews)
                {
                    childView.Data = null;
                    childView.Dispose();
                }
            }
            ChildViews.Clear();
            var windView = CreateFmMeteoView(fmMeteoItemsListBox.SelectedItem as IFmMeteoField);
            if (windView != null)
            {
                ChildViews.Add(windView);
                ActivateChildView(windView);
            }
        }

        public TableViewTimeSeriesGeneratorTool TimeSeriesGeneratorTool { get; set; }

        private IView CreateFmMeteoView(IFmMeteoField fmMeteoField)
        {
            if (fmMeteoField == null) return null;
            if (fmMeteoField is FmMeteoField)
            {
                var functionView = new FunctionView {Data = fmMeteoField.Data, ChartSeriesType = ChartSeriesType.BarSeries};
                if (TimeSeriesGeneratorTool != null)
                {
                    TimeSeriesGeneratorTool.ConfigureTableView(functionView.TableView);
                }
                return functionView;
            }
            return null;
        }

        public object Data
        {
            get { return FmMeteoItems; }
            set { FmMeteoItems = value as IEventedList<IFmMeteoField>; }
        }

        public Image Image { get; set; }
        
        public void EnsureVisible(object item){}

        public ViewInfo ViewInfo { get; set; }

        public IEventedList<IView> ChildViews { get; private set; }

        public bool HandlesChildViews
        {
            get { return true; }
        }

        public void ActivateChildView(IView childView)
        {
            if (!(childView is Control)) return;
            panel2.Controls.Clear();
            ((Control)childView).Dock = DockStyle.Fill;
            panel2.Controls.Add((Control)childView);
        }

        private void ButtonAddClick(object sender, EventArgs e)
        {
            var dialog = new FmMeteoSelectionDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var fmMeteoItem = dialog.FmMeteoField;
                if (fmMeteoItem != null)
                {
                    if (FmMeteoItems.Contains(fmMeteoItem))
                    {
                        if (MessageBox.Show(string.Format("Do you want to remove the existing meteo item : {0}? This will remove all data of this type of meteo item and create an empty meteo item.", fmMeteoItem.Name), "", MessageBoxButtons.YesNo) == DialogResult.No) return;
                        FmMeteoItems.Remove(fmMeteoItem);
                    }
                    FmMeteoItems.Insert(fmMeteoItemsListBox.SelectedIndex + 1, fmMeteoItem);
                }
            }
        }

        private void ButtonRemoveClick(object sender, EventArgs e)
        {
            FmMeteoItems.Remove(fmMeteoItemsListBox.SelectedItem as IFmMeteoField);
        }
    }
}
