using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using System.ComponentModel;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "CompartmentProperties_DisplayName")]
    public class CompartmentProperties : ObjectProperties<ICompartment>
    {
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        public string Name
        {
            get => data.Name;
            set => data.Name = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Shape")]
        [PropertyOrder(4)]
        public CompartmentShape Shape
        {
            get => data.Shape;
            set => data.Shape = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Length")]
        [PropertyOrder(5)]
        public double Length
        {
            get => data.ManholeLength;
            set => data.ManholeLength = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Width")]
        [PropertyOrder(6)]
        public double Width
        {
            get => data.ManholeWidth;
            set => data.ManholeWidth = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Floodable area")]
        [PropertyOrder(7)]
        public double FloodableArea
        {
            get => data.FloodableArea;
            set => data.FloodableArea = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Bottom level")]
        [PropertyOrder(8)]
        public double BottomLevel
        {
            get => data.BottomLevel;
            set => data.BottomLevel = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Surface level")]
        [PropertyOrder(9)]
        public double SurfaceLevel
        {
            get => data.SurfaceLevel;
            set => data.SurfaceLevel = value;
        }
    }
}