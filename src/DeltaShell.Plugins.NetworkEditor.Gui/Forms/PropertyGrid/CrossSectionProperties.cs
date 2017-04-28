using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.CommonTools.Gui.Property.Functions;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    /// <summary>
    /// Enables nice display of properties of a cross section in a propertygrid.
    /// </summary>
    [ResourcesDisplayName(typeof(Resources), "CrossSectionProperties_DisplayName")]
    public class CrossSectionProperties : ObjectProperties<ICrossSection>
    {
        #region Cross Section Properties

        [Description("Id of the cross section.")]
        [Category("General")]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }
        
        [Description("Name of the cross section.")]
        [Category("General")]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }


        [Description("Branch to which this cross section belongs.")]
        [Category("General")]
        public string Branch
        {
            get { return data.Branch == null ? "<none>" : data.Branch.Name; }
        }
        
        [Description("Chainage of the bridge in the channel as used in the simulation.")]
        [Category("General")]
        [DisplayName("Chainage")]
        [DynamicReadOnly]
        public double CompuChainage
        {
            get { return data.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }

        [Description("Chainage of the cross section on the branch.")]
        [Category("General")]
        [DisplayName("Chainage (Map)")]
        public double GeometryChainage
        {
            get { return data.Branch.IsLengthCustom ? BranchFeature.SnapChainage(data.Branch.Geometry.Length, (data.Branch.Geometry.Length / data.Branch.Length) * data.Chainage) : data.Chainage; }
        }

        [Category("General")]
        [Description("All the (custom) attributes for this object.")]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }
        
        #endregion

        #region Definition Properties

        [Description("Id of the cross section definition")]
        [Category("Definition")]
        [DisplayName("Definition Id")]
        public string DefinitionId
        {
            get { return data.Definition.IsProxy ? data.Definition.Name : ""; }
        }

        [Description("Type of the cross section definition.")]
        [Category("Definition")]
        public CrossSectionType CrossSectionType
        {
            get { return data.CrossSectionType; }
        }
        
        [Description("Thalweg; offset in cross section where thalweg intersects channel.")]
        [Category("Definition")]
        [DynamicReadOnly]
        public double Thalweg
        {
            get { return Math.Round(data.Definition.Thalweg, 2); }
            set
            {
                data.Definition.Thalweg = CrossSectionHelper.ValidateThalWay(data.Definition, value);
                data.Geometry = data.Definition.GetGeometry(data);
            }
        }

        [Description("Lowest level of the cross section (m)")]
        [Category("Metrics")]
        public double LowestPoint
        {
            get { return data.Definition.LowestPoint; }
        }

        [Description("Highest level of the cross section (m)")]
        [Category("Metrics")]
        public double HighestPoint
        {
            get { return data.Definition.HighestPoint; }
        }

        [Description("Width of the cross section (m)")]
        [Category("Metrics")]
        public double Width
        {
            get { return data.Definition.Width; }
        }

        #endregion

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            var isGeometryBased = data.CrossSectionType == CrossSectionType.GeometryBased;

            //for geobased CS do not allow editing of compu chainage
            if (propertyName == "CompuChainage")
            {
                return isGeometryBased;
            }

            // Marks properties marked with DynamicReadOnlyAttribute as ReadOnly for Proxy definitions
            if (data.Definition.IsProxy)
            {
                return true;
            }
            
            if (propertyName == "Thalweg")
            {
                return isGeometryBased;
            }

            return true;
        }
    }
}