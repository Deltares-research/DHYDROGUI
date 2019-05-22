# set Directory of Sobek
dir = r"D:\src\delta-shell\test-data\Plugins\DelftModels\DeltaShell.Plugins.DelftModels.WaterFlowModel.CompareSobek.Tests\BasicTests.lit\1";

# import sobek model
model = CurrentProject.ImportSobek(dir + @"\network.tp") # ..........<<<<<<!!!!!

#runmodel
CurrentProject.Run(model)

waterlevel = CurrentProject.GetItemByName("waterlevel")

# get hisfile to compare
calcpnt = CurrentProject.Open(dir + "\calcpnt.his") # Project? Application.OpenProject

# difference
diff = calcpnt - waterlevel

# open view for diff
Application.OpenView(diff) # <<< Gui.OpenView(diff)

