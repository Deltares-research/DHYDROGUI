[SedimentFileInformation]
	FileCreatedBy 		= Pedro Barrera Crespo
	FileCreationDate 	= Tue Sep 22 2015, 14:47:46
	FileVersion 		= 02.00
[SedimentOverall]
	IopSus 			= 1 			#		Suspended sediment size is Y/N calculated dependent on d50
	Cref 			= 1.60e+03 		# [kg/m3] 	CSoil Reference density for hindered settling
[Sediment]
	Name 			= #Sediment sand# 	#		Name as specified in NamC in mdf-file
	SedTyp 			= bedload			#		Must be "sand", "mud" or "bedload"
	RhoSol 			= 2.65e+003 		# [kg/m3] 	Specific density
	SedDia 			= 5.0e-004 		# [m] 		Median sediment diameter (D50)
	CDryB 			= 1.6e+003 		# [kg/m3] 	Dry bed density
	IniSedThick 	= 5.0000000e+000      [m]      Initial sediment layer thickness at bed (uniform value or filename)
	FacDSS 			= 1.0e+2		# [-] 		FacDss*SedDia = Initial suspended sediment diameter.
	
    TraFrm          = 1
    Name            = #Engelund-Hansen (1967)#
    ACal            = 0.6
    RouKs           = 0.08