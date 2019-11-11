using System;
using System.ComponentModel;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public class ManholeProperties : ObjectProperties<Manhole>
    {
        [Category("General")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Category("General")]
        [PropertyOrder(1)]
        public double X
        {
            get { return data.Geometry.Coordinate.X; }
            set { data.Geometry.Coordinate.X = value; }
        }

        [Category("General")]
        [PropertyOrder(2)]
        public double Y
        {
            get { return data.Geometry.Coordinate.Y; }
            set { data.Geometry.Coordinate.Y = value; }
        }

        private int manholeOneIndex = 0;
        private int manholeTwoIndex = 1;
        private int manholeThreeIndex = 2;

        #region Compartment 1

        [Category("Manhole 1")]
        [PropertyOrder(0)]
        [DynamicVisible]
        public string CompartmentOneName
        {
            get { return GetStringPropertyFromCompartmentAtIndex(manholeOneIndex, comp => comp.Name); }
            set { data.Compartments[manholeOneIndex].Name = value; }
        }

        [Category("Manhole 1")]
        [PropertyOrder(1)]
        [DisplayName("Bottom level (m)")]
        [DynamicVisible]
        public double CompartmentOneBottomLevel
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeOneIndex, comp => comp.BottomLevel); }
            set { data.Compartments[manholeOneIndex].BottomLevel = value; }
        }

        [Category("Manhole 1")]
        [PropertyOrder(2)]
        [DisplayName("Street level (m)")]
        [DynamicVisible]
        public double CompartmentOneStreetLevel
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeOneIndex, comp => comp.SurfaceLevel); }
            set { data.Compartments[manholeOneIndex].SurfaceLevel = value; }
        }

        #endregion

        #region Compartment 2

        [Category("Manhole 2")]
        [PropertyOrder(0)]
        [DynamicVisible]
        public string CompartmentTwoName
        {
            get { return GetStringPropertyFromCompartmentAtIndex(manholeTwoIndex, comp => comp.Name); }
            set { data.Compartments[manholeTwoIndex].Name = value; }
        }

        [Category("Manhole 2")]
        [PropertyOrder(1)]
        [DisplayName("Bottom level (m)")]
        [DynamicVisible]
        public double CompartmentTwoBottomLevel
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeTwoIndex, comp => comp.BottomLevel); }
            set { data.Compartments[manholeTwoIndex].BottomLevel = value; }
        }

        #endregion

        #region Compartment 3

        [Category("Manhole 3")]
        [PropertyOrder(0)]
        [DynamicVisible]
        public string CompartmentThreeName
        {
            get { return GetStringPropertyFromCompartmentAtIndex(manholeThreeIndex, comp => comp.Name); }
            set { data.Compartments[manholeThreeIndex].Name = value; }
        }

        [Category("Manhole 3")]
        [PropertyOrder(1)]
        [DisplayName("Bottom level (m)")]
        [DynamicVisible]
        public double CompartmentThreeBottomLevel
        {
            get { return GetDoublePropertyFromCompartmentAtIndex(manholeThreeIndex, comp => comp.BottomLevel); }
            set { data.Compartments[manholeThreeIndex].BottomLevel = value; }
        }

        #endregion

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            var compartmentCount = data.Compartments.Count;
            switch (propertyName)
            {
                case "CompartmentOneName":
                case "CompartmentOneBottomLevel":
                    return compartmentCount > 0;
                case "CompartmentTwoName":
                case "CompartmentTwoBottomLevel":
                    return compartmentCount > 1;
                case "CompartmentThreeName":
                case "CompartmentThreeBottomLevel":
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
