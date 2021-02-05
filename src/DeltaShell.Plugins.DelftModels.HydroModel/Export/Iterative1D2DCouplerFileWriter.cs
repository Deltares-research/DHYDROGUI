namespace DeltaShell.Plugins.DelftModels.HydroModel.Export
{
    /*public static class Iterative1D2DCouplerFileWriter
    {
        public static void Write(string file, Iterative1D2DCoupler iterative1D2DCoupler)
        {
            var mapfile = Iterative1D2DCouplerMappingWriter(file, iterative1D2DCoupler); 
            Iterative1D2DCouplerDefinitionWriter(file, mapfile, iterative1D2DCoupler);
            
        }

        private static string Iterative1D2DCouplerMappingWriter(string file, Iterative1D2DCoupler iterative1D2DCoupler)
        {
            var iterative1D2DCouplerDirectoryName = Path.GetDirectoryName(file) ?? ".";
            var iterative1D2DCouplerMappingWriter = Path.GetFileNameWithoutExtension(file) + "_mapping" + Path.GetExtension(file);
            FileUtils.DeleteIfExists(iterative1D2DCouplerMappingWriter); 
            FileUtils.CreateDirectoryIfNotExists(iterative1D2DCouplerDirectoryName);

            var categories = new List<DelftIniCategory>()
            {
                GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.Iterative1D2DCouplerMappingMajorVersion, GeneralRegion.Iterative1D2DCouplerMappingMinorVersion,
                    GeneralRegion.FileTypeName.Iterative1D2DCouplerMapping)
            };

            var iterative1D2DCouplerLinkCollection = new FeatureCollection((IList)iterative1D2DCoupler.Features, typeof(Iterative1D2DCouplerLink));
            foreach (var iterative1D2DCouplerLinkFeature in iterative1D2DCouplerLinkCollection.Features.Cast<Feature>())
            {
                var link1D2DCategory = new DelftIniCategory("1d2dLink");
                //getting the coordinates, stole this from RefreshMappings method in Iterative1D2DCoupler class
                var coordinate2D = iterative1D2DCouplerLinkFeature.Geometry.Coordinates[0]; // coordinate of 2d cell centre
                var coordinate1D = iterative1D2DCouplerLinkFeature.Geometry.Coordinates[1]; // coordinate of 1d closest grid point

                link1D2DCategory.AddProperty("XY_2D", coordinate2D, format:"F4");
                link1D2DCategory.AddProperty("XY_1D", coordinate1D, format:"F4");
                categories.Add(link1D2DCategory);
            }

            new IniFileWriter().WriteIniFile(categories, Path.Combine(iterative1D2DCouplerDirectoryName,iterative1D2DCouplerMappingWriter));

            return iterative1D2DCouplerMappingWriter;


        }

        private static void Iterative1D2DCouplerDefinitionWriter(string file, string mapfile, Iterative1D2DCoupler iterative1D2DCoupler)
        {
            FileUtils.DeleteIfExists(file);
            var iterative1D2DCouplerDirectoryName = Path.GetDirectoryName(file) ?? ".";
            FileUtils.CreateDirectoryIfNotExists(iterative1D2DCouplerDirectoryName);

            var categories = new List<DelftIniCategory>()
            {
                GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.Iterative1D2DCouplerMajorVersion, GeneralRegion.Iterative1D2DCouplerMinorVersion,
                    GeneralRegion.FileTypeName.Iterative1D2DCoupler)
            };
            
            var f1dModelCategory = new DelftIniCategory("Model");
            f1dModelCategory.AddProperty("type", "Flow1D");
            f1dModelCategory.AddProperty("name", iterative1D2DCoupler.Flow1DModel.Name);
            var dHydroActivity1D = iterative1D2DCoupler.Flow1DModel as IDimrModel;
            if (dHydroActivity1D != null) 
            {
                f1dModelCategory.AddProperty("directory", @"..\" + dHydroActivity1D.DirectoryName.ToLower());
            }
            f1dModelCategory.AddProperty("modelDefinitionFile", iterative1D2DCoupler.Flow1DModel.Name + ".md1d");
            categories.Add(f1dModelCategory);

            var f2dModelCategory = new DelftIniCategory("Model");
            f2dModelCategory.AddProperty("type", "FlowFM");
            f2dModelCategory.AddProperty("name", iterative1D2DCoupler.Flow2DModel.Name);
            var dHydroActivity2D = iterative1D2DCoupler.Flow2DModel as IDimrModel;
            if (dHydroActivity2D != null)
            {
                f2dModelCategory.AddProperty("directory", @"..\" + dHydroActivity2D.DirectoryName.ToLower());
            }
            f2dModelCategory.AddProperty("modelDefinitionFile", iterative1D2DCoupler.Flow2DModel.Name + ".mdu");
            categories.Add(f2dModelCategory);

            var filesCategory = new DelftIniCategory("Files");
            filesCategory.AddProperty("mappingFile", mapfile);
            filesCategory.AddProperty("logFile", "1d2d.log");
            categories.Add(filesCategory);

            var parametersCategory = new DelftIniCategory("Parameters");
            var iterative1D2DCouplerData = new Iterative1D2DCouplerData { Coupler = iterative1D2DCoupler };
            parametersCategory.AddProperty("maximumIterations", iterative1D2DCouplerData.MaxIteration);
            parametersCategory.AddProperty("maximumError", iterative1D2DCouplerData.MaxError, format:"F4");
            categories.Add(parametersCategory);

            new IniFileWriter().WriteIniFile(categories, file);
        }
    }*/
}