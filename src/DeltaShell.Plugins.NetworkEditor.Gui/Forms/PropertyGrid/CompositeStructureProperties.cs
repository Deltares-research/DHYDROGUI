using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "CompositeStructureProperties_DisplayName")]
    public class CompositeStructureProperties : ObjectProperties<ICompositeBranchStructure>
    {
        [PropertyOrder(0)]
        public string Name
        {
            get
            {
                return data.Name;
            }
            set
            {
                data.Name = value;
            }
        }

        [PropertyOrder(1)]
        public string LongName
        {
            get
            {
                return data.LongName;
            }
            set
            {
                data.LongName = value;
            }
        }

        [Description("Number of structures in the composite structure.")]
        [DisplayName("Number of Structures")]
        [PropertyOrder(2)]
        public float StructureCount
        {
            get
            {
                return data.Structures.Count;
            }
        }

        [Description("Channel in which the composite structure is located.")]
        [PropertyOrder(3)]
        public string Channel
        {
            get
            {
                return data.Channel == null ? "Channel not set" : data.Channel.ToString();
            }
        }

        [Description("Chainage of the composite structure in the channel on the map.")]
        [PropertyOrder(4)]
        [Category("Administration")]
        [DisplayName("Chainage (Map)")]
        public double Chainage
        {
            get
            {
                return NetworkHelper.MapChainage(data);
            }
        }

        [Description("Chainage of the composite structure in the channel as used in the simulation.")]
        [PropertyOrder(5)]
        [Category("Administration")]
        [DisplayName("Chainage")]
        public double CompuChainage
        {
            get
            {
                return data.ParentStructure != null ? data.ParentStructure.Chainage : data.Chainage;
            }
            set
            {
                HydroRegionEditorHelper.MoveBranchFeatureTo(data, value);
            }
        }
    }
}