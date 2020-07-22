#!/usr/bin/python3
"""
Add the pdb files of a DeltaShell framework repository to a specified 
D-HYDRO repository
"""
__author__ = "Maarten Tegelaers"
__copyright__ = "Copyright 2020"
__version__ = "0.3"
__maintainer__ = "Maarten Tegelaers"
__email__ = "Maarten.Tegelaers@deltares.nl"
__status__ = "Development"

import os
import sys
import shutil
import subprocess
import re
import argparse

from distutils.dir_util import copy_tree
from pathlib import *

class RunConfig(object):
    """
    RunConfig holds the processed run arguments.
    """
    def __init__(self, dhydro_root: Path, 
                       deltashell_root: Path,
                       only_update_bin: bool,
                       rebuild_framework: bool,
                       add_plugins: bool,
                       use_revision: bool,
                       compile: bool,
                       verbose: bool):
        self._dhydro_root = dhydro_root
        self._deltashell_root = deltashell_root
        self._only_update_bin = only_update_bin
        self._rebuild_framework = rebuild_framework
        self._add_plugins = add_plugins
        self._use_revision = use_revision
        self._compile = compile
        self._verbose = verbose

    @classmethod
    def from_args(cls, args):
        return RunConfig(Path(args.dhydro_root),
                         Path(args.delta_shell_framework_root),
                         args.update_only_bin,
                         args.rebuild_framework,
                         args.add_plugins_to_framework,
                         args.use_revision, 
                         not args.skip_compile,
                         not args.quiet)

    @property
    def dhydro_root(self) -> Path:
        return self._dhydro_root

    @property
    def deltashell_root(self) -> Path:
        return self._deltashell_root

    @property
    def only_update_bin(self) -> bool:
        return self._only_update_bin

    @property
    def rebuild_framework(self) -> bool:
        return self._rebuild_framework

    @property
    def add_plugins(self) -> bool:
        return self._add_plugins

    @property
    def use_revision(self) -> bool:
        return self._use_revision

    @property
    def compile(self) -> bool:
        return self._compile

    @property
    def verbose(self) -> bool:
        return self._verbose


def validate_folder(root: Path, solution_file: str, verbose: bool):
    """
    Validate the specified folder exists and contains the specified solution 
    file.

    :param root: The path to the repository root.
    :type root: Path
    :param solution_file: String specifying the name (and extension) of the 
                          solution file.
    :type solution_file: str
    :param verbose: Whether to print messages.
    :type verbose: bool
    """
    if not root.exists():
        raise Exception("The provided path does not exist.")
    if not root.is_dir():
        raise Exception("The provided path is not a directory.")

    file_path = root / Path(solution_file)
    if not file_path.exists():
        raise Exception(f"The specified sln file, {solution_file}, does not exist in the repository.")
    if not file_path.is_file():
        raise Exception(f"The specified sln file, {solution_file}, is not a file.")


def validate_dhydro_root(dhydro_root: Path, verbose: bool) -> None:
    """
    Validate the path to the D-HYDRO root.

    A path is considered a D-HYDRO root if it points to a folder containing
    the NGHS.sln file.

    :param dhydro_root: The path to the D-HYDRO root.
    :type dhydro_root: Path
    :param verbose: Whether to print messages.
    :type verbose: bool
    """
    if verbose:
        print(f"Validating the D-HYDRO root: {str(dhydro_root)}")

    validate_folder(dhydro_root, "NGHS.sln", verbose)

    if verbose:
        print("Succesfully validated the D-HYDRO root.")


def validate_deltashell_root(deltashell_root: Path, verbose: bool) -> None:
    """
    Validate the path to the DeltaShell root.

    A path is considered a DeltaShell Framework root if it points to a folder 
    containing the Framework.sln file.

    :param deltashell_root: The path to the DeltaShell Framework root.
    :type deltashell_root: Path
    :param verbose: Whether to print messages.
    :type verbose: bool
    """
    if verbose:
        print(f"Validating the DeltaShell Framework root: {str(deltashell_root)}")

    validate_folder(deltashell_root, "Framework.sln", verbose)

    if verbose:
        print("Succesfully validated the DeltaShell Framework root.")


def validate(run_config: RunConfig):
    """
    Validate the provided paths.

    :param run_config: The run config 
    :type run_config: RunConfig 
    """
    try:
        validate_dhydro_root(run_config.dhydro_root, 
                             run_config.verbose)
        validate_deltashell_root(run_config.deltashell_root, 
                                 run_config.verbose)
    except Exception as e:
        print(f"Could not validate provided path: {str(e)}")
        raise


def get_src_csproj_files(root: Path):
    """
    Get all the csproj files in the source subfolder.

    :param root: The root folder
    :type root: Path:
    """
    return (root / Path("src")).glob("**/*.csproj")


