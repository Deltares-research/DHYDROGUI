using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public class ManholeProperties : ObjectProperties<Manhole>
    {
        private NameValidator manholeNameValidator = NameValidator.CreateDefault();
        private NameValidator compartmentNameValidator = NameValidator.CreateDefault();
        
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set
            {
                if (manholeNameValidator.ValidateWithLogging(value))
                {
                    data.Name = value;
                }
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("X coordinate")]
        [PropertyOrder(1)]
        public double X
        {
            get { return data.Geometry.Coordinate.X; }
            set { HydroRegionEditorHelper.MoveNodeTo(data, value, Y); }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Y coordinate")]
        [PropertyOrder(2)]
        public double Y
        {
            get { return data.Geometry.Coordinate.Y; }
            set { HydroRegionEditorHelper.MoveNodeTo(data, X, value); }
        }

        private int manholeOneIndex = 0;
        private int manholeTwoIndex = 1;
        private int manholeThreeIndex = 2;

        #region Compartment 1

        [Category("Compartment 1")]
        [DisplayName("Name")]
        [PropertyOrder(0)]
        [DynamicVisible]
        public string CompartmentOneName
        {
            get { return GetStringPropertyFromCompartmentAtIndex(manholeOneIndex, comp => comp.Name); }
            set
            {
                if (compartmentNameValidator.ValidateWithLogging(value))
                {
                    data.Compartments[manholeOneIndex].Name = value;
                }
            }
        }

        [Category("Compartment 1")]
        [PropertyOrder(1)]
        [DisplayName("Bottom level (m)")]
        [DynamicVisible]
        public double CompartmentOneBottomLevel
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeOneIndex, comp => comp.BottomLevel); }
            set { data.Compartments[manholeOneIndex].BottomLevel = value; }
        }

        [Category("Compartment 1")]
        [PropertyOrder(3)]
        [DisplayName("Surface level (m)")]
        [DynamicVisible]
        public double CompartmentOneStreetLevel
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeOneIndex, comp => comp.SurfaceLevel); }
            set { data.Compartments[manholeOneIndex].SurfaceLevel = value; }
        }

        [Category("Compartment 1")]
        [PropertyOrder(4)]
        [DisplayName("Floodable area (m²)")]
        [DynamicVisible]
        public double CompartmentOneFloodableArea
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeOneIndex, comp => comp.FloodableArea); }
            set { data.Compartments[manholeOneIndex].FloodableArea = value; }
        }

        [Category("Compartment 1")]
        [PropertyOrder(5)]
        [DisplayName("Compartment Storage Type")]
        [DynamicVisible]
        public CompartmentStorageType CompartmentOneStorageType
        {
            get { return GetCompartmentPropertyAtIndex(manholeOneIndex, comp => comp.CompartmentStorageType); }
            set { data.Compartments[manholeOneIndex].CompartmentStorageType = value; }
        }

        [Category("Compartment 1")]
        [PropertyOrder(6)]
        [DisplayName("Use table")]
        [DynamicVisible]
        public bool CompartmentOneUseTable
        {
            get { return GetCompartmentPropertyAtIndex(manholeOneIndex, comp => comp.UseTable); }
            set { data.Compartments[manholeOneIndex].UseTable = value; }
        }
        
        [Category("Compartment 1")]
        [PropertyOrder(7)]
        [DisplayName("Interpolation Type")]
        [DynamicReadOnly]
        [DynamicVisible]
        public InterpolationType CompartmentOneInterpolationType
        {
            get { return GetCompartmentPropertyAtIndex(manholeOneIndex, comp => comp.InterpolationType); }
            set { data.Compartments[manholeOneIndex].InterpolationType = value; }
        }
        
        [Category("Compartment 1")]
        [PropertyOrder(8)]
        [DisplayName("Storage")]
        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [DynamicReadOnly]
        [DynamicVisible]
        public Function CompartmentOneStorage
        {
            get { return (Function) GetCompartmentPropertyAtIndex(manholeOneIndex, comp => comp.Storage); }
            set { data.Compartments[manholeOneIndex].Storage = value; }
        }
        #endregion

        #region Compartment 2

        [Category("Compartment 2")]
        [DisplayName("Name")]
        [PropertyOrder(0)]
        [DynamicVisible]
        public string CompartmentTwoName
        {
            get { return GetStringPropertyFromCompartmentAtIndex(manholeTwoIndex, comp => comp.Name); }
            set
            {
                if (compartmentNameValidator.ValidateWithLogging(value))
                {
                    data.Compartments[manholeTwoIndex].Name = value;
                }
            }
        }

        [Category("Compartment 2")]
        [PropertyOrder(1)]
        [DisplayName("Bottom level (m)")]
        [DynamicVisible]
        public double CompartmentTwoBottomLevel
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeTwoIndex, comp => comp.BottomLevel); }
            set { data.Compartments[manholeTwoIndex].BottomLevel = value; }
        }

        [Category("Compartment 2")]
        [PropertyOrder(3)]
        [DisplayName("Surface level (m)")]
        [DynamicVisible]
        public double CompartmentTwoStreetLevel
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeTwoIndex, comp => comp.SurfaceLevel); }
            set { data.Compartments[manholeTwoIndex].SurfaceLevel = value; }
        }

        [Category("Compartment 2")]
        [PropertyOrder(4)]
        [DisplayName("Floodable area (m²)")]
        [DynamicVisible]
        public double CompartmentTwoFloodableArea
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeTwoIndex, comp => comp.FloodableArea); }
            set { data.Compartments[manholeTwoIndex].FloodableArea = value; }
        }

        [Category("Compartment 2")]
        [PropertyOrder(5)]
        [DisplayName("Compartment Storage Type")]
        [DynamicVisible]
        public CompartmentStorageType CompartmentTwoStorageType
        {
            get { return GetCompartmentPropertyAtIndex(manholeTwoIndex, comp => comp.CompartmentStorageType); }
            set { data.Compartments[manholeTwoIndex].CompartmentStorageType = value; }
        }
        
        [Category("Compartment 2")]
        [PropertyOrder(6)]
        [DisplayName("Use table")]
        [DynamicVisible]
        public bool CompartmentTwoUseTable
        {
            get { return GetCompartmentPropertyAtIndex(manholeTwoIndex, comp => comp.UseTable); }
            set { data.Compartments[manholeTwoIndex].UseTable = value; }
        }
        
        [Category("Compartment 2")]
        [PropertyOrder(7)]
        [DisplayName("Interpolation Type")]
        [DynamicReadOnly]
        [DynamicVisible]
        public InterpolationType CompartmentTwoInterpolationType
        {
            get { return GetCompartmentPropertyAtIndex(manholeTwoIndex, comp => comp.InterpolationType); }
            set { data.Compartments[manholeTwoIndex].InterpolationType = value; }
        }
        
        [Category("Compartment 2")]
        [PropertyOrder(8)]
        [DisplayName("Storage")]
        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [DynamicReadOnly]
        [DynamicVisible]
        public Function CompartmentTwoStorage
        {
            get { return (Function) GetCompartmentPropertyAtIndex(manholeTwoIndex, comp => comp.Storage); }
            set { data.Compartments[manholeTwoIndex].Storage = value; }
        }
        #endregion

        #region Compartment 3

        [Category("Compartment 3")]
        [DisplayName("Name")]
        [PropertyOrder(0)]
        [DynamicVisible]
        public string CompartmentThreeName
        {
            get { return GetStringPropertyFromCompartmentAtIndex(manholeThreeIndex, comp => comp.Name); }
            set
            {
                if (compartmentNameValidator.ValidateWithLogging(value))
                {
                    data.Compartments[manholeThreeIndex].Name = value;
                }
            }
        }

        [Category("Compartment 3")]
        [PropertyOrder(1)]
        [DisplayName("Bottom level (m)")]
        [DynamicVisible]
        public double CompartmentThreeBottomLevel
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeThreeIndex, comp => comp.BottomLevel); }
            set { data.Compartments[manholeThreeIndex].BottomLevel = value; }
        }

        [Category("Compartment 3")]
        [PropertyOrder(3)]
        [DisplayName("Surface level (m)")]
        [DynamicVisible]
        public double CompartmentThreeStreetLevel
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeThreeIndex, comp => comp.SurfaceLevel); }
            set { data.Compartments[manholeThreeIndex].SurfaceLevel = value; }
        }

        [Category("Compartment 3")]
        [PropertyOrder(4)]
        [DisplayName("Floodable area (m²)")]
        [DynamicVisible]
        public double CompartmentThreeFloodableArea
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeThreeIndex, comp => comp.FloodableArea); }
            set { data.Compartments[manholeThreeIndex].FloodableArea = value; }
        }
        [Category("Compartment 3")]
        [PropertyOrder(5)]
        [DisplayName("Compartment Storage Type")]
        [DynamicVisible]
        public CompartmentStorageType CompartmentThreeStorageType
        {
            get { return GetCompartmentPropertyAtIndex(manholeThreeIndex, comp => comp.CompartmentStorageType); }
            set { data.Compartments[manholeThreeIndex].CompartmentStorageType = value; }
        }
        
        [Category("Compartment 3")]
        [PropertyOrder(6)]
        [DisplayName("Use table")]
        [DynamicVisible]
        public bool CompartmentThreeUseTable
        {
            get { return GetCompartmentPropertyAtIndex(manholeThreeIndex, comp => comp.UseTable); }
            set { data.Compartments[manholeThreeIndex].UseTable = value; }
        }
        
        [Category("Compartment 3")]
        [PropertyOrder(7)]
        [DisplayName("Interpolation Type")]
        [DynamicReadOnly]
        [DynamicVisible]
        public InterpolationType CompartmentThreeInterpolationType
        {
            get { return GetCompartmentPropertyAtIndex(manholeThreeIndex, comp => comp.InterpolationType); }
            set { data.Compartments[manholeThreeIndex].InterpolationType = value; }
        }
        
        [Category("Compartment 3")]
        [PropertyOrder(8)]
        [DisplayName("Storage")]
        [Editor(typeof(ViewPropertyEditor), typeof(UITypeEditor))]
        [DynamicReadOnly]
        [DynamicVisible]
        public Function CompartmentThreeStorage
        {
            get { return (Function) GetCompartmentPropertyAtIndex(manholeThreeIndex, comp => comp.Storage); }
            set { data.Compartments[manholeThreeIndex].Storage = value; }
        }
        #endregion

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            int compartmentCount = data.Compartments.Count;
            switch (propertyName)
            {
                case nameof(CompartmentOneName):
                case nameof(CompartmentOneBottomLevel):
                case nameof(CompartmentOneStreetLevel):
                case nameof(CompartmentOneFloodableArea):
                case nameof(CompartmentOneStorageType):
                    return compartmentCount > 0;
                case nameof(CompartmentOneUseTable):
                case nameof(CompartmentOneInterpolationType):
                case nameof(CompartmentOneStorage):
                    return compartmentCount > 0 && !IsOutletCompartment(manholeOneIndex);
                case nameof(CompartmentTwoName):
                case nameof(CompartmentTwoBottomLevel):
                case nameof(CompartmentTwoStreetLevel):
                case nameof(CompartmentTwoFloodableArea):
                case nameof(CompartmentTwoStorageType):
                    return compartmentCount > 1;
                case nameof(CompartmentTwoUseTable):
                case nameof(CompartmentTwoInterpolationType):
                case nameof(CompartmentTwoStorage):
                    return compartmentCount > 1 && !IsOutletCompartment(manholeTwoIndex);
                case nameof(CompartmentThreeName):
                case nameof(CompartmentThreeBottomLevel):
                case nameof(CompartmentThreeStreetLevel):
                case nameof(CompartmentThreeFloodableArea):
                case nameof(CompartmentThreeStorageType):
                    return compartmentCount > 2;
                case nameof(CompartmentThreeUseTable):
                case nameof(CompartmentThreeInterpolationType):
                case nameof(CompartmentThreeStorage):
                    return compartmentCount > 2 && !IsOutletCompartment(manholeThreeIndex);
                default:
                    return false;
            }
        }
        
        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(CompartmentOneInterpolationType):
                case nameof(CompartmentOneStorage):
                    return !CompartmentOneUseTable;
                case nameof(CompartmentTwoInterpolationType):
                case nameof(CompartmentTwoStorage):
                    return !CompartmentTwoUseTable;
                case nameof(CompartmentThreeInterpolationType):
                case nameof(CompartmentThreeStorage):
                    return !CompartmentThreeUseTable;
                default:
                    return true;
            }
        }

        private bool IsOutletCompartment(int index) 
            => GetCompartmentPropertyAtIndex(index, compartment => compartment is OutletCompartment);
        
        private string GetStringPropertyFromCompartmentAtIndex(int index, Func<ICompartment, string> function)
            => GetCompartmentPropertyAtIndex(index, function, string.Empty);

        private double GetDoublePropertyFromCompartmentAtIndex(int index, Func<ICompartment, double> function)
            => GetCompartmentPropertyAtIndex(index, function, double.NaN);
        
        private T GetCompartmentPropertyAtIndex<T>(int index, Func<ICompartment, T> func, T defaultValue = default)
        {
            ICompartment compartment = data.Compartments.ElementAtOrDefault(index);
            return compartment != null ? func(compartment) : defaultValue;
        }

        /// <summary>
        /// Sets the name validator to be called when setting the name of the manhole.
        /// </summary>
        /// <param name="nameValidator"> The name validator. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public void SetManholeNameValidator(NameValidator nameValidator)
        {
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            manholeNameValidator = nameValidator;
        }

        /// <summary>
        /// Get or set the <see cref="NameValidator"/> for this instance that is used for the name validation of the manhole.
        /// Property is initialized with a default name validator.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public NameValidator ManholeNameValidator
        {
            get => manholeNameValidator;
            set
            {
                Ensure.NotNull(value, nameof(value));
                manholeNameValidator = value;
            }
        }

        /// <summary>
        /// Get or set the <see cref="NameValidator"/> for this instance that is used for the name validation of the manhole
        /// compartments.
        /// Property is initialized with a default name validator.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public NameValidator CompartmentNameValidator
        {
            get => compartmentNameValidator;
            set
            {
                Ensure.NotNull(value, nameof(value));
                compartmentNameValidator = value;
            }
        }
    }
}
