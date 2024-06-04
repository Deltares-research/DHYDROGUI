using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "NetworkLocationProperties_DisplayName")]
    public class NetworkLocationProperties : ObjectProperties<INetworkLocation>
    {
        private NameValidator nameValidator = NameValidator.CreateDefault();
        
        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Branch")]
        [PropertyOrder(1)]
        public string Branch
        {
            get { return null != data.Branch ? data.Branch.Name : ""; }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [Description("Chainage of the network location in the channel on the map.")]
        [DisplayName("Chainage (map)")]
        [PropertyOrder(2)]
        public double ChainageUsingGeometry
        {
            get { return NetworkHelper.MapChainage(data); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [Description("Chainage of the network location in the channel as used in the simulation.")]
        [DisplayName("Chainage")]
        [PropertyOrder(3)]
        public double Chainage
        {
            get { return data.Chainage; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    data.Name = value;
                }
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Long name")]
        [PropertyOrder(2)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }
        
        /// <summary>
        /// Get or set the <see cref="NameValidator"/> for this instance.
        /// Property is initialized with a default name validator. 
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public NameValidator NameValidator
        {
            get => nameValidator;
            set
            {
                Ensure.NotNull(value, nameof(value));
                nameValidator = value;
            }
        }
    }
}
