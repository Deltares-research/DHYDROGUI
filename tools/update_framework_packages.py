import os
from pathlib import Path
import re
import argparse

def get_args(): 
    parser = argparse.ArgumentParser()
    parser.add_argument("svn_root_path", help="Path to the root of the SVN directory")
    parser.add_argument("version_number", help="Version number of the Framework NuGet package")
    return parser.parse_args()

def searchfiles(extension, folder):
    for r, d, f in os.walk(folder):
        for file in f:
            if file.endswith(extension):
                yield Path(r) / Path(file)

if __name__ == "__main__":
    args = get_args()
    root_path = args.svn_root_path
    version_number = args.version_number

    config_file_paths = list(searchfiles('packages.config', root_path))
    for config_file_path in config_file_paths:
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

    project_file_paths = list(searchfiles('.csproj', root_path))
    for project_file_path in project_file_paths:
        print(project_file_path)
        with project_file_path.open(encoding="utf-8-sig") as f:
            lines = f.readlines()
        for i in range(len(lines)):
            lines[i] = re.sub(r'DeltaShell\.Framework\.1\.4\.0\.\d{5}',
                                "DeltaShell.Framework.1.4.0." + version_number, lines[i])
            lines[i] = re.sub(r'DeltaShell\.TestProject\.1\.4\.0\.\d{5}',
                                "DeltaShell.TestProject.1.4.0." + version_number, lines[i])
            lines[i] = re.sub(r'Version=1\.4\.0\.\d{5}',
                                "Version=1.4.0." + version_number, lines[i])
        with project_file_path.open(mode='w') as f:
            f.writelines(lines)