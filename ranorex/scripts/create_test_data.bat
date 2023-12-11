@ECHO off

:: SUMMARY:
:: 		This script checks out all the test data in the specified location and builds the correct folder hierarchy.
:: 		It also creates a mapping from R: to this location as needed by running the Ranorex tests.
:: 		Note that running this script will also clean up and revert the SVN checkouts.

:: REQUIREMENTS:
:: 		- You need to have TortoiseSVN installed: https://tortoisesvn.net/downloads.html
:: 		- The svn.exe location needs to be referenced in the PATH environment variable (e.g. C:\Program Files\TortoiseSVN\bin)

:: USAGE COMMAND LINE:
::		create_test_data.bat <root> <user_name> <password>

:: ========== ARGUMENTS ==========
	:: Args:
	:: 		1: The root location where the test data should be placed (optional). The default relative location with respect to the location of this script is '..\test-data'.
	:: 		2: SVN user name (optional).
	:: 		3: SVN password (optional).

	SET root=%1%
	SET username=%2%
	SET password=%3%
	
	IF "%root%" == "" (SET root=..\test-data) 
	
:: ========== TEST DATA LOCATIONS ==========
	:: Relative paths to the specific test data folders from the root folder.
	SET rel_tutorials=input\tutorials
	SET rel_dflowfm_tutorials=%rel_tutorials%\D-Flow_Flexible_Mesh
	SET rel_dmorph_tutorials=%rel_tutorials%\D-Morphology
	SET rel_drtc_tutorials=%rel_tutorials%\D-Real_Time_Control
	SET rel_dwaq_tutorials=%rel_tutorials%\D-Water_Quality
	SET rel_dwaves_tutorials=%rel_tutorials%\D-Waves
	
	SET rel_courses=input\courses
	SET rel_coastal_morphodynamic_modelling_courses=%rel_courses%\Coastal_morphodynamic_modelling
	SET rel_coastal_hydrodynamic_modelling_courses=%rel_courses%\Coastal_hydrodynamic_modelling
	SET rel_river_modelling_courses=%rel_courses%\River_modelling
	SET rel_environmental_modelling_courses=%rel_courses%\Environmental_modelling
	SET rel_wave_dynamics_courses=%rel_courses%\Wave_dynamics
		
	
	:: SVN repositories.
	SET svn_dflowfm_tutorials=https://repos.deltares.nl/repos/ds/trunk/doc/user_manuals_tutorial_data/Tutorial_D-Flow_FM
	SET svn_dmorph_tutorials=https://repos.deltares.nl/repos/ds/trunk/doc/user_manuals_tutorial_data/Tutorial_D-Morphology/
	SET svn_drtc_tutorials=https://repos.deltares.nl/repos/ds/trunk/doc/user_manuals_tutorial_data/Tutorial_D-RTC/
	SET svn_dwaq_tutorials=
	SET svn_dwaves_tutorials=https://repos.deltares.nl/repos/ds/trunk/doc/course_academy/delft3dfm_wave_dynamics/exercises/day1/exercises/A1-Tutorial/
	
	SET svn_coastal_morphodynamic_modelling_courses=https://repos.deltares.nl/repos/ds/trunk/doc/course_academy/delft3dfm_coastal_morphodynamic_modelling/exercises/
	SET svn_coastal_hydrodynamic_modelling_courses=https://repos.deltares.nl/repos/ds/trunk/doc/course_academy/delft3dfm_coastal_hydrodynamic_modelling/exercises/
	SET svn_river_modelling_courses=https://repos.deltares.nl/repos/ds/trunk/doc/course_academy/delft3dfm_river_modelling/exercises/
	SET svn_environmental_modelling_courses=https://repos.deltares.nl/repos/ds/trunk/doc/course_academy/delft3dfm_environmental_modelling/exercises/input/
	SET svn_wave_dynamics_courses=https://repos.deltares.nl/repos/ds/trunk/doc/course_academy/delft3dfm_wave_dynamics/exercises/
	
	
:: ========== RUN SCRIPT ==========
	CALL :NORMALIZE_PATH %root%
	CALL :CREATE_FILE_STRUCTURE
	CALL :CREATE_NETWORK_MAPPING
	CALL :CHECKOUT_DATA
	
	EXIT /B
	
	
