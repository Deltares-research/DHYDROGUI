using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    /// <summary>
    /// Generic property grid for objects which implement a name, long name and features.
    /// </summary>
    /// <typeparam name="T">The type of backing object.</typeparam>
    /// <seealso cref="ObjectProperties{T}"/>
    public abstract class FeatureWithAttributeProperties<T> : ObjectProperties<T> where T : INameable, ILongNameable, IFeature
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Category("General")]
        [PropertyOrder(1)]
        public string Name
        {
            get => data.Name;
            set => data.Name = value;
        }

        /// <summary>
        /// Gets or sets the long name.
        /// </summary>
        [Category("General")]
        [PropertyOrder(2)]
        public string LongName
        {
            get => data.LongName;
            set => data.LongName = value;
        }

        /// <summary>
        /// Gets all the (custom) attributes for this object..
        /// </summary>
        [Category("General")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(2)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes =>
            data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray();

        /// <summary>
        /// Gets the x location of the Centroid of the geometry of this object.
        /// </summary>
        [Category("General")]
        public double X => data.Geometry.Centroid.X;

        /// <summary>
        /// Gets the y location of the Centroid of the geometry of this object.
        /// </summary>
        [Category("General")]
        public double Y => data.Geometry.Centroid.Y;
    }
}