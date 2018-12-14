#!/usr/bin/python3
"""
Add pdb files generated with DeltaShell framework to a specified NGHS repository

This script is responsible for generating the .pdb files of a DeltaShell
framework for an NGHS repository. It does so by first determining the revision
used by the NGHS repository. Then updating and compiling the DeltaShell
framework if required and finally replacing the current DeltaShell Framework lib
folders with the newly generated ones.

This script can either be run stand-alone or used as a library. In standalone
version it is ran as specified in the help:

python3 add_pdb.py --svn_root|-sr <path_to_NGHS_svn_root>
                   [--create_command|-cc]
                   [--rebuild_framework|-rf}
                   [--force_compile_framework|-fcf]
                   [--non_verbose|-nv]

--svn_root <path_to_NGHS_svn_root>: Path to the root folder of the NGHS
                                    repository to update
--create_command: Flag to create a .bat file instead of executing the
                  command.
--rebuild_framework: Flag to force rebuild the framework by deleting the
                     bin and package folders.
--force_compile_framework: Flag to force the compile step even if
                           the used revision number is equal to the
                           current revision number.
--non_verbose: Suppress (some) of the messages printed by this script.

These arguments can be passed in any order, other arguments are ignored.
"""
__author__ = "Maarten Tegelaers"
__copyright__ = "Copyright 2018"
__version__ = "0.2"
__maintainer__ = "Maarten Tegelaers"
__email__ = "Maarten.Tegelaers@deltares.nl"
__status__ = "Development"

import os
import sys
from pathlib import *
import shutil
import subprocess


# ------------------------------------------------------------------------------
# Constants
DEVENV_PATH = Path(r"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\devenv.com")

DELTASHELL_REPOSITORY = Path(r"D:\SVN\DeltaShell_1.3")
DELTASHELL_FRAMEWORK = DELTASHELL_REPOSITORY / Path("Framework")

NGHS_PACKAGES = Path("packages")
NGHS_FRAMEWORK_PACKAGE_NAME = "DeltaShell.Framework"

BAT_FILE_NAME = Path("add_framework_pdb.bat")


# ------------------------------------------------------------------------------
# Supporting Classes
class VersionInfo(object):
    """
    VersionInfo is a data object containing the relevant version information
    of the DeltaShell framework package of an SVN.
    """
    def __init__(self, package_name: str):
        """
        Construct a new VersionInfo object from the specified package_name.

        :param package_name: The name of the directory containing the
                             DeltaShell Framework.
        :type package_name: str
        """
        if (not package_name.startswith(NGHS_FRAMEWORK_PACKAGE_NAME)):
            raise Exception("Not a valid package name.")

        version_info = package_name.replace(
            NGHS_FRAMEWORK_PACKAGE_NAME, "")[1:].split(".")

        if (not len(version_info) == 4):
            raise Exception("Not a valid package name.")

        self._major_version = int(version_info[0])
        self._minor_version = int(version_info[1])
        self._smallest_version = int(version_info[2])
        self._revision = int(version_info[3])

    @property
    def revision(self) -> int:
        """ The revision of this DeltaShell package."""
        return self._revision

    @property
    def major_version(self) -> int:
        """ The major version of this DeltaShell package."""
        return self._major_version

    @property
    def minor_version(self) -> int:
        """ The minor version of this DeltaShell package."""
        return self._minor_version

    @property
    def smallest_version(self) -> int:
        """ The smallest version of this DeltaShell package."""
        return self._smallest_version

    @property
    def version_string(self) -> str:
        """
        The version string of this DeltaShell package:
        "<major_version>.<minor_version>.<smallest_version>"
        """
        return "{}.{}.{}".format(self.major_version,
                                 self.minor_version,
                                 self.smallest_version)

    @property
    def version_string_rev(self) -> str:
        """
        The version string and revision of this DeltaShell package:
        "<version_string>.<revision>"
        """
        return "{}.{}".format(self.version_string, self.revision)


# ------------------------------------------------------------------------------
# Script functions
def validate_svn_root(svn_repository_root: Path,
                      verbose=True) -> None:
    """
    Validate the obtained svn_repository_root

    :param svn_repository_root: The root folder of the repository of which the
                                version info is requested.
    :type svn_repository_root: Path
    """
    if verbose:
        print("Validating path...")

    if not svn_repository_root.exists():
        raise Exception("Provided SVN Repository path does not exist on disk.")

    if not svn_repository_root.is_dir():
        raise Exception("Provided SVN Repository path is not a directory.")

    p = subprocess.Popen(["svn", "info", str(svn_repository_root)],
                          stdout=subprocess.PIPE)
    output, error = p.communicate()
    output = output.decode("utf-8")

    if not ("Repository Root:" in output):
        raise Exception("Provided SVN Repository path is not a SVN Repository.")

    if verbose:
        print("Validated path!")


