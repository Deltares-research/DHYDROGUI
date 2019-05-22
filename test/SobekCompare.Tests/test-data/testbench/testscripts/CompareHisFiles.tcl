# DOC
#
# CompareHisFiles.tcl
#
# Compare the *.his file from DeltaShell sobeksim with the original reference *.his file within the SOBEK212 
# testbench using TCL.
# Specific parameters on specific locations are being compared using the sobek.cnf file that comes with the testcase
# 
# syntax: tclsh.exe CompareHisFiles.tcl newHisFilesPath refHisFilesPath
#
# Source both procedures.tcl and numcomp.tcl to be able to debug and access external procedures
#
# Copyright (C) 2011 Deltares
#
# Author: Hans van Putten
#
# ENDDOC

# global variables
global coverage
global skipTest
global binDir
global log
global logcase
global scriptspath
global msgBuild
global unacceptable

set coverage 0
set skipTest 0

# first argument after file name: scriptspath
set scriptspath [lindex $argv 0]
# second argument after file name: testcase number (for example Test_001)
set testCase [lindex $argv 1]
# third argument after file name: path to DeltaShell his files
set hisFilesPath [lindex $argv 2]
# fourth argument after file name: path to SOBEK212 his files
set refFilesPath [lindex $argv 3]

# External procedures (originally from sobek 2.12 testbench)
set ProceduresPath [file join $scriptspath "procedures.tcl"]
set NumCompPath [file join $scriptspath "numcomp.tcl"]

# executables ReaHis.exe and ReaMap.exe are in the same directory
set binDir $scriptspath

source $ProceduresPath
source $NumCompPath

# Obtain the path to the test to be able to read the sobek.cnf file
# containing the locations and parameters to compare within the *.his files
set chan [open d:\my.log w]
puts $chan "testCase: $testCase"
puts $chan "refFilesPath: $refFilesPath"
set index1 [string first $testCase $refFilesPath 0]
puts $chan "index1: $index1"
set index2 [string length $testCase]
puts $chan "index2: $index2"
set index3 [expr $index1 + $index2]
puts $chan "index3: $index3"
set testPath [string replace $refFilesPath $index3 end ""]
puts $chan "testPath: $testPath"

# Open log file for messages to return when comparison returns failure
set log [Procedures::Logfile $testPath]

# Initialise the comparison by reading the sobek.cnf file of the test
Procedures::InitialiseTest $testPath

# Create an analysis directory to store the data files, and convert them to ASCII for comparison.
set analDir [file join $testPath analysis]
# Delete the previous $analDir first (if present)
if {[file exists $analDir]} {
      file delete -force $analDir
}
file mkdir $analDir
# Logfile for specific case analysis
set logcase [open [file join $analDir "log.txt"] "w"]

# Copy the data files to compare to the analysis directory
file copy $refFilesPath $analDir
file copy $hisFilesPath $analDir

# Set refCaseNum and newCaseNum
set refCaseNum [file tail $refFilesPath]
set newCaseNum [file tail $hisFilesPath]

# Copy the FileWriters directory also
set resultsPath [string trimright $hisFilesPath "work\\"]

# Copy the sobek.log file if it is present
set sobek_log_name [concat $resultsPath\\sobek.log]
if { [file exists $sobek_log_name] == 1} {
   file copy $sobek_log_name $analDir
}

set fileWritersPath [concat $resultsPath\\FileWriters]
if { [file exists $sobek_log_name] == 1} {
   file copy $fileWritersPath $analDir
}

# Compare the results
Procedures::CheckResults $analDir $elements $refCaseNum $newCaseNum

# Simulation ran but no comparison has been done due to error
if {$skipTest} {
   error $msgBuild
}

if {$unacceptable == 2} {
   error $msgBuild
} else {
   #Everything is OK, so return 0 (do nothing)
}
