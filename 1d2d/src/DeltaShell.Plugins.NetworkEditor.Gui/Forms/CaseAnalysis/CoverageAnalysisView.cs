using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Threading;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CaseAnalysis
{
    [Entity(FireOnCollectionChange = false)]
    public class CoverageWrapper: IDisposable
    {
        public CoverageWrapper(INetworkCoverage coverage, INameable model)
        {
            Parent = model;
            Coverage = coverage;
        }

        public INameable Parent { get; private set; }

        public INetworkCoverage Coverage { get; private set; }

        public string DisplayName
        {
            get
            {
                return Coverage == null
                           ? ""
                           : (Parent != null ? Coverage.Name + " (" + Parent.Name + ")" : Coverage.Name);
            }
        }

        public void Dispose()
        {
            Coverage = null;
        }
    }
    
    public partial class CoverageAnalysisView : UserControl, ICompositeView
    {
        private Project data;
        
        private readonly CoverageView coverageView;

        private readonly BindingList<CoverageWrapper> primaryCoverages =
            new ThreadsafeBindingList<CoverageWrapper>(SynchronizationContext.Current);

        private readonly BindingList<CoverageWrapper> secondaryCoverages =
            new ThreadsafeBindingList<CoverageWrapper>(SynchronizationContext.Current);

        private readonly IEventedList<IView> childViews = new EventedList<IView>();

        public CoverageAnalysisView()
        {
            InitializeComponent();
            Text = "Basic Case Analysis";

            warningToolTip.SetToolTip(secondaryCoverageWarningBox, "The two selected network datasets are not defined on the same network (instance). Wherever the geometries do not exactly match, the selected operation cannot be performed.");
            warningToolTip.SetToolTip(referenceValueWarningBox, "The reference value must be a number.");

            coverageView = new CoverageView {Dock = DockStyle.Fill};
            networkCoveragePanel.Controls.Add(coverageView);
            coverageView.BringToFront();

            comboboxPrimary.DataSource = primaryCoverages;
            comboboxSecondary.DataSource = secondaryCoverages;

            comboboxPrimary.DisplayMember = "DisplayName";
            comboboxPrimary.ValueMember = "Coverage";
            comboboxSecondary.DisplayMember = "DisplayName";
            comboboxSecondary.ValueMember = "Coverage";

            FillOperationsList();

            childViews.Add(coverageView);
        }

        public CoverageWrapper CreateWrapper(INetworkCoverage item)
        {
            var models = data.GetAllItemsRecursive().OfType<IModel>().Where(model => !(model is ICompositeActivity));

            var parentModel = models.FirstOrDefault(m => m.GetAllItemsRecursive().OfType<INetworkCoverage>().Contains(item));

            return new CoverageWrapper(item, parentModel);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                foreach (var coverageWrapper in primaryCoverages)
                {
                    coverageWrapper.Dispose();
                }
                foreach (var coverageWrapper in secondaryCoverages)
                {
                    coverageWrapper.Dispose();
                }
                coverageView.Data = null;
                coverageView.Dispose();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region IView members

        public object Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    ((INotifyCollectionChanged)data).CollectionChanged -= ProjectCollectionChanged;
                }

                data = (Project) value;

                if (data != null)
                {
                    ((INotifyCollectionChanged)data).CollectionChanged += ProjectCollectionChanged;
                    foreach (var cov in data.GetAllItemsRecursive().OfType<INetworkCoverage>())
                    {
                        OnNetworkCoverageAdded(cov);
                    }
                }
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion

        private void FillOperationsList()
        {
            comboboxOperation.Items.Clear();

            foreach (var operation in GetOperations())
            {
                comboboxOperation.Items.Add(operation);
            }

            comboboxOperation.SelectedIndex = 0;
        }

        private static IEnumerable<NetworkCoverageOperations.INetworkCoverageOperation> GetOperations()
        {
            yield return new NetworkCoverageOperations.CoverageMeanOperation();
            yield return new NetworkCoverageOperations.CoverageMinOperation();
            yield return new NetworkCoverageOperations.CoverageMaxOperation();
            yield return new NetworkCoverageOperations.CoverageAddOperation();
            yield return new NetworkCoverageOperations.CoverageGreaterThanAsDoubleOperation();
            yield return new NetworkCoverageOperations.CoverageLessThanAsDoubleOperation();
            yield return new NetworkCoverageOperations.CoverageSubtractOperation();
            yield return new NetworkCoverageOperations.CoverageAbsDiffOperation();
            yield return new NetworkCoverageOperations.CoverageGreaterThanDurationAsDoubleOperation();
            yield return new NetworkCoverageOperations.CoverageLessThanDurationAsDoubleOperation();
        }

        private void ProjectCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IEnumerable<INetworkCoverage> coverages = findNetworkCoverages(e.GetRemovedOrAddedItem());
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var cov in coverages)
                {
                    OnNetworkCoverageAdded(cov);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var cov in coverages)
                {
                    OnNetworkCoverageRemoved(cov);
                }
            }
        }

        private static IEnumerable<INetworkCoverage> findNetworkCoverages(object obj)
        {
            var matches = new List<INetworkCoverage>();

            var cov = obj as INetworkCoverage;
            if (cov != null)
            {
                matches.Add(cov);
            }

            var itemContainer = obj as IItemContainer;
            if (itemContainer != null)
            {
                matches.AddRange(itemContainer.GetAllItemsRecursive().OfType<INetworkCoverage>().Distinct());
            }

            return matches;
        }

        private void OnNetworkCoverageAdded(INetworkCoverage cov)
        {
            if (primaryCoverages.Any(cw => ReferenceEquals(cw.Coverage, cov)))
            {
                return;
            }
            var wrapper = CreateWrapper(cov);
            primaryCoverages.Add(wrapper);
            secondaryCoverages.Add(wrapper);
        }

        private void OnNetworkCoverageRemoved(INetworkCoverage cov)
        {
            var wrapper = primaryCoverages.FirstOrDefault(cw => ReferenceEquals(cw.Coverage, cov));
            primaryCoverages.Remove(wrapper);
            secondaryCoverages.Remove(wrapper);
        }

        private void ApplyButtonClick(object sender, EventArgs e)
        {
            var processedCoverage = GetProcessedCoverageFromSelections();

            if (processedCoverage == null)
            {
                return;
            }

            ResetCoverageView();
            coverageView.Data = processedCoverage;
        }

        private INetworkCoverage GetProcessedCoverageFromSelections()
        {
            var operation = GetSelectedOperation();
            var primaryCoverage = comboboxPrimary.SelectedValue as INetworkCoverage;
            if (primaryCoverage == null)
            {
                return null;
            }
            if (ShowMessageBoxIfTimeRequirementPrimaryCoverageNotMet(operation, primaryCoverage))
            {
                return null;
            }

            double referenceValue;
            if (operation.RequiresScalarArgument)
            {
                if (!Double.TryParse(referenceValueTextBox.Text, out referenceValue))
                {
                    return null;
                }
            }
            else
            {
                referenceValue = Double.NaN;
            }

            var secondaryCoverage = operation.RequiresSecondCoverage
                                                     ? comboboxSecondary.SelectedValue as INetworkCoverage
                                                     : null;

            if (ShowMessageBoxIfTimeRequirementSecondaryCoverageNotMet(operation, primaryCoverage, secondaryCoverage))
            {
                return null;
            }

            INetworkCoverage processedCoverage;

            if (operation.RequiresScalarArgument && !operation.RequiresSecondCoverage)
            {
                processedCoverage = operation.Perform(primaryCoverage, referenceValue);
                processedCoverage.Name =
                    String.Format("{0} ({1}) {2}", primaryCoverage.Name, operation, referenceValue);
            }
            else if (operation.RequiresSecondCoverage && !operation.RequiresScalarArgument)
            {
                processedCoverage = operation.Perform(primaryCoverage, secondaryCoverage);
                processedCoverage.Name =
                    String.Format("{0} ({1}) {2}", primaryCoverage.Name, operation, secondaryCoverage != null
                                                                                        ? secondaryCoverage.Name
                                                                                        : "");
            }
            else if (operation.RequiresSecondCoverage && operation.RequiresScalarArgument)
            {
                throw new NotImplementedException();
            }
            else
            {
                processedCoverage = operation.Perform(primaryCoverage, secondaryCoverage);
                processedCoverage.Name =
                    String.Format("{0} ({1}) {2}", primaryCoverage.Name, operation, secondaryCoverage != null
                                                                                        ? secondaryCoverage.Name
                                                                                        : "");
            }

            processedCoverage.Components[0].Name = operation.ToString();
            return processedCoverage;
        }

        private static bool ShowMessageBoxIfTimeRequirementPrimaryCoverageNotMet(
            NetworkCoverageOperations.INetworkCoverageOperation operation,
            INetworkCoverage primaryCoverage)
        {
            if (operation.RequiresPrimaryTimeDependent && !primaryCoverage.IsTimeDependent)
            {
                MessageBox.Show(
                    String.Format("Spatial data {0} is not time dependent, which is required for this operation.",
                                  primaryCoverage.Name));
                return true;
            }

            return false;
        }

        private static bool ShowMessageBoxIfTimeRequirementSecondaryCoverageNotMet(
            NetworkCoverageOperations.INetworkCoverageOperation operation,
            INetworkCoverage primaryCoverage,
            INetworkCoverage secondaryCoverage)
        {
            if (!operation.RequiresSecondCoverage || operation.AllowSecondaryNonTimeDependentIfFirstIs ||
                !primaryCoverage.IsTimeDependent || secondaryCoverage.IsTimeDependent) return false;
            
            MessageBox.Show(String.Format(
                "Spatial data {0} is not time dependent, which is required for this operation because spatial data {1} is time dependent",
                secondaryCoverage.Name, primaryCoverage.Name));

            return true;
        }

        //wtf: why do we even have to do this: fixit!!
        private void ResetCoverageView()
        {
            var mapView = coverageView.ChildViews.OfType<MapView>().First();

            mapView.Map?.Layers.Clear();
            coverageView.Data = null;
        }

        private void ComboboxOperationSelectedValueChanged(object sender, EventArgs e)
        {
            var operation = GetSelectedOperation();

            // For coverage arguments
            comboboxSecondary.Enabled = operation.RequiresSecondCoverage;
            secondaryCoverageWarningBox.Visible = ShowWarningSecondaryCoverage(operation.RequiresSecondCoverage);

            // For scalar arguments
            referenceValueTextBox.Enabled = operation.RequiresScalarArgument;
            referenceValueWarningBox.Visible = ShowWarningReferenceValue(operation.RequiresScalarArgument);
        }

        private NetworkCoverageOperations.INetworkCoverageOperation GetSelectedOperation()
        {
            return (NetworkCoverageOperations.INetworkCoverageOperation) comboboxOperation.SelectedItem;
        }

        private void ComboboxesSelectedValueChanged(object sender, EventArgs e)
        {
            var operation = GetSelectedOperation();
            secondaryCoverageWarningBox.Visible = operation != null && ShowWarningSecondaryCoverage(operation.RequiresSecondCoverage);
        }

        public IEventedList<IView> ChildViews
        {
            get { return childViews; }
        }

        public bool HandlesChildViews { get { return true; } }

        public void ActivateChildView(IView childView) { }

        private void referenceValueTextBox_TextChanged(object sender, EventArgs e)
        {
            var operation = GetSelectedOperation();
            referenceValueWarningBox.Visible = ShowWarningReferenceValue(operation.RequiresScalarArgument);
        }

        private bool ShowWarningSecondaryCoverage(bool secondaryCoverageRequired)
        {
            var primaryCoverage = (INetworkCoverage)comboboxPrimary.SelectedValue;
            var secondaryCoverage = secondaryCoverageRequired
                                                     ? comboboxSecondary.SelectedValue as INetworkCoverage
                                                     : null;
            if (secondaryCoverageRequired)
            {
                return secondaryCoverage == null || primaryCoverage == null || 
                    !ReferenceEquals(primaryCoverage.Network, secondaryCoverage.Network);
            }
            return false;
        }

        private bool ShowWarningReferenceValue(bool requiresScalarArgument)
        {
            if (requiresScalarArgument)
            {
                return !Double.TryParse(referenceValueTextBox.Text, out double _);
            }
            return false;
        }
    }
}