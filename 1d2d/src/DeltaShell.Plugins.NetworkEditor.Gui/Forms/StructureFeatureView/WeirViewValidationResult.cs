namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public class WeirViewValidationResult
    {
        public bool FreeFormValid { get; private set; }
        public bool GatedValid { get; private set; }
        public bool CrestShapeValid { get; private set; }
        public WeirViewValidationResult(bool freeFormValid,bool gatedValid,bool crestShapeValid)
        {
            FreeFormValid = freeFormValid;
            GatedValid = gatedValid;
            CrestShapeValid = crestShapeValid;
        }
    }
}