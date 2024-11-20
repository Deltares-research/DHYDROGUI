using System;
using System.ComponentModel;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "CrossSectionDefinitionProperties_DisplayName")]
    public class CrossSectionDefinitionProperties : ObjectProperties<ICrossSectionDefinition>
    {
        [Description("Name of the cross section definition")]
        [Category("General")]
        [DisplayName("Name")]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Description("Type of the cross section definition.")]
        [Category("General")]
        [DisplayName("Definition Type")]
        public CrossSectionType Type
        {
            get { return data.CrossSectionType; }
        }
        
        [Description("Thalweg; offset in cross section where thalweg intersects channel.")]
        [Category("General")]
        public double Thalweg
        {
            get { return Math.Round(data.Thalweg, 2); }
            set { data.Thalweg = CrossSectionHelper.ValidateThalWay(data, value); }
        }

        [Description("Does this (ZW) definition have a summer dike")]
        [Category("General")]
        [DisplayName("Has Summerdike")]
        public bool HasSummerdike
        {
            get
            {
                if (data.CrossSectionType == CrossSectionType.ZW)
                {
                    var zwDef = data as CrossSectionDefinitionZW;
                    if (zwDef != null)
                    {
                        return zwDef.SummerDike.Active;
                    }
                }

                return false;
            }
        }

        [Description("Lowest level of the cross section (m)")]
        [Category("Metrics")]
        public double LowestPoint
        {
            get { return data.LowestPoint; }
        }

        [Description("Highest level of the cross section (m)")]
        [Category("Metrics")]
        public double HighestPoint
        {
            get { return data.HighestPoint; }
        }

        [Description("Width of the cross section (m)")]
        [Category("Metrics")]
        public double Width
        {
            get { return data.Width; }
        }
    }
}