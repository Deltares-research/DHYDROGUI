#!/usr/bin/python3
"""
Script
"""

__author__ = "Maarten Tegelaers"
__copyright__ = "Copyright (C) Stichting Deltares, 2020"
__version__ = "0.1.0"
__maintainer__ = "Maarten Tegelaers"
__email__ = "Maarten.Tegelaers@deltares.nl"
__status__ = "Development"

from pathlib import Path
from typing import Generator, List
import shutil
import argparse
import json
from itertools import chain


def determine_analysis_dlls(src_directories: List[Path]) -> Generator[str, None, None]:
    relevant_csprojs = chain.from_iterable(p.glob("**/*.csproj") for p in src_directories)
    return (x.with_suffix(".dll").name for x in relevant_csprojs if (not str(x).endswith("wpftmp.csproj") and not str(x).endswith("RainfallRunoffModelEngine.csproj")))


def get_dll_path(bin_directory: Path, dll_name: str) -> Path:
    elems = list((bin_directory.glob(f"**/{dll_name}")))

    if elems:
        return elems[0].parent / Path(dll_name)
    
    exe_name = Path(dll_name).with_suffix('.exe')
    elems = (bin_directory.glob(f"**/{exe_name}"))

    return next(elems).parent / Path(exe_name)


def find_all_dll_paths(bin_directory: Path, dll_names: Generator[str, None, None]) -> Generator[Path, None, None]:
    return (get_dll_path(bin_directory, x) for x in dll_names)


def copy_dll(destination_directory: Path, src_dll_path: Path, repo_path: Path) -> None:
    src_dll_path.parent.mkdir(parents=True, exist_ok=True)
    relative_path = src_dll_path.relative_to(repo_path)

    goal_path = destination_directory / relative_path
    goal_path.parent.mkdir(parents=True, exist_ok=True)

    src_pdb_path = src_dll_path.with_suffix('.pdb')
    if src_pdb_path.exists() and src_dll_path.exists() :
        shutil.move(str(src_dll_path), str(goal_path))
        shutil.move(str(src_pdb_path), str(goal_path.with_suffix('.pdb')))


def run(src_directories: List[Path], bin_directory: Path,destination_directory: Path) -> None:
    dll_names = determine_analysis_dlls(src_directories)
    dll_paths = find_all_dll_paths(bin_directory, dll_names)

    for p in dll_paths:
        copy_dll(destination_directory, p, bin_directory)


def parse_arguments():
    """
    Parse the arguments with which this script was called through
    argparse.ArgumentParser
    """
    parser = argparse.ArgumentParser()
    parser.add_argument("bin_path", help="Path to the binary folder.")
    parser.add_argument("dest_path", help="Path to the destination folder.")
    parser.add_argument("src_folders", nargs='+', help="Folders to the look for csproj files.")
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_arguments()
    src_folders = (Path(p) for p in args.src_folders)
    run(src_folders, Path(args.bin_path), Path(args.dest_path))