def determine_used_deltashell_version(svn_repository_root: Path,
                                      verbose=True) -> VersionInfo:
    """
    Determine the framework VersionInfo in the specified svn_repository.

    :param svn_repository_root: The root folder of the repository of which the
                                version info is requested.
    :type svn_repository_root: Path
    :param verbose: Whether to print additional debug information.
    :type verbose: bool
    :returns: A VersionInfo object containing the version and revision of the
              used DeltaShell framework.
    """
    if verbose:
        print("Determining version...")

    package_info_root = svn_repository_root / NGHS_PACKAGES

    if (not package_info_root.exists()):
        raise Exception("Cannot find package folder.")

    delta_shell_framework_files = list(package_info_root.glob(
        "{}*".format(NGHS_FRAMEWORK_PACKAGE_NAME)))

    if (len(delta_shell_framework_files) != 1):
        raise Exception("Cannot find DeltaShell.Framework package folder.")

    version_info = VersionInfo(delta_shell_framework_files[0].name)

    if verbose:
        print("Using version: {}!".format(version_info.version_string_rev))

    return version_info


def update_framework_to_version(revision : int,
                                verbose=True) -> bool:
    """
    Check out the DELTASHELL_REPOSITORY at the specified revision.

    :param revision: The revision at which the DELTASHELL_REPOSITORY will be
                     checked out
    :type revision: int
    :param verbose: Whether to print additional debug information.
    :type verbose: bool

    :returns: True when the framework needs to be recompiled, false otherwise
    """
    if verbose:
        print("Updating framework to rev. {}...".format(revision))

    # Calculate the current revision
    p = subprocess.Popen(["svn", "info", str(DELTASHELL_FRAMEWORK)],
                         stdout=subprocess.PIPE)
    output, error = p.communicate()
    output = output.decode("utf-8")

    current_framework_revision = -1
    for line in output.splitlines():
        key, val = line.split(sep=":", maxsplit=1)
        if key == "Revision":
            current_framework_revision = int(val)
            break
    else:
        raise Exception("Could not determine framework revision.")

    if current_framework_revision == revision:
        # Framework already at target revision, no need to update or compile.
        return False

    p = subprocess.call(["svn", "update",
                         "-r", "{}".format(revision),
                         str(DELTASHELL_FRAMEWORK)])
    return True


def compile_framework(rebuild_framework=False,
                      verbose=True) -> None:
    """
    Compile the framework at DELTASHELL_FRAMEWORK with debug settings

    :param rebuild_framework: Whether to rebuild the framework, if True then
                              the bin and packages folder will be deleted and
                              the nuget packages restored.
    :param verbose: Whether to print additional debug information.
    :type verbose: bool
    """
    if verbose:
        print("Compiling framework ...")

    solution_path = DELTASHELL_FRAMEWORK / Path("Framework.sln")
    if rebuild_framework:
        # Remove bin folder
        bin_folder_path = DELTASHELL_FRAMEWORK / Path("bin")
        if bin_folder_path.exists():
            shutil.rmtree(str(bin_folder_path))

        # Remove packages folder
        for folder_path in (DELTASHELL_FRAMEWORK / Path("packages")).glob("*"):
            p = subprocess.Popen(["svn", "info", str(folder_path)],
                                 stdout=subprocess.PIPE)
            output, error = p.communicate()
            output = output.decode("utf-8")

            if not ("Repository Root:" in output):
                shutil.rmtree(str(folder_path))

    # Restore packages
    subprocess.call(["nuget.exe",
                     "restore", str(solution_path),
                    ])

    # Compile framework
    subprocess.call([str(DEVENV_PATH),
                     str(solution_path),
                     "/Build", "Debug"])
    print("\nCompiled framework!")