def get_framework_revision(dhydro_root: Path, 
                           verbose: bool) -> int:
    """
    Get the framework revision used within the D-HYDRO repository based upon
    the packages.config files.

    :param dhydro_root: Path to the D-HYDRO repository
    :type dhydro_root: Path
    :param verbose: whether to output additional messages.
    :type verbose: bool
    """
    # We use the version specified in a package file to get the accurate 
    # version.
    version_regex = re.compile(r'DeltaShell\.Framework\.1\.5\.0\.(?P<version>\d{5})(?:-beta)?(?:-SIGNED)?')

    csproj_files = get_src_csproj_files(dhydro_root)

    for p in csproj_files:
        with p.open() as f:
            matched = version_regex.search(f.read())

            if matched and matched.group("version"):
                return int(matched.group("version"))

    raise Exception("Could not find a package file with a framework definition")


def update_framework_revision(deltashell_root: Path, 
                              revision: int, 
                              verbose: bool):
    """
    Update the DeltaShell framework to the specified revision.
    """
    # Note we currently not check for modifications, it might be better
    # to do so in the future.
    p = subprocess.call(["svn", "update",
                         "-r", "{}".format(revision),
                         str(deltashell_root)])


def update_revision(run_config: RunConfig):
    """
    Update the revision of the framework to the one used within the 
    D-HYDRO.
    """
    try:
        revision = get_framework_revision(run_config.dhydro_root, 
                                          run_config.verbose)
        update_framework_revision(run_config.deltashell_root,
                                  revision,
                                  run_config.verbose)
    except Exception as e:
        print(f"Could not update the revision of the framework: {str(e)}")
        raise


def clean_framework_bin(deltashell_root: Path, 
                        verbose: bool):
    bin_folder_path = deltashell_root / Path("bin") / Path("Debug")
    if bin_folder_path.exists():
        shutil.rmtree(str(bin_folder_path))

    for folder_path in (deltashell_root / Path("packages")).glob("*"):
        p = subprocess.Popen(["svn", "info", str(folder_path)],
                             stdout=subprocess.PIPE)
        output, error = p.communicate()
        output = output.decode("utf-8")

        if not ("Repository Root:" in output):
            shutil.rmtree(str(folder_path))


def clean_framework(run_config: RunConfig):
    try:
        clean_framework_bin(run_config.deltashell_root,
                            run_config.verbose)
    except Exception as e:
        print(f"Could not clean the bin folder of the framework: {str(e)}")
        return

def get_devenv() -> Path:
    return next(Path("C:\PROGRA~2\Microsoft Visual Studio").glob("**/devenv.com"), None)


def compile_solution(solution_path: Path):
    """
    Compile the solution at solution_path with debug settings

    :param solution_path: the path to the solution.
    """
    subprocess.call(["nuget.exe", "restore", str(solution_path),])

    devenv = get_devenv()

    if not devenv:
        raise Exception("Could not locate devenv.com...")

    subprocess.call([str(devenv), str(solution_path), "/Rebuild", "debug", "/NoSplash"])
    subprocess.call([str(devenv), str(solution_path), "/Rebuild", "debug", "/NoSplash"])


def compile_framework(run_config: RunConfig):
    """
    Compile the framework at run_config.deltashell_root with debug settings

    :param run_config: the run configuration
    """
    solution_path = run_config.deltashell_root / Path("Framework.sln")
    compile_solution(solution_path)


def get_msbuild() -> Path:
    return next(Path("C:\PROGRA~2\Microsoft Visual Studio").glob("**/msbuild.exe"), None)

def compile_dhydro(run_config: RunConfig):
    """
    Compile dhydro at run_config.dhydro_root with debug settings

    :param run_config: the run configuration
    """
    solution_path = run_config.dhydro_root / Path("NGHS.sln")
    msbuild = get_msbuild()
    
    subprocess.call(["nuget.exe", "restore", str(solution_path),])
    subprocess.call([str(msbuild), str(solution_path), "/t:Build", "/property:Configuration=Debug"])


def get_framework_version(dhydro_root: Path, 
                          verbose: bool) -> str:
    """
    Get the framework version used within the D-HYDRO repository based upon
    the packages.config files.

    :param dhydro_root: Path to the D-HYDRO repository
    :type dhydro_root: Path
    :param verbose: whether to output additional messages.
    :type verbose: bool
    """
    # We use the version specified in a package file to get the accurate 
    # version.
    version_regex = re.compile(r'(?P<version>DeltaShell\.Framework\.1\.5\.0\.\d{5}(?:-beta)?(?:-SIGNED)?)')

    csproj_files = get_src_csproj_files(dhydro_root)

    for p in csproj_files:
        with p.open() as f:
            matched = version_regex.search(f.read())

            if matched and matched.group("version"):
                return matched.group("version")

    raise Exception("Could not find a package file with a framework definition")


