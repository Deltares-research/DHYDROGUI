using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public class ManholeProperties : ObjectProperties<Manhole>
    {
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
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
            set { data.Compartments[manholeOneIndex].Name = value; }
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
        public CompartmentStorageType Compartment1StorageType
        {
            get
            {
                return data.Compartments.ElementAtOrDefault(manholeOneIndex)?.CompartmentStorageType ?? CompartmentStorageType.Reservoir; }
            set { data.Compartments[manholeOneIndex].CompartmentStorageType = value; }
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
            set { data.Compartments[manholeTwoIndex].Name = value; }
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
        public CompartmentStorageType Compartment2StorageType
        {
            get
            {
                return data.Compartments.ElementAtOrDefault(manholeTwoIndex)?.CompartmentStorageType ?? CompartmentStorageType.Reservoir;
            }
            set { data.Compartments[manholeTwoIndex].CompartmentStorageType = value; }
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
            set { data.Compartments[manholeThreeIndex].Name = value; }
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
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeOneIndex, comp => comp.SurfaceLevel); }
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
        public CompartmentStorageType Compartment3StorageType
        {
            get
            {
                return data.Compartments.ElementAtOrDefault(manholeThreeIndex)?.CompartmentStorageType ?? CompartmentStorageType.Reservoir;
            }
            set { data.Compartments[manholeThreeIndex].CompartmentStorageType = value; }
        }
        #endregion

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            var compartmentCount = data.Compartments.Count;
            switch (propertyName)
            {
                case nameof(CompartmentOneName):
                case nameof(CompartmentOneBottomLevel):
                case nameof(CompartmentOneStreetLevel):
                case nameof(CompartmentOneFloodableArea):
                case nameof(Compartment1StorageType):
                    return compartmentCount > 0;
                case nameof(CompartmentTwoName):
                case nameof(CompartmentTwoBottomLevel):
                case nameof(CompartmentTwoStreetLevel):
                case nameof(CompartmentTwoFloodableArea):
                case nameof(Compartment2StorageType):
                    return compartmentCount > 1;
                case nameof(CompartmentThreeName):
                case nameof(CompartmentThreeBottomLevel):
                case nameof(CompartmentThreeStreetLevel):
                case nameof(CompartmentThreeFloodableArea):
                case nameof(Compartment3StorageType):
                    return compartmentCount > 2;
                default:
                    return false;
            }
        }

        private string GetStringPropertyFromCompartmentAtIndex(int index, Func<ICompartment, string> function)
        {
            var compartments = data.Compartments;
            return compartments.Count > index ? function(compartments[index]) : string.Empty;
        }

        private double GetDoublePropertyFromCompartmentAtIndex(int index, Func<ICompartment, double> function)
        {
            var compartments = data.Compartments;
            return compartments.Count > index ? function(compartments[index]) : double.NaN;
        }
    }
}
