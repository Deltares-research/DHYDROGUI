# DOC
#
# numcomp.tcl - Script to report the numerical differences between
#               text files
#
# Copyright (C) 2000 Arjen Markus
#
#
# General information:
#
# Numcomp compares files line by line. Unlike utilities like diff or fc
# it does know about numbers and allows differences in both format and value
# between corresponding numbers.
# Absolute and relative tolerances are global parameters.
# It reports those lines that are different:
# - the text is diferent
# - the numbers differ by more than a certain fraction.
#
# Usage:
# numcomp [-concise|-silent] [-out outfile] \
#         [-report filename] file1 file2
#
# -concise:
#    Only report the first fifty lines that are different
#    Default: report all lines that are different
# -silent:
#    Only return a zero or a one, do not list the differences
# -out:
#    Argument that follows is a handle to an opened file
# -report:
#    Argument that follows is the name of a file to be opened
#
# version 0.1: initial version, october 1999
# version 0.2: revised - provide two modes, april 2000
#
# ENDDOC
#
#  $Author$
#  $Date$
#  $Source$
#

# --------------------------------------------------------------------
# Namespace: NumComp
# --------------------------------------------------------------------
package provide NumComp 0.2

namespace eval ::NumComp {

namespace export NumComp
variable threshold      50000
variable noreport       0
variable output         stdout
variable sumout         stdout
variable header         ""
variable line_max_abs_diff       0.0
variable line_max_abs_diff_rel   0.0
variable line_max_abs_diff_x     0.0
variable line_max_abs_diff_y     0.0
variable line_max_rel_diff       0.0
variable line_max_rel_diff_abs   0.0
variable line_max_rel_diff_x     0.0
variable line_max_rel_diff_y     0.0
variable line_max_mix_diff       0.0
variable line_max_mix_diff_x     0.0
variable line_max_mix_diff_y     0.0
variable file_max_abs_diff       0.0
variable file_max_abs_diff_rel   0.0
variable file_max_abs_diff_x     0.0
variable file_max_abs_diff_y     0.0
variable file_max_abs_diff_line  0
variable file_max_rel_diff       0.0
variable file_max_rel_diff_abs   0.0
variable file_max_rel_diff_x     0.0
variable file_max_rel_diff_y     0.0
variable file_max_rel_diff_line  0
variable file_max_mix_diff       0.0
variable file_max_mix_diff_x     0.0
variable file_max_mix_diff_y     0.0
variable file_max_mix_diff_line  0
variable max_neg                 0.0
variable max_pos                 0.0
variable min_neg                 0.0
variable min_pos                 0.0
variable is_zero                 "false"

# --------------------------------------------------------------------
#   Procedure: SetRange
#   Author:    Adri Mourits
#   Purpose:   Checks if value lies in the range
#   Context:   Called from CompareLines, result used in CompareFiles
#   Summary:
#              If value is positive, check whether it is smaller than
#              minimum_positive and whether it is bigger than
#              maximum_positive. Analogue when value is negative.
#              Else value is zero
#   Note:
#              The max and min values are initialized in CompareFiles
#              The first four lines of Tekal files (generated out of
#              Nefis files) do not contain parameter values and should
#              be skipped
#   Arguments:
#   value      a value
#   lineno     linenumber
#   Returns:
#   Nothing
# --------------------------------------------------------------------
#
proc SetRange { value lineno} {
   variable max_neg
   variable max_pos
   variable min_neg
   variable min_pos
   variable is_zero

   #skip when line number < 5 (tekal header)
   if { $lineno < 5 } {
      return
   }
   if { $value > 0 } {
      if { $value > $max_pos } {
         set max_pos $value
      }
      if { $value < $min_pos } {
         set min_pos $value
      }
   } elseif { $value < 0 } {
      if { $value < $max_neg } {
         set max_neg $value
      }
      if { $value > $min_neg } {
         set min_neg $value
      }
   } else {
   set is_zero "true"
   }
}

# --------------------------------------------------------------------
#   Procedure: CompareLines
#   Author:    Arjen Markus
#   Purpose:   Compare two lines and report if they are different
#   Context:   Used by CompareFiles
#   Summary:
#              Loop over all words in the first line and
#              compare with the corresponding words in the
#              second line. Calculate the relative difference
#              if the words are actually numbers.
#   Note:
#              Numbers like 5D+00 are not recognised
#              If the lines do not have the same number of
#              words, this is regarded as a difference.
#   Arguments:
#   line1      The first line
#   line2      The second line
#   lineno     line number (needed for SetRange)
#   Returns:
#   0          lines are different
#   1          lines are considered equal
#   -1         lines have different number of items
# --------------------------------------------------------------------
#
proc CompareLines { line1 line2 lineno} {
   global abstol
   global reltol
   variable line_max_abs_diff       0.0
   variable line_max_abs_diff_rel   0.0
   variable line_max_abs_diff_x     0.0
   variable line_max_abs_diff_y     0.0
   variable line_max_rel_diff       0.0
   variable line_max_rel_diff_abs   0.0
   variable line_max_rel_diff_x     0.0
   variable line_max_rel_diff_y     0.0
   variable line_max_mix_diff       0.0
   variable line_max_mix_diff_x     0.0
   variable line_max_mix_diff_y     0.0

   set nowords1 [ llength $line1 ]
   set nowords2 [ llength $line2 ]

   if { $nowords1 != $nowords2 } {
      set equal -1
   } else {
      set idx   0
      set equal 1
      foreach substr1 $line1 {
         set handled 0
         set substr2 [ lindex $line2 $idx ]

         if { [ string is double $substr1 ] } {
            SetRange $substr1 $lineno
            if { [ string is double $substr2 ] } {
               SetRange $substr2 $lineno
               set handled  1
               set abs_diff [ expr abs($substr1  - $substr2) ]
               set abs_sum  [ expr abs($substr1) + abs($substr2) ]
               if { $abs_sum != 0.0 } {
                  set rel_diff [ expr 2.0 * $abs_diff / $abs_sum ]
               } else {
                  set rel_diff 0.0
               }
               set rel_tol  [ expr $abs_sum * 0.5 * $reltol   ]
               set abs_rel_tol [ expr $rel_tol + $abstol ]
               # Numbers x and y differ when:
               # |x-y| > abstol + 0.5*(|x|+|y|)*reltol
               if { $abs_diff > $abs_rel_tol } {
                  set equal 0
                  set pos_abs_rel_tol [ expr {($abs_rel_tol > 1.0e-10) ? $abs_rel_tol : 1.0e-10} ]
                  set mix_diff [ expr $abs_diff / $pos_abs_rel_tol ]
                  if { $mix_diff > $line_max_mix_diff } {
                     set line_max_mix_diff $mix_diff
                     set line_max_mix_diff_x [ expr $substr1 ]
                     set line_max_mix_diff_y [ expr $substr2 ]
                  }
               }
               if { $abs_diff > $line_max_abs_diff } {
                  set line_max_abs_diff $abs_diff
                  set line_max_abs_diff_rel $rel_diff
                  set line_max_abs_diff_x [ expr $substr1 ]
                  set line_max_abs_diff_y [ expr $substr2 ]
               }
               if { $rel_diff > $line_max_rel_diff } {
                  set line_max_rel_diff $rel_diff
                  set line_max_rel_diff_abs $abs_diff
                  set line_max_rel_diff_x [ expr $substr1 ]
                  set line_max_rel_diff_y [ expr $substr2 ]
               }
            }

         }
         if { ! $handled } {
            if { [ string compare $substr1 $substr2 ] != 0 } {
               set equal 0
            }
         }

         # Check all items, do not just break off after the first
         # if { ! $equal } {
         #    break
         # }

         incr idx
      }
   }

   return $equal
}


# --------------------------------------------------------------------
#   Procedure: CompareFiles
#   Author:    Arjen Markus
#   Purpose:   Compare two files by reading them line by line
#   Context:   Used by NumComp
#   Summary:
#              Read a line from both files. Compare them and
#              if they are different, write them out (as long as
#              the number of output lines remains below the threshold).
#   Arguments:
#   infile1    The first fileid
#   infile2    The second fileid
#   Returns:
#              Nothing
# --------------------------------------------------------------------
#
proc CompareFiles { infile1 infile2 } {

   global file_max_abs_diff
   global file_max_rel_diff
   global file_max_mix_diff

   variable threshold
   variable noreport              0
   variable output
   variable sumout
   variable header
   variable line_max_abs_diff
   variable line_max_abs_diff_rel
   variable line_max_abs_diff_x
   variable line_max_abs_diff_y
   variable line_max_rel_diff
   variable line_max_rel_diff_abs
   variable line_max_rel_diff_x
   variable line_max_rel_diff_y
   variable line_max_mix_diff
   variable line_max_mix_diff_x
   variable line_max_mix_diff_y
#   variable file_max_abs_diff       0.0
   variable file_max_abs_diff_rel   0.0
   variable file_max_abs_diff_x     0.0
   variable file_max_abs_diff_y     0.0
   variable file_max_abs_diff_line  0
#   variable file_max_rel_diff       0.0
   variable file_max_rel_diff_abs   0.0
   variable file_max_rel_diff_x     0.0
   variable file_max_rel_diff_y     0.0
   variable file_max_rel_diff_line  0
#   variable file_max_mix_diff       0.0
   variable file_max_mix_diff_x     0.0
   variable file_max_mix_diff_y     0.0
   variable file_max_mix_diff_line  0
   variable max_neg               -1.0e-50
   variable max_pos                1.0e-50
   variable min_neg               -1.0e+50
   variable min_pos                1.0e+50
   variable is_zero               "false"

   set file_max_abs_diff 0.0
   set file_max_rel_diff 0.0
   set file_max_mix_diff 0.0
   
   set lineno     0
   set errmessage ""
   set precision  "%4.2e"
   set precision2 "%+8.6e"

   while { [ gets $infile1 line1 ] >= 0 } {
      incr lineno
      if { [ gets $infile2 line2 ] >= 0 } {
         set equal [ CompareLines $line1 $line2 $lineno]
         if { $line_max_abs_diff > $file_max_abs_diff } {
            set file_max_abs_diff $line_max_abs_diff
            set file_max_abs_diff_rel $line_max_abs_diff_rel
            set file_max_abs_diff_x $line_max_abs_diff_x
            set file_max_abs_diff_y $line_max_abs_diff_y
            set file_max_abs_diff_line $lineno
         }
         if { $line_max_rel_diff > $file_max_rel_diff } {
            set file_max_rel_diff $line_max_rel_diff
            set file_max_rel_diff_abs $line_max_rel_diff_abs
            set file_max_rel_diff_x $line_max_rel_diff_x
            set file_max_rel_diff_y $line_max_rel_diff_y
            set file_max_rel_diff_line $lineno
         }
         if { $equal == 0 } {
            incr noreport
            if { $noreport < $threshold } {
               set lformat "     Line %5d: max abs diff: $precision\
                            (rel diff: $precision) x: $precision2 y: $precision2"
               puts $output [format $lformat $lineno\
                               $line_max_abs_diff $line_max_abs_diff_rel\
                               $line_max_abs_diff_x $line_max_abs_diff_y]
               set lformat "     Line %5d: max rel diff: $precision\
                            (abs diff: $precision) x: $precision2 y: $precision2"
               puts $output [format $lformat $lineno\
                               $line_max_rel_diff $line_max_rel_diff_abs\
                               $line_max_rel_diff_x $line_max_rel_diff_y]
               set lformat "     Line %5d: max mix fact: $precision                       \
                            x: $precision2 y: $precision2"
               puts $output [format $lformat $lineno $line_max_mix_diff\
                               $line_max_mix_diff_x $line_max_mix_diff_y]
               puts $output "          a< $line1"
               puts $output "          b< $line2"
            }
            if { $line_max_mix_diff > $file_max_mix_diff } {
               set file_max_mix_diff $line_max_mix_diff
               set file_max_mix_diff_x $line_max_mix_diff_x
               set file_max_mix_diff_y $line_max_mix_diff_y
               set file_max_mix_diff_line $lineno
            }
         }
         if { $equal == -1 } {
            set errmessage "Files contain different number of items"
            incr noreport
            if { $noreport < $threshold } {
               puts $output "     Line $lineno: different number of items"
               puts $output "          a< $line1"
               puts $output "          b< $line2"
            }
         }
         if { $noreport > $threshold } {
            # It makes no sense to continue
            return
         }
      } else {
         set errmessage "Files contain different number of items"
         puts $output "     Note:"
         puts $output "     Second file is shorter than first!"
      }
   }

   # Try to read another line from the second file

   if { [ gets $infile2 line2 ] >= 0 } {
      set errmessage "Files contain different number of items"
      puts $output "     Note:"
      puts $output "     Second file is longer than first!"

   }

   if { $errmessage == "" } {
      if { $noreport == 0 } {
         #puts $sumout "no differences found"
      } else {
         if { $header != "" } {
            puts $sumout "\n$header"
         }
         # Print range
         # Start by placing strings for values
         set range0 "range: \[ max_neg , min_neg \],\[x\],\[ min_pos , max_pos \]"
         # Replace string max_neg by its value (or . when it still has its default value)
         if { $max_neg == -1.00e-050 } {
            regsub max_neg $range0 "    .     " range1
         } else {
            regsub max_neg $range0 [format "$precision" $max_neg] range1
         }
         # Replace string min_neg by its value (or . when it still has its default value)
         if { $min_neg == -1.00e+050 } {
            regsub min_neg $range1 "    .     " range2
         } else {
            regsub min_neg $range1 [format "$precision" $min_neg] range2
         }
         # Replace string min_pos by its value (or . when it still has its default value)
         if { $min_pos ==  1.00e+050 } {
            regsub min_pos $range2 "    .     " range3
         } else {
            regsub min_pos $range2 [format "$precision" $min_pos] range3
         }
         # Replace string max_pos by its value (or . when it still has its default value)
         if { $max_pos ==  1.00e-050 } {
            regsub max_pos $range3 "    .     " range4
         } else {
            regsub max_pos $range3 [format "$precision" $max_pos] range4
         }
         # Replace string x by 0 (or . when no zero value found)
         if { $is_zero == "true" } {
            regsub x $range4 "0" range
         } else {
            regsub x $range4 "." range
         }
         puts $sumout $range
         # Print maximum absolute difference (plus related values)
         set lformat "max abs diff: $precision (rel diff: $precision)\
                       \[Line %5d\] x: $precision2 y: $precision2"
         puts $sumout [format $lformat $file_max_abs_diff\
                         $file_max_abs_diff_rel $file_max_abs_diff_line\
                         $file_max_abs_diff_x $file_max_abs_diff_y]
         # Print maximum relative difference (plus related values)
         set lformat "max rel diff: $precision (abs diff: $precision)\
                       \[Line %5d\] x: $precision2 y: $precision2"
         puts $sumout [format $lformat $file_max_rel_diff\
                         $file_max_rel_diff_abs $file_max_rel_diff_line\
                         $file_max_rel_diff_x $file_max_rel_diff_y]
         # Print maximum mixed difference (plus related values)
         set lformat "max mix fact: $precision                      \
                       \[Line %5d\] x: $precision2 y: $precision2"
         puts $sumout [format $lformat $file_max_mix_diff\
                         $file_max_mix_diff_line\
                         $file_max_mix_diff_x $file_max_mix_diff_y]
      }
   } else {
      puts $sumout $errmessage
   }

   return $equal
}

# --------------------------------------------------------------------
#   Procedure: NumCompare
#   Author:    Arjen Markus
#   Purpose:   Compare the two files
#   Context:   Used by main code
#   Summary:
#              Open the files. Compare them via CompareFiles and
#              close them. Return if there is a difference or not
#   Arguments:
#   filename1  Name of the first file
#   filename2  Name of the second file
#   Returns:
#   0          If no differences encountered
#   1          If differences encountered or a file could not
#              be opened
# --------------------------------------------------------------------
#
proc NumCompare { filename1 filename2 } {
   variable threshold
   variable noreport
   variable output

# Ignore the options for the moment

   # Open the input files

   set error 0
   if [ catch { open $filename1 "r" } file1 ] {
      puts stderr "Cannot open $filename1: $file1"
      set error 1
   }
   if [ catch { open $filename2 "r" } file2 ] {
      puts stderr "Cannot open $filename2: $file2"
      set error 1
   }

   if { $error == 0 } {
      puts $output "File a: $filename1"
      puts $output "File b: $filename2"
      CompareFiles $file1 $file2
      if { $noreport == 0 } {
         puts $output "     No differences found"
      } else {
        set error 1
      }
   }

   close $file1
   close $file2

   return $error
}

# --------------------------------------------------------------------
#   Procedure: GetArgs
#   Author:    Arjen Markus
#   Purpose:   Analyse the command line and compare the two files
#   Context:   Used by main code
#   Summary:
#              Analyse the command line to get the options
#              Return the names of the files via the arguments
#   Arguments:
#   filename1  Reference to name of the first file
#   filename2  Reference to name of the second file
#   Returns:
#              Nothing
# --------------------------------------------------------------------
#
proc GetArgs { optlist filename1 filename2 } {
   variable threshold
   variable output
   variable sumout
   variable header
   upvar $filename1 name1
   upvar $filename2 name2

   set next "filename1"
   set noopt [llength $optlist]
   for { set i 0 } { $i < $noopt } { incr i } {
      set arg [lindex $optlist $i ]
      switch -- $arg {
         "-concise"   { set threshold 50
                        set next      "filename1" }
         "-silent"    { set threshold  0
                        set next      "filename2" }

         "-out"       { set next      "output" }

         "-sum"       { set next      "sumout" }

         "-report"    { set next      "report" }

         "-header"    { set next      "header" }

         default      { switch $next {
                           "filename1" { set name1     $arg
                                         set next      "filename2" }
                           "filename2" { set name2     $arg
                                         set next      ""   }
                           "output"    { set output    $arg
                                         set next      "filename1"   }
                           "sumout"    { set sumout    $arg
                                         set next      "filename1"   }
                           "report"    { set output    [ open $arg "w" ]
                                         set next      "filename1"   }
                           "header"    { set header    $arg
                                         set next      "filename1"   }
                           default     { set next      "" }
                         }
                      }
      }
   }
   return
}

# --------------------------------------------------------------------
#   Procedure: NumComp
#   Author:    Arjen Markus
#   Purpose:   Public procedure (include all options)
#   Context:   Used by main code or by applications
#   Summary:
#              Parse the arguments, call the comparison procedure
#   Arguments:
#              (see above description)
#   Returns:
#       0      No differences
#       1      Differences or error
# --------------------------------------------------------------------
#
proc NumComp { args } {
   global   abstol
   global   reltol
   variable output

   GetArgs $args filename1 filename2
   
   set precision "%4.2e"
   set lformat "Rel tolerance: $precision Abs tolerance: $precision"
   puts $output [format $lformat $reltol $abstol]
   set retval [ NumCompare $filename1 $filename2 ]
   if { $retval } {
      puts $output "Files differ"
   }

   return $retval
}

}
# --------------------------------------------------------------------
# End of namespace
# --------------------------------------------------------------------

# --------------------------------------------------------------------
#   Procedure: main code
#   Author:    Arjen Markus
#   Purpose:   Call the comparison procedure, unless sourced
#   Context:   --
#   Summary:
#              Analyse the command line to get the options
#              Compare the files via NumComp
#   Arguments (command-line):
#   filename1  Name of the first file
#   filename2  Name of the second file
#   options
#   Returns:
#   0          If no differences encountered
#   1          If differences encountered or a file could not
#              be opened
# --------------------------------------------------------------------
#
# Check if the script is called directly. Otherwise we need to do
# nothing
#
namespace import ::NumComp::*
global argv0
global argv
if { [ info script ] == $argv0 } {
   global filename1
   global filename2
   eval NumComp $argv
}
