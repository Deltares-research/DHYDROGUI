# DOC
#
# procedures.tcl - Script with procedures used for Sobek validation testcases 
# within Delta Shell
#
# Copyright (C) 2011 Deltares
#
# Author: Hans van Putten
#
# General information:
#
# The script provides a number of procedures that can be used within the
# CompareHisFiles.tcl script to manipulate the input and result files for a 
# test case.
#
# ENDDOC
#
#  $Author$
#  $Date$
#  $Source$
#

# Jaap Zeekant 23 FEB 2016
# Plots can be suppressed by "set makeplots 0"
# Plots can be (re)activated by setting variable makeplots to a value != 0
# Search for "set makeplots"

package require math

namespace eval Procedures {

global                  env

namespace export debug-prompt
namespace export Logfile
namespace export LogMsg
namespace export ErrMsg
namespace export InitialiseTest
namespace export ConvertDataFileToAscii
namespace export ConvertDataFilesToAscii
namespace export RemoveIntermediateFiles
namespace export CheckResults

# --------------------------------------------------------------------
#   Procedure: debug-prompt
# --------------------------------------------------------------------
#
proc debug-prompt {} {
   set cmd ""
   set level [expr {[info level]-1}]
   set prompt "DEBUG\[$level\]% "
   while 1 {
      puts -nonewline $prompt
      flush stdout
      gets stdin line
      append cmd $line\n
      if {[info complete $cmd]} {
         set code [catch {uplevel #$level $cmd} result]
         if {$code == 0 && [string length $result]} {
            puts stdout $result
         } elseif {$code == 3} {
            # break
            error "aborted debugger"
         } elseif {$code == 4} {
            # continue
            return
         } else {
            puts stderr $result
         }
         set prompt "DEBUG\[$level\]% "
         set cmd ""
      } else {
         set prompt " "
      }
   }
}
# --------------------------------------------------------------------
# End of debug-prompt procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: Logfile
# --------------------------------------------------------------------
#
proc Logfile { testPath } {
   global logfilename
   set date        [clock format [clock seconds] -format "%Y-%h-%d"]
   set datetime    [clock format [clock seconds] -format "%Y-%h-%d %H:%M"]
   set logfilename [file join $testPath "log-$date.txt"]
   
   # Delete the old file first, if present
   file delete -force [glob -nocomplain [file join $testPath "log-*.txt"]]
   
   set log         [open $logfilename "a"]
   puts $log "\n---------------------------------------------"
   puts $log "Date/time: $datetime"
   puts $log "---------------------------------------------\n"

   return $log
}
# --------------------------------------------------------------------
# End of Logfile procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: LogMsg
# --------------------------------------------------------------------
#
proc LogMsg { msg } {
   global log
   global logcase
      
   puts $msg
   puts $log $msg
   if {[info exists logcase]} {
      puts $logcase $msg
   }
}
# --------------------------------------------------------------------
# End of LogMsg procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: ErrMsg
# --------------------------------------------------------------------
#
proc ErrMsg { msg } {
  
   LogMsg "ERROR"
   LogMsg $msg
}
# --------------------------------------------------------------------
# End of ErrMsg procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: InitialiseTest
# --------------------------------------------------------------------
#
proc InitialiseTest { test } {
   
   upvar 1 testProjectPath testProjectPath
   upvar 1 projectDir projectDir
   upvar 1 elements elements

   set testProjectPath [glob -nocomplain [file join $test "*.lit"]]
   if {[llength $testProjectPath] == 0} {
      return 0
   }
   
   if {[llength [split $testProjectPath " "]] != 1} {
      ErrMsg "More than one projects found in $test"
      LogMsg "Test skipped!"
      continue
   }

   set datetime [clock format [clock seconds] -format "%Y-%h-%d %H:%M"]
   LogMsg "\n$datetime: Start testing $test"

   # Source the cnf file
   set cnfFile [file join $test "sobek.cnf"]
   if {![file exists $cnfFile]} {
     ErrMsg "Error testing $project: cnf file not found"
     return 0
   } 

   set projectDir [file tail $testProjectPath]
   source $cnfFile
   update
 
   return 1
}
# --------------------------------------------------------------------
# End of InitialiseTest procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: ConvertDataFileToAscii
# --------------------------------------------------------------------
#
proc ConvertDataFileToAscii { exeFile datPath fName outFile } {

   global skipTest
   global binDir

   set workdir [pwd]
   # Get all data from the his file
   set error [catch {
      # Below is the old call to the old ReaHis.exe, do use this call for ReaMap.exe!
      # exec [file join $binDir $exeFile] "-all" $fName ">$outFile"
      if {$exeFile == "reahis.exe"} {
        # ReaHis.exe
        cd $datPath
        exec [file join $binDir $exeFile] $fName ">$outFile"
        # return to workingdir
        cd $workdir
      } elseif {$exeFile == "reamap.exe"} {
        # ReaMap.exe
        cd $datPath
        exec [file join $binDir $exeFile] "-all" $fName ">$outFile"
        # return to workingdir
        cd $workdir
      } else {
        puts "Error: $exeFile not found!"
        LogMsg "Error: $exeFile not found!"
      }
   } msg]
   if { $error } {
      ErrMsg "Error running $exeFile:"
      LogMsg $msg
   }
}
# --------------------------------------------------------------------
# End of ConvertDataFileToAscii procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: ConvertDataFilesToAscii
# --------------------------------------------------------------------
#
proc ConvertDataFilesToAscii {} {

   global analDir
   global datFile
   global refCaseNum
   global newCaseNum
   global elements
   global datList
   global diffs
   global skipTest
   global logcase
   global msgBuild

   set datList {}
   set diffs {}
   set msg ""
   foreach {datFile parameter location abstol reltol} $elements {
      set found 0
      foreach f $datList {
         if {[string equal $datFile $f]} {
            set found 1
            break
         }
      }
      if {!$found} {
         lappend datList $datFile
         lappend diffs 0
      }
   }
   
   foreach f $datList {
      set datName [file join $analDir $refCaseNum $f]
      set ref_size [file size $datName]
      set datPath [file join $analDir $refCaseNum]
      set txtName [file join $analDir [file tail [file rootname $f]]]
      if {[string equal -nocase [file extension $f] ".his"]} {
         # Convert reference his files to ascii
         set txtPath ${txtName}_his_ref.txt
         ConvertDataFileToAscii "reahis.exe" $datPath $f $txtPath
      }

      if {[string equal -nocase [file extension $f] ".map"]} {
         # Convert reference map file to ascii
         set txtPath ${txtName}_map_ref.txt
         ConvertDataFileToAscii "reamap.exe" $datPath $f $txtPath
      }   

      set datName [file join $analDir $newCaseNum $f]
      set new_size [file size $datName]
      set datPath [file join $analDir $newCaseNum]
      
      # Check if both files are of similar size, otherwise fail the test
      if { $new_size < [expr 0.7*$ref_size]} {
         ErrMsg "ERROR in file: $f, reference data file contains a considerably larger amount of data than the new data file!"
   append msg "ERROR in file: $f, reference data file contains a considerably larger amount of data than the new data file! "
   set skipTest 1
      # This might be used in the future:
      #} elseif { $ref_size < [expr 0.99*$new_size]} {
      #   ErrMsg "ERROR in file: $f, new data file contains a considerably larger amount of data than the reference data file!"
  # append msg "ERROR in file: $f, new data file contains a considerably larger amount of data than the reference data file! "
        # set skipTest 1
      }
      
      if {[string equal -nocase [file extension $f] ".his"]} {
         # Convert new hisfile to ascii
         set txtPath ${txtName}_his_new.txt
         ConvertDataFileToAscii "reahis.exe" $datPath $f $txtPath
      }
      
      if {[string equal -nocase [file extension $f] ".map"]} {
         # Convert new mapfile to ascii
         set txtPath ${txtName}_map_new.txt
         ConvertDataFileToAscii "reamap.exe" $datPath $f $txtPath
      }
   }
   if { $skipTest == 1} {
      LogMsg "Finished"
      LogMsg "========================================================================="
      # Close the logfile
      if {[info exists logcase]} {
         close $logcase
         unset logcase
      }
      set msgBuild $msg
   }
}
# --------------------------------------------------------------------
# End of ConvertDataFilesToAscii procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: RemoveIntermediateFiles
# --------------------------------------------------------------------
#
proc RemoveIntermediateFiles {} {

   global datList
   global analDir
   global diffs
   
   set index 0
   foreach f $datList {

      # Check if this datfile (.his or .map) file has differences
      if {![lindex $diffs $index]} {

         # Remove the intermediate his and map txt-files
         set txtName [file join $analDir [file tail [file rootname $f]]]
         if {[string equal -nocase [file extension $f] ".his"]} {
            set txtPath ${txtName}_his_ref.txt
            file delete -force $txtPath
            set txtPath ${txtName}_his_new.txt
            file delete -force $txtPath
         }

         if {[string equal -nocase [file extension $f] ".map"]} {
            set txtPath ${txtName}_map_ref.txt
            file delete -force $txtPath
            set txtPath ${txtName}_map_new.txt
            file delete -force $txtPath
         }
      }
      incr index
   }
}
# --------------------------------------------------------------------
# End of RemoveIntermediateFiles procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: GetNoOfLocations
# --------------------------------------------------------------------
#
proc GetNoOfLocations { in fName } {

   global skipTest

   # Get the number of locations
   set found 0
   set nLocs 0
   while {!$found && ([gets $in line] != -1)} {
      if {[string match " Number of Locations : *" $line]} {
         set found 1
      }
   }

   if {!$found} {
      ErrMsg "Cannot find string ' Number of Locations :' in $fName"
      set skipTest 1
      return
   }
   scan $line " Number of Locations :%d" nLocs

   return $nLocs
}
# --------------------------------------------------------------------
# End of GetNoOfLocations procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: GetAllHisLocations
# --------------------------------------------------------------------
#
proc GetAllHisLocations { txtPath } {

   global skipTest

   set lList {}
   set in [open $txtPath r]
   set nLocs [GetNoOfLocations $in $txtPath]
   if {$skipTest} {
      close $in
      return
   }

   set found 0
   while {($found == 0) && ([gets $in line] != -1)} {
      if {[string match " Parameter??????: *" $line]} {
         set found 1
      }
   }

   if {!$found} {
      ErrMsg "Cannot find parameter line"
      set skipTest 1
      close $in
      return
   }

   # Get the locations
   while {($nLocs > 0) && ([gets $in line] != -1)} {
      set tokens [split $line ":"]
      lappend lList [string trim [lindex $tokens 1]]         
      incr nLocs -1
   }
   close $in 
   
   return $lList
}
# --------------------------------------------------------------------
# End of GetAllHisLocations procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: GetAllMapLocations
# --------------------------------------------------------------------
#
proc GetAllMapLocations { txtFile } {

   global skipTest

   set allLocs{}
   set in [open $txtFile r]
   set nLocs [GetNoOfLocations $in $txtFile]
   if {$skipTest} {
      close $in
      return
   }

   for {set loc 0} {$loc < $nLocs} {incr loc} {
      lappend allLocs $loc
   }
   
   return $allLocs
}
# --------------------------------------------------------------------
# End of GetAllMapLocations procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: GetAllLocations
# --------------------------------------------------------------------
#
proc GetAllLocations { analDir datFile } {

   global skipTest

   set allLocs {}
   set datName [file join $analDir [file rootname $datFile]]
   if {[string equal -nocase [file extension $datFile] ".his"]} {
      set txtPath ${datName}_his_new.txt
      set allLocs [GetAllHisLocations $txtPath]
      if {$skipTest} return
   } else {
      set txtPath ${datName}_map_new.txt
      set allLocs [GetAllMapLocations $txtPath]
      if {$skipTest} return
   }
   
   return $allLocs
}
# --------------------------------------------------------------------
# End of GetAllLocations procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: GetAllParameters
# --------------------------------------------------------------------
#
proc GetAllParameters { analDir datFile } {

   global skipTest

   set allParms {}
   set datName [file join $analDir [file rootname $datFile]]
   set txtPath ${datName}_his_new.txt
   set in [open $txtPath r]
   while {[gets $in line] != -1} {
      if {[string match " Parameter??????: *" $line]} {
         set fields [split $line ":"]
         set newPar [string trim [lindex $fields 1]]
         regsub {\[.*$} $newPar "" newPar
         lappend allParms $newPar
      }
   }
   close $in  
  
   if {[llength $allParms] <= 0} {
      ErrMsg "Cannot find parameters"
      set skipTest 1
      close $in
      return
   }   
   
   return $allParms
}
# --------------------------------------------------------------------
# End of GetAllParameters procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: SearchPar1
# --------------------------------------------------------------------
#
proc SearchPar1 { in par } {

   upvar 1 count cnt
   upvar 1 parmNo parmNo

   # Remove spaces
   set newPar [string map {" " ""} $par]
   # Search for $newPar in the his file result
   set cnt 0
   set parmNo 0

   set header "    Date      Time"
   set len1 [string length $header]
   while {([gets $in line] != -1) && (![string equal -length $len1 $header $line])} {
      if {[string match " Parameter??????: *" $line]} {

         set match 0
         # Remove spaces
         set newLine [string map {" " ""} $line]
         # Try matching newPar
         if {[string equal -nocase [lindex [split $newLine ":"] 1] $newPar]} {
            # Match
            set match 1
         } else {
            # Remove the trailing unit specification and try again
            if {[regsub {\[.*$} $newLine "" newLine]} {
               # Trailing unit removed. Now newPar must match completely
               if {[string equal -nocase [lindex [split $newLine ":"] 1] $newPar]} {
                  set match 1
               }
            } else {
               # No unit present; match only for the length of newPar
               set len2 [string length $newPar]
               if {[string equal -nocase -length $len2 [lindex [split $newLine ":"] 1] $newPar]} {
                  set match 1
               }
            }               
         }
         if {$match} {
            incr cnt
            # Read the parameter number and remember where the position
            scan $line "%*s%d" parmNo
         }         
      }
   }
}
# --------------------------------------------------------------------
# End of SearchPar1 procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: SearchPar2
# --------------------------------------------------------------------
#
proc SearchPar2 { in par } {

   upvar 1 count cnt
   upvar 1 parmNo parmNo
   
   # Add a space
   set newPar "$par "
   set cnt 0
   set parmNo 0
   set header "    Date      Time"
   set len1 [string length $header]
   while {([gets $in line] != -1) && (![string equal -length $len1 $header $line])} {
      if {[string match " Parameter??????: *" $line]} {
         # Match the parameter
         set len2 [string length $newPar]
         if {[string equal -nocase -length $len2 [string trimleft [lindex [split $line ":"] 1]] $newPar]} {
            incr cnt
            # Read the parameter number and remember where the position
            scan $line "%*s%d" parmNo
         }
      }
   }
}
# --------------------------------------------------------------------
# End of SearchPar2 procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: FindParameter
# --------------------------------------------------------------------
#
proc FindParameter { in fName param } {

   global skipTest
 
   # Look for the parameter
   set found 0
   set count 0
   # Remember the position where we start to find
   set offSet [tell $in]
   SearchPar1 $in $param
   if {$count >= 1} {
      if {$count == 1} {
         set found 1
      } else {
         # Ambiguous parameter. Try adding space after the parameter
         # Skip back to the starting position
         seek $in $offSet
         SearchPar2 $in $param
         if {$count >= 1} {
            if {$count == 1} {
               set found 1
            } else {
               ErrMsg "Ambiguous paramater in $fName: $param"
               set skipTest 1
            }
         }
      }
   } 
   # Skip back to the starting position
   seek $in $offSet
   
   return $parmNo
}
# --------------------------------------------------------------------
# End of FindParameter procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: GetParameterNo
# --------------------------------------------------------------------
#
proc GetParameterNo { in fName parameter } {

   global skipTest

   # Get the number of the parameter
   set found 0
   set parmNo 0
   while {!$found && ([gets $in line] != -1)} {
      if {[string match "*$parameter*" $line]} {
         set found 1
      }
   }

   if {!$found} {
      ErrMsg "Cannot find '$parameter' in $fName"
      set skipTest 1
      return
   }
   scan $line " Parameter%d" parmNo

   return $parmNo
}
# --------------------------------------------------------------------
# End of GetParameterNo procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: GetNoOfParameters
# --------------------------------------------------------------------
#
proc GetNoOfParameters { in fName } {

   global skipTest

   # Get the number of locations
   set found 0
   set parmNo 0
   while {!$found && ([gets $in line] != -1)} {
      if {[string match " Number of Parameters: *" $line]} {
         set found 1
      }
   }

   if {!$found} {
      ErrMsg "Cannot find string ' Number of Parameters :' in $fName"
      set skipTest 1
      return
   }
   scan $line " Number of Parameters:%d" nLocs

   return $nLocs
}
# --------------------------------------------------------------------
# End of GetNoOfParameters procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: GetNoOfLocations
# --------------------------------------------------------------------
#
proc GetNoOfLocations { in fName } {

   global skipTest

   # Get the number of locations
   set found 0
   set nLocs 0
   while {!$found && ([gets $in line] != -1)} {
      if {[string match " Number of Locations : *" $line]} {
         set found 1
      }
   }

   if {!$found} {
      ErrMsg "Cannot find string ' Number of Locations :' in $fName"
      set skipTest 1
      return
   }
   scan $line " Number of Locations :%d" nLocs

   return $nLocs
}
# --------------------------------------------------------------------
# End of GetNoOfLocations procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: GetLocationFromHiaFile
# --------------------------------------------------------------------
#
proc GetLocationFromHiaFile { projectDir refCaseNum hianame loc } {

   global skipTest

   # Search for the group "Long Locations"
   set found 0
   set hiaPath [file join $projectDir $refCaseNum $hianame]
   if {![file exists $hiaPath]} {
      return 0
   }
   
   set in [open $hiaPath r]
   while {!$found && ([gets $in line] != -1)} {
      if {[string equal "\[Long Locations\]" $line]} {
         set found 1
      }
   }

   set locNo 0
   if {$found} {
      set offSet [tell $in]
      # Remove spaces
      set newLoc [string map {" " ""} $loc]
      # Search for $newLoc in the hia file
      set count 0
      while {([gets $in line] != -1) && ([string match "\[*\]" $line] != 1)} {
         # Remove spaces
         set match 0
         set newLine [string map {" " ""} $line]
         # Try matching newLoc
         if {[string equal -nocase [lindex [split $newLine "="] 1] $newLoc]} {
            # Match
            set match 1
         } else {
            # Match only for the length of newLoc
            set len [string length $newLoc]
            if {[string equal -nocase -length $len [lindex [split $newLine "="] 1] $newLoc]} {
               set match 1
            }               
         }
         if {$match} {
            scan $line "%d" locNo
            incr count
         }         
      }

      if {$count > 1} {
         # Ambiguous location try adding space after the location and try again
         seek $in $offSet
         set newLoc "$loc "
         set count 0
         while {([gets $in line] != -1) && ([string match "\[*\]" $line] != 1)} {
            # Match the location
            set len [string length $newLoc]
            if {[string equal -nocase -length $len [string trimleft [lindex [split $line "="] 1]] $newLoc]} {
               scan $line "%d" locNo
               incr count
            }
         }
         if {$count >= 1} {
            if {$count > 1} {
               ErrMsg "Ambiguous location in $hianame: $loc"
               set skipTest 1
               close $in
               return 0
            }
         } else { 
            # Location not found, return 0
         }    
      }
   }
   close $in
   return $locNo
}
# --------------------------------------------------------------------
# End of GetLocationFromHiaFile procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: GetParmNumFromHiaFile
# --------------------------------------------------------------------
#
proc GetParmNumFromHiaFile { projectDir refCaseNum hianame param } {

   global skipTest

   # Search for the group "Long Parameters"
   set found 0
   set hiaPath [file join $projectDir $refCaseNum $hianame]
   if {![file exists $hiaPath]} {
      return 0
   }
   
   set in [open $hiaPath r]
   while {!$found && ([gets $in line] != -1)} {
      if {[string equal "\[Long Parameters\]" $line]} {
         set found 1
      }
   }

   set parmNo 0
   if {$found} {
      set offSet [tell $in]
      # Remove spaces
      set newPar [string map {" " ""} $param]
      # Search for $newPar in the hia file
      set count 0
      while {([gets $in line] != -1) && ([string match "\[*\]" $line] != 1)} {
         # Remove spaces
         set match 0
         set newLine [string map {" " ""} $line]
         # Try matching newPar
         if {[string equal -nocase [lindex [split $newLine "="] 1] $newPar]} {
            # Match
            set match 1
         } else {
            # Remove the trailing unit specification and try again
            if {[regsub {\[.*$} $newLine "" newLine]} {
               # Trailing unit removed. Now newPar must match completely
               if {[string equal -nocase [lindex [split $newLine "="] 1] $newPar]} {
                  set match 1
               }
            } else {
               # No unit present; match only for the length of newPar
               set len [string length $newPar]
               if {[string equal -nocase -length $len [lindex [split $newLine "="] 1] $newPar]} {
                  set match 1
               }
            }               
         }
         if {$match} {
            scan $line "%d" parmNo
            incr count
         }         
      }

      if {$count > 1} {
         # Ambiguous parameter try adding space after the parameter and try again
         seek $in $offSet
         set newPar "$param "
         set count 0
         while {([gets $in line] != -1) && ([string match "\[*\]" $line] != 1)} {
            # Match the parameter
            set len [string length $newPar]
            if {[string equal -nocase -length $len [string trimleft [lindex [split $line "="] 1]] $newPar]} {
               scan $line "%d" parmNo
               incr count
            }
         }
         if {$count >= 1} {
            if {$count > 1} {
               ErrMsg "Ambiguous parameter in $hianame: $param"
               set skipTest 1
               close $in
               return 0
            }
         } else { 
            # Param not found, return 0
         }    
      }
   }

   close $in
   return $parmNo
}
# --------------------------------------------------------------------
# End of GetParmNumFromHiaFile procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: GetMapData
# --------------------------------------------------------------------
#
proc GetMapData { projectDir refCaseNum mapPath txtFile param loc outFile } {

   global skipTest

   set in [open $txtFile r]
   # Look for the parameter
   set nParm [GetNoOfParameters $in $txtFile]
   if {$skipTest} {
      close $in
      return
   }
   
   set nLocs [GetNoOfLocations $in $txtFile]
   if {$skipTest} {
      close $in
      return
   }

   if {$nParm <= 0} {
      # Parameter not found in map file. Try hia file
      set nParm [GetParmNumFromHiaFile $projectDir $refCaseNum \
                                           "[file rootname $mapPath].hia" $param]
   }
   if {$skipTest} {
      close $in
      return
   }

   set parmNo [GetParameterNo $in $txtFile $param]
   if {$skipTest} {
      close $in
      return
   }

   # Read and dump the data
   set colNo [expr [expr [expr $parmNo-1]*$nLocs] + $loc]
   ReadAndDumpData $in $mapPath $txtFile $param $loc $colNo $outFile
   close $in
}
# --------------------------------------------------------------------
# End of GetMapData procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: ReadAndDumpData
# --------------------------------------------------------------------
#
proc ReadAndDumpData { in fName txtFile param loc colNo outname } {

   global skipTest
   
   set found 0
   set header "    Date      Time"
   while {!$found && ([gets $in line] != -1)} {
      set len [string length $header]
      if {[string equal -length $len $header $line]} {
         set found 1
      }
   }
   
   if {!$found} {
      ErrMsg "Cannot find string '     Date      Time' in $txtFile"
      set skipTest 1
      return
   }
   
   # Skip next line
   gets $in line
   while {([string match "*(*" $line] == 1) && ([gets $in line] != -1)} {
      # The data part has not been reached yet, so read the next line
      gets $in line
   }
   # This is the data part
   set out [open $outname w]
   incr colNo
   while {[gets $in line] != -1} {
      puts $out "[lindex $line 0] [lindex $line 1] [lindex $line $colNo]"
   }
   close $out
}
# --------------------------------------------------------------------
# End of ReadAndDumpData procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: GetHisData
# --------------------------------------------------------------------
#
proc GetHisData { projectDir refCaseNum hisPath txtFile param loc outFile } { 

   global skipTest

   set in [open $txtFile r]
   set nLocs [GetNoOfLocations $in $txtFile]
   if {$skipTest} {
      close $in
      return
   }

   # Look for the parameter
   set parmNo [FindParameter $in $txtFile $param]
   if {$skipTest} {
      close $in
      return
   }

   if {$parmNo <= 0} {
      # Parameter not found in his file. Try hia file
      set parmNo [GetParmNumFromHiaFile $projectDir $refCaseNum \
                                             "[file rootname $hisPath].hia" $param]
   }
   if {$skipTest} {
      close $in
      return
   }
   
   if {$parmNo <= 0} {
      ErrMsg "Parameter not found: '$param'"
      set skipTest 1
      close $in
      return
   }   

   set found 0
   while {(!$found) && ([gets $in line] != -1)} {
      if {[regexp " Parameter *$parmNo:.*" $line]} {
         set found 1
      }
   }
   if {!$found} {
      # Programming error: parameter was found before in
      # either FindParameter or GetParmNumFromHiaFile
      ErrMsg "Parameter not found: '$param'"
      set skipTest 1
      close $in
      return
   }   

   # Get the locations
   set tokens {}
   set found 0
   while {!$found && ($nLocs > 0) && ([gets $in line] != -1) &&   \
          ([string equal -length 13 $line "     Location"] == 1)} {
      set tokens [split $line ":"]
      set location [string map {" " ""} $loc]
      set token [string map {" " ""} [string trim [lindex $tokens 1]]]
      if {[string equal -nocase $token $location]} {
         set found 1
      }
      incr nLocs -1
   }
   if {$found} {
      set col [lindex $tokens 2]
      scan $col " Column%d" colNo
   } else {   
      # Location not found. Try hia file
      set colNo [GetLocationFromHiaFile $projectDir $refCaseNum "[file rootname $hisPath].hia" $loc]
      if {$colNo < 1} {
         ErrMsg "Location not found: '$loc'"
         close $in
         set skipTest 1
         return
      }
   }

   ReadAndDumpData $in $hisPath $txtFile $param $loc $colNo $outFile
   close $in
}
# --------------------------------------------------------------------
# End of GetHisData procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: produceFixFile
# --------------------------------------------------------------------
#
proc produceFixFile { parameter location refseries newseries fixName } {

   set file1 [open $refseries "r"]
   set file2 [open $newseries "r"]
   set fixOut [open $fixName "w"]

   puts $fixOut "Differences"
   puts $fixOut ""
   puts $fixOut "Parameter                Location                 Date       Time          SOBEK2            SOBEK3            Diff"

   set lineno 0
   while {[gets $file1 line1] >= 0} {
      incr lineno
      if {([gets $file2 line2] >= 0)} {
         set tokens [split $line1 " "]
         set date1 [split [lindex $tokens 0] "-"]
         set time1 [lindex $tokens 1]
         set val1 [lindex $tokens 2]
         set tokens [split $line2 " "]
         set date2 [split [lindex $tokens 0] "-"]
         set time2 [lindex $tokens 1]
         set val2 [lindex $tokens 2]
         if {([string equal $date1 $date2]) && ([string equal $time1 $time2])} {
            set diff [expr ${val1}-${val2}]
            set date1 [lindex $tokens 0]
            puts $fixOut [format "%-25s%-25s%s %-13s %+8.6e    %+8.6e    %+8.6e" $parameter $location $date1 $time1 $val1 $val2 $diff]
         } else {
            puts "Date/Time does not equal: $date1;$time1  $date2;$time2"
         }   
      }
   }
   close $fixOut
   close $file1
   close $file2
}
# --------------------------------------------------------------------
# End of produceFixFile procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: CompareParLoc
# --------------------------------------------------------------------
#
proc CompareParLoc {analDir refCaseNum newCaseNum datFile parameter location maxdiff} {

   global skipTest
   global log
   global logcase
   global scriptspath
   global binDir


   global file_max_abs_diff
   global file_max_rel_diff
   global file_max_mix_diff
   
   global unacceptable
   global makeplots
   global msgBuild
      
   set par [string map {" " "" "/" ""} $parameter]
   set loc [string map {" " "" "/" ""} $location]
   set baseName [file join $analDir "${par}_$loc"]
   set datName [file join $analDir [file rootname $datFile]]
   
   # makeplots == 0 then no plots are made
   set makeplots 1
   
   if {[string equal -nocase [file extension $datFile] ".his"]} {
      # Get the Parameter - location series for *.his files
      set hisPath [file join $analDir $refCaseNum $datFile]
      set txtPath ${datName}_his_ref.txt
      set refseries ${baseName}.ref
      GetHisData $analDir $refCaseNum $hisPath $txtPath $parameter $location $refseries
      if {$skipTest} return

      # Get the Parameter - location series for *.his files
      set hisPath [file join $analDir $newCaseNum $datFile]
      set txtPath ${datName}_his_new.txt
      set newseries ${baseName}.new
      GetHisData $analDir $refCaseNum $hisPath $txtPath $parameter $location $newseries
      if {$skipTest} return

   } else {
      # Get the Parameter - location series for *.map files
      set mapPath [file join $analDir $refCaseNum $datFile]
      set txtPath ${datName}_map_ref.txt
      set refseries ${baseName}.ref
      GetMapData $analDir $refCaseNum $mapPath $txtPath $parameter $location $refseries
      if {$skipTest} return
      
      # Get the Parameter - location series for *.map files
      set mapPath [file join $analDir $newCaseNum $datFile]
      set txtPath ${datName}_map_new.txt
      set newseries ${baseName}.new
      GetMapData $analDir $refCaseNum $mapPath $txtPath $parameter $location $newseries
      if {$skipTest} return
   }
   
   # Compare the files
   set retval [NumComp::NumComp $refseries $newseries -out $logcase -sum $log]
   
   if {$retval} {
      set fixName "${baseName}.fix"
      produceFixFile $parameter $location $refseries $newseries $fixName
      set testNum [file rootname $analDir]
      set round [open "testround.csv" "a"]
      
      if {$file_max_abs_diff > $maxdiff} {
         append msgBuild "\n ERROR: difference too big, datFile: $datFile, parameter: $parameter, location: $location, difference: $file_max_abs_diff "
         set unacceptable 2

         # Create Semaphore File to detect failing test if not exists yet
         if {![file exists $analDir\\HasDiff.txt]} {
            set hasDiff [open $analDir\\HasDiff.txt "w"]
            puts -nonewline $hasDiff "Differences Found"
            close $hasDiff
         }
         
         if {$makeplots != 0} {

            set pythonScript [file join $binDir MakePlot.py]
            set command "python $pythonScript $fixName"
            eval "exec $command"

            # Copy the created *.png files to the analyses directory, and delete them
            set copyfiles [glob -dir [pwd] *.png]
            foreach singlefile $copyfiles {
               file copy -force $singlefile [file join $analDir]
               file delete -force $singlefile
            }
         }
         
         set homedir [pwd]
         cd $homedir

      }
      puts $round [format "%s,%8.6e,%8.6e,%8.6e,%s" \
                   $testNum $file_max_abs_diff $file_max_rel_diff \
                   $file_max_mix_diff $fixName]
      close $round
   }
   
   if {[string equal -nocase [file extension $datFile] ".his"]} {
   # Delete the *.ref and *.new files for *.his data files, because ReaHis
   # produces more complete information than ReaMap does
      file delete -force $refseries $newseries
   }
   
   return $retval
}
# --------------------------------------------------------------------
# End of CompareParLoc procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: RunDiff
# --------------------------------------------------------------------
#
proc RunDiff { analDir refCaseNum newCaseNum datFile parameter location maxdiff} {

   global skipTest
   global diffs
   global dtaIndex
   global scriptspath
   
   global unacceptable

   if {$refCaseNum == $newCaseNum} {
      ErrMsg "New case and reference case are the same!"
      set skipTest 1
      return
   }

   if {[string equal $parameter "*"]} {
      set parameters [GetAllParameters $analDir $datFile]
      if {$skipTest} return
   } else {
      lappend parameters $parameter
   }

   if {[string equal $location "*"]} {
      set locations [GetAllLocations $analDir $datFile]
      if {$skipTest} return
   } else {
      lappend locations $location
   }

   set differ 0
   foreach parameter $parameters {
      foreach location $locations {
         set retVal [CompareParLoc $analDir $refCaseNum $newCaseNum \
                                   $datFile $parameter $location $maxdiff]
         if {$skipTest} break
         if {$retVal == 1} {
            set differ 1
            set diffs [lreplace $diffs $dtaIndex $dtaIndex 1]
         } 
      }  
   }

   if {$unacceptable == 2} {
      set differ 2
   }
   return $differ
}
# --------------------------------------------------------------------
# End of RunDiff procedure
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: CheckResults
# --------------------------------------------------------------------
#
proc CheckResults { analDir elements refCaseNum newCaseNum } {

   global skipTest
   global logcase
   global datList
   global abstol
   global reltol
   global dtaIndex
   global scriptspath
   
   global msgBuild
   global unacceptable
   
   # Diff the results
   LogMsg "Checking results"
   set differ 0
   set unacceptable 1
   set msgBuild ""

   ConvertDataFilesToAscii
   if {$skipTest} return
   foreach {datFile parameter location abstol reltol} $elements {
      set maxdiff $abstol
      
      set dtaIndex 0
      set found 0
      foreach f $datList {
         if {[string equal $datFile $f]} {
            set found 1
            break
         }
         incr dtaIndex
      }
      if {!$found} {
         LogMsg "Data file not found"
         error "Data file not found"
      }

      set retVal [RunDiff $analDir $refCaseNum $newCaseNum $datFile $parameter $location $maxdiff]
      if {$skipTest} {
         LogMsg "ERROR: No *.his or *.map file comparison, check testcase cnf file to check data and location names"
         append msgBuild "\n ERROR: No *.his or *.map file comparison, check testcase cnf file to check data and location names" 
         break
      }
      if {$retVal == 1} {
         set differ 1
      } elseif {$retVal == 2} {
         set differ 2
      }
   }
   
   RemoveIntermediateFiles
   update
   if {$skipTest} return
   
   if { $differ == 1 } {
      LogMsg "Differences found, but acceptable!"
   } elseif { $differ == 2 } {
      LogMsg "Differences found, unacceptable!"
      #set projDir [file nativename $projDir]
      #puts "##teamcity\[testFailed name='$projDir' message='$msgBuild' details='$msgBuild'\]"
      set skipTest 1
   } else {
      LogMsg "No differences"
   }
   if {[info exists logcase]} {
      LogMsg "Finished"
      LogMsg "========================================================================="
      close $logcase
      unset logcase
   }
   update
}
# --------------------------------------------------------------------
# End of CheckResults procedure
# --------------------------------------------------------------------

}
# --------------------------------------------------------------------
# End of namespace
# --------------------------------------------------------------------