def update_framework_in_svn(svn_repository_root: Path,
                            version_info: VersionInfo,
                            verbose=True) -> None:
    """
    Update the framework DeltaShell and Plugins with the compiled framework at
    DELTASHELL_FRAMEWORK / bin / Debug

    :param svn_repository_root: The root of the repository which will be updated
    :type svn_repository_root: Path
    :param version_info: The version info of the framework package
    :type version_info: VersionInfo
    :param verbose: Whether to print additional debug information.
    :type verbose: bool

    :post: | svn_repository_root / NGHS_PACKAGES /
           | NGHS_FRAMEWORK_PACKAGE_NAME.(version_info.version_stringrev)/
           | lib/net40/(plugins | DeltaShell) =
           | DELTASHELL_FRAMEWORK / bin / Debug / (plugins | DeltaShell)
    """
    if verbose:
        print("Updating SVN, {}, with compiled framework ...".format(
            str(svn_repository_root)))

    pdb_target_path = (svn_repository_root / NGHS_PACKAGES /
                       Path("{}.{}".format(NGHS_FRAMEWORK_PACKAGE_NAME,
                                           version_info.version_string_rev)) /
                       Path("lib/net40/"))
    pdb_target_plugin_path = pdb_target_path / Path("plugins")
    pdb_target_deltashell_path = pdb_target_path / Path("DeltaShell")

    pdb_source_path = DELTASHELL_FRAMEWORK / Path("bin/Debug")
    pdb_source_plugin_path = pdb_source_path / Path("plugins")
    pdb_source_deltashell_path = pdb_source_path / Path("DeltaShell")

    # Check if paths exist
    if (not pdb_target_path.exists()):
        raise Exception("Cannot find framework packages in NGHS SVN repository.")
    if (not (pdb_source_path.exists() and
             pdb_source_plugin_path.exists() and
             pdb_source_deltashell_path.exists())):
        raise Exception("Cannot find compiled framework packages in DeltaShell SVN repository.")

    # Remove old folders
    if pdb_target_plugin_path.exists():
        shutil.rmtree(str(pdb_target_plugin_path))
    if pdb_target_deltashell_path.exists():
        shutil.rmtree(str(pdb_target_deltashell_path))

    # Copy new folders
    shutil.copytree(str(pdb_source_plugin_path),
                    str(pdb_target_plugin_path))
    shutil.copytree(str(pdb_source_deltashell_path),
                    str(pdb_target_deltashell_path))

    if verbose:
        print("Updated SVN with compiled framework!")


def run(svn_repository_root: Path,
        rebuild_framework=False,
        force_compile_framework=False,
        verbose=True):
    """
    Update the framework package of the specified svn repository with compiled
    debug framework files.

    :param svn_repository_root: The root of the repository which will be updated
    :type svn_repository_root: Path
    :param rebuild_framework: Whether to rebuild the framework, if True then
                              the bin and packages folder will be deleted and
                              the nuget packages restored.
    :type rebuild_framework: bool
    :param force_compile_framework: Force the compile_framework to run, even if
                                    the framework is not updated to a different
                                    revision.
    :type force_compile_framework: bool
    :param verbose: Whether to print additional debug information.
    :type verbose: bool
    """
    # Validate provided path
    try:
        validate_svn_root(svn_repository_root=svn_repository_root,
                          verbose=verbose)
    except Exception as e:
        print("Could not validate provided path: {}".format(str(e)))
        return

    if verbose:
        print("")

    # Determine DeltaShellVersion
    try:
        version = determine_used_deltashell_version(
            svn_repository_root=svn_repository_root,
            verbose=verbose)
    except Exception as e:
        print("Could not determine DeltaShell revision: {}".format(str(e)))
        return

    if verbose:
        print("")

    # Update DeltaShell Package to the specified version.
    try:
        needs_to_be_recompiled = update_framework_to_version(version.revision,
                                                             verbose=verbose)
    except Exception as e:
        print("Could not update DeltaShell to revision: {}".format(str(e)))
        return

    if verbose:
        print("")

    # Compile DeltaShell Framework
    if needs_to_be_recompiled or force_compile_framework:
        try:
            compile_framework(rebuild_framework=rebuild_framework,
                              verbose=verbose)
        except Exception as e:
            print("Could not compile DeltaShell: {}".format(str(e)))
            return
    else:
        print("Framework already up to date!")


    if verbose:
        print("")

    # Update DeltaShell Package in SVN
    try:
        update_framework_in_svn(svn_repository_root=svn_repository_root,
                                version_info=version,
                                verbose=verbose)
    except Exception as e:
        print("Could not update DeltaShell Framework in SVN: {}".format(str(e)))
        return

    if verbose:
        print("Finished!")


