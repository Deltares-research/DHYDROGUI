using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    public partial class ControlGroupLayerEditorView : UserControl, ILayerEditorView
    {
        private IEventedList<ControlGroup> controlGroups;
        private EventedList<ControlGroupRuleWrapper> rulesList;
        private EventedList<ControlGroupConditionWrapper> conditionsList;
        private IModel rtcModel;

        public ControlGroupLayerEditorView()
        {
            InitializeComponent();
            AddOpenViewMenuItems();
        }

        public IModel RtcModel
        {
            get { return rtcModel; }
            set
            {
                if (rtcModel != null)
                {
                    ((INotifyPropertyChanged)rtcModel).PropertyChanged -= ModelPropertyChanged;
                    ((INotifyCollectionChanged)rtcModel).CollectionChanged -= ModelCollectionChanged;
                }

                rtcModel = value;

                if (rtcModel != null)
                {
                    ((INotifyPropertyChanged)rtcModel).PropertyChanged += ModelPropertyChanged;
                    ((INotifyCollectionChanged)rtcModel).CollectionChanged += ModelCollectionChanged;
                }
            }
        }

        private void ModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var ruleBase = e.GetRemovedOrAddedItem() as RuleBase;
            if (sender is IEventedList<RuleBase> && ruleBase != null)
            {
                var ruleToRemove = rulesList.FirstOrDefault(rw => rw.GetRuleBase().Equals(ruleBase));
                if (ruleToRemove != null)
                {
                    rulesList.Remove(ruleToRemove);
                }

                if (e.Action == NotifyCollectionChangedAction.Add && !rulesList.Any(w => w.GetRuleBase().Equals(ruleBase)))
                {
                    rulesList.Add(new ControlGroupRuleWrapper(ruleBase));
                }
            }

            var conditionBase = e.GetRemovedOrAddedItem() as ConditionBase;
            if (sender is IEventedList<ConditionBase> && conditionBase != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    var conditionToRemove = conditionsList.FirstOrDefault(rw => rw.GetConditionBase().Equals(conditionBase));
                    if (conditionToRemove != null)
                    {
                        conditionsList.Remove(conditionToRemove);
                    }
                }
                
                if (e.Action == NotifyCollectionChangedAction.Add && !conditionsList.Any(w => w.GetConditionBase().Equals(conditionBase)))
                {
                    conditionsList.Add(new ControlGroupConditionWrapper(conditionBase)); 
                }
            }
        }

        void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ConditionBase)
            {
                tableViewConditions.ScheduleRefresh();
            }

            if (sender is RuleBase)
            {
                tableViewRules.ScheduleRefresh();
            }
        }

        public object Data
        {
            get { return controlGroups; }
            set
            {
                if (controlGroups != null)
                {
                    controlGroups.CollectionChanged -= ControlGroupsCollectionChanged;
                    ((INotifyPropertyChanged)controlGroups).PropertyChanged -= ControlGroupLayerEditorViewPropertyChanged;
                }

                controlGroups = (IEventedList<ControlGroup>) value;

                if (controlGroups == null)
                {
                    tableViewRules.Data = null;
                    return;
                }

                rulesList = new EventedList<ControlGroupRuleWrapper>(controlGroups.SelectMany(cg => cg.Rules).Select(r => new ControlGroupRuleWrapper(r)));
                conditionsList = new EventedList<ControlGroupConditionWrapper>(controlGroups.SelectMany(cg => cg.Conditions).Select(c => new ControlGroupConditionWrapper(c)));
                
                controlGroups.CollectionChanged += ControlGroupsCollectionChanged;
                ((INotifyPropertyChanged)controlGroups).PropertyChanged += ControlGroupLayerEditorViewPropertyChanged;

                tableViewRules.Data = rulesList;
                tableViewRules.BestFitColumns();

                tableViewConditions.Data = conditionsList;
                tableViewConditions.BestFitColumns();
            }
        }

        void ControlGroupLayerEditorViewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            tableViewConditions.ScheduleRefresh();
            tableViewRules.ScheduleRefresh();
        }

        public Action<object> OpenViewAction { get; set; }

        public Image Image { get; set; }

        public IEnumerable<IFeature> SelectedFeatures { get; set; }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }

        public event EventHandler SelectedFeaturesChanged;
        public ILayer Layer { set; get; }
        public void OnActivated() { }
        public void OnDeactivated() { }

        private void AddOpenViewMenuItems()
        {
            var btnOpenViewMenuItemConditions = new ToolStripMenuItem {Name = "btnOpenViewMenuItem", Text = "Open view...", Image = Resources.PropertiesHS};
            btnOpenViewMenuItemConditions.Click += BtnOpenViewConditionsClick;
            btnOpenViewMenuItemConditions.Font = new Font(btnOpenViewMenuItemConditions.Font, FontStyle.Bold);

            tableViewConditions.RowContextMenu.Items.Add(btnOpenViewMenuItemConditions);

            var btnOpenViewMenuItemRules = new ToolStripMenuItem {Name = "btnOpenViewMenuItem", Text = "Open view...", Image = Resources.PropertiesHS};
            btnOpenViewMenuItemRules.Click += BtnOpenViewRulesClick;
            btnOpenViewMenuItemRules.Font = new Font(btnOpenViewMenuItemRules.Font, FontStyle.Bold);

            tableViewRules.RowContextMenu.Items.Add(btnOpenViewMenuItemRules);
        }

        private void BtnOpenViewRulesClick(object sender, EventArgs e)
        {
            var ruleWrapper = tableViewRules.CurrentFocusedRowObject as ControlGroupRuleWrapper;
            if (ruleWrapper == null) return;

            var ruleBase = ruleWrapper.GetRuleBase();
            OpenView(controlGroups.FirstOrDefault(cg => cg.Rules.Contains(ruleBase)));
        }

        private void BtnOpenViewConditionsClick(object sender, EventArgs e)
        {
            var condition = tableViewConditions.CurrentFocusedRowObject as ControlGroupConditionWrapper;
            if (condition == null) return;

            var conditionBase = condition.GetConditionBase();
            OpenView(controlGroups.FirstOrDefault(cg => cg.Conditions.Contains(conditionBase)));
        }

        private void OpenView(ControlGroup controlGroup)
        {
            if (controlGroup != null && OpenViewAction != null)
            {
                OpenViewAction(controlGroup);
            }
        }

        private void ControlGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var controlGroup = e.GetRemovedOrAddedItem() as ControlGroup;
            if (controlGroup == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    rulesList.AddRange(controlGroup.Rules.Select(r => new ControlGroupRuleWrapper(r)));
                    conditionsList.AddRange(controlGroup.Conditions.Select(c => new ControlGroupConditionWrapper(c)));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var rulesToRemove = rulesList.Where(rw => controlGroup.Rules.Contains(rw.GetRuleBase())).ToList();
                    var conditionsToRemove = conditionsList.Where(cw => controlGroup.Conditions.Contains(cw.GetConditionBase())).ToList();

                    foreach (var ruleWrapper in rulesToRemove)
                    {
                        rulesList.Remove(ruleWrapper);
                    }

                    foreach (var conditionWrapper in conditionsToRemove)
                    {
                        conditionsList.Remove(conditionWrapper);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private class ControlGroupRuleWrapper
        {
            private readonly RuleBase rule;

            public ControlGroupRuleWrapper(RuleBase rule)
            {
                this.rule = rule;
            }

            public RuleBase GetRuleBase()
            {
                return rule;
            }

            public string Name
            {
                get { return rule.Name; }
                set { rule.Name = value; }
            }

            public string Description
            {
                get { return rule.LongName; }
                set { rule.LongName = value; }
            }

            public string Inputs
            {
                get { return GiveNameList(rule.Inputs); }
            }

            public string Outputs
            {
                get { return GiveNameList(rule.Outputs); }
            }

            private string GiveNameList(IEnumerable<INameable> nameables)
            {
                return string.Join(", ", nameables.Select(n => "'" + n.Name + "'"));
            }

        }

        private class ControlGroupConditionWrapper
        {
            private readonly ConditionBase condition;

            public ControlGroupConditionWrapper(ConditionBase condition)
            {
                this.condition = condition;
            }

            public ConditionBase GetConditionBase()
            {
                return condition;
            }

            public string Name
            {
                get { return condition.Name; }
                set { condition.Name = value; }
            }

            public string Description
            {
                get { return condition.LongName; }
                set { condition.LongName = value; }
            }

            public string Input
            {
                get { return condition.Input != null ? condition.Input.Name : " - "; }
            }

            [DisplayName("True outputs")]
            public string TrueOutputs
            {
                get { return GiveNameList(condition.TrueOutputs); }
            }

            [DisplayName("False outputs")]
            public string FalseOutputs
            {
                get { return GiveNameList(condition.FalseOutputs); }
            }

            private string GiveNameList(IEnumerable<INameable> nameables)
            {
                return string.Join(", ", nameables.Select(n => "'" + n.Name + "'"));
            }
        }
    }
}
