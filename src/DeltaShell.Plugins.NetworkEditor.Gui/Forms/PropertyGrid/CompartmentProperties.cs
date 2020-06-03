using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

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
        [DynamicVisible]
        public CompartmentShape Shape
        {
            get => data.Shape;
            set => data.Shape = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Length")]
        [PropertyOrder(5)]
        [DynamicVisible]
        public double Length
        {
            get => data.ManholeLength;
            set => data.ManholeLength = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Width")]
        [PropertyOrder(6)]
        [DynamicVisible]
        public double Width
        {
            get => data.ManholeWidth;
            set => data.ManholeWidth = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Floodable area")]
        [PropertyOrder(7)]
        [DynamicVisible]
        public double FloodableArea
        {
            get => data.FloodableArea;
            set => data.FloodableArea = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Bottom level")]
        [PropertyOrder(8)]
        [DynamicVisible]
        public double BottomLevel
        {
            get => data.BottomLevel;
            set => data.BottomLevel = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Surface level")]
        [PropertyOrder(9)]
        [DynamicVisible]
        public double SurfaceLevel
        {
            get => data.SurfaceLevel;
            set => data.SurfaceLevel = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Surface water level")]
        [PropertyOrder(9)]
        [DynamicVisible]
        public double SurfaceWaterLevel
        {
            get
            {
                var outlet = data as OutletCompartment;
                if (outlet == null) return 0;
                return outlet.SurfaceWaterLevel;
            }
            set
            {
                var outlet = data as OutletCompartment;
                if (outlet == null) return;
                outlet.SurfaceWaterLevel = value;
            }
        }

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            if (propertyName == nameof(Shape) ||
                propertyName == nameof(Length) ||
                propertyName == nameof(Width) ||
                propertyName == nameof(FloodableArea) ||
                propertyName == nameof(BottomLevel) ||
                propertyName == nameof(SurfaceLevel))
            {
                return !(data is OutletCompartment);
            }

            if (propertyName == nameof(SurfaceWaterLevel))
            {
                return (data is OutletCompartment);
            }

            return true;
        }
    }
}