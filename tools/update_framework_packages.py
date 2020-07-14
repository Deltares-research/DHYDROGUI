from pathlib import Path
import re
import argparse

def search_files(folder_path: Path, extension: str):
    """Returns full file paths with the specified extension in the folder."""
    return folder_path.glob(f"**/*{extension}")


def update_files(file_paths, find_and_replace_list, encoding=None):
    """Finds and replaces the content for each specified file."""
    for p in file_paths:
        with p.open(encoding=encoding) as f:
            lines = f.read()

        for regex_find, replacement in find_and_replace_list:
            lines = regex_find.sub(replacement, lines)

        with p.open(mode='w', encoding=encoding) as f:
            f.writelines(lines)


def get_args():
    """Parses and returns the arguments"""
    parser = argparse.ArgumentParser()
    parser.add_argument("svn_root_path", help="Path to the root of the SVN directory")
    parser.add_argument("version_number", help="Version number of the Framework NuGet package")
    parser.add_argument("--postfix", help="Add the signed value to the revision number.")
    return parser.parse_args()


if __name__ == "__main__":
    args = get_args()
    root_path = Path(args.svn_root_path)
    version_number = args.version_number
    
    # -beta is currently the default, due to DS Framework being in beta. This should be removed 
    # once the framework is officially released.
    revision_number = "{}{}".format(version_number, args.postfix if args.postfix else "-beta")

    project_file_paths = search_files(root_path, '.csproj')
    
    version_regex = r'1\.5\.0\.\d{5}(?:-beta)?(?:-SIGNED)?'
    new_version_string = f"1.5.0.{revision_number}"

    find_and_replace_csproj = [
        (re.compile(f'DeltaShell\\.Framework\\.{version_regex}'),   f"DeltaShell.Framework.{new_version_string}"),
        (re.compile(f'DeltaShell\\.TestProject\\.{version_regex}'), f"DeltaShell.TestProject.{new_version_string}"),
        (re.compile(f'Version={version_regex}'),                    f"Version={new_version_string}"),
    ]

    update_files(project_file_paths, find_and_replace_csproj, "utf-8-sig")

    config_file_paths = search_files(root_path, 'packages.config')
    find_and_replace_config = [
        (re.compile(f'"DeltaShell\\.ApplicationPlugin" version="{version_regex}"'), f'"DeltaShell.ApplicationPlugin" version="{new_version_string}"'),
        (re.compile(f'"DeltaShell\\.Framework" version="{version_regex}"'),         f'"DeltaShell.Framework" version="{new_version_string}"'),
        (re.compile(f'"DeltaShell\\.TestProject" version="{version_regex}"'),       f'"DeltaShell.TestProject" version="{new_version_string}"'),
    ]

    update_files(config_file_paths, find_and_replace_config)
