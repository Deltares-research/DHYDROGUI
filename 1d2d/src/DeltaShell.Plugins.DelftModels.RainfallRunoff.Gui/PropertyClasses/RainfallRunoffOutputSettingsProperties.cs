using System;
using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses
{
    public class RainfallRunoffOutputSettingsProperties : ObjectProperties<RainfallRunoffModel>
    {
        [PropertyOrder(0)]
        [DisplayName("Output timestep")]
        [Description("Sets the output timestep")]
        [TypeConverter(typeof (DeltaShellTimeSpanConverter))]
        public TimeSpan OutputTimestep
        {
            get { return data.OutputSettings.OutputTimeStep; }
            set { data.OutputSettings.OutputTimeStep = value; }
        }
        
        [PropertyOrder(0)]
        [DisplayName("Aggregation options")]
        [Description("Sets the aggregation option for the output parameters")]
        [DefaultValue(typeof (AggregationOptions), "None")]
        public AggregationOptions AggregationOption
        {
            get
            {
                return data.OutputSettings.AggregationOption; 
            }
            set
            {
                data.OutputSettings.BeginEdit(string.Format("Setting aggregation option to {0}.", value));
                data.OutputSettings.AggregationOption = value;
                data.OutputSettings.EndEdit();
            }
        }
        
        [PropertyOrder(10)]
        [DisplayName("Unpaved output")]
        [Description("Enables/disables unpaved output")]
        public bool Unpaved
        {
            get => data.OutputSettings.IsOutputEnabledForElementSet(ElementSet.UnpavedElmSet);
            set
            {
                data.OutputSettings.ToggleEngineParametersForElementSet(ElementSet.UnpavedElmSet, value);
            }
        }
        
        [PropertyOrder(20)]
        [DisplayName("Paved output")]
        [Description("Enables/disables paved output")]
        public bool Paved
        {
            get => data.OutputSettings.IsOutputEnabledForElementSet(ElementSet.PavedElmSet);
            set
            {
                data.OutputSettings.ToggleEngineParametersForElementSet(ElementSet.PavedElmSet, value);
            } 
        }
        
        [PropertyOrder(30)]
        [DisplayName("Greenhouse output")]
        [Description("Enables/disables greenhouse output")]
        public bool Greenhouse
        {
            get => data.OutputSettings.IsOutputEnabledForElementSet(ElementSet.GreenhouseElmSet);
            set
            {
                data.OutputSettings.ToggleEngineParametersForElementSet(ElementSet.GreenhouseElmSet, value);
            }
        }
        
        [PropertyOrder(40)]
        [DisplayName("Open water output")]
        [Description("Enables/disables open water output")]
        public bool OpenWater
        {
            get => data.OutputSettings.IsOutputEnabledForElementSet(ElementSet.OpenWaterElmSet);
            set
            {
                data.OutputSettings.ToggleEngineParametersForElementSet(ElementSet.OpenWaterElmSet, value);
            }
        }
        
        [PropertyOrder(50)]
        [DisplayName("Sacramento/HBV output")]
        [Description("Enables/disables sacramento and HBV output")]
        public bool Sacramento
        {
            get => data.OutputSettings.IsOutputEnabledForElementSet(ElementSet.SacramentoElmSet) && data.OutputSettings.IsOutputEnabledForElementSet(ElementSet.HbvElmSet);
            set
            {
                data.OutputSettings.ToggleEngineParametersForElementSet(ElementSet.SacramentoElmSet, value);
                data.OutputSettings.ToggleEngineParametersForElementSet(ElementSet.HbvElmSet, value);
            }
        }
        
        [PropertyOrder(70)]
        [DisplayName("Waste water treatment plant output")]
        [Description("Enables/disables waste water treatment plant output")]
        public bool WasteWaterTreatmentPlant
        {
            get => data.OutputSettings.IsOutputEnabledForElementSet(ElementSet.WWTPElmSet);
            set
            {
                data.OutputSettings.ToggleEngineParametersForElementSet(ElementSet.WWTPElmSet, value);
            }
        }
        
        [PropertyOrder(80)]
        [DisplayName("Balances output")]
        [Description("Enables/disables balances output")]
        public bool BalancePerNode
        {
            get => data.OutputSettings.IsOutputEnabledForElementSet(ElementSet.BalanceNodeElmSet) 
                   && data.OutputSettings.IsOutputEnabledForElementSet(ElementSet.BalanceModelElmSet) ;
            set
            {
                data.OutputSettings.ToggleEngineParametersForElementSet(ElementSet.BalanceNodeElmSet, value);
                data.OutputSettings.ToggleEngineParametersForElementSet(ElementSet.BalanceModelElmSet, value);
            }
        }
        
        [PropertyOrder(100)]
        [DisplayName("Link flow output")]
        [Description("Enables/disables link flow output")]
        public bool LinkFlow
        {
            get => data.OutputSettings.IsOutputEnabledForElementSet(ElementSet.LinkElmSet);
            set
            {
                data.OutputSettings.ToggleEngineParametersForElementSet(ElementSet.LinkElmSet, value);
            }
        }
        
        [PropertyOrder(110)]
        [DisplayName("Link boundary discharge output")]
        [Description("Enables/disables boundary discharge output")]
        public bool BoundaryDischarge
        {
            get => data.OutputSettings.IsOutputEnabledForElementSet(ElementSet.BoundaryElmSet);
            set
            {
                data.OutputSettings.ToggleEngineParametersForElementSet(ElementSet.BoundaryElmSet, value);
            }
        }
        
        [PropertyOrder(120)]
        [DisplayName("NWRW output")]
        [Description("Enables/disables NWRW output")]
        public bool Nwrw
        {
            get => data.OutputSettings.IsOutputEnabledForElementSet(ElementSet.NWRWElmSet);
            set
            {
                data.OutputSettings.ToggleEngineParametersForElementSet(ElementSet.NWRWElmSet, value);
            } 
        }
    }
}