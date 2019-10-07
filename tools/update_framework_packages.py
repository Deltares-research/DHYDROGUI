import os
from pathlib import Path
import re
import argparse
import glob


def get_args():
    """Parses and returns the arguments"""
    parser = argparse.ArgumentParser()
    parser.add_argument("svn_root_path", help="Path to the root of the SVN directory")
    parser.add_argument("version_number", help="Version number of the Framework NuGet package")
    parser.add_argument("--with_signed", action="store_true", help="Add the signed value to the revision number.")
    return parser.parse_args()


def search_files(folder, extension):
    """Returns full file paths with the specified extension in the folder."""
    for r, d, f in os.walk(folder):
        for file in f:
            if file.endswith(extension):
                yield Path(r) / Path(file)


def update_files(file_paths, find_and_replace_list, encoding=None):
    """Finds and replaces the content for each specified file."""
    for p in file_paths:
        with p.open(encoding=encoding) as f:
            lines = f.readlines()

        for i in range(len(lines)):
            for find, replace in find_and_replace_list:
                lines[i] = re.sub(find, replace, lines[i])

        with p.open(mode='w', encoding=encoding) as f:
            f.writelines(lines)

if __name__ == "__main__":
    args = get_args()
    root_path = args.svn_root_path
    version_number = args.version_number
    revision_number = "{}{}".format(version_number, "-SIGNED" if args.with_signed else "")

    project_file_paths = search_files(root_path, '.csproj')
    find_and_replace_csproj = [
        (r'DeltaShell\.Framework\.1\.4\.0\.\d{5}(?:-SIGNED)?',      "DeltaShell.Framework.1.4.0.{}".format(revision_number)),
        (r'DeltaShell\.TestProject\.1\.4\.0\.\d{5}(?:-SIGNED)?',    "DeltaShell.TestProject.1.4.0.{}".format(revision_number)),
        (r'Version=1\.4\.0\.\d{5}(?:-SIGNED)?',                     "Version=1.4.0.{}".format(revision_number))
    ]

    update_files(project_file_paths, find_and_replace_csproj, "utf-8-sig")

    config_file_paths = search_files(root_path, 'packages.config')
    find_and_replace_config = [
        (r'"DeltaShell\.ApplicationPlugin" version="1\.4\.0\.\d{5}(?:-SIGNED)?"',   '"DeltaShell.ApplicationPlugin" version="1.4.0.{}"'.format(revision_number)),
        (r'"DeltaShell\.Framework" version="1\.4\.0\.\d{5}(?:-SIGNED)?"',           '"DeltaShell.Framework" version="1.4.0.{}"'.format(revision_number)),
        (r'"DeltaShell\.TestProject" version="1\.4\.0\.\d{5}(?:-SIGNED)?"',         '"DeltaShell.TestProject" version="1.4.0.{}"'.format(revision_number)),
    ]

    update_files(config_file_paths, find_and_replace_config)