def construct_cmd_bat(file_name: Path,
                      svn_repository: Path,
                      rebuild_framework: bool,
                      force_compile_framework: bool,
                      verbose: bool) -> None:
    """
    Construct a BAT_FILE_NAME in the specified repository, containing the
    command used to update the PDB.

    :param file_name: The name of the script
    :type file_name: Path
    :param svn_repository_root: The root of the repository in which the cmd is
                                created.
    :type svn_repository_root: Path
    :param rebuild_framework: Whether to rebuild the framework, if True then
                              the bin and packages folder will be deleted and
                              the nuget packages restored.
    :type rebuild_framework: bool
    :param force_compile_framework: Force the compile_framework to run, even if
                                    the framework is not updated to a different
                                    revision.
    :type force_compile_framework: bool
    :param verbose: Whether to print additional debug information.
    :type verbose: bool
    """
    print("Constructing {} file in {}".format(str(BAT_FILE_NAME), str(svn_repository)))
    try:
        if not file_name.exists():
            raise Exception("Script file does not exist.")
        validate_svn_root(svn_repository)

        bat_file_path = svn_repository / BAT_FILE_NAME
        if bat_file_path.exists():
            raise Exception(".bat file already exists in repository.")

        bat_cmd = '"{python_path}" "{file_path}" -sr "{svn_path}"{rb}{fcf}{nv}'.format(python_path=sys.executable,
                                                                                     file_path = str(file_name.resolve()),
                                                                                     svn_path=str(svn_repository.resolve()),
                                                                                     rb=(" -rb" if rebuild_framework else ""),
                                                                                     fcf=(" -fcf" if force_compile_framework else ""),
                                                                                     nv=(" -nv" if not verbose else ""))
        print("Generated cmd: {}".format(bat_cmd))

        with open(str(bat_file_path), 'w') as bath_file:
            bath_file.write(bat_cmd)

    except Exception as e:
        print("Could not construct .bat file: {}".format(str(e)))




def print_help():
    help_output = (
"""
add_pdb.py adds the PDB files to the specified NGHS svn repository.
These PDB files are created by compiling the DeltaShell framework,
specified within this script, at the revision used by the NGHS
repository.

It expects the following format to run:
python3 add_pdb.py --svn_root|-sr <path_to_NGHS_svn_root>
                   [--create_command|-cc]
                   [--rebuild_framework|-rf}
                   [--force_compile_framework|-fcf]
                   [--non_verbose|-nv]

--svn_root <path_to_NGHS_svn_root>: Path to the root folder of the NGHS
                                    repository to update
--create_command: Flag to create a .bat file instead of executing the
                  command.
--rebuild_framework: Flag to force rebuild the framework by deleting the
                     bin and package folders.
--force_compile_framework: Flag to force the compile step even if
                           the used revision number is equal to the
                           current revision number.
--non_verbose: Suppress (some) of the messages printed by this script.

These arguments can be passed in any order, other arguments are ignored.
"""
)
    print(help_output)


# ------------------------------------------------------------------------------
# Main
if __name__ == "__main__":
    # Determine user flags
    if len(sys.argv) <= 1:
        print_help()
    else:
        svn_root = None
        create_command = False
        rebuild_framework = False
        force_compile_framework = False
        verbose = True
        ignored_arguments = []

        next_is_root = False
        for arg in sys.argv[1:]:
            if next_is_root:
                svn_root = Path(arg)
                next_is_root = False
            elif arg == "--svn_root" or arg == "-sr":
                next_is_root = True
            elif arg == "--create_command" or arg == "-cc":
                create_command = True
            elif arg == "--rebuild_framework" or arg == "-rf":
                rebuild_framework = True
            elif arg == "--force_compile_framework" or arg == "-fcf":
                force_compile_framework = True
            elif arg == "--non_verbose" or arg == "-nv":
                verbose = False
            else:
                ignored_arguments.append(arg)

        print("ignored the following arguments:\n{}".format(
            "\n".join(ignored_arguments)))

        # Run command
        if not create_command:
            run(svn_root,
                rebuild_framework=rebuild_framework,
                force_compile_framework=force_compile_framework,
                verbose=verbose)
        else:
            construct_cmd_bat(Path(sys.argv[0]),
                              svn_root,
                              rebuild_framework=rebuild_framework,
                              force_compile_framework=force_compile_framework,
                              verbose=verbose)

