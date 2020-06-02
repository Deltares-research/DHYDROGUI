#!/usr/bin/python3
"""
Generate unique guid wxi generates a ApplicationGUIDs.wxi file with unique 
GUIDs from the ApplicationGUIDs.wxi.template contained in the provided 
source_directory at the same level as ApplicationGUIDs.wxi.template.

This script can either be run stand-alone or used as a library. In stand-alone:

python3 generate_unique_guid_wxi.py <source_directory_path>
"""

__author__ = "Maarten Tegelaers"
__copyright__ = "Copyright (C) Stichting Deltares, 2020"
__version__ = "0.1.0"
__maintainer__ = "Maarten Tegelaers"
__email__ = "Maarten.Tegelaers@deltares.nl"
__status__ = "Development"


from uuid import uuid4
from pathlib import Path
import argparse


TEMPLATE_FILE_NAME = "ApplicationGUIDs.wxi.template"
TARGET_FILE_NAME = "ApplicationGUIDs.wxi"
GUID_TEMPLATE_STR = "#REPLACE_WITH_GUID#"


def generate_guid() -> str:
    """
    Generate a new unique GUID string.

    Returns:
       str: A unique GUID string formatted in uppercase.
    """
    return str(uuid4()).upper()


def get_guid_wix_template_path(source_dir: Path) -> Path:
    """
    Get the path of the TEMPLATE_FILE_NAME within the provided source_dir.

    Args:
        source_dir (Path): the source directory in which the template file is 
                           located.
    
    Returns:
        Path: The path to the template file.
    """
    return next(source_dir.glob(f"**/{TEMPLATE_FILE_NAME}"))


def get_guid_wix_target_path(source_file_path) -> Path:
    """
    Get the path of the TARGET_FILE_NAME given the provided source_file_path.
    
    It is assumed that the TARGET_FILE_NAME should be created within the same
    directory as the TEMPLATE_FILE_NAME.

    Args:
        source_file_path (Path): the path where the template file is located.
    
    Returns:
        Path: The path to the target file.
    """
    return source_file_path.parent / Path(TARGET_FILE_NAME)


def read_guid_wix_template(file_path: Path) -> str:
    """
    Read the guid wix file and return the contents as a string.

    Note that it is expected for the TEMPLATE_FILE_NAME to exist within the
    provided source_dir.

    Args:
        file_path (Path): Path to the template string to read.

    Returns:
        str: The content of the template file located at the file_path.
    """
    with file_path.open('r', encoding='utf-8') as f:
        template_string = f.read()

    return template_string


def write_guid_wix(target_path: Path, content: str) -> None:
    """
    Write the GUID wix content to the specified target path.

    Args:
        target_path (Path): The path where to write the content to
        content (str): The filled in template to write to file.
    """
    with target_path.open('w', encoding='utf-8') as f:
        f.write(content)


def calculate_number_of_required_guids(raw_template_str: str) -> int:
    """
    Calculate the number of GUIDs that need to be generated in order 
    to fill in the raw template str.

    Args:
        raw_template_str (str): The unedited template string

    Returns:
        int: The number of GUIDs to generate in order to fill in the 
             template string fully.
    """
    return raw_template_str.count(GUID_TEMPLATE_STR)


def clean_template_string(raw_template_str: str) -> str:
    """
    Clean the GUID_TEMPLATE_STR values from the raw_template_str.

    Args:
        raw_template_str (str): The string to clean

    Returns:
        The template string without GUID_TEMPLATE_STRs.
    """
    return raw_template_str.replace(GUID_TEMPLATE_STR, "")


def generate_formatted_guid_strings(n_guids: int):
    """
    Generate n_guids number of formatted guid strings, where
    each string is unique and formatted as:
    {GUID-STRING-VALUE}.

    Args:
        n_guids (int): number of guids to generate

    Returns:
        A generator expression describing the guids
    """
    return (f"{{{generate_guid()}}}" for x in range(n_guids))


def build_guid_wix_content(raw_template_str: str) -> str:
    """
    Build a filled in guid wix template from the given raw_template_str.

    Args:
        raw_template_str (str): the template string to fill in.

    Returns:
        (str) a filled in guid wix template.
    """
    n_guids = calculate_number_of_required_guids(raw_template_str)
    cleaned_template = clean_template_string(raw_template_str)

    guids = generate_formatted_guid_strings(n_guids)
    return cleaned_template.format(*guids)


def run(src_dir: Path) -> None:
    """
    Build a filled in guid wix template given the src_dir in which
    a TEMPLATE_FILE_NAME exists.

    Args:
        src_dir (Path): source directory in which the TEMPLATE_FILE_NAME exists.
    """
    source_file_path = get_guid_wix_template_path(src_dir)

    raw_template_str = read_guid_wix_template(source_file_path)
    content = build_guid_wix_content(raw_template_str)

    target_file_path = get_guid_wix_target_path(source_file_path)
    write_guid_wix(target_file_path, content)


def parse_arguments():
    """
    Parse the arguments with which this script was called through
    argparse.ArgumentParser
    """
    parser = argparse.ArgumentParser()

    parser.add_argument("source_directory", help=f"Path to the source directory containing {TEMPLATE_FILE_NAME}.")
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_arguments()
    run(Path(args.source_directory))
