#!/usr/bin/python3
"""
Update all AssemblyInfo.cs.svn contained in a folder structure by incrementing the
specified version step.

This script is responsible for updating the AssemblyInfo.cs.svn files. It detects the
[assembly: AssemblyVersion("major.minor.patch.$WCREV$")]
[assembly: AssemblyFileVersion("major.minor.patch.$WCREV$")]

lines in the specified fields, and increments them, depending on the arguments. By
default it will increment with the smallest step (the z value). These values will be
updated and written back to the file.

The script can be ran as follows:

python3 assembly_info_incrementer [--root|-r <path_to_root_folder_containing_files>]
                                  [--major|--minor|--patch]
                                  [--keep-smaller-labels|-ksl]

--root <path_to_root_folder_contaning_files>: Set the path in which to search for
                                              AssemblyInfo.cs.svn to update. Any files
                                              recursively found in this directory are
                                              incremented.
                                              This will default to the current working
                                              directory, if no path is specified.
--major|--minor|--patch: The version label to update. If multiple are defined, the
                         largest value will be incremented.
                         This will default to patch, if no version label is specified.
--keep-smaller-labels: This will preserve the smaller version labels if specified.
                       If not specified it will set the smaller version labels back to
                       zero.

These arguments cane be passed in any order, other arguments are ignored.
"""
__author__ = "Maarten Tegelaers"
__copyright__ = "Copyright 2018"
__version__ = "0.1"
__maintainer__ = "Maarten Tegelaers"
__email__ = "Maarten.Tegelaers@deltares.nl"
__status__ = "Development"

from pathlib import *
import re
import os
import sys


FILE_NAME = "AssemblyInfo.cs.svn"

# Regular Expressions
# ------------------------------------------------------------------------------
p_int = r"0|(?:[1-9][0-9]*)"
pattern_version = r'^\[assembly: AssemblyVersion\("(?P<major>{0}).(?P<minor>{0}).(?P<patch>{0}).\$WCREV\$"\)\]$'.format(p_int)
pattern_file_version = r'^\[assembly: AssemblyFileVersion\("(?P<major>{0}).(?P<minor>{0}).(?P<patch>{0}).\$WCREV\$"\)\]$'.format(p_int)

prog_version = re.compile(pattern_version)
prog_file_version = re.compile(pattern_file_version)

# Templates
# ------------------------------------------------------------------------------
template_version = '[assembly: AssemblyVersion("{}.{}.{}.$WCREV$")]\n'
template_file_version = '[assembly: AssemblyFileVersion("{}.{}.{}.$WCREV$")]\n'


# Methods
# ------------------------------------------------------------------------------
def find_assembly_info_files(path: Path) -> list:
    """
    Find the paths to all FILE_NAME within the specified path.
    """
    return path.glob("**/{}".format(FILE_NAME))


def increment_version_in_file(path: Path, version: str, reset_smaller_labels: bool) -> None:
    if not (path.exists() and path.is_file()):
        print("  Ignoring: {}".format(str(path)))
        return

    print("  Updating: {}".format(str(path)))

    with path.open() as f:
        lines = f.readlines()


    # Check if the file contains the right values that we are interested in
    for i in range(len(lines)):
        m = prog_version.match(lines[i])
        template = template_version

        if not m:
            m = prog_file_version.match(lines[i])
            template = template_file_version

            if not (m):
                continue

        major = int(m.group("major"))
        minor = int(m.group("minor"))
        patch = int(m.group("patch"))

        if version == "major":
            major += 1

            if reset_smaller_labels:
                minor = 0
                patch = 0
        elif version == "minor":
            minor += 1

            if reset_smaller_labels:
                patch = 0
        else:
            patch += 1

        lines[i] = template.format(major, minor, patch)

    with path.open(mode='w') as f:
        f.write("".join(lines))


def run(root_path: Path, version: str, reset_smaller_labels: bool) -> None:
    """
    Run the assembly info incrementer comment with the given parameters.
    """
    if not (root_path.exists() and root_path.is_dir()):
        print("Provided path is not a valid directory.")
        return

    print("Finding files in {} ...".format(str(root_path)))
    filePaths = find_assembly_info_files(root_path)

    print("Updating files ...")
    for fp in filePaths:
        increment_version_in_file(fp, version, reset_smaller_labels)

    print("Done")


def print_help():
    help_output = (
"""
python3 assembly_info_incrementer [--root|-r <path_to_root_folder_containing_files>]
                                  [--major|--minor|--patch]
                                  [--keep-smaller-labels|-ksl]

--root <path_to_root_folder_contaning_files>: Set the path in which to search for
                                              AssemblyInfo.cs.svn to update. Any files
                                              recursively found in this directory are
                                              incremented.
                                              This will default to the current working
                                              directory, if no path is specified.
--major|--minor|--patch: The version label to update. If multiple are defined, the
                         largest value will be incremented.
                         This will default to patch, if no version label is specified.
--keep-smaller-labels: This will preserve the smaller version labels if specified.
                       If not specified it will set the smaller version labels back to
                       zero.

These arguments cane be passed in any order, other arguments are ignored.
""")
    print(help_output)


if __name__ == "__main__":
    if sys.argv[1] == "--help" or sys.argv[1] == "-h":
        print_help()

    else:
        next_is_root = False
        root_path = Path.cwd()
        version = "patch"
        reset_smaller_labels = True

        ignored_arguments = []

        for arg in sys.argv[1:]:
            if next_is_root:
                root_path = Path(arg)
                next_is_root = False
            elif arg == "--root" or arg == "-r":
                next_is_root = True
            elif arg == "--major":
                version = "major"
            elif arg == "--minor":
                version = "minor"
            elif arg == "--patch":
                version = "patch"
            elif arg == "--keep-smaller-labels" or arg == "-ksl":
                reset_smaller_labels = False
            else:
                ignored_arguments.append(arg)

        print("ignored the following arguments:\n{}".format(
            "\n".join(ignored_arguments)))

        run(root_path, version, reset_smaller_labels)
