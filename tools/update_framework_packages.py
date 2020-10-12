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
    parser.add_argument("root_path", help="Path to the root of the working directory")
    parser.add_argument("framework_version", help="The full DSF version to update to i.e. Major.Minor.Patch[-Prefixes.counter.hash]")
    return parser.parse_args()


def get_framework_version_regex_string() -> str:
    integer_regex = r'(0|([1-9]\d*))'
    counter_regex = r'(?:(\.|-)\b[0-9]*)?'
    git_hash_regex = r'(?:(\.|-)\b[0-9a-f]{7})?'

    known_prefixes = ['beta', 'SIGNED']
    prefix_regex = ''.join(f'(?:-{prefix})?' for prefix in known_prefixes)

    return f'{integer_regex}\\.{integer_regex}\\.{integer_regex}{prefix_regex}{counter_regex}{git_hash_regex}'


if __name__ == "__main__":
    args = get_args()
    root_path = Path(args.root_path)
    
    project_file_paths = search_files(root_path, '.csproj')

    new_version_string = args.framework_version
    version_regex = get_framework_version_regex_string()

    find_and_replace_csproj = [
        (re.compile(f'DeltaShell\\.Framework\\.{version_regex}'),   f"DeltaShell.Framework.{new_version_string}"),
        (re.compile(f'DeltaShell\\.TestProject\\.{version_regex}'), f"DeltaShell.TestProject.{new_version_string}"),
    ]

    update_files(project_file_paths, find_and_replace_csproj, "utf-8-sig")

    config_file_paths = search_files(root_path, 'packages.config')
    find_and_replace_config = [
        (re.compile(f'"DeltaShell\\.ApplicationPlugin" version="{version_regex}"'), f'"DeltaShell.ApplicationPlugin" version="{new_version_string}"'),
        (re.compile(f'"DeltaShell\\.Framework" version="{version_regex}"'),         f'"DeltaShell.Framework" version="{new_version_string}"'),
        (re.compile(f'"DeltaShell\\.TestProject" version="{version_regex}"'),       f'"DeltaShell.TestProject" version="{new_version_string}"'),
    ]

    update_files(config_file_paths, find_and_replace_config)
