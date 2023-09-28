using System;
using DelftTools.Hydro.CrossSections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    public static class SewerConnectionExtensions
    {
        public static double Slope(this ISewerConnection sewerConnection)
        {
            if (sewerConnection == null) return double.NaN;
            var length = sewerConnection.Length;
            var dy = sewerConnection.LevelTarget - sewerConnection.LevelSource;

            var angle = Math.Asin(dy / length);

            return RadToDeg(angle);
        }

        private static double RadToDeg(double rad)
        {
            return 180.0 / Math.PI * rad;
        }

        /// <summary>
        /// If branch of type sewer connection we will generate the default profile for this sewer connection / pipe.
        /// </summary>
        /// <param name="branch"><seealso cref="IBranch"/></param>
        public static void GenerateDefaultProfileForSewerConnections(this IBranch branch)
        {
            if (!(branch is ISewerConnection sewerConnection) ||
                sewerConnection.HydroNetwork == null) return;
            var crossSection = CrossSection.CreateDefault(CrossSectionType.Standard, sewerConnection, sewerConnection.Length / 2, false);
            IEnumerable<ICrossSection> crossSections = sewerConnection
                                                       .HydroNetwork
                                                       .CrossSections
                                                       .Concat(
                                                           sewerConnection
                                                               .HydroNetwork
                                                               .SewerConnections
                                                               .Where(sc => sc.CrossSection != null)
                                                               .Select(sc => sc.CrossSection));

            crossSection.Name = crossSections.Any()
                                    ? NamingHelper.GetUniqueName("SewerProfile_{0}", crossSections, typeof(ICrossSection), true)
                                    : "SewerProfile_1";
            var defaultProfile = sewerConnection.GetDefaultProfile();
            sewerConnection.LevelSource = sewerConnection.LevelSource.Equals(0.0d)
                                              ? sewerConnection.GetDefaultLevelValue()
                                              : sewerConnection.LevelSource;
            sewerConnection.LevelTarget = sewerConnection.LevelTarget.Equals(0.0d)
                                              ? sewerConnection.GetDefaultLevelValue()
                                              : sewerConnection.LevelTarget;
            
            crossSection.UseSharedDefinition(defaultProfile);
            sewerConnection.CrossSection = crossSection;
            if (crossSection.Definition != null)
                sewerConnection.CrossSectionDefinitionName = crossSection.Definition.Name;
        }
    }
}