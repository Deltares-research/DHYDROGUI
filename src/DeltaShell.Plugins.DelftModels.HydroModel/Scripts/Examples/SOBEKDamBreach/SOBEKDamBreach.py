import os
import time
from Libraries.StandardFunctions import *

class DamBreachRunner():
	def __init__(self):
		self._integratedmodel = GetModelByName("integrated model")
		self._flowmodel = GetModelByName("dflow-fm")
		self._logpath = os.path.join(Application.ProjectFilePath, '..', Application.Project.Name+'.dblog')		
		
	def run(self):
		"""Run model with dam breach commands"""
		self._inititiate_logger()		
		try:
			# Subscribe function to event
			self._integratedmodel.CurrentWorkflow.StatusChanged += self._dd_event
            
			# run model
			Application.RunActivity(self._integratedmodel)
    
			# Unsubscribe from event
			self._integratedmodel.CurrentWorkflow.StatusChanged -= self._dd_event
		finally:
			self._logger.close()
			
	# ============================        
	# Private methods
	# ============================
	def _dd_event(self, event, args):
		self._logger.write(self._flowmodel.Name+'\n')
		self._logger.write(str(args.NewStatus)+'\n')
		if (str(args.NewStatus) == "Executing"):
			# Get and Set BANK LEVELS
			values = self._flowmodel.BMIEngine.GetValues("zcrest1d2d")
			self._logger.write(str(len(values))+'\n')
			# Overwrite BANK LEVELS at index 9
			if values[9] > 0.0:
				values[9] = values[9] - 0.05 
			self._logger.write(str(values)+'\n')
			self._flowmodel.BMIEngine.SetValuesDouble("zcrest1d2d", values)
			
			
	def _inititiate_logger(self):
		self._logger = open(self._logpath,'wb')
		self._logger.write('SOBEK 3 Dam Breach script log \n')
		self._logger.write(time.strftime('%b-%d-%Y %H:%M:%S')+'\n')

			