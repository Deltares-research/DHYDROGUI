using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    public static class PointFeatureRenderingHelper
    {
        public static void DetermineTranslationFactorForStructures(NetworkFeatureType type, int numberOfFeatures, out int upwardTranslation, out int downwardTranslation)
        {
            switch (type)
            {
                case NetworkFeatureType.Branch:
                    upwardTranslation = -1;
                    downwardTranslation = 1;
                    break;
                case NetworkFeatureType.Node:
                    if (numberOfFeatures > 1)
                    {
                        upwardTranslation = -4;
                        downwardTranslation = -2;
                        break;
                    }

                    upwardTranslation = -3;
                    downwardTranslation = -1;
                    break;
                default:
                    upwardTranslation = -1;
                    downwardTranslation = 1;
                    break;
            }
        }

        public static void DetermineTranslationFactorForComposite(NetworkFeatureType type, out int upwardTranslationFactor, out int downwardTranslationFactor)
        {
            switch (type)
            {
                case NetworkFeatureType.Branch:
                    upwardTranslationFactor = 1;
                    downwardTranslationFactor = 2;
                    break;
                case NetworkFeatureType.Node:
                    upwardTranslationFactor = -2;
                    downwardTranslationFactor = -1;
                    break;
                default:
                    upwardTranslationFactor = 1;
                    downwardTranslationFactor = 2;
                    break;
            }
        }

    
    }
}