:: ========== FUNCTIONS ==========
	:NORMALIZE_PATH
		:: Sets the test data root folder with the absolute path.
		::
		:: Args:
		:: 		1: The path that should be normalized.
		
		SET root=%~f1
		
		EXIT /B
	
	:CREATE_FILE_STRUCTURE
		:: Creates all the sub folders in the root folder.
		
		CALL :LOG "CREATING FILE STRUCTURE AT : %root%"
		MKDIR %root%\%rel_dflowfm_tutorials%
		MKDIR %root%\%rel_dmorph_tutorials%
		MKDIR %root%\%rel_drtc_tutorials%
		MKDIR %root%\%rel_dwaq_tutorials%
		MKDIR %root%\%rel_dwaves_tutorials%
		
		MKDIR %root%\%rel_coastal_morphodynamic_modelling_courses%
		MKDIR %root%\%rel_coastal_hydrodynamic_modelling_courses%
		MKDIR %root%\%rel_river_modelling_courses%
		MKDIR %root%\%rel_environmental_modelling_courses%
		MKDIR %root%\%rel_wave_dynamics_courses%
		
		EXIT /B
		
	:CREATE_NETWORK_MAPPING
		:: Creates a network mapping from R: to the root folder.
		
		CALL :SET_UNC
		
		CALL :LOG "MAP R: DRIVE TO            : %unc%"
		net use R: /delete /y
		net use R: %unc%
		
		EXIT /B
		
		
	:SET_UNC
		:: Sets the network location path. 
		:: It first determines whether the location is already a network location by checking whether there is a ':' (colon) in the path. 
		:: If the colon is absent, we assume it already is a netork location.
		:: If the colon is present, we replace it with a '$' and prefix the path with '\\localhost\'.
		
		ECHO.%root% | findstr  /C:":" 1>nul
		
		IF ERRORLEVEL 1 (
			SET unc=%root%
		) ELSE (
			SET unc=\\localhost\%root::=$% 
		)
				
		EXIT /B
		
	:CHECKOUT_DATA
		:: Checks out all the test data at within the root folder.
		
		CALL :SVN_CHECKOUT %svn_dflowfm_tutorials% %rel_dflowfm_tutorials%	
		CALL :SVN_CHECKOUT %svn_dmorph_tutorials% %rel_dmorph_tutorials%	
		CALL :SVN_CHECKOUT %svn_drtc_tutorials% %rel_drtc_tutorials%
		:: CALL :SVN_CHECKOUT %svn_dwaq_tutorials% %rel_dwaq_tutorials%	
		CALL :SVN_CHECKOUT %svn_dwaves_tutorials% %rel_dwaves_tutorials%
				
		CALL :SVN_CHECKOUT %svn_coastal_morphodynamic_modelling_courses% %rel_coastal_morphodynamic_modelling_courses%
		CALL :SVN_CHECKOUT %svn_coastal_hydrodynamic_modelling_courses% %rel_coastal_hydrodynamic_modelling_courses%
		CALL :SVN_CHECKOUT %svn_river_modelling_courses% %rel_river_modelling_courses%
		CALL :SVN_CHECKOUT %svn_environmental_modelling_courses% %rel_environmental_modelling_courses%
		CALL :SVN_CHECKOUT %svn_wave_dynamics_courses% %rel_wave_dynamics_courses%
		
		EXIT /B
		
	:SVN_CHECKOUT
		:: Performs an SVN checkout.
		::
		:: Args:
		:: 		1: The SVN repository.
		:: 		2: The relative checkout location to the root folder.
		
		CALL :LOG "SVN CHECKOUT               : %~1%"
		
		svn cleanup %root%\%~2
		IF "%username%" == "" (
			svn checkout %~1 %root%\%~2
		) ELSE (
			svn checkout %~1 %root%\%~2 --username="%username%" --password="%password%"
		)
		
		svn cleanup %root%\%~2 --remove-unversioned --remove-ignored --vacuum-pristines --include-externals
		svn revert %root%\%~2 --recursive --remove-added
		
		EXIT /B
		
	:LOG
		:: Logs a colored message.
		::
		:: Args:
		:: 		1: The message string.
		
		ECHO [32m# %~1 [0m
		