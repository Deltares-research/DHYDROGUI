using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public class CompartmentListProperties : ObjectProperties<List<Compartment>>
    {
        public CompartmentListProperties(List<Compartment> compartments)
        {
            data = compartments;
        }
        
        [PropertyOrder(0)]
        [DisplayName("[1]")]
        [DynamicVisible]
        public string CompartmentOne
        {
            get { return data.Any() ? data[0].Name : string.Empty; }
        }

        [PropertyOrder(1)]
        [DisplayName("[2]")]
        [DynamicVisible]
        public string CompartmentTwo
        {
            get { return data.Count > 1 ? data[1].Name : string.Empty; }
        }

        [PropertyOrder(2)]
        [DisplayName("[3]")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DynamicVisible]
        public string CompartmentThree
        {
            get { return data.Count > 2 ? data[2].Name : string.Empty; }
        }

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            switch (propertyName)
            {
                case "CompartmentOne":
                    return data.Any();
                case "CompartmentTwo":
                    return data.Count > 1;
                case "CompartmentThree":
                    return data.Count > 2;
                default:
                    return true;
            }
        }

        public override string ToString()
        {
            return "Count = " + data.Count;
        }
    }
}
