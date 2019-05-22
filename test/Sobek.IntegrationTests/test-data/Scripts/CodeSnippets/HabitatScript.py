from API import ComposeProjectName
from DeltaShell.Plugins.SharpMapGis.ImportExport import GdalFileImporter

model = CurrentProject.RootFolder["HSI_test_scripting"]
print model.Name
runArray = ["e1", "e2"]
for run in runArray:
   # Change model input here
   # Import Flow velocity model grid
   importer = Application.CreateImporterByName("Raster File")
   flow_vel = model.Activities[0]
   gridpath = "c:\\Users\\putten_hs\\Desktop\\ProjectFiles-OldDesktop\\Delft3Druns\\"
   gridpath += run
   gridpath += "\\flow_y1"
   gridpath += run
   gridpath += ".bil"
   print gridpath
   flow_vel.InputGridCoverages[0] = importer.ImportItem("", gridpath); 
   
   # Import Flooding model grid
   flooding = model.Activities[1]
   gridpath = "c:\\Users\\putten_hs\\Desktop\\ProjectFiles-OldDesktop\\Delft3Druns\\"
   gridpath += run
   gridpath += "\\flood_y1"
   gridpath += run
   gridpath += ".bil"
   print gridpath
   flooding.InputGridCoverages[0] = importer.ImportItem("", gridpath);
    
   # Run the model 
   Application.RunActivity(model)
   # Save project and results under a different name in a separate folder
   projectName = ComposeProjectName("HSI_test", run)
   path = "c:\\Users\\putten_hs\\Desktop\\ProjectFiles-OldDesktop\\Habitat\\"
   path += run
   path += "\\"
   path += projectName
   Application.SaveProjectAs(path)