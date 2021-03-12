from pathlib import Path
from typing import Generator
import shutil
import argparse
import json


def determine_analysis_dlls(repo_root: Path) -> Generator[str, None, None]:
    """
    Gather all the dll names which need to be analysed.

    We assume that the only dll's produced by the D-HYDRO solution are the
    ones created by their corresponding .csproj. Any other dll is the result of
    a NuGet package, and as such don't require analysis.

    Currently we still ignore the test folder as well.

    Args:
        repo_root (Path): Path to the git root

    Returns:
        Generator[str, None, None]: Generator containing the dll names to sign.
    """
    src_csprojs = ((repo_root / Path("src")).glob("**/*.csproj"))
    return (x.with_suffix(".dll").name for x in src_csprojs)


def get_dll_path(repo_root: Path, dll_name: str, config: str) -> Path:
    """
    Get the dll path corresponding with the dll_name from the provided repo_root.

    Args:
        repo_root (Path): Path to the repository root
        dll_name (str): Name of the dll
        config (str): The config

    Returns:
        The path to the specified dll.
    """
    # this hack is required to keep the capitalisation
    return next((repo_root / Path(f"bin/{config}")).glob("**/{}".format(dll_name))).parent / Path(dll_name)


def find_all_dll_paths(repo_root: Path, dll_names: Generator[str, None, None], config: str) -> Generator[Path, None, None]:
    """
    Find the dll paths of the dll names provided in dll_names.

    Args:
        repo_root (Path): Path to the svn root
        dll_names (Generator[str, None, None]): Generator containing the dll names to analyse.

    Returns:
        Generator[Path, None, None]: Generator containing the dll paths to analyse.
    """
    return (get_dll_path(repo_root, x, config) for x in dll_names)


def copy_dll(destination_directory: Path, src_dll_path: Path, repo_path: Path) -> None:
    src_dll_path.parent.mkdir(parents=True, exist_ok=True)
    relative_path = src_dll_path.relative_to(repo_path)

    goal_path = destination_directory / relative_path
    goal_path.parent.mkdir(parents=True, exist_ok=True)

    shutil.move(str(src_dll_path), str(goal_path))
    shutil.move(str(src_dll_path.with_suffix('.pdb')), str(goal_path.with_suffix('.pdb')))


def run(repo_root: Path, destination_directory: Path, config: str) -> None:
    dll_names = determine_analysis_dlls(repo_root)
    dll_paths = find_all_dll_paths(repo_root, dll_names, config)

    for p in dll_paths:
        copy_dll(destination_directory, p, repo_root / Path("bin") / Path(config))


def parse_arguments():
    """
    Parse the arguments with which this script was called through
    argparse.ArgumentParser
    """
    parser = argparse.ArgumentParser()
    parser.add_argument("repo_path", help="Path to the root of the repository.")
    parser.add_argument("dest_path", help="Path to the destination folder.")
    parser.add_argument("config", help="The build configuration")
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_arguments()
    run(Path(args.repo_path), Path(args.dest_path), args.config)
