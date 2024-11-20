using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures
{
    public class GwswConnectionWeir : Weir
    {
        public GwswConnectionWeir(string name) : base(name)
        {
        }

        public string SourceCompartmentName { get; set; }

        public string TargetCompartmentName { get; set; }
        
        protected override void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
            weir.FlowDirection = FlowDirection;
        }

        protected override void SetSewerConnectionProperties(ISewerConnection sewerConnection, IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            sewerConnection.SourceCompartmentName = SourceCompartmentName;
            sewerConnection.TargetCompartmentName = TargetCompartmentName;
            sewerConnection.AddOrUpdateGeometry(hydroNetwork, helper);
            sewerConnection.Length = Length;
        }
    }
}
