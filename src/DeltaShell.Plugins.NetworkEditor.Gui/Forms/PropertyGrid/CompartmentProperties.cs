using System.ComponentModel;
using System.Drawing.Design;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "CompartmentProperties_DisplayName")]
    public class CompartmentProperties : ObjectProperties<ICompartment>
    {
        private NameValidator nameValidator = NameValidator.CreateDefault();
        
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        public string Name
        {
            get => data.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    data.Name = value;
                }
            }
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
        [DynamicReadOnly]
        public double Length
        {
            get => data.ManholeLength;
            set => data.ManholeLength = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Width")]
        [PropertyOrder(6)]
        [DynamicVisible]
        [DynamicReadOnly]
        public double Width
        {
            get => data.ManholeWidth;
            set => data.ManholeWidth = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Floodable area")]
        [PropertyOrder(7)]
        [DynamicVisible]
        [DynamicReadOnly]
        public double FloodableArea
        {
            get => data.FloodableArea;
            set => data.FloodableArea = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Bottom level")]
        [PropertyOrder(8)]
        [DynamicVisible]
        [DynamicReadOnly]
        public double BottomLevel
        {
            get => data.BottomLevel;
            set => data.BottomLevel = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Surface level")]
        [PropertyOrder(9)]
        [DynamicVisible]
        [DynamicReadOnly]
        public double SurfaceLevel
        {
            get => data.SurfaceLevel;
            set => data.SurfaceLevel = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Surface water level")]
        [PropertyOrder(10)]
        [DynamicVisible]
        [DynamicReadOnly]
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
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Compartment Storage Type")]
        [PropertyOrder(11)]
        public CompartmentStorageType CompartmentStorageType
        {
            get => data.CompartmentStorageType;
            set => data.CompartmentStorageType = value;
        }
        
        [Category(PropertyWindowCategoryHelper.TableCategory)]
        [Description("Storage area definition.")]
        [PropertyOrder(12)]
        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [DynamicReadOnly]
        [DynamicVisible]
        public Function Storage
        {
            get { return (Function)data.Storage; }
            set { data.Storage = value; }
        }

        [Category(PropertyWindowCategoryHelper.TableCategory)]
        [Description("Type")]
        [PropertyOrder(13)]
        [DynamicReadOnly]
        [DynamicVisible]
        public InterpolationType InterpolationType
        {
            get { return data.InterpolationType; }
            set { data.InterpolationType = value; }
        }

        [Category(PropertyWindowCategoryHelper.TableCategory)]
        [Description("Use storage as function of level.")]
        [DisplayName("Use table")]
        [PropertyOrder(14)]
        [DynamicVisible]
        public bool UseTable
        {
            get { return data.UseTable; }
            set { data.UseTable = value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsFieldReadOnly(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(SurfaceLevel):
                case nameof(FloodableArea):
                case nameof(Length):
                case nameof(Width):
                case nameof(BottomLevel):
                    return UseTable;
                case nameof(Storage):
                case nameof(InterpolationType):
                    return !UseTable;
                default:
                    return true;
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
                propertyName == nameof(SurfaceLevel) ||
                propertyName == nameof(UseTable) ||
                propertyName == nameof(Storage) ||
                propertyName == nameof(InterpolationType))
            {
                return !(data is OutletCompartment);
            }

            if (propertyName == nameof(SurfaceWaterLevel))
            {
                return (data is OutletCompartment);
            }

            return true;
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