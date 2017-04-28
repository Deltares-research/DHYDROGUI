from DeltaShell.Plugins.DelftModels.HydroModel import HydroModel as _HydroModel
from DeltaShell.Plugins.DelftModels.HydroModel import ModelGroup as _ModelGroup
from DeltaShell.Plugins.DelftModels.HydroModel.Export import Iterative1D2DCouplerExporter as _Iterative1D2DCouplerExporter

def CreateIntegratedModel(subModels, workingDirectory):
	"""Creates an integrated model containing the provided submodels
	The workingDirectory is the directory were working files will be created"""
	integratedModel = _HydroModel.BuildModel(_ModelGroup.Empty)
	integratedModel.Activities.Clear()
	integratedModel.Activities.AddRange(subModels)
	for model in subModels:
		integratedModel.AutoAddRequiredLinks(model)
		
	integratedModel.ExplicitWorkingDirectory = workingDirectory
	return integratedModel