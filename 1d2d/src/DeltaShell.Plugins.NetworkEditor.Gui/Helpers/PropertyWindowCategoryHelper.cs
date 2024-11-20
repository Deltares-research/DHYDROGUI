namespace DeltaShell.Plugins.NetworkEditor.Gui.Helpers
{
    public static class PropertyWindowCategoryHelper
    {
        /*
         * At the moment there is no clean way to sort categories.
         * A simple HACK is to add tabs in front of categories.
         * These tabs will be trimmed before they are displayed,
         * but it allows you to sort categories.
         *
         * An issue has been created to properly support category
         * sorting. See issue FM1D2D-866.
         */

        public const string GeneralCategory = "\tGeneral";
        public const string RelationsCategory = "\tRelations";
        public const string BranchFeaturesCategory = "Branch Features";
        public const string AdministrationCategory = "Administration";
        public const string GroundLayerCategory = "Ground Layer";
        public const string PillarCategory = "Pillar";
        public const string DefinitionCategory = "Definition";
        public const string MetricsCategory = "Metrics";
        public const string CalculationCategory = "Calculation";
        public const string ShapeCategory = "Shape";
        public const string LateralDiffusionCategory = "Lateral Diffusion";
        public const string CrossSectionCategory = "Cross Section";
        public const string TableCategory = "Table";

    }
}