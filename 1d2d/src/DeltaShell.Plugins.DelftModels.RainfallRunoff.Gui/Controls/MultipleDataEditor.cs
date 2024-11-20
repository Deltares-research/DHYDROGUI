using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;
using PropertyInfo = System.Reflection.PropertyInfo;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public partial class MultipleDataEditor : UserControl, IReusableView, ILayerEditorView
    {
        private IEnumerable<IDataRowProvider> data;
        private bool locked;

        public MultipleDataEditor()
        {
            InitializeComponent();
        }

        #region IReusableView Members

        public bool Locked
        {
            get { return locked; }
            set
            {
                if (locked == value)
                    return;

                locked = value;

                if (LockedChanged != null)
                {
                    LockedChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler LockedChanged;

        #endregion

        #region IView<IEnumerable<IDataRowProvider>> Members

        public virtual object Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    UnsubscribeEvents();
                }
                data = (IEnumerable<IDataRowProvider>) value;
                if (data != null)
                {
                    CreateTabsForProviders();
                    ShowDataForSelectedTab();
                    SubscribeEvents();
                }
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion

        private void UnsubscribeEvents()
        {
            foreach (IDataRowProvider provider in data)
            {
                provider.RefreshRequired -= ProviderRefreshRequired;
                provider.Disconnect();
            }
        }

        private void SubscribeEvents()
        {
            foreach (IDataRowProvider provider in data)
            {
                provider.RefreshRequired += ProviderRefreshRequired;
            }
        }

        private void ProviderRefreshRequired(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                var activeProvider = tabControl.SelectedTab.Tag as IDataRowProvider;
                var provider = sender as IDataRowProvider;

                if (activeProvider != null && activeProvider == provider) //only for current page
                {
                    SetDataSource(activeProvider.Rows, true);
                }
            }
        }
        

        private void CreateTabsForProviders()
        {
            tabControl.TabPages.Clear();
            foreach (IDataRowProvider provider in data)
            {
                var tabPage = new TabPage(provider.Name) {Tag = provider};
                tabControl.TabPages.Add(tabPage);
            }
        }

        private void SetDataSource(IEnumerable<IDataRow> data, bool refreshOnly = false)
        {
            tableView.Data = null;

            if (!refreshOnly || tableView.Columns.Count == 0)
            {
                RebuildColumns(data);
            }

            tableView.Data = new BindingSource {DataSource = data};
            tableView.HeaderHeigth = 50;
            tableView.IncludeHeadersOnCopy = true;
            tableView.UseCenteredHeaderText = true;
        }

        private void RebuildColumns(IEnumerable<IDataRow> data)
        {
            tableView.BeginInit();
            tableView.AutoGenerateColumns = false;
            tableView.Columns.Clear();
            if (data.Any())
            {
                AddColumnsForObject(data.First());
            }
            tableView.AllowAddNewRow = false; //?
            tableView.AllowDeleteRow = false; //?
            tableView.EndInit();
        }

        private void AddColumnsForObject(IDataRow item)
        {
            IEnumerable<PropertyInfo> publicProperties = TypeUtils.GetPublicProperties(item.GetType());

            foreach (PropertyInfo prop in publicProperties)
            {
                AddColumnForProperty(prop);
            }
            item.SetColumnEditorForDataWithModel(data?.FirstOrDefault()?.Model, tableView?.Columns);
        }

        private void AddColumnForProperty(PropertyInfo prop)
        {
            DescriptionAttribute descriptionAttribute =
                prop.GetCustomAttributes(false).OfType<DescriptionAttribute>().FirstOrDefault();
            ITableViewColumn column = tableView.AddColumn(prop.Name,
                                                          descriptionAttribute != null
                                                              ? descriptionAttribute.Description
                                                              : prop.Name, prop.GetSetMethod() == null, 90);

            if (typeof (Enum).IsAssignableFrom(prop.PropertyType))
            {
                column.Editor = new ComboBoxTypeEditor {Items = Enum.GetValues(prop.PropertyType)};
            }
        }

        private void TabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            ShowDataForSelectedTab();
        }

        protected void ShowDataForSelectedTab()
        {
            if (tabControl.SelectedTab != null) //happens on closing view
            {
                var provider = tabControl.SelectedTab.Tag as IDataRowProvider;
                if (provider != null)
                {
                    ShowIfFiltered(provider);
                    SetDataSource(provider.Rows);
                }
            }
        }

        private void ShowIfFiltered(IDataRowProvider provider)
        {
            if (provider.HasFilter())
            {
                filterPanel.Visible = true;
            }
        }

        private void BtnClearFilterClick(object sender, EventArgs e)
        {
            filterPanel.Visible = false;
            ClearFilters();
        }

        private void ClearFilters()
        {
            if (Data == null)
                return;

            foreach (IDataRowProvider provider in data)
            {
                provider.ClearFilter();
            }
            ShowDataForSelectedTab();
        }

        public void OnActivated()
        {
            
        }

        public void OnDeactivated()
        {
            
        }

        public IEnumerable<IFeature> SelectedFeatures { get; set; }
        public ILayer Layer { get; set; }
        public event EventHandler SelectedFeaturesChanged;
    }
}