def update_framework_in_dhydro(run_config: RunConfig):
    """
    Update the framework in the D-HYDRO repository by copying the pdbs.

    :param run_config: The run config
    :type run_config: RunConfig
    """
    try:
        if run_config.only_update_bin and not run_config.add_plugins:
            target_path = run_config.dhydro_root / Path("bin/Debug/")
        else:
            framework_package_name = get_framework_version(run_config.dhydro_root, run_config.verbose)
            target_path = run_config.dhydro_root / Path(f"packages/{framework_package_name}/lib/net461/")

        print(str(target_path))
        copy_tree(str(run_config.deltashell_root / Path("bin/Debug/DeltaShell/")), 
                  str(target_path / Path("DeltaShell")))
        copy_tree(str(run_config.deltashell_root / Path("bin/Debug/plugins/")), 
                  str(target_path / Path("plugins")))
        print("Updated framework in D-HYDRO...")
    except Exception as e:
        print(f"Could not update D-HYDRO: {str(e)}")
        return


def update_plugins_in_framework(run_config: RunConfig):
    """
    Update the framework plugins from the D-HYDRO repository in the framework bin.

    :param run_config: The run config
    :type run_config: RunConfig
    """
    try:
        # the following files need to be copied
        # Note that adding new project will potentially break this script
        plugin_folders = [ "DeltaShell.Dimr"
                         , "DeltaShell.NGHS"
                         , "DeltaShell.Plugins.DelftModels"
                         , "DeltaShell.Plugins.FMSuite"
                         , "DeltaShell.Plugins.NetworkEditor"
                         ]

        deltashell_files = [ "DelftTools.Hydro", "DeltaShell.NGHS" ]

        framework_bin_path = run_config.deltashell_root / Path("bin/Debug/")
        dhydro_bin_path = run_config.dhydro_root / Path("bin/Debug")

        for plugin in plugin_folders:
            for p in (dhydro_bin_path / Path("plugins")).glob(f"{plugin}*"):
                print(str(p))
                copy_tree(str(p), str(framework_bin_path / Path("plugins") / p.name))

        for file_set in deltashell_files:
            for p in (dhydro_bin_path / Path("DeltaShell")).glob(f"{file_set}*"):
                print(str(p))
                shutil.copy(str(p), framework_bin_path / Path("DeltaShell") / p.name)

        print("Updated D-HYDRO in framework ...")
    except Exception as e:
        print(f"Could not update framework: {str(e)}")
        return


def add_plugins_to_framework(run_config: RunConfig):
    compile_dhydro(run_config)
    update_plugins_in_framework(run_config)


def run(run_config: RunConfig):
    """
    Update the framework package of the specified svn repository with compiled
    debug framework files.

    :param run_config: The run config 
    :type run_config: RunConfig 
    """
    try:
        validate(run_config)

        if run_config.compile or run_config.add_plugins:
            if run_config.use_revision:
                update_revision(run_config)
            if run_config.rebuild_framework or run_config.add_plugins:
                clean_framework(run_config)

            compile_framework(run_config)

        update_framework_in_dhydro(run_config)

        if run_config.add_plugins:
            add_plugins_to_framework(run_config)
    except:
        return


def get_args():
    """
    Parses and returns the arguments.
    """
    parser = argparse.ArgumentParser(prog="Add the DeltaShell PDBs")
    parser.add_argument("dhydro_root",
                        help="Path to the D-HYDRO repository root, e.g. https://github.com/Deltares/D-HYDRO.git")
    parser.add_argument("delta_shell_framework_root", 
                        help="Path to the DeltaShell root path, e.g. https://repos.deltares.nl/repos/delft-tools/trunk/delta-shell/Framework.")
    parser.add_argument("--update_only_bin",
                        action="store_true", 
                        help="Flag to make the script update only the bin folder, default behaviour is to update the packages.")
    parser.add_argument("--rebuild_framework",
                        action="store_true", 
                        help="Flag to force rebuild the framework by deleting the bin and packages folders.")
    parser.add_argument("--use_revision",
                        action="store_true",
                        help=("Flag to update the framework to the revision " +
                              "used within D-HYDRO, make sure your framework " +
                              "repository does not have local changes when " +
                              "setting this flag."))
    parser.add_argument("--skip_compile",
                        action="store_true",
                        help="Skip the compilation step of this program and use the Debug bin as is.")
    parser.add_argument("--add_plugins_to_framework",
                        action="store_true",
                        help="Compiles the D-HYDRO solution and copies the relevant dlls back to the framework. Note this ignores the update_only_bin flag.")
    parser.add_argument("--quiet", 
                        action="store_true",
                        help="Flag to suppress (some of) the messages printed by this script.")

    return parser.parse_args()


if __name__ == "__main__":
    args = get_args()
    run_config = RunConfig.from_args(args)

    run(run_config)
