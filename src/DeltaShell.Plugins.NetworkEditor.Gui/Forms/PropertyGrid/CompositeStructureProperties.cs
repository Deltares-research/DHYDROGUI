using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "CompositeStructureProperties_DisplayName")]
    public class CompositeStructureProperties : ObjectProperties<ICompositeBranchStructure>
    {
        private NameValidator nameValidator = NameValidator.CreateDefault();
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(0)]
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
        [PropertyOrder(1)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Number of Structures")]
        [Description("Number of structures in the compound structure.")]
        [PropertyOrder(10)]
        public float StructureCount
        {
            get { return data.Structures.Count; }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Branch")]
        [Description("Channel in which the compound structure is located.")]
        [PropertyOrder(3)]
        public string Channel
        {
            get { return data.Branch == null ? "Channel not set" : data.Branch.ToString(); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage (map)")]
        [Description("Chainage of the compound structure in the channel on the map.")]
        [PropertyOrder(4)]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage")]
        [Description("Chainage of the compound structure in the channel as used in the simulation.")]
        [PropertyOrder(5)]
        public double CompuChainage
        {
            get { return data.ParentStructure != null ? data.ParentStructure.Chainage : data.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
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