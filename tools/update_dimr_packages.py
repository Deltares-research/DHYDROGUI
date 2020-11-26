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
    parser.add_argument("dimr_version", help="The full DIMR version to update to i.e. Major.Minor.Patch[-Prefixes.counter.hash]")
    return parser.parse_args()


def get_dimr_version_regex_string() -> str:
    integer_regex = r'(0|([1-9]\d*))'
    integers_regex = fr'(\.{integer_regex})*'
    known_prefixes = ['beta']
    prefix_regex = ''.join(f'(?:-{prefix})?' for prefix in known_prefixes)

    return f'{integer_regex}{integers_regex}{prefix_regex}'


if __name__ == "__main__":
    args = get_args()
    root_path = Path(args.root_path)
    
    project_file_paths = search_files(root_path, '.csproj')

    new_version_string = args.dimr_version
    version_regex = get_dimr_version_regex_string()

    package_name = 'Dimr.Libs'
    escaped_package_name = re.escape(package_name)

    find_and_replace_csproj = [
        (re.compile(f'{escaped_package_name}\\.{version_regex}'),   f"{package_name}.{new_version_string}"),
    ]

    update_files(project_file_paths, find_and_replace_csproj, "utf-8-sig")

    config_file_paths = search_files(root_path, 'packages.config')
    find_and_replace_config = [
        (re.compile(f'"{escaped_package_name}" version="{version_regex}"'), f'"{package_name}" version="{new_version_string}"'),
    ]    

    update_files(config_file_paths, find_and_replace_config)
