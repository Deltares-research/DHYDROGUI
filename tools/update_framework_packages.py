import os
from pathlib import Path
import re
import argparse


def get_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("svn_root_path", help="Path to the root of the SVN directory")
    parser.add_argument("version_number", help="Version number of the Framework NuGet package")
    return parser.parse_args()


def search_files(extension, folder):
    for r, d, f in os.walk(folder):
        for file in f:
            if file.endswith(extension):
                yield Path(r) / Path(file)


def update_project_files(root):
    _encoding = "utf-8-sig"
    project_file_paths = list(search_files('.csproj', root))

    for project_file_path in project_file_paths:
        print("Updating " + str(project_file_path))

        with project_file_path.open(encoding=_encoding) as f:
            lines = f.readlines()

        for i in range(len(lines)):
            lines[i] = re.sub(r'DeltaShell\.Framework\.1\.4\.0\.\d{5}',
                              "DeltaShell.Framework.1.4.0." + version_number, lines[i])
            lines[i] = re.sub(r'DeltaShell\.TestProject\.1\.4\.0\.\d{5}',
                              "DeltaShell.TestProject.1.4.0." + version_number, lines[i])
            lines[i] = re.sub(r'Version=1\.4\.0\.\d{5}',
                              "Version=1.4.0." + version_number, lines[i])

        with project_file_path.open(mode='w', encoding=_encoding) as f:
            f.writelines(lines)


def update_config_files(root):
    config_file_paths = list(search_files('packages.config', root))

    for config_file_path in config_file_paths:
        print("Updating " + str(config_file_path))

        with config_file_path.open() as f:
            lines = f.readlines()

        for i in range(len(lines)):
            lines[i] = re.sub(r'"DeltaShell\.ApplicationPlugin" version="1\.4\.0\.\d{5}"',
                              '"DeltaShell.ApplicationPlugin" version="1.4.0.' + version_number + '"', lines[i])
            lines[i] = re.sub(r'"DeltaShell\.Framework" version="1\.4\.0\.\d{5}"',
                              '"DeltaShell.Framework" version="1.4.0.' + version_number + '"', lines[i])
            lines[i] = re.sub(r'"DeltaShell\.TestProject" version="1\.4\.0\.\d{5}"',
                              '"DeltaShell.TestProject" version="1.4.0.' + version_number + '"', lines[i])

        with config_file_path.open(mode='w') as f:
            f.writelines(lines)


if __name__ == "__main__":
    args = get_args()
    root_path = args.svn_root_path
    version_number = args.version_number

    update_project_files(root_path)
    update_config_files(root_path)
