using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public partial class WindFieldListView : UserControl, ICompositeView
    {
        private IEventedList<IWindField> windItems;

        public TableViewTimeSeriesGeneratorTool TimeSeriesGeneratorTool { get; set; }

        public override string Text
        {
            get
            {
                return "Wind editor";
            }
        }

        public object Data
        {
            get
            {
                return WindItems;
            }
            set
            {
                WindItems = value as IEventedList<IWindField>;
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public IEventedList<IView> ChildViews { get; private set; }

        public bool HandlesChildViews
        {
            get
            {
                return true;
            }
        }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        public void ActivateChildView(IView childView)
        {
            if (!(childView is Control))
            {
                return;
            }

            panel2.Controls.Clear();
            ((Control) childView).Dock = DockStyle.Fill;
            panel2.Controls.Add((Control) childView);
        }

        private IEventedList<IWindField> WindItems
        {
            get
            {
                return windItems;
            }
            set
            {
                if (windItems != null)
                {
                    windItems.CollectionChanged -= WindItemsCollectionChanged;
                }

                windItems = value;
                if (windItems != null)
                {
                    windItems.CollectionChanged += WindItemsCollectionChanged;
                }

                PopulateWindItemsListBox();
                windItemsListBox.SelectedIndex = windItems == null || !windItems.Any() ? -1 : 0;
            }
        }

        private void PopulateWindItemsListBox()
        {
            windItemsListBox.Items.Clear();
            bool zeroData = windItems == null;
            buttonAdd.Enabled = !zeroData;
            buttonRemove.Enabled = !zeroData && windItems.Any();
            if (!zeroData)
            {
                windItemsListBox.Items.AddRange(windItems.OfType<object>().ToArray());
            }
        }

        private void WindItemsListBoxOnFormat(object sender, ListControlConvertEventArgs e)
        {
            var windField = e.ListItem as IWindField;
            if (windField != null)
            {
                e.Value = windField.Name;
            }
        }

        private void WindItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            int removedOrAddedIndex = e.GetRemovedOrAddedIndex();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    windItemsListBox.Items.Insert(removedOrAddedIndex, removedOrAddedItem);
                    windItemsListBox.SelectedIndex = removedOrAddedIndex;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    int oldIndex = windItemsListBox.SelectedIndex;
                    windItemsListBox.Items.Remove(removedOrAddedItem);
                    windItemsListBox.SelectedIndex = windItems.Any()
                                                         ? -1
                                                         : Math.Min(oldIndex, windItemsListBox.Items.Count - 1);
                    break;
                default:
                    throw new ArgumentException("Collection change event on wind items is not supported.");
            }
        }

        private void WindItemsListBoxOnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            buttonRemove.Enabled = windItemsListBox.SelectedIndex != -1;
            if (ChildViews.Any())
            {
                foreach (IView childView in ChildViews)
                {
                    childView.Data = null;
                    childView.Dispose();
                }
            }

            ChildViews.Clear();
            IView windView = CreateWindView(windItemsListBox.SelectedItem as IWindField);
            if (windView != null)
            {
                ChildViews.Add(windView);
                ActivateChildView(windView);
            }
        }

        private IView CreateWindView(IWindField windField)
        {
            if (windField == null)
            {
                return null;
            }

            if (windField is UniformWindField)
            {
                var functionView = new FunctionView {Data = windField.Data};
                if (TimeSeriesGeneratorTool != null)
                {
                    TimeSeriesGeneratorTool.ConfigureTableView(functionView.TableView);
                }

                return functionView;
            }

            if (windField is GriddedWindField || windField is SpiderWebWindField)
            {
                return new GriddedWindView {Data = windField};
            }

            return null;
        }

        private void ButtonAddClick(object sender, EventArgs e)
        {
            var dialog = new WindSelectionDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                IWindField windItem = dialog.WindField;
                if (windItem != null)
                {
                    WindItems.Insert(windItemsListBox.SelectedIndex + 1, windItem);
                }
            }
        }

        private void ButtonRemoveClick(object sender, EventArgs e)
        {
            WindItems.Remove(windItemsListBox.SelectedItem as IWindField);
        }
    }